using System.ComponentModel;
using FlowNRW.Core.Favorites;
using FlowNRW.Core.Presentation;

namespace FlowNRW.Core.Refresh;

/// <summary>Coordinates bounded background work, foreground ownership and the protected per-area resume entry.</summary>
public sealed class RefreshLifecycle
{
    private readonly ForegroundState foreground;
    private readonly RefreshSettingsViewModel settings;
    private readonly FavoriteHomeViewModel favorites;
    private readonly StopMonitorViewModel monitor;
    private readonly JourneySearchViewModel journeys;
    private readonly RefreshFreshness freshness;
    private readonly TimeProvider clock;
    private readonly Dictionary<RefreshArea, object> visible = [];
    private readonly Dictionary<RefreshArea, CancellationTokenSource> resume = [];
    private CancellationTokenSource? background;

    /// <summary>Composes lifecycle coordination without loading data or accessing location.</summary>
    /// <param name="foreground">Shared window activity.</param>
    /// <param name="settings">Committed automatic refresh preference.</param>
    /// <param name="favorites">Existing favorite session.</param>
    /// <param name="monitor">Existing single-stop monitor session.</param>
    /// <param name="journeys">Existing journey search session.</param>
    /// <param name="freshness">Shared realtime age policy.</param>
    /// <param name="clock">Optional clock for the bounded execution deadline.</param>
    public RefreshLifecycle(ForegroundState foreground, RefreshSettingsViewModel settings,
        FavoriteHomeViewModel favorites, StopMonitorViewModel monitor, JourneySearchViewModel journeys,
        RefreshFreshness freshness, TimeProvider? clock = null)
    {
        this.foreground = foreground;
        this.settings = settings;
        this.favorites = favorites;
        this.monitor = monitor;
        this.journeys = journeys;
        this.freshness = freshness;
        this.clock = clock ?? TimeProvider.System;
        foreground.PropertyChanged += ForegroundChanged;
        favorites.PropertyChanged += FavoritesChanged;
    }

    /// <summary>Raised after a started area resume finished, so pages can reconcile their refresh loops.</summary>
    public event EventHandler<RefreshAreaEventArgs>? ResumeCompleted;

    /// <summary>Raised when an area occasion or resume fails while the area stays visible.</summary>
    public event EventHandler<RefreshFailedEventArgs>? RefreshFailed;

    /// <summary>Registers an area as displayed under its owning page and starts its appearing occasion.</summary>
    /// <param name="area">Display area becoming visible.</param>
    /// <param name="owner">Owning page instance; a later registration replaces the current owner.</param>
    /// <returns>The occasion's completion; monitor reports visibility only.</returns>
    public Task NotifyVisibleAsync(RefreshArea area, object owner)
    {
        visible[area] = owner;
        return area switch
        {
            RefreshArea.Favorites when foreground.IsActive
                => RunOccasionAsync(area, RefreshOccasion.Startup,
                    () => favorites.RefreshForOccasionAsync(RefreshOccasion.Startup, freshness)),
            RefreshArea.Journeys => ResumeAsync(area),
            _ => Task.CompletedTask
        };
    }

    /// <summary>Unregisters an area only when the caller owns its registration, then aborts its running resume and pending refreshable work.</summary>
    /// <param name="area">Display area becoming hidden.</param>
    /// <param name="owner">Page instance that registered the area.</param>
    public void NotifyHidden(RefreshArea area, object owner)
    {
        if (!visible.TryGetValue(area, out var current) || !ReferenceEquals(current, owner)) return;
        visible.Remove(area);
        CancelArea(area);
    }

    /// <summary>Whether a protected resume of the area is currently running.</summary>
    /// <param name="area">Display area to check.</param>
    /// <returns>Resume state for refresh-loop reconciliation.</returns>
    public bool IsResuming(RefreshArea area) => resume.ContainsKey(area);

