using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Explicit location requests preserve selections and reject superseded responses.</summary>
public sealed class LocationTests
{
    /// <summary>Every failure keeps a completed manual endpoint usable.</summary>
    /// <param name="status">Platform failure.</param>
    [Theory]
    [InlineData(LocationStatus.Denied)]
    [InlineData(LocationStatus.Disabled)]
    [InlineData(LocationStatus.Unsupported)]
    [InlineData(LocationStatus.Timeout)]
    [InlineData(LocationStatus.Unavailable)]
    [InlineData(LocationStatus.Error)]
    public async Task FailedPositionPreservesSelection(LocationStatus status)
    {
        var location = new ControlledLocation();
        var endpoint = new EndpointViewModel(new ControlledSearchService(), 100, location)
        { IsCoordinateMode = true, Latitude = "51", Longitude = "7" };
        await endpoint.SearchAsync();
        var selected = endpoint.SelectedAddress;
        var task = endpoint.LocationCommand.ExecuteAsync();
        Assert.Same(selected, endpoint.SelectedAddress);
        location.Pending[0].SetResult(new(status));
        await task;
        Assert.Same(selected, endpoint.SelectedAddress);
        Assert.False(endpoint.IsBusy);
        Assert.True(endpoint.LocationCommand.CanExecute(null));
    }

