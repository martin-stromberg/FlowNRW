using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Manual stop lookup and departure monitor with retained data on refresh errors.</summary>
public sealed class StopMonitorViewModel : ObservableObject
{
    private readonly IDepartureService departures;
    private readonly IDepartureNavigation navigation;
    private readonly IStopSearchService nearby;
    private readonly ICurrentLocationService? location;
    private CancellationTokenSource? request;
    private long revision;
    private bool opening;
    private CancellationTokenSource? nearbyRequest;
    private long nearbyRevision;
    private bool nearbyActive;

    /// <summary>Creates an independent stop monitor session.</summary>
    /// <param name="search">Independent lookup service.</param>
    /// <param name="departures">Departure service.</param>
    /// <param name="navigation">Monitor navigation.</param>
    /// <param name="maxSearchLength">Configured search length.</param>
    /// <param name="location">Optional current-location provider.</param>
    /// <param name="nearbySearch">Independent nearby lookup scope.</param>
    public StopMonitorViewModel(IStopSearchService search, IDepartureService departures, IDepartureNavigation navigation, int maxSearchLength, ICurrentLocationService? location = null, IStopSearchService? nearbySearch = null)
    {
        Lookup = new EndpointViewModel(search, maxSearchLength);
        this.departures = departures;
        this.navigation = navigation;
        this.location = location;
        nearby = nearbySearch ?? search;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => SelectedStop is not null && !IsBusy,
            _ => SetStatus("Aktualisierung fehlgeschlagen. Bitte erneut versuchen."));
        Lookup.PropertyChanged += (_, _) => RefreshSearchBindings();
        Lookup.Changed += OnLookupChanged;
        NearbyCommand = new AsyncRelayCommand(FindNearbyAsync, () => !IsNearbyBusy && location is not null, _ => SetNearbyFailure("Nahe Haltestellen konnten nicht geladen werden."));
    }

    /// <summary>Retained stop lookup input.</summary>
    public EndpointViewModel Lookup { get; }
    /// <summary>Only identifiable stop candidates can open a monitor.</summary>
    public IReadOnlyList<Address> Stops
    {
        get
        {
            return nearbyActive ? NearbyStops : Lookup.Matches.Where(item => !string.IsNullOrWhiteSpace(item.Stop?.Id)).ToArray();
        }
    }
    /// <summary>Stop-specific search state.</summary>
    public string SearchStatus => nearbyActive || IsNearbyBusy ? NearbyStatus : !Lookup.IsBusy && Lookup.Result is { ErrorCode: null } && Stops.Count == 0
        ? "Keine Haltestellen gefunden. Bitte Eingabe ändern." : Lookup.Status;
    /// <summary>Provenance belonging to the currently displayed candidates.</summary>
    public string SearchMetadata
    {
        get { return nearbyActive ? NearbyResult is null ? "" : JourneyPresentation.Metadata(NearbyResult) : Lookup.Metadata; }
    }
    /// <summary>Whether the location-to-nearby request chain is running.</summary>
    public bool IsNearbyBusy { get; private set; }
    /// <summary>Complete selected stop identity.</summary>
    public Stop? SelectedStop { get; private set; }
    /// <summary>Selected stop title.</summary>
    public string Title => SelectedStop?.Name ?? "Abfahrten";
    /// <summary>Last successful result, retained on failed updates.</summary>
    public ProviderResult<StopEvent>? Result { get; private set; }
    /// <summary>Latest attempted provider response.</summary>
    public ProviderResult<StopEvent>? LastAttempt { get; private set; }
    /// <summary>Currently displayed departures.</summary>
    public IReadOnlyList<StopEvent> Items => Result?.Items ?? [];
    /// <summary>Provenance of the displayed data or the first failed attempt.</summary>
    public string Metadata
    {
        get
        {
            var displayed = Result ?? LastAttempt;
            return displayed is null ? "" : JourneyPresentation.Metadata(displayed);
        }
    }
    /// <summary>Whether an update is active.</summary>
    public bool IsBusy { get; private set; }
    /// <summary>Loading, empty or retained-data status.</summary>
    public string Status { get; private set; } = "Bitte eine Haltestelle auswählen.";
    /// <summary>Manual refresh action.</summary>
    public AsyncRelayCommand RefreshCommand { get; }
    /// <summary>Finds stops around the explicitly requested current position.</summary>
    public AsyncRelayCommand NearbyCommand { get; }
    /// <summary>Nearby result status.</summary>
    public string NearbyStatus { get; private set; } = "Standort nur nach Aktion verwenden.";
    /// <summary>Current nearby candidates.</summary>
    public IReadOnlyList<Address> NearbyStops { get; private set; } = [];
    /// <summary>Nearby provenance.</summary>
    public ProviderResult<NearbyStopResult>? NearbyResult { get; private set; }

    private async Task FindNearbyAsync()
    {
        if (location is null) return;
        CancelNearbyPending();
        Lookup.CancelPending();
        var version = nearbyRevision;
        using var source = new CancellationTokenSource(); nearbyRequest = source;
        IsNearbyBusy = true; NearbyStatus = "Standort wird ermittelt …"; RefreshSearchBindings();
        try
        {
            var position = await location.GetCurrentAsync(source.Token);
            if (version != nearbyRevision) return;
            if (!position.HasCurrentPosition) { SetNearbyFailure(position.FailureDescription); return; }
            NearbyStatus = "Nahe Haltestellen werden geladen …"; RefreshSearchBindings();
            var result = await nearby.NearbyAsync(position.Coordinate!, source.Token);
            if (version != nearbyRevision) return;
            if (result.ErrorCode is not null) { SetNearbyFailure("Nahe Haltestellen konnten nicht geladen werden."); return; }
            NearbyResult = result;
            NearbyStops = result.Items.Where(x => !string.IsNullOrWhiteSpace(x.Stop?.Id)).Select(x => new Address { Name = x.Stop!.Name, Stop = x.Stop, Coordinate = x.Stop.Coordinate }).ToArray();
            nearbyActive = true;
            NearbyStatus = NearbyStops.Count == 0 ? "Keine Haltestellen in der Nähe gefunden." : $"{NearbyStops.Count} nahe Haltestellen gefunden.";
            NearbyStatus += position.AccuracyDescription;
        }
        catch (OperationCanceledException) { if (version == nearbyRevision) SetNearbyFailure("Umgebungssuche abgebrochen."); }
        catch (Exception) { if (version == nearbyRevision) SetNearbyFailure("Nahe Haltestellen konnten nicht geladen werden. Bitte erneut versuchen."); }
        finally { if (version == nearbyRevision) { nearbyRequest = null; IsNearbyBusy = false; RefreshSearchBindings(); } }
    }

    /// <summary>Describes only an actual provider-supplied distance for an active candidate.</summary>
    /// <param name="candidate">Current list candidate.</param>
    /// <returns>Distance or an explicit unknown label.</returns>
    public string DistanceLabel(Address candidate)
    {
        if (!nearbyActive || !NearbyStops.Any(item => ReferenceEquals(item, candidate))) return "";
        var distance = NearbyResult?.Items.FirstOrDefault(item => ReferenceEquals(item.Stop, candidate.Stop))?.DistanceMeters;
        return distance is { } meters && double.IsFinite(meters) && meters >= 0
            ? $" · Entfernung: {meters.ToString("0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"))} m" : " · Entfernung unbekannt";
    }

    /// <summary>Cancels pending nearby work while preserving completed candidates.</summary>
    public void CancelNearbyPending()
    {
        nearbyRevision++;
        nearbyRequest?.Cancel(); nearbyRequest = null;
        if (IsNearbyBusy) SetNearbyFailure("Umgebungssuche abgebrochen.");
        IsNearbyBusy = false;
        NearbyCommand.InvalidateExecution();
        RefreshSearchBindings();
    }

    private void OnLookupChanged(object? sender, EventArgs args)
    {
        CancelNearbyPending();
        nearbyActive = false; NearbyStops = []; NearbyResult = null;
        NearbyStatus = "Standort nur nach Aktion verwenden.";
        RefreshSearchBindings();
    }

    private void SetNearbyFailure(string description)
    {
        NearbyStatus = description + (Stops.Count > 0 ? " Vorherige Ergebnisse werden angezeigt; keine neue Umgebung ermittelt." : "");
        RefreshSearchBindings();
    }

    private void RefreshSearchBindings()
    {
        Notify(nameof(Stops)); Notify(nameof(SearchStatus)); Notify(nameof(SearchMetadata));
        Notify(nameof(NearbyStatus)); Notify(nameof(NearbyResult)); Notify(nameof(NearbyStops)); Notify(nameof(IsNearbyBusy));
        NearbyCommand?.Refresh();
    }

    /// <summary>Opens an actual candidate and loads its departures.</summary>
    /// <param name="candidate">Complete selected lookup record.</param>
    /// <returns>Navigation and initial update completion.</returns>
    public async Task OpenAsync(Address candidate)
    {
        if (opening || !Stops.Any(item => ReferenceEquals(item, candidate)) || candidate.Stop is null) return;
        opening = true;
        CancelNearbyPending();
        CancelPending();
        if (!nearbyActive) Lookup.SelectAddress(candidate);
        SelectedStop = candidate.Stop;
        Result = null;
        LastAttempt = null;
        SetStatus("Abfahrten werden geladen …");
        RefreshBindings();
        var version = revision;
        try
        {
            await navigation.ShowMonitorAsync();
        }
        finally
        {
            opening = false;
        }
        if (version == revision) await RefreshAsync();
    }

    /// <summary>Refreshes without discarding the last successful data on errors.</summary>
    /// <returns>Refresh completion.</returns>
    public async Task RefreshAsync()
    {
        if (SelectedStop is null || IsBusy) return;
        var version = ++revision;
        using var source = new CancellationTokenSource();
        request = source;
        IsBusy = true;
        SetStatus("Abfahrten werden aktualisiert …");
        RefreshBindings();
        try
        {
            var started = DateTimeOffset.Now;
            var result = await departures.DeparturesAsync(SelectedStop, started, source.Token);
            if (version != revision) return;
            LastAttempt = result;
            if (result.ErrorCode is not null)
                SetFailure();
            else
            {
                Result = result with
                {
                    Items = result.Items.Where(item => EffectiveTime(item) is not { } time || time >= started)
                        .OrderBy(item => EffectiveTime(item) ?? DateTimeOffset.MaxValue).ToArray()
                };
                SetStatus(Items.Count == 0 ? "Keine nächsten Abfahrten gefunden." : $"{Items.Count} Abfahrten · manuell aktualisiert.");
            }
        }
        catch (OperationCanceledException)
        {
            if (version == revision) SetStatus("Aktualisierung abgebrochen.");
        }
        catch (Exception)
        {
            if (version == revision) SetFailure();
        }
        finally
        {
            if (version == revision)
            {
                request = null;
                IsBusy = false;
                RefreshBindings();
            }
        }
    }

    /// <summary>Cancels a departed page without clearing completed data.</summary>
    public void CancelPending()
    {
        revision++;
        request?.Cancel();
        request = null;
        if (IsBusy) SetStatus("Aktualisierung abgebrochen.");
        IsBusy = false;
        RefreshCommand.InvalidateExecution();
        RefreshBindings();
    }

    private static DateTimeOffset? EffectiveTime(StopEvent item) => item.Realtime.ActualTime
        ?? (item.PlannedTime is { } planned ? planned + (item.Realtime.Delay ?? TimeSpan.Zero) : null);

    private void SetFailure() => SetStatus(Result is null ? "Abfahrten konnten nicht geladen werden. Bitte erneut versuchen."
        : "Aktualisierung fehlgeschlagen. Letzte bekannte Daten werden angezeigt; bitte Datenstand beachten.");

    private void SetStatus(string text)
    {
        Status = text;
        Notify(nameof(Status));
    }

    private void RefreshBindings()
    {
        Notify(nameof(SelectedStop));
        Notify(nameof(Title));
        Notify(nameof(Result));
        Notify(nameof(LastAttempt));
        Notify(nameof(Items));
        Notify(nameof(Metadata));
        Notify(nameof(IsBusy));
        RefreshCommand.Refresh();
    }
}
