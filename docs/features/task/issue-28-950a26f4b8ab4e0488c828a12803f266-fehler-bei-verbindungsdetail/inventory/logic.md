# Logik – Bestandsaufnahme

## `JourneySearchViewModel`
Datei: `FlowNRW.Core/Presentation/JourneySearchViewModel.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SearchAsync` | public | Neue Suche, setzt `Result`, navigiert zur Ergebnisliste |
| `OpenJourneyAsync` | public | Setzt `SelectedJourney`, navigiert zur Detailseite |
| `RefreshIfStaleAsync` | public | Erneuert `Result` bei `IsStale`; ordnet `SelectedJourney` per `SameJourney` neu zu; setzt Status „…nicht mehr eindeutig bestätigt…", wenn `selected != null && SelectedJourney == null` (Zeile 188-194) |
| `SameJourney` | private static | Alle Abschnittsereignisse müssen per `SameEvent` übereinstimmen (Zeile 204-206) |
| `SameEvent` | private static | Erfordert nicht-leere `Source`, `TripId`, `Stop.Source`, `Stop.Id` und exakt gleiche `PlannedTime` — bei Fußwegabschnitten (`TripId = null`, evtl. leere `Stop.Id`) nie erfüllt (Zeile 208-217) |
| `CancelPending` | public | Bricht laufende Anfrage ab |
| `EndpointChanged` | private | Setzt `Result`/`SelectedJourney` zurück |

## `JourneyDetailViewModel`
Datei: `FlowNRW.Core/Presentation/JourneyDetailViewModel.cs`

| Methode / Eigenschaft | Sichtbarkeit | Kurzbeschreibung |
|----------------------|-------------|------------------|
| `Session` | public | Geteilte `JourneySearchViewModel`-Sitzung |
| `Details` | public | `SelectedJourney == null` → `Session.Status` (sonst „Keine Verbindung ausgewählt."); sonst `JourneyPresentation.Detail` — liefert bei Statusmeldung denselben Text wie das `DetailStatus`-Label (Zeile 20-22) |

## `JourneyDetailPage`
Datei: `FlowNRW/JourneyDetailPage.cs`

| Element | Kurzbeschreibung |
|---------|------------------|
| `DetailStatus`-Label | Bindet an `Session.Status` (Zeile 37-38) |
| `RenderDetails` | Bei `SelectedJourney == null` wird `model.Details` (=`Session.Status`) als `JourneyDetailSection*`-Karte gerendert → Status erscheint zweimal (Zeile 102-115) |
| `OnAppearing` → `ResumeAsync` | Ruft `Session.RefreshIfStaleAsync(freshness)` bei aktivem Fenster und konfiguriertem Intervall (Zeile 63-100) |

## `ResultsPage`
Datei: `FlowNRW/ResultsPage.cs`

| Element | Kurzbeschreibung |
|---------|------------------|
| `ResumeAsync` | Ruft `Session.RefreshIfStaleAsync(freshness)` bei Appearing/Vordergrund (Zeile 93-98) — Aktualisierung kann bereits auf der Ergebnisseite erfolgen |
| `Journey{i}`-Karten | `AsyncRelayCommand` → `OpenJourneyAsync` (Zeile 104-109) |

## `ProviderOrchestrator` (Referenzmuster für Zuordnung)
Datei: `FlowNRW.Core/Transit/ProviderOrchestrator.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SameJourney` | private static | Pro Abschnitt: unterschiedliche `Walking`-Belegung → `false`; Verkehrsabschnitt → `RealtimeConsolidator.Matches` für Departure+Arrival; Fußweg → `SameWalkingEvent` (Zeile 213-224) |
| `SameWalkingEvent` | private static | `PlannedTime`-Differenz ≤ 60 s **und** `RealtimeConsolidator.SameStop` — erfordert weder `TripId` noch `Stop.Id` (Zeile 226-228) |
| `MergeJourneys`/`EnrichJourney`/`Unmatched` | private | Zusammenführung regionaler/nationaler Ergebnisse mit Eindeutigkeitsgebot in beiden Richtungen |

## `RealtimeConsolidator`
Datei: `FlowNRW.Core/Transit/RealtimeConsolidator.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Consolidate` | public | Echtzeitanreicherung nur bei eindeutiger beidseitiger Zuordnung |
| `Matches` | internal static | `SameStop` + `PlannedTime` ≤ 60 s + (`SharedTripId` gleich, oder `Source`+`TripId` gleich, oder `Line`+`Operator`+`Direction` gleich) |
| `SameStop` | internal static | `Dhid`-Gleichheit, sonst `Source`+`Id`-Gleichheit, sonst Name + Koordinate ≤ 100 m |

## `RefreshFreshness`
Datei: `FlowNRW.Core/Refresh/RefreshFreshness.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `IsStale<T>` | `true` bei fehlendem/fehlerhaftem Ergebnis oder Alter ≥ `RealtimeTimeToLive` (Echtzeit-Lebensdauer, ~30 s) |

## Ablauf des gemeldeten Fehlers

1. Suche → `Result` mit `RetrievedAt`; Ergebnisliste wird ≥ Lebensdauer betrachtet.
2. `OpenJourneyAsync` → `JourneyDetailPage.OnAppearing` → `ResumeAsync` → `RefreshIfStaleAsync` (Daten veraltet).
3. Neues `Result` ersetzt das alte; `SameJourney` schlägt bei Fußwegabschnitten fehl (`TripId`/`Stop.Id` leer) → `SelectedJourney = null`.
4. Status „…nicht mehr eindeutig bestätigt…" erscheint einmal im `DetailStatus`-Label und erneut als `JourneyDetailSection0`-Karte via `Details`.
