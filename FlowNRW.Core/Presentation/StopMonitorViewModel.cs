using FlowNRW.Core.Transit;
using FlowNRW.Core.Favorites;
using FlowNRW.Core.Refresh;

namespace FlowNRW.Core.Presentation;

/// <summary>Manual stop lookup and departure monitor with retained data on refresh errors.</summary>
public sealed class StopMonitorViewModel : ObservableObject
{
    private readonly IDepartureService departures;
    private readonly IDepartureNavigation navigation;
    private readonly IStopSearchService nearby;
    private readonly ICurrentLocationService? location;
    private readonly IFavoriteStore? favoriteStore;
    private readonly IDepartureCacheStore? departureCache;
    private readonly RefreshFreshness freshness;
    private CancellationTokenSource? request;
    private long revision;
    private bool opening;
    private CancellationTokenSource? nearbyRequest;
    private long nearbyRevision;
    private bool nearbyActive;
    private readonly Dictionary<(string Source, string Id), SessionBoard> sessionBoards = [];
    private long sessionOrder;
    private const int MaximumSessionBoards = 100;

    /// <summary>Creates an independent stop monitor session.</summary>
    /// <param name="search">Independent lookup service.</param>
    /// <param name="departures">Departure service.</param>
    /// <param name="navigation">Monitor navigation.</param>
    /// <param name="maxSearchLength">Configured search length.</param>
    /// <param name="location">Optional current-location provider.</param>
    /// <param name="nearbySearch">Independent nearby lookup scope.</param>
    /// <param name="favoriteStore">Saved favorites used to authorize persistent cache reads.</param>
    /// <param name="departureCache">Persistent cache containing saved favorite boards.</param>
    /// <param name="freshness">Freshness policy applied before a persistent cache board is displayed.</param>
    public StopMonitorViewModel(IStopSearchService search, IDepartureService departures, IDepartureNavigation navigation, int maxSearchLength, ICurrentLocationService? location = null, IStopSearchService? nearbySearch = null,
        IFavoriteStore? favoriteStore = null, IDepartureCacheStore? departureCache = null, RefreshFreshness? freshness = null)
    {
        Lookup = new EndpointViewModel(search, maxSearchLength);
        this.departures = departures;
        this.navigation = navigation;
        this.location = location;
        this.favoriteStore = favoriteStore;
        this.departureCache = departureCache;
        this.freshness = freshness ?? new RefreshFreshness(new TransitCacheOptions());
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
    /// <summary>Whether the latest explicitly requested nearby lookup needs user attention.</summary>
    public bool IsNearbyFailure { get; private set; }
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
        IsNearbyBusy = true; IsNearbyFailure = false; NearbyStatus = "Standort wird ermittelt …"; RefreshSearchBindings();
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
            IsNearbyFailure = false;
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
        IsNearbyFailure = false; NearbyStatus = "Standort nur nach Aktion verwenden.";
        RefreshSearchBindings();
    }

    private void SetNearbyFailure(string description)
    {
        IsNearbyFailure = true;
        NearbyStatus = description + (Stops.Count > 0 ? " Vorherige Ergebnisse werden angezeigt; keine neue Umgebung ermittelt." : "");
        RefreshSearchBindings();
    }

    private void RefreshSearchBindings()
    {
        Notify(nameof(Stops)); Notify(nameof(SearchStatus)); Notify(nameof(SearchMetadata));
        Notify(nameof(NearbyStatus)); Notify(nameof(NearbyResult)); Notify(nameof(NearbyStops)); Notify(nameof(IsNearbyBusy)); Notify(nameof(IsNearbyFailure));
        NearbyCommand?.Refresh();
    }

    /// <summary>Opens an actual candidate and loads its departures.</summary>
    /// <param name="candidate">Complete selected lookup record.</param>
    /// <returns>Navigation and initial update completion.</returns>
    public async Task OpenAsync(Address candidate)
    {
        if (opening || !Stops.Any(item => ReferenceEquals(item, candidate)) || candidate.Stop is null) return;
        if (!nearbyActive) Lookup.SelectAddressKeepingMatches(candidate);
        await OpenStopAsync(candidate.Stop);
    }

    /// <summary>Opens a complete stop identity supplied by the retained home nearby list.</summary>
    /// <param name="candidate">Nearby stop independently verified by the home projection.</param>
    /// <returns>Navigation and initial update completion, or immediate completion for an incomplete identity.</returns>
    public Task OpenNearbyFromHomeAsync(Address candidate)
    {
        if (candidate.Stop is not { } stop
            || string.IsNullOrWhiteSpace(stop.Id)
            || string.IsNullOrWhiteSpace(stop.Source)) return Task.CompletedTask;
        return OpenStopAsync(stop);
    }

    /// <summary>Opens an exact currently saved favorite without weakening search membership.</summary>
    /// <param name="home">Authoritative favorite session.</param>
    /// <param name="card">Current saved card instance.</param>
    /// <returns>Navigation and first departure refresh completion.</returns>
    public Task OpenFavoriteAsync(FavoriteHomeViewModel home, FavoriteMonitorViewModel card) => home.Contains(card)
        ? OpenStopAsync(card.Stop, card.Result) : Task.CompletedTask;

