using FlowNRW.Core.Favorites;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Manual and automatic updates share busy guards, retained data and revision boundaries.</summary>
public sealed class RefreshMonitorTests_Concurrency
{
    /// <summary>A card permits one provider request regardless of which refresh path starts first.</summary>
    [Fact]
    public async Task FavoriteManualAndAutomaticPathsShareBusyGuard()
    {
        var service = new ControlledDepartureService(); var model = new FavoriteMonitorViewModel(new() { Id = "one", Source = "test" }, service);
        var automatic = model.RefreshAutomaticallyAsync();
        await model.RefreshAsync(); await model.RefreshCommand.ExecuteAsync(); await model.RefreshAutomaticallyAsync();
        Assert.Single(service.Pending);
        service.Pending[0].SetResult(Data("auto")); await automatic;
        Assert.Contains("automatisch aktualisiert", model.Status);
        var manual = model.RefreshCommand.ExecuteAsync(); await model.RefreshAutomaticallyAsync();
        Assert.Equal(2, service.Pending.Count);
        service.Pending[1].SetResult(Data("manual")); await manual;
        Assert.Contains("manuell aktualisiert", model.Status);
    }

    /// <summary>A board permits no duplicate automatic request during its initial manual load.</summary>
    [Fact]
    public async Task StopMonitorManualAndAutomaticPathsShareBusyGuard()
    {
        var search = new ControlledSearchService(); var service = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, service, new DepartureTestNavigation(), 100);
        model.Lookup.Text = "one"; var lookup = model.Lookup.SearchAsync();
        var candidate = new Address { Name = "one", Stop = new() { Id = "one", Source = "test" } };
        search.Pending[0].SetResult(new() { Items = [candidate] }); await lookup;
        var opening = model.OpenAsync(candidate); await model.RefreshAutomaticallyAsync(); Assert.Single(service.Pending);
        service.Pending[0].SetResult(Data("initial")); await opening;
        var automatic = model.RefreshAutomaticallyAsync(); await model.RefreshAsync(); await model.RefreshCommand.ExecuteAsync();
        Assert.Equal(2, service.Pending.Count); service.Pending[1].SetResult(Data("auto")); await automatic;
        Assert.Contains("automatisch aktualisiert", model.Status);
        var previous = model.Result;
        automatic = model.RefreshAutomaticallyAsync(); service.Pending[2].SetResult(new() { Source = "error", ErrorCode = "timeout" }); await automatic;
        Assert.Same(previous, model.Result); Assert.Contains("Letzte bekannte Daten", model.Status); Assert.Contains("auto", model.Metadata);
    }

    /// <summary>Stopping an in-flight automatic card update cancels its token and rejects its late answer.</summary>
    [Fact]
    public async Task LoopStopCancelsInflightAndRejectsLateCardResult()
    {
        var service = new ControlledDepartureService(); var model = new FavoriteMonitorViewModel(new() { Id = "one", Source = "test" }, service);
        var initial = model.RefreshAsync(); service.Pending[0].SetResult(Data("initial")); await initial;
        var delays = new ControlledRefreshDelay();
        using var loop = new RefreshLoop(model.RefreshAutomaticallyAsync, model.CancelPending, delays.WaitAsync);
        loop.Start(30); delays.Pending[0].SetResult(); Assert.Equal(2, service.Pending.Count);
        loop.Stop(); Assert.True(service.Tokens[1].IsCancellationRequested); Assert.False(model.IsBusy);
        var manual = model.RefreshAsync(); service.Pending[2].SetResult(Data("current")); await manual;
        service.Pending[1].SetResult(Data("old"));
        Assert.Equal("current", model.Result?.Source); Assert.Single(delays.Pending);
    }

    /// <summary>Automatic failure keeps last data, and the scheduler waits before recovery.</summary>
    [Fact]
    public async Task AutomaticFailureRetainsDataAndRecoveryWaits()
    {
        var service = new ControlledDepartureService(); var model = new FavoriteMonitorViewModel(new() { Id = "one", Source = "test" }, service);
        var initial = model.RefreshAsync(); service.Pending[0].SetResult(Data("good")); await initial;
        var previous = model.Result; var delays = new ControlledRefreshDelay();
        using var loop = new RefreshLoop(model.RefreshAutomaticallyAsync, model.CancelPending, delays.WaitAsync);
        loop.Start(60); delays.Pending[0].SetResult();
        service.Pending[1].SetException(new IOException("private"));
        Assert.Same(previous, model.Result); Assert.Contains("Letzte bekannte Daten", model.Status); Assert.DoesNotContain("private", model.Status);
        Assert.Equal(2, service.Pending.Count); Assert.Equal(2, delays.Pending.Count);
        delays.Pending[1].SetResult(); service.Pending[2].SetResult(Data("recovered"));
        Assert.Equal("recovered", model.Result?.Source); Assert.Contains("automatisch aktualisiert", model.Status);
    }

    private static ProviderResult<StopEvent> Data(string source) => new() { Source = source, Items = [new() { PlannedTime = DateTimeOffset.Now.AddMinutes(5) }] };
}
