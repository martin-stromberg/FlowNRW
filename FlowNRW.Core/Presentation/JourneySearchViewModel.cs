using FlowNRW.Core.Transit;
using FlowNRW.Core.Refresh;

namespace FlowNRW.Core.Presentation;

/// <summary>Retained search session shared by all pages.</summary>
public sealed class JourneySearchViewModel : ObservableObject
{
    private readonly IRoutingService routing;
    private readonly IJourneyNavigation navigation;
    private CancellationTokenSource? request;
    private long revision;
    private bool useCurrentTime = true;
    private DateTime selectedDate = DateTime.Now.Date;
    private TimeSpan selectedTime = DateTime.Now.TimeOfDay;
    private bool arriveBy;
    /// <summary>Creates a search session.</summary>
    /// <param name="origin">Independent origin.</param>
    /// <param name="destination">Independent destination.</param>
    /// <param name="routing">Real routing contract.</param>
    /// <param name="navigation">Page navigation.</param>
    public JourneySearchViewModel(EndpointViewModel origin, EndpointViewModel destination, IRoutingService routing, IJourneyNavigation navigation)
    {
        Origin = origin; Destination = destination; this.routing = routing; this.navigation = navigation;
        SearchCommand = new AsyncRelayCommand(SearchAsync, () => CanSearch, _ => SetStatus("Navigation fehlgeschlagen. Bitte erneut versuchen."));
        Origin.Changed += EndpointChanged; Destination.Changed += EndpointChanged;
    }
    /// <summary>Origin input.</summary>
    public EndpointViewModel Origin { get; }
    /// <summary>Destination input.</summary>
    public EndpointViewModel Destination { get; }
    /// <summary>Complete provider result.</summary>
    public ProviderResult<Journey>? Result { get; private set; }
    /// <summary>Provider-ordered journeys.</summary>
    public IReadOnlyList<Journey> Journeys => Result?.Items ?? [];
    /// <summary>Selected result.</summary>
    public Journey? SelectedJourney { get; private set; }
    /// <summary>Routing activity.</summary>
    public bool IsBusy { get; private set; }
    /// <summary>Whether routing is allowed.</summary>
    public bool CanSearch => Origin.SelectedAddress is not null && Destination.SelectedAddress is not null && !IsBusy;
    /// <summary>Readable status.</summary>
    public string Status { get; private set; } = "Bitte Start und Ziel auswählen.";
    /// <summary>Readable provider metadata.</summary>
    public string Metadata
    {
        get
        {
            return Result is null ? "" : JourneyPresentation.Metadata(Result);
        }
    }
    /// <summary>Route action.</summary>
    public AsyncRelayCommand SearchCommand { get; }
    /// <summary>Whether routing uses the current time.</summary>
    public bool UseCurrentTime
    {
        get => useCurrentTime;
        set
        {
            if (useCurrentTime == value) return;
            useCurrentTime = value;
            Notify();
            Notify(nameof(PlannedTime));
        }
    }
    /// <summary>Alternative local search date.</summary>
    public DateTime SelectedDate
    {
        get => selectedDate;
        set
        {
            if (selectedDate == value) return;
            selectedDate = value.Date;
            Notify();
            Notify(nameof(PlannedTime));
        }
    }
    /// <summary>Alternative local search time.</summary>
    public TimeSpan SelectedTime
    {
        get => selectedTime;
        set
        {
            if (selectedTime == value) return;
            selectedTime = value;
            Notify();
            Notify(nameof(PlannedTime));
        }
    }
    /// <summary>Whether the requested time is an arrival deadline.</summary>
    public bool ArriveBy
    {
        get => arriveBy;
        set
        {
            if (arriveBy == value) return;
            arriveBy = value;
            Notify();
        }
    }
    /// <summary>Effective request time.</summary>
    public DateTimeOffset PlannedTime
        => UseCurrentTime
            ? DateTimeOffset.Now
            : new DateTimeOffset(SelectedDate.Date + SelectedTime, TimeZoneInfo.Local.GetUtcOffset(SelectedDate.Date + SelectedTime));
    /// <summary>Routes the selected identities.</summary>
    /// <returns>Routing and navigation completion.</returns>
    public async Task SearchAsync()
    {
        if (!CanSearch) return;
        CancelPending(); var version = revision;
        using var source = new CancellationTokenSource(); request = source;
        IsBusy = true; Refresh(); SetStatus("Verbindungen werden geladen …");
        try
        {
            var result = await routing.RouteAsync(Origin.SelectedAddress!, Destination.SelectedAddress!, PlannedTime, source.Token, ArriveBy);
            if (version != revision) return;
            Result = result; SelectedJourney = null; Refresh();
            SetStatus(result.ErrorCode is not null ? "Verbindungssuche fehlgeschlagen. Bitte erneut versuchen." : result.Items.Count == 0 ? "Keine Verbindungen gefunden. Bitte erneut suchen." : $"{result.Items.Count} Verbindungen gefunden.");
            if (result.HasData) { request = null; IsBusy = false; Refresh(); await navigation.ShowResultsAsync(); }
        }
        catch (OperationCanceledException) { if (version == revision) SetStatus("Verbindungssuche abgebrochen."); }
        catch (Exception) { if (version == revision) SetStatus("Verbindungssuche fehlgeschlagen. Bitte erneut versuchen."); }
        finally { if (version == revision) { request = null; IsBusy = false; Refresh(); } }
    }
    /// <summary>Opens a journey belonging to the current result.</summary>
    /// <param name="journey">Current journey.</param>
    /// <returns>Navigation completion.</returns>
    public async Task OpenJourneyAsync(Journey journey)
    {
        if (!Journeys.Contains(journey)) return;
        SelectedJourney = journey; Notify(nameof(SelectedJourney)); await navigation.ShowDetailAsync();
    }

    /// <summary>Swaps the resolved start and destination without performing a provider request.</summary>
    public void SwapEndpoints()
    {
        var origin = Origin.SelectedAddress;
        var destination = Destination.SelectedAddress;
        Origin.SetSelectedAddress(destination);
        Destination.SetSelectedAddress(origin);
    }

    /// <summary>Applies a saved connection without searching for either endpoint.</summary>
    /// <param name="origin">Saved origin.</param>
    /// <param name="destination">Saved destination.</param>
    public void SelectConnection(Address origin, Address destination)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);
        Origin.SetSelectedAddress(origin);
        Destination.SetSelectedAddress(destination);
    }

    /// <summary>Renews expired visible results without navigating or guessing the previously selected trip.</summary>
    /// <param name="freshness">Shared realtime age policy.</param>
    /// <param name="cancellationToken">Visible page lifetime.</param>
    /// <returns>Refresh completion with errors represented in the retained session.</returns>
    public async Task RefreshIfStaleAsync(RefreshFreshness freshness, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Notify(nameof(Metadata));
        if (Result is null || !freshness.IsStale(Result) || !CanSearch) return;
        var previous = Result;
        var selected = SelectedJourney;
        var version = ++revision;
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        request = source;
        IsBusy = true;
        SetStatus("Verbindungen werden aktualisiert …");
        Refresh();
        try
        {
            var result = await routing.RouteAsync(Origin.SelectedAddress!, Destination.SelectedAddress!, PlannedTime, source.Token, ArriveBy).WaitAsync(source.Token);
            source.Token.ThrowIfCancellationRequested();
            if (version != revision) return;
            if (result.ErrorCode is not null)
            {
                SetStatus("Aktualisierung fehlgeschlagen. Letzte bekannte Verbindungen werden angezeigt; bitte Datenstand beachten.");
                return;
            }
            var matches = selected is null ? [] : result.Items.Where(item => SameJourney(selected, item)).ToArray();
            SelectedJourney = matches.Length == 1 && previous.Items.Count(item => SameJourney(item, matches[0])) == 1 ? matches[0] : null;
            Result = result;
            SetStatus(selected is not null && SelectedJourney is null
                ? "Die gewählte Verbindung ist nicht mehr eindeutig bestätigt. Bitte in der Ergebnisliste neu auswählen."
                : result.Items.Count == 0 ? "Keine aktuellen Verbindungen gefunden. Bitte erneut suchen."
                : $"{result.Items.Count} Verbindungen automatisch aktualisiert.");
        }
        catch (OperationCanceledException) { if (version == revision) SetStatus("Aktualisierung abgebrochen. Letzte bekannte Verbindungen bleiben sichtbar."); }
        catch (Exception) { if (version == revision) SetStatus("Aktualisierung fehlgeschlagen. Letzte bekannte Verbindungen werden angezeigt; bitte Datenstand beachten."); }
        finally
        {
            if (version == revision) { request = null; IsBusy = false; Refresh(); }
        }
    }

    private static bool SameJourney(Journey left, Journey right) => left.Legs.Count > 0 && left.Legs.Count == right.Legs.Count
        && left.Legs.Zip(right.Legs).All(pair => SameEvent(pair.First.Departure, pair.Second.Departure)
            && SameEvent(pair.First.Arrival, pair.Second.Arrival));

    private static bool SameEvent(StopEvent left, StopEvent right)
    {
        var first = left.Identity;
        var second = right.Identity;
        return !string.IsNullOrWhiteSpace(first.Source) && first.Source == second.Source
            && !string.IsNullOrWhiteSpace(first.TripId) && first.TripId == second.TripId
            && !string.IsNullOrWhiteSpace(first.Stop.Source) && first.Stop.Source == second.Stop.Source
            && !string.IsNullOrWhiteSpace(first.Stop.Id) && first.Stop.Id == second.Stop.Id
            && first.PlannedTime is not null && first.PlannedTime == second.PlannedTime;
    }
    /// <summary>Cancels only unfinished work.</summary>
    public void CancelPending()
    {
        revision++;
        request?.Cancel();
        request = null;
        if (IsBusy) SetStatus("Verbindungssuche abgebrochen.");
        IsBusy = false;
        Refresh();
        SearchCommand.InvalidateExecution();
    }
    private void EndpointChanged(object? sender, EventArgs e) { CancelPending(); Result = null; SelectedJourney = null; SetStatus("Bitte Start und Ziel auswählen oder Verbindung suchen."); Refresh(); }
    private void SetStatus(string value) { Status = value; Notify(nameof(Status)); }
    private void Refresh() { Notify(nameof(Result)); Notify(nameof(Journeys)); Notify(nameof(Metadata)); Notify(nameof(SelectedJourney)); Notify(nameof(IsBusy)); Notify(nameof(CanSearch)); SearchCommand.Refresh(); }
}
