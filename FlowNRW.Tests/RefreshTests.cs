using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>Foreground refresh settings and loop bounds.</summary>
public sealed class RefreshTests
{
    /// <summary>Supported values survive a new settings view model.</summary>
    [Fact]
    public async Task SettingsRoundTripAndInvalidValuesStayOut()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonRefreshSettingsStore(path);
            var settings = new RefreshSettingsViewModel(store);
            await settings.LoadAsync();
            Assert.Equal(60, settings.IntervalSeconds);
            settings.SelectedSeconds = 30; await settings.SaveAsync();
            var reloaded = new RefreshSettingsViewModel(store); await reloaded.LoadAsync();
            Assert.Equal(30, reloaded.IntervalSeconds);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.SaveAsync(31));
        }
        finally { File.Delete(path); }
    }

    /// <summary>A loop starts only after its delay and duplicate starts do not create a second delay.</summary>
    [Fact]
    public void LoopIsDelayedAndIdempotent()
    {
        var delays = new ControlledRefreshDelay(); var updates = 0; var cancellations = 0;
        using var loop = new RefreshLoop(() => { updates++; return Task.CompletedTask; }, () => cancellations++, delays.WaitAsync);
        loop.Start(30); loop.Start(30);
        Assert.Single(delays.Pending); Assert.Equal(0, updates); Assert.Equal(0, cancellations);
        Assert.Equal(TimeSpan.FromSeconds(30), delays.Intervals[0]);
        delays.Pending[0].SetResult();
        Assert.Equal(1, updates);
        Assert.Equal(2, delays.Pending.Count);
    }

    /// <summary>Stopping invalidates a late refresh and zero disables the loop.</summary>
    [Fact]
    public void StopRejectsLateWork()
    {
        var updates = 0; var delays = new ControlledRefreshDelay();
        using var loop = new RefreshLoop(() => { updates++; return Task.CompletedTask; }, () => { }, delays.WaitAsync);
        loop.Start(30); loop.Stop(); delays.Pending[0].SetResult();
        Assert.Equal(0, updates);
        Assert.True(delays.Tokens[0].IsCancellationRequested);
        loop.Start(0); Assert.Single(delays.Pending);
    }
}

internal sealed class ControlledRefreshDelay
{
    internal List<TaskCompletionSource> Pending { get; } = [];
    internal List<TimeSpan> Intervals { get; } = [];
    internal List<CancellationToken> Tokens { get; } = [];
    internal Task WaitAsync(TimeSpan interval, CancellationToken token)
    {
        var source = new TaskCompletionSource(); Pending.Add(source); Intervals.Add(interval); Tokens.Add(token); return source.Task;
    }
}
