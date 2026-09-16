using FlowNRW.Core.Transit;

namespace FlowNRW;

/// <summary>Deterministic services compiled exclusively into the UiTest configuration.</summary>
internal sealed class UiTestFixtureServices : IStopSearchService, IRoutingService
{
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
        var coordinate = new GeoCoordinate(51.45 + index * .01, 7.01);
        return new Address { Name = text + " Treffer " + index, Coordinate = coordinate,
            Stop = text.Contains("Adresse") ? null : new Stop { Id = "fixture-" + index, Name = text, Source = "fixture", Dhid = "de:05113:001:" + index, Coordinate = coordinate } };
    }

    private static Journey Route(DateTimeOffset time, string line, Address origin, Address destination)
    {
        return new Journey
        {
            Legs = [new JourneyLeg { Departure = Event(time, origin.Name, true), Arrival = Event(time.AddMinutes(20), "Umstieg", false), Line = new Line { Name = line, Operator = new Operator { Name = "Fixture Bahn" } } },
                new JourneyLeg { Departure = Event(time.AddMinutes(20), "Umstieg", false), Arrival = Event(time.AddMinutes(25), "Bussteig", false), Walking = new WalkingSegment { DistanceMeters = 300, Duration = TimeSpan.FromMinutes(5) } },
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

