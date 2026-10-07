using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Stop identity, refresh retention and asynchronous monitor boundaries.</summary>
public sealed class StopMonitorTests
{
    /// <summary>Only identifiable stops open a board and their complete identity is retained.</summary>
    [Fact]
    public async Task FiltersAddressesAndPassesCompleteStop()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var navigation = new DepartureTestNavigation();
        var model = new StopMonitorViewModel(search, departures, navigation, 200);
        var candidate = Candidate("Essen");
        var address = new Address { Name = "Straße", Coordinate = new(51, 7) };
        var invalid = new Address { Name = "Ohne ID", Stop = new() };
        model.Lookup.Text = "Essen";
        var lookup = model.Lookup.SearchAsync();
        search.Pending[0].SetResult(new() { Items = [address, invalid, candidate] });
        await lookup;
        Assert.Same(candidate, Assert.Single(model.Stops));
        await model.OpenAsync(address);
        Assert.Empty(departures.Pending);
        var opening = model.OpenAsync(candidate);
        Assert.Same(candidate.Stop, departures.Stops[0]);
        departures.Pending[0].SetResult(new());
        await opening;
        Assert.Equal(1, navigation.Opens);
        Assert.Same(candidate, model.Lookup.SelectedAddress);
    }

    /// <summary>Home nearby candidates use their verified identity without weakening lookup membership checks.</summary>
    [Fact]
    public async Task OpensVerifiedHomeNearbyAndRejectsIncompleteIdentity()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var navigation = new DepartureTestNavigation();
        var model = new StopMonitorViewModel(search, departures, navigation, 200);
        var nearby = Candidate("Home nearby");

        var opening = model.OpenNearbyFromHomeAsync(nearby);
        Assert.Same(nearby.Stop, Assert.Single(departures.Stops));
        departures.Pending[0].SetResult(new());
        await opening;

        await model.OpenNearbyFromHomeAsync(new Address
        {
            Name = "Unvollständig",
            Stop = new() { Name = "Unvollständig", Id = "missing-source" }
        });

        Assert.Equal(1, navigation.Opens);
        Assert.Single(departures.Stops);
        Assert.Same(nearby.Stop, model.SelectedStop);
    }

    /// <summary>Failed refresh retains data and metadata; an authoritative empty result replaces it.</summary>
    [Fact]
    public async Task RetainsDataOnErrorAndRecoversWithEmptyResult()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 200);
        var candidate = await Lookup(model, search, "Essen");
        var opening = model.OpenAsync(candidate);
        departures.Pending[0].SetResult(new() { Items = [FutureEvent()], Source = "gute Quelle" });
        await opening;
        var previous = model.Result;
        var refresh = model.RefreshCommand.ExecuteAsync();
        Assert.True(model.IsBusy);
        Assert.False(model.RefreshCommand.CanExecute(null));
        await model.RefreshCommand.ExecuteAsync();
        Assert.Equal(2, departures.Pending.Count);
        departures.Pending[1].SetResult(new() { ErrorCode = "timeout", Source = "Fehlerquelle" });
        await refresh;
        Assert.Same(previous, model.Result);
        Assert.Single(model.Items);
        Assert.Contains("Letzte bekannte Daten", model.Status);
        Assert.Contains("gute Quelle", model.Metadata);
        refresh = model.RefreshCommand.ExecuteAsync();
        departures.Pending[2].SetResult(new() { Source = "neue Quelle" });
        await refresh;
        Assert.Empty(model.Items);
        Assert.Contains("Keine nächsten", model.Status);
        Assert.Contains("neue Quelle", model.Metadata);
    }

    /// <summary>A departed monitor cannot overwrite the newly selected stop.</summary>
    [Fact]
    public async Task IgnoresLateOldStopResponse()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 200);
        var first = await Lookup(model, search, "Alt");
        var old = model.OpenAsync(first);
        model.CancelPending();
        Assert.True(departures.Tokens[0].IsCancellationRequested);
        var next = await Lookup(model, search, "Neu");
        var current = model.OpenAsync(next);
        departures.Pending[1].SetResult(new() { Items = [FutureEvent()], Source = "neu" });
        await current;
        departures.Pending[0].SetResult(new() { Source = "alt" });
        await old;
        Assert.Same(next.Stop, model.SelectedStop);
        Assert.Equal("neu", model.Result?.Source);
        Assert.Single(model.Items);
    }

    /// <summary>Cancellation releases the command and late errors cannot change the new result.</summary>
    [Fact]
    public async Task CancelledRefreshAllowsNewRequestAndIgnoresLateError()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 200);
        var candidate = await Lookup(model, search, "Essen");
        var opening = model.OpenAsync(candidate);
        departures.Pending[0].SetResult(new());
        await opening;
        var old = model.RefreshCommand.ExecuteAsync();
        model.CancelPending();
        Assert.True(model.RefreshCommand.CanExecute(null));
        var current = model.RefreshCommand.ExecuteAsync();
        departures.Pending[2].SetResult(new() { Items = [FutureEvent()], Source = "neu" });
        await current;
        departures.Pending[1].SetException(new InvalidOperationException("late failure"));
        await old;
        Assert.Equal("neu", model.Result?.Source);
        Assert.DoesNotContain("fehlgeschlagen", model.Status);
    }

    /// <summary>Past actual departures disappear while delayed future and unknown-time events remain.</summary>
    [Fact]
    public async Task ShowsNextDeparturesInEffectiveTimeOrder()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 200);
        var candidate = await Lookup(model, search, "Essen");
        var opening = model.OpenAsync(candidate);
        var now = DateTimeOffset.Now;
        var late = new StopEvent { PlannedTime = now.AddMinutes(-1), Realtime = new() { Delay = TimeSpan.FromMinutes(8) } };
        var early = new StopEvent { PlannedTime = now.AddMinutes(3) };
        var unknown = new StopEvent();
        departures.Pending[0].SetResult(new() { Items = [late, unknown, new() { PlannedTime = now.AddMinutes(-5) }, early] });
        await opening;
        Assert.Equal(new[] { early, late, unknown }, model.Items);
    }

    /// <summary>Search addresses and initial provider failures remain clearly recoverable.</summary>
    [Fact]
    public async Task ExplainsNoStopsAndInitialFailure()
    {
        var search = new ControlledSearchService();
        var departures = new ControlledDepartureService();
        var model = new StopMonitorViewModel(search, departures, new DepartureTestNavigation(), 200);
        model.Lookup.Text = "Adresse";
        var lookup = model.Lookup.SearchAsync();
        search.Pending[0].SetResult(new() { Items = [new Address { Name = "Straße" }] });
        await lookup;
        Assert.Contains("Keine Haltestellen", model.SearchStatus);
        await model.RefreshAsync();
        Assert.Empty(departures.Pending);
        var candidate = await Lookup(model, search, "Essen");
        var opening = model.OpenAsync(candidate);
        departures.Pending[0].SetException(new InvalidOperationException("offline"));
        await opening;
        Assert.Contains("nicht geladen", model.Status);
        Assert.Null(model.Result);
        Assert.True(model.RefreshCommand.CanExecute(null));
    }

    private static Address Candidate(string name) => new() { Name = name, Stop = new() { Name = name, Id = name, Source = "test", Dhid = "de:" + name } };
    private static StopEvent FutureEvent() => new() { PlannedTime = DateTimeOffset.Now.AddMinutes(5) };
    private static async Task<Address> Lookup(StopMonitorViewModel model, ControlledSearchService search, string name)
    {
        model.Lookup.Text = name;
        var task = model.Lookup.SearchAsync();
        var candidate = Candidate(name);
        search.Pending[^1].SetResult(new() { Items = [candidate] });
        await task;
        return candidate;
    }
}