    /// <summary>Typing cancels the OS request and an uncooperative old result cannot overwrite input.</summary>
    [Fact]
    public async Task ManualEditCancelsAndRejectsLatePosition()
    {
        var location = new ControlledLocation();
        var endpoint = new EndpointViewModel(new ControlledSearchService(), 100, location);
        var old = endpoint.LocationCommand.ExecuteAsync();
        endpoint.Text = "Berlin";
        Assert.True(location.Tokens[0].IsCancellationRequested);
        var current = endpoint.LocationCommand.ExecuteAsync();
        location.Pending[1].SetResult(new(LocationStatus.Success, new(52, 13)));
        await current;
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7)));
        await old;
        Assert.Equal(new GeoCoordinate(52, 13), endpoint.SelectedAddress?.Coordinate);
    }

    /// <summary>Nearby candidates, provenance, distances and map selection use the same complete identity.</summary>
    [Fact]
    public async Task NearbyUsesActualPositionAndOpensSameMapStop()
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby();
        var departures = new ControlledDepartureService(); var navigation = new DepartureTestNavigation();
        var model = new StopMonitorViewModel(new ControlledSearchService(), departures, navigation, 100, location, nearby);
        var task = model.NearbyCommand.ExecuteAsync();
        var position = new GeoCoordinate(51, 7);
        location.Pending[0].SetResult(new(LocationStatus.Success, position));
        Assert.Same(position, Assert.Single(nearby.Centres));
        var stop = new Stop { Id = "id", Dhid = "de:test", Source = "efa", Name = "Station", Coordinate = position };
        nearby.Pending[0].SetResult(new() { Items = [new() { Stop = stop, DistanceMeters = 123 }], Source = "near", IsFallback = true, IsStale = true });
        await task;
        var candidate = Assert.Single(model.Stops);
        Assert.Same(stop, candidate.Stop);
        Assert.Contains("123", model.DistanceLabel(candidate));
        Assert.Contains("Fallback", model.SearchMetadata);
        var map = new MapViewModel(model); map.ShowStops();
        Assert.Equal(model.SearchMetadata, map.Metadata);
        var opening = map.SelectAsync(map.Session, 0);
        Assert.Same(stop, Assert.Single(departures.Stops));
        departures.Pending[0].SetResult(new()); await opening;
        Assert.Equal(1, navigation.Opens);
    }

    /// <summary>Manual input cancels a provider request and removes old map membership.</summary>
    [Fact]
    public async Task ManualInputRejectsLateNearby()
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 100, location, nearby);
        var task = model.NearbyCommand.ExecuteAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7)));
        model.Lookup.Text = "manual";
        Assert.True(nearby.Tokens[0].IsCancellationRequested);
        nearby.Pending[0].SetResult(new() { Items = [new() { Stop = new() { Id = "old" } }] });
        await task;
        Assert.Empty(model.Stops);
        Assert.False(model.IsNearbyBusy);
    }

    /// <summary>Independent endpoints do not cancel each other; navigation releases a request.</summary>
    [Fact]
    public async Task EndpointsAndNavigationCancelIndependently()
    {
        var location = new ControlledLocation();
        var origin = new EndpointViewModel(new ControlledSearchService(), 100, location);
        var destination = new EndpointViewModel(new ControlledSearchService(), 100, location);
        Assert.Empty(location.Pending);
        var first = origin.LocationCommand.ExecuteAsync(); var second = destination.LocationCommand.ExecuteAsync();
        origin.CancelPending();
        Assert.True(location.Tokens[0].IsCancellationRequested);
        Assert.False(location.Tokens[1].IsCancellationRequested);
        location.Pending[1].SetResult(new(LocationStatus.Success, new(52, 13), IsReducedAccuracy: true));
        location.Pending[0].SetException(new InvalidOperationException("sensitive"));
        await Task.WhenAll(first, second);
        Assert.Null(origin.SelectedAddress);
        Assert.Contains("Ungefähre", destination.LocationStatus);
        Assert.DoesNotContain("sensitive", origin.LocationStatus);
        Assert.True(origin.LocationCommand.CanExecute(null));
    }

    /// <summary>Selecting a retained candidate supersedes a pending position.</summary>
    [Fact]
    public async Task CandidateSelectionWinsOverLatePosition()
    {
        var location = new ControlledLocation(); var search = new ControlledSearchService();
        var model = new EndpointViewModel(search, 100, location) { Text = "station" };
        var lookup = model.SearchAsync(); var candidate = new Address { Name = "station" };
        search.Pending[0].SetResult(new() { Items = [candidate] }); await lookup;
        var position = model.LocationCommand.ExecuteAsync(); model.SelectAddress(candidate);
        Assert.True(location.Tokens[0].IsCancellationRequested);
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7))); await position;
        Assert.Same(candidate, model.SelectedAddress);
    }

    /// <summary>Missing or stale positions never become endpoints.</summary>
    /// <param name="minutes">Timestamp offset; null means no coordinate.</param>
    [Theory]
    [InlineData(null)]
    [InlineData(-10)]
    [InlineData(10)]
    public async Task RejectsMissingStaleAndFuturePositions(int? minutes)
    {
        var location = new ControlledLocation(); var model = new EndpointViewModel(new ControlledSearchService(), 100, location);
        var task = model.LocationCommand.ExecuteAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, minutes.HasValue ? new(51, 7) : null,
            minutes.HasValue ? DateTimeOffset.UtcNow.AddMinutes(minutes.Value) : null));
        await task;
        Assert.Null(model.SelectedAddress);
        Assert.Contains("Keine aktuelle Position", model.LocationStatus);
    }

    /// <summary>Unknown distances are explicit and failed updates retain clearly labelled previous data.</summary>
    [Fact]
    public async Task NearbyFailureRetainsProvenanceAndEmptySuccessClears()
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 100, location, nearby);
        var task = model.NearbyCommand.ExecuteAsync(); location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7)));
        nearby.Pending[0].SetResult(new() { Source = "original", Items = [new() { Stop = new() { Id = "1" }, DistanceMeters = double.NaN }, new() { Stop = new() { Id = "2" }, DistanceMeters = -1 }, new() { Stop = new() { Id = "3" } }] });
        await task;
        Assert.All(model.Stops, item => Assert.Contains("unbekannt", model.DistanceLabel(item)));
        var previous = model.Stops;
        task = model.NearbyCommand.ExecuteAsync(); location.Pending[1].SetResult(new(LocationStatus.Success, new(52, 13)));
        nearby.Pending[1].SetResult(new() { Source = "failure", ErrorCode = "timeout" }); await task;
        Assert.Same(previous, model.Stops);
        Assert.Contains("Vorherige Ergebnisse", model.SearchStatus);
        Assert.Contains("original", model.SearchMetadata);
        task = model.NearbyCommand.ExecuteAsync(); location.Pending[2].SetResult(new(LocationStatus.Success, new(52, 13)));
        nearby.Pending[2].SetResult(new() { Source = "empty" }); await task;
        Assert.Empty(model.Stops);
        Assert.Contains("Keine Haltestellen", model.SearchStatus);
        Assert.Contains("empty", model.SearchMetadata);
    }

    /// <summary>A departed location request can be replaced and its late answer never calls the provider.</summary>
    [Fact]
    public async Task NearbyNavigationAndReplacementRejectLatePosition()
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 100, location, nearby);
        var old = model.NearbyCommand.ExecuteAsync(); model.CancelNearbyPending();
        Assert.True(location.Tokens[0].IsCancellationRequested);
        var current = model.NearbyCommand.ExecuteAsync();
        location.Pending[1].SetResult(new(LocationStatus.Success, new(52, 13)));
        nearby.Pending[0].SetResult(new() { Source = "current" }); await current;
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7))); await old;
        Assert.Single(nearby.Pending);
        Assert.Equal("current", model.NearbyResult?.Source);
    }

    /// <summary>Old maps cannot select a value-equal candidate from a new result generation.</summary>
    [Fact]
    public async Task NewNearbyGenerationRejectsOldMapSelection()
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby(); var navigation = new DepartureTestNavigation();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), navigation, 100, location, nearby);
        var stop = new Stop { Id = "same" };
        var first = model.NearbyCommand.ExecuteAsync(); location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7)));
        nearby.Pending[0].SetResult(new() { Items = [new() { Stop = stop }] }); await first;
        var map = new MapViewModel(model); map.ShowStops();
        var second = model.NearbyCommand.ExecuteAsync(); location.Pending[1].SetResult(new(LocationStatus.Success, new(51, 7)));
        nearby.Pending[1].SetResult(new() { Items = [new() { Stop = stop }] }); await second;
        await map.SelectAsync(map.Session, 0);
        Assert.Equal(0, navigation.Opens);
    }

    /// <summary>Location failures never call Nearby, and a subsequent explicit request recovers.</summary>
    /// <param name="status">Returned failure.</param>
    [Theory]
    [InlineData(LocationStatus.Denied)]
    [InlineData(LocationStatus.Disabled)]
    [InlineData(LocationStatus.Timeout)]
    [InlineData(LocationStatus.Unsupported)]
    [InlineData(LocationStatus.Unavailable)]
    [InlineData(LocationStatus.Error)]
    public async Task NearbyLocationFailureAllowsRetry(LocationStatus status)
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 100, location, nearby);
        var first = model.NearbyCommand.ExecuteAsync(); location.Pending[0].SetResult(new(status) { Message = "sensitive provider detail" }); await first;
        Assert.Empty(nearby.Pending);
        Assert.DoesNotContain("sensitive", model.NearbyStatus);
        Assert.True(model.NearbyCommand.CanExecute(null));
        var second = model.NearbyCommand.ExecuteAsync(); location.Pending[1].SetResult(new(LocationStatus.Success, new(51, 7)));
        nearby.Pending[0].SetResult(new()); await second;
        Assert.Contains("Keine Haltestellen", model.SearchStatus);
    }

    /// <summary>A provider that ignores cancellation cannot replace a more recent environment.</summary>
    [Fact]
    public async Task LateProviderCannotReplaceNewNearbyResult()
    {
        var location = new ControlledLocation(); var nearby = new ControlledNearby();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 100, location, nearby);
        var old = model.NearbyCommand.ExecuteAsync(); location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7)));
        model.CancelNearbyPending();
        var current = model.NearbyCommand.ExecuteAsync(); location.Pending[1].SetResult(new(LocationStatus.Success, new(52, 13)));
        nearby.Pending[1].SetResult(new() { Source = "current", Items = [new() { Stop = new() { Id = "new" } }] }); await current;
        Assert.True(nearby.Tokens[0].IsCancellationRequested);
        nearby.Pending[0].SetException(new InvalidOperationException("private")); await old;
        Assert.Equal("new", Assert.Single(model.Stops).Stop?.Id);
        Assert.DoesNotContain("private", model.NearbyStatus);
    }

    /// <summary>Exceptions release both actions and omit sensitive exception text.</summary>
    [Fact]
    public async Task ExceptionsRemainSafeAndRecoverable()
    {
        var location = new ControlledLocation();
        var endpoint = new EndpointViewModel(new ControlledSearchService(), 100, location);
        var endpointTask = endpoint.LocationCommand.ExecuteAsync();
        location.Pending[0].SetException(new InvalidOperationException("private")); await endpointTask;
        Assert.DoesNotContain("private", endpoint.LocationStatus);
        Assert.True(endpoint.LocationCommand.CanExecute(null));
        var nearby = new ControlledNearby();
        var model = new StopMonitorViewModel(new ControlledSearchService(), new ControlledDepartureService(), new DepartureTestNavigation(), 100, location, nearby);
        var task = model.NearbyCommand.ExecuteAsync(); location.Pending[1].SetResult(new(LocationStatus.Success, new(51, 7)));
        nearby.Pending[0].SetException(new InvalidOperationException("private")); await task;
        Assert.DoesNotContain("private", model.NearbyStatus);
        Assert.Contains("nicht geladen", model.NearbyStatus);
        Assert.True(model.NearbyCommand.CanExecute(null));
    }

    /// <summary>A current position clears provenance from an earlier address lookup.</summary>
    [Fact]
    public async Task SuccessfulPositionClearsOldLookupMetadata()
    {
        var location = new ControlledLocation(); var search = new ControlledSearchService();
        var model = new EndpointViewModel(search, 100, location) { Text = "stop" };
        var lookup = model.SearchAsync(); search.Pending[0].SetResult(new() { Source = "old", Items = [new() { Name = "stop" }] }); await lookup;
        var task = model.LocationCommand.ExecuteAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7), AccuracyMeters: 150, IsReducedAccuracy: true)); await task;
        Assert.Empty(model.Matches); Assert.Null(model.Result); Assert.Empty(model.Metadata);
        Assert.Contains("übernommen", model.Status);
        Assert.Contains("150 m", model.LocationStatus);
        Assert.Contains("Ungefähre", model.LocationStatus);
    }

    /// <summary>Coordinate mode changes cancel an outstanding location lookup.</summary>
    [Fact]
    public async Task CoordinateModeChangeRejectsLatePosition()
    {
        var location = new ControlledLocation(); var model = new EndpointViewModel(new ControlledSearchService(), 100, location);
        var task = model.LocationCommand.ExecuteAsync(); model.IsCoordinateMode = true;
        Assert.True(location.Tokens[0].IsCancellationRequested);
        model.Latitude = "52"; model.Longitude = "13"; await model.SearchAsync();
        location.Pending[0].SetResult(new(LocationStatus.Success, new(51, 7))); await task;
        Assert.Equal(new GeoCoordinate(52, 13), model.SelectedAddress?.Coordinate);
    }
}

internal sealed class ControlledLocation : ICurrentLocationService
{
    internal List<TaskCompletionSource<LocationResult>> Pending { get; } = [];
    internal List<CancellationToken> Tokens { get; } = [];
    public Task<LocationResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        Tokens.Add(cancellationToken); var source = new TaskCompletionSource<LocationResult>(); Pending.Add(source); return source.Task;
    }
}

internal sealed class ControlledNearby : IStopSearchService
{
    internal List<TaskCompletionSource<ProviderResult<NearbyStopResult>>> Pending { get; } = [];
    internal List<CancellationToken> Tokens { get; } = [];
    internal List<GeoCoordinate> Centres { get; } = [];
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default) => Task.FromResult(new ProviderResult<Address>());
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        Centres.Add(coordinate); Tokens.Add(cancellationToken);
        var source = new TaskCompletionSource<ProviderResult<NearbyStopResult>>(); Pending.Add(source); return source.Task;
    }
}
