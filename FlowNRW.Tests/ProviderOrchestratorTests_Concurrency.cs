using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Shared requests, cancellation ownership and cache fallback.</summary>
public sealed class ProviderOrchestratorTests_Concurrency
{
    /// <summary>Two identical requests share one transport; cancellation belongs to each waiter.</summary>
    [Fact]
    public async Task SearchAsync_IdenticalConcurrentRequests_SharesOneFetch()
    {
        var completion = new TaskCompletionSource<ProviderResult<Address>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var national = new TransitTestProvider { Search = token => completion.Task.WaitAsync(token) };
        var orchestrator = new ProviderOrchestrator(national, new TransitTestProvider(), new NrwRegionClassifier(), new RealtimeConsolidator(), new MemoryTransitCache(new()), new());
        using var cancellation = new CancellationTokenSource();
        var first = orchestrator.SearchAsync("Berlin", cancellation.Token);
        var second = orchestrator.SearchAsync("Berlin");
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
        completion.SetResult(new() { Source = "db-rest", Items = new[] { new Address { Name = "Berlin" } } });
        Assert.Equal("Berlin", Assert.Single((await second).Items).Name);
        Assert.Equal(1, national.Calls);
        Assert.Equal("Berlin", Assert.Single((await orchestrator.SearchAsync("Berlin")).Items).Name);
        Assert.Equal(1, national.Calls);
    }

    /// <summary>When all waiters leave, transport is cancelled and cannot populate the cache.</summary>
    [Fact]
    public async Task SearchAsync_AllWaitersCancel_CancelsUnderlyingFetch()
    {
        var stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var national = new TransitTestProvider
        {
            Search = async token =>
        {
            try { await Task.Delay(Timeout.Infinite, token); }
            finally { stopped.TrySetResult(); }
            return new();
        }
        };
        var orchestrator = new ProviderOrchestrator(national, new TransitTestProvider(), new NrwRegionClassifier(), new RealtimeConsolidator(), new MemoryTransitCache(new()), new());
        using var cancellation = new CancellationTokenSource();
        var request = orchestrator.SearchAsync("Berlin", cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
        await stopped.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(1, national.Calls);
    }

    /// <summary>A marked stale result is available only during a provider failure and within five minutes.</summary>
    [Fact]
    public async Task DeparturesAsync_RefreshFails_UsesMarkedStaleCache()
    {
        var clock = new TransitTestClock();
        var national = new TransitTestProvider { Departures = new() { Source = "db-rest", RetrievedAt = clock.Now, Items = new[] { new StopEvent() } } };
        var orchestrator = new ProviderOrchestrator(national, new TransitTestProvider(), new NrwRegionClassifier(), new RealtimeConsolidator(), new MemoryTransitCache(new(), clock), new());
        var stop = new Stop { Id = "Berlin", Coordinate = new(52.52, 13.4) };
        var departure = clock.Now;
        await orchestrator.DeparturesAsync(stop, departure);
        clock.Now += TimeSpan.FromSeconds(31);
        national.Departures = new() { Source = "db-rest", ErrorCode = "http-503" };
        var result = await orchestrator.DeparturesAsync(stop, departure);
        Assert.True(result.IsStale);
        Assert.True(result.IsFallback);
        Assert.Equal(departure, result.RetrievedAt);
        Assert.Contains("stale-fallback", result.Warnings);
    }
}
