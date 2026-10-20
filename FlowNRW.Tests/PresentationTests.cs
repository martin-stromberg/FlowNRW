using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Endpoint identity and asynchronous state.</summary>
public sealed class EndpointViewModelTests
{
    /// <summary>Selection retains the complete identity until input changes.</summary>
    [Fact]
    public async Task PreservesSelectedIdentityAndTextChangeInvalidatesSelection()
    {
        var service = new ControlledSearchService(); var model = new EndpointViewModel(service, 100) { Text = "Essen" };
        var task = model.SearchAsync(); var address = new Address { Name = "Essen", Stop = new Stop { Id = "1", Source = "efa", Dhid = "de:1" } };
        service.Pending[0].SetResult(new() { Items = [address] }); await task; model.SelectAddress(address);
        Assert.Same(address, model.SelectedAddress); Assert.Equal("Essen", model.Text); Assert.Empty(model.Matches);
        model.Text = "Berlin"; Assert.Null(model.SelectedAddress); Assert.Empty(model.Matches);
    }
    /// <summary>Late responses cannot overwrite newer state and commands unlock immediately.</summary>
    [Fact]
    public async Task IgnoresLateResponse()
    {
        var service = new ControlledSearchService(); var model = new EndpointViewModel(service, 100) { Text = "alt" };
        var old = model.SearchCommand.ExecuteAsync(); model.Text = "neu";
        Assert.True(model.SearchCommand.CanExecute(null)); var current = model.SearchCommand.ExecuteAsync();
        var address = new Address { Name = "neu" };
        service.Pending[1].SetResult(new() { Items = [address], Source = "new" }); await current;
        service.Pending[0].SetResult(new() { Items = [new Address { Name = "alt" }], Source = "old" }); await old;
        Assert.Same(address, Assert.Single(model.Matches)); Assert.Equal("new", model.Result?.Source);
    }
    /// <summary>Separate endpoint service scopes progress independently.</summary>
    [Fact]
    public async Task SearchesEndpointsIndependently()
    {
        var a = new ControlledSearchService(); var b = new ControlledSearchService();
        var origin = new EndpointViewModel(a, 100) { Text = "a" }; var destination = new EndpointViewModel(b, 100) { Text = "b" };
        var first = origin.SearchAsync(); var second = destination.SearchAsync(); origin.Text = "c";
        Assert.True(a.Tokens[0].IsCancellationRequested); Assert.False(b.Tokens[0].IsCancellationRequested);
        a.Pending[0].SetResult(new()); b.Pending[0].SetResult(new() { Source = "b", IsStale = true, IsFallback = true, Warnings = ["timeout"] }); await Task.WhenAll(first, second);
        Assert.Contains("Keine Treffer", destination.Status); Assert.Contains("Veralteter Cache", destination.Metadata); Assert.Contains("Fallback", destination.Metadata);
    }
    /// <summary>The selected place name is carried by the text field without the technical identity.</summary>
    [Fact]
    public async Task Selection_NameGoesIntoTextField()
    {
        var service = new ControlledSearchService(); var model = new EndpointViewModel(service, 100) { Text = "Essen" };
        var task = model.SearchAsync();
        var address = new Address { Name = "Essen, Essen Hbf", Coordinate = new GeoCoordinate(51.45, 7.0116), Stop = new Stop { Id = "1", Source = "efa", Dhid = "de:05113:1" } };
        service.Pending[0].SetResult(new() { Items = [address] }); await task; model.SelectAddress(address);
        Assert.Equal("Essen, Essen Hbf", model.Text);
        Assert.DoesNotContain("efa", model.Text);
        Assert.DoesNotContain("de:05113", model.Text);
    }
    /// <summary>A map-picked coordinate resolves to the nearest provider address.</summary>
    [Fact]
    public async Task MapPick_ResolvesToNearbyAddress()
    {
        var service = new ControlledSearchService(); var model = new EndpointViewModel(service, 100);
        var stop = new Stop { Id = "1", Source = "efa", Name = "Essen Hbf", Coordinate = new GeoCoordinate(51.45, 7.01) };
        service.NearbyHandler = (_, _) => Task.FromResult(new ProviderResult<NearbyStopResult> { Items = [new() { Stop = stop, DistanceMeters = 42 }] });
        var coordinate = new GeoCoordinate(51.455, 7.011);
        await model.SelectCoordinateAsync(coordinate);
        Assert.Same(stop, model.SelectedAddress?.Stop);
        Assert.Equal("Essen Hbf", model.Text);
        Assert.Contains("übernommen", model.Status);
        Assert.Single(service.NearbyCentres);
        Assert.Same(coordinate, service.NearbyCentres[0]);
    }
    /// <summary>An unresolvable map pick still yields a usable coordinate endpoint.</summary>
    [Fact]
    public async Task MapPick_FallsBackToCoordinateWithoutNearby()
    {
        var service = new ControlledSearchService(); var model = new EndpointViewModel(service, 100);
        var coordinate = new GeoCoordinate(51.455, 7.011);
        await model.SelectCoordinateAsync(coordinate);
        Assert.Null(model.SelectedAddress?.Stop);
        Assert.Same(coordinate, model.SelectedAddress?.Coordinate);
        Assert.Equal("Koordinate", model.Text);
        Assert.False(model.IsBusy);
    }
    /// <summary>Validation, errors and cancellation remain recoverable.</summary>
    [Fact]
    public async Task RepresentsEmptyErrorAndMetadata()
    {
        var service = new ControlledSearchService(); var model = new EndpointViewModel(service, 4);
        await model.SearchAsync(); Assert.Empty(service.Pending); model.Text = "a";
        var task = model.SearchAsync(); Assert.True(model.IsBusy); service.Pending[0].SetResult(new() { ErrorCode = "timeout" }); await task;
        Assert.Contains("fehlgeschlagen", model.Status); Assert.False(model.IsBusy);
        task = model.SearchAsync(); model.CancelPending(); service.Pending[1].SetException(new OperationCanceledException()); await task;
        Assert.Contains("abgebrochen", model.Status);
        var coordinate = new GeoCoordinate(51.4, 7.0); await model.SelectCoordinateAsync(coordinate);
        Assert.Same(coordinate, model.SelectedAddress?.Coordinate);
    }
}

