# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

26 von 30 Tasks weiterhin vollständig umgesetzt (`dotnet test FlowNRW.Tests` in dieser Iteration erneut ausgeführt: **307/307 bestanden**). Die vier Befunde aus `review-code.2.md` sind seit Iteration 2 korrigiert, ohne dass Planelemente brachen: `map.js` bindet Session/Generation jetzt als Closure-Konstanten an die Ladegeneration (`sess`/`gen`, `map.js:40`), verworfene Tile-Anfragen werden mit `error:true` beantwortet und schlagen JS-seitig als `failed` durch (`MapPage.cs:235-244`, `map.js:28`), tote `Text`-Initialisierer an `Candidate`-Buttons in `StopSearchPage`/`MapPage` sind entfernt, und die duplizierte Warning-Färbung ist als `NoticeToggle.ApplyWarningColors()` zentralisiert (`TransitVisuals.cs:226-229`, Aufrufe `HomePage.cs:234`, `DeparturePage.cs:100`). Offen bleiben die 4 geräte-/interaktiv gebundenen Tasks #27–#30.

## Umgesetzte Planelemente

- [x] `JourneyPresentation.DisplayName(string)` — `FlowNRW.Core/Presentation/JourneyPresentation.cs:50-61` (Prefix-Regel mit Wortgrenze `' '`, `'('`, `'-'`, `'/'` oder Ende; `null` → `string.Empty`; XML-Doc, Block-Body)
- [x] `JourneyPresentation.DisplayAddress(Address)` — `JourneyPresentation.cs:65-68` (Name + optionale Koordinaten, ohne `Source`/`Id`/`Dhid`); `JourneyPresentation.Address` unverändert erhalten (`:43-46`)
- [x] `JourneyPresentation.Event` auf `DisplayName` — `JourneyPresentation.cs:120`
- [x] `FavoriteMonitorViewModel.HasDataNotice` + Notify — `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs:47-50`, `:196`; Ableitung über gemeinsamen Helfer `FlowNRW.Core/Presentation/DataNotice.cs:12-15` (`Warnings.Count > 0 || IsFallback || IsStale`)
- [x] `FavoriteMonitorViewModel.RefreshCoreAsync` Lines-Merge — `:125-149`; `EnrichLines` (`:159-163`) läuft nur im Anzeige-Zweig (`else`, `:148`); `IsComplete`-Ersetzung und Persistenz ausschließlich im kompletten Zweig `:136-147`
- [x] `FavoriteMonitorViewModel.Title` → `DisplayName` — `:29-32`
- [x] `StopMonitorViewModel.HasDataNotice` + Notify — `FlowNRW.Core/Presentation/StopMonitorViewModel.cs:100-103`, `:392`; `Title` → `DisplayName` `:80-83`; `Status`-Text „unvollständig" als ViewModel-String unverändert `:297-299`
- [x] `FavoriteHomeViewModel.nearbyItems` + `NearbyDistanceLabel` — `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs:122-127`, `:138-148` (Provider `DistanceMeters` → „Entfernung", Haversine-`position`-Fallback → „Luftlinie", sonst „Entfernung unbekannt"; Match per `ReferenceEquals` **oder** `SameIdentity`, `:141`)
- [x] `EndpointViewModel.Selection` → `DisplayAddress` — `FlowNRW.Core/Presentation/EndpointViewModel.cs:84-88`
- [x] `MapStation.Label` → `DisplayName` — `FlowNRW.Core/Maps/MapViewModel.cs:25-28` (Kartenliste + Marker-Tooltips via `MapPage.cs:149`)
- [x] `TransitIcons.Warning` (U+F083) — `FlowNRW/TransitIcons.cs:55`; Codepoint in `docs/design/licenses/MaterialSymbolsRounded.codepoints` Z.4163 (`warning f083`) verifiziert
- [x] `TransitVisuals.Candidate` mit `detailsId`-Parameter und `DisplayName`-Rendering — `FlowNRW/TransitVisuals.cs:71-85`; `NoticeToggle`-Helfer (48×48, `Warning`-Glyph, Beschreibungswechsel, `Update`-Reset) `:185-220`, neu zentralisierte Warning-Färbung `ApplyWarningColors` `:226-229` mit dokumentierter Aufrufreihenfolge nach `ApplyRoles`
- [x] `HomePage` Header-Toggle — `ToggleFavorite{index}` + `SetExpandIcon` in `BuildCardHeader` `FlowNRW/HomePage.cs:238-257` (Grid `[Star, Auto, Auto]`); `BuildCardActions` auf drei Spalten reduziert `:310-336`
- [x] `HomePage` Notice-Muster — `NoticeToggle("FavoriteNotice{index}", "FavoriteMetadata{index}")` `:215`, `Metadata`-Binding auf `card.Metadata` `:216`, Sichtbarkeit `HasDataNotice && !IsBusy` `:217`, Warning-Färbung via `notice.ApplyWarningColors()` nach `ApplyRoles` `:233-234`; `FavoriteCardViews` erweitert `:290-291`; Sichtbarkeit/Reset in `SubscribeCardChanges` `:293-308` (`notice.Update` `:303`)
- [x] `HomePage.ShowsFavoriteStatus` ohne „unvollständig"-Match — `:370-373`
- [x] `HomePage.RenderNearby` mit `NearbyDistanceLabel` und `detailsId: "NearbyDistance" + index` — `:347`
- [x] `DeparturePage` `MonitorNotice` + `MonitorMetadata` — `FlowNRW/DeparturePage.cs:88-100` (`NoticeToggle`-Paar `:90-92` im `monitorState`-Block `:93-96`, `ApplyWarningColors` `:100` nach `Page`/`ApplyRoles` `:99`); `UpdateMonitorStatus` ohne „unvollständig" `:144-155`; `HasDataNotice`-Bewertung in `ModelChanged` `:137-142`
- [x] `SearchPage.Endpoint` ohne technische Kennung + `RenderFavorites` via `DisplayName` — `FlowNRW/SearchPage.cs:145`, `:164-165`
- [x] `JourneyTimelineView` auf `DisplayName` — `FlowNRW/JourneyTimelineView.cs:32-33`, `:53`
- [x] `map.js` Per-Tile-Timeout + Fortschritts-Watchdog — `FlowNRW/Resources/Raw/map/map.js:3-51` (`TILE_TIMEOUT`/`WATCHDOG_INTERVAL` `:3`, `expire` mit `cancel`-Nachricht und Fortschritts-Re-Arm `:11-20`, `init`-Reset `:34-36`, Intervall-Watchdog `:42`, `tileunload`-Aufräumen `:50`); **seit Iteration 2:** Closure-Generationsbindung `sess`/`gen` `:40` für `announceFailed` `:9`, Watchdog `:42`, `load`-Handler `:51`, `createTile`-Timer `:47`, `tileunload` `:50`, `moveend`/Marker-`select` `:44`/`:56` — ein verspätetes `load`/Ereignis einer per `map.remove()` verworfenen Generation kann weder `loaded` mit neuer `loadId` melden noch den frischen Watchdog entschärfen
- [x] `MapPage` nativer Post-`init`-Watchdog — `FlowNRW/MapPage.cs:165`, `WatchLoadAsync` `:170-182` (30 s, Fehlertext `:177`, `tileRequests`-Abbruch `:178`, Generations-/Session-Guard `:175`); `loadId`-Generation `:143`/`:158` mit nativem Verwurf veralteter `loaded`/`failed` `:221-225`; Beendigung über `loadSettled` `:226` und `lifetime`-Token `:104-123`
- [x] `MapPage.MessageReceived` stille Pfade geschlossen + `AppLog`-Diagnose — `:235-244` (explizite leere `tile`-Antwort bei `pending >= 64`/Duplikat — **seit Iteration 2 mit `error:true`**, sodass `map.js:28` `announceFailed` auslöst und der Status „Basiskarte fehlt oder ist veraltet." statt „geladen." zeigt), `AppLog`-Aufrufe `:153`, `:176`, `:223`, `:227`, `:241`, `:251`
- [x] Test `JourneyPresentationTests` — `FlowNRW.Tests/PresentationTests.cs:163-199` (`DisplayName_RemovesRepeatedPrefix`, `_KeepsNonRepeatedAndMissingComma`, `_RequiresWordBoundary`, `DisplayAddress_OmitsIdentityKeepsCoordinates`); bestanden
- [x] Test `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` angepasst + `Refresh_DegradedResultEnrichesDisplayedLines` neu — `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs:111-131`, `:134-169`; bestanden
- [x] Test `HasDataNotice_ReflectsDisplayedResult` + `HasDataNotice_FlagsFallbackAndStale` — `FlowNRW.Tests/FavoriteTests.cs:133-165`, `FlowNRW.Tests/StopMonitorTests.cs:157-191` (Warnings→true, `IsFallback`→true, `IsStale`→true, komplett/`null`→false); bestanden
- [x] Test `NearbyDistanceLabel_PrefersProviderFallsBackToAirline` — `FlowNRW.Tests/FavoriteTests.cs:168-187` (Provider/Haversine/unbekannt/Fremd-Kandidat); bestanden
- [x] Test `Selection_OmitsTechnicalIdentityKeepsCoordinates` — `FlowNRW.Tests/PresentationTests.cs:74-84`; bestanden
- [x] `UiTestMapTiles`-Szenario `map-hang` — `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs:168`
- [x] `WindowsJourneyUiTests.ps1` angepasst/erweitert (Code-Teil) — Kennungs-Assertion durch `-notmatch 'de:'` ersetzt (`:750-751`), `NearbyDistance0`-/`NearbyStop0`-Checks `:218`, `FavoriteNotice`/`FavoriteMetadata`-Toggle + Header-Expand `:337-342`, `map-hang`-Watchdog-Check `:622-628`, `MonitorNotice`/`MonitorMetadata` `:698-700`

## Offene Aufgaben

- [ ] `WindowsJourneyUiTests.ps1` Skriptlauf (Task 27) — Skript vollständig angepasst/erweitert, aber kein dokumentiert bestandener Lauf; erfordert interaktive Windows-Session (`schtasks /it`), keine neuen Artefakte unter `artifacts/tests/` seit dem 14.10. (Verzeichnisse unverändert)
- [ ] `WindowsDepartureCacheUiTests.ps1`/`WindowsLifecycleUiTests.ps1`/`WindowsRefreshUiTests.ps1` (Task 28) — kein nachweisbarer Lauf auf dem geänderten Code; interaktive Session erforderlich
- [ ] `WindowsDesignUiTests.ps1` Screenshot-Matrix (Task 29) — neue Captures nur für `light-430-900-100-favorite-cache-seed-*`; `matrix.jsonl`-Einträge referenzieren weiterhin Commit `ad90d8d` (Branch `modernes-design`, enthält das Feature nicht) mit `workingTree=clean` → Aufnahmen nicht gegen den geänderten Code erstellt; dunkles Theme, 1024×768 und 150 % Textskalierung nicht neu erfasst; kein Matrix-Szenario erzeugt `HasDataNotice` (kein `warning`-Icon-Nachweis); `WindowsDesignUiTests.ps1` unverändert
- [ ] iOS-Verifikation des Karten-Fixes (Task 30) — nicht automatisierbar/gerätegebunden; im Plan ausdrücklich als ausstehender manueller Geräte-/Simulatornachweis (Safari-Web-Inspector, `AppLog`) deklariert

## Hinweise

- `dotnet test FlowNRW.Tests` im Rahmen dieses Reviews ausgeführt: **307/307 Tests bestanden**, keine Fehler (unverändert zu Iteration 2 — die review-code.2-Korrekturen betreffen UI-/JS-Code ohne Testabdeckung).
- **Iteration-2-Korrekturen verifiziert (keine Planelemente gebrochen):** (a) `map.js` Closure-Generation `sess`/`gen` (`:40`) schließt die Restlücke aus review-code.2 — `announceFailed` (`:9`), Watchdog (`:42`) und `load` (`:51`) prüfen `gen !== loadId`; (b) `MapPage.cs:242` sendet `["error"] = true` für verworfene Tiles, `map.js:28` propagiert zu `announceFailed`; (c) tote `Text`-Initialisierer in `StopSearchPage.cs:72-78` und `MapPage.cs:77-82` entfernt (kein doppelter `DistanceLabel`-Aufruf mehr); (d) `NoticeToggle.ApplyWarningColors` (`TransitVisuals.cs:226-229`) kapselt die Reihenfolge-Konvention, Aufrufe `HomePage.cs:234` (nach `ApplyRoles` `:233`) und `DeparturePage.cs:100` (nach `Page`/`ApplyRoles` `:99`).
- **Planabweichung (fachlich zielkonform, aus review-code.2):** Der Plan spezifizierte die leere Tile-Antwort bei verworfenen Anfragen bewusst mit `error:false` („einzelne Löcher sollen die Basiskarte nicht als `failed` markieren"); umgesetzt ist jetzt `error:true` — eine saturierte `pending`-Leitung bedeutet hängende Requests, und ein „geladen"-Status trotz dauerhaft fehlender Kacheln wäre falsch positiv. Einzelne echte Kachelfehler (`tile.Error` aus `GetAsync`) erreichen den Drop-Pfad nicht und verhalten sich unverändert.
- **Planabweichung (bereits Iteration 2 dokumentiert):** `showTechnicalIdentity`-Parameter in `TransitVisuals.Candidate` ganz entfernt statt Flag an Call-Sites (`TransitVisuals.cs:71`) — `JourneyPresentation.Address` bleibt als technische Formatierung erhalten (`:43-46`).
- `map.js` `expire()` implementiert den Per-Tile-Timeout weiterhin als Stall-Timeout (Re-Arm bei Kachel-Fortschritt, `:13-16`) — deckt die geforderte Fortschritts-Rücksetzung ab.
- `FavoriteHomeViewModel.NearbyDistanceLabel` (`:141`) akzeptiert neben `ReferenceEquals` auch `SameIdentity`-Treffer — robustere Variante des geplanten Referenz-Matchings.
- Alle Implementierungsänderungen sind **uncommittet** (Working Tree auf `2e3ef6b`, 24 geänderte Dateien + neue Matrix-PNGs und Review-Artefakte).
- E2E/UIA-Läufe benötigen eine interaktive Windows-Session und konnten in diesem Review nicht automatisiert verifiziert werden; die Artefakte unter `artifacts/tests/` stammen vom 14.10. und belegen keinen Lauf auf dem geänderten Stand.
