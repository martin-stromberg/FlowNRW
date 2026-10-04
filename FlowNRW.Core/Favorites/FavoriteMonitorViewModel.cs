using System.Globalization;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;
using FlowNRW.Core.Refresh;

namespace FlowNRW.Core.Favorites;

/// <summary>Independent refresh and presentation state for one saved stop.</summary>
public sealed class FavoriteMonitorViewModel : ObservableObject
{
    private readonly IDepartureService departures;
    private readonly Func<DepartureCacheEntry, Task>? persist;
    private CancellationTokenSource? request;
    private long revision;

    /// <summary>Creates a card with an independent service scope.</summary>
    /// <param name="stop">Complete saved identity.</param>
    /// <param name="departures">Independent departure service.</param>
    /// <param name="persist">Optional durable cache writer for accepted successful results.</param>
    public FavoriteMonitorViewModel(Stop stop, IDepartureService departures, Func<DepartureCacheEntry, Task>? persist = null)
    {
        Stop = stop; this.departures = departures; this.persist = persist;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy, _ => SetFailure());
    }

    /// <summary>Complete saved provider stop.</summary>
    public Stop Stop { get; }
    /// <summary>Card title.</summary>
    public string Title => Stop.Name;
    /// <summary>Last successful departures.</summary>
    public ProviderResult<StopEvent>? Result { get; private set; }
    /// <summary>Latest attempted provider result.</summary>
    public ProviderResult<StopEvent>? LastAttempt { get; private set; }
    /// <summary>Displayed departures in effective-time order.</summary>
    public IReadOnlyList<StopEvent> Items => Result?.Items ?? [];
    /// <summary>Provenance belonging to displayed data.</summary>
    public string Metadata
    {
        get { var displayed = Result ?? LastAttempt; return displayed is null ? "" : JourneyPresentation.Metadata(displayed); }
    }
    /// <summary>Whether this card alone is loading.</summary>
    public bool IsBusy { get; private set; }
    /// <summary>Refresh or retained-data status.</summary>
    public string Status { get; private set; } = "Noch keine Abfahrten geladen.";
    /// <summary>Calculated straight-line distance from the explicitly requested position.</summary>
    public double? DistanceMeters { get; private set; }
    /// <summary>Explicit known or unknown straight-line distance.</summary>
    public string DistanceLabel => DistanceMeters is { } distance
        ? "Luftlinie: " + distance.ToString("0", CultureInfo.GetCultureInfo("de-DE")) + " m" : "Entfernung unbekannt";
    /// <summary>Manual duplicate-protected refresh.</summary>
    public AsyncRelayCommand RefreshCommand { get; }

    /// <summary>Refreshes without discarding last known data on a failed request.</summary>
    /// <returns>Request completion.</returns>
    public Task RefreshAsync() => RefreshCoreAsync(false);

    /// <summary>Refreshes from a foreground interval without labelling it as a manual action.</summary>
    /// <returns>Update completion.</returns>
    public Task RefreshAutomaticallyAsync() => RefreshCoreAsync(true);

    /// <summary>Renews expired data without creating a concurrent request or discarding retained data.</summary>
    /// <param name="freshness">Shared realtime freshness policy.</param>
    /// <param name="cancellationToken">Page or background execution lifetime.</param>
    /// <returns>Refresh completion, or immediate completion for fresh data.</returns>
    public Task RefreshIfStaleAsync(RefreshFreshness freshness, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Notify(nameof(Metadata));
        return freshness.IsStale(Result) ? RefreshCoreAsync(true, cancellationToken) : Task.CompletedTask;
    }

    /// <summary>Restores a successful board only when it belongs to this card and still has future departures.</summary>
    /// <param name="entry">Cached board keyed by technical favorite identity.</param>
    /// <param name="now">Current instant used to reject expired departures.</param>
    /// <returns>Whether a usable local board was applied.</returns>
    internal bool Restore(DepartureCacheEntry entry, DateTimeOffset now)
    {
        if (entry.Key.Source != Stop.Source || entry.Key.Id != Stop.Id || entry.Result.ErrorCode is not null || entry.Result.IsStale) return false;
        var retained = entry.Result.Items.Where(item => EffectiveTime(item) is { } time && time >= now)
            .OrderBy(item => EffectiveTime(item)!.Value).ToArray();
        if (retained.Length == 0) return false;
        Result = entry.Result with { Items = retained };
        LastAttempt = null;
        Status = "Letzter Stand wird aktualisiert …";
        RefreshBindings();
        return true;
    }

    private async Task RefreshCoreAsync(bool automatic, CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;
        var version = ++revision;
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); request = source;
        IsBusy = true; Status = "Abfahrten werden aktualisiert …"; RefreshBindings();
        try
        {
            var started = DateTimeOffset.Now;
            var result = await departures.DeparturesAsync(Stop, started, source.Token).WaitAsync(source.Token);
            source.Token.ThrowIfCancellationRequested();
            if (version != revision) return;
            LastAttempt = result;
            if (result.ErrorCode is not null) SetFailure();
            else
            {
                Result = result with
                {
                    Items = result.Items.Where(item => EffectiveTime(item) is not { } time || time >= started)
                        .OrderBy(item => EffectiveTime(item) ?? DateTimeOffset.MaxValue).ToArray()
                };
                Status = Items.Count == 0 ? "Keine nächsten Abfahrten gefunden." : $"{Items.Count} Abfahrten · {(automatic ? "automatisch" : "manuell")} aktualisiert.";
                if (!Result.IsStale && persist is not null)
                {
                    var cached = Result with
                    {
                        Items = Result.Items.Where(item => EffectiveTime(item) is { } time && time >= started)
                            .OrderBy(item => EffectiveTime(item)!.Value).ToArray()
                    };
                    try { await persist(new DepartureCacheEntry { Source = Stop.Source, StopId = Stop.Id, Result = cached }); }
                    catch (Exception) { }
                }
            }
        }
        catch (OperationCanceledException) { if (version == revision) Status = "Aktualisierung abgebrochen."; }
        catch (Exception) { if (version == revision) SetFailure(); }
        finally { if (version == revision) { request = null; IsBusy = false; RefreshBindings(); } }
    }

    /// <summary>Cancels removed or departed cards and rejects uncooperative late responses.</summary>
    public void CancelPending()
    {
        revision++; request?.Cancel(); request = null;
        if (IsBusy) Status = "Aktualisierung abgebrochen.";
        IsBusy = false; RefreshCommand.InvalidateExecution(); RefreshBindings();
    }

    internal void SetDistance(double? distance)
    {
        DistanceMeters = distance; Notify(nameof(DistanceMeters)); Notify(nameof(DistanceLabel));
    }

    private static DateTimeOffset? EffectiveTime(StopEvent item) => item.Realtime.ActualTime
        ?? (item.PlannedTime is { } planned ? planned + (item.Realtime.Delay ?? TimeSpan.Zero) : null);

    private void SetFailure()
    {
        Status = Result is null ? "Abfahrten konnten nicht geladen werden. Bitte erneut versuchen."
            : "Aktualisierung fehlgeschlagen. Letzte bekannte Daten werden angezeigt; bitte Datenstand beachten.";
        Notify(nameof(Status));
    }

    private void RefreshBindings()
    {
        Notify(nameof(Result)); Notify(nameof(LastAttempt)); Notify(nameof(Items)); Notify(nameof(Metadata)); Notify(nameof(Status)); Notify(nameof(IsBusy));
        RefreshCommand.Refresh();
    }
}
