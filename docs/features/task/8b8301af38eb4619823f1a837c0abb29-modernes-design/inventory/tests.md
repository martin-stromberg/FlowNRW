# Tests und UI-Test-Fixtures

Bestehende Testinfrastruktur, die vom Design-Umbau berührt bzw. stabil gehalten werden muss.

## Unit-Tests (`FlowNRW.Tests/`)

Presentation-bezogene Testklassen (xUnit; Hilfstypen in `FlowNRW.Tests/TransitTestSupport.cs`):

| Datei | Testklasse(n) | Design-Bezug |
|---|---|---|
| `DeparturePresentationTests_Badges.cs` | `DeparturePresentationTests_Badges` | `BadgeColor_SuppliedModeOrRecognizedName_DoesNotInventCategory` (Mode-/Namens-Matrix), `BadgeColor_WhiteText_DoesNotFallBelowNormalTextContrast` (WCAG ≥4,5:1 für Weiß auf jeder Badgefarbe) |
| `PresentationTests.cs` | `CoordinateParserTests`, `EndpointViewModelTests`, `JourneySearchViewModelTests`, `JourneyPresentationTests` | `JourneyPresentationTests`: `CompactsStaleFallbackAndIncompleteWarnings`, `DisplaysDayLabelsAndUnknownRealtime`, `LabelsOvernightArrivalAsFollowingDay` — sichern die Texte, die als `Secondary`-/Metadata-Labels gerendert werden |
| `StopMonitorTests.cs`, `FavoriteTests.cs`, `ConnectionFavoriteTests.cs`, `JourneyResumeTests_Selection.cs`, `RefreshTests.cs` u. a. | div. | ViewModel-Verhalten (Status-Strings werden textlich von den Pages geprüft, z. B. `ShowsFavoriteStatus`) |
| `Transit/` | Provider-/Mapper-Tests | kein Design-Bezug |

**Keine** Unit-Tests existieren für die UI-Schicht (`TransitVisuals`, `*CardView`, Pages) — diese werden ausschließlich über die Windows-UI-Tests geprüft.

## UI-Test-Skripte (`tests/WindowsJourneyUiTests/`)

| Datei | Zweck |
|---|---|
| `UiTestFixtureServices.cs` | Wird nur in der `UiTest`-Konfiguration in die App gelinkt (`FlowNRW.csproj` Z. 3–8, `UI_TEST_FIXTURES`); deterministische `IStopSearchService`-/`IRoutingService`-/`IDepartureService`-/`IMapTileService`-Fixtures inkl. `UiTestLocationServices` (Szenarien `complete`, `favorite-cache-seed`, `refresh-error` u. a.) und `UiTestRefreshSettingsStore`/`UiTestFavoriteStore` |
| `WindowsDesignUiTests.ps1` | Visuelle Abnahme-Matrix: DPI-bewusste Fenster-Resize, `AssertTouchTarget` (≥44×44 logisch), Tastaturfokus-Prüfung, `Capture`-Screenshots mit `matrix.jsonl`-Metadaten (Commit, SHA256 der `FlowNRW.dll`, Thema, Fenstergröße, Textskalierung, Referenzbild); fährt die komplette Abnahmematrix aus `docs/design/acceptance.md` ab |
| `WindowsJourneyUiTests.ps1` | Funktionale E2E: Suche, Ergebnisse, Detail, Map, Favoriten |
| `WindowsRefreshUiTests.ps1` | Refresh-/Intervall-Flows |
| `WindowsLifecycleUiTests.ps1` | Vordergrund-/Hintergrund-Lifecycle |
| `WindowsDepartureCacheUiTests.ps1` | Departure-Cache-Szenarien |
| `README.md` | Aufrufdokumentation |

Alle Skripte steuern die App über UIAutomation (`AutomationId`, `Name` = `SemanticProperties.Description`) und das `UiTest`-Build-Flavor.

## UiTest-Steuerungsvertrag (darf nicht brechen)

