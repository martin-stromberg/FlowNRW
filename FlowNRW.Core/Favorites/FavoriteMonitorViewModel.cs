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
    public string Title
    {
        get { return JourneyPresentation.DisplayName(Stop.Name); }
    }
    /// <summary>Last successful departures.</summary>
    public ProviderResult<StopEvent>? Result { get; private set; }
    /// <summary>Latest attempted provider result.</summary>
    public ProviderResult<StopEvent>? LastAttempt { get; private set; }
    /// <summary>Displayed departures in effective-time order.</summary>
    public IReadOnlyList<StopEvent> Items => Result?.Items ?? [];
    /// <summary>Last complete line inventory merged with the delivered lines of displayed degraded results, including lines without a currently known departure time.</summary>
    public IReadOnlyList<string> Lines { get; private set; } = [];
    /// <summary>Provenance belonging to displayed data.</summary>
    public string Metadata
    {
        get { var displayed = Result ?? LastAttempt; return displayed is null ? "" : JourneyPresentation.Metadata(displayed); }
    }
    /// <summary>Whether the displayed result carries a data-quality notice worth an unobtrusive indicator.</summary>
    public bool HasDataNotice
    {
        get { return Result.HasDataQualityNotice(); }
    }
    /// <summary>Whether this card alone is loading.</summary>
    public bool IsBusy { get; private set; }
    /// <summary>Whether the departure rows are expanded on the home page.</summary>
    public bool IsExpanded { get; private set; }
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

    /// <summary>Applies an already projected persistent board, keeping its line inventory even without departures.</summary>
    /// <param name="board">Filtered and stale-marked board distributed by technical favorite identity.</param>
    /// <returns>Whether a usable local board was applied.</returns>
    internal bool Restore(RestoredDepartureBoard board)
    {
        Lines = board.Lines;
        if (board.Result is null) { RefreshBindings(); return false; }
        Result = board.Result;
        LastAttempt = null;
        Status = "Letzter Stand wird aktualisiert …";
        RefreshBindings();
        return true;
    }

    /// <summary>Toggles the compact home card without changing its persisted data.</summary>
    public void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
        Notify(nameof(IsExpanded));
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
            Diagnostics.AppLog.Write("favorite", $"{Stop.Name}: items={result.Items.Count} error={result.ErrorCode ?? "-"} fallback={result.IsFallback} stale={result.IsStale} warnings={result.Warnings.Count}");
            if (result.ErrorCode is not null || (!IsComplete(result) && Result is not null && IsComplete(Result))) SetFailure();
            else
            {
                Result = result with
                {
                    Items = result.Items.Where(item => DepartureBoardCache.EffectiveTime(item) is { } time && time >= started)
                        .OrderBy(item => DepartureBoardCache.EffectiveTime(item)!.Value).ToArray()
                };
                Status = result.Warnings.Count > 0 || result.IsFallback || result.IsStale
                    ? "Daten möglicherweise unvollständig oder veraltet."
                    : Items.Count == 0 ? "Keine nächsten Abfahrten gefunden." : $"{Items.Count} Abfahrten · {(automatic ? "automatisch" : "manuell")} aktualisiert.";
                if (IsComplete(result))
                {
                    Lines = DepartureBoardCache.SortLines(result.Items.Select(DepartureBoardCache.LineName));
                    Notify(nameof(Lines));
                    var cached = Result with
                    {
                        Items = Result.Items.Where(item => DepartureBoardCache.EffectiveTime(item) is { } time && time >= started)
                            .OrderBy(item => DepartureBoardCache.EffectiveTime(item)!.Value).ToArray()
                    };
                    try { if (persist is not null) await persist(new DepartureCacheEntry { Source = Stop.Source, StopId = Stop.Id, Result = cached, Lines = Lines }); }
                    catch (Exception) { }
                }
                else EnrichLines(result);
            }
        }
        catch (OperationCanceledException) { if (version == revision) Status = "Aktualisierung abgebrochen."; }
        catch (Exception) { if (version == revision) SetFailure(); }
        finally { if (version == revision) { request = null; IsBusy = false; RefreshBindings(); } }
    }

    private static bool IsComplete(ProviderResult<StopEvent> result) => result.ErrorCode is null && result.Warnings.Count == 0 && !result.IsFallback && !result.IsStale;

    private void EnrichLines(ProviderResult<StopEvent> result)
    {
        Lines = DepartureBoardCache.SortLines(result.Items.Select(DepartureBoardCache.LineName).Concat(Lines));
        Notify(nameof(Lines));
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

    private void SetFailure()
    {
        Status = Result is null ? "Abfahrten konnten nicht geladen werden. Bitte erneut versuchen."
            : "Aktualisierung fehlgeschlagen. Letzte bekannte Daten werden angezeigt; bitte Datenstand beachten.";
        Notify(nameof(Status));
    }

    private void RefreshBindings()
    {
        Notify(nameof(Result)); Notify(nameof(LastAttempt)); Notify(nameof(Items)); Notify(nameof(Lines)); Notify(nameof(Metadata)); Notify(nameof(HasDataNotice)); Notify(nameof(Status)); Notify(nameof(IsBusy)); Notify(nameof(IsExpanded));
        RefreshCommand.Refresh();
    }
}