/// <summary>Departure text distinguishes uncertainty, realtime and disruptions.</summary>
public sealed class DeparturePresentationTests
{
    /// <summary>Cancellation and platform changes are explicit alongside delay.</summary>
    [Fact]
    public void DescribesCancellationDelayAndPlatformChange()
    {
        var planned = DateTimeOffset.Parse("2026-09-16T21:59:00Z");
        var text = DeparturePresentation.Describe(new()
        {
            Identity = new() { Line = "RE 1", Direction = "Essen", Operator = "Bahn" },
            PlannedTime = planned,
            Realtime = new() { ActualTime = planned.AddMinutes(4), Cancelled = true, PlannedPlatform = "1", Platform = "2" }
        });
        Assert.Contains("FÄLLT AUS", text);
        Assert.Contains("+4 Min.", text);
        Assert.Contains("Gleis-/Steigwechsel", text);
        Assert.Contains("17.09.2026 00:03", text);
        Assert.Contains("RE 1 → Essen", text);
    }

    /// <summary>No realtime is different from reported punctuality or early running.</summary>
    [Fact]
    public void DistinguishesUnknownPunctualAndEarly()
    {
        var planned = DateTimeOffset.Now;
        var item = new StopEvent { PlannedTime = planned };
        var unknown = DeparturePresentation.Describe(item);
        Assert.Contains("keine Echtzeitdaten", unknown);
        Assert.Contains("Verspätung unbekannt", unknown);
        Assert.DoesNotContain("Pünktlich", unknown);
        Assert.Contains("Pünktlich gemeldet", DeparturePresentation.Describe(item with { Realtime = new() { ActualTime = planned, Cancelled = false } }));
        Assert.Contains("-2 Min.", DeparturePresentation.Describe(item with { Realtime = new() { Delay = TimeSpan.FromMinutes(-2) } }));
    }
}

internal sealed class ControlledDepartureService : IDepartureService
{
    internal List<TaskCompletionSource<ProviderResult<StopEvent>>> Pending { get; } = [];
    internal List<Stop> Stops { get; } = [];
    internal List<CancellationToken> Tokens { get; } = [];
    public Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        Stops.Add(stop);
        Tokens.Add(cancellationToken);
        var source = new TaskCompletionSource<ProviderResult<StopEvent>>();
        Pending.Add(source);
        return source.Task;
    }
}

internal sealed class DepartureTestNavigation : IDepartureNavigation
{
    internal int Opens { get; private set; }
    public Task ShowMonitorAsync()
    {
        Opens++;
        return Task.CompletedTask;
    }
}
