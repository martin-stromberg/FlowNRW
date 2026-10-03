using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Coordinate input boundaries.</summary>
public sealed class CoordinateParserTests
{
    /// <summary>Accepts both decimal spellings and bounds.</summary>
    /// <param name="lat">Latitude.</param>
    /// <param name="lon">Longitude.</param>
    [Theory]
    [InlineData("51,45", "7.01")]
    [InlineData("-90", "180")]
    [InlineData("90", "-180")]
    public void ParsesValidCoordinates(string lat, string lon)
    {
        Assert.True(CoordinateParser.TryParse(lat, lon, out _, out _));
    }
    /// <summary>Rejects malformed and nonfinite coordinates.</summary>
    /// <param name="lat">Latitude.</param>
    /// <param name="lon">Longitude.</param>
    [Theory]
    [InlineData("", "7")]
    [InlineData("91", "7")]
    [InlineData("51", "181")]
    [InlineData("NaN", "7")]
    [InlineData("51", "Infinity")]
    [InlineData("51,1.2", "7")]
    public void RejectsInvalidCoordinates(string lat, string lon)
    {
        Assert.False(CoordinateParser.TryParse(lat, lon, out _, out _));
    }
}

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
        model.IsCoordinateMode = true; model.Latitude = "51,4"; model.Longitude = "7.0"; await model.SearchAsync(); Assert.NotNull(model.SelectedAddress?.Coordinate);
        model.Longitude = "999"; await model.SearchAsync(); Assert.Null(model.SelectedAddress); Assert.Contains("Länge", model.Status);
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
        var service = new ControlledRoutingService(); var nav = new RecordingNavigation(); var model = Create(service, nav); await Select(model);
        var old = model.SearchCommand.ExecuteAsync(); model.Origin.Latitude = "52"; await model.Origin.SearchAsync();
        Assert.True(model.SearchCommand.CanExecute(null)); var current = model.SearchCommand.ExecuteAsync();
        var journey = new Journey { Id = "new" }; service.Pending[1].SetResult(new() { Items = [journey], Source = "new" }); await current;
        service.Pending[0].SetResult(new() { Items = [new Journey { Id = "old" }], Source = "old" }); await old;
        Assert.Same(journey, Assert.Single(model.Journeys)); Assert.Equal("new", model.Result?.Source); Assert.Equal(1, nav.Results);
    }
    /// <summary>Alternative arrival time and mode reach the routing contract.</summary>
    [Fact]
    public async Task SendsSelectedArrivalTime()
    {
        var service = new ControlledRoutingService(); var model = Create(service, new RecordingNavigation()); await Select(model);
        var selected = new DateTime(2026, 10, 3).AddHours(18).AddMinutes(30);
        model.UseCurrentTime = false; model.SelectedDate = selected.Date; model.SelectedTime = selected.TimeOfDay; model.ArriveBy = true;
        var task = model.SearchAsync(); service.Pending[0].SetResult(new() { Items = [] }); await task;
        Assert.True(service.ArriveBy[0]); Assert.Equal(model.PlannedTime, service.Departures[0]);
    }
    /// <summary>Completed contexts survive page departure and selected detail navigation.</summary>
    [Fact]
    public async Task PreservesContextThroughNavigation()
    {
        var service = new ControlledRoutingService(); var nav = new RecordingNavigation(); var model = Create(service, nav); await Select(model);
        var origin = model.Origin.SelectedAddress; var task = model.SearchAsync(); var journey = new Journey { Id = "1" };
        service.Pending[0].SetResult(new() { Items = [journey] }); await task; model.CancelPending(); model.Origin.CancelPending();
        await new ResultsViewModel(model).OpenJourneyAsync(journey);
        Assert.Same(origin, model.Origin.SelectedAddress); Assert.Same(journey, model.SelectedJourney); Assert.Equal(1, nav.Details);
        Assert.Contains("Umstiege", new JourneyDetailViewModel(model).Details);
        model.Destination.Longitude = "8"; Assert.Null(model.SelectedJourney); Assert.Empty(model.Journeys);
    }
    private static JourneySearchViewModel Create(ControlledRoutingService service, RecordingNavigation nav) => new(new(new ControlledSearchService(), 100), new(new ControlledSearchService(), 100), service, nav);
    private static async Task Select(JourneySearchViewModel model)
    {
        foreach (var endpoint in new[] { model.Origin, model.Destination }) { endpoint.IsCoordinateMode = true; endpoint.Latitude = "51"; endpoint.Longitude = "7"; await endpoint.SearchAsync(); }
    }
}

/// <summary>Display semantics around midnight and unknown realtime.</summary>
public sealed class JourneyPresentationTests
{
    /// <summary>German local dates, offset, walking and cancellations remain explicit.</summary>
    [Fact]
    public void DisplaysMidnightOffsetAndUnknownRealtime()
    {
        var time = DateTimeOffset.Parse("2026-09-15T22:05:00Z");
        var journey = new Journey { Legs = [new() { Walking = new() { DistanceMeters = 200 }, Departure = new() { PlannedTime = time, Realtime = new() { Cancelled = true } } }] };
        var detail = JourneyPresentation.Detail(journey);
        Assert.Contains("16.09.2026 00:05 UTC+02:00", detail); Assert.Contains("Fußweg", detail); Assert.Contains("keine Echtzeitdaten", detail); Assert.Contains("Ausfall", detail); Assert.Contains("unbekannt", detail);
    }
}

internal sealed class ControlledSearchService : IStopSearchService
{
    internal List<TaskCompletionSource<ProviderResult<Address>>> Pending { get; } = [];
    internal List<CancellationToken> Tokens { get; } = [];
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default) { var source = new TaskCompletionSource<ProviderResult<Address>>(); Pending.Add(source); Tokens.Add(cancellationToken); return source.Task; }
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default) => Task.FromResult(new ProviderResult<NearbyStopResult>());
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
