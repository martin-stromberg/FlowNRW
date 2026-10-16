# Namen und interne Kennungen — Formatierung und alle Anzeigestellen

Deckt Anforderungspunkte 5/6 (interne Kennungen `Source`, `Id`, `Dhid`, Koordinaten) und 7 (Namensbereinigung „X, X Hbf" → „X Hbf") ab.

## Interne Kennungen: Erzeugung

### `JourneyPresentation.Address` (`FlowNRW.Core/Presentation/JourneyPresentation.cs` Z.43)

```csharp
public static string Address(Address address) =>
    $"{address.Name} · {address.Stop?.Source} {address.Stop?.Id} {address.Stop?.Dhid}".Trim()
    + (address.Coordinate is { } c ? FormattableString.Invariant($" · {c.Latitude:0.#####}, {c.Longitude:0.#####}") : "");
```

Erzeugt den kompletten Kennungs-String: `Name · Source Id Dhid · Lat, Lon`.

### `TransitVisuals.Candidate` (`FlowNRW/TransitVisuals.cs` Z.65–78)

| Parameter | Wirkung |
|---|---|
| `candidate: Address` | `action.Text = candidate.Name + suffix` (Z.68) |
| `action: Button` | transparenter Hintergrund, `Ink`-Töne, `Padding = 0` |
| `suffix: string = ""` | Wird an `description` und `action.Text` angehängt |
| `showTechnicalIdentity: bool = true` | `true` → `description = JourneyPresentation.Address(candidate)`; `false` → `description = candidate.Name` (Z.67) |
| Rückgabe | `Border{Padding 16}` mit Button + optionaler `Secondary(details)`-Zeile; `details` = `description` abzüglich führendem `candidate.Name` (Z.73–76) — bei `true` bleibt `· Source Id Dhid · Lat, Lon` (+ suffix) als sichtbare Detailzeile; `SemanticProperties.Description` erhält immer die volle `description` (Z.72) |

## Alle `TransitVisuals.Candidate`-Call-Sites

