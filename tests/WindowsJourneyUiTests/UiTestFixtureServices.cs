using FlowNRW.Core.Transit;
using FlowNRW.Core.Maps;

namespace FlowNRW;

/// <summary>Deterministic services compiled exclusively into the UiTest configuration.</summary>
internal sealed class UiTestFixtureServices : IStopSearchService, IRoutingService, IDepartureService, IMapTileService
{
    private readonly Dictionary<string, int> updates = [];

    /// <inheritdoc />
    public async Task<MapTile> GetAsync(int zoom, int x, int y, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' width='256' height='256'><rect width='256' height='256' fill='#edf2f5' stroke='#aabbcc'/><path d='M0 128H256M128 0V256' stroke='#aabbcc'/><text x='12' y='25' font-size='14'>KARTENFIXTURE {zoom}/{x}/{y}</text></svg>";
        return new("data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg)));
    }

    /// <inheritdoc />
    public async Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        var count = updates.GetValueOrDefault(stop.Name) + 1;
        updates[stop.Name] = count;
        await Task.Delay(stop.Name.Contains("monitor-slow") ? 6000 : 700);
        var sequence = stop.Name.Contains("monitor-sequence");
        var error = stop.Name.Contains("monitor-error") || sequence && count == 3;
        var empty = stop.Name.Contains("monitor-empty") || sequence && count == 4;
        return new ProviderResult<StopEvent>
        {
            Items = error || empty ? [] : [Departure(stop, departure.AddMinutes(5), "RE 1 · Stand " + count, TimeSpan.FromMinutes(3), false, "2"),
                Departure(stop, departure.AddMinutes(10), "S2", TimeSpan.Zero, false, "1"),
                Departure(stop, departure.AddMinutes(15), "107", null, null, null),
                Departure(stop, departure.AddMinutes(20), "U11", null, true, null)],
            ErrorCode = error ? "fixture_unavailable" : null,
            Source = "UI-Fixture " + stop.Name + " " + stop.Id, RetrievedAt = DateTimeOffset.UtcNow.AddMinutes(-2),
            IsFallback = true, IsStale = true, Warnings = ["fixture_warning"]
        };
    }

    private static StopEvent Departure(Stop stop, DateTimeOffset planned, string line, TimeSpan? delay, bool? cancelled, string? platform)
    {
        return new StopEvent
        {
            Identity = new() { Stop = stop, Line = line, Direction = "Essen Hauptbahnhof", Operator = "Fixture Bahn" },
            PlannedTime = planned,
            Realtime = new() { ActualTime = delay is { } value ? planned + value : null, Delay = delay, Cancelled = cancelled, PlannedPlatform = "1", Platform = platform }
        };
    }

    /// <inheritdoc />
    public async Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default)
    {
        await Task.Delay(text.Contains("slow", StringComparison.OrdinalIgnoreCase) ? 6000 : 600);
        return new ProviderResult<Address>
        {
            Items = text == "empty" || text == "error" ? [] : [Candidate(text, 0), Candidate(text, 1)],
            ErrorCode = text == "error" ? "fixture_unavailable" : null,
            Source = "UI-Fixture " + text,
            RetrievedAt = DateTimeOffset.UtcNow.AddMinutes(-8),
            IsFallback = true, IsStale = true, Warnings = ["fixture_warning"]
        };
    }

    /// <inheritdoc />
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderResult<NearbyStopResult>());
    }

    /// <inheritdoc />
    public async Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        await Task.Delay(origin.Name.Contains("route-slow") ? 6000 : 800);
        var time = new DateTimeOffset(2026, 9, 16, 23, 55, 0, TimeSpan.FromHours(2));
        return new ProviderResult<Journey>
        {
            Items = origin.Name.Contains("route-empty") || origin.Name.Contains("route-error") ? [] : [Route(time, "RE 1", origin, destination), Route(time.AddHours(1), "RE 2", origin, destination)],
            ErrorCode = origin.Name.Contains("route-error") ? "fixture_unavailable" : null,
            Source = "UI-Fixture " + origin.Name,
            RetrievedAt = DateTimeOffset.UtcNow.AddMinutes(-8),
            IsFallback = true, IsStale = true, Warnings = ["fixture_warning"]
        };
    }

    private static Address Candidate(string text, int index)
    {
        var coordinate = text.Contains("map-missing") ? null : new GeoCoordinate(51.45 + index * .01, 7.01);
        return new Address { Name = text + " Treffer " + index, Coordinate = coordinate,
            Stop = text.Contains("Adresse") ? null : new Stop { Id = "fixture-" + index, Name = text, Source = "fixture", Dhid = "de:05113:001:" + index, Coordinate = coordinate } };
    }

    private static Journey Route(DateTimeOffset time, string line, Address origin, Address destination)
    {
        return new Journey
        {
            Legs = [new JourneyLeg { Departure = Event(time, origin.Name, true), Arrival = Event(time.AddMinutes(20), "Umstieg", false), Line = new Line { Name = line, Operator = new Operator { Name = "Fixture Bahn" } }, Geometry = line == "RE 1" ? new GeoGeometry { Coordinates = [new(51.45, 7.01), new(51.46, 7.02), new(51.47, 7.03)] } : null },
                new JourneyLeg { Departure = Event(time.AddMinutes(20), "Umstieg", false), Arrival = Event(time.AddMinutes(25), "Bussteig", false), Walking = new WalkingSegment { DistanceMeters = 300, Duration = TimeSpan.FromMinutes(5), Geometry = line == "RE 1" ? new GeoGeometry { Coordinates = [new(51.47, 7.03), new(51.4705, 7.031)] } : null } },
                new JourneyLeg { Departure = Event(time.AddMinutes(30), "Bussteig", false), Arrival = Event(time.AddMinutes(45), destination.Name, false), Line = new Line { Name = "Bus 10", Operator = new Operator { Name = "Fixture Bus" } } }],
            Transfers = [new Transfer { Stop = new Stop { Name = "Umstieg" }, Duration = TimeSpan.FromMinutes(10) }]
        };
    }

    private static StopEvent Event(DateTimeOffset time, string name, bool realtime)
    {
        return new StopEvent { PlannedTime = time, Identity = new TripIdentity { Stop = new Stop { Name = name } },
            Realtime = realtime ? new RealtimeStatus { ActualTime = time.AddMinutes(3), Cancelled = true, Source = "Fixture Echtzeit" } : new RealtimeStatus() };
    }
}

/// <summary>Offline and cancellable map fixtures isolated from the release provider.</summary>
/// <param name="model">Current map snapshot.</param>
/// <returns>Isolated test tile source.</returns>
internal sealed class UiTestMapTiles(MapViewModel model) : IMapTileService
{
    private readonly UiTestFixtureServices images = new();
    private int failedTiles;
    /// <inheritdoc />
    public async Task<MapTile> GetAsync(int zoom, int x, int y, CancellationToken cancellationToken)
    {
        var name = model.Stations.FirstOrDefault()?.Candidate.Name ?? "";
        if (name.Contains("map-offline") && Interlocked.Increment(ref failedTiles) <= 4)
            return new("", Error: true);
        if (name.Contains("map-slow")) await Task.Delay(6000, cancellationToken);
        return await images.GetAsync(zoom, x, y, cancellationToken);
    }
}
