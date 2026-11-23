using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>A running resume guards the same area while other areas resume independently.</summary>
public sealed class RefreshLifecycleTests_Overlap
{
    /// <summary>A second trigger while a resume runs issues no duplicate provider request.</summary>
    [Fact]
    public async Task Resume_Twice_SameArea_NoDuplicate()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedJourneysAsync();
        await fixture.Settings.LoadAsync();
        fixture.Lifecycle.SetActive(true);
        var first = fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Journeys, this);
        Assert.Equal(2, fixture.Routing.Pending.Count);
        fixture.Routing.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
        await first;
    }

    /// <summary>Two visible areas hold independent resume tokens and finish independently.</summary>
    [Fact]
    public async Task Resume_DifferentAreas_RunConcurrently()
    {
        var fixture = new LifecycleFixture(1);
        await fixture.Home.LoadAsync();
        await fixture.SeedMonitorAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Favorites, this);
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        fixture.Lifecycle.SetActive(true);
        Assert.Single(fixture.Services[0].Pending);
        Assert.Equal(2, fixture.MonitorService.Pending.Count);
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Favorites));
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        var favoritesCompleted = new TaskCompletionSource();
        fixture.Lifecycle.ResumeCompleted += (_, args) =>
        {
            if (args.Area == RefreshArea.Favorites) favoritesCompleted.TrySetResult();
        };
        fixture.Complete(0);
        await favoritesCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Favorites));
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        fixture.MonitorService.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
    }

    /// <summary>IsResuming is true while a resume runs and false once ResumeCompleted fired.</summary>
    [Fact]
    public async Task IsResuming_ReflectsResumeState()
    {
        var fixture = new LifecycleFixture(0);
        await fixture.SeedMonitorAsync();
        await fixture.Settings.LoadAsync();
        await fixture.Lifecycle.NotifyVisibleAsync(RefreshArea.Monitor, this);
        var completed = new TaskCompletionSource<RefreshArea>();
        fixture.Lifecycle.ResumeCompleted += (_, args) => completed.TrySetResult(args.Area);
        fixture.Lifecycle.SetActive(true);
        Assert.True(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Favorites));
        fixture.MonitorService.Pending[1].SetResult(new() { RetrievedAt = fixture.Clock.Now });
        Assert.Equal(RefreshArea.Monitor, await completed.Task.WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.False(fixture.Lifecycle.IsResuming(RefreshArea.Monitor));
    }
}
