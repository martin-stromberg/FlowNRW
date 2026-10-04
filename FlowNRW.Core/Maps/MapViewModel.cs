using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;
using FlowNRW.Core.Favorites;

namespace FlowNRW.Core.Maps;

/// <summary>A provider geometry segment, never joined across missing legs.</summary>
/// <param name="Label">Accessible line or walking description.</param>
/// <param name="Points">Original ordered WGS84 points.</param>
/// <returns>Immutable segment projection.</returns>
public sealed record MapSegment(string Label, IReadOnlyList<GeoCoordinate> Points);

/// <summary>A selectable original search candidate with an optional map position.</summary>
/// <param name="Index">Session-local selection index.</param>
/// <param name="Candidate">Complete original candidate.</param>
/// <returns>Immutable station projection.</returns>
public sealed record MapStation(int Index, Address Candidate)
{
    /// <summary>Actual provider position, absent outside the renderer's projection.</summary>
    public GeoCoordinate? Position
    {
        get { return MapViewModel.IsMappable(Candidate.Stop?.Coordinate ?? Candidate.Coordinate) ? Candidate.Stop?.Coordinate ?? Candidate.Coordinate : null; }
    }
    /// <summary>Readable station label and missing-position explanation.</summary>
    public string Label
    {
        get { return Candidate.Name + (Position is null ? " · Keine Kartenposition vorhanden" : ""); }
    }
}

/// <summary>Immutable map snapshots with revision-scoped station selection.</summary>
public sealed class MapViewModel
{
    private readonly StopMonitorViewModel monitor;
    private Func<int, Task>? favoriteSelection;
    /// <summary>Creates the map session.</summary>
    /// <param name="monitor">Existing stop lookup and monitor session.</param>
    public MapViewModel(StopMonitorViewModel monitor) { this.monitor = monitor; Reset(); }
    /// <summary>Opaque identity rejects callbacks from earlier maps.</summary>
    public string Session { get; private set; } = "";
    /// <summary>Map page title.</summary>
    public string Title { get; private set; } = "Haltestellenkarte";
    /// <summary>Accessible provider provenance.</summary>
    public string Metadata { get; private set; } = "";
    /// <summary>Missing/partial geometry explanation.</summary>
    public string Status { get; private set; } = "";
    /// <summary>Selectable search results, including stops without positions.</summary>
    public IReadOnlyList<MapStation> Stations { get; private set; } = [];
    /// <summary>Provider-supplied separate route segments.</summary>
    public IReadOnlyList<MapSegment> Segments { get; private set; } = [];
    /// <summary>Non-selectable journey endpoints for geographic orientation.</summary>
    public IReadOnlyList<GeoCoordinate> Endpoints { get; private set; } = [];

    /// <summary>Captures the current stop candidates without changing their identity.</summary>
    public void ShowStops()
    {
        Reset();
        Title = "Haltestellenkarte";
        Stations = monitor.Stops.Select((candidate, index) => new MapStation(index, candidate)).ToArray();
        Metadata = monitor.SearchMetadata;
        Status = Stations.Count == 0 ? "Keine Haltestellen vorhanden. Bitte zuerst suchen."
            : $"{Stations.Count} Haltestellen · {Stations.Count(item => item.Position is not null)} Kartenpositionen. Auswahl öffnet die Abfahrten.";
    }

    /// <summary>Captures current favorite cards while revalidating membership on selection.</summary>
    /// <param name="home">Authoritative favorite session.</param>
    public void ShowFavorites(FavoriteHomeViewModel home)
    {
        Reset();
        var cards = home.Cards.ToArray();
        Title = "Favoritenkarte";
        Stations = cards.Select((card, index) => new MapStation(index, new Address { Name = card.Stop.Name, Stop = card.Stop, Coordinate = card.Stop.Coordinate })).ToArray();
        Metadata = "Gespeicherte technische Haltestellen. Abfahrten werden nach Auswahl geladen.";
        Status = Stations.Count == 0 ? "Keine Favoriten vorhanden. Bitte zuerst eine Haltestelle speichern."
            : $"{Stations.Count} Favoriten · {Stations.Count(item => item.Position is not null)} Kartenpositionen. Auswahl öffnet die Abfahrten.";
        favoriteSelection = index => monitor.OpenFavoriteAsync(home, cards[index]);
    }

    /// <summary>Captures one selected journey without fabricating missing segments.</summary>
    /// <param name="journey">Selected journey.</param>
    /// <param name="metadata">Provenance of its source result.</param>
    public void ShowJourney(Journey journey, string metadata)
    {
        Reset();
        Title = "Verbindungsverlauf";
        Metadata = metadata;
        var segments = new List<MapSegment>();
        var missing = 0;
        foreach (var leg in journey.Legs)
        {
            var points = leg.Geometry?.Coordinates ?? leg.Walking?.Geometry?.Coordinates ?? [];
            var label = leg.Walking is not null ? "Fußweg" : leg.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt";
            var chunk = new List<GeoCoordinate>();
            var complete = points.Count >= 2;
            foreach (var point in points)
            {
                if (IsMappable(point)) chunk.Add(point);
                else { if (chunk.Count >= 2) segments.Add(new(label, chunk.ToArray())); chunk.Clear(); complete = false; }
            }
            if (chunk.Count >= 2) segments.Add(new(label, chunk.ToArray()));
            if (!complete) missing++;
        }
        Segments = segments;
        Endpoints = journey.Legs.SelectMany(leg => new[] { leg.Departure.Identity.Stop.Coordinate, leg.Arrival.Identity.Stop.Coordinate })
            .Where(IsMappable).OfType<GeoCoordinate>().ToArray();
        Status = segments.Count == 0 ? "Keine darstellbare Geometrie geliefert. Es wird kein Streckenverlauf erfunden."
            : missing > 0 ? $"Teilweiser Verlauf: Für {missing} Teilstrecken fehlen vollständige Kartenpunkte."
            : "Gelieferter Verbindungsverlauf · Linien und Fußwege getrennt dargestellt.";
    }

    /// <summary>Opens only a current original candidate through the monitor's identity validation.</summary>
    /// <param name="session">Expected session identity.</param>
    /// <param name="index">Local marker/list index.</param>
    /// <returns>Navigation completion.</returns>
    public Task SelectAsync(string session, int index) => session == Session && index >= 0 && index < Stations.Count
        ? favoriteSelection is null ? monitor.OpenAsync(Stations[index].Candidate) : favoriteSelection(index) : Task.CompletedTask;

    /// <summary>Tests the supported Web Mercator latitude range.</summary>
    /// <param name="coordinate">Optional validated WGS84 coordinate.</param>
    /// <returns>Whether it can be drawn without inventing a position.</returns>
    public static bool IsMappable(GeoCoordinate? coordinate) => coordinate is not null && Math.Abs(coordinate.Latitude) <= 85.05112878;

    private void Reset()
    {
        Session = Guid.NewGuid().ToString("N");
        favoriteSelection = null;
        Stations = []; Segments = []; Endpoints = [];
    }
}
