# Entfernungsmechanik: `DistanceLabel`, `NearbyStopResult.DistanceMeters`, `FavoriteHomeViewModel.Distance`

Deckt die Entfernungs-Teilanforderung aus Punkt 5 ab (Nahe Haltestellen: Entfernung statt Kennungen).

## Provider-Entfernung

| Element | Ort | Beschreibung |
|---|---|---|
| `NearbyStopResult` | `FlowNRW.Core/Transit/NearbyStopResult.cs` Z.4–13 | `{ Stop, double? DistanceMeters }` |
| EFA-Mapping | `FlowNRW.Core/Transit/EfaResponseMapper.cs` Z.23–26 | `DistanceMeters = Number(Get(Get(e,"properties"),"distance"))` |
| DB-REST-Mapping | `FlowNRW.Core/Transit/DbRestResponseMapper.cs` Z.23–26 | `DistanceMeters = Number(Get(e,"distance"))` |
| Fixture | `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs` Z.84–102 | Erster Treffer `DistanceMeters = 225`, zweiter ohne (Z.94–95) — liefert `NearbyStopResult`-Antworten für UI-Tests |

## Bestehende Anzeige mit Entfernung (Referenzmuster)

| Element | Ort | Beschreibung |
|---|---|---|
| `StopMonitorViewModel.DistanceLabel(Address candidate)` | `FlowNRW.Core/Presentation/StopMonitorViewModel.cs` Z.140–149 | Nur bei `nearbyActive` und `ReferenceEquals`-Match in `NearbyStops`; liest `NearbyResult.Items.FirstOrDefault(item => ReferenceEquals(item.Stop, candidate.Stop))?.DistanceMeters`; Ausgabe „ · Entfernung: {de-DE,0} m" oder „ · Entfernung unbekannt"; `""` außerhalb Nahe-Kontext |
| `FindNearbyAsync` | `StopMonitorViewModel.cs` Z.111–138 | `location.GetCurrentAsync` → `nearby.NearbyAsync(coordinate)` → `NearbyStops = Items → Address{Name, Stop, Coordinate}` (verwirft `DistanceMeters`, nur `NearbyResult` behält es); `nearbyActive = true` |
| `StopSearchPage.RenderMatches` | `FlowNRW/StopSearchPage.cs` Z.66–81 | `Candidate(candidate, button, model.DistanceLabel(candidate), showTechnicalIdentity: false)` — Nahe-Einträge zeigen „ · Entfernung: X m" (Suffix an Beschreibung und Button-Text) |

## Home-Page-Nahe-Liste (ohne Entfernung, Ist-Stand)

| Element | Ort | Beschreibung |
|---|---|---|
| `FavoriteHomeViewModel.NearbyStops` | `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs` Z.41 | `IReadOnlyList<Address>` — `Address` hat **kein** Entfernungsfeld (`Address.cs` Z.4–15: `Name`, `Coordinate`, `Stop`) |
| `RefreshNearbyAsync` | Z.107–129 | `location.GetCurrentAsync` → `position = current.Coordinate` (Z.114) → `nearby.NearbyAsync` → `NearbyStops = Items.Where(kein Favorit).Select(new Address{Name, Stop, Coordinate})` (Z.119–123) — `item.DistanceMeters` wird **verworfen** |
| `position` | Z.21 | interne `GeoCoordinate?` — gesetzt in `RefreshNearbyAsync` (Z.114) und `UpdateDistancesAsync` (Z.235); in `SetLocationFailure` auf `null` (Z.253) |
| `Distance(origin, destination)` | Z.274–281 | private statische Haversine-Berechnung (Erdradius 6371000 m) |
| `SortCards()` | Z.267–272 | nutzt `Distance(position, card.Stop.Coordinate)` für `card.SetDistance` → `DistanceLabel` der Favoritenkarten („Luftlinie: X m", `FavoriteMonitorViewModel` Z.52–53) |
| `RenderNearby` | `FlowNRW/HomePage.cs` Z.327–339 | `Candidate(candidate, button)` ohne Suffix — **keine** Entfernungsanzeige, Kennung sichtbar (siehe `naming.md`) |
| `OpenNearbyFromHomeAsync` | `StopMonitorViewModel.cs` Z.197–203 | öffnet `candidate.Stop` nach `Id`/`Source`-Validierung |

## Unterschied der beiden Nahe-Mechaniken

| Aspekt | `StopSearchPage` (Haltestellen-Tab) | `HomePage` (Startseite) |
|---|---|---|
| Datenquelle | `StopMonitorViewModel.NearbyResult` + `NearbyStops` | `FavoriteHomeViewModel.NearbyStops` |
| Entfernung | `DistanceLabel(candidate)` aus `NearbyStopResult.DistanceMeters` (Provider) | nicht vorhanden; `DistanceMeters` verworfen; `position`+`Distance()` nur intern für Favoriten-Sortierung |
| Kennung | `showTechnicalIdentity: false` | `showTechnicalIdentity: true` (Default) |
| AutomationId | `StopMatch{index}` | `NearbyStop{index}` |
| Command | `model.OpenAsync(candidate)` | `monitor.OpenNearbyFromHomeAsync(candidate)` (`HomePage` Z.335–336) |
