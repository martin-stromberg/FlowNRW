using FlowNRW.Core.Favorites;
using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Retained monitor updates respect age and bounded external lifetimes.</summary>
public sealed class ResumeMonitorTests_Cancellation
{
    /// <summary>A fresh card causes no additional provider request.</summary>
    [Fact]
    public async Task RefreshIfStale_FreshCard_DoesNotFetch()
    {
        var clock = new TransitTestClock();
        var service = new ControlledDepartureService();
        var model = new FavoriteMonitorViewModel(new() { Id = "one", Source = "test" }, service);
        var initial = model.RefreshAsync();
        service.Pending[0].SetResult(new() { RetrievedAt = clock.Now });
        await initial;
        await model.RefreshIfStaleAsync(new(new(), clock));
        Assert.Single(service.Pending);
    }

    /// <summary>Expiry ends an uncooperative request promptly and its later answer cannot overwrite data.</summary>
    [Fact]
    public async Task RefreshIfStale_ExpiredExecution_RejectsLateAnswer()
    {
        var clock = new TransitTestClock();
        var service = new ControlledDepartureService();
        var model = new FavoriteMonitorViewModel(new() { Id = "one", Source = "test" }, service);
        var initial = model.RefreshAsync();
        service.Pending[0].SetResult(new() { Source = "retained", RetrievedAt = clock.Now.AddMinutes(-1) });
        await initial;
        using var lifetime = new CancellationTokenSource();
        var resume = model.RefreshIfStaleAsync(new(new(), clock), lifetime.Token);
        lifetime.Cancel();
        await resume.WaitAsync(TimeSpan.FromSeconds(2));
        service.Pending[1].SetResult(new() { Source = "late", RetrievedAt = clock.Now });
        Assert.False(model.IsBusy);
        Assert.Equal("retained", model.Result?.Source);
        Assert.Contains("abgebrochen", model.Status);
        Assert.True(service.Tokens[1].IsCancellationRequested);
    }

    /// <summary>A manually started request prevents a second resume request.</summary>
    [Fact]
    public async Task RefreshIfStale_ManualBusy_DoesNotDuplicate()
    {
        var service = new ControlledDepartureService();
        var model = new FavoriteMonitorViewModel(new() { Id = "one", Source = "test" }, service);
        var manual = model.RefreshAsync();
        await model.RefreshIfStaleAsync(new(new()));
        Assert.Single(service.Pending);
        service.Pending[0].SetResult(new());
        await manual;
    }
}
