using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Manual stop lookup and departure monitor with retained data on refresh errors.</summary>
public sealed class StopMonitorViewModel : ObservableObject
{
    private readonly IDepartureService departures;
    private readonly IDepartureNavigation navigation;
    private CancellationTokenSource? request;
    private long revision;
    private bool opening;

    /// <summary>Creates an independent stop monitor session.</summary>
    /// <param name="search">Independent lookup service.</param>
    /// <param name="departures">Departure service.</param>
    /// <param name="navigation">Monitor navigation.</param>
    /// <param name="maxSearchLength">Configured search length.</param>
    public StopMonitorViewModel(IStopSearchService search, IDepartureService departures, IDepartureNavigation navigation, int maxSearchLength)
    {
        Lookup = new EndpointViewModel(search, maxSearchLength);
        this.departures = departures;
        this.navigation = navigation;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => SelectedStop is not null && !IsBusy,
            _ => SetStatus("Aktualisierung fehlgeschlagen. Bitte erneut versuchen."));
        Lookup.PropertyChanged += (_, _) => { Notify(nameof(Stops)); Notify(nameof(SearchStatus)); };
    }

    /// <summary>Retained stop lookup input.</summary>
    public EndpointViewModel Lookup { get; }
    /// <summary>Only identifiable stop candidates can open a monitor.</summary>
    public IReadOnlyList<Address> Stops
    {
        get
        {
            return Lookup.Matches.Where(item => !string.IsNullOrWhiteSpace(item.Stop?.Id)).ToArray();
        }
    }
    /// <summary>Stop-specific search state.</summary>
    public string SearchStatus => !Lookup.IsBusy && Lookup.Result is { ErrorCode: null } && Stops.Count == 0
        ? "Keine Haltestellen gefunden. Bitte Eingabe ändern." : Lookup.Status;
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

    /// <summary>Opens an actual candidate and loads its departures.</summary>
    /// <param name="candidate">Complete selected lookup record.</param>
    /// <returns>Navigation and initial update completion.</returns>
    public async Task OpenAsync(Address candidate)
    {
        if (opening || !Stops.Contains(candidate) || candidate.Stop is null) return;
        opening = true;
        CancelPending();
        Lookup.SelectAddress(candidate);
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