    private async Task OpenStopAsync(Stop stop, ProviderResult<StopEvent>? retained = null)
    {
        if (opening) return;
        opening = true;
        CancelNearbyPending();
        CancelPending();
        SelectedStop = stop;
        var key = (stop.Source, stop.Id);
        var cached = retained ?? ReadSession(key);
        var version = revision;
        if (cached is null) cached = await ReadPersistentFavoriteCacheAsync(stop, version);
        if (version != revision || SelectedStop != stop) { opening = false; return; }
        Result = cached is null ? null : cached with
        {
            Items = cached.Items.Where(item => EffectiveTime(item) is { } time && time >= DateTimeOffset.Now)
                .OrderBy(item => EffectiveTime(item)!.Value).ToArray()
        };
        LastAttempt = null;
        SetStatus("Abfahrten werden geladen …");
        RefreshBindings();
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
    public Task RefreshAsync() => RefreshCoreAsync(false);

    /// <summary>Refreshes from a foreground interval while retaining the existing busy and revision guards.</summary>
    /// <returns>Automatic update completion.</returns>
    public Task RefreshAutomaticallyAsync() => RefreshCoreAsync(true);

    /// <summary>Renews expired retained data through the existing duplicate-protected refresh path.</summary>
    /// <param name="freshness">Shared realtime freshness policy.</param>
    /// <param name="cancellationToken">Visible page lifetime.</param>
    /// <returns>Refresh completion, or immediate completion for fresh data.</returns>
    public Task RefreshIfStaleAsync(RefreshFreshness freshness, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Notify(nameof(Metadata));
        return freshness.IsStale(Result) ? RefreshCoreAsync(true, cancellationToken) : Task.CompletedTask;
    }

    private async Task RefreshCoreAsync(bool automatic, CancellationToken cancellationToken = default)
    {
        if (SelectedStop is null || IsBusy) return;
        var version = ++revision;
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        request = source;
        IsBusy = true;
        SetStatus("Abfahrten werden aktualisiert …");
        RefreshBindings();
        try
        {
            var started = DateTimeOffset.Now;
            var result = await departures.DeparturesAsync(SelectedStop, started, source.Token).WaitAsync(source.Token);
            source.Token.ThrowIfCancellationRequested();
            if (version != revision) return;
            LastAttempt = result;
            Diagnostics.AppLog.Write("monitor", $"{SelectedStop.Name}: items={result.Items.Count} error={result.ErrorCode ?? "-"} fallback={result.IsFallback} stale={result.IsStale} warnings={result.Warnings.Count}");
            if (result.ErrorCode is not null || (!IsComplete(result) && Result is not null))
                SetFailure();
            else
            {
                Result = result with
                {
                    Items = result.Items.Where(item => EffectiveTime(item) is not { } time || time >= started)
                        .OrderBy(item => EffectiveTime(item) ?? DateTimeOffset.MaxValue).ToArray()
                };
                StoreSession(SelectedStop, Result);
                SetStatus(result.Warnings.Count > 0 || result.IsFallback || result.IsStale
                    ? "Daten möglicherweise unvollständig oder veraltet."
                    : Items.Count == 0 ? "Keine nächsten Abfahrten gefunden." : $"{Items.Count} Abfahrten · {(automatic ? "automatisch" : "manuell")} aktualisiert.");
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

    private ProviderResult<StopEvent>? ReadSession((string Source, string Id) key)
    {
        if (!sessionBoards.TryGetValue(key, out var cached)) return null;
        sessionBoards[key] = cached with { Order = ++sessionOrder };
        return cached.Result;
    }

    private void StoreSession(Stop stop, ProviderResult<StopEvent> result)
    {
        if (string.IsNullOrWhiteSpace(stop.Source) || string.IsNullOrWhiteSpace(stop.Id)
            || result.Items.Count > 100 || result.ErrorCode is not null) return;
        var key = (stop.Source, stop.Id);
        sessionBoards[key] = new SessionBoard(result, ++sessionOrder);
        while (sessionBoards.Count > MaximumSessionBoards)
        {
            var oldest = sessionBoards.Where(entry => entry.Key != key).OrderBy(entry => entry.Value.Order).FirstOrDefault();
            if (oldest.Key == default) break;
            sessionBoards.Remove(oldest.Key);
        }
    }

    private sealed record SessionBoard(ProviderResult<StopEvent> Result, long Order);

    private async Task<ProviderResult<StopEvent>?> ReadPersistentFavoriteCacheAsync(Stop stop, long version)
    {
        if (favoriteStore is null || departureCache is null) return null;
        try
        {
            var favorites = await favoriteStore.LoadAsync();
            if (version != revision || !favorites.Any(item => item.Source == stop.Source && item.Id == stop.Id)) return null;
            var entry = (await departureCache.LoadAsync()).FirstOrDefault(item => item.Source == stop.Source && item.StopId == stop.Id);
            return version == revision && entry is not null && !freshness.IsStale(entry.Result) ? entry.Result : null;
        }
        catch (OperationCanceledException) { return null; }
        catch (Exception) { return null; }
    }

    private static bool IsComplete(ProviderResult<StopEvent> result) => result.ErrorCode is null && result.Warnings.Count == 0 && !result.IsFallback && !result.IsStale;

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
