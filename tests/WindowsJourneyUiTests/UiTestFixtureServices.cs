using FlowNRW.Core.Transit;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using System.ComponentModel;
using FlowNRW.Core.Favorites;
using FlowNRW.Core.Refresh;

namespace FlowNRW;

/// <summary>Deterministic services compiled exclusively into the UiTest configuration.</summary>
internal sealed class UiTestFixtureServices : IStopSearchService, IRoutingService, IDepartureService, IMapTileService
{
    private readonly Dictionary<string, int> updates = [];
    private readonly UiTestLocationServices? location;

    /// <summary>Creates transit fixtures with an optional shared location scenario.</summary>
    /// <param name="location">UiTest-only scenario controller.</param>
    public UiTestFixtureServices(UiTestLocationServices? location = null) { this.location = location; }

    /// <inheritdoc />
    public async Task<MapTile> GetAsync(int zoom, int x, int y, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        var svg = "<svg xmlns='http://www.w3.org/2000/svg' width='256' height='256'><rect width='256' height='256' fill='#edf2f5' stroke='#aabbcc'/><path d='M0 128H256M128 0V256' stroke='#aabbcc'/><path d='M-20 205 C55 165 155 235 276 175' fill='none' stroke='#c4d5c0' stroke-width='16'/><path d='M-20 205 C55 165 155 235 276 175' fill='none' stroke='#ffffff' stroke-width='3'/></svg>";
        return new("data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg)));
    }

    /// <inheritdoc />
    public async Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        var count = updates.GetValueOrDefault(stop.Name) + 1;
        updates[stop.Name] = count;
        var scenario = location?.Scenario ?? "success";
        var complete = scenario is "favorite-cache-seed" or "cache-start-slow-nearby" or "cache-start-error" or "cache-start-cancel" or "cache-lines-bc" or "cache-lines-empty" or "cache-expired-lines" or "cache-timeless-lines";
        location?.RecordDeparture(stop.Id);
        var favoriteTarget = stop.Id == "fixture-favorite-far-0";
        await Task.Delay(stop.Name.Contains("monitor-slow") || scenario == "refresh-slow" || favoriteTarget && scenario is "favorite-slow" or "cache-start-slow-nearby" or "cache-start-cancel" ? 6000 : 700);
        var sequence = stop.Name.Contains("monitor-sequence");
        var error = stop.Name.Contains("monitor-error") || scenario == "refresh-error" || sequence && count == 3 || favoriteTarget && scenario is "favorite-error" or "cache-start-error";
        var empty = stop.Name.Contains("monitor-empty") || sequence && count == 4 || favoriteTarget && scenario == "cache-lines-empty";
        var lines = favoriteTarget && scenario == "cache-lines-bc"
            ? new[] { Departure(stop, departure.AddMinutes(5), "B", TimeSpan.Zero, false, "2"), Departure(stop, departure.AddMinutes(10), "C", null, false, "1") }
            : favoriteTarget && scenario == "cache-expired-lines"
                ? new[] { Departure(stop, departure.AddMinutes(-5), "Expired", TimeSpan.Zero, false, "2") }
                : favoriteTarget && scenario == "cache-timeless-lines"
                    ? new[] { Departure(stop, null, "Zeitlos", null, false, "2") }
                    : new[] { Departure(stop, departure.AddMinutes(5), favoriteTarget && scenario is "favorite-slow" or "cache-start-slow-nearby" ? "RE 1 · Live Stand " + count : "RE 1 · Stand " + count, TimeSpan.FromMinutes(3), false, "2"),
                Departure(stop, departure.AddMinutes(10), "S2", TimeSpan.Zero, false, "1"), Departure(stop, departure.AddMinutes(15), "107", null, null, null), Departure(stop, departure.AddMinutes(20), "U11", null, true, null) };
        return new ProviderResult<StopEvent>
        {
            Items = error || empty ? [] : lines,
            ErrorCode = error ? "fixture_unavailable" : null,
            Source = "UI-Fixture " + stop.Name + " " + stop.Id,
            RetrievedAt = scenario == "resume-fresh" ? DateTimeOffset.UtcNow : DateTimeOffset.UtcNow.AddMinutes(-2),
            IsFallback = !complete, IsStale = !complete && scenario != "resume-fresh", Warnings = complete ? [] : ["fixture_warning"]
        };
    }

