# Tasks: Vordergrund-Aktualisierung und Anlass-Zuordnung zentralisieren

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Core-Typen | `RefreshArea`-Enum anlegen | Erledigt | `FlowNRW.Core/Refresh/RefreshArea.cs`; Werte in `_Occasions`/`_Foreground`/`_Overlap`/`_Cancellation` verwendet (Enum-Coverage) |
| 2 | Core-Typen | `RefreshOccasion`-Enum anlegen | Erledigt | `FlowNRW.Core/Refresh/RefreshOccasion.cs`; `Startup`/`CardsAdded`/`ForegroundReturn` in Tests abgedeckt |
| 3 | Core-Typen | `RefreshAreaEventArgs` + `RefreshFailedEventArgs` anlegen | Erledigt | `RefreshAreaEventArgs.cs`, `RefreshFailedEventArgs.cs`; `ResumeFailed_Event_CarriesAreaAndOccasion`, `IsResuming_ReflectsResumeState` |
| 4 | Fassade | `FavoriteHomeViewModel.RefreshForOccasionAsync` einführen, Triplik privat setzen | Erledigt | `FavoriteHomeViewModel.cs` Z. 106–116; `FavoriteTests`, `FavoriteDepartureCacheIntegrationTests`, `RefreshLifecycleTests_Occasions` |
| 5 | Lifecycle | Konstruktor um `StopMonitorViewModel`/`JourneySearchViewModel` erweitern; `visible`/`resume`-Zustand | Erledigt | `RefreshLifecycle.cs` Z. 14–18, 29–42; `RefreshLifecycleTests_Foreground` |
| 6 | Lifecycle | Interne Subscriptions (`foreground.PropertyChanged`, `favorites.PropertyChanged(Cards)`) | Erledigt | `RefreshLifecycle.cs` Z. 160–181; `SetActive_VisibleArea_ResumesStale`, `CardsChanged_VisibleAndActive_RefreshesOnlyMissing` |
| 7 | Lifecycle | `NotifyVisibleAsync`/`NotifyHidden`/`IsResuming` + privates `ResumeAsync` + `RunOccasionAsync` | Erledigt | `RefreshLifecycle.cs` Z. 53–147; alle vier neuen Lifecycle-Testklassen |
| 8 | Lifecycle | `ResumeCompleted`/`RefreshFailed`-Events + `RunBackgroundAsync` auf Fassade umstellen | Erledigt | `RefreshLifecycle.cs` Z. 45–48, 105; `RefreshLifecycleTests_Background` (12 Tests unverändert grün), `ResumeFailed_Event_CarriesAreaAndOccasion` |
| 9 | Pages | `HomePage` umstellen (Sichtbar-Meldung, Event-Abos, `resume`-Feld/Methoden entfernen) | Erledigt | `FlowNRW/HomePage.cs`; FlowNRW-Build 0 Warnungen |
| 10 | Pages | `DeparturePage` umstellen | Erledigt | `FlowNRW/DeparturePage.cs` (`refreshLoop.Stop()` in `OnDisappearing` erhalten); FlowNRW-Build 0 Warnungen |
| 11 | Pages | `JourneyDetailPage` + `ResultsPage` umstellen | Erledigt | `FlowNRW/JourneyDetailPage.cs`, `FlowNRW/ResultsPage.cs`; FlowNRW-Build 0 Warnungen |
| 12 | Tests | `LifecycleFixture` um Monitor-/Journey-Doubles erweitern; `RefreshLifecycleTests_Background` anpassen | Erledigt | `RefreshLifecycleTests_Background.cs` Z. 151–200 (`Monitor`/`MonitorService`, `Journeys`/`Routing`, `SeedMonitorAsync`/`SeedJourneysAsync`) |
| 13 | Tests | `FavoriteTests` + `FavoriteDepartureCacheIntegrationTests` auf `RefreshForOccasionAsync` umstellen | Erledigt | `FavoriteTests.cs` Z. 78/88, `FavoriteDepartureCacheIntegrationTests.cs` Z. 23/59 |
| 14 | Tests | `RefreshLifecycleTests_Occasions` neu | Erledigt | 6 Tests grün |
| 15 | Tests | `RefreshLifecycleTests_Foreground` neu | Erledigt | 4 Tests grün |
| 16 | Tests | `RefreshLifecycleTests_Overlap` neu | Erledigt | 3 Tests grün |
| 17 | Tests | `RefreshLifecycleTests_Cancellation` neu | Erledigt | 2 Tests grün |
| 17a | Review-Fixes | Besitzerbasierte Bereichs-Registrierung (`NotifyVisibleAsync`/`NotifyHidden` + `object owner`), `PropertyName`-Guard in `ForegroundChanged`, defensiv gekapselte Event-Invokes, `freshness` an `RefreshMissingAsync` durchgereicht | Erledigt | `RefreshLifecycleTests_Ownership` (3 Tests grün): Non-Owner-Hidden behält Registrierung/Token; Takeover-Semantik |
| 18 | Abnahme | Build, `dotnet test`, `WindowsLifecycleUiTests.ps1`-Regression | Erledigt | 330/330; Builds 0 Warnungen; UI-Lauf bestanden |

Stand: Umsetzung inkl. Review-Fixes abgeschlossen — `dotnet build` (Core/Tests, TreatWarningsAsErrors) 0 Warnungen, `dotnet test` 330/330 grün, `dotnet format --verify-no-changes` sauber, `dotnet build FlowNRW` (MAUI-App) 0 Warnungen. Task 18 (Abnahme inkl. E2E-Regression) offen für die Eltern-Verifikation.
