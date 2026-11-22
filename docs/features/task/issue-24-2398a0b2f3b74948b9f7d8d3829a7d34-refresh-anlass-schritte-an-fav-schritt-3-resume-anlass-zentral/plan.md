# Umsetzungsplan: Schritt 3 — Vordergrund-Aktualisierung und Anlass-Zuordnung zentralisieren

## Übersicht

Die vier Resume-Kompositionen (`HomePage`, `DeparturePage`, `JourneyDetailPage`, `ResultsPage`)
werden in den `RefreshLifecycle` zentralisiert: Pages melden künftig nur noch `NotifyVisible`/
`NotifyHidden` je Anzeige-Bereich, während der Lebenszyklus das
`IsLoaded && IntervalSeconds != 0`-Gate, den Overlap-Guard pro Bereich, den Abbruch beim
Verdecken und die Anlass→Schritt-Zuordnung (Startup → vollständig, neue Karten → fehlende,
Vordergrund → veraltet) kapselt. Die öffentliche Triplik am `FavoriteHomeViewModel` geht hinter
einer anlassbezogenen Fassade `RefreshForOccasionAsync` auf; `JourneyDetailPage`/`ResultsPage`
erhalten dabei erstmals denselben Overlap-/Abbruch-Schutz (beabsichtigte Verhaltensänderung, B2).

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Anzeige-Bereich (`RefreshArea`: `Favorites`, `Monitor`, `Journeys`) mit eigenem Resume-Token; Refresh-Anlass (`RefreshOccasion`: `Startup`, `CardsAdded`, `ForegroundReturn`) |
| Auslösende Ereignisse | `NotifyVisibleAsync`/`NotifyHidden` aus `OnAppearing`/`OnDisappearing`; `SetActive(bool)` aus dem Fenster-Lebenszyklus (`App.xaml.cs`, unverändert); `favorites.PropertyChanged(nameof(Cards))` intern im Lifecycle |
| Erwartete Wirkung | Veraltete Daten des sichtbaren Bereichs werden geschützt erneuert; laufende Resumes werden beim Verdecken/Deaktivieren abgebrochen; Intervall-Schleifen bleiben seitenverantwortlich |
| Umgang mit fehlenden Objekten | Sichtbarkeit je Bereich: durch `NotifyVisible`/`NotifyHidden` gepflegt (erzeugen/verwerfen der Registrierung); Resume-`CancellationTokenSource`: je Resume erzeugt und nach Ende/Abbruch verworfen; Monitor-/Journey-Refreshables: Singletons aus DI (wiederverwenden) |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | `SearchPage`, `StopSearchPage`, `MapPage`, `LocationPickerPage` werden nicht an den zentralen Resume-Einstieg angebunden. | Diese Seiten haben heute kein Resume-Refresh (nur Hidden-`CancelPending`); Verhaltensneutralität ist ausdrücklich gefordert. | bestätigt |
| 2 | `RunBackgroundAsync`, `RefreshLoop`-Steuerung und die fachlichen Refresh-Schritte (`card.RefreshAutomaticallyAsync`, `RefreshCoreAsync`, `Session.RefreshIfStaleAsync`-Inhalt) bleiben unverändert. | Explizites Nicht-Ziel der Anforderung; nur Komposition und Nebenbedingungen werden zentralisiert. | bestätigt |
| 3 | `JourneyDetailPage` und `ResultsPage` teilen sich den Bereich `Journeys` (und damit ein Resume-Token). | Beide Pages arbeiten auf derselben `JourneySearchViewModel`-Singleton-Session und sind nie gleichzeitig sichtbar; ein gemeinsamer Token ist die nächstliegende Schutz-Semantik. | bestätigt |
| 4 | Kein neuer UI-/E2E-Test für den Overlap-Guard der Journey-Pages. | Überlappende Resumes sind über die UI nicht deterministisch auslösbar; der Schutz wird auf Core-Ebene nachgewiesen. Die bestehende Suite `WindowsLifecycleUiTests.ps1` (`LifecycleActive`-Fixture → `SetActive`) dient als E2E-Regression. | bestätigt |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Zentrale Stelle | Erweiterung von `RefreshLifecycle` um `NotifyVisibleAsync`/`NotifyHidden`/`IsResuming` + internes `ResumeAsync` | `RefreshLifecycle` besitzt bereits `ForegroundState`, `RefreshSettingsViewModel`, `FavoriteHomeViewModel`, `RefreshFreshness` und ist der DI-Singleton-Einstieg für `SetActive`/`RunBackgroundAsync` — kein neuer Service nötig, keine neue Registrierung. |
| Anbindung Monitor/Journeys | Direkte Konstruktor-Injektion von `StopMonitorViewModel` und `JourneySearchViewModel` in `RefreshLifecycle`; Bereich→Refreshable-Zuordnung per `RefreshArea`-Switch | Geprüft in `MauiProgram.cs`: `StopMonitorViewModel` (Z. 101), `JourneySearchViewModel` (Z. 90), `ResultsViewModel`/`JourneyDetailViewModel` (Z. 94/95, beide halten dieselbe Session) und `FavoriteHomeViewModel` (Z. 157) sind allesamt **Singletons** — eine Registrierung mit Delegaten wäre unnötige Indirektion; Core-Referenzen bleiben MAUI-frei (alle in `FlowNRW.Core`). |
| Overlap-Semantik | Pro Bereich ein Resume-`CancellationTokenSource` (`Dictionary<RefreshArea, CancellationTokenSource>`), Guard per `ReferenceEquals` im `finally` | Entspricht dem bisherigen Verhalten (`resume`-CTS je Page bei `HomePage`/`DeparturePage`) und liefert den B2-Schutz für `Journeys`; ein globales Token würde Resumes verschiedener Bereiche unnötig serialisieren — eine neue Einschränkung, die nicht gefordert ist. |
| Anlass-Auslöser | `ForegroundState.PropertyChanged` und `favorites.PropertyChanged(nameof(Cards))` werden im `RefreshLifecycle`-Konstruktor abonniert; Pages melden nur Sichtbarkeit | Damit liegt auch die Trigger-Quelle (Cards-Changed → `CardsAdded`) zentral; `HomePage` verliert sämtliches Anlass-Wissen. |
| Favoriten-Anlass-Fassade | `FavoriteHomeViewModel.RefreshForOccasionAsync(RefreshOccasion, RefreshFreshness, CancellationToken)` (Service-Layer-Fassade); die drei bisherigen Schritte werden private | Bündelt B4: Aufrufer wählen nur noch den Anlass, nicht den Schritt; `RefreshFreshness` bleibt Parameter (Tests injizieren abweichende Policies; produktiv identischer Singleton). `RunBackgroundAsync` ruft die Fassade mit `ForegroundReturn` — verhaltensgleich. |
| Fehler→Title-Pfad | `RefreshLifecycle.RefreshFailed`-Event (`RefreshArea` + `RefreshOccasion`), ausgelöst nur solange der Bereich sichtbar ist; Pages abonnieren und setzen ihren `Title` | `Title` ist MAUI-Typ und darf nicht nach Core; das Event hält den bisherigen Fehlerpfad gleichwertig (HomePage: drei anlassspezifische Texte; übrige: „Aktualisierung fehlgeschlagen") und ersetzt den `version == appearance`-Guard durch „nur sichtbare Bereiche erhalten Events". |
| Schleifen-Nachlauf | `RefreshLifecycle.ResumeCompleted`-Event (`RefreshArea`); `HomePage`/`DeparturePage` reconcilen daraufhin ihre `RefreshLoop`s | Heute reconcilen die Pages im `finally` ihres eigenen Resume-Aufrufs; das wandert in den Lifecycle, also muss das Ende signalisiert werden — `IsResuming` + `ResumeCompleted` erhält die `resume is null`-Bedingung der Schleifen exakt. |
| Sichtbar-Zeitpunkt Favorites | `NotifyVisibleAsync(Favorites)` erst nach `settings.LoadAsync` + `model.LoadAsync` an der Stelle des bisherigen `RefreshAtStartupAsync`-Aufrufs | `RefreshAtStartup` wirkt nur auf geladene Karten; früheres Melden würde den Startup-Anlass leer laufen lassen. Ein Foreground-Resume mitten im Ladevorgang entfällt — der Startup-Anlass (alle Karten) subsumiert den Stale-Anlass fachlich. |

## Programmabläufe

### Sichtbar-Meldung und Bereichs-Anlass beim Erscheinen

1. `HomePage.OnAppearing` (nach `settings.LoadAsync`, `model.LoadAsync`, `RenderCards`,
   `ReconcileRefreshLoops`) ruft `_ = lifecycle.NotifyVisibleAsync(RefreshArea.Favorites)`.
2. `NotifyVisibleAsync` trägt den Bereich in `visible` ein und startet den Bereichs-Anlass:
   `Favorites` → bei `foreground.IsActive` `favorites.RefreshForOccasionAsync(Startup, freshness)`;
   `Monitor` → kein Anlass (verhaltensgleich, `OpenStopAsync` lädt bereits);
   `Journeys` → `ResumeAsync(Journeys)` (gated).
3. `DeparturePage.OnAppearing` meldet `_ = lifecycle.NotifyVisibleAsync(RefreshArea.Monitor)`
   direkt nach `active = true`; `JourneyDetailPage`/`ResultsPage` rufen
   `await lifecycle.NotifyVisibleAsync(RefreshArea.Journeys)` an der Stelle des bisherigen
   `ResumeAsync`-Aufrufs (nach `settings.LoadAsync`).
4. Fehler des Anlass-Tasks werden intern gefangen; ist der Bereich noch sichtbar, wird
   `RefreshFailed` mit Bereich + Anlass ausgelöst.

Beteiligte Klassen: `RefreshLifecycle`, `FavoriteHomeViewModel`, `JourneySearchViewModel`,
`HomePage`, `DeparturePage`, `JourneyDetailPage`, `ResultsPage`.

### Vordergrund-Rückkehr (geschützter Einstieg)

1. `App.xaml.cs` ruft weiterhin `lifecycle.SetActive(true)` (unverändert).
2. `SetActive` führt den bestehenden Handoff aus (Hintergrund-`Cancel`,
   `favorites.CancelPending()`) und setzt `foreground.IsActive = true`.
3. Der interne `PropertyChanged`-Handler des Lifecycle startet für jeden Bereich in `visible`
   `ResumeAsync(area)` (fire-and-forget; Fehler intern gefangen).
4. `ResumeAsync(area)` Gate: `!visible.Contains(area) || !foreground.IsActive ||
   !settings.IsLoaded || settings.IntervalSeconds == 0 || resume.ContainsKey(area)` → Rückkehr.
5. Sonst: `CancellationTokenSource` in `resume[area]` eintragen und dispatch:
   `Favorites` → `favorites.RefreshForOccasionAsync(ForegroundReturn, freshness, token)`;
   `Monitor` → `monitor.RefreshIfStaleAsync(freshness, token)`;
   `Journeys` → `journeys.RefreshIfStaleAsync(freshness, token)`.
6. `finally`: bei `ReferenceEquals(resume[area], source)` Eintrag entfernen und
   `ResumeCompleted(area)` auslösen → `HomePage`/`DeparturePage` reconcilen ihre Schleifen.

Beteiligte Klassen: `RefreshLifecycle`, `ForegroundState`, `RefreshSettingsViewModel`,
`FavoriteHomeViewModel`, `StopMonitorViewModel`, `JourneySearchViewModel`, `RefreshAreaEventArgs`.

### Neue Favoriten-Karten (Anlass CardsAdded)

1. `FavoriteHomeViewModel` meldet `PropertyChanged(nameof(Cards))` (z. B. nach `ToggleAsync`).
2. Der interne Lifecycle-Handler prüft `visible(Favorites) && foreground.IsActive &&
   !IsResuming(Favorites)` und startet
   `favorites.RefreshForOccasionAsync(CardsAdded, freshness)` (fire-and-forget).
3. `HomePage` rendert in seinem eigenen `model.PropertyChanged`-Handler weiterhin nur
   `RenderCards()` + `ReconcileRefreshLoops()` — kein Refresh-Aufruf mehr.

Beteiligte Klassen: `RefreshLifecycle`, `FavoriteHomeViewModel`, `HomePage`.

### Verdecken und Deaktivieren (Abbruch)

1. `OnDisappearing` jeder Page ruft `lifecycle.NotifyHidden(area)`.
2. `NotifyHidden` entfernt den Bereich aus `visible`, cancelt einen laufenden Resume-Token des
   Bereichs und ruft `CancelPending()` des zugeordneten Refreshables
   (`favorites`/`monitor`/`journeys`) auf.
3. Bei `SetActive(false)`: nach `foreground.IsActive = false` reagiert der interne Handler —
   für jeden sichtbaren Bereich Resume-Token canceln und `CancelPending()` aufrufen
   (gleichwertig zu den bisherigen Page-Hidden-Pfaden).
4. `JourneyDetailPage`/`ResultsPage` erhalten damit erstmals Token-Abbruch für
   `Session.RefreshIfStaleAsync` (Signatur akzeptiert bereits einen `CancellationToken`).

Beteiligte Klassen: `RefreshLifecycle`, alle vier Pages.

### Fehlerpfad

1. `ResumeAsync`/Anlass-Runner fangen `OperationCanceledException` (still) und `Exception`.
2. Bei `Exception` und noch sichtbarem Bereich: `RefreshFailed(area, occasion)` auslösen.
3. `HomePage` mappt `Startup` → „Abfahrten konnten nicht aktualisiert werden",
   `CardsAdded` → „Abfahrten konnten nicht geladen werden",
   `ForegroundReturn` → „Aktualisierung fehlgeschlagen" auf `Title`; die übrigen Pages setzen
   bei eigenem Bereich `Title = "Aktualisierung fehlgeschlagen"`.

Beteiligte Klassen: `RefreshLifecycle`, `RefreshFailedEventArgs`, alle vier Pages.

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `RefreshArea` | Enum (`Favorites`, `Monitor`, `Journeys`) | Geschlossene Menge der anbindbaren Anzeige-Bereiche |
| `RefreshOccasion` | Enum (`Startup`, `CardsAdded`, `ForegroundReturn`) | Zentrale Anlass-Menge für Fassade und Fehler-Event |
| `RefreshAreaEventArgs` | EventArgs-Klasse | `Area`-Nutzlast für `ResumeCompleted` |
| `RefreshFailedEventArgs` | EventArgs-Klasse | `Area` + `Occasion` für `RefreshFailed` (Page mappt auf Title-Text) |

## Änderungen an bestehenden Klassen

### `RefreshLifecycle` (Service / Singleton, `FlowNRW.Core/Refresh`)

- **Neue Konstruktor-Parameter:** `StopMonitorViewModel monitor`, `JourneySearchViewModel journeys` — Bereichs-Refreshables (alle DI-Singletons, keine Zyklen).
- **Neue Felder:** `HashSet<RefreshArea> visible`, `Dictionary<RefreshArea, CancellationTokenSource> resume`.
- **Neue Methoden:** `Task NotifyVisibleAsync(RefreshArea area)` — Sichtbar-Meldung + Bereichs-Anlass; `void NotifyHidden(RefreshArea area)` — Abmeldung + Abbruch + `CancelPending`; `bool IsResuming(RefreshArea area)` — Schleifen-Abfrage; `private Task ResumeAsync(RefreshArea area)` — der eine geschützte Gate-/Overlap-/Dispatch-Einstieg; `private async Task RunOccasionAsync(RefreshArea, RefreshOccasion, Func<Task>)` — Fehler→`RefreshFailed`-Runner.
- **Neue Events:** `EventHandler<RefreshAreaEventArgs>? ResumeCompleted` — nach Abschluss eines gestarteten Resumes; `EventHandler<RefreshFailedEventArgs>? RefreshFailed` — Anlass-/Resume-Fehler solange der Bereich sichtbar ist.
- **Neue interne Handler:** Konstruktor abonniert `foreground.PropertyChanged` (aktiv → Resume je sichtbarem Bereich; inaktiv → Abbruch + `CancelPending` je sichtbarem Bereich) und `favorites.PropertyChanged` (`Cards` → `CardsAdded`-Anlass mit Guard).
- **Geänderte Methoden:** `RunBackgroundAsync` — Aufruf `favorites.RefreshStaleAsync(freshness, token)` wird zu `favorites.RefreshForOccasionAsync(ForegroundReturn, freshness, token)` (verhaltensgleicher Fassadenaufruf). `SetActive` bleibt äußerlich unverändert.

### `FavoriteHomeViewModel` (`FlowNRW.Core/Favorites`)

- **Neue Methoden:** `Task<bool> RefreshForOccasionAsync(RefreshOccasion occasion, RefreshFreshness freshness, CancellationToken cancellationToken = default)` — Anlass-Fassade; dispatcht intern auf die drei Schritte (`Startup`/`CardsAdded` liefern `true`, `ForegroundReturn` den bisherigen Erfolgswert).
- **Geänderte Methoden (Sichtbarkeit):** `RefreshMissingAsync`, `RefreshAtStartupAsync`, `RefreshStaleAsync` → `private` (als Schritt-Implementierungen hinter der Fassade, Logik unverändert).
- Keine sonstigen Änderungen (LoadAsync, Nearby, Persistenz, `CancelPending` bleiben).

### `HomePage` (`FlowNRW`)

- **Geänderte Signatur:** Konstruktor + `RefreshLifecycle lifecycle`, − `RefreshFreshness freshness`.
- **Entfällt:** Feld `resume`; private `RefreshMissingAsync`/`RefreshAtStartupAsync`; die `RefreshMissing`-Zeile im `Cards`-Handler; Gate/CTS/Dispatch in `ForegroundChanged`.
- **OnAppearing:** `_ = lifecycle.NotifyVisibleAsync(RefreshArea.Favorites)` an Stelle von `_ = RefreshAtStartupAsync(version)`; `lifecycle.RefreshFailed`/`ResumeCompleted` abonnieren.
- **OnDisappearing:** `lifecycle.NotifyHidden(Favorites)` ersetzt `resume.Cancel` + `model.CancelPending`; Events abmelden.
- **ForegroundChanged:** reduziert auf `ReconcileRefreshLoops()` (beide Richtungen).
- **ReconcileRefreshLoops:** `resume is null` → `!lifecycle.IsResuming(Favorites)`; `ResumeCompleted`-Handler reconciliert bei `Area == Favorites`.
- **Neue Handler:** `LifecycleRefreshFailed` (Occasion→Title-Mapping oben), `LifecycleResumeCompleted` → `ReconcileRefreshLoops()`.
- `appearance`-Version, `active`, `settings.Changed`-Abo und `CancelPending`-freie Logik bleiben.

### `DeparturePage` (`FlowNRW`)

- **Geänderte Signatur:** Konstruktor + `RefreshLifecycle lifecycle`, − `RefreshFreshness freshness` (nur für den bisherigen Resume-Aufruf genutzt; `foreground` bleibt wegen `RefreshForeground`-Fixture-Bindung und Loop-Reconcile).
- **Entfällt:** Feld `resume`; Gate/CTS/Dispatch in `ForegroundChanged` → nur `ReconcileRefreshLoop()`.
- **OnAppearing:** `_ = lifecycle.NotifyVisibleAsync(RefreshArea.Monitor)` nach `active = true`; `ResumeCompleted`/`RefreshFailed` abonnieren.
- **OnDisappearing:** `lifecycle.NotifyHidden(Monitor)` ersetzt `resume.Cancel` + `model.CancelPending`.
- **ReconcileRefreshLoop:** `resume is null` → `!lifecycle.IsResuming(Monitor)`.

### `JourneyDetailPage` / `ResultsPage` (`FlowNRW`)

- **Geänderte Signatur:** Konstruktor + `RefreshLifecycle lifecycle`, − `ForegroundState foreground`, − `RefreshFreshness freshness` (beide nur für den alten Resume-Pfad benötigt).
- **Entfällt:** `active`-Feld, `ForegroundChanged`, `ResumeAsync`, `foreground.PropertyChanged`-Abo.
- **OnAppearing:** `await lifecycle.NotifyVisibleAsync(RefreshArea.Journeys)` an der bisherigen `await ResumeAsync()`-Stelle; `RefreshFailed` abonnieren (`Area == Journeys` → `Title = "Aktualisierung fehlgeschlagen"`).
- **OnDisappearing:** `lifecycle.NotifyHidden(Journeys)` ersetzt `model.Session.CancelPending()`; Event abmelden.

### `MauiProgram` (`FlowNRW`)

- Keine Registrierungsänderung nötig: `AddSingleton<RefreshLifecycle>()` löst den erweiterten
  Konstruktor auf; `StopMonitorViewModel`/`JourneySearchViewModel`/`FavoriteHomeViewModel` sind
  bereits Singletons. (Seiten-Signaturen werden per DI aufgelöst.)

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine — keine neuen Eingaben an Schnittstellen.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **`LifecycleFixture`/Testkonstruktion:** `RefreshLifecycle`-Konstruktor wächst um zwei Pflichtparameter — `LifecycleFixture` und jede direkte Instanziierung muss `StopMonitorViewModel` (mit `ControlledDepartureService`/`ControlledSearchService`/`DepartureTestNavigation`) und `JourneySearchViewModel` (mit `ControlledRoutingService`/`RecordingNavigation`) übergeben.
- **Startup-Anlass deckt mid-load-Aktivierung ab:** Ein Vordergrund-Ereignis während des Home-Ladevorgangs startet kein separates Stale-Resume mehr; der anschließende Startup-Anlass aktualisiert ohnehin alle Karten — bewusste, fachlich abgedeckte Vereinfachung.
- **Journey-Überlappungsschutz neu:** `JourneyDetail`/`Results` doppelte Resume-Auslösungen werden jetzt unterdrückt — gewünschte B2-Änderung; `Session.RefreshIfStaleAsync` erhält erstmals ein abbrechbares Token.
- **Event-Reihenfolge:** Der Lifecycle-Handler (Ctor-Abo) läuft vor den Page-Handlern bei `IsActive`-Wechseln — gewollt, damit `IsResuming` bei der Schleifen-Reconcile bereits stimmt.
- **`RefreshFailed`-Zustellung:** Nur sichtbare Bereiche erhalten Events — ein verdeckter Bereich mit noch laufendem (nicht abbrechbarem) Anlass meldet keinen Fehler mehr (entspricht dem bisherigen `version`-Guard).

## Umsetzungsreihenfolge

1. **Neue Typen anlegen:** `RefreshArea`, `RefreshOccasion`, `RefreshAreaEventArgs`, `RefreshFailedEventArgs` in `FlowNRW.Core/Refresh/`.
   - Voraussetzungen: Keine.
2. **`FavoriteHomeViewModel`: Anlass-Fassade einführen** — `RefreshForOccasionAsync` hinzufügen, Triplik auf `private` setzen.
   - Voraussetzungen: Schritt 1 (`RefreshOccasion`).
3. **`RefreshLifecycle` erweitern** — Konstruktor-Deps, `visible`/`resume`-Zustand, interne Subscriptions, `NotifyVisibleAsync`/`NotifyHidden`/`IsResuming`/`ResumeAsync`, `ResumeCompleted`/`RefreshFailed`, `RunBackgroundAsync` auf Fassade umstellen.
   - Voraussetzungen: Schritte 1–2.
4. **Pages umstellen** — `HomePage`, `DeparturePage`, `JourneyDetailPage`, `ResultsPage` auf Sichtbar-Meldungen + Event-Abos; alte Resume-Felder/-Methoden entfernen.
   - Voraussetzungen: Schritt 3.
5. **Test-Fixture erweitern + bestehende Tests anpassen** — `LifecycleFixture` (neue Lifecycle-Deps), `FavoriteTests`, `FavoriteDepartureCacheIntegrationTests` auf Fassade.
   - Voraussetzungen: Schritte 2–3.
6. **Neue Lifecycle-Testklassen schreiben** — `RefreshLifecycleTests_Foreground`, `_Overlap`, `_Cancellation`, `_Occasions`.
   - Voraussetzungen: Schritte 3, 5.
7. **Build + `dotnet test` + E2E-Regression** (`WindowsLifecycleUiTests.ps1` im UiTest-Build).
   - Voraussetzungen: Schritte 4–6.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `NotifyVisible_Favorites_Active_StartsFullRefresh` | `RefreshLifecycleTests_Occasions` | Startup-Anlass aktualisiert alle nicht-busy Karten (Provideraufruf je Karte) |
| `NotifyVisible_Monitor_Active_IssuesNoRequest` | `RefreshLifecycleTests_Occasions` | Monitor-Sichtbarmeldung löst keinen Provideraufruf aus (verhaltensgleich) |
| `NotifyVisible_Journeys_Gated_StaleSessionRefreshes` | `RefreshLifecycleTests_Occasions` | Journey-Appearing nutzt denselben Gate-Einstieg (`RouteAsync` nur bei stale) |
| `CardsChanged_VisibleAndActive_RefreshesOnlyMissing` | `RefreshLifecycleTests_Occasions` | `Cards`-PropertyChanged → nur stale/fehlende Karten werden geladen |
| `CardsChanged_ResumeRunning_DoesNotStart` | `RefreshLifecycleTests_Occasions` | CardsAdded-Guard respektiert `IsResuming` |
| `ResumeFailed_Event_CarriesAreaAndOccasion` | `RefreshLifecycleTests_Occasions` | `RefreshFailed` mit korrekten Args; bei verdecktem Bereich kein Event |
| `SetActive_VisibleArea_ResumesStale` | `RefreshLifecycleTests_Foreground` | Aktivierung → genau ein Stale-Resume des sichtbaren Bereichs (je Area: Favorites/Monitor/Journeys) |
| `SetActive_IntervalZero_DoesNotResume` | `RefreshLifecycleTests_Foreground` | `IntervalSeconds == 0` unterdrückt Resume |
| `SetActive_SettingsNotLoaded_DoesNotResume` | `RefreshLifecycleTests_Foreground` | `IsLoaded == false` unterdrückt Resume |
| `SetActive_AreaNotVisible_DoesNotResume` | `RefreshLifecycleTests_Foreground` | Kein sichtbarer Bereich → keine Requests |
| `Resume_Twice_SameArea_NoDuplicate` | `RefreshLifecycleTests_Overlap` | Laufender Resume + erneute Aktivierung → kein zweiter Provideraufruf |
| `Resume_DifferentAreas_RunConcurrently` | `RefreshLifecycleTests_Overlap` | Zwei sichtbare Bereiche resumen unabhängig (Per-Bereich-Token) |
| `IsResuming_ReflectsResumeState` | `RefreshLifecycleTests_Overlap` | `true` während, `false` nach Abschluss; `ResumeCompleted` feuert |
| `NotifyHidden_RunningResume_CancelsToken` | `RefreshLifecycleTests_Cancellation` | Provider-Token gecancelt; `CancelPending` des Refreshables aufgerufen; späte Antwort verworfen |
| `SetActiveFalse_RunningResume_CancelsAllVisible` | `RefreshLifecycleTests_Cancellation` | Deaktivierung bricht sichtbare Resumes ab |
| Erweiterung `LifecycleFixture` | (Hilfsmethode) | `Monitor` (`ControlledDepartureService` + `DepartureTestNavigation` + `ControlledSearchService`) und `Journeys` (`ControlledRoutingService` + `RecordingNavigation`) bereitstellen |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `RefreshLifecycleTests_Background` (alle Tests via `LifecycleFixture`) | `RefreshLifecycle`-Konstruktor erhält `monitor`/`journeys` — Fixture-Aufruf anpassen; Verhalten der Background-Tests bleibt |
| `FavoriteTests.BoardsRefreshIndependentlyAndRejectLateResponses` (Z. 77, 87) | `RefreshMissingAsync` privat → `RefreshForOccasionAsync(CardsAdded, freshness)` |
| `FavoriteDepartureCacheIntegrationTests` (Z. 23, 59) | `RefreshAtStartupAsync` privat → `RefreshForOccasionAsync(Startup, freshness)` |

### E2E-Tests (primärer Funktionsnachweis)

Keine neuen E2E-Tests: Der Schritt ändert keinen Benutzerfluss — Auslöser (Fenster-Aktivierung,
Seitenwechsel, Favorit hinzufügen) und sichtbare Wirkung (veraltete Daten werden erneuert) sind
unverändert. Die bestehende Suite `tests/WindowsJourneyUiTests/WindowsLifecycleUiTests.ps1`
deckt den Happy Path bereits ab: Sie treibt über das `LifecycleActive`-Fixture
`RefreshLifecycle.SetActive` deterministisch und prüft Resume für Favoriten (inkl. Overlap-
Nebenbedingung „slow favorite does not block another resume card"), Monitor, Results und
JourneyDetail (Fehler-Retention, frische Daten ohne neuen Request, Aus-Intervall).
Als Regression ist diese Suite im UiTest-Build auszuführen; der neue Overlap-Schutz der
Journey-Pages ist UI-seitig nicht deterministisch auslösbar und wird auf Core-Ebene nachgewiesen.

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| Keine — bestehende E2E-Tests bleiben unverändert und dienen als Regression. | — |

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path Vordergrund-Resume | Fenster wird aktiviert | Favoriten-Seite sichtbar, Intervall 60 s geladen, Karten-Daten veraltet | `SetActive(true)` → interner Handler → Gate ok → `RefreshForOccasionAsync(ForegroundReturn)` | Veraltete Karten aktualisieren sich; Schleifen starten nach `ResumeCompleted` neu | `RefreshLifecycleTests_Foreground` + `WindowsLifecycleUiTests` |
| 2 | Intervall „Aus" | Fenster wird aktiviert | Intervall 0 gespeichert/geladen | `SetActive(true)` → Gate `IntervalSeconds == 0` → kein Resume | Keine Provideraufrufe, Anzeige unverändert | `SetActive_IntervalZero_DoesNotResume` + UI-Test „Off suppresses automatic favorite resume" |
| 3 | Verdecken bricht ab | Nutzer navigiert weg | Resume des Bereichs läuft | `NotifyHidden` → Token-Cancel + `CancelPending` | Laufende Aktualisierung endet, späte Antwort wird verworfen | `NotifyHidden_RunningResume_CancelsToken` |
| 4 | Neuer Favorit | Nutzer speichert Favorit | Home sichtbar, Fenster aktiv, kein Resume | `Cards`-PropertyChanged → `CardsAdded`-Anlass | Nur die neue Karte lädt ihre Abfahrten | `CardsChanged_VisibleAndActive_RefreshesOnlyMissing` |
| 5 | Journey-Overlap-Schutz | Zwei schnelle Aktivierungen | JourneyDetail sichtbar, Session stale | Erster Resume läuft → zweiter `ResumeAsync(Journeys)` vom Gate abgelehnt | Genau eine `RouteAsync`-Anfrage | `Resume_Twice_SameArea_NoDuplicate` |
| 6 | Resume-Fehler | Provider-Fehler beim Resume | Monitor sichtbar | Exception → `RefreshFailed(Monitor, ForegroundReturn)` → Page setzt `Title` | „Aktualisierung fehlgeschlagen" als Seitentitel; behaltene Daten bleiben | `ResumeFailed_Event_CarriesAreaAndOccasion` |

## Offene Punkte

Keine.

## Tasks

| # | Bereich | Aufgabe | Status |
|---|---------|---------|--------|
| 1 | Core-Typen | `RefreshArea`-Enum anlegen | Offen |
| 2 | Core-Typen | `RefreshOccasion`-Enum anlegen | Offen |
| 3 | Core-Typen | `RefreshAreaEventArgs` + `RefreshFailedEventArgs` anlegen | Offen |
| 4 | Fassade | `FavoriteHomeViewModel.RefreshForOccasionAsync` einführen, Triplik privat setzen | Offen |
| 5 | Lifecycle | Konstruktor um `StopMonitorViewModel`/`JourneySearchViewModel` erweitern; `visible`/`resume`-Zustand | Offen |
| 6 | Lifecycle | Interne Subscriptions (`foreground.PropertyChanged`, `favorites.PropertyChanged(Cards)`) | Offen |
| 7 | Lifecycle | `NotifyVisibleAsync`/`NotifyHidden`/`IsResuming` + privates `ResumeAsync` + `RunOccasionAsync` | Offen |
| 8 | Lifecycle | `ResumeCompleted`/`RefreshFailed`-Events + `RunBackgroundAsync` auf Fassade umstellen | Offen |
| 9 | Pages | `HomePage` umstellen (Sichtbar-Meldung, Event-Abos, `resume`-Feld/Methoden entfernen) | Offen |
| 10 | Pages | `DeparturePage` umstellen | Offen |
| 11 | Pages | `JourneyDetailPage` + `ResultsPage` umstellen | Offen |
| 12 | Tests | `LifecycleFixture` um Monitor-/Journey-Doubles erweitern; `RefreshLifecycleTests_Background` anpassen | Offen |
| 13 | Tests | `FavoriteTests` + `FavoriteDepartureCacheIntegrationTests` auf `RefreshForOccasionAsync` umstellen | Offen |
| 14 | Tests | `RefreshLifecycleTests_Occasions` neu | Offen |
| 15 | Tests | `RefreshLifecycleTests_Foreground` neu | Offen |
| 16 | Tests | `RefreshLifecycleTests_Overlap` neu | Offen |
| 17 | Tests | `RefreshLifecycleTests_Cancellation` neu | Offen |
| 18 | Abnahme | Build, `dotnet test`, `WindowsLifecycleUiTests.ps1`-Regression | Offen |
