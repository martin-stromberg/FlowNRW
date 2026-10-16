# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### `FlowNRW.Core/Presentation/JourneyPresentation.cs` (JourneyPresentation)

- **Toter Code** — `Address(Address)` (Z.43–46) formatiert die technische Endpunkt-Identität (`Source`, `Id`, `Dhid`, Koordinaten), hat nach diesem Diff aber keinen einzigen Aufrufer mehr: `EndpointViewModel.Selection` nutzt jetzt `DisplayAddress` (`EndpointViewModel.cs` Z.88) und `TransitVisuals.Candidate` setzt die Beschreibung aus `DisplayName` + `suffix` — der entfernte `showTechnicalIdentity`-Zweig war der letzte Aufrufer (`TransitVisuals.cs` Z.73–74). Auch in den Tests ist kein Aufruf mehr vorhanden. Die Methode liegt als verwaiste öffentliche API herum, obwohl die technische Identität bewusst überall aus der Oberfläche entfernt wurde.
  Empfehlung: Methode löschen; der gemeinsame `CoordinateSuffix`-Helfer bleibt über `DisplayAddress` genutzt. Falls die technische Identität bewusst für künftige Diagnosezwecke erhalten bleiben soll, wenigstens eine Begründung im Kommentar hinterlegen — aktuell suggeriert die XML-Doku eine lebendige Formatierungsfunktion.

## Geprüfte Dateien

- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- `FlowNRW.Core/Maps/MapViewModel.cs`
- `FlowNRW.Core/Presentation/DataNotice.cs` (neu)
- `FlowNRW.Core/Presentation/EndpointViewModel.cs`
- `FlowNRW.Core/Presentation/JourneyPresentation.cs`
- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs`
- `FlowNRW.Tests/FavoriteTests.cs`
- `FlowNRW.Tests/PresentationTests.cs`
- `FlowNRW.Tests/StopMonitorTests.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/JourneyTimelineView.cs`
- `FlowNRW/MapPage.cs`
- `FlowNRW/Resources/Raw/map/map.js`
- `FlowNRW/SearchPage.cs`
- `FlowNRW/StopSearchPage.cs`
- `FlowNRW/TransitIcons.cs`
- `FlowNRW/TransitVisuals.cs`
- `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`
- `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`
