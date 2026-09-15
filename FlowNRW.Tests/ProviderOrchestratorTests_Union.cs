using FlowNRW.Core.Transit;
namespace FlowNRW.Tests;
/// <summary>Partial regional results retain additional national trips.</summary>
public sealed class ProviderOrchestratorTests_Union
{
    private static readonly DateTimeOffset When = new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
    private static StopEvent Event(string source, string trip, int minutes = 0, string stop = "origin") => new()
    {
        PlannedTime = When.AddMinutes(minutes),
        Identity = new() { Source = source, TripId = source + trip, SharedTripId = trip, PlannedTime = When.AddMinutes(minutes), Stop = new() { Id = stop, Source = source, Dhid = "de:stop:" + stop } },
        Realtime = new() { Source = source, Delay = TimeSpan.FromMinutes(source == "efa" ? 3 : 1) }
    };
    private static Journey Journey(string source, string trip, int minutes = 0) => new()
    {
        Id = source + trip,
        Legs = new[] { new JourneyLeg { Departure = Event(source, trip, minutes), Arrival = Event(source, trip, minutes + 10, "destination"), Geometry = new() { Coordinates = new[] { new GeoCoordinate(51.5, 7.1) } } } },
        Transfers = new[] { new Transfer { Duration = TimeSpan.FromMinutes(2) } }
    };
    private static ProviderOrchestrator Create(TransitTestProvider national, TransitTestProvider regional, int limit = 100) =>
        new(national, regional, new NrwRegionClassifier(), new RealtimeConsolidator(), new MemoryTransitCache(new()), new(), new() { MaxResults = limit });
    /// <summary>A appears once with regional realtime and national B is retained intact.</summary>
    [Fact]
    public async Task RouteAsync_RegionalAAndNationalAB_ReturnsUnion()
    {
        var a = Journey("efa", "A"); var b = Journey("db-rest", "B", 20);
        var national = new TransitTestProvider { Journeys = new() { Source = "db-rest", Items = new[] { Journey("db-rest", "A"), b } } };
        var regional = new TransitTestProvider { Journeys = new() { Source = "efa", Items = new[] { a }, Warnings = new[] { "partial" } } };
        var result = await Create(national, regional).RouteAsync(new() { Coordinate = new(51.5, 7.1) }, new() { Coordinate = new(52.5, 13.4) }, When);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(a.Id, result.Items[0].Id);
        Assert.Same(a.Legs[0].Geometry, result.Items[0].Legs[0].Geometry);
        Assert.Same(a.Transfers, result.Items[0].Transfers);
        Assert.Same(b, result.Items[1]);
        Assert.Equal(TimeSpan.FromMinutes(3), result.Items[0].Legs[0].Departure.Realtime.Delay);
        Assert.True(result.IsFallback);
        Assert.Contains("partial-primary", result.Warnings);
    }
    /// <summary>Departure A keeps regional realtime and additional B remains visible.</summary>
    [Fact]
    public async Task DeparturesAsync_RegionalAAndNationalAB_ReturnsUnion()
    {
        var a = Event("efa", "A"); var b = Event("db-rest", "B", 20);
        var national = new TransitTestProvider { Departures = new() { Source = "db-rest", Items = new[] { Event("db-rest", "A"), b } } };
        var regional = new TransitTestProvider { Departures = new() { Source = "efa", Items = new[] { a }, Warnings = new[] { "partial" } } };
        var result = await Create(national, regional).DeparturesAsync(new() { Id = "origin", Coordinate = new(51.5, 7.1) }, When);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(TimeSpan.FromMinutes(3), result.Items[0].Realtime.Delay);
        Assert.Same(b, result.Items[1]);
        Assert.Contains("partial-primary", result.Warnings);
    }
    /// <summary>Ambiguous matches and foreign trips remain separate for both result types.</summary>
    /// <param name="ambiguous">Whether two primary candidates match the same secondary.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Merge_AmbiguousOrForeignTrips_DoesNotDropCandidates(bool ambiguous)
    {
        var national = new TransitTestProvider
        {
            Journeys = new() { Items = new[] { Journey("db-rest", ambiguous ? "A" : "B") } },
            Departures = new() { Items = new[] { Event("db-rest", ambiguous ? "A" : "B") } }
        };
        var regional = new TransitTestProvider
        {
            Journeys = new() { Items = ambiguous ? new[] { Journey("efa", "A"), Journey("efa", "A") } : new[] { Journey("efa", "A") } },
            Departures = new() { Items = ambiguous ? new[] { Event("efa", "A"), Event("efa", "A") } : new[] { Event("efa", "A") } }
        };
        var service = Create(national, regional);
        var routes = await service.RouteAsync(new() { Coordinate = new(51.5, 7.1) }, new() { Coordinate = new(52.5, 13.4) }, When);
        var departures = await service.DeparturesAsync(new() { Id = "origin", Coordinate = new(51.5, 7.1) }, When);
        Assert.Equal(ambiguous ? 3 : 2, routes.Items.Count);
        Assert.Equal(ambiguous ? 3 : 2, departures.Items.Count);
        Assert.Equal(TimeSpan.FromMinutes(1), departures.Items.Last().Realtime.Delay);
    }
    /// <summary>Union sorting is stable and the configured final bound applies to both result types.</summary>
    [Fact]
    public async Task Merge_UnsortedUnion_OrdersAndLimitsResults()
    {
        var national = new TransitTestProvider
        {
            Journeys = new() { Items = new[] { Journey("db-rest", "C", 40), Journey("db-rest", "B", 0) } },
            Departures = new() { Items = new[] { Event("db-rest", "C", 40), Event("db-rest", "B", 0) } }
        };
        var regional = new TransitTestProvider
        {
            Journeys = new() { Items = new[] { Journey("efa", "A", 20) } },
            Departures = new() { Items = new[] { Event("efa", "A", 20) } }
        };
        var service = Create(national, regional, 2);
        var routes = await service.RouteAsync(new() { Coordinate = new(51.5, 7.1) }, new() { Coordinate = new(52.5, 13.4) }, When);
        var departures = await service.DeparturesAsync(new() { Id = "origin", Coordinate = new(51.5, 7.1) }, When);
        Assert.Equal(new[] { "db-restB", "efaA" }, routes.Items.Select(item => item.Id));
        Assert.Equal(new[] { "B", "A" }, departures.Items.Select(item => item.Identity.SharedTripId));
    }
    /// <summary>All legs including walking endpoints participate in journey identity.</summary>
    /// <param name="differentLeg">Whether the second transit leg differs.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MergeJourneys_MultipleLegs_ChecksEntireJourney(bool differentLeg)
    {
        var walking = new JourneyLeg { Departure = Event("efa", "walk", 20), Arrival = Event("efa", "walk", 25, "destination"), Walking = new() { Duration = TimeSpan.FromMinutes(5) } };
        var a = Journey("efa", "A") with { Legs = new[] { Journey("efa", "A").Legs[0], Journey("efa", "C", 10).Legs[0], walking } };
        var b = Journey("db-rest", "A") with { Legs = new[] { Journey("db-rest", "A").Legs[0], Journey("db-rest", differentLeg ? "D" : "C", 10).Legs[0], walking } };
        var service = Create(new() { Journeys = new() { Items = new[] { b } } }, new() { Journeys = new() { Items = new[] { a } } });
        var result = await service.RouteAsync(new() { Coordinate = new(51.5, 7.1) }, new() { Coordinate = new(52.5, 13.4) }, When);
        Assert.Equal(differentLeg ? 2 : 1, result.Items.Count);
        Assert.Equal(a.Id, result.Items[0].Id);
    }

