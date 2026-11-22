using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Visibility reports and trigger notifications are mapped centrally to their refresh step.</summary>
public sealed class RefreshLifecycleTests_Occasions
{
    /// <summary>The startup occasion refreshes every non-busy card with one provider request each.</summary>
    [Fact]
    public async Task NotifyVisible_Favorites_Active_StartsFullRefresh()
    {
        var fixture = new LifecycleFixture(2);
        await fixture.Home.LoadAsync();
        await fixture.Settings.LoadAsync();
        fixture.Lifecycle.SetActive(true);
        var startup = fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        Assert.Single(fixture.Services[0].Pending);
        Assert.Single(fixture.Services[1].Pending);
        fixture.Complete(0);
        fixture.Complete(1);
        await startup;
    }

    /// <summary>The monitor reports visibility without an appearing refresh; its retained open stays.</summary>
    [Fact]
    public async Task NotifyVisible_Monitor_Active_IssuesNoRequest()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedMonitorAsync();
        await fixture.Settings.LoadAsync();
        fixture.Lifecycle.SetActive(true);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        Assert.Single(fixture.MonitorService.Pending);
    }

    /// <summary>Journey appearing shares the gated resume entry; a fresh session is not re-routed.</summary>
    [Fact]
    public async Task NotifyVisible_Journeys_Gated_StaleSessionRefreshes()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        fixture.Lifecycle.SetActive(true);
        var appearing = fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        Assert.Equal(2, fixture.Routing.Pending.Count);
        fixture.Routing.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
        await appearing;
        fixture.Lifecycle.NotifyHidden(RefreshArea.Journeys, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        Assert.Equal(2, fixture.Routing.Pending.Count);
    }

    /// <summary>A Cards notification refreshes only missing or stale cards, not fresh ones.</summary>
    [Fact]
    public async Task CardsChanged_VisibleAndActive_RefreshesOnlyMissing()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.Settings.LoadAsync();
        fixture.Lifecycle.SetActive(true);
        var startup = fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        fixture.Complete(0);
        await startup;
        await fixture.Home.ToggleAsync(new Stop { Id = "new", Source = "fixture" });
        Assert.Single(fixture.Services[1].Pending);
        Assert.Single(fixture.Services[0].Pending);
        fixture.Services[1].Pending[0].SetResult(new() { RetrievedAt = fixture.Clock.Now });
    }

    /// <summary>The cards-added occasion does not start while a resume of the same area runs.</summary>
    [Fact]
    public async Task CardsChanged_ResumeRunning_DoesNotStart()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        fixture.Lifecycle.SetActive(true);
        Assert.Single(fixture.Services[0].Pending);
        await fixture.Home.ToggleAsync(new Stop { Id = "new", Source = "fixture" });
        Assert.Empty(fixture.Services[1].Pending);
        fixture.Complete(0);
    }

    /// <summary>A failing resume raises RefreshFailed with area and occasion; a hidden area raises nothing.</summary>
    [Fact]
    public async Task ResumeFailed_Event_CarriesAreaAndOccasion()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedMonitorAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        var events = new List<RefreshFailedEventArgs>();
        fixture.Lifecycle.RefreshFailed += (_, args) => events.Add(args);
        var boom = true;
        fixture.Monitor.PropertyChanged += (_, _) =>
        {
            if (boom) { boom = false; throw new InvalidOperationException("boom"); }
        };
        fixture.Lifecycle.SetActive(true);
        var failure = Assert.Single(events);
        Assert.Equal(RefreshArea.Monitor, failure.Area);
        Assert.Equal(RefreshOccasion.ForegroundReturn, failure.Occasion);
        fixture.Lifecycle.NotifyHidden(RefreshArea.Monitor, this);
        fixture.Lifecycle.SetActive(false);
        fixture.Lifecycle.SetActive(true);
        Assert.Single(events);
        Assert.Single(fixture.MonitorService.Pending);
    }
}
