using FlowNRW.Core.Favorites;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Shared persistent-board restore facade: membership, item filtering, freshness and bundled reads.</summary>
public sealed class DepartureBoardCacheTests
{
    /// <summary>A stop that is not a saved favorite yields no board and never touches the cache.</summary>
    [Fact]
    public async Task RestoreBoardAsync_NonFavoriteReturnsNullWithoutBoard()
    {
        var stop = Stop("unsaved");
        var favorites = new RecordingFavorites();
        var cache = new RecordingDepartureCache(Entry(stop, "cached", DateTimeOffset.Now.AddMinutes(5)));
        var boards = new DepartureBoardCache(cache, favorites);

        var board = await boards.RestoreBoardAsync(stop);

        Assert.Null(board);
        Assert.Equal(1, favorites.Loads);
        Assert.Equal(0, cache.Loads);
    }

    /// <summary>A saved favorite without a cache entry restores nothing.</summary>
    [Fact]
    public async Task RestoreBoardAsync_FavoriteWithoutEntryReturnsNull()
    {
        var stop = Stop("saved");
        var cache = new RecordingDepartureCache();
        var boards = new DepartureBoardCache(cache, new RecordingFavorites(stop));

        Assert.Null(await boards.RestoreBoardAsync(stop));
        Assert.Equal(1, cache.Loads);
    }

    /// <summary>Timeless and expired departures are dropped; future departures keep effective-time order.</summary>
    [Fact]
    public async Task RestoreBoardAsync_DropsTimelessAndExpiredItems()
    {
        var stop = Stop("saved");
        var future = Event(DateTimeOffset.Now.AddMinutes(3));
        var later = Event(DateTimeOffset.Now.AddMinutes(8));
        var entry = new DepartureCacheEntry
        {
            Source = stop.Source,
            StopId = stop.Id,
            Result = new() { Source = "cached", Items = [later, Event(null), Event(DateTimeOffset.Now.AddMinutes(-2)), future] }
        };
        var boards = new DepartureBoardCache(new RecordingDepartureCache(entry), new RecordingFavorites(stop));

        var board = await boards.RestoreBoardAsync(stop);

        var result = board?.Result;
        Assert.NotNull(result);
        Assert.Equal(new[] { future, later }, result.Items);
    }

    /// <summary>A stale persisted board is displayed and marked instead of being discarded.</summary>
    [Fact]
    public async Task RestoreBoardAsync_StaleBoardIsReturnedMarked()
    {
        var stop = Stop("saved");
        var clock = new TransitTestClock { Now = DateTimeOffset.UtcNow };
        var freshness = new FlowNRW.Core.Refresh.RefreshFreshness(new TransitCacheOptions { RealtimeTimeToLive = TimeSpan.FromSeconds(1) }, clock);
        var entry = new DepartureCacheEntry
        {
            Source = stop.Source,
            StopId = stop.Id,
            Result = new() { Source = "stale", RetrievedAt = clock.Now.AddMinutes(-1), Items = [Event(DateTimeOffset.Now.AddMinutes(5))] }
        };
        var boards = new DepartureBoardCache(new RecordingDepartureCache(entry), new RecordingFavorites(stop), freshness);

        var board = await boards.RestoreBoardAsync(stop);

        var result = board?.Result;
        Assert.NotNull(result);
        Assert.Equal("stale", result.Source);
        Assert.True(result.IsStale);
        Assert.Single(result.Items);
    }

    /// <summary>A board without future departures keeps only its stored or derived line inventory.</summary>
    [Fact]
    public async Task RestoreBoardAsync_OnlyExpiredKeepsLineInventory()
    {
        var stored = Stop("stored");
        var derived = Stop("derived");
        var boards = new DepartureBoardCache(new RecordingDepartureCache(
            Entry(stored, "expired", DateTimeOffset.Now.AddMinutes(-1)) with { Lines = ["S1"] },
            new DepartureCacheEntry
            {
                Source = derived.Source,
                StopId = derived.Id,
                Result = new() { Source = "expired", Items = [Event(DateTimeOffset.Now.AddMinutes(-1)) with { Line = new Line { Name = "RE 9" } }] }
            }), new RecordingFavorites(stored, derived));

        var storedBoard = await boards.RestoreBoardAsync(stored);
        var derivedBoard = await boards.RestoreBoardAsync(derived);

        Assert.Null(storedBoard?.Result);
        Assert.Equal(["S1"], storedBoard?.Lines);
        Assert.Null(derivedBoard?.Result);
        Assert.Equal(["RE 9"], derivedBoard?.Lines);
    }

    /// <summary>The bulk restore reads the cache once and projects only the supplied membership keys.</summary>
    [Fact]
    public async Task RestoreBoardsAsync_ProjectsOnlyGivenKeys()
    {
        var kept = Stop("kept");
        var orphan = Stop("orphan");
        var favorites = new RecordingFavorites(kept, orphan);
        var cache = new RecordingDepartureCache(
            Entry(kept, "cached", DateTimeOffset.Now.AddMinutes(4)),
            Entry(orphan, "orphan", DateTimeOffset.Now.AddMinutes(4)));
        var boards = new DepartureBoardCache(cache, favorites);

        var restored = await boards.RestoreBoardsAsync([new DepartureCacheKey(kept.Source, kept.Id)]);

        var pair = Assert.Single(restored);
        Assert.Equal(new DepartureCacheKey(kept.Source, kept.Id), pair.Key);
        Assert.Equal("cached", pair.Value.Result?.Source);
        Assert.Equal(1, cache.Loads);
        Assert.Equal(0, favorites.Loads);
    }

    /// <summary>A pre-cancelled token aborts the bundled read before any storage access.</summary>
    [Fact]
    public async Task RestoreBoardAsync_HonorsCancellation()
    {
        var stop = Stop("saved");
        var favorites = new RecordingFavorites(stop);
        var cache = new RecordingDepartureCache(Entry(stop, "cached", DateTimeOffset.Now.AddMinutes(4)));
        var boards = new DepartureBoardCache(cache, favorites);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => boards.RestoreBoardAsync(stop, cancelled.Token));
        Assert.Equal(0, favorites.Loads);
        Assert.Equal(0, cache.Loads);
    }

    private static Stop Stop(string id) => new() { Id = id, Source = "fixture", Name = id };

    private static DepartureCacheEntry Entry(Stop stop, string source, DateTimeOffset time) => new()
    {
        Source = stop.Source,
        StopId = stop.Id,
        Result = new() { Source = source, Items = [Event(time)] }
    };

    private static StopEvent Event(DateTimeOffset? time) => new()
    {
        PlannedTime = time,
        Identity = new TripIdentity { Source = "fixture", Stop = Stop("event"), Line = "S2" },
        Realtime = new RealtimeStatus { Delay = TimeSpan.Zero }
    };

    private sealed class RecordingFavorites(params Stop[] stops) : IFavoriteStore
    {
        internal int Loads;
        public Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default)
        {
            Loads++;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<Stop>>(stops);
        }
        public Task SaveAsync(IReadOnlyList<Stop> favorites, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RecordingDepartureCache(params DepartureCacheEntry[] entries) : IDepartureCacheStore
    {
        internal int Loads;
        public Task<IReadOnlyList<DepartureCacheEntry>> LoadAsync(CancellationToken cancellationToken = default)
        {
            Loads++;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<DepartureCacheEntry>>(entries);
        }
        public Task UpsertAsync(DepartureCacheEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(DepartureCacheKey key, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveOrphansAsync(IReadOnlyCollection<DepartureCacheKey> keys, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