    /// <summary>Ends background ownership before allowing any foreground refresh.</summary>
    /// <param name="active">Whether the application window can perform foreground work.</param>
    public void SetActive(bool active)
    {
        if (active && background is { } source)
        {
            source.Cancel();
            favorites.CancelPending();
        }
        foreground.IsActive = active;
    }

    /// <summary>Runs at most one finite background refresh, respecting automatic refresh being disabled.</summary>
    /// <param name="expiration">Operating-system expiration or application shutdown.</param>
    /// <returns>Whether the eligible work completed successfully within its lifetime.</returns>
    public async Task<bool> RunBackgroundAsync(CancellationToken expiration = default)
    {
        if (foreground.IsActive || background is not null || expiration.IsCancellationRequested) return false;
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20), clock);
        using var source = CancellationTokenSource.CreateLinkedTokenSource(expiration, deadline.Token);
        background = source;
        try
        {
            await settings.LoadAsync().WaitAsync(source.Token);
            source.Token.ThrowIfCancellationRequested();
            if (settings.IntervalSeconds == 0) return true;
            var success = await favorites.RefreshForOccasionAsync(RefreshOccasion.ForegroundReturn, freshness, source.Token).WaitAsync(source.Token);
            return success && !source.IsCancellationRequested;
        }
        catch (OperationCanceledException) { return false; }
        catch (Exception) { return false; }
        finally
        {
            if (source.IsCancellationRequested && !foreground.IsActive) favorites.CancelPending();
            background = null;
        }
    }

    private async Task ResumeAsync(RefreshArea area)
    {
        if (!visible.ContainsKey(area) || !foreground.IsActive || !settings.IsLoaded
            || settings.IntervalSeconds == 0 || resume.ContainsKey(area)) return;
        using var source = new CancellationTokenSource();
        resume[area] = source;
        try
        {
            await RunOccasionAsync(area, RefreshOccasion.ForegroundReturn, () => area switch
            {
                RefreshArea.Favorites => favorites.RefreshForOccasionAsync(RefreshOccasion.ForegroundReturn, freshness, source.Token),
                RefreshArea.Monitor => monitor.RefreshIfStaleAsync(freshness, source.Token),
                _ => journeys.RefreshIfStaleAsync(freshness, source.Token)
            });
        }
        finally
        {
            if (ReferenceEquals(resume.GetValueOrDefault(area), source))
            {
                resume.Remove(area);
                try { ResumeCompleted?.Invoke(this, new RefreshAreaEventArgs(area)); }
                catch (Exception) { }
            }
        }
    }

    private async Task RunOccasionAsync(RefreshArea area, RefreshOccasion occasion, Func<Task> step)
    {
        try { await step(); }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            if (visible.ContainsKey(area))
            {
                try { RefreshFailed?.Invoke(this, new RefreshFailedEventArgs(area, occasion)); }
                catch (Exception) { }
            }
        }
    }

    private void CancelArea(RefreshArea area)
    {
        if (resume.Remove(area, out var source)) source.Cancel();
        switch (area)
        {
            case RefreshArea.Favorites: favorites.CancelPending(); break;
            case RefreshArea.Monitor: monitor.CancelPending(); break;
            case RefreshArea.Journeys: journeys.CancelPending(); break;
        }
    }

    private void ForegroundChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is not (null or nameof(ForegroundState.IsActive))) return;
        if (foreground.IsActive)
        {
            foreach (var area in visible.Keys.ToArray()) _ = ResumeAsync(area);
        }
        else
        {
            foreach (var area in visible.Keys.ToArray()) CancelArea(area);
        }
    }

    private void FavoritesChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(FavoriteHomeViewModel.Cards)
            && visible.ContainsKey(RefreshArea.Favorites) && foreground.IsActive
            && !IsResuming(RefreshArea.Favorites))
        {
            _ = RunOccasionAsync(RefreshArea.Favorites, RefreshOccasion.CardsAdded,
                () => favorites.RefreshForOccasionAsync(RefreshOccasion.CardsAdded, freshness));
        }
    }
}
