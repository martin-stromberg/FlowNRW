using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>Hiding or deactivation aborts running resumes and discards their late responses.</summary>
public sealed class RefreshLifecycleTests_Cancellation
{
    /// <summary>Hiding cancels the resume token, clears the refreshable's pending work and drops late answers.</summary>
    [Fact]
    public async Task NotifyHidden_RunningResume_CancelsToken()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedMonitorAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        fixture.Lifecycle.SetActive(true);
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        fixture.Lifecycle.NotifyHidden(RefreshArea.Monitor, this);
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        Assert.True(fixture.MonitorService.Tokens[1].IsCancellationRequested);
        Assert.False(fixture.Monitor.IsBusy);
        fixture.MonitorService.Pending[1].SetResult(new() { Source = "late", RetrievedAt = fixture.Clock.Now });
        Assert.Equal("seed", fixture.Monitor.Result?.Source);
    }

    /// <summary>Deactivation cancels the running resume of every visible area.</summary>
    [Fact]
    public async Task SetActiveFalse_RunningResume_CancelsAllVisible()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.SeedMonitorAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        fixture.Lifecycle.SetActive(true);
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Favorites));
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        fixture.Lifecycle.SetActive(false);
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Favorites));
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        Assert.True(fixture.Services[0].Tokens[0].IsCancellationRequested);
        Assert.True(fixture.MonitorService.Tokens[1].IsCancellationRequested);
    }
}