    /// <summary>Uniquely matched journeys fill missing realtime while preserving regional values and geometry.</summary>
    [Fact]
    public async Task MergeJourneys_UniqueMatch_EnrichesMissingFieldsWithoutReplacingRegionalRealtime()
    {
        var regionalJourney = Journey("efa", "A");
        var nationalJourney = Journey("db-rest", "A");
        var nationalLeg = nationalJourney.Legs[0];
        nationalJourney = nationalJourney with
        {
            Legs = new[] { nationalLeg with { Departure = nationalLeg.Departure with
                { Realtime = nationalLeg.Departure.Realtime with { Cancelled = true, Platform = "4" } } } }
        };
        var service = Create(new() { Journeys = new() { Items = new[] { nationalJourney } } },
            new() { Journeys = new() { Items = new[] { regionalJourney } } });
        var result = await service.RouteAsync(new() { Coordinate = new(51.5, 7.1) }, new() { Coordinate = new(52.5, 13.4) }, When);
        var journey = Assert.Single(result.Items);
        Assert.True(journey.Legs[0].Departure.Realtime.Cancelled);
        Assert.Equal("4", journey.Legs[0].Departure.Realtime.Platform);
        Assert.Equal(TimeSpan.FromMinutes(3), journey.Legs[0].Departure.Realtime.Delay);
        Assert.Same(regionalJourney.Legs[0].Geometry, journey.Legs[0].Geometry);
    }
}