/// <summary>Routing revisions and retained navigation state.</summary>
public sealed class JourneySearchViewModelTests
{
    /// <summary>Unselected input never routes.</summary>
    [Fact]
    public async Task RequiresTwoSelections()
    {
        var service = new ControlledRoutingService(); var model = Create(service, new RecordingNavigation());
        await model.SearchAsync(); Assert.Empty(service.Pending); Assert.False(model.CanSearch);
    }
    /// <summary>Input changes supersede pending routes, including command availability.</summary>
    [Fact]
    public async Task EndpointChangeInvalidatesRouteAndIgnoresOldRoute()
    {
        var service = new ControlledRoutingService(); var nav = new RecordingNavigation(); var model = Create(service, nav); Select(model);
        var old = model.SearchCommand.ExecuteAsync(); model.Origin.SetSelectedAddress(new Address { Name = "changed" });
        Assert.True(model.SearchCommand.CanExecute(null)); var current = model.SearchCommand.ExecuteAsync();
        var journey = new Journey { Id = "new" }; service.Pending[1].SetResult(new() { Items = [journey], Source = "new" }); await current;
        service.Pending[0].SetResult(new() { Items = [new Journey { Id = "old" }], Source = "old" }); await old;
        Assert.Same(journey, Assert.Single(model.Journeys)); Assert.Equal("new", model.Result?.Source); Assert.Equal(1, nav.Results);
    }
    /// <summary>Alternative arrival time and mode reach the routing contract.</summary>
    [Fact]
    public async Task SendsSelectedArrivalTime()
    {
        var service = new ControlledRoutingService(); var model = Create(service, new RecordingNavigation()); Select(model);
        var selected = new DateTime(2026, 10, 3).AddHours(18).AddMinutes(30);
        model.UseCurrentTime = false; model.SelectedDate = selected.Date; model.SelectedTime = selected.TimeOfDay; model.ArriveBy = true;
        var task = model.SearchAsync(); service.Pending[0].SetResult(new() { Items = [] }); await task;
        Assert.True(service.ArriveBy[0]); Assert.Equal(model.PlannedTime, service.Departures[0]);
    }
    /// <summary>Completed contexts survive page departure and selected detail navigation.</summary>
    [Fact]
    public async Task PreservesContextThroughNavigation()
    {
        var service = new ControlledRoutingService(); var nav = new RecordingNavigation(); var model = Create(service, nav); Select(model);
        var origin = model.Origin.SelectedAddress; var task = model.SearchAsync(); var journey = new Journey { Id = "1" };
        service.Pending[0].SetResult(new() { Items = [journey] }); await task; model.CancelPending(); model.Origin.CancelPending();
        await new ResultsViewModel(model).OpenJourneyAsync(journey);
        Assert.Same(origin, model.Origin.SelectedAddress); Assert.Same(journey, model.SelectedJourney); Assert.Equal(1, nav.Details);
        Assert.Contains("Umstiege", new JourneyDetailViewModel(model).Details);
        model.Destination.Text = "8"; Assert.Null(model.SelectedJourney); Assert.Empty(model.Journeys);
    }
    private static JourneySearchViewModel Create(ControlledRoutingService service, RecordingNavigation nav) => new(new(new ControlledSearchService(), 100), new(new ControlledSearchService(), 100), service, nav);
    private static void Select(JourneySearchViewModel model)
    {
        foreach (var endpoint in new[] { model.Origin, model.Destination }) endpoint.SetSelectedAddress(new Address { Name = "endpoint", Coordinate = new GeoCoordinate(51, 7) });
    }
}

