# Bestehende Tests und UI-Test-Fixtures zu den betroffenen Bereichen

## Core-Tests (`FlowNRW.Tests/`)

| Datei | Relevante Tests | Bezug |
|---|---|---|
| `FavoriteDepartureCacheIntegrationTests.cs` | `LoadAsync_KeepsCacheBoardWithOnlyExpiredDeparturesForItsLineInventory` (Z.94–108): `Lines` werden auch ohne zukünftige Abfahrten aus `DepartureCacheEntry.Lines` restauriert, `card.Result` bleibt `null` | Punkt 1 |
| | `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` (Z.110–131): nach kompletter Antwort `Lines=["S1"]`; nach degradiertem Ergebnis (`Warnings=["partial-primary"]`) bleiben `card.Lines` und Cache-`Lines` unverändert, `Result` bleibt „live" | Punkt 1 — pinnt aktuelles `IsComplete`-Verhalten |
| | `LoadAsync_RestoresFutureBoardThenStartupRefreshesIt` (Z.9–32), `LoadAsync_DiscardsExpiredEventsAndRetainsFutureCacheOnFailure` (Z.34–66), `LoadAsync_RestoresStaleBoardWithFutureDepartureAsMarkedStale` (Z.68–92), `LoadAndRemoval_CleanOrphanedAndRemovedCacheBoards` (Z.133–146) | Cache-/Restore-Pfade |
| `FavoriteTests.cs` | `RefreshFailureAndCancellationPreserveCompletedBoard` (Z.91–106): Fehlschlag behält `Result`, Status „Letzte bekannte" | Punkte 1/2 |
| | `DistanceSortIsExplicitStableAndDoesNotPersistPosition` (Z.108–129), `EqualDistancesAndStaleMapMembership` (Z.131–158), `StoreRoundTripPreservesIdentityAndCoordinates` (Z.11–26), `BoardsRefreshIndependentlyAndRejectLateResponses` (Z.68–89), `SaveFailuresPreserveMembershipAndAllowRetry` (Z.49–66) | Sortierung/Identität |
| `LocationTests.cs` | `NearbyUsesActualPositionAndOpensSameMapStop` (Z.52–78): `DistanceLabel` enthält „123" (Z.68); `NearbyFailureRetainsProvenanceAndEmptySuccessClears` (Z.147–168): `DistanceLabel` → „unbekannt" (Z.155) | Punkt 5 — vorhandene `DistanceLabel`-Assertions |
| | `ControlledNearby`-Hilfsklasse (Z.251 ff.) sowie `ControlledLocation`/`ControlledSearchService`/`ControlledDepartureService` aus `TransitTestSupport.cs` | Test-Doubles |
| `MapTests.cs` | `PreservesCandidatesAndRejectsStaleSelection` (Z.13–33), `PreservesGeometryGapsAndClearsPreviousJourney` (Z.35–51), `RejectsUnsafeTemplates` (Z.53–60), Tile-Service-Tests `CachesAcrossSessions…`/`RevalidatesAndHonorsNoStore`/`ReportsOfflineAndStale`/`BoundsContentAndCancellation`/`BoundsDiskCacheAndTimeout` (Z.62–139) | Punkt 4 — `MapTileService`-Verhalten, `MapViewModel`-Projektion |
| `PresentationTests.cs` | `JourneyPresentationTests.CompactsStaleFallbackAndIncompleteWarnings` (Z.141–147): `CompactWarning` == „Daten möglicherweise unvollständig" | Punkt 2; natürlicher Ort für Namensbereinigungs-Tests (Punkt 7) — keine Namens-Tests vorhanden |
| | `EndpointViewModelTests` (Z.36–85), `JourneySearchViewModelTests` (Z.87–135), `JourneyPresentationTests` Zeit-/Leg-Tests (Z.138–208) | Punkte 6/7 |
| `StopMonitorTests.cs` | Status-Assertions „nicht geladen" (Z.173), „keine Echtzeitdaten" (Z.219) u. a. | Punkt 2 |
| `DepartureCacheStoreTests.cs`, `DeparturePresentationTests_Badges.cs`, `Transit/`-Mapper-Tests | Persistenz-Validierung, Badge-Farben, Mapper-Normalisierung | Punkte 1/7 |

## Native Windows-UI-Tests (`tests/WindowsJourneyUiTests/`)

PowerShell-Skripte gegen den `UiTest`-Build (`UI_TEST_FIXTURES`); die Fixture-Services werden in die App kompiliert (`FlowNRW.csproj` Z.3–8).

