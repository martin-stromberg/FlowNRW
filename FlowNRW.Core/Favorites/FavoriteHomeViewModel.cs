using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;
using FlowNRW.Core.Refresh;

namespace FlowNRW.Core.Favorites;

/// <summary>Saved-stop membership, independently loaded cards and explicit distance ordering.</summary>
public sealed class FavoriteHomeViewModel : ObservableObject
{
    private readonly IFavoriteStore store;
    private readonly Func<IDepartureService> departureFactory;
    private readonly ICurrentLocationService location;
    private readonly SemaphoreSlim persistence = new(1, 1);
    private readonly List<FavoriteMonitorViewModel> savedOrder = [];
    private bool loaded;
    private CancellationTokenSource? locationRequest;
    private long locationRevision;
    private GeoCoordinate? position;

    /// <summary>Creates the home session without accessing location or providers.</summary>
    /// <param name="store">Bounded technical stop persistence.</param>
    /// <param name="departureFactory">Creates an independent service for each card.</param>
    /// <param name="location">Explicit current-position provider.</param>
    public FavoriteHomeViewModel(IFavoriteStore store, Func<IDepartureService> departureFactory, ICurrentLocationService location)
    {
        this.store = store; this.departureFactory = departureFactory; this.location = location;
        LocationCommand = new AsyncRelayCommand(UpdateDistancesAsync, () => !IsLocating, _ => SetLocationFailure("Standort konnte nicht ermittelt werden."));
    }

    /// <summary>Current cards sorted by known distance, with stable saved order for ties.</summary>
    public IReadOnlyList<FavoriteMonitorViewModel> Cards { get; private set; } = [];
    /// <summary>Load, persistence or empty-list status.</summary>
    public string Status { get; private set; } = "Favoriten werden geladen …";
    /// <summary>Whether an atomic persistence operation is pending.</summary>
    public bool IsSaving { get; private set; }
    /// <summary>Whether an explicit position request is running.</summary>
    public bool IsLocating { get; private set; }
    /// <summary>Current-position outcome without persisted coordinates.</summary>
    public string LocationStatus { get; private set; } = "Entfernungen unbekannt. Standort nur nach Aktion verwenden.";
    /// <summary>Explicitly recalculates distance ordering.</summary>
    public AsyncRelayCommand LocationCommand { get; }

