using FlowNRW.Core.Transit;

namespace FlowNRW.Tests.Transit;

/// <summary>db-rest request and error handling.</summary>
public sealed class DbRestProviderTests_Requests
{
    /// <summary>Foreign identifiers never enter db-rest routing.</summary>
    [Fact]
    public async Task RouteAsync_ForeignStop_UsesCoordinateParameters()
    {
        var gateway = new AdapterGateway(); gateway.Add("{\"journeys\":[]}");
        var point = new Address { Stop = new() { Id = "foreign", Source = "efa", Coordinate = new(51, 7) } };
        await new DbRestProvider(gateway, new(new()), new()).RouteAsync(point, point, DateTimeOffset.UtcNow);
        Assert.Contains("from.latitude=51", gateway.Requests[0].Query);
        Assert.DoesNotContain("foreign", gateway.Requests[0].Query);
    }
    /// <summary>HTTP 503 is visible to orchestration.</summary>
    [Fact]
    public async Task SearchAsync_Http503_PropagatesError()
    {
        var gateway = new AdapterGateway(); gateway.Responses.Enqueue(new() { ErrorCode = "http_503" });
        var result = await new DbRestProvider(gateway, new(new()), new()).SearchAsync("Berlin");
        Assert.Equal("http_503", result.ErrorCode);
        Assert.Equal("db-rest", result.Source);
    }
    /// <summary>Unambiguous name search produces a provider stop route.</summary>
    [Fact]
    public async Task RouteAsync_Names_ResolvesStops()
    {
        var gateway = new AdapterGateway(); gateway.Add("[{\"type\":\"stop\",\"id\":\"1\",\"name\":\"A\"}]"); gateway.Add("[{\"type\":\"stop\",\"id\":\"2\",\"name\":\"B\"}]"); gateway.Add("{\"journeys\":[]}");
        await new DbRestProvider(gateway, new(new()), new()).RouteAsync(new() { Name = "A" }, new() { Name = "B" }, DateTimeOffset.UtcNow);
        Assert.Contains("from=1", gateway.Requests[2].Query);
        Assert.Contains("to=2", gateway.Requests[2].Query);
    }
    /// <summary>Stop identifier is escaped within URL path.</summary>
    [Fact]
    public async Task DeparturesAsync_OwnStop_UsesEscapedId()
    {
        var gateway = new AdapterGateway(); gateway.Add("{\"departures\":[]}");
        await new DbRestProvider(gateway, new(new()), new()).DeparturesAsync(new() { Id = "a/b", Source = "db-rest" }, DateTimeOffset.UtcNow);
        Assert.Contains("stops/a%2Fb/departures", gateway.Requests[0].AbsoluteUri);
    }
    /// <summary>Nearby uses actual WGS84 and preserves distances.</summary>
    [Fact]
    public async Task NearbyAsync_Coordinate_MapsDistance()
    {
        var gateway = new AdapterGateway(); gateway.Add("[{\"id\":\"1\",\"name\":\"A\",\"distance\":42}]");
        var result = await new DbRestProvider(gateway, new(new()), new()).NearbyAsync(new(51, 7));
        Assert.Equal(42, result.Items[0].DistanceMeters);
        Assert.Contains("longitude=7", gateway.Requests[0].Query);
    }
    /// <summary>Cancellation reaches gateway and caller.</summary>
    [Fact]
    public async Task SearchAsync_Cancelled_ThrowsCancellation()
    {
        var gateway = new AdapterGateway();
        await Assert.ThrowsAsync<OperationCanceledException>(() => new DbRestProvider(gateway, new(new()), new()).SearchAsync("A", new CancellationToken(true)));
    }
}
