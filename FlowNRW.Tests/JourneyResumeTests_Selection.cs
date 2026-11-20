using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Resume renews the existing route session without guessing or navigating.</summary>
public sealed class JourneyResumeTests_Selection
{
    /// <summary>A unique complete identity preserves selection without opening another page.</summary>
    [Fact]
    public async Task RefreshIfStale_UniqueJourney_UpdatesSelectionWithoutNavigation()
    {
        var fixture = await CreateAsync();
        var replacement = Journey("one") with { Id = "renewed" };
        var task = fixture.Model.RefreshIfStaleAsync(new(new()));
        fixture.Service.Pending[1].SetResult(new() { Items = [replacement] });
        await task;
        Assert.Same(replacement, fixture.Model.SelectedJourney);
        Assert.Equal(1, fixture.Navigation.Results);
        Assert.Equal(1, fixture.Navigation.Details);
    }

    /// <summary>A journey with a walking leg without trip identity stays selected.</summary>
    [Fact]
    public async Task RefreshIfStale_JourneyWithWalkingLeg_KeepsSelection()
    {
        var fixture = await CreateAsync(WalkingJourney);
        var replacement = WalkingJourney() with { Id = "renewed" };
        var task = fixture.Model.RefreshIfStaleAsync(new(new()));
        fixture.Service.Pending[1].SetResult(new() { Items = [replacement] });
        await task;
        Assert.Same(replacement, fixture.Model.SelectedJourney);
        Assert.Contains("automatisch aktualisiert", fixture.Model.Status);
    }

    /// <summary>A leg that stops being a walk cannot keep the previous selection.</summary>
    [Fact]
    public async Task RefreshIfStale_WalkingLegTurnedTransit_ClearsSelection()
    {
        var fixture = await CreateAsync(WalkingJourney);
        var replacement = WalkingJourney();
        replacement = replacement with
        { Legs = [replacement.Legs[0], replacement.Legs[1] with { Walking = null, Line = new Line { Name = "Bus 10" } }] };
        var task = fixture.Model.RefreshIfStaleAsync(new(new()));
        fixture.Service.Pending[1].SetResult(new() { Items = [replacement] });
        await task;
        Assert.Null(fixture.Model.SelectedJourney);
        Assert.Contains("nicht mehr eindeutig", fixture.Model.Status);
    }

    /// <summary>Missing and ambiguous matches cannot silently become the selected trip.</summary>
    /// <param name="duplicate">Whether the response has duplicate matching identities.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RefreshIfStale_UnsafeIdentity_ClearsSelection(bool duplicate)
    {
        var fixture = await CreateAsync();
        var replacement = Journey(duplicate ? "one" : "other");
        var task = fixture.Model.RefreshIfStaleAsync(new(new()));
        fixture.Service.Pending[1].SetResult(new() { Items = duplicate ? [replacement, replacement] : [replacement] });
        await task;
        Assert.Null(fixture.Model.SelectedJourney);
        Assert.Contains("nicht mehr eindeutig", fixture.Model.Status);
        Assert.Equal(1, fixture.Navigation.Results);
    }

    /// <summary>A failed refresh keeps the old data and its provenance visible.</summary>
    [Fact]
    public async Task RefreshIfStale_ProviderFailure_PreservesResult()
    {
        var fixture = await CreateAsync();
        var previous = fixture.Model.Result;
        var selected = fixture.Model.SelectedJourney;
        var task = fixture.Model.RefreshIfStaleAsync(new(new()));
        fixture.Service.Pending[1].SetResult(new() { ErrorCode = "offline" });
        await task;
        Assert.Same(previous, fixture.Model.Result);
        Assert.Same(selected, fixture.Model.SelectedJourney);
        Assert.Contains("Letzte bekannte", fixture.Model.Status);
    }

    /// <summary>Changing an endpoint invalidates a non-cooperative old response.</summary>
    [Fact]
    public async Task RefreshIfStale_EndpointChanged_RejectsLateResponse()
    {
        var fixture = await CreateAsync();
        var task = fixture.Model.RefreshIfStaleAsync(new(new()));
        fixture.Model.Origin.Text = "changed";
        await task.WaitAsync(TimeSpan.FromSeconds(2));
        fixture.Service.Pending[1].SetResult(new() { Items = [Journey("late")] });
        Assert.Null(fixture.Model.Result);
        Assert.Null(fixture.Model.SelectedJourney);
        Assert.False(fixture.Model.IsBusy);
    }

    private static async Task<(JourneySearchViewModel Model, ControlledRoutingService Service, RecordingNavigation Navigation)> CreateAsync(Func<Journey>? journey = null)
    {
        var service = new ControlledRoutingService();
        var navigation = new RecordingNavigation();
        var model = new JourneySearchViewModel(new(new ControlledSearchService(), 100), new(new ControlledSearchService(), 100), service, navigation);
        foreach (var endpoint in new[] { model.Origin, model.Destination })
        { endpoint.SetSelectedAddress(new Address { Name = "endpoint", Coordinate = new GeoCoordinate(51, 7) }); }
        var search = model.SearchAsync();
        var selected = journey?.Invoke() ?? Journey("one");
        service.Pending[0].SetResult(new() { Items = [selected], RetrievedAt = DateTimeOffset.UtcNow.AddMinutes(-2) });
        await search;
        await model.OpenJourneyAsync(selected);
        return (model, service, navigation);
    }

    private static Journey Journey(string id)
    {
        var time = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
        return new() { Legs = [new() { Departure = Event(id, "a", time), Arrival = Event(id, "b", time.AddMinutes(10)) }] };
    }

    private static Journey WalkingJourney()
    {
        var time = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
        return new()
        {
            Legs =
            [
                new() { Departure = Event("walk-trip", "a", time), Arrival = Event("walk-trip", "b", time.AddMinutes(10)) },
                new() { Departure = Walk("Umstieg", time.AddMinutes(10)), Arrival = Walk("Adresse", time.AddMinutes(15)), Walking = new() { Duration = TimeSpan.FromMinutes(5) } }
            ]
        };
        static StopEvent Walk(string name, DateTimeOffset planned) => new()
        { PlannedTime = planned, Identity = new() { Source = "fixture", PlannedTime = planned, Stop = new() { Name = name, Coordinate = new GeoCoordinate(51.45, 7.01) } } };
    }

    private static StopEvent Event(string trip, string stop, DateTimeOffset planned) => new()
    { Identity = new() { Source = "fixture", TripId = trip, PlannedTime = planned, Stop = new() { Id = stop, Source = "fixture" } } };
}
