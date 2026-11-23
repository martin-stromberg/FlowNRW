# Favoritenkarten: Lines-Logik, Expand-Mechanik und Kartenstruktur

Deckt Anforderungspunkte 1 (Linienliste im zugeklappten Zustand) und 3 (Auf-/Zuklapp-Schalter) ab.

## `FavoriteMonitorViewModel` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`)

| Element | Typ/Zeile | Beschreibung |
|---|---|---|
| `Lines` | `IReadOnlyList<string>` Eigenschaft, Z.37 | „Last complete line inventory"; initial `[]`, private set |
| `Restore(DepartureCacheEntry, now, freshness)` | `internal bool`, Z.81–95 | Setzt `Lines` aus `entry.Lines` oder — falls leer — aus `entry.Result.Items.Select(LineName)`, dedupliziert und sortiert (Z.84–86). Läuft **vor** jeder Vollständigkeitsprüfung; auch bei `retained.Length == 0` (Z.89) bleibt `Lines` gesetzt |
| `RefreshCoreAsync(automatic, ct)` | `private async Task`, Z.104–146 | Setzt `Result` bei `ErrorCode is null` und (`IsComplete(result)` oder Board nicht vollständig) (Z.118–125); `Lines` nur im `IsComplete(result)`-Zweig neu berechnet (Z.129–132) und anschließend über `persist` als `DepartureCacheEntry{Lines=Lines}` geschrieben (Z.133–139) |
| `IsComplete(result)` | `private static bool`, Z.148 | `ErrorCode is null && Warnings.Count == 0 && !IsFallback && !IsStale` |
| `LineName(StopEvent)` | `private static string`, Z.149 | `item.Line?.Name ?? item.Identity.Line ?? string.Empty` |
| `Status` | `string`, Z.48 | Z.126–128: „Daten möglicherweise unvollständig oder veraltet." bei `Warnings>0 || IsFallback || IsStale`, sonst „Keine nächsten Abfahrten gefunden." oder „{n} Abfahrten · {automatisch|manuell} aktualisiert."; Z.92 „Letzter Stand wird aktualisiert …"; `SetFailure` Z.167–172 „Abfahrten konnten nicht geladen werden…"/„Aktualisierung fehlgeschlagen. Letzte bekannte Daten…" |
| `IsExpanded` / `ToggleExpanded()` | `bool` Z.46 / Methode Z.97–102 | Invertiert `IsExpanded`, `Notify(nameof(IsExpanded))` |
| `Metadata` | `string`, Z.39–42 | `JourneyPresentation.Metadata(Result ?? LastAttempt)` — vorhanden, in `HomePage` nicht gebunden |
| `DistanceLabel` | `string`, Z.52–53 | „Luftlinie: X m" / „Entfernung unbekannt" (Favoritenkarten-Distanz, nicht Nahe-Liste) |
| `RefreshBindings()` | `private void`, Z.174–178 | Notifiziert u. a. `Result`, `Items`, `Lines`, `Metadata`, `Status`, `IsBusy`, `IsExpanded` |
| `revision`/`request` | Z.13–14 | Versionsguard: Antworten mit `version != revision` werden verworfen (Z.115); `CancelPending` Z.152–157 |

**Änderungshistorie (Commit `69e6f93`):** `if (result.ErrorCode is not null || (!IsComplete(result) && Result is not null))` → `... && IsComplete(Result)`. Degradierte frische Antworten ersetzen seither ein bereits degradiertes Board; `Lines` wird dabei nicht aktualisiert. Commit-Message begründet: EFA-Antworten ~700 ms, db-rest `/locations` für fremde Quellen ~20 s Timeout → `Warnings` auf ansonsten nutzbaren Antworten.

## `DepartureCacheEntry.Lines` (`FlowNRW.Core/Favorites/IDepartureCacheStore.cs` Z.45–65)

| Element | Typ/Zeile | Beschreibung |
|---|---|---|
| `DepartureCacheEntry.Lines` | `IReadOnlyList<string>`, Z.57–58 | „Last complete set of line names … retained even without a future departure" |
| `Key` | `DepartureCacheKey(Source, StopId)`, Z.42, 60–64 | Technischer Schlüssel je Favorit |

`JsonDepartureCacheStore.Validate` (`JsonDepartureCacheStore.cs` Z.141–163): `entry.Result.ErrorCode is not null || IsStale` → ungültig (Z.152); `Lines` ≤100 Einträge, nicht leer/≤256 Zeichen (Z.154–156); beim Schreiben dedupliziert + sortiert (Z.160). Auf ladender Seite prüft `HomePage`/`FavoriteHomeViewModel.LoadAsync` (Z.70–88) via `card.Restore`.

## `HomePage` — Kartenaufbau (`FlowNRW/HomePage.cs`)

