using FlowNRW.Core.Favorites;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Favorite cards restore local boards before independently refreshing their live provider state.</summary>
public sealed class FavoriteDepartureCacheIntegrationTests
{
    /// <summary>A future cached board is visible after loading and still receives one automatic startup request.</summary>
    [Fact]
    public async Task LoadAsync_RestoresFutureBoardThenStartupRefreshesIt()
    {
        var stop = Stop("one");
        var cache = new MemoryDepartureCache { Entries = [Entry(stop, "cached", DateTimeOffset.Now.AddMinutes(4))] };
        var services = new List<ControlledDepartureService>();
        var home = new FavoriteHomeViewModel(new MemoryFavoriteStore(stop), () => AddService(services), new ControlledLocation(), cache: cache);

        await home.LoadAsync();

        var card = Assert.Single(home.Cards);
        Assert.Equal("cached", card.Result?.Source);
        Assert.Contains("Letzter Stand", card.Status);
        var refresh = home.RefreshAtStartupAsync();
        var provider = Assert.Single(services);
        Assert.Single(provider.Pending);
        provider.Pending[0].SetResult(Result("live", DateTimeOffset.Now.AddMinutes(6)));
        await refresh;

        Assert.Equal("live", card.Result?.Source);
        var persisted = Assert.Single(cache.Entries);
        Assert.Equal("live", persisted.Result.Source);
    }

    /// <summary>Expired local events are discarded while a provider failure keeps remaining cached data usable.</summary>
    [Fact]
    public async Task LoadAsync_DiscardsExpiredEventsAndRetainsFutureCacheOnFailure()
    {
        var stop = Stop("one");
        var cache = new MemoryDepartureCache
        {
            Entries = [new DepartureCacheEntry
        {
            Source = stop.Source,
            StopId = stop.Id,
            Result = new ProviderResult<StopEvent>
            {
                Source = "cached",
                Items = [Event(DateTimeOffset.Now.AddMinutes(-1)), Event(DateTimeOffset.Now.AddMinutes(3))]
            }
        }]
        };
        var services = new List<ControlledDepartureService>();
        var home = new FavoriteHomeViewModel(new MemoryFavoriteStore(stop), () => AddService(services), new ControlledLocation(), cache: cache);
        await home.LoadAsync();
        var card = Assert.Single(home.Cards);
        var retained = Assert.Single(card.Items);
        Assert.True(retained.PlannedTime > DateTimeOffset.Now);

        var refresh = home.RefreshAtStartupAsync();
        Assert.Single(services[0].Pending);
        services[0].Pending[0].SetException(new IOException());
        await refresh;

        Assert.Same(retained, Assert.Single(card.Items));
        Assert.Contains("Letzte bekannte", card.Status);
    }

    /// <summary>A future event from an expired cache response stays visible and marked stale.</summary>
    [Fact]
    public async Task LoadAsync_RestoresStaleBoardWithFutureDepartureAsMarkedStale()
    {
        var stop = Stop("stale");
        var clock = new TransitTestClock { Now = DateTimeOffset.UtcNow };
        var freshness = new FlowNRW.Core.Refresh.RefreshFreshness(new TransitCacheOptions { RealtimeTimeToLive = TimeSpan.FromSeconds(1) }, clock);
        var cache = new MemoryDepartureCache
        {
            Entries = [new DepartureCacheEntry
            {
                Source = stop.Source,
                StopId = stop.Id,
                Result = new ProviderResult<StopEvent> { Source = "stale", RetrievedAt = clock.Now.AddMinutes(-1), Items = [Event(DateTimeOffset.Now.AddMinutes(5))] }
            }]
        };
        var home = new FavoriteHomeViewModel(new MemoryFavoriteStore(stop), () => new ControlledDepartureService(), new ControlledLocation(), cache: cache, freshness: freshness);

        await home.LoadAsync();

        var restored = Assert.Single(home.Cards).Result;
        Assert.NotNull(restored);
        Assert.True(restored.IsStale);
        Assert.Single(restored.Items);
    }

    /// <summary>A board with no remaining future departure is removed from durable cache during hydration.</summary>
    [Fact]
    public async Task LoadAsync_KeepsCacheBoardWithOnlyExpiredDeparturesForItsLineInventory()
    {
        var stop = Stop("one");
        var cache = new MemoryDepartureCache { Entries = [Entry(stop, "expired", DateTimeOffset.Now.AddMinutes(-1)) with { Lines = ["S1"] }] };
        var home = new FavoriteHomeViewModel(new MemoryFavoriteStore(stop), () => new ControlledDepartureService(), new ControlledLocation(), cache: cache);

        await home.LoadAsync();

        var card = Assert.Single(home.Cards);
        Assert.Null(card.Result);
        Assert.Equal(["S1"], card.Lines);
        Assert.Single(cache.Entries);
    }

