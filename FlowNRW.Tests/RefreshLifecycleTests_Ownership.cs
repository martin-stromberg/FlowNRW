using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>Shared area registrations survive a hidden report from a page that no longer owns them.</summary>
public sealed class RefreshLifecycleTests_Ownership
{
    /// <summary>A hidden report from a non-owning instance keeps the registration and its resume eligibility.</summary>
    [Fact]
    public async Task NotifyHidden_NonOwner_KeepsRegistration()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        var owner = new object();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, owner);
        fixture.Lifecycle.NotifyHidden(RefreshArea.Journeys, new object());
        fixture.Lifecycle.SetActive(true);
        Assert.Equal(2, fixture.Routing.Pending.Count);
        fixture.Routing.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
    }

    /// <summary>A hidden report from a non-owning instance cancels neither the token nor the refreshable's work.</summary>
    [Fact]
    public async Task NotifyHidden_NonOwner_KeepsRunningResume()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        var owner = new object();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, owner);
        fixture.Lifecycle.SetActive(true);
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Journeys));
        fixture.Lifecycle.NotifyHidden(RefreshArea.Journeys, new object());
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Journeys));
        Assert.False(fixture.Routing.Tokens[1].IsCancellationRequested);
        Assert.True(fixture.Journeys.IsBusy);
        fixture.Lifecycle.NotifyHidden(RefreshArea.Journeys, owner);
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Journeys));
        Assert.True(fixture.Routing.Tokens[1].IsCancellationRequested);
        Assert.False(fixture.Journeys.IsBusy);
    }

    /// <summary>A later registration takes over; the former owner's hidden report no longer unregisters the area.</summary>
    [Fact]
    public async Task NotifyHidden_FormerOwner_AfterTakeover_KeepsArea()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        var results = new object();
        var detail = new object();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, results);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, detail);
        fixture.Lifecycle.NotifyHidden(RefreshArea.Journeys, results);
        fixture.Lifecycle.SetActive(true);
        Assert.Equal(2, fixture.Routing.Pending.Count);
        fixture.Routing.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
        fixture.Lifecycle.NotifyHidden(RefreshArea.Journeys, detail);
        fixture.Lifecycle.SetActive(false);
        fixture.Lifecycle.SetActive(true);
        Assert.Equal(2, fixture.Routing.Pending.Count);
    }
}
