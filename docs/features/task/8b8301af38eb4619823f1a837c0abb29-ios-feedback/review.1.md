# Plan-Review

## Ergebnis
**Status:** Offene Aufgaben vorhanden

26 von 30 Tasks vollständig umgesetzt (alle Code-Planelemente inkl. Unit-Tests, `dotnet test FlowNRW.Tests`: 303/303 bestanden am 2026-10-15). Offen bzw. teilweise: 4 Tasks — Testabdeckungslücke bei `HasDataNotice`-Flags (#23), fehlende E2E-Laufnachweise (#27, #28), unzureichender Screenshot-Matrix-Nachweis (#29) und die gerätegebundene iOS-Verifikation (#30).

## Umgesetzte Planelemente

- [x] `JourneyPresentation.DisplayName(string)` — `FlowNRW.Core/Presentation/JourneyPresentation.cs:47-58` (Prefix-Regel mit Wortgrenze `' '`, `'('`, `'-'`, `'/'` oder Ende; `null` → `string.Empty`; XML-Doc, Block-Body)
- [x] `JourneyPresentation.DisplayAddress(Address)` — `JourneyPresentation.cs:62-65` (Name + optionale Koordinaten, ohne `Source`/`Id`/`Dhid`); `JourneyPresentation.Address` unverändert erhalten (`:43`)
- [x] `JourneyPresentation.Event` auf `DisplayName` — `JourneyPresentation.cs:113`
- [x] `FavoriteMonitorViewModel.HasDataNotice` + Notify — `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs:47-50`, `:193`
- [x] `FavoriteMonitorViewModel.RefreshCoreAsync` Lines-Merge — `:126` + `EnrichLines` `:160-166`; `IsComplete`-Ersetzung und Persistenz ausschließlich im kompletten Zweig `:138-149`
- [x] `FavoriteMonitorViewModel.Title` → `DisplayName` — `:29-32`
- [x] `StopMonitorViewModel.HasDataNotice` + Notify — `FlowNRW.Core/Presentation/StopMonitorViewModel.cs:100-103`, `:392`; `Title` → `DisplayName` `:80-83`; `Status`-Text „unvollständig" als ViewModel-String unverändert `:297-299`
- [x] `FavoriteHomeViewModel.nearbyItems` + `NearbyDistanceLabel` — `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs:23`, `:122-127`, `:138-146` (Provider `DistanceMeters` → „Entfernung", Haversine-`position`-Fallback → „Luftlinie", sonst „Entfernung unbekannt")
- [x] `EndpointViewModel.Selection` → `DisplayAddress` — `FlowNRW.Core/Presentation/EndpointViewModel.cs:84-90`
- [x] `MapStation.Label` → `DisplayName` — `FlowNRW.Core/Maps/MapViewModel.cs:25-28` (Kartenliste + Marker-Tooltips via `MapPage.cs:147`)
- [x] `TransitIcons.Warning` (U+F083) — `FlowNRW/TransitIcons.cs:54-55`; Codepoint in `docs/design/licenses/MaterialSymbolsRounded.codepoints` Z.4163 (`warning f083`) verifiziert
- [x] `TransitVisuals.Candidate` mit `detailsId`-Parameter und `DisplayName`-Rendering — `FlowNRW/TransitVisuals.cs:65-80`
- [x] `HomePage` Header-Toggle — `ToggleFavorite{index}` + `SetExpandIcon` in `BuildCardHeader` `FlowNRW/HomePage.cs:246-265` (Grid `[Star, Auto, Auto]`); `BuildCardActions` auf drei Spalten reduziert `:324-350`
- [x] `HomePage` Notice-Muster — `FavoriteNotice{index}`-Button (48×48, `Warning`-Glyph, `Warning`/`WarningDark`, Beschreibungswechsel) `:217-225`, `:242`; `FavoriteMetadata{index}`-Detail `:215-216`; `FavoriteCardViews` erweitert `:299-300`; Sichtbarkeit/Reset in `SubscribeCardChanges` `:307-318`
- [x] `HomePage.ShowsFavoriteStatus` ohne „unvollständig"-Match — `:385-388`
- [x] `HomePage.RenderNearby` mit `NearbyDistanceLabel`, `showTechnicalIdentity: false`, `detailsId` — `:352-364`
- [x] `DeparturePage` `MonitorNotice` + `MonitorMetadata` — `FlowNRW/DeparturePage.cs:89-104`; `UpdateMonitorStatus` ohne „unvollständig" `:153-169`; `HasDataNotice`-Bewertung in `ModelChanged` `:150`
- [x] `SearchPage.Endpoint` `showTechnicalIdentity: false` + `RenderFavorites` via `DisplayName` — `FlowNRW/SearchPage.cs:145`, `:164-165`
- [x] `JourneyTimelineView` auf `DisplayName` — `FlowNRW/JourneyTimelineView.cs:32-33`, `:53`
- [x] `map.js` Per-Tile-Timeout + Fortschritts-Watchdog — `FlowNRW/Resources/Raw/map/map.js:3-44` (`TILE_TIMEOUT=15000`, `expire` mit `cancel`-Nachricht und Fortschritts-Re-Arm `:9-18`, Intervall-Watchdog `:35`, `init`-Reset `:31-34`, `tileunload`-Aufräumen `:43`)
- [x] `MapPage` nativer Post-`init`-Watchdog — `FlowNRW/MapPage.cs:162`, `WatchLoadAsync` `:167-182` (30 s, Fehlertext `:174`, `tileRequests`-Abbruch); Beendigung über `loadSettled` `:209-210` und `lifetime`-Token
- [x] `MapPage.MessageReceived` stille Pfade geschlossen + `AppLog`-Diagnose — `:215-222` (explizite leere `tile`-Antwort bei `pending >= 64`/Duplikat), `:151`, `:202`, `:209-210`, `:219`, `:229`
- [x] Test `JourneyPresentationTests` — `FlowNRW.Tests/PresentationTests.cs:164-199` (`DisplayName_RemovesRepeatedPrefix`, `_KeepsNonRepeatedAndMissingComma`, `_RequiresWordBoundary`, `DisplayAddress_OmitsIdentityKeepsCoordinates`); bestanden
- [x] Test `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` angepasst + `Refresh_DegradedResultEnrichesDisplayedLines` neu — `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs:112-131`, `:135-169`; bestanden
- [x] Test `NearbyDistanceLabel_PrefersProviderFallsBackToAirline` — `FlowNRW.Tests/FavoriteTests.cs:151-169` (Provider/Haversine/unbekannt/Fremd-Kandidat); bestanden
- [x] Test `Selection_OmitsTechnicalIdentityKeepsCoordinates` — `FlowNRW.Tests/PresentationTests.cs:74-84`; bestanden
- [x] `UiTestMapTiles`-Szenario `map-hang` — `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs:168`
- [x] `WindowsJourneyUiTests.ps1` angepasst/erweitert (Code-Teil) — Kennungs-Assertion durch Name + `-notmatch 'de:'` ersetzt (`:749-751`), `NearbyDistance0`-/`NearbyStop0`-Checks `:218`, `FavoriteNotice`/`FavoriteMetadata`-Toggle + Header-Expand `:335-342`, `map-hang`-Watchdog-Check `:622-628`, `MonitorNotice`/`MonitorMetadata` `:694-701`

## Offene Aufgaben

- [ ] `HasDataNotice`-Unit-Tests (Task 23) — Tests existieren (`FavoriteTests.cs:133-147`, `StopMonitorTests.cs:157-172`) und bestanden, decken aber nur `Warnings`→true sowie komplett/`null`→false ab; isolierte `IsFallback`- und `IsStale`-Trigger fehlen
- [ ] `WindowsJourneyUiTests.ps1` Skriptlauf (Task 27) — Skript vollständig angepasst/erweitert, aber kein dokumentiert bestandener Lauf; erfordert interaktive Windows-Session (`schtasks /it`)
- [ ] `WindowsDepartureCacheUiTests.ps1`/`WindowsLifecycleUiTests.ps1`/`WindowsRefreshUiTests.ps1` (Task 28) — kein nachweisbarer Lauf auf dem geänderten Code (nur Fixture-Artefakte vom 14.10. ohne Ergebnisprotokolle); interaktive Session erforderlich
- [ ] `WindowsDesignUiTests.ps1` Screenshot-Matrix (Task 29) — neue Captures nur für eine Kombination (`light-430-900-100-favorite-cache-seed`); `matrix.jsonl`-Einträge referenzieren Commit `ad90d8d` (Branch `modernes-design`, enthält das Feature nicht — `FavoriteNotice` fehlt in `git show ad90d8d:FlowNRW/HomePage.cs`) mit `workingTree=clean` → Aufnahmen nicht gegen den geänderten Code erstellt; dunkles Theme, 1024×768 und 150 % Textskalierung nicht neu erfasst; kein Matrix-Szenario erzeugt `HasDataNotice` (kein `warning`-Icon-Nachweis); `WindowsDesignUiTests.ps1` unverändert
- [ ] iOS-Verifikation des Karten-Fixes (Task 30) — nicht automatisierbar/gerätegebunden; im Plan ausdrücklich als ausstehender manueller Geräte-/Simulatornachweis (Safari-Web-Inspector, `AppLog`) deklariert

## Hinweise

- `dotnet test FlowNRW.Tests` im Rahmen dieses Reviews ausgeführt: **303/303 Tests bestanden**, keine Fehler.
- `FavoriteMonitorViewModel.RefreshCoreAsync` (`:126-127`): `EnrichLines` läuft vor der Entscheidung `SetFailure` vs. Anzeige — ein degradiertes Ergebnis reichert `Lines` auch an, wenn es wegen vorhandenem komplettem `Result` nicht angezeigt wird. Fachlich unkritisch (nur gelieferte Linien), aber streng genommen breiter als der Plantext „degradierte, angezeigte Ergebnisse".
- `map.js` `expire()` implementiert den Per-Tile-Timeout als Stall-Timeout (Re-Arm bei Kachel-Fortschritt, `:13-14`) — deckt die im Plan geforderte `map-slow`-Rücksetz-Logik ab, geht aber über ein starres 15-s-Per-Tile-Limit hinaus.
- Alle Implementierungsänderungen sind **uncommittet** (Working Tree auf `2e3ef6b`, 18 geänderte Dateien + neue Matrix-PNGs).
- E2E/UIA-Läufe benötigen eine interaktive Windows-Session und konnten in diesem Review nicht automatisiert verifiziert werden; die Artefakte unter `artifacts/tests/` stammen vom 14.10. und belegen keinen Lauf auf dem geänderten Stand.
