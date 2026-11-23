# Klassen und gebundene Eigenschaften

Betroffene UI-Klassen in `FlowNRW/` und die ViewModel-/Modelleigenschaften aus `FlowNRW.Core/`, an die die Oberflächen binden. Die UI wird vollständig prozedural in C# erzeugt (kein XAML außer `App.xaml`, `AppShell.xaml`, `Resources/Styles/*.xaml`).

## UI-Klassen (FlowNRW)

| Klasse | Datei | Basistyp | Beschreibung |
|---|---|---|---|
| `TransitVisuals` | `FlowNRW/TransitVisuals.cs` | `static class` (internal) | Zentrale Visual-Helfer: Kandidatenkarten, Linienbadges, Icon-Buttons, Rollenfarben, Seitenrahmen |
| `DepartureCardView` | `FlowNRW/DepartureCardView.cs` | `Border` | Abfahrtszeile als Karte: Linienbadge, Ziel, Soll-/Ist-Zeit, Ausfall, Gleis/Steig |
| `JourneyCardView` | `FlowNRW/JourneyCardView.cs` | `Border` | Verbindungs-Ergebniskarte: Zeitspanne, Dauer, Umstiege, Linienbadges, Öffnen-Button |
| `JourneyTimelineView` | `FlowNRW/JourneyTimelineView.cs` | `VerticalStackLayout` | Timeline im Verbindungsdetail: Kopfkarte, Abschnittskarten je Leg, Umstiegskarten |
| `HomePage` | `FlowNRW/HomePage.cs` | `ContentPage` | Favoritenübersicht: Shortcut-Reihe, Favoritenkarten (`RenderCards()`), Nearby-Liste (`RenderNearby()`) |
| `DeparturePage` | `FlowNRW/DeparturePage.cs` | `ContentPage` | Einzelmonitor: Stationskopf, Aktionsgrid, Abfahrtsliste aus `DepartureCardView` |
| `SearchPage` | `FlowNRW/SearchPage.cs` | `ContentPage` | Verbindungssuche: Endpoint-Karten (`Endpoint()`), Zeitpunkt-Block, gespeicherte Verbindungen |
| `StopSearchPage` | `FlowNRW/StopSearchPage.cs` | `ContentPage` | Haltestellensuche: Eingabezeile mit Aktionsbuttons, Trefferliste via `TransitVisuals.Candidate` |
| `ResultsPage` | `FlowNRW/ResultsPage.cs` | `ContentPage` | Ergebnisliste aus `JourneyCardView`, Verbindungsfavorit-Button |
| `JourneyDetailPage` | `FlowNRW/JourneyDetailPage.cs` | `ContentPage` | Verbindungsdetail: Kartenbutton + `JourneyTimelineView` bzw. Detailtext-Abschnitte |
| `MapPage` | `FlowNRW/MapPage.cs` | `ContentPage` | `HybridWebView`-Karte (Leaflet aus `Resources/Raw/map/`), Aktionsgrid, native Stationsliste |
| `RefreshSettingsPage` | `FlowNRW/RefreshSettingsPage.cs` | `ContentPage` | Intervall-`Picker`, Speichern, Diagnose/Logging-Karte |
| `AppShell` | `FlowNRW/AppShell.xaml(.cs)` | `Shell` | Drei Tabs („Abfahrten"/„Verbindungen"/„Haltestellen"), Routen `results`, `detail`, `departures`, `map`, `refresh-settings`; Tabs besitzen nur Titel, **keine Icons** |
| `App` | `FlowNRW/App.xaml(.cs)` | `Application` | Merged Dictionaries, UiTest-Theme/Textskalierung, Window-Lifecycle |
| `MauiProgram` | `FlowNRW/MauiProgram.cs` | `static class` | `ConfigureFonts` (OpenSans-Aliase), DI-Komposition aller Seiten/ViewModels |
| `ShellJourneyNavigation` / `ShellDepartureNavigation` | `FlowNRW/ShellJourneyNavigation.cs` / `ShellDepartureNavigation.cs` | `IJourneyNavigation` / `IDepartureNavigation` | Shell-Routing-Adapter (keine Visuals) |
| `MauiCurrentLocationService` | `FlowNRW/Services/MauiCurrentLocationService.cs` | `ICurrentLocationService` | Standortdienst (keine Visuals) |

## Betroffene ViewModel-/Modelleigenschaften (FlowNRW.Core)

Nur Eigenschaften, die die gelisteten Views tatsächlich binden oder lesen.

### `FavoriteMonitorViewModel` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`) — Datenquelle der Favoritenkarten

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Stop` | `Stop` | Gespeicherte Haltestellenidentität |
| `Title` | `string` | Kartentitel (= `Stop.Name`), gebunden in `FavoriteName{index}` |
| `Result` / `LastAttempt` | `ProviderResult<StopEvent>?` | Letztes erfolgreiches / letztes versuchtes Abfahrtsergebnis |
| `Items` | `IReadOnlyList<StopEvent>` | Angezeigte Abfahrten → `DepartureCardView` (max. 5 pro Karte) |
| `Lines` | `IReadOnlyList<string>` | Linieninventar → kompakte Badge-Zusammenfassung (`FavoriteLines{index}`) |
| `Metadata` | `string` | Quellen-/Alterstext via `JourneyPresentation.Metadata` |
| `IsBusy` | `bool` | `ActivityIndicator` `FavoriteBusy{index}` |
| `IsExpanded` | `bool` | Steuert `departures`-/`summaryContainer`-Sichtbarkeit und `⌃`/`⌄`-Glyph |
| `Status` | `string` | `FavoriteStatus{index}` (sichtbar nur bei definierten Fehler-/Hinweistexten) |
| `DistanceMeters` / `DistanceLabel` | `double?` / `string` | `FavoriteDistance{index}` („Luftlinie: … m" / „Entfernung unbekannt") |
| `RefreshCommand` | `AsyncRelayCommand` | Refresh-Button `RefreshFavorite{index}` |
| `ToggleExpanded()` | Methode | Schaltet `IsExpanded` |

### `FavoriteHomeViewModel` (`FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`) — HomePage

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Cards` | `IReadOnlyList<FavoriteMonitorViewModel>` | Favoritenkarten (triggert `RenderCards`) |
| `NearbyStops` | `IReadOnlyList<Address>` | Nearby-Liste → `TransitVisuals.Candidate` (`NearbyStop{index}`) |
| `NearbyStatus` / `Status` / `LocationStatus` | `string` | Statuslabels `NearbyStatus`, `HomeStatus`, `HomeLocationStatus` |
| `IsSaving` / `IsLocating` | `bool` | Button-CanExecute / Statussichtbarkeit |
| `LocationCommand` | `AsyncRelayCommand` | `SortFavorites`-Button |
| `Contains(card)` / `IsFavorite(stop)` / `ToggleAsync(stop)` / `RemoveAsync(card)` | Methoden | Favoriten-Mutation (Open/Remove/Toggle-Aktionen) |

### `StopMonitorViewModel` (`FlowNRW.Core/Presentation/StopMonitorViewModel.cs`) — DeparturePage, StopSearchPage, HomePage-Navigation

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Lookup` | `EndpointViewModel` | Sucheingabe-ViewModel (`StopQuery`, `Lookup.IsBusy`, `Lookup.SearchCommand`) |
| `Stops` | `IReadOnlyList<Address>` | Suchtreffer → `StopMatch{index}`-Kandidaten |
| `SearchStatus` / `NearbyStatus` | `string` | `StopSearchStatus` / `NearbyStatus` |
| `IsNearbyBusy` / `IsNearbyFailure` | `bool` | `NearbyBusy`-Indikator / Statussichtbarkeit |
| `NearbyResult` | `ProviderResult<NearbyStopResult>?` | Nearby-Warnungen via `JourneyPresentation.CompactWarning` |
| `SelectedStop` / `Title` | `Stop?` / `string` | `MonitorStop`-Überschrift, Favoriten-Buttonzustand |
| `Items` / `Result` / `LastAttempt` | `IReadOnlyList<StopEvent>` / `ProviderResult<StopEvent>?` | Abfahrtsliste → `DepartureCardView` (`Departure{index}`) |
| `Status` / `IsBusy` | `string` / `bool` | `MonitorStatus` / `MonitorBusy` |
| `RefreshCommand` / `NearbyCommand` | `AsyncRelayCommand` | `RefreshDepartures` / `FindNearbyStops` |
| `DistanceLabel(candidate)` | Methode → `string` | Entfernungssuffix für Trefferbuttons |
| `OpenAsync` / `OpenNearbyFromHomeAsync` / `OpenFavoriteAsync` | Methoden | Drilldown-Navigation |

### `JourneySearchViewModel` + `EndpointViewModel` (`FlowNRW.Core/Presentation/`) — SearchPage

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `model.Origin` / `model.Destination` | `EndpointViewModel` | Je eine `Endpoint()`-Eingabekarte (`{prefix}Text`, `{prefix}Search`, `{prefix}Location`, `{prefix}CoordinateMode`, `{prefix}Latitude`/`Longitude`, `{prefix}Matches`, `{prefix}Favorites`, `{prefix}Status`/`Selection`/`Metadata`/`LocationStatus`/`Busy`) |
| `model.UseCurrentTime` / `SelectedDate` / `SelectedTime` / `ArriveBy` | `bool` / `DateTime` / `TimeSpan?` / `bool` | Zeitpunkt-Steuerung (`RoutingTimeMode`, `RoutingDate`, `RoutingTime`, `RoutingArriveBy`) |
| `model.SearchCommand` / `IsBusy` / `Status` / `Metadata` | `AsyncRelayCommand` / `bool` / `string` | `SearchJourneys`, `RoutingBusy`, `RoutingStatus`, `RoutingMetadata` |
| `model.SwapEndpoints()` | Methode | `SwapEndpoints`-Button |
| `endpoint.IsCoordinateMode` / `IsTextMode` / `Text` / `Matches` / `SelectCommand` / `SelectFavorite` | div. | Endpunkt-Moduswechsel (ändert `lookup.Text` `⌕`↔`✓`), Trefferliste via `TransitVisuals.Candidate` |

### `ResultsViewModel` / `JourneyDetailViewModel` (`FlowNRW.Core/Presentation/`)

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `model.Session.Journeys` / `.Status` / `.Metadata` / `.Origin` / `.Destination` / `.SelectedJourney` / `.CompactWarning` | div. | `JourneyCardView`-Liste (`Journey{i}`), `ResultsStatus`/`ResultsMetadata`, `ConnectionHeader`, `DetailStatus`/`DetailMetadata`, `JourneyDetails`-Abschnitte |
| `model.OpenJourneyAsync(journey)` | Methode | Öffnen-Command je `JourneyCardView` |
| `model.Details` | `string` | Fallback-Textabschnitte ohne `SelectedJourney` |

### `RefreshSettingsViewModel` (`FlowNRW.Core/Refresh/RefreshSettingsViewModel.cs`)

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `IntervalSeconds` / `SelectedSeconds` / `IsLoaded` / `IsSaving` | `int` / `bool` | `RefreshInterval`-`Picker` (Werte 0/30/60/120/300), `SaveRefreshSettings` |
| `Status` / `Description` | `string` | `RefreshSettingsStatus` / `RefreshIntervalStatus` |

### `MapViewModel` (`FlowNRW.Core/Maps/MapViewModel.cs`)

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Title` / `Status` / `Metadata` / `Session` | `string` | `MapDataStatus`, `MapStatus`, `MapMetadata` |
| `Stations` / `Segments` / `Endpoints` | Listen | Marker-/Listeninhalte (`MapStation{i}`, `MapSegment{i}`), JSON-Bridge zur `HybridWebView` `MapCanvas` |
| `ShowFavorites` / `ShowStops` / `ShowJourney` / `SelectAsync` | Methoden | Snapshot-Auswahl aus `HomePage`, `StopSearchPage`, `JourneyDetailPage` |

### Transit-Modelle (`FlowNRW.Core/Transit/`) — von Karten/Listen konsumiert

| Record | Relevante Eigenschaften | Verwendung |
|---|---|---|
| `StopEvent` | `Identity` (`TripIdentity`: `Line`, `Direction`, `Stop`, `Operator`), `PlannedTime`, `Realtime`, `Line` | `DepartureCardView`, `JourneyTimelineView.Event`, `DeparturePresentation.Describe` |
| `RealtimeStatus` | `ActualTime`, `Delay`, `Cancelled`, `Platform`, `PlannedPlatform`, `Source`, `RetrievedAt` | Ist-Zeit, Strikethrough bei Ausfall, Gleis-/Steigtexte |
| `Line` | `Id`, `Name`, `Mode`, `Operator` | `TransitVisuals.Badge`-Text und `-Farbe` |
| `Journey` | `Id`, `Legs`, `Transfers` | `JourneyCardView`, `JourneyTimelineView` |
| `JourneyLeg` | `Departure`, `Arrival`, `Line`, `Walking`, `Geometry` | Timeline-Abschnitte, „Fußweg"-Badges |
| `Address` | `Name`, `Coordinate`, `Stop` | `TransitVisuals.Candidate`-Einträge |
