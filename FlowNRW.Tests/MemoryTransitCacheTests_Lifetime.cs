using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Cache freshness and hard stale limits.</summary>
public sealed class MemoryTransitCacheTests_Lifetime
{
    /// <summary>Fresh data expires without receiving a new retrieval timestamp.</summary>
    [Fact]
    public void Get_ExpiredRealtime_ReturnsMarkedStaleOnlyWithinLimit()
    {
        var clock = new TransitTestClock();
        var cache = new MemoryTransitCache(new(), clock);
        var result = new ProviderResult<string> { Items = new[] { "item" }, RetrievedAt = clock.Now };
        cache.Set("key", result, TimeSpan.FromSeconds(30));
        Assert.Same(result, cache.Get<string>("key"));
        clock.Now += TimeSpan.FromSeconds(31);
        Assert.Null(cache.Get<string>("key"));
        var stale = cache.Get<string>("key", true);
        Assert.True(stale!.IsStale);
        Assert.True(stale.IsFallback);
        Assert.Equal(result.RetrievedAt, stale.RetrievedAt);
        clock.Now += TimeSpan.FromMinutes(5);
        Assert.Null(cache.Get<string>("key", true));
    }

    /// <summary>Cache keys include their item type and storage is bounded.</summary>
    [Fact]
    public void Set_AtCapacity_EvictsOldestEntry()
    {
        var clock = new TransitTestClock();
        var cache = new MemoryTransitCache(new() { MaxEntries = 1 }, clock);
        cache.Set("first", new ProviderResult<string> { Items = new[] { "one" } }, TimeSpan.FromHours(24));
        clock.Now += TimeSpan.FromSeconds(1);
        cache.Set("second", new ProviderResult<int> { Items = new[] { 2 } }, TimeSpan.FromHours(24));
        Assert.Null(cache.Get<string>("first"));
        Assert.Null(cache.Get<string>("second"));
        Assert.Equal(2, cache.Get<int>("second")!.Items[0]);
    }

    /// <summary>Errors cannot poison a valid cache result.</summary>
    [Fact]
    public void Set_ErrorResult_DoesNotCache()
    {
        var cache = new MemoryTransitCache(new());
        cache.Set("key", new ProviderResult<string> { ErrorCode = "http-503" }, TimeSpan.FromSeconds(30));
        Assert.Null(cache.Get<string>("key"));
    }
}