| Skript | Relevante Erwartungen | Bezug |
|---|---|---|
| `WindowsJourneyUiTests.ps1` | `FavoriteLines0` enthält „RE 1"/„Live Stand 1" (Z.231, 239, 242, 264, 275); `FavoriteStatus{far}` matcht „Letzte bekannte\|fehlgeschlagen" (Z.277); `ToggleFavorite0`-Klick öffnet/schließt (Z.150–156 in DepartureCache-Tests analog); `NearbyStop0` klickbar (Z.217–218); `OriginSelection`/`DestinationSelection` **erwarten technische Kennung** „fixture-0.*de:05113" (Z.717), „Aktueller Standort" (Z.344, 430–432), „Newest manual endpoint" (Z.453), „Umgebung"/„Adresse Berlin" (Z.690–694) | Punkte 1, 3, 5, 6 |
| | `MapStatus`-Erwartungen: „Basiskarte geladen." (Z.480, 525, 576, 582, 590–591, 602, 608, 614), „fehlt oder ist veraltet" (Z.589, 604 in `map-offline`-Szenario); `MapViewport` ändert sich bei Pan (Z.591); `MapStation0`, `ShowMapList`/`ShowMapCanvas`, `ResetMap` | Punkt 4 |
| | `StopSearchMetadata` enthält „Daten möglicherweise unvollständig" (Z.472) | Punkt 2 |
| `WindowsDepartureCacheUiTests.ps1` | `FavoriteLines0`↔„RE 1" bei Zuklappen (Z.104, 156, 205, 245, 254, 261); `FavoriteStatus0` „Letzte bekannte\|fehlgeschlagen" (Z.192, 253); `ToggleFavorite0` auf/zu (Z.150–155, 187, 207); Fixture-Szenarien `favorite-cache-seed`, `cache-lines-bc`, `cache-lines-empty`, `cache-expired-lines`, `cache-timeless-lines` | Punkt 1 |
| `WindowsLifecycleUiTests.ps1` | `FavoriteLines{far}` unverändert bei Resume-Fehler (Z.257–261) | Punkt 1 |
| `WindowsRefreshUiTests.ps1` | `FavoriteLines{far}` unverändert bei automatischem Fehler (Z.276–280) | Punkt 1 |
| `WindowsDesignUiTests.ps1` | `ToggleFavorite0` auf/zu mit Screenshots (Z.294–295); `MapStatus` „Basiskarte geladen"/„fehlt oder ist veraltet" (Z.305–308) | Punkte 3, 4 |

## UI-Test-Fixtures (`tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`)

| Element | Zeile | Beschreibung |
|---|---|---|
| `UiTestFixtureServices` | Z.11–153 | `IStopSearchService`/`IRoutingService`/`IDepartureService`/`IMapTileService`; Abfahrtsantworten je Szenario: `complete`/`complete-slow`/`resume-fresh`/`favorite-cache-seed`/`cache-start-*`/`cache-lines-*` → `IsFallback=false`, `Warnings=[]` („komplett"); alle anderen Standard-Szenarien → `IsFallback=true`, `IsStale=true`, `Warnings=["fixture_warning"]` (Z.33–56) — d. h. Standard-Fixture liefert absichtlich degradierte Ergebnisse |
| `DeparturesAsync` | Z.29–57 | `RecordDeparture`, Verzögerungen (`monitor-slow`, `favorite-slow`, 6000 ms vs. 700 ms), `cache-lines-*`-Linienbestände |
| `NearbyAsync` | Z.84–102 | `nearby-empty`/`nearby-error`, `nearby-missing` (Koordinate fehlt), `nearby-slow`; `DistanceMeters=225` nur erster Treffer |
| `SearchAsync`/`RouteAsync`/`Candidate`/`Route`/`Event` | Z.70–153 | Treffer mit `Dhid="de:05113:001:{i}"`, `Source="fixture"` |
| `UiTestMapTiles` | Z.158–171 | SVG-Kacheln; `map-offline` → 4× `Error`, `map-slow` → 6 s Delay |
| `UiTestLocationServices` | Z.174–236 | `Scenario`-Steuerung über `HomeScenario`/`FavoriteScenario`/`NearbyScenario`/`LocationScenario`/`LifecycleScenario`-Entries; `Calls`/`DepartureCalls`/`RouteCalls`-Zähler als Labels |
| `UiTestFavoriteStore`/`UiTestRefreshSettingsStore` | Z.241–276 | Fehlerinjektion `store-error`/`refresh-store-error` |

## Lücke (Ist-Stand)

- Kein Unit-Test für das Zusammenspiel „degradierte frische Antwort ersetzt degradiertes Board, `Lines` bleibt leer wenn nie ein komplettes Ergebnis kam" (Szenario aus Commit `69e6f93`/iOS-Meldung).
- Keine Namensbereinigungs-Tests; `JourneyPresentationTests` ist der Ort mit den nächstliegenden `Assert.Equal`-Textvergleichen.
- Kein iOS-Testpfad — alle UI-Nachweise sind Windows (`WEBVIEW2`-basiert); die Fixture `UiTestMapTiles` liefert SVG statt OSM-PNG.