    private static StopEvent Departure(Stop stop, DateTimeOffset? planned, string line, TimeSpan? delay, bool? cancelled, string? platform)
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
    public async Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        var scenario = location?.Scenario ?? "success";
        await Task.Delay(scenario is "nearby-slow" or "cache-start-slow-nearby" ? 6000 : 600);
        var newest = coordinate.Latitude > 51.49;
        var name = newest ? "Neue Umgebung" : "Umgebung";
        return new ProviderResult<NearbyStopResult>
        {
            Items = scenario is "nearby-empty" or "nearby-error" ? [] :
            [
                new() { Stop = new Stop { Id = "fixture-nearby-0", Name = name + " Süd", Source = "fixture", Coordinate = new(51.45, 7.01) }, DistanceMeters = 225 },
                new() { Stop = new Stop { Id = "fixture-nearby-1", Name = name + " Nord", Source = "fixture", Coordinate = scenario == "nearby-missing" ? null : new(51.46, 7.01) } }
            ],
            ErrorCode = scenario == "nearby-error" ? "fixture_unavailable" : null,
            Source = "UI-Fixture Nearby " + (newest ? "new" : "original"),
            RetrievedAt = DateTimeOffset.UtcNow.AddMinutes(-2),
            IsFallback = true, IsStale = true, Warnings = ["fixture_warning"]
        };
    }

    /// <inheritdoc />
    public async Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default, bool arriveBy = false)
    {
        var scenario = location?.Scenario ?? "success";
        var count = updates.GetValueOrDefault("routes") + 1;
        updates["routes"] = count;
        location?.RecordRoute();
        await Task.Delay(origin.Name.Contains("route-slow") || scenario == "resume-route-slow" ? 6000 : 800);
        var time = new DateTimeOffset(2026, 9, 16, 23, 55, 0, TimeSpan.FromHours(2));
        if (scenario == "resume-route-missing") time = time.AddDays(1);
        var first = Route(time, "RE 1", origin, destination);
        var error = origin.Name.Contains("route-error") || scenario == "resume-route-error";
        return new ProviderResult<Journey>
        {
            Items = origin.Name.Contains("route-empty") || error ? [] : [first, scenario == "resume-route-duplicate" ? first : Route(time.AddHours(1), "RE 2", origin, destination)],
            ErrorCode = error ? "fixture_unavailable" : null,
            Source = "UI-Fixture " + origin.Name + (scenario.StartsWith("resume-", StringComparison.Ordinal) ? " Stand " + count : ""),
            RetrievedAt = scenario == "resume-route-fresh" ? DateTimeOffset.UtcNow : DateTimeOffset.UtcNow.AddMinutes(-8),
            IsFallback = true, IsStale = scenario != "resume-route-fresh", Warnings = ["fixture_warning"]
        };
    }

    private static Address Candidate(string text, int index)
    {
        var favorite = text.StartsWith("Favorite ", StringComparison.Ordinal);
        var kind = favorite ? text[9..].ToLowerInvariant() : "";
        var coordinate = text.Contains("map-missing") || kind == "missing" ? null :
            new GeoCoordinate(kind == "far" ? 51.5 : kind == "near" ? 51.4555 : 51.45 + index * .01, 7.01);
        return new Address { Name = text + " Treffer " + index, Coordinate = coordinate,
            Stop = text.Contains("Adresse") ? null : new Stop { Id = favorite ? "fixture-favorite-" + kind + "-" + index : "fixture-" + index, Name = text, Source = "fixture", Dhid = "de:05113:001:" + index, Coordinate = coordinate } };
    }

    private static Journey Route(DateTimeOffset time, string line, Address origin, Address destination)
    {
        return new Journey
        {
            Legs = [new JourneyLeg { Departure = Event(time, origin.Name, true), Arrival = Event(time.AddMinutes(20), "Umstieg", false), Line = new Line { Name = line, Operator = new Operator { Name = "Fixture Bahn" } }, Geometry = line == "RE 1" ? new GeoGeometry { Coordinates = [new(51.45, 7.01), new(51.46, 7.02), new(51.47, 7.03)] } : null },
                new JourneyLeg { Departure = Event(time.AddMinutes(20), "Umstieg", false), Arrival = Event(time.AddMinutes(25), "Bussteig", false), Walking = new WalkingSegment { DistanceMeters = 300, Duration = TimeSpan.FromMinutes(5), Geometry = line == "RE 1" ? new GeoGeometry { Coordinates = [new(51.47, 7.03), new(51.4705, 7.031)] } : null } },
                new JourneyLeg { Departure = Event(time.AddMinutes(30), "Bussteig", false) with { Line = new Line { Name = "Bus 10", Mode = "bus", Operator = new Operator { Name = "Fixture Bus" } } }, Arrival = Event(time.AddMinutes(45), destination.Name, false) }],
            Transfers = [new Transfer { Stop = new Stop { Name = "Umstieg" }, Duration = TimeSpan.FromMinutes(10) }]
        };
    }

    private static StopEvent Event(DateTimeOffset time, string name, bool realtime)
    {
        return new StopEvent { PlannedTime = time, Identity = new TripIdentity { Source = "fixture", TripId = "trip-" + time.ToString("O"), PlannedTime = time, Stop = new Stop { Name = name, Id = name, Source = "fixture" } },
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

/// <summary>Explicitly selected location scenarios compiled only for native UI tests.</summary>
internal sealed class UiTestLocationServices : ICurrentLocationService, INotifyPropertyChanged
{
    private string scenario = Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_SCENARIO") ?? "success";
    private int calls;
    private int routeCalls;
    private readonly Dictionary<string, int> departureCalls = [];

    /// <summary>Per-stop invocation counts, available only in UiTest UI.</summary>
    public string DepartureCalls
    {
        get { return string.Join(";", departureCalls.OrderBy(pair => pair.Key).Select(pair => pair.Key + "=" + pair.Value)); }
    }

    /// <summary>Records a native fixture departure request before its asynchronous completion.</summary>
    /// <param name="stopId">Synthetic fixture identity.</param>
    public void RecordDeparture(string stopId)
    {
        departureCalls[stopId] = departureCalls.GetValueOrDefault(stopId) + 1;
        PropertyChanged?.Invoke(this, new(nameof(DepartureCalls)));
    }

    /// <summary>Number of synthetic route requests for lifecycle assertions.</summary>
    public int RouteCalls => routeCalls;

    /// <summary>Records a route request before any asynchronous fixture delay.</summary>
    public void RecordRoute() { routeCalls++; PropertyChanged?.Invoke(this, new(nameof(RouteCalls))); }

    /// <summary>Notifies native fixture controls of scenario and counter updates.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Selected deterministic scenario; never supplied to Release builds.</summary>
    public string Scenario
    {
        get => scenario;
        set { scenario = value.Trim().ToLowerInvariant(); PropertyChanged?.Invoke(this, new(nameof(Scenario))); }
    }

    /// <summary>Number of explicit location requests since this process started.</summary>
    public int Calls => calls;

    /// <inheritdoc />
    public async Task<LocationResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var selected = Scenario;
        calls++;
        PropertyChanged?.Invoke(this, new(nameof(Calls)));
        // Deliberately non-cooperative to prove UI revisions also reject late completions.
        await Task.Delay(selected == "slow" ? 6000 : 400);
        var status = selected switch
        {
            "denied" => LocationStatus.Denied,
            "disabled" => LocationStatus.Disabled,
            "unsupported" => LocationStatus.Unsupported,
            "timeout" => LocationStatus.Timeout,
            "unavailable" => LocationStatus.Unavailable,
            "error" => LocationStatus.Error,
            _ => LocationStatus.Success
        };
        if (status != LocationStatus.Success) return new(status);
        return new(LocationStatus.Success, new GeoCoordinate(selected == "new" ? 51.5 : 51.4556, 7.0116),
            DateTimeOffset.UtcNow, selected == "reduced" ? 1000 : 30, selected == "reduced");
    }
}
/// <summary>UiTest-only storage fault injection wrapping the same JSON implementation as Release.</summary>
/// <param name="inner">Isolated test file store.</param>
/// <param name="scenario">Shared explicit fixture control.</param>
/// <returns>Storage fixture with genuine underlying persistence.</returns>
internal sealed class UiTestFavoriteStore(IFavoriteStore inner, UiTestLocationServices scenario) : IFavoriteStore
{
    /// <inheritdoc />
    public Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default) => inner.LoadAsync(cancellationToken);

    /// <inheritdoc />
    public Task SaveAsync(IReadOnlyList<Stop> stops, CancellationToken cancellationToken = default)
    {
        if (scenario.Scenario == "store-error") throw new IOException("Synthetic UiTest write failure.");
        return inner.SaveAsync(stops, cancellationToken);
    }
}

/// <summary>UiTest-only interval storage failure switch.</summary>
internal sealed class UiTestRefreshSettingsStore : IRefreshSettingsStore
{
    private readonly IRefreshSettingsStore inner;
    private readonly UiTestLocationServices scenario;

    /// <summary>Creates the fixture wrapper.</summary>
    /// <param name="inner">Real bounded settings store.</param>
    /// <param name="scenario">Fixture scenario controller.</param>
    public UiTestRefreshSettingsStore(IRefreshSettingsStore inner, UiTestLocationServices scenario)
    {
        this.inner = inner; this.scenario = scenario;
    }
    /// <inheritdoc />
    public Task<int?> LoadAsync(CancellationToken cancellationToken = default) => inner.LoadAsync(cancellationToken);

    /// <inheritdoc />
    public Task SaveAsync(int seconds, CancellationToken cancellationToken = default)
    {
        if (scenario.Scenario == "refresh-store-error") throw new IOException("Synthetic UiTest settings write failure.");
        return inner.SaveAsync(seconds, cancellationToken);
    }
}
