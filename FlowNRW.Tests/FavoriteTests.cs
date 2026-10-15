using FlowNRW.Core.Favorites;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Durable favorite membership and independent, cancellable home boards.</summary>
public sealed class FavoriteTests
{
    /// <summary>Technical identities survive reload and deduplicate within their provider namespace.</summary>
    [Fact]
    public async Task StoreRoundTripPreservesIdentityAndCoordinates()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var first = StopAt("a", 51);
            var second = first with { Source = "other" };
            await new JsonFavoriteStore(path).SaveAsync([first, first, second]);
            Assert.Equal([first, second], await new JsonFavoriteStore(path).LoadAsync());
            await new JsonFavoriteStore(path).SaveAsync([]);
            Assert.Empty(await new JsonFavoriteStore(path).LoadAsync());
        }
        finally { File.Delete(path); }
    }

    /// <summary>Malformed or oversized existing files are preserved rather than overwritten.</summary>
    /// <param name="oversized">Whether the file exceeds the bounded storage size.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidFileCannotBeOverwritten(bool oversized)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var original = oversized ? new string(' ', 262145) : "{broken";
        try
        {
            await File.WriteAllTextAsync(path, original);
            var store = new JsonFavoriteStore(path);
            await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync());
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync([StopAt("a", 51)]));
            Assert.Equal(original, await File.ReadAllTextAsync(path));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Failed persistence never exposes a success or removes an existing card.</summary>
    [Fact]
    public async Task SaveFailuresPreserveMembershipAndAllowRetry()
    {
        var store = new MemoryFavorites { Fail = true };
        var home = new FavoriteHomeViewModel(store, () => new ControlledDepartureService(), new ControlledLocation());
        var stop = StopAt("a", 51);
        await home.ToggleAsync(stop);
        Assert.Empty(home.Cards);
        Assert.Contains("nicht gespeichert", home.Status);
        store.Fail = false; await home.ToggleAsync(stop);
        var card = Assert.Single(home.Cards);
        store.Fail = true; await home.RemoveAsync(card);
        Assert.Same(card, Assert.Single(home.Cards));
        store.Fail = false; await home.RemoveAsync(card);
        Assert.Empty(home.Cards);
        await home.RemoveAsync(card);
        Assert.Empty(home.Cards);
    }

    /// <summary>A slow card cannot block another board; duplicate refreshes do not issue more requests.</summary>
    [Fact]
    public async Task BoardsRefreshIndependentlyAndRejectLateResponses()
    {
        var services = new List<ControlledDepartureService>();
        var home = new FavoriteHomeViewModel(new MemoryFavorites { Stops = [StopAt("a", 51), StopAt("b", 52)] },
            () => { var service = new ControlledDepartureService(); services.Add(service); return service; }, new ControlledLocation());
        await home.LoadAsync();
        var first = home.Cards[0]; var second = home.Cards[1];
        var loading = home.RefreshMissingAsync();
        await first.RefreshAsync();
        Assert.Single(services[0].Pending);
        services[1].Pending[0].SetResult(new() { Source = "b" });
        Assert.False(second.IsBusy); Assert.True(first.IsBusy);
        await home.RemoveAsync(first);
        Assert.True(services[0].Tokens[0].IsCancellationRequested);
        services[0].Pending[0].SetResult(new() { Source = "late" });
        await loading;
        Assert.Null(first.Result); Assert.Same(second, Assert.Single(home.Cards));
        await home.RefreshMissingAsync();
        Assert.Single(services[1].Pending);
    }

    /// <summary>Failed refresh retains known data; leaving rejects an uncooperative late response.</summary>
    [Fact]
    public async Task RefreshFailureAndCancellationPreserveCompletedBoard()
    {
        var service = new ControlledDepartureService();
        var card = new FavoriteMonitorViewModel(StopAt("a", 51), service);
        var request = card.RefreshAsync();
        service.Pending[0].SetResult(new() { Source = "known", Items = [new() { PlannedTime = DateTimeOffset.Now.AddHours(1) }] });
        await request;
        var result = card.Result;
        request = card.RefreshAsync(); service.Pending[1].SetException(new IOException()); await request;
        Assert.Same(result, card.Result); Assert.Contains("Letzte bekannte", card.Status);
        request = card.RefreshAsync(); card.CancelPending();
        service.Pending[2].SetResult(new() { Source = "late" }); await request;
        Assert.Same(result, card.Result); Assert.False(card.IsBusy);
    }

    /// <summary>Known distances sort first; denied and cancelled positions cannot retain false distances.</summary>
    [Fact]
    public async Task DistanceSortIsExplicitStableAndDoesNotPersistPosition()
    {
        var store = new MemoryFavorites { Stops = [StopAt("far", 52), StopAt("unknown", 0) with { Coordinate = null }, StopAt("near", 51)] };
        var location = new ControlledLocation();
        var home = new FavoriteHomeViewModel(store, () => new ControlledDepartureService(), location);
        await home.LoadAsync(); Assert.Empty(location.Pending);
        var request = home.LocationCommand.ExecuteAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7))); await request;
        Assert.Equal(["near", "far", "unknown"], home.Cards.Select(card => card.Stop.Id));
        Assert.Equal(0, home.Cards[0].DistanceMeters);
        Assert.Null(home.Cards[2].DistanceMeters); Assert.Equal(0, store.Saves);
        Assert.Contains("fehlen Koordinaten", home.LocationStatus);
        request = home.LocationCommand.ExecuteAsync();
        location.Pending[1].SetResult(new(LocationStatus.Denied)); await request;
        Assert.Equal(["far", "unknown", "near"], home.Cards.Select(card => card.Stop.Id));
        Assert.All(home.Cards, card => Assert.Null(card.DistanceMeters));
        request = home.LocationCommand.ExecuteAsync(); home.CancelPending();
        location.Pending[2].SetResult(new(LocationStatus.Success, new(51, 7))); await request;
        Assert.All(home.Cards, card => Assert.Null(card.DistanceMeters));
    }

    /// <summary>The notice flag follows the displayed board's data quality.</summary>
    [Fact]
    public async Task HasDataNotice_ReflectsDisplayedResult()
    {
        var service = new ControlledDepartureService();
        var card = new FavoriteMonitorViewModel(StopAt("a", 51), service);
        Assert.False(card.HasDataNotice);
        var request = card.RefreshAsync();
        service.Pending[0].SetResult(new() { Source = "partial", Warnings = ["timeout"], Items = [new() { PlannedTime = DateTimeOffset.Now.AddHours(1) }] });
        await request;
        Assert.True(card.HasDataNotice);
        Assert.Contains("unvollständig", card.Status);
        request = card.RefreshAsync();
        service.Pending[1].SetResult(new() { Source = "ok", Items = [new() { PlannedTime = DateTimeOffset.Now.AddHours(1) }] });
        await request;
        Assert.False(card.HasDataNotice);
    }

    /// <summary>A fallback source or stale marker alone each raises the notice flag on the displayed board.</summary>
    /// <param name="fallback">Whether the response came from a secondary source.</param>
    /// <param name="stale">Whether the response is beyond its freshness period.</param>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task HasDataNotice_FlagsFallbackAndStale(bool fallback, bool stale)
    {
        var service = new ControlledDepartureService();
        var card = new FavoriteMonitorViewModel(StopAt("a", 51), service);
        Assert.False(card.HasDataNotice);
        var request = card.RefreshAsync();
        service.Pending[0].SetResult(new() { Source = "marked", IsFallback = fallback, IsStale = stale, Items = [new() { PlannedTime = DateTimeOffset.Now.AddHours(1) }] });
        await request;
        Assert.True(card.HasDataNotice);
        Assert.Equal("marked", card.Result?.Source);
    }

    /// <summary>Nearby labels prefer the provider distance, fall back to the air line and admit unknown distances.</summary>
    [Fact]
    public async Task NearbyDistanceLabel_PrefersProviderFallsBackToAirline()
    {
        var location = new ControlledLocation();
        var nearby = new ControlledNearby();
        var home = new FavoriteHomeViewModel(new MemoryFavorites(), () => new ControlledDepartureService(), location, nearby);
        await home.LoadAsync();
        var task = home.RefreshNearbyAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, new GeoCoordinate(51.4556, 7.0116)));
        var providerStop = new Stop { Id = "p", Source = "fixture", Name = "Provider", Coordinate = new GeoCoordinate(51.46, 7.01) };
        var airlineStop = new Stop { Id = "a", Source = "fixture", Name = "Airline", Coordinate = new GeoCoordinate(51.47, 7.01) };
        var unlocatedStop = new Stop { Id = "u", Source = "fixture", Name = "Unlocated" };
        nearby.Pending[0].SetResult(new() { Items = [new() { Stop = providerStop, DistanceMeters = 225 }, new() { Stop = airlineStop }, new() { Stop = unlocatedStop }] });
        await task;

        Assert.Equal(" · Entfernung: 225 m", home.NearbyDistanceLabel(home.NearbyStops[0]));
        Assert.Contains("Luftlinie", home.NearbyDistanceLabel(home.NearbyStops[1]));
        Assert.Equal(" · Entfernung unbekannt", home.NearbyDistanceLabel(home.NearbyStops[2]));
        Assert.Equal(" · Entfernung unbekannt", home.NearbyDistanceLabel(new Address { Name = "other" }));
    }

    /// <summary>Equal distances preserve stored order and stale map cards cannot open re-added identities.</summary>
    [Fact]
    public async Task EqualDistancesAndStaleMapMembership()
    {
        var location = new ControlledLocation();
        var home = new FavoriteHomeViewModel(new MemoryFavorites { Stops = [StopAt("b", 51), StopAt("a", 51)] },
            () => new ControlledDepartureService(), location);
        await home.LoadAsync();
        var sorting = home.LocationCommand.ExecuteAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7))); await sorting;
        Assert.Equal(["b", "a"], home.Cards.Select(card => card.Stop.Id));
        var navigation = new DepartureTestNavigation();
        var service = new ControlledDepartureService();
        var monitor = new StopMonitorViewModel(new ControlledSearchService(), service, navigation, 100);
        var map = new MapViewModel(monitor); map.ShowFavorites(home);
        var session = map.Session; var old = home.Cards[0];
        await home.RemoveAsync(old); await home.ToggleAsync(old.Stop);
        await map.SelectAsync(session, 0);
        await monitor.OpenFavoriteAsync(home, old);
        Assert.Equal(0, navigation.Opens); Assert.Empty(service.Pending);
        map.ShowFavorites(home);
        await map.SelectAsync(session, 0);
        Assert.Equal(0, navigation.Opens);
        var opening = map.SelectAsync(map.Session, 1);
        Assert.Equal(1, navigation.Opens);
        Assert.Same(old.Stop, Assert.Single(service.Stops));
        service.Pending[0].SetResult(new()); await opening;
    }

    private static Stop StopAt(string id, double latitude) => new() { Id = id, Source = "test", Name = id, Dhid = "de:" + id, Coordinate = new(latitude, 7) };

    private sealed class MemoryFavorites : IFavoriteStore
    {
        internal IReadOnlyList<Stop> Stops { get; set; } = [];
        internal bool Fail { get; set; }
        internal int Saves { get; private set; }
        public Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Stops);
        public Task SaveAsync(IReadOnlyList<Stop> stops, CancellationToken cancellationToken = default)
        {
            if (Fail) throw new IOException();
            Stops = stops.ToArray(); Saves++; return Task.CompletedTask;
        }
    }
}
