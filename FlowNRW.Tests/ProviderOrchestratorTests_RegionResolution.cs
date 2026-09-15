using FlowNRW.Core.Transit;
namespace FlowNRW.Tests;
/// <summary>Region selection uses resolved names and identities, not missing coordinates.</summary>
public sealed class ProviderOrchestratorTests_RegionResolution
{
    private static Address Resolved => new() { Name = "Gelsenkirchen Hbf", Coordinate = new(51.505, 7.1), Stop = new() { Id = "nrw", Source = "db-rest", Name = "Gelsenkirchen Hbf", Coordinate = new(51.505, 7.1) } };
    private static ProviderOrchestrator Create(TransitTestProvider national, TransitTestProvider regional) => new(national, regional, new NrwRegionClassifier(), new RealtimeConsolidator(), new MemoryTransitCache(new()), new());
    private static TransitTestProvider National() => new()
    {
        Search = _ => Task.FromResult(new ProviderResult<Address> { Items = new[] { Resolved }, Source = "db-rest" }),
        Journeys = new() { Items = new[] { new Journey { Id = "national" } }, Source = "db-rest" },
        Departures = new() { Items = new[] { new StopEvent { Identity = new() { TripId = "national" } } }, Source = "db-rest" }
    };
    /// <summary>A resolved name or selected identity inside NRW activates regional priority.</summary>
    /// <param name="identityOnly">Use a selected stop with no coordinates.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RouteAsync_UnlocatedNrwEndpoint_ResolvesBeforeChoosingProvider(bool identityOnly)
    {
        var national = National();
        var regional = new TransitTestProvider { Journeys = new() { Items = new[] { new Journey { Id = "regional" } }, Source = "efa" } };
        var origin = identityOnly ? new Address { Stop = new() { Id = "nrw", Source = "db-rest" } } : new Address { Name = "Gelsenkirchen Hbf" };
        var result = await Create(national, regional).RouteAsync(origin, new() { Coordinate = new(52.52, 13.4) }, DateTimeOffset.UtcNow);
        Assert.Equal(new[] { "regional", "national" }, result.Items.Select(item => item.Id));
        Assert.True(regional.Calls > 0);
    }
    /// <summary>A coordinate-less NRW stop activates regional departures after resolution.</summary>
    [Fact]
    public async Task DeparturesAsync_UnlocatedNrwStop_UsesRegionalProvider()
    {
        var regional = new TransitTestProvider { Departures = new() { Items = new[] { new StopEvent { Identity = new() { TripId = "regional" } } }, Source = "efa" } };
        var result = await Create(National(), regional).DeparturesAsync(new() { Id = "nrw", Source = "db-rest" }, DateTimeOffset.UtcNow);
        Assert.Equal(new[] { "regional", "national" }, result.Items.Select(item => item.Identity.TripId));
    }
    /// <summary>Unresolved region is explicitly diagnosed even when routing succeeds.</summary>
    [Fact]
    public async Task RouteAsync_UnresolvedRegion_PreservesUnknownWarning()
    {
        var national = National();
        national.Search = _ => Task.FromResult(new ProviderResult<Address>());
        var result = await Create(national, new()).RouteAsync(new() { Name = "unknown" }, new() { Coordinate = new(52.52, 13.4) }, DateTimeOffset.UtcNow);
        Assert.Contains("region-unknown", result.Warnings);
    }
}
