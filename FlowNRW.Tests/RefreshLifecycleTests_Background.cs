using FlowNRW.Core.Favorites;
using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Background work is finite, isolated from position and subordinate to foreground ownership.</summary>
public sealed class RefreshLifecycleTests_Background
{
    /// <summary>The internal execution deadline cancels even a provider that ignores its token.</summary>
    [Fact]
    public async Task RunBackgroundAsync_Deadline_EndsWithoutProviderCompletion()
    {
        var clock = new BackgroundDeadlineClock();
        var fixture = new LifecycleFixture(1, clock: clock);
        var pending = fixture.Lifecycle.RunBackgroundAsync();
        Assert.Equal(TimeSpan.FromSeconds(20), clock.DueTime);
        clock.Expire();
        Assert.False(await pending.WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.True(fixture.Services[0].Tokens[0].IsCancellationRequested);
        fixture.Complete(0);
        Assert.Null(fixture.Home.Cards[0].Result);
        Assert.Equal(0, fixture.Store.Saves);
    }

    /// <summary>An empty saved list completes without fetching or accessing position.</summary>
    [Fact]
    public async Task RunBackgroundAsync_EmptyFavorites_CompletesWithoutFetching()
    {
        var fixture = new LifecycleFixture(0);
        Assert.True(await fixture.Lifecycle.RunBackgroundAsync());
        Assert.Empty(fixture.Services);
        Assert.Empty(fixture.Location.Pending);
        Assert.Equal(0, fixture.Store.Saves);
    }

    /// <summary>Invalid saved identities fail without replacing the stored list.</summary>
    [Fact]
    public async Task RunBackgroundAsync_InvalidFavorites_DoesNotFetchOrSave()
    {
        var fixture = new LifecycleFixture(1);
        fixture.Store.Stops = [new() { Id = "", Source = "fixture" }];
        Assert.False(await fixture.Lifecycle.RunBackgroundAsync());
        Assert.Empty(fixture.Services);
        Assert.Empty(fixture.Home.Cards);
        Assert.Equal(0, fixture.Store.Saves);
    }

    /// <summary>A cancelled store load cannot populate cards later or block the next foreground load.</summary>
    [Fact]
    public async Task RunBackgroundAsync_CancelledLoad_DiscardsLateSavedList()
    {
        var fixture = new LifecycleFixture(1);
        var load = new TaskCompletionSource<IReadOnlyList<Stop>>();
        fixture.Store.PendingLoad = load.Task;
        using var expiration = new CancellationTokenSource();
        var pending = fixture.Lifecycle.RunBackgroundAsync(expiration.Token);
        expiration.Cancel();
        Assert.False(await pending.WaitAsync(TimeSpan.FromSeconds(2)));
        load.SetResult([new() { Id = "late", Source = "fixture" }]);
        Assert.Empty(fixture.Home.Cards);
        fixture.Store.PendingLoad = null;
        await fixture.Home.LoadAsync().WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("0", Assert.Single(fixture.Home.Cards).Stop.Id);
        Assert.Equal(0, fixture.Store.Saves);
    }

    /// <summary>Disabled automatic refresh performs no provider, favorite or position access.</summary>
    [Fact]
    public async Task RunBackgroundAsync_Disabled_DoesNotLoadFavorites()
    {
        var fixture = new LifecycleFixture(1, 0);
        Assert.True(await fixture.Lifecycle.RunBackgroundAsync());
        Assert.Equal(0, fixture.Store.Loads);
        Assert.Empty(fixture.Services);
        Assert.Empty(fixture.Location.Pending);
    }

    /// <summary>Foreground ownership excludes a background request.</summary>
    [Fact]
    public async Task RunBackgroundAsync_Active_DoesNotStart()
    {
        var fixture = new LifecycleFixture(1);
        fixture.Lifecycle.SetActive(true);
        Assert.False(await fixture.Lifecycle.RunBackgroundAsync());
        Assert.Equal(0, fixture.Store.Loads);
    }

    /// <summary>Only four cards run at once; completion opens the next batch without writing or locating.</summary>
    [Fact]
    public async Task RunBackgroundAsync_ManyFavorites_BoundsConcurrency()
    {
        var fixture = new LifecycleFixture(5);
        var pending = fixture.Lifecycle.RunBackgroundAsync();
        Assert.Equal(4, fixture.Services.Sum(service => service.Pending.Count));
        Assert.False(await fixture.Lifecycle.RunBackgroundAsync());
        for (var i = 0; i < 4; i++) fixture.Complete(i);
        Assert.Single(fixture.Services[4].Pending);
        fixture.Complete(4);
        Assert.True(await pending);
        Assert.Equal(0, fixture.Store.Saves);
        Assert.Empty(fixture.Location.Pending);
    }

    /// <summary>OS expiry finishes promptly even when a provider ignores cancellation.</summary>
    [Fact]
    public async Task RunBackgroundAsync_Expiration_RejectsLateResult()
    {
        var fixture = new LifecycleFixture(1);
        using var expiration = new CancellationTokenSource();
        var pending = fixture.Lifecycle.RunBackgroundAsync(expiration.Token);
        expiration.Cancel();
        Assert.False(await pending.WaitAsync(TimeSpan.FromSeconds(2)));
        fixture.Complete(0);
        Assert.Null(fixture.Home.Cards[0].Result);
        Assert.False(fixture.Home.Cards[0].IsBusy);
        Assert.True(fixture.Services[0].Tokens[0].IsCancellationRequested);
    }

    /// <summary>Background cancellation precedes foreground notification and permits a fresh foreground request.</summary>
    [Fact]
    public async Task SetActive_BackgroundRunning_HandsOffBeforeNotification()
    {
        var fixture = new LifecycleFixture(1);
        var pending = fixture.Lifecycle.RunBackgroundAsync();
        var cancelledWhenActivated = false;
        fixture.Foreground.PropertyChanged += (_, _) => cancelledWhenActivated = fixture.Services[0].Tokens[0].IsCancellationRequested;
        fixture.Lifecycle.SetActive(true);
        Assert.True(cancelledWhenActivated);
        Assert.False(await pending);
        var current = fixture.Home.Cards[0].RefreshAsync();
        fixture.Services[0].Pending[1].SetResult(new() { Source = "foreground", RetrievedAt = fixture.Clock.Now });
        await current;
        fixture.Complete(0);
        Assert.Equal("foreground", fixture.Home.Cards[0].Result?.Source);
    }

    /// <summary>A failed provider produces a failed completion without exposing exception details.</summary>
    [Fact]
    public async Task RunBackgroundAsync_ProviderFailure_ReturnsFailure()
    {
        var fixture = new LifecycleFixture(1);
        var pending = fixture.Lifecycle.RunBackgroundAsync();
        fixture.Services[0].Pending[0].SetException(new IOException("private"));
        Assert.False(await pending);
        Assert.DoesNotContain("private", fixture.Home.Cards[0].Status);
        Assert.Equal(0, fixture.Store.Saves);
    }
}

internal sealed class LifecycleFixture
{
    internal TransitTestClock Clock { get; } = new();
    internal LifecycleFavorites Store { get; }
    internal ControlledLocation Location { get; } = new();
    internal List<ControlledDepartureService> Services { get; } = [];
    internal FavoriteHomeViewModel Home { get; }
    internal ForegroundState Foreground { get; } = new();
    internal RefreshLifecycle Lifecycle { get; }

