using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Regional priority, fallback and conservative enrichment.</summary>
public sealed class ProviderOrchestratorTests_Fallback
{
    private static ProviderOrchestrator Create(TransitTestProvider national, TransitTestProvider regional) =>
        new(national, regional, new NrwRegionClassifier(), new RealtimeConsolidator(), new MemoryTransitCache(new()), new());

    /// <summary>A point in NRW selects the regional provider before national results.</summary>
    /// <param name="originInNrw">Whether NRW is the origin or destination.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RouteAsync_EitherPointInNrw_PrioritizesRegional(bool originInNrw)
    {
        var national = new TransitTestProvider { Journeys = new() { Source = "db-rest", Items = new[] { new Journey { Id = "national" } } } };
        var regional = new TransitTestProvider { Journeys = new() { Source = "efa", Items = new[] { new Journey { Id = "regional" } } } };
        var nrw = new Address { Coordinate = new(51.45, 7.01) };
        var berlin = new Address { Coordinate = new(52.52, 13.40) };
        var result = await Create(national, regional).RouteAsync(originInNrw ? nrw : berlin, originInNrw ? berlin : nrw, DateTimeOffset.UtcNow);
        Assert.Equal(new[] { "regional", "national" }, result.Items.Select(item => item.Id));
        Assert.Equal("efa+db-rest", result.Source);
    }

    /// <summary>Empty, warning-bearing and failing responses invoke the secondary source.</summary>
    /// <param name="kind">Primary response kind.</param>
    [Theory]
    [InlineData("empty")]
    [InlineData("error")]
    [InlineData("partial")]
    public async Task SearchAsync_UnusablePrimary_ReturnsMarkedFallback(string kind)
    {
        var national = new TransitTestProvider
        {
            Search = _ => Task.FromResult(new ProviderResult<Address>
            {
                Source = "db-rest",
                ErrorCode = kind == "error" ? "http-503" : null,
                Items = kind == "partial" ? new[] { new Address { Name = "partial" } } : Array.Empty<Address>(),
                Warnings = kind == "partial" ? new[] { "partial" } : Array.Empty<string>()
            })
        };
        var regional = new TransitTestProvider { Search = _ => Task.FromResult(new ProviderResult<Address> { Source = "efa", Items = new[] { new Address { Name = "Berlin" } } }) };
        var result = await Create(national, regional).SearchAsync("Berlin");
        Assert.Equal("Berlin", Assert.Single(result.Items).Name);
        Assert.True(result.IsFallback);
        Assert.Equal(1, regional.Calls);
        Assert.NotEmpty(result.Warnings);
    }

    /// <summary>Outside NRW, a good national result avoids secondary requests.</summary>
    [Fact]
    public async Task NearbyAsync_OutsideNrw_UsesNationalOnly()
    {
        var national = new TransitTestProvider { Nearby = new() { Source = "db-rest", Items = new[] { new NearbyStopResult { Stop = new() { Name = "Berlin" } } } } };
        var regional = new TransitTestProvider();
        Assert.Equal("db-rest", (await Create(national, regional).NearbyAsync(new(52.52, 13.40))).Source);
        Assert.Equal(0, regional.Calls);
    }

    /// <summary>Provider errors remain visible when both sources fail.</summary>
    [Fact]
    public async Task DeparturesAsync_BothProvidersFail_PreservesError()
    {
        var national = new TransitTestProvider { Departures = new() { Source = "db-rest", ErrorCode = "http-503" } };
        var regional = new TransitTestProvider { Departures = new() { Source = "efa", ErrorCode = "timeout" } };
        var result = await Create(national, regional).DeparturesAsync(new() { Id = "stop", Coordinate = new(51.45, 7.01) }, DateTimeOffset.UtcNow);
        Assert.Equal("timeout", result.ErrorCode);
        Assert.Contains("http-503", result.Warnings);
    }
}