/// <summary>Display semantics around midnight and unknown realtime.</summary>
public sealed class JourneyPresentationTests
{
    /// <summary>Only non-current or incomplete provider data receives a compact list warning.</summary>
    [Fact]
    public void CompactsStaleFallbackAndIncompleteWarnings()
    {
        Assert.Equal("", JourneyPresentation.CompactWarning<StopEvent>(new()));
        var warning = JourneyPresentation.CompactWarning(new ProviderResult<StopEvent> { IsStale = true, IsFallback = true, Warnings = ["partial"] });
        Assert.Equal("Daten möglicherweise unvollständig", warning);
    }

    /// <summary>A literally repeated leading place name is removed once for display.</summary>
    [Fact]
    public void DisplayName_RemovesRepeatedPrefix()
    {
        Assert.Equal("Münster (Westf) Hbf", JourneyPresentation.DisplayName("Münster (Westf), Münster (Westf) Hbf"));
        Assert.Equal("Köln Hbf", JourneyPresentation.DisplayName("Köln, Köln Hbf"));
    }

    /// <summary>Names without a repeated comma prefix stay untouched.</summary>
    [Fact]
    public void DisplayName_KeepsNonRepeatedAndMissingComma()
    {
        Assert.Equal("Bochum, Hbf", JourneyPresentation.DisplayName("Bochum, Hbf"));
        Assert.Equal("Essen Hbf", JourneyPresentation.DisplayName("Essen Hbf"));
        Assert.Equal("", JourneyPresentation.DisplayName(""));
        Assert.Equal("", JourneyPresentation.DisplayName(null));
    }

    /// <summary>The repeated prefix only counts when it ends on a word boundary.</summary>
    [Fact]
    public void DisplayName_RequiresWordBoundary()
    {
        Assert.Equal("Bergheim, Bergheimer Str.", JourneyPresentation.DisplayName("Bergheim, Bergheimer Str."));
        Assert.Equal("Essen-Steele", JourneyPresentation.DisplayName("Essen, Essen-Steele"));
    }

    /// <summary>Display addresses keep coordinates but drop source, id and shared identifier.</summary>
    [Fact]
    public void DisplayAddress_OmitsIdentityKeepsCoordinates()
    {
        var address = new Address { Name = "Essen, Essen Hbf", Coordinate = new GeoCoordinate(51.45, 7.0116), Stop = new Stop { Id = "1", Source = "efa", Dhid = "de:05113:1" } };
        var text = JourneyPresentation.DisplayAddress(address);
        Assert.Contains("Essen Hbf", text);
        Assert.Contains("51.45", text);
        Assert.DoesNotContain("efa", text);
        Assert.DoesNotContain("de:05113", text);
        Assert.Equal("Essen Hbf", JourneyPresentation.DisplayAddress(new Address { Name = "Essen, Essen Hbf" }));
    }

    /// <summary>Local clock readings mark cross-day events instead of technical offsets.</summary>
    [Fact]
    public void DisplaysDayLabelsAndUnknownRealtime()
    {
        var time = DateTimeOffset.Parse("2026-09-15T22:05:00Z");
        var journey = new Journey { Legs = [new() { Walking = new() { DistanceMeters = 200 }, Departure = new() { PlannedTime = time, Realtime = new() { Cancelled = true } } }] };
        var detail = JourneyPresentation.Detail(journey);
        Assert.DoesNotContain("UTC", detail);
        Assert.Contains("00:05", detail); Assert.Contains("Fußweg", detail); Assert.Contains("keine Echtzeitdaten", detail); Assert.Contains("Ausfall", detail); Assert.Contains("unbekannt", detail);
    }

