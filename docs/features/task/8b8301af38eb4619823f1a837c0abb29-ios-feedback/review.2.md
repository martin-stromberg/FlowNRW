# Plan-Review

## Ergebnis
**Status:** Offene Aufgaben vorhanden

26 von 30 Tasks vollständig umgesetzt (alle Code-Planelemente inkl. Unit-Tests, `dotnet test FlowNRW.Tests`: **307/307 bestanden**). Die Iteration-1-Lücke #23 ist geschlossen: `HasDataNotice_FlagsFallbackAndStale`-Theories decken jetzt `IsFallback`→true und `IsStale`→true isoliert ab (`FavoriteTests.cs:152-165`, `StopMonitorTests.cs:177-191`). Offen bleiben 4 geräte-/interaktiv gebundene Tasks: fehlende E2E-Laufnachweise (#27, #28), unzureichender Screenshot-Matrix-Nachweis (#29) und die iOS-Verifikation (#30).

## Umgesetzte Planelemente

- [x] `JourneyPresentation.DisplayName(string)` — `FlowNRW.Core/Presentation/JourneyPresentation.cs:50-61` (Prefix-Regel mit Wortgrenze `' '`, `'('`, `'-'`, `'/'` oder Ende; `null` → `string.Empty`; XML-Doc, Block-Body)
- [x] `JourneyPresentation.DisplayAddress(Address)` — `JourneyPresentation.cs:65-68` (Name + optionale Koordinaten, ohne `Source`/`Id`/`Dhid`); `JourneyPresentation.Address` unverändert erhalten (`:43-46`)
- [x] `JourneyPresentation.Event` auf `DisplayName` — `JourneyPresentation.cs:120`
- [x] `FavoriteMonitorViewModel.HasDataNotice` + Notify — `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs:47-50`, `:196`; Ableitung über gemeinsamen Helfer `FlowNRW.Core/Presentation/DataNotice.cs:12-15` (`Warnings.Count > 0 || IsFallback || IsStale`)
- [x] `FavoriteMonitorViewModel.RefreshCoreAsync` Lines-Merge — `:125-149`; `EnrichLines` (`:159-163`) läuft jetzt nur im Anzeige-Zweig (`else`, `:148`) — ein verworfenes degradiertes Ergebnis bei vorhandenem komplettem `Result` reichert `Lines` nicht mehr an (Iteration-1-Hinweis behoben, deckt sich exakt mit dem Plantext „degradierte, angezeigte Ergebnisse"); `IsComplete`-Ersetzung und Persistenz ausschließlich im kompletten Zweig `:136-147`
- [x] `FavoriteMonitorViewModel.Title` → `DisplayName` — `:29-32`
- [x] `StopMonitorViewModel.HasDataNotice` + Notify — `FlowNRW.Core/Presentation/StopMonitorViewModel.cs:100-103`, `:392`; `Title` → `DisplayName` `:80-83`; `Status`-Text „unvollständig" als ViewModel-String unverändert `:297-299`
- [x] `FavoriteHomeViewModel.nearbyItems` + `NearbyDistanceLabel` — `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs:122-127`, `:138-148` (Provider `DistanceMeters` → „Entfernung", Haversine-`position`-Fallback → „Luftlinie", sonst „Entfernung unbekannt"; Match per `ReferenceEquals` **oder** `SameIdentity`, `:141`)
- [x] `EndpointViewModel.Selection` → `DisplayAddress` — `FlowNRW.Core/Presentation/EndpointViewModel.cs:84-88`
- [x] `MapStation.Label` → `DisplayName` — `FlowNRW.Core/Maps/MapViewModel.cs:25-28` (Kartenliste + Marker-Tooltips via `MapPage.cs:150`)
- [x] `TransitIcons.Warning` (U+F083) — `FlowNRW/TransitIcons.cs:55`; Codepoint in `docs/design/licenses/MaterialSymbolsRounded.codepoints` Z.4163 (`warning f083`) verifiziert
- [x] `TransitVisuals.Candidate` mit `detailsId`-Parameter und `DisplayName`-Rendering — `FlowNRW/TransitVisuals.cs:71-85`; zusätzlich `NoticeToggle`-Helfer (48×48, `Warning`-Glyph, Beschreibungswechsel) `:185-220`
- [x] `HomePage` Header-Toggle — `ToggleFavorite{index}` + `SetExpandIcon` in `BuildCardHeader` `FlowNRW/HomePage.cs:240-258` (Grid `[Star, Auto, Auto]`); `BuildCardActions` auf drei Spalten reduziert `:312-338`
- [x] `HomePage` Notice-Muster — `NoticeToggle("FavoriteNotice{index}", "FavoriteMetadata{index}")` `:215`, `Metadata`-Binding auf `card.Metadata` `:216`, Sichtbarkeit `HasDataNotice && !IsBusy` `:217`, Warning-Färbung nach `ApplyRoles` `:236`; `FavoriteCardViews` erweitert `:292-293`; Sichtbarkeit/Reset in `SubscribeCardChanges` `:300-306`
- [x] `HomePage.ShowsFavoriteStatus` ohne „unvollständig"-Match — `:372-375`
- [x] `HomePage.RenderNearby` mit `NearbyDistanceLabel` und `detailsId: "NearbyDistance" + index` — `:349`
- [x] `DeparturePage` `MonitorNotice` + `MonitorMetadata` — `FlowNRW/DeparturePage.cs:88-102` (`NoticeToggle` im `monitorState`-Block, Warning-Färbung `:102`); `UpdateMonitorStatus` ohne „unvollständig" `:146-157`; `HasDataNotice`-Bewertung in `ModelChanged` `:143`
- [x] `SearchPage.Endpoint` ohne technische Kennung + `RenderFavorites` via `DisplayName` — `FlowNRW/SearchPage.cs:145`, `:164-165`
- [x] `JourneyTimelineView` auf `DisplayName` — `FlowNRW/JourneyTimelineView.cs:32-33`, `:53`
- [x] `map.js` Per-Tile-Timeout + Fortschritts-Watchdog — `FlowNRW/Resources/Raw/map/map.js:3-46` (`TILE_TIMEOUT`/`WATCHDOG_INTERVAL` `:3`, `expire` mit `cancel`-Nachricht und Fortschritts-Re-Arm `:11-20`, Intervall-Watchdog `:37`, `init`-Reset `:33-36`, `tileunload`-Aufräumen `:45`)
- [x] `MapPage` nativer Post-`init`-Watchdog — `FlowNRW/MapPage.cs:166`, `WatchLoadAsync` `:171-183` (30 s, Fehlertext `:178`, `tileRequests`-Abbruch `:179`, Generations-/Session-Guard `:176`); Beendigung über `loadSettled` `:227` und `lifetime`-Token `:104-123`
- [x] `MapPage.MessageReceived` stille Pfade geschlossen + `AppLog`-Diagnose — `:236-243` (explizite leere `tile`-Antwort bei `pending >= 64`/Duplikat), `:154`, `:177`, `:224`, `:228`, `:240`, `:250`
- [x] Test `JourneyPresentationTests` — `FlowNRW.Tests/PresentationTests.cs:163-199` (`DisplayName_RemovesRepeatedPrefix`, `_KeepsNonRepeatedAndMissingComma`, `_RequiresWordBoundary`, `DisplayAddress_OmitsIdentityKeepsCoordinates`); bestanden
- [x] Test `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` angepasst + `Refresh_DegradedResultEnrichesDisplayedLines` neu — `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs:111-131`, `:134-169`; bestanden
- [x] Test `HasDataNotice_ReflectsDisplayedResult` + `HasDataNotice_FlagsFallbackAndStale` — `FlowNRW.Tests/FavoriteTests.cs:133-165`, `FlowNRW.Tests/StopMonitorTests.cs:157-191` (Warnings→true, `IsFallback`→true, `IsStale`→true, komplett/`null`→false); bestanden
- [x] Test `NearbyDistanceLabel_PrefersProviderFallsBackToAirline` — `FlowNRW.Tests/FavoriteTests.cs:168-187` (Provider/Haversine/unbekannt/Fremd-Kandidat); bestanden
- [x] Test `Selection_OmitsTechnicalIdentityKeepsCoordinates` — `FlowNRW.Tests/PresentationTests.cs:74-84`; bestanden
- [x] `UiTestMapTiles`-Szenario `map-hang` — `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs:168`
- [x] `WindowsJourneyUiTests.ps1` angepasst/erweitert (Code-Teil) — Kennungs-Assertion durch Name + `-notmatch 'de:'` ersetzt (`:750-751`), `NearbyDistance0`-/`NearbyStop0`-Checks `:218`, `FavoriteNotice`/`FavoriteMetadata`-Toggle + Header-Expand `:337-342`, `map-hang`-Watchdog-Check `:622-628`, `MonitorNotice`/`MonitorMetadata` `:698-700`

## Offene Aufgaben

- [ ] `WindowsJourneyUiTests.ps1` Skriptlauf (Task 27) — Skript vollständig angepasst/erweitert, aber kein dokumentiert bestandener Lauf; erfordert interaktive Windows-Session (`schtasks /it`), keine neuen Artefakte unter `artifacts/tests/` seit dem 14.10.
- [ ] `WindowsDepartureCacheUiTests.ps1`/`WindowsLifecycleUiTests.ps1`/`WindowsRefreshUiTests.ps1` (Task 28) — kein nachweisbarer Lauf auf dem geänderten Code (nur Fixture-Artefakte vom 14.10. ohne Ergebnisprotokolle); interaktive Session erforderlich
- [ ] `WindowsDesignUiTests.ps1` Screenshot-Matrix (Task 29) — neue Captures nur für `light-430-900-100-favorite-cache-seed-*`; `matrix.jsonl`-Einträge referenzieren Commit `ad90d8d` (Branch `modernes-design`, enthält das Feature nicht) mit `workingTree=clean` → Aufnahmen nicht gegen den geänderten Code erstellt; dunkles Theme, 1024×768 und 150 % Textskalierung nicht neu erfasst; kein Matrix-Szenario erzeugt `HasDataNotice` (kein `warning`-Icon-Nachweis); `WindowsDesignUiTests.ps1` unverändert
- [ ] iOS-Verifikation des Karten-Fixes (Task 30) — nicht automatisierbar/gerätegebunden; im Plan ausdrücklich als ausstehender manueller Geräte-/Simulatornachweis (Safari-Web-Inspector, `AppLog`) deklariert

## Hinweise

- `dotnet test FlowNRW.Tests` im Rahmen dieses Reviews ausgeführt: **307/307 Tests bestanden**, keine Fehler (Iteration 1: 303; +4 durch die neuen `HasDataNotice_FlagsFallbackAndStale`-Theory-Fälle).
- Iteration-1-Lücke #23 geschlossen: isolierte `IsFallback`-/`IsStale`-Trigger werden jetzt durch `[Theory]`-Fälle in beiden ViewModel-Testklassen abgedeckt.
- Iteration-1-Hinweis zur Merge-Semantik behoben: `EnrichLines` (`FavoriteMonitorViewModel.cs:148`, `:159-163`) läuft nur noch, wenn das degradierte Ergebnis tatsächlich angezeigt wird; der angepasste Test `:111-131` pinnt, dass ein verworfenes degradiertes Ergebnis `Lines` unverändert lässt.
- **Planabweichung (fachlich zielkonform):** Der Plan sah explizite `showTechnicalIdentity: false`-Flags an den Call-Sites vor; umgesetzt wurde der Parameter in `TransitVisuals.Candidate` ganz entfernt (`TransitVisuals.cs:71`) — die Funktion kann Kennungen nicht mehr rendern. Wirkung identisch (Nahe-Liste, Verbindungssuche, Kartenliste, `StopSearchPage:72-79` kennungsfrei), aber strenger als geplant: der „technische" Pfad existiert nicht mehr. `JourneyPresentation.Address` bleibt als technische Formatierung erhalten (`:43-46`, weiterhin in `StopSearchPage:74` genutzt, dort aber nur als Button-Rohtext, der durch `Candidate` mit `DisplayName` überschrieben wird).
- **Refactoring seit Iteration 1:** `HasDataNotice`-Ableitung in beiden ViewModels über neuen gemeinsamen Helfer `DataNotice.HasDataQualityNotice` (`FlowNRW.Core/Presentation/DataNotice.cs:12-15`, Ergebnis des Code-Reviews `review-code.1.md`); Notice-Button/Detail-Paar als gemeinsame Klasse `TransitVisuals.NoticeToggle` (`TransitVisuals.cs:185-220`) statt dupliziertem Code in `HomePage`/`DeparturePage`.
- `map.js` `expire()` implementiert den Per-Tile-Timeout als Stall-Timeout (Re-Arm bei Kachel-Fortschritt, `:13-16`) — deckt die im Plan geforderte Fortschritts-Rücksetzung ab, geht aber über ein starres 15-s-Per-Tile-Limit hinaus.
- `FavoriteHomeViewModel.NearbyDistanceLabel` (`:141`) akzeptiert neben `ReferenceEquals` auch `SameIdentity`-Treffer — robustere Variante des geplanten Referenz-Matchings.
- Alle Implementierungsänderungen sind **uncommittet** (Working Tree auf `2e3ef6b`, 24 geänderte Dateien + neue Matrix-PNGs und Review-Artefakte).
- E2E/UIA-Läufe benötigen eine interaktive Windows-Session und konnten in diesem Review nicht automatisiert verifiziert werden; die Artefakte unter `artifacts/tests/` stammen vom 14.10. und belegen keinen Lauf auf dem geänderten Stand.
