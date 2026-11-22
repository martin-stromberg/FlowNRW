# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen (FlowNRW.Core/Refresh)

- [x] `RefreshArea` (Enum `Favorites`/`Monitor`/`Journeys`) — `FlowNRW.Core/Refresh/RefreshArea.cs` angelegt
- [x] `RefreshOccasion` (Enum `Startup`/`CardsAdded`/`ForegroundReturn`) — `FlowNRW.Core/Refresh/RefreshOccasion.cs` angelegt
- [x] `RefreshAreaEventArgs` (EventArgs mit `Area`) — `FlowNRW.Core/Refresh/RefreshAreaEventArgs.cs` angelegt
- [x] `RefreshFailedEventArgs` (EventArgs mit `Area` + `Occasion`) — `FlowNRW.Core/Refresh/RefreshFailedEventArgs.cs` angelegt

### `RefreshLifecycle` (`FlowNRW.Core/Refresh/RefreshLifecycle.cs`)

- [x] Konstruktor-Parameter `StopMonitorViewModel monitor`, `JourneySearchViewModel journeys` — Z. 29–31
- [x] Felder `HashSet<RefreshArea> visible`, `Dictionary<RefreshArea, CancellationTokenSource> resume` — Z. 17–18
- [x] Methode `NotifyVisibleAsync(RefreshArea)` — Z. 53–64 (Favorites→Startup bei aktiv, Journeys→`ResumeAsync`, Monitor→kein Anlass)
- [x] Methode `NotifyHidden(RefreshArea)` — Z. 68–72 (visible-Entfernung + `CancelArea`)
- [x] Methode `IsResuming(RefreshArea)` — Z. 77
- [x] private Methode `ResumeAsync(RefreshArea)` — Z. 117–140 (Gate `!visible || !IsActive || !IsLoaded || IntervalSeconds == 0 || resume.ContainsKey`, Per-Bereich-CTS, `ReferenceEquals`-Guard im `finally`, `ResumeCompleted`)
- [x] private Methode `RunOccasionAsync(RefreshArea, RefreshOccasion, Func<Task>)` — Z. 142–147 (OCE still, Exception → `RefreshFailed` nur bei sichtbarem Bereich)
- [x] private Methode `CancelArea(RefreshArea)` — Z. 149–158 (Token-Cancel + `CancelPending` des Bereichs-Refreshables)
- [x] Event `ResumeCompleted` (`EventHandler<RefreshAreaEventArgs>`) — Z. 45
- [x] Event `RefreshFailed` (`EventHandler<RefreshFailedEventArgs>`) — Z. 48
- [x] Interne Handler: Konstruktor-Abo `foreground.PropertyChanged` (aktiv → `ResumeAsync` je sichtbarem Bereich; inaktiv → `CancelArea` je sichtbarem Bereich) — Z. 40, 160–170; `favorites.PropertyChanged` (`Cards` → `CardsAdded` mit Guard `visible && IsActive && !IsResuming`) — Z. 41, 172–181
- [x] `RunBackgroundAsync` auf `favorites.RefreshForOccasionAsync(ForegroundReturn, …)` umgestellt — Z. 105
- [x] `SetActive` äußerlich unverändert (Hintergrund-Handoff + `favorites.CancelPending()` vor `IsActive`) — Z. 81–89

### `FavoriteHomeViewModel` (`FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`)

- [x] `RefreshForOccasionAsync(RefreshOccasion, RefreshFreshness, CancellationToken)` öffentlich — Z. 106–116 (Startup/CardsAdded → `true` via `Report`, ForegroundReturn → bisheriger Erfolgswert)
- [x] `RefreshMissingAsync`/`RefreshAtStartupAsync`/`RefreshStaleAsync` → `private`, Logik unverändert — Z. 120, 124, 173

### `HomePage` (`FlowNRW/HomePage.cs`)