| Element | Typ/Zeile | Beschreibung |
|---|---|---|
| `RenderCards()` | `private void`, Z.197–203 | Baut `cards.Children` aus `model.Cards`; räumt `unsubscribe`/`actions` auf |
| `BuildFavoriteCard(card, index)` | `private Border`, Z.205–227 | Reihenfolge: `BuildCardHeader` → `FavoriteDistance{index}` (AddLabel Z.209) → `FavoriteStatus{index}`-Label (Z.210–213) → `summaryContainer` = `ScrollView`(horizontal) um `FavoriteLines{index}`-`HorizontalStackLayout` (Z.214–216) → `departures`-`VerticalStackLayout` (Z.217–218) → `BuildCardActions` (Z.224); initial `departures.IsVisible = card.IsExpanded`, `summaryContainer.IsVisible = !card.IsExpanded` (Z.221–222) |
| `BuildCardHeader(card, index)` | `private static Grid`, Z.229–239 | Grid `[Star, Auto]`; `FavoriteName{index}`-Titel (22, bold) in Spalte 0; `FavoriteBusy{index}`-`ActivityIndicator` in Spalte 1 (rechte Auto-Spalte, gebunden an `IsBusy`, Beschreibung „Abfahrten werden aktualisiert") |
| `RenderFavoriteSummary(card, summary)` | `private static void`, Z.241–256 | Iteriert `card.Lines`; je Linie `TransitVisuals.Badge(line + "  " + Clock(next))` mit nächstem Abfahrtszeitpunkt aus `card.Items`; `SemanticProperties.Description` = „line1 · line2 …" |
| `RenderFavoriteDepartures` | `private static void`, Z.258–263 | Max. 5 `DepartureCardView`-Einträge (`FavoriteDeparture{index}_{item}`, `compact: true`) |
| `FavoriteCardViews` | `private sealed record`, Z.271–272 | Bündelt `Status`-Label, `Summary`, `SummaryContainer`, `Departures` |
| `SubscribeCardChanges` | `private void`, Z.274–288 | Bei `Items`/`Lines`: Summary+Departures neu rendern (Z.278); bei `Status`/`IsBusy`/`Result`/`IsExpanded`: `Status.IsVisible = ShowsFavoriteStatus && !IsBusy`, `Departures.IsVisible = IsExpanded`, `SummaryContainer.IsVisible = !IsExpanded` (Z.279–284) |
| `BuildCardActions(card, index)` | `private Grid`, Z.290–325 | Vier-Spalten-Aktionsleiste **am Kartenende**: `ToggleFavorite{index}` (Z.293–300, Icon `ExpandMore`/`ExpandLess` via `SetExpandIcon`, 48×48, Beschreibung „Abfahrten aufklappen"/„einklappen") · `RefreshFavorite{index}` (Z.302–304) · `OpenFavorite{index}` (Z.316–318) · `RemoveFavorite{index}` (Z.320–322) |
| `ShowsFavoriteStatus(card)` | `private static bool`, Z.360–364 | Matcht „Keine nächsten", „Letzter Stand", „unvollständig", „fehlgeschlagen", „nicht geladen" |
| `ShowsHomeStatus` / `ShowsLocationStatus` / `ShowsNearbyStatus` | Z.347–358 | Sichtbarkeitsregeln für `HomeStatus`, `HomeLocationStatus`, `NearbyStatus` |

## Refresh-Orchestrierung

| Element | Ort | Beschreibung |
|---|---|---|
| `ReconcileRefreshLoops()` | `HomePage.cs` Z.165–183 | Je Karte ein `RefreshLoop` (`FlowNRW.Core/Refresh/RefreshLoop.cs` Z.4); gestartet mit `settings.IntervalSeconds`, nur wenn `active && foreground.IsActive && settings.IsLoaded && resume is null` |
| `RefreshMissingAsync` / `RefreshAtStartupAsync` | `HomePage.cs` Z.185–195 → `FavoriteHomeViewModel` Z.99–103 | `RefreshMissing` nur für `freshness.IsStale(card.Result)`; `RefreshAtStartup` für alle Karten |
| `ForegroundChanged`/`OnAppearing`/`OnDisappearing` | `HomePage.cs` Z.100–163 | Lebenszyklus: Load, Render, Refresh, CancelPending |

## Relevante AutomationIds

`FavoriteName{index}`, `FavoriteBusy{index}`, `FavoriteDistance{index}`, `FavoriteStatus{index}`, `FavoriteLines{index}`, `FavoriteDeparture{index}_{item}`, `ToggleFavorite{index}`, `RefreshFavorite{index}`, `OpenFavorite{index}`, `RemoveFavorite{index}`, `NearbyStop{index}`, `HomeStatus`, `HomeLocationStatus`, `NearbyStatus`, `NearbyHeading`, `HomeMap`, `SortFavorites`, `OpenHomeStops`, `OpenRefreshSettings`, `OpenJourneySearch`.

## Designvorgaben (Ist-Dokumentation)

`docs/help/design/business-rules.md` „Bedienziele": Symbolschalter 48×48, jeder trägt `SemanticProperties.Description`; Zustandswechsel (z. B. Chevron) aktualisiert die Beschreibung im selben Aufruf (absorbiert aktuell `SetExpandIcon` in `BuildCardActions`).
