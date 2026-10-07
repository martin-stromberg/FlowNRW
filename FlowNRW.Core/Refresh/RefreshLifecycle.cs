using FlowNRW.Core.Favorites;

namespace FlowNRW.Core.Refresh;

/// <summary>Coordinates bounded background work and foreground ownership on the application's UI context.</summary>
public sealed class RefreshLifecycle
{
    private readonly ForegroundState foreground;
    private readonly RefreshSettingsViewModel settings;
    private readonly FavoriteHomeViewModel favorites;
    private readonly RefreshFreshness freshness;
    private readonly TimeProvider clock;
    private CancellationTokenSource? background;

    /// <summary>Composes lifecycle coordination without loading data or accessing location.</summary>
    /// <param name="foreground">Shared window activity.</param>
    /// <param name="settings">Committed automatic refresh preference.</param>
    /// <param name="favorites">Existing favorite session.</param>
    /// <param name="freshness">Shared realtime age policy.</param>
    /// <param name="clock">Optional clock for the bounded execution deadline.</param>
    public RefreshLifecycle(ForegroundState foreground, RefreshSettingsViewModel settings,
        FavoriteHomeViewModel favorites, RefreshFreshness freshness, TimeProvider? clock = null)
    {
        this.foreground = foreground;
        this.settings = settings;
        this.favorites = favorites;
        this.freshness = freshness;
        this.clock = clock ?? TimeProvider.System;
    }

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
            var success = await favorites.RefreshStaleAsync(freshness, source.Token).WaitAsync(source.Token);
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
}