    /// <summary>Loads saved cards once; failed loads remain retryable without overwriting the file.</summary>
    /// <param name="cancellationToken">Lifetime of an optional bounded background load.</param>
    /// <returns>Load completion.</returns>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await persistence.WaitAsync(cancellationToken);
        try
        {
            if (loaded) return;
            var stops = await store.LoadAsync(cancellationToken).WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var unique = stops.DistinctBy(stop => (stop.Source, stop.Id)).ToArray();
            if (unique.Length > 100 || unique.Any(stop => string.IsNullOrWhiteSpace(stop.Id) || string.IsNullOrWhiteSpace(stop.Source)))
                throw new InvalidDataException("Invalid favorite identity.");
            savedOrder.AddRange(unique.Select(stop => new FavoriteMonitorViewModel(stop, departureFactory())));
            loaded = true;
            SortCards(); SetStatus(EmptyOrCount());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception) { SetStatus("Favoriten konnten nicht geladen werden. Gespeicherte Daten bleiben unverändert; bitte erneut versuchen."); }
        finally { persistence.Release(); }
    }

    /// <summary>Starts independent initial loads without repeating completed successful card requests.</summary>
    /// <returns>All initial requests finishing or being cancelled.</returns>
    public Task RefreshMissingAsync() => Task.WhenAll(Cards.Where(card => card.Result is null && !card.IsBusy).Select(card => card.RefreshAsync()));

    /// <summary>Renews current stale cards in bounded batches without accessing position or writing favorites.</summary>
    /// <param name="freshness">Shared realtime freshness policy.</param>
    /// <param name="cancellationToken">Visible page or background execution lifetime.</param>
    /// <returns>Whether all eligible cards finished with a current successful response.</returns>
    public async Task<bool> RefreshStaleAsync(RefreshFreshness freshness, CancellationToken cancellationToken = default)
    {
        await LoadAsync(cancellationToken);
        if (!loaded) return false;
        var successful = true;
        foreach (var batch in Cards.ToArray().Chunk(4))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = batch.Where(Contains).ToArray();
            await Task.WhenAll(current.Select(card => card.RefreshIfStaleAsync(freshness, cancellationToken)));
            cancellationToken.ThrowIfCancellationRequested();
            successful &= current.Where(Contains).All(card => !card.IsBusy && card.LastAttempt?.ErrorCode is null && !freshness.IsStale(card.Result));
        }
        return successful;
    }

    /// <summary>Checks the complete technical namespace and provider identifier.</summary>
    /// <param name="stop">Identity to check.</param>
    /// <returns>Whether it is durably saved in the current session.</returns>
    public bool IsFavorite(Stop stop) => savedOrder.Any(card => SameIdentity(card.Stop, stop));

    /// <summary>Validates an exact current card instance for navigation.</summary>
    /// <param name="card">Candidate from a home or map snapshot.</param>
    /// <returns>Whether the card is still a current saved favorite.</returns>
    public bool Contains(FavoriteMonitorViewModel card) => savedOrder.Any(current => ReferenceEquals(current, card));

    /// <summary>Adds or removes one identity, exposing success only after persistence completes.</summary>
    /// <param name="stop">Full provider identity from the active monitor.</param>
    /// <returns>Save completion, with failures represented in Status.</returns>
    public async Task ToggleAsync(Stop stop)
    {
        if (IsSaving) return;
        await LoadAsync();
        if (!loaded) return;
        await MutateAsync(stop, null);
    }

    /// <summary>Removes a current card only after successful persistence.</summary>
    /// <param name="card">Exact current favorite card.</param>
    /// <returns>Save completion, with failures represented in Status.</returns>
    public Task RemoveAsync(FavoriteMonitorViewModel card) => Contains(card) ? MutateAsync(card.Stop, card) : Task.CompletedTask;

    /// <summary>Cancels page-scoped provider and location work while keeping completed favorites.</summary>
    public void CancelPending()
    {
        locationRevision++; locationRequest?.Cancel(); locationRequest = null;
        if (IsLocating) LocationStatus = "Standortabfrage abgebrochen.";
        IsLocating = false; LocationCommand.InvalidateExecution();
        Notify(nameof(IsLocating)); Notify(nameof(LocationStatus));
        foreach (var card in savedOrder) card.CancelPending();
    }

    private async Task MutateAsync(Stop stop, FavoriteMonitorViewModel? removing)
    {
        if (IsSaving) return;
        IsSaving = true; Notify(nameof(IsSaving));
        await persistence.WaitAsync();
        try
        {
            if (removing is not null && !Contains(removing)) return;
            var existing = savedOrder.FirstOrDefault(card => SameIdentity(card.Stop, stop));
            var desired = savedOrder.Where(card => !ReferenceEquals(card, existing)).Select(card => card.Stop).ToList();
            if (existing is null)
            {
                if (string.IsNullOrWhiteSpace(stop.Id) || string.IsNullOrWhiteSpace(stop.Source))
                { SetStatus("Diese Haltestelle hat keine vollständige technische Identität."); return; }
                if (desired.Count >= 100) { SetStatus("Maximal 100 Favoriten möglich. Bitte zuerst einen Favoriten entfernen."); return; }
                desired.Add(stop);
            }
            await store.SaveAsync(desired);
            if (existing is not null) { existing.CancelPending(); savedOrder.Remove(existing); }
            else savedOrder.Add(new FavoriteMonitorViewModel(stop, departureFactory()));
            SortCards();
            SetStatus((existing is null ? "Favorit gespeichert. " : "Favorit entfernt. ") + EmptyOrCount());
        }
        catch (Exception) { SetStatus("Favoriten konnten nicht gespeichert werden. Bisherige Favoriten bleiben unverändert; bitte erneut versuchen."); }
        finally { persistence.Release(); IsSaving = false; Notify(nameof(IsSaving)); }
    }

    private async Task UpdateDistancesAsync()
    {
        locationRevision++;
        var version = locationRevision;
        using var source = new CancellationTokenSource(); locationRequest = source;
        IsLocating = true; LocationStatus = "Standort wird ermittelt …";
        Notify(nameof(IsLocating)); Notify(nameof(LocationStatus)); LocationCommand.Refresh();
        try
        {
            var result = await location.GetCurrentAsync(source.Token);
            if (version != locationRevision) return;
            if (!result.HasCurrentPosition) { SetLocationFailure(result.FailureDescription); return; }
            position = result.Coordinate;
            SortCards();
            LocationStatus = "Nach Luftlinie sortiert. Unbekannte Entfernungen stehen am Ende." + result.AccuracyDescription;
        }
        catch (OperationCanceledException) { if (version == locationRevision) SetLocationFailure("Standortabfrage abgebrochen."); }
        catch (Exception) { if (version == locationRevision) SetLocationFailure("Standort konnte nicht ermittelt werden. Bitte erneut versuchen."); }
        finally
        {
            if (version == locationRevision)
            {
                locationRequest = null; IsLocating = false;
                Notify(nameof(IsLocating)); Notify(nameof(LocationStatus)); LocationCommand.Refresh();
            }
        }
    }

    private void SetLocationFailure(string message)
    {
        position = null; SortCards();
        LocationStatus = message + " Entfernungen unbekannt; gespeicherte Reihenfolge wird angezeigt.";
        Notify(nameof(LocationStatus));
    }

    private void SortCards()
    {
        foreach (var card in savedOrder) card.SetDistance(position is not null && card.Stop.Coordinate is { } target ? Distance(position, target) : null);
        Cards = savedOrder.OrderBy(card => card.DistanceMeters ?? double.PositiveInfinity).ToArray();
        Notify(nameof(Cards));
    }

    private static double Distance(GeoCoordinate origin, GeoCoordinate destination)
    {
        const double radians = Math.PI / 180;
        var latitude = (destination.Latitude - origin.Latitude) * radians;
        var longitude = (destination.Longitude - origin.Longitude) * radians;
        var value = Math.Pow(Math.Sin(latitude / 2), 2) + Math.Cos(origin.Latitude * radians) * Math.Cos(destination.Latitude * radians) * Math.Pow(Math.Sin(longitude / 2), 2);
        return 6371000 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(value, 0, 1)));
    }

    private static bool SameIdentity(Stop first, Stop second) => first.Source == second.Source && first.Id == second.Id;
    private string EmptyOrCount() => savedOrder.Count == 0 ? "Noch keine Favoriten. Über Haltestellen suchen eine Station auswählen und als Favorit speichern." : $"{savedOrder.Count} Favoriten.";
    private void SetStatus(string value) { Status = value; Notify(nameof(Status)); }
}