| Aufruf | Datei/Zeile | `showTechnicalIdentity` | Sichtbarer Detailtext |
|---|---|---|---|
| `RenderNearby` (Nahe Haltestellen auf Startseite) | `FlowNRW/HomePage.cs` Z.337 | nicht übergeben → **true** | `Source Id Dhid · Lat, Lon` als `Secondary`-Zeile + in `SemanticProperties.Description` (Z.334 setzt zusätzlich „Nahe Haltestelle {Name}") |
| `Endpoint`-Trefferliste (Verbindungssuche) | `FlowNRW/SearchPage.cs` Z.145 | nicht übergeben → **true** | dieselbe Kennungs-Zeile; AutomationId `{prefix}Match{i}` (`Origin`/`Destination`) |
| Stationsliste der Karte | `FlowNRW/MapPage.cs` Z.75–81 | **false** | nur `station.Label`-Rest („Keine Kartenposition vorhanden") als Details |
| `RenderMatches` (Haltestellensuche) | `FlowNRW/StopSearchPage.cs` Z.72–79 | **false** + `suffix = model.DistanceLabel(candidate)` | nur Entfernung „ · Entfernung: X m" / „ · Entfernung unbekannt"; Button-`Text` explizit `JourneyPresentation.Address(candidate) + DistanceLabel` — d. h. **der Button-Text** enthält weiterhin die Kennung (wird von `Candidate` mit `action.Text = candidate.Name + suffix` überschrieben, Z.68) |

## Weitere Kennungs-Anzeigestellen

| Stelle | Datei/Zeile | Inhalt |
|---|---|---|
| `EndpointViewModel.Selection` | `FlowNRW.Core/Presentation/EndpointViewModel.cs` Z.84–90 | „Ausgewählt: " + `JourneyPresentation.Address(SelectedAddress)` — volle Kennung + Koordinaten |
| `{prefix}Selection`-Label | `FlowNRW/SearchPage.cs` Z.149, 153, 178 | gebunden an `endpoint.Selection`; initial unsichtbar, sichtbar bei `Selection`-Notify |
| `SelectFavorite(Stop)` | `EndpointViewModel.cs` Z.113–121 | erzeugt `Address{Name = stop.Name, Stop = stop, Coordinate = stop.Coordinate}` — `Selection` zeigt danach ebenfalls die Kennung |
| Favoriten-Schnellauswahl | `FlowNRW/SearchPage.cs` Z.155–170 | Buttons `{prefix}Favorite{card.Stop.Id}` zeigen `card.Stop.Name` (keine Kennung im Text) |
| Gespeicherte Verbindungen | `SearchPage.cs` Z.75–90 | `favorite.Name` (fachlich benannt), keine Kennung |
| `JourneyPresentation.Metadata` | `JourneyPresentation.cs` Z.48 | enthält `result.Source` (z. B. „efa"/„db-rest"/Fixture-Text) — technische Quellangabe in allen `{prefix}Metadata`/`RoutingMetadata`/`MapMetadata`-Zeilen |
| UI-Test-Erwartung | `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1` Z.717 | `Contains 'OriginSelection' 'fixture-0.*de:05113'` — ein vorhandener UI-Test **erwartet** die Kennung in `OriginSelection`/`DestinationSelection` |

## Haltestellennamen: Quellen und Anzeigestellen (Punkt 7)

### Quelle der Namen (unverändert aus Providern)

| Mapper | Datei/Zeile | Zuweisung |
|---|---|---|
| EFA Suche | `FlowNRW.Core/Transit/EfaResponseMapper.cs` Z.18 | `Address.Name = Text(e,"name")` |
| EFA Stop | Z.41–47 | `Stop.Name = Text(e,"name")` (bei `platform`-Treffern `parent`-Stop) |
| DB-REST Suche | `FlowNRW.Core/Transit/DbRestResponseMapper.cs` Z.18 | `Address.Name = name ?? address` |
| DB-REST Stop | Z.42 | `Stop.Name = name ?? address` |

Die gemeldete Form „Münster (Westf), Münster (Westf) Hbf" entspricht dem typischen EFA-Format „Ort, Haltestellenname" wenn Ort und Name beginnend identisch sind. **Es existiert kein Bereinigungs-/Dedup-Helfer** — alle Anzeigestellen rendern `Name` unverändert.

### Alle Anzeigestellen von `Name`/`Stop.Name`

| Stelle | Datei/Zeile | Beschreibung |
|---|---|---|
| `Candidate`-Buttons | `TransitVisuals.cs` Z.68 | `action.Text = candidate.Name + suffix` — Trefferlisten Suche, Nahe, Kartenliste |
| `Selection` | `EndpointViewModel.cs` Z.88 | „Ausgewählt: …" |
| `JourneyPresentation.Event` (privat) | `JourneyPresentation.cs` Z.91 | `item.Identity.Stop.Name` in Verbindungsdetails (Abfahrt/Ankunft-Zeilen) |
| `JourneyTimelineView` | `FlowNRW/JourneyTimelineView.cs` Z.32–33 | `leg.Departure/Arrival.Identity.Stop.Name` („Start unbekannt"/„Ziel unbekannt"-Fallback) |
| `JourneyTimelineView` Umstieg | Z.53 | `transfer.Stop.Name` („Umstieg: {name} · {n} Min.") |
| `MapStation.Label` | `FlowNRW.Core/Maps/MapViewModel.cs` Z.25–28 | `Candidate.Name` + optional „Keine Kartenposition" — Liste + Leaflet-Marker-Tooltips (`map.js` Z.33–34) |
| `FavoriteMonitorViewModel.Title` | `FavoriteMonitorViewModel.cs` Z.29 | `Stop.Name` → `FavoriteName{index}` (`HomePage` Z.232) |
| `StopMonitorViewModel.Title` | `StopMonitorViewModel.cs` Z.80 | `SelectedStop.Name` → `MonitorStop` (`DeparturePage` Z.44–45), Page-`Title` |
| `DepartureCardView` Richtung | `FlowNRW/DepartureCardView.cs` Z.30 | `item.Identity.Direction` (Zielname, nicht Haltestellenname) |
| `MapViewModel.ShowFavorites` | `MapViewModel.cs` Z.72 | `new Address { Name = card.Stop.Name, … }` |
| `NearbyStops`-Projektionen | `FavoriteHomeViewModel.cs` Z.122 / `StopMonitorViewModel.cs` Z.129 | `Address{Name = item.Stop.Name, …}` — verwirft `DistanceMeters` |

## Technische Identität (unverändert notwendig)

`Stop` (`FlowNRW.Core/Transit/Stop.cs` Z.4–21): `Id`, `Name`, `Source`, `Dhid`, `Coordinate`. Identität wird fachlich genutzt für: Favoriten-Speicherung (`SameIdentity` `FavoriteHomeViewModel` Z.283: `Source`+`Id`), Cache-Keys (`DepartureCacheKey`, `IDepartureCacheStore.cs` Z.42), `IsFavorite`-Prüfung (Z.154), `OpenNearbyFromHomeAsync`-Validierung (`StopMonitorViewModel` Z.197–203), `MapStation`-Selektion. `Address` (`Address.cs` Z.4–15): `Name`, `Coordinate`, `Stop`.
