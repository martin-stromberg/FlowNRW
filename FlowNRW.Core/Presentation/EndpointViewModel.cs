using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>One independently cancellable endpoint lookup.</summary>
public sealed class EndpointViewModel : ObservableObject
{
    private readonly IStopSearchService service;
    private readonly int maxLength;
    private readonly ICurrentLocationService? location;
    private CancellationTokenSource? request;
    private long revision;
    private string text = "", latitude = "", longitude = "";
    private bool coordinates;
    private bool locating;
    /// <summary>Creates an endpoint with its own search service.</summary>
    /// <param name="service">Independent search scope.</param>
    /// <param name="maxLength">Configured search limit.</param>
    /// <param name="location">Optional current-location provider.</param>
    public EndpointViewModel(IStopSearchService service, int maxLength, ICurrentLocationService? location = null)
    {
        this.service = service; this.maxLength = maxLength; this.location = location;
        SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy, _ => SetStatus("Suche fehlgeschlagen. Bitte erneut versuchen."));
        SelectCommand = new RelayCommand(value => { if (value is Address address) SelectAddress(address); });
        LocationCommand = new AsyncRelayCommand(UseCurrentLocationAsync, () => !IsBusy && location is not null,
            _ => SetLocationStatus("Standort konnte nicht ermittelt werden."));
    }
    /// <summary>Reports changed endpoint identity.</summary>
    public event EventHandler? Changed;
    /// <summary>Address or stop query.</summary>
    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;
            text = value;
            Invalidate();
            Notify();
        }
    }
    /// <summary>Latitude input.</summary>
    public string Latitude
    {
        get => latitude;
        set
        {
            if (latitude == value) return;
            latitude = value;
            Invalidate();
            Notify();
        }
    }
    /// <summary>Longitude input.</summary>
    public string Longitude
    {
        get => longitude;
        set
        {
            if (longitude == value) return;
            longitude = value;
            Invalidate();
            Notify();
        }
    }
    /// <summary>Whether coordinate input is active.</summary>
    public bool IsCoordinateMode
    {
        get => coordinates;
        set
        {
            if (coordinates == value) return;
            coordinates = value;
            Invalidate();
            Notify();
            Notify(nameof(IsTextMode));
        }
    }
    /// <summary>Whether lookup input is active.</summary>
    public bool IsTextMode => !IsCoordinateMode;
    /// <summary>Complete selected identity.</summary>
    public Address? SelectedAddress { get; private set; }
    /// <summary>Readable selected identity.</summary>
    public string Selection
    {
        get
        {
            return SelectedAddress is null ? "Noch kein Endpunkt ausgewählt." : "Ausgewählt: " + JourneyPresentation.Address(SelectedAddress);
        }
    }
    /// <summary>Current candidates in provider order.</summary>
    public IReadOnlyList<Address> Matches { get; private set; } = [];
    /// <summary>Current provenance.</summary>
    public ProviderResult<Address>? Result { get; private set; }
    /// <summary>Readable provenance.</summary>
    public string Metadata
    {
        get
        {
            return Result is null ? "" : JourneyPresentation.Metadata(Result);
        }
    }
    /// <summary>Activity state.</summary>
    public bool IsBusy { get; private set; }
    /// <summary>Validation or provider status.</summary>
    public string Status { get; private set; } = "Adresse oder Haltestelle suchen und einen Treffer auswählen.";
    /// <summary>Lookup or coordinate validation action.</summary>
    public AsyncRelayCommand SearchCommand { get; }
    /// <summary>Candidate selection action.</summary>
    public RelayCommand SelectCommand { get; }
    /// <summary>Accepts a saved favorite without exposing its technical identity in the field.</summary>
    /// <param name="stop">Favorite stop including the routing identity and coordinate.</param>
    public void SelectFavorite(Stop stop)
    {
        SelectAddress(new Address
        {
            Name = stop.Name,
            Stop = stop,
            Coordinate = stop.Coordinate
        });
    }
    /// <summary>Explicitly requests the current position.</summary>
    public AsyncRelayCommand LocationCommand { get; }
    /// <summary>Readable location request state.</summary>
    public string LocationStatus { get; private set; } = "Standort nur nach Aktion verwenden.";
    /// <summary>Resolves the current input.</summary>
    /// <returns>Lookup completion.</returns>
    public async Task SearchAsync()
    {
        Invalidate();
        if (IsCoordinateMode)
        {
            if (CoordinateParser.TryParse(Latitude, Longitude, out var coordinate, out var error))
                SelectAddress(new Address { Name = "Koordinate", Coordinate = coordinate });
            else SetStatus(error);
            return;
        }
        if (string.IsNullOrWhiteSpace(Text) || Text.Trim().Length > maxLength) { SetStatus($"Bitte einen Suchtext mit 1 bis {maxLength} Zeichen eingeben."); return; }
        var version = revision;
        using var source = new CancellationTokenSource(); request = source;
        SetBusy(true); SetStatus("Treffer werden geladen …");
        try
        {
            var result = await service.SearchAsync(Text.Trim(), source.Token);
            if (version != revision) return;
            Result = result; Matches = result.ErrorCode is null ? result.Items : [];
            Notify(nameof(Result)); Notify(nameof(Matches)); Notify(nameof(Metadata));
            SetStatus(result.ErrorCode is not null ? "Suche beim Anbieter fehlgeschlagen. Bitte erneut versuchen." : Matches.Count == 0 ? "Keine Treffer. Bitte Eingabe ändern." : "Bitte einen Treffer auswählen.");
        }
        catch (OperationCanceledException) { if (version == revision) SetStatus("Suche abgebrochen."); }
        catch (Exception) { if (version == revision) SetStatus("Suche beim Anbieter fehlgeschlagen. Bitte erneut versuchen."); }
        finally { if (version == revision) { request = null; SetBusy(false); } }
    }
    /// <summary>Accepts a complete provider candidate.</summary>
    /// <param name="address">Selected candidate.</param>
    public void SelectAddress(Address address)
    {
        if (!IsCoordinateMode && !Matches.Contains(address) && address.Stop is null) return;
        SetSelectedAddress(address, true);
    }

    /// <summary>Accepts an address while retaining the current candidate list for return navigation.</summary>
    /// <param name="address">Resolved stop or coordinate.</param>
    public void SelectAddressKeepingMatches(Address address)
    {
        if (!IsCoordinateMode && !Matches.Contains(address) && address.Stop is null) return;
        SetSelectedAddress(address, false);
    }

    /// <summary>Sets an already resolved endpoint without starting a provider lookup.</summary>
    /// <param name="address">Resolved endpoint or <see langword="null"/> to clear it.</param>
    public void SetSelectedAddress(Address? address)
    {
        CancelPending();
        SelectedAddress = address;
        text = address?.Name ?? "";
        Notify(nameof(Text)); Notify(nameof(SelectedAddress)); Notify(nameof(Selection));
        Matches = []; Result = null;
        Notify(nameof(Matches)); Notify(nameof(Result)); Notify(nameof(Metadata));
        SetStatus(address is null ? "Adresse oder Haltestelle suchen und einen Treffer auswählen." : "Endpunkt übernommen.");
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SetSelectedAddress(Address address, bool clearMatches)
    {
        CancelPending();
        text = address.Name;
        Notify(nameof(Text));
        SelectedAddress = address; Notify(nameof(SelectedAddress)); Notify(nameof(Selection));
        if (clearMatches)
        {
            Matches = []; Result = null;
            Notify(nameof(Matches)); Notify(nameof(Result)); Notify(nameof(Metadata));
        }
        SetStatus("Endpunkt übernommen."); Changed?.Invoke(this, EventArgs.Empty);
    }

    private async Task UseCurrentLocationAsync()
    {
        if (location is null) return;
        CancelPending();
        var version = revision;
        using var source = new CancellationTokenSource(); request = source;
        locating = true;
        SetBusy(true); SetLocationStatus("Standort wird ermittelt …");
        try
        {
            var result = await location.GetCurrentAsync(source.Token);
            if (version != revision) return;
            if (result.HasCurrentPosition)
            {
                SelectedAddress = new Address { Name = "Aktueller Standort", Coordinate = result.Coordinate };
                Matches = []; Result = null;
                Notify(nameof(Matches)); Notify(nameof(Result)); Notify(nameof(Metadata));
                Notify(nameof(SelectedAddress)); Notify(nameof(Selection));
                SetStatus("Endpunkt übernommen.");
                SetLocationStatus("Aktueller Standort übernommen." + result.AccuracyDescription);
                Changed?.Invoke(this, EventArgs.Empty);
            }
            else SetLocationStatus(result.FailureDescription);
        }
        catch (OperationCanceledException) { if (version == revision) SetLocationStatus("Standortabfrage abgebrochen."); }
        catch (Exception) { if (version == revision) SetLocationStatus("Standort konnte nicht ermittelt werden."); }
        finally { if (version == revision) { request = null; locating = false; SetBusy(false); } }
    }
    /// <summary>Cancels pending work while retaining completed state.</summary>
    public void CancelPending()
    {
        revision++;
        request?.Cancel();
        request = null;
        if (IsBusy) { if (locating) SetLocationStatus("Standortabfrage abgebrochen."); else SetStatus("Suche abgebrochen."); }
        locating = false;
        SetBusy(false);
        SearchCommand.InvalidateExecution();
        LocationCommand.InvalidateExecution();
    }
    private void Invalidate()
    {
        CancelPending(); SelectedAddress = null; Matches = []; Result = null;
        Notify(nameof(SelectedAddress)); Notify(nameof(Selection)); Notify(nameof(Matches)); Notify(nameof(Result)); Notify(nameof(Metadata));
        SetStatus(IsCoordinateMode ? "Breite und Länge eingeben und übernehmen." : "Adresse oder Haltestelle suchen und einen Treffer auswählen.");
        Changed?.Invoke(this, EventArgs.Empty);
    }
    private void SetLocationStatus(string value) { LocationStatus = value; Notify(nameof(LocationStatus)); }
    private void SetStatus(string value) { Status = value; Notify(nameof(Status)); }
    private void SetBusy(bool value) { IsBusy = value; Notify(nameof(IsBusy)); SearchCommand.Refresh(); LocationCommand.Refresh(); }
}