- [x] Konstruktor +`RefreshLifecycle`, −`RefreshFreshness` — Z. 32–33
- [x] `resume`-Feld, `RefreshMissingAsync`/`RefreshAtStartupAsync`, `RefreshMissing`-Zeile im `Cards`-Handler und Gate/CTS/Dispatch in `ForegroundChanged` entfallen
- [x] `OnAppearing`: `RefreshFailed`/`ResumeCompleted`-Abos (Z. 106–107) und `_ = lifecycle.NotifyVisibleAsync(RefreshArea.Favorites)` an der Stelle des bisherigen Startup-Refreshs, nach `settings.LoadAsync`/`model.LoadAsync`/`RenderCards`/`ReconcileRefreshLoops` (Z. 116)
- [x] `OnDisappearing`: `lifecycle.NotifyHidden(Favorites)` + Event-Abmeldung (Z. 127–131), `appearance++`/`active=false`/`ReconcileRefreshLoops` erhalten
- [x] `ForegroundChanged` → nur `ReconcileRefreshLoops()` (Z. 141); `resume is null` → `!lifecycle.IsResuming(Favorites)` (Z. 161)
- [x] `LifecycleRefreshFailed` mit Occasion→Title-Mapping (Startup→„Abfahrten konnten nicht aktualisiert werden", CardsAdded→„Abfahrten konnten nicht geladen werden", sonst „Aktualisierung fehlgeschlagen") — Z. 148–157; `LifecycleResumeCompleted` mit `Area == Favorites`-Filter — Z. 143–146

### `DeparturePage` (`FlowNRW/DeparturePage.cs`)

- [x] Konstruktor +`RefreshLifecycle`, −`RefreshFreshness`; `foreground` bleibt — Z. 31–32
- [x] `resume`-Feld und Gate/CTS/Dispatch in `ForegroundChanged` entfallen → nur `ReconcileRefreshLoop()` (Z. 180)
- [x] `OnAppearing`: `_ = lifecycle.NotifyVisibleAsync(RefreshArea.Monitor)` nach `active = true` (Z. 110); `RefreshFailed`/`ResumeCompleted`-Abos (Z. 111–112)
- [x] `OnDisappearing`: `lifecycle.NotifyHidden(Monitor)` + Event-Abmeldung (Z. 130–132); `refreshLoop.Stop()` erhalten (Z. 135)
- [x] `ReconcileRefreshLoop`: `resume is null` → `!lifecycle.IsResuming(Monitor)` (Z. 194); `LifecycleRefreshFailed` → `Title = "Aktualisierung fehlgeschlagen"` bei `Area == Monitor` (Z. 187–190)

### `JourneyDetailPage` / `ResultsPage` (`FlowNRW/`)

- [x] Konstruktoren +`RefreshLifecycle`, −`ForegroundState`, −`RefreshFreshness` (`JourneyDetailPage.cs` Z. 20–21, `ResultsPage.cs` Z. 21–22)
- [x] `active`-Feld, `ForegroundChanged`, `ResumeAsync`, `foreground.PropertyChanged`-Abo entfallen
- [x] `OnAppearing`: `await lifecycle.NotifyVisibleAsync(RefreshArea.Journeys)` nach `settings.LoadAsync` (`JourneyDetailPage.cs` Z. 66–67, `ResultsPage.cs` Z. 63–64); `RefreshFailed`-Abo mit `Area == Journeys`-Filter → `Title = "Aktualisierung fehlgeschlagen"`
- [x] `OnDisappearing`: `lifecycle.NotifyHidden(Journeys)` + Event-Abmeldung (`JourneyDetailPage.cs` Z. 73–74, `ResultsPage.cs` Z. 70–71)

### Tests

- [x] `LifecycleFixture` um `Monitor`/`MonitorService` (`ControlledDepartureService` + `DepartureTestNavigation` + `ControlledSearchService`), `Journeys`/`Routing`/`JourneyNavigation` (`ControlledRoutingService` + `RecordingNavigation`) sowie `SeedMonitorAsync`/`SeedJourneysAsync` erweitert — `RefreshLifecycleTests_Background.cs` Z. 152–200; `RefreshLifecycle`-Ctor-Aufruf angepasst (Z. 175)
- [x] `FavoriteTests` auf `RefreshForOccasionAsync(CardsAdded, …)` umgestellt — `FavoriteTests.cs` Z. 78, 88
- [x] `FavoriteDepartureCacheIntegrationTests` auf `RefreshForOccasionAsync(Startup, …)` umgestellt — `FavoriteDepartureCacheIntegrationTests.cs` Z. 23, 59
- [x] `RefreshLifecycleTests_Occasions` neu — 6/6 geplante Tests vorhanden (`NotifyVisible_Favorites_Active_StartsFullRefresh`, `NotifyVisible_Monitor_Active_IssuesNoRequest`, `NotifyVisible_Journeys_Gated_StaleSessionRefreshes`, `CardsChanged_VisibleAndActive_RefreshesOnlyMissing`, `CardsChanged_ResumeRunning_DoesNotStart`, `ResumeFailed_Event_CarriesAreaAndOccasion`)
- [x] `RefreshLifecycleTests_Foreground` neu — 4/4 geplante Tests vorhanden
- [x] `RefreshLifecycleTests_Overlap` neu — 3/3 geplante Tests vorhanden
- [x] `RefreshLifecycleTests_Cancellation` neu — 2/2 geplante Tests vorhanden

### Sonstiges

- [x] `MauiProgram` ohne Registrierungsänderung (nicht im Diff; `AddSingleton<RefreshLifecycle>` löst den erweiterten Konstruktor auf)
- [x] `App.xaml.cs` unverändert (`lifecycle.SetActive` weiterhin der Fenster-Einstieg)
- [x] Keine Resume-Anbindung an `SearchPage`/`StopSearchPage`/`MapPage`/`LocationPickerPage` (Scope-Ausschluss 1 eingehalten)

## Offene Aufgaben

Keine — alle Implementierungs-Tasks (1–17) sind umgesetzt. Task 18 (Build/`dotnet test`/`WindowsLifecycleUiTests.ps1`-Regression) ist die Verifikationsaufgabe der Abnahme und verbleibt beim Parent.

## Hinweise

- **Verifikation im Review:** `dotnet build FlowNRW.Tests` → 0 Warnungen/0 Fehler; `dotnet test --filter FullyQualifiedName~RefreshLifecycle` → 25/25 grün (10 Background + 15 neue Tests).
- **Dokumentierte Abweichung 1 (bewertet, kein Mangel):** `ResumeFailed_Event_CarriesAreaAndOccasion` (`RefreshLifecycleTests_Occasions.cs` Z. 97–100) provoziert den Fehler über einen werfenden `PropertyChanged`-Subscriber am Monitor statt über einen Provider-Fehler — da `StopMonitorViewModel.RefreshIfStaleAsync` Exceptions intern schluckt, ist das der pragmatische Weg, den `RefreshFailed`-Pfad zu treiben.
- **Dokumentierte Abweichung 2 (bewertet, kein Mangel):** `CardsChanged_VisibleAndActive_RefreshesOnlyMissing` (Z. 63) nutzt `DateTimeOffset.Now` statt `fixture.Clock.Now`, damit die Karte gegenüber der produktiven `RefreshFreshness`-Policy frisch bleibt.
- **Plan-check-Hinweis eingehalten:** `refreshLoop.Stop()` in `DeparturePage.OnDisappearing` (Z. 135) erhalten; Event-Abmeldung der transienten Pages (`DeparturePage`/`JourneyDetailPage`/`ResultsPage`) sowie der Singleton-`HomePage` in `OnDisappearing` vorhanden.
- **Beobachtung (plankonform):** `CancelArea` entfernt den Bereich nicht aus `visible` beim `SetActive(false)`-Pfad — entspricht dem Plan (nur Abbruch, Bereich bleibt sichtbar-registriert). `ResumeCompleted` feuert bei abgebrochenen Resumes nicht, da `NotifyHidden`/`CancelArea` den Eintrag bereits entfernt hat — exakt die geplante `ReferenceEquals`-Semantik.
- Uncommittete Änderungen decken sich vollständig mit dem Branch-Diff-Umfang; keine planfremden Produktivänderungen.
