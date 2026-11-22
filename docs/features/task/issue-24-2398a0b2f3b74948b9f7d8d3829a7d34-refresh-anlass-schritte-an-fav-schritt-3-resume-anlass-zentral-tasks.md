# Tasks: Vordergrund-Aktualisierung und Anlass-Zuordnung zentralisieren

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Core-Typen | `RefreshArea`-Enum anlegen | Offen | — |
| 2 | Core-Typen | `RefreshOccasion`-Enum anlegen | Offen | — |
| 3 | Core-Typen | `RefreshAreaEventArgs` + `RefreshFailedEventArgs` anlegen | Offen | — |
| 4 | Fassade | `FavoriteHomeViewModel.RefreshForOccasionAsync` einführen, Triplik privat setzen | Offen | — |
| 5 | Lifecycle | Konstruktor um `StopMonitorViewModel`/`JourneySearchViewModel` erweitern; `visible`/`resume`-Zustand | Offen | — |
| 6 | Lifecycle | Interne Subscriptions (`foreground.PropertyChanged`, `favorites.PropertyChanged(Cards)`) | Offen | — |
| 7 | Lifecycle | `NotifyVisibleAsync`/`NotifyHidden`/`IsResuming` + privates `ResumeAsync` + `RunOccasionAsync` | Offen | — |
| 8 | Lifecycle | `ResumeCompleted`/`RefreshFailed`-Events + `RunBackgroundAsync` auf Fassade umstellen | Offen | — |
| 9 | Pages | `HomePage` umstellen (Sichtbar-Meldung, Event-Abos, `resume`-Feld/Methoden entfernen) | Offen | — |
| 10 | Pages | `DeparturePage` umstellen | Offen | — |
| 11 | Pages | `JourneyDetailPage` + `ResultsPage` umstellen | Offen | — |
| 12 | Tests | `LifecycleFixture` um Monitor-/Journey-Doubles erweitern; `RefreshLifecycleTests_Background` anpassen | Offen | — |
| 13 | Tests | `FavoriteTests` + `FavoriteDepartureCacheIntegrationTests` auf `RefreshForOccasionAsync` umstellen | Offen | — |
| 14 | Tests | `RefreshLifecycleTests_Occasions` neu | Offen | — |
| 15 | Tests | `RefreshLifecycleTests_Foreground` neu | Offen | — |
| 16 | Tests | `RefreshLifecycleTests_Overlap` neu | Offen | — |
| 17 | Tests | `RefreshLifecycleTests_Cancellation` neu | Offen | — |
| 18 | Abnahme | Build, `dotnet test`, `WindowsLifecycleUiTests.ps1`-Regression | Offen | — |

Stand der Prüfung: Plan vollständig (plan-check.md); Umsetzung offen.