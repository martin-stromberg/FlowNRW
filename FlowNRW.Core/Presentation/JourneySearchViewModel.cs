using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Retained search session shared by all pages.</summary>
public sealed class JourneySearchViewModel : ObservableObject
{
    private readonly IRoutingService routing;
    private readonly IJourneyNavigation navigation;
    private CancellationTokenSource? request;
    private long revision;
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
            var result = await routing.RouteAsync(Origin.SelectedAddress!, Destination.SelectedAddress!, DateTimeOffset.Now, source.Token);
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
