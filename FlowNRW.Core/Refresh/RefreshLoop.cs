namespace FlowNRW.Core.Refresh;

/// <summary>One cancellable completion-relative foreground refresh loop.</summary>
public sealed class RefreshLoop : IDisposable
{
    private readonly Func<Task> refresh;
    private readonly Action cancel;
    private readonly Func<TimeSpan, CancellationToken, Task> delay;
    private CancellationTokenSource? request;
    private long revision;
    private int interval;
    private bool disposed;

    /// <summary>Creates an inactive loop whose continuations retain the calling context.</summary>
    /// <param name="refresh">One awaited monitor update.</param>
    /// <param name="cancel">Invalidates and cancels the monitor's pending request.</param>
    /// <param name="delay">Optional deterministic test delay.</param>
    public RefreshLoop(Func<Task> refresh, Action cancel, Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        this.refresh = refresh; this.cancel = cancel; this.delay = delay ?? Task.Delay;
    }

    /// <summary>Starts one delayed loop, or replaces an existing interval without an immediate request.</summary>
    /// <param name="seconds">A supported interval, with zero meaning off.</param>
    public void Start(int seconds)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!RefreshSettingsViewModel.IsSupported(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
        if (seconds == 0) { Stop(); return; }
        if (request is not null && interval == seconds) return;
        Stop();
        interval = seconds;
        var source = new CancellationTokenSource(); request = source;
        var version = ++revision;
        _ = RunAsync(seconds, version, source);
    }

    /// <summary>Stops the active delay and invalidates its pending refresh, once.</summary>
    public void Stop()
    {
        if (request is not { } source) return;
        revision++; request = null;
        try { source.Cancel(); }
        catch (AggregateException) { }
        try { cancel(); }
        catch (Exception) { }
    }

    /// <summary>Stops and permanently releases the loop.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; Stop();
    }

    private async Task RunAsync(int seconds, long version, CancellationTokenSource source)
    {
        try
        {
            while (version == revision && !source.IsCancellationRequested)
            {
                await delay(TimeSpan.FromSeconds(seconds), source.Token);
                if (version != revision || source.IsCancellationRequested) return;
                try { await refresh(); }
                catch (Exception) { }
            }
        }
        catch (Exception) { }
        finally
        {
            if (version == revision) request = null;
            source.Dispose();
        }
    }
}
