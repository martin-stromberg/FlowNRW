using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>Foreground activation runs the gated resume only for visible areas with automatic refresh enabled.</summary>
public sealed class RefreshLifecycleTests_Foreground
{
    /// <summary>Activation produces exactly one stale resume per visible area.</summary>
    [Fact]
    public async Task SetActive_VisibleArea_ResumesStale()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.SeedMonitorAsync();
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        fixture.Lifecycle.SetActive(true);
        Assert.Single(fixture.Services[0].Pending);
        Assert.Equal(2, fixture.MonitorService.Pending.Count);
        Assert.Equal(2, fixture.Routing.Pending.Count);
        fixture.Complete(0);
        fixture.MonitorService.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
        fixture.Routing.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
    }

    /// <summary>A disabled interval suppresses every foreground resume.</summary>
    [Fact]
    public async Task SetActive_IntervalZero_DoesNotResume()
    {
        var fixture = new LifecycleFixture(1, 0);
        await fixture.Home.LoadAsync();
        await fixture.SeedMonitorAsync();
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        fixture.Lifecycle.SetActive(true);
        Assert.Empty(fixture.Services[0].Pending);
        Assert.Single(fixture.MonitorService.Pending);
        Assert.Single(fixture.Routing.Pending);
    }

    /// <summary>Unloaded settings suppress every foreground resume.</summary>
    [Fact]
    public async Task SetActive_SettingsNotLoaded_DoesNotResume()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.SeedMonitorAsync();
        await fixture.SeedJourneysAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        fixture.Lifecycle.SetActive(true);
        Assert.Empty(fixture.Services[0].Pending);
        Assert.Single(fixture.MonitorService.Pending);
        Assert.Single(fixture.Routing.Pending);
    }

    /// <summary>Activation without a visible area issues no provider request.</summary>
    [Fact]
    public async Task SetActive_AreaNotVisible_DoesNotResume()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.SeedMonitorAsync();
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        fixture.Lifecycle.SetActive(true);
        Assert.Empty(fixture.Services[0].Pending);
        Assert.Single(fixture.MonitorService.Pending);
        Assert.Single(fixture.Routing.Pending);
    }
}
