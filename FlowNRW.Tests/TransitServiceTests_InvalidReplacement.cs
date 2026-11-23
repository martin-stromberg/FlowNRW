using FlowNRW.Core.Transit;
namespace FlowNRW.Tests;
/// <summary>Invalid replacement input still supersedes pending work.</summary>
public sealed class TransitServiceTests_InvalidReplacement
{
    /// <summary>Clearing a search invalidates the preceding request.</summary>
    [Fact]
    public async Task SearchAsync_InvalidReplacement_CancelsOlderResult()
    {
        var completion = new TaskCompletionSource<ProviderResult<Address>>();
        var provider = new TransitTestProvider { Search = _ => completion.Task };
        var service = new StopSearchService(provider, new());
        var older = service.SearchAsync("Berlin");
        Assert.Equal("invalid-search", (await service.SearchAsync("")).ErrorCode);
        completion.SetResult(new() { Items = new[] { new Address { Name = "Berlin" } } });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => older);
    }
    /// <summary>Clearing an endpoint invalidates the preceding route.</summary>
    [Fact]
    public async Task RouteAsync_InvalidReplacement_CancelsOlderResult()
    {
        var completion = new TaskCompletionSource<ProviderResult<Journey>>();
        var provider = new TransitTestProvider { RoutePending = _ => completion.Task };
        var service = new RoutingService(provider, new());
        var older = service.RouteAsync(new() { Name = "Berlin" }, new() { Name = "Hamburg" }, DateTimeOffset.UtcNow);
        Assert.Equal("invalid-location", (await service.RouteAsync(new(), new(), DateTimeOffset.UtcNow)).ErrorCode);
        completion.SetResult(new());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => older);
    }
    /// <summary>Invalidating a stop invalidates preceding departures.</summary>
    [Fact]
    public async Task DeparturesAsync_InvalidReplacement_CancelsOlderResult()
    {
        var completion = new TaskCompletionSource<ProviderResult<StopEvent>>();
        var provider = new TransitTestProvider { DeparturesPending = _ => completion.Task };
        var service = new DepartureService(provider, new());
        var older = service.DeparturesAsync(new() { Id = "Berlin" }, DateTimeOffset.UtcNow);
        Assert.Equal("invalid-stop", (await service.DeparturesAsync(new(), DateTimeOffset.UtcNow)).ErrorCode);
        completion.SetResult(new());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => older);
    }
}
