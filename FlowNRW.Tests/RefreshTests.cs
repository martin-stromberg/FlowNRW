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
    public async Task LoopIsDelayedAndIdempotent()
    {
        var delays = 0; var updates = 0;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var loop = new RefreshLoop(() => { updates++; return Task.CompletedTask; }, () => { }, async (_, token) =>
        {
            delays++;
            if (delays == 1) await gate.Task.WaitAsync(token);
            else await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });
        loop.Start(30); loop.Start(30);
        Assert.Equal(1, delays); Assert.Equal(0, updates);
        gate.SetResult();
        for (var i = 0; i < 20 && updates == 0; i++) await Task.Delay(10);
        Assert.Equal(1, updates);
        loop.Dispose();
    }

    /// <summary>Stopping invalidates a late refresh and zero disables the loop.</summary>
    [Fact]
    public async Task StopRejectsLateWork()
    {
        var updates = 0; var delay = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var loop = new RefreshLoop(() => { updates++; return Task.CompletedTask; }, () => { }, (_, _) => delay.Task);
        loop.Start(30); loop.Stop(); delay.SetResult(); await Task.Delay(20);
        Assert.Equal(0, updates);
        loop.Start(0); loop.Dispose();
    }
}
