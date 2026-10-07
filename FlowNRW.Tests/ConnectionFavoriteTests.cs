using FlowNRW.Core.Favorites;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Explicit journey favorites and navigation-state retention.</summary>
public sealed class ConnectionFavoriteTests
{
    /// <summary>A directed pair survives reload, while a duplicate does not create a second record.</summary>
    [Fact]
    public async Task StoreRoundTripKeepsDirectedPair()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var first = Stop("a"); var second = Stop("b");
            var store = new JsonConnectionFavoriteStore(path);
            await store.SaveAsync([new() { Origin = first, Destination = second }, new() { Origin = first, Destination = second }]);
            var saved = Assert.Single(await store.LoadAsync());
            Assert.Equal("a → b", saved.Name);
            Assert.Equal(first, saved.Origin); Assert.Equal(second, saved.Destination);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Saving and selecting a connection does not perform endpoint or routing searches, and swap retains time options.</summary>
    [Fact]
    public async Task SelectionAndSwapKeepResolvedStopsWithoutRequests()
    {
        var routing = new ControlledRoutingService();
        var model = new JourneySearchViewModel(new(new ControlledSearchService(), 100), new(new ControlledSearchService(), 100), routing, new RecordingNavigation())
        {
            UseCurrentTime = false,
            SelectedDate = new DateTime(2026, 10, 4),
            SelectedTime = TimeSpan.FromHours(12),
            ArriveBy = true
        };
        var first = new Address { Name = "a", Stop = Stop("a") };
        var second = new Address { Name = "b", Stop = Stop("b") };
        model.SelectConnection(first, second);
        model.SwapEndpoints();
        Assert.Equal("b", model.Origin.SelectedAddress?.Name);
        Assert.Equal("a", model.Destination.SelectedAddress?.Name);
        Assert.True(model.ArriveBy); Assert.False(model.UseCurrentTime);
        Assert.Empty(routing.Pending);
    }

    /// <summary>Opening a stop retains its list candidate and reuses a completed session board before refreshing.</summary>
    [Fact]
    public async Task StopSearchAndSessionBoardAreRetained()
    {
        var search = new ControlledSearchService(); var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 100);
        var candidate = new Address { Name = "Essen", Stop = Stop("Essen") };
        model.Lookup.Text = "Essen";
        var lookup = model.Lookup.SearchAsync(); search.Pending[0].SetResult(new() { Items = [candidate] }); await lookup;
        var first = model.OpenAsync(candidate);
        departures.Pending[0].SetResult(new() { Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(10) }] }); await first;
        Assert.Same(candidate, Assert.Single(model.Stops)); Assert.Equal("Essen", model.Lookup.Text);
        var second = model.OpenAsync(candidate);
        Assert.Single(model.Items);
        departures.Pending[1].SetResult(new() { Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(11) }] }); await second;
    }

    /// <summary>A stored favorite board is visible through the stop search before its current provider refresh finishes.</summary>
    [Fact]
    public async Task StopSearch_UsesPersistentFavoriteBoardBeforeRefresh()
    {
        var stop = Stop("saved");
        var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(new ControlledSearchService(), departures, new DepartureTestNavigation(), 100,
            favoriteStore: new MemoryFavorites(stop), departureCache: new MemoryDepartureCache(new DepartureCacheEntry
            {
                Source = stop.Source,
                StopId = stop.Id,
                Result = new() { Source = "cached", Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(8) }] }
            }));
        var opening = model.OpenNearbyFromHomeAsync(new Address { Name = stop.Name, Stop = stop });
        Assert.Equal("cached", model.Result?.Source);
        departures.Pending[0].SetResult(new() { Source = "live", Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(9) }] });
        await opening;
        Assert.Equal("live", model.Result?.Source);
    }

    /// <summary>An expired persistent board is not displayed before the live request.</summary>
    [Fact]
    public async Task StopSearch_DoesNotDisplayStalePersistentBoard()
    {
        var stop = Stop("stale");
        var departures = new ControlledDepartureService();
        var clock = new TransitTestClock { Now = DateTimeOffset.UtcNow };
        var freshness = new FlowNRW.Core.Refresh.RefreshFreshness(new TransitCacheOptions { RealtimeTimeToLive = TimeSpan.FromSeconds(1) }, clock);
        var model = new StopMonitorViewModel(new ControlledSearchService(), departures, new DepartureTestNavigation(), 100,
            favoriteStore: new MemoryFavorites(stop), departureCache: new MemoryDepartureCache(new DepartureCacheEntry
            {
                Source = stop.Source,
                StopId = stop.Id,
                Result = new() { Source = "stale", RetrievedAt = clock.Now.AddMinutes(-1), Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(8) }] }
            }), freshness: freshness);
        var opening = model.OpenNearbyFromHomeAsync(new Address { Name = stop.Name, Stop = stop });
        Assert.Null(model.Result);
        departures.Pending[0].SetResult(new() { Source = "live", Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(9) }] });
        await opening;
        Assert.Equal("live", model.Result?.Source);
    }

    /// <summary>A partial response cannot replace a complete retained stop board.</summary>
    [Fact]
    public async Task StopSearch_PartialRefreshRetainsCompleteBoard()
    {
        var service = new ControlledDepartureService();
        var stop = Stop("complete");
        var model = new StopMonitorViewModel(new ControlledSearchService(), service, new DepartureTestNavigation(), 100);
        var opening = model.OpenNearbyFromHomeAsync(new Address { Name = stop.Name, Stop = stop });
        service.Pending[0].SetResult(new() { Source = "complete", Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(8) }] }); await opening;
        var refresh = model.RefreshAsync();
        service.Pending[1].SetResult(new() { Source = "partial", Warnings = ["partial-primary"], Items = [new StopEvent { PlannedTime = DateTimeOffset.Now.AddMinutes(9) }] }); await refresh;
        Assert.Equal("complete", model.Result?.Source);
        Assert.Contains("Letzte bekannte", model.Status);
    }

    private static Stop Stop(string id) => new() { Id = id, Source = "fixture", Name = id };

    private sealed class MemoryFavorites(params Stop[] stops) : IFavoriteStore
    {
        public Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Stop>>(stops);
        public Task SaveAsync(IReadOnlyList<Stop> favorites, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class MemoryDepartureCache(params DepartureCacheEntry[] entries) : IDepartureCacheStore
    {
        public Task<IReadOnlyList<DepartureCacheEntry>> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DepartureCacheEntry>>(entries);
        public Task SaveAsync(IReadOnlyList<DepartureCacheEntry> value, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpsertAsync(DepartureCacheEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(DepartureCacheKey key, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveOrphansAsync(IReadOnlyCollection<DepartureCacheKey> keys, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