    /// <summary>Overnight arrivals are labeled as following day relative to the journey departure.</summary>
    [Fact]
    public void LabelsOvernightArrivalAsFollowingDay()
    {
        var departure = DateTimeOffset.Parse("2026-09-16T21:55:00Z");
        var arrival = departure.AddMinutes(45);
        var journey = new Journey { Legs = [new() { Departure = new() { PlannedTime = departure }, Arrival = new() { PlannedTime = arrival } }] };
        var detail = JourneyPresentation.Detail(journey);
        Assert.Contains("23:55", detail);
        Assert.Contains("Folgetag 00:40", detail);
    }

    /// <summary>Clock readings use the Europe/Berlin wall time and a single fallback wording.</summary>
    [Fact]
    public void FormatsBerlinClockAndUnifiedFallback()
    {
        Assert.Equal("unbekannt", JourneyPresentation.Clock(null));
        Assert.Equal("00:05", JourneyPresentation.Clock(DateTimeOffset.Parse("2026-09-15T22:05:00Z")));
    }

    /// <summary>Local instants convert into the Europe/Berlin zone for every offset.</summary>
    [Fact]
    public void ConvertsInstantsToBerlinLocalTime()
    {
        var winter = JourneyPresentation.LocalTime(DateTimeOffset.Parse("2026-01-15T12:00:00Z"));
        Assert.Equal(13, winter.Hour);
        Assert.Equal(TimeSpan.FromHours(1), winter.Offset);
        var summer = JourneyPresentation.LocalTime(DateTimeOffset.Parse("2026-07-15T12:00:00Z"));
        Assert.Equal(14, summer.Hour);
        Assert.Equal(TimeSpan.FromHours(2), summer.Offset);
    }

    /// <summary>The shared leg fallback resolves line name and mode from leg, event and identity.</summary>
    [Fact]
    public void ResolvesLegLineThroughSharedFallback()
    {
        var leg = new JourneyLeg { Line = new() { Name = "Leg", Mode = "bus" }, Departure = new() { Line = new() { Name = "Event", Mode = "tram" }, Identity = new() { Line = "Identität" } } };
        Assert.Equal("Leg", JourneyPresentation.LegLineName(leg));
        Assert.Equal("bus", JourneyPresentation.LegLineMode(leg));
        var unnamedLegLine = new JourneyLeg { Line = new(), Departure = new() { Line = new() { Name = "Event" } } };
        Assert.Equal("Event", JourneyPresentation.LegLineName(unnamedLegLine));
        Assert.Null(JourneyPresentation.LegLineMode(unnamedLegLine));
        var identityOnly = new JourneyLeg { Departure = new() { Identity = new() { Line = "Identität" } } };
        Assert.Equal("Identität", JourneyPresentation.LegLineName(identityOnly));
        Assert.Null(JourneyPresentation.LegLineMode(identityOnly));
        Assert.Equal("Linie unbekannt", JourneyPresentation.LegLineName(new()));
        Assert.Equal("Linie unbekannt", JourneyPresentation.LegLineName(new() { Walking = new() }));
    }
}

internal sealed class ControlledSearchService : IStopSearchService
{
    internal List<TaskCompletionSource<ProviderResult<Address>>> Pending { get; } = [];
    internal List<CancellationToken> Tokens { get; } = [];
    internal List<GeoCoordinate> NearbyCentres { get; } = [];
    internal Func<GeoCoordinate, CancellationToken, Task<ProviderResult<NearbyStopResult>>>? NearbyHandler { get; set; }
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default) { var source = new TaskCompletionSource<ProviderResult<Address>>(); Pending.Add(source); Tokens.Add(cancellationToken); return source.Task; }
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        NearbyCentres.Add(coordinate);
        return NearbyHandler is { } handler ? handler(coordinate, cancellationToken) : Task.FromResult(new ProviderResult<NearbyStopResult>());
    }
}
internal sealed class ControlledRoutingService : IRoutingService
{
    internal List<TaskCompletionSource<ProviderResult<Journey>>> Pending { get; } = [];
    internal List<DateTimeOffset> Departures { get; } = [];
    internal List<bool> ArriveBy { get; } = [];
    public Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default, bool arriveBy = false) { var source = new TaskCompletionSource<ProviderResult<Journey>>(); Pending.Add(source); Departures.Add(departure); ArriveBy.Add(arriveBy); return source.Task; }
}
internal sealed class RecordingNavigation : IJourneyNavigation
{
    internal int Results { get; private set; }
    internal int Details { get; private set; }
    public Task ShowResultsAsync() { Results++; return Task.CompletedTask; }
    public Task ShowDetailAsync() { Details++; return Task.CompletedTask; }
}