    internal LifecycleFixture(int count, int interval = 60, TimeProvider? clock = null)
    {
        Store = new() { Stops = Enumerable.Range(0, count).Select(i => new Stop { Id = i.ToString(), Source = "fixture" }).ToArray() };
        Home = new(Store, () => { var service = new ControlledDepartureService(); Services.Add(service); return service; }, Location);
        Lifecycle = new(Foreground, new(new ControlledRefreshSettingsStore { Loaded = interval }), Home, new(new(), Clock), clock);
    }

    internal void Complete(int index) => Services[index].Pending[0].SetResult(new() { RetrievedAt = Clock.Now });
}

internal sealed class LifecycleFavorites : IFavoriteStore
{
    internal IReadOnlyList<Stop> Stops { get; set; } = [];
    internal int Loads { get; private set; }
    internal int Saves { get; private set; }
    internal Task<IReadOnlyList<Stop>>? PendingLoad { get; set; }
    public Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default) { Loads++; return PendingLoad ?? Task.FromResult(Stops); }
    public Task SaveAsync(IReadOnlyList<Stop> stops, CancellationToken cancellationToken = default) { Saves++; return Task.CompletedTask; }
}

internal sealed class BackgroundDeadlineClock : TimeProvider
{
    private Action? expire;
    internal TimeSpan DueTime { get; private set; }
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        DueTime = dueTime;
        expire = () => callback(state);
        return new DeadlineTimer(due => DueTime = due, () => expire = null);
    }
    internal void Expire() => expire?.Invoke();

    private sealed class DeadlineTimer(Action<TimeSpan> change, Action dispose) : ITimer
    {
        private bool disposed;
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (disposed || period != Timeout.InfiniteTimeSpan) return false;
            change(dueTime);
            return true;
        }
        public void Dispose() { disposed = true; dispose(); }
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