    /// <summary>A partial provider response retains both the visible complete board and its durable line inventory.</summary>
    [Fact]
    public async Task Refresh_PersistsLineInventoryOnlyFromCompleteResponses()
    {
        var stop = Stop("one");
        var cache = new MemoryDepartureCache();
        var service = new ControlledDepartureService();
        var home = new FavoriteHomeViewModel(new MemoryFavoriteStore(stop), () => service, new ControlledLocation(), cache: cache);
        await home.LoadAsync();
        var card = Assert.Single(home.Cards);
        var first = card.RefreshAsync();
        service.Pending[0].SetResult(new ProviderResult<StopEvent> { Source = "live", Items = [Event(DateTimeOffset.Now.AddMinutes(4)) with { Identity = new TripIdentity { Source = "fixture", Stop = Stop("event"), Line = "S1" } }] });
        await first;
        Assert.Equal(["S1"], Assert.Single(cache.Entries).Lines);

        var second = card.RefreshAsync();
        service.Pending[1].SetResult(new ProviderResult<StopEvent> { Source = "partial", Warnings = ["partial-primary"], Items = [Event(DateTimeOffset.Now.AddMinutes(5)) with { Identity = new TripIdentity { Source = "fixture", Stop = Stop("event"), Line = "S2" } }] });
        await second;
        Assert.Equal(["S1"], Assert.Single(cache.Entries).Lines);
        Assert.Equal(["S1"], card.Lines);
        Assert.Equal("live", card.Result?.Source);
    }

    /// <summary>Loading cleans unrelated cache boards, and removing a favorite removes its matching board.</summary>
    [Fact]
    public async Task LoadAndRemoval_CleanOrphanedAndRemovedCacheBoards()
    {
        var kept = Stop("kept");
        var cache = new MemoryDepartureCache { Entries = [Entry(kept, "cached", DateTimeOffset.Now.AddMinutes(3)), Entry(Stop("orphan"), "orphan", DateTimeOffset.Now.AddMinutes(3))] };
        var home = new FavoriteHomeViewModel(new MemoryFavoriteStore(kept), () => new ControlledDepartureService(), new ControlledLocation(), cache: cache);

        await home.LoadAsync();

        Assert.Collection(cache.Entries, entry => Assert.Equal("kept", entry.StopId));
        await home.RemoveAsync(Assert.Single(home.Cards));
        Assert.Empty(cache.Entries);
    }

    private static ControlledDepartureService AddService(ICollection<ControlledDepartureService> services)
    {
        var service = new ControlledDepartureService();
        services.Add(service);
        return service;
    }

    private static Stop Stop(string id) => new() { Id = id, Source = "fixture", Name = id };

    private static DepartureCacheEntry Entry(Stop stop, string source, DateTimeOffset time) => new()
    {
        Source = stop.Source,
        StopId = stop.Id,
        Result = Result(source, time)
    };

    private static ProviderResult<StopEvent> Result(string source, DateTimeOffset time) => new() { Source = source, Items = [Event(time)] };

    private static StopEvent Event(DateTimeOffset time) => new()
    {
        PlannedTime = time,
        Identity = new TripIdentity { Source = "fixture", Stop = Stop("event") },
        Realtime = new RealtimeStatus { Delay = TimeSpan.Zero }
    };

    private sealed class MemoryFavoriteStore(params Stop[] stops) : IFavoriteStore
    {
        public Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Stop>>(stops);
        public Task SaveAsync(IReadOnlyList<Stop> desired, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class MemoryDepartureCache : IDepartureCacheStore
    {
        internal IReadOnlyList<DepartureCacheEntry> Entries { get; set; } = [];
        public Task<IReadOnlyList<DepartureCacheEntry>> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Entries);
        public Task SaveAsync(IReadOnlyList<DepartureCacheEntry> entries, CancellationToken cancellationToken = default) { Entries = entries.ToArray(); return Task.CompletedTask; }
        public Task UpsertAsync(DepartureCacheEntry entry, CancellationToken cancellationToken = default)
        {
            Entries = Entries.Where(current => current.Key != entry.Key).Append(entry).ToArray();
            return Task.CompletedTask;
        }
        public Task RemoveAsync(DepartureCacheKey key, CancellationToken cancellationToken = default)
        {
            Entries = Entries.Where(entry => entry.Key != key).ToArray();
            return Task.CompletedTask;
        }
        public Task RemoveOrphansAsync(IReadOnlyCollection<DepartureCacheKey> keys, CancellationToken cancellationToken = default)
        {
            Entries = Entries.Where(entry => keys.Contains(entry.Key)).ToArray();
            return Task.CompletedTask;
        }
    }
}