| Mechanismus | Ort | Beschreibung |
|---|---|---|
| `UI_TEST_FIXTURES` | `FlowNRW.csproj` Z. 3–8 | DefineConstants nur in Konfiguration `UiTest` |
| DI-Umschaltung | `MauiProgram.cs` Z. 78–84, 107–173 | `UiTestLocationServices`, `UiTestFixtureServices`, UiTest-Stores, `UiTestMapTiles` |
| `FLOWNRW_UI_TEST_THEME` | `App.xaml.cs` Z. 37 | Erzwingt `UserAppTheme` light/dark |
| `FLOWNRW_UI_TEST_TEXT_SCALE` | `App.xaml.cs` Z. 38–44 | ×1.5-FontSize-Mapping (`DesignTextScale`) |
| `FLOWNRW_UI_TEST_PIN_ACTIVE` | `App.xaml.cs` Z. 53–59 | Hält `ForegroundState.IsActive` beim Screenshot-Fokuswechsel |
| `FLOWNRW_UI_TEST_HIDE_CONTROLS` | alle Pages (`Loaded`-Handler) | Unterdrückt Szenario-Fixturefelder in Abnahmebildern |
| `FLOWNRW_UI_TEST_*` (Pfade) | `MauiProgram.cs` Z. 110–149 | Separate Favorites-/Settings-/Cache-Dateien pro Testlauf |
| `MapViewport` | `MapPage.cs` Z. 24, 44–46, 181 | Test-Label mit „lat;lon;zoom" des Map-Viewports |

## AutomationIds, die die Skripte erwarten (Auswahl, vollständige Liste im Code)

Navigation/Tabs: `OpenHomeStops`, `HomeMap`, `SortFavorites`, `OpenRefreshSettings`, `OpenJourneySearch`, `OpenStopSearch`, `NavigationViewBackButton` (WinUI-Back).

Home: `HomeStatus`, `HomeLocationStatus`, `NearbyStatus`, `FavoriteName{i}`, `FavoriteBusy{i}`, `FavoriteDistance{i}`, `FavoriteStatus{i}`, `FavoriteLines{i}`, `ToggleFavorite{i}`, `RefreshFavorite{i}`, `OpenFavorite{i}`, `RemoveFavorite{i}`, `FavoriteDeparture{c}_{i}`, `NearbyStop{i}`.

Suche/Verbindung: `OriginText`/`DestinationText`, `{prefix}Search`, `{prefix}Location`, `{prefix}CoordinateMode`, `{prefix}Match{i}`, `{prefix}Favorite{id}`, `{prefix}Latitude`/`Longitude`, `{prefix}Status`/`Selection`/`Metadata`/`LocationStatus`/`Busy`/`Favorites`/`Matches`, `SwapEndpoints`, `RoutingTimeMode`, `RoutingDate`, `RoutingTime`, `RoutingArriveBy`, `SearchJourneys`, `RoutingBusy`, `RoutingStatus`, `RoutingMetadata`, `ConnectionFavorites`, `ConnectionFavorite…`, `ConnectionHeader`, `ToggleConnectionFavorite`, `ResultsStatus`, `ResultsMetadata`, `Journey{i}`, `Journey{i}Realtime`, `Journey{i}Date`.

Haltestellen/Monitor/Detail: `StopQuery`, `FindStops`, `FindNearbyStops`, `ShowStopMap`, `StopMatch{i}`, `StopSearchBusy`, `StopSearchStatus`, `StopSearchMetadata`, `NearbyBusy`, `MonitorStop`, `MonitorBusy`, `MonitorStatus`, `FavoriteToggleStatus`, `ToggleFavorite`, `RefreshDepartures`, `Departure{i}`, `DetailStatus`, `DetailMetadata`, `JourneyDetails`, `JourneyDetailSection{i}`, `JourneyTimeline{i}`, `JourneyLine{i}`, `JourneyWalk{i}`, `JourneyOperator{i}`, `ShowJourneyMap`.

Karte/Einstellungen: `MapCanvas`, `MapStatus`, `MapDataStatus`, `MapMetadata`, `MapSegment{i}`, `MapStation{i}`, `MapAttribution`, `MapInformation`, `ResetMap`, `ShowMapList`, `ShowMapCanvas`, `RefreshInterval`, `SaveRefreshSettings`, `RefreshSettingsStatus`, `RefreshIntervalStatus`, `LoggingEnabled`, `SendLog`, `LogStatus`.

## Verbindliche Abnahme-Grundlage

`docs/design/acceptance.md` (Stand 24.09.2026): Token-Tabelle (Flächen, Text, Interaktion `#0058BC`, Semantik `#006E28`/`#894D00`/`#BA1A1A`, Linienfarben, Dunkeltokens, Schrift-Hierarchie „native Systemschrift … wie Inter/SF-Pro-Referenz", 4/8/12/16/24/32-Raster, Radien 16–20/10/12/6–8, ≥44er-Touchziele), Kontrastregeln (≥4,5:1 Text, ≥3:1 Konturen), Bildaufbau je Oberfläche, Screenshot-/Bedienmatrix (Windows schmal 430×900 und breit 1024×768, 150 % Textskalierung) und Freigaberegel (unabhängige visuelle Prüfung, Ablage unter `docs/help/design/verification`). Vorherige visuelle Abnahme: `docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/` (u. a. `inventory-visual-final.md`, `review-visual-final.md`).
