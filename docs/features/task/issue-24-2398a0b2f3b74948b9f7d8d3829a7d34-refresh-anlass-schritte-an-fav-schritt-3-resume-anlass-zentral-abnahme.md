# Abnahme – Schritt 3: Vordergrund-Resume und Anlass-Zuordnung im RefreshLifecycle zentralisieren

Datum: 2026-11-23
Geprüfter Stand: `task/issue-24-...-schritt-3-resume-anlass-zentral` (HEAD `7a25305`) gegen Basis `task/issue-24-...-refresh-anlass-schritte-an-fav` (`git diff <basis>...HEAD`, 21 Dateien, vollständig gesichtet).
Anforderung: Issues #22/#24, Befunde B2/B4 aus Review #19.

## Geprüfte Punkte

### Zentraler Einstieg (B2)

- `FlowNRW.Core/Refresh/RefreshLifecycle.cs`: `NotifyVisibleAsync(area, owner)` / `NotifyHidden(area, owner)` sind der einzige Einstieg; das Gate `visible && foreground.IsActive && settings.IsLoaded && IntervalSeconds != 0` existiert genau einmal (Z. 122–123, `ResumeAsync`). Die fünf früheren Kopien in den Pages sind entfernt.
- Overlap-Guard: `resume`-Dictionary pro Bereich mit `CancellationTokenSource`; `IsResuming` ersetzt die seiten-seitigen `resume`-Felder. Doppelte `NotifyVisibleAsync`/Aktivierungen erzeugen keine Doppel-Requests (`RefreshLifecycleTests_Overlap`).
- Owner-Semantik: `NotifyHidden` entfernt die Registrierung nur bei `ReferenceEquals(current, owner)` — Appearing-vor-Disappearing beim Push Results→JourneyDetail bleibt registriert; fremdes Hidden cancelt nichts (`RefreshLifecycleTests_Ownership`, 3 Tests).
- Abbruch: `CancelArea` bricht den laufenden Resume-Token und ruft `CancelPending` des Bereichs-ViewModels auf — bei `NotifyHidden` und bei `foreground.IsActive == false` (`ForegroundChanged`, intern abonniert). `RefreshLifecycleTests_Cancellation` belegt Token-Abbruch und verworfene Spätantworten.

### Vier Resume-Pfade

- Favoriten: `favorites.RefreshForOccasionAsync(ForegroundReturn, freshness, token)` — früher `RefreshStaleAsync`, gleiche Semantik.
- Monitor: `monitor.RefreshIfStaleAsync(freshness, token)` — wie bisher (DeparturePage hatte bereits Guard+Token), jetzt zentral.
- Journeys: `journeys.RefreshIfStaleAsync(freshness, token)` — der CancellationToken wird jetzt **erstmalig durchgereicht** (JourneyDetailPage/ResultsPage riefen vorher ohne Token, ungeschützt). Geteilter `RefreshArea.Journeys`-Bereich mit Owner-Wechsel zwischen ResultsPage und JourneyDetailPage — verbessert das alte Verhalten (früher cancelte das Disappearing der ResultsPage den laufenden Session-Request des Detail-Pushes).
- Favoriten-Anlässe: `Startup` (OnAppearing nach Load, nur wenn `foreground.IsActive` — wie bisher ohne Intervall-Gate) und `CardsAdded` (`Cards`-PropertyChanged, intern im Lifecycle-Konstruktor abonniert, Gate `!IsResuming` — entspricht dem früheren `resume is null`).

### FavoriteHomeViewModel (B4)

- `RefreshForOccasionAsync(RefreshOccasion, freshness, ct)` dispatcht auf die jetzt **privaten** Schritte `RefreshAtStartupAsync`, `RefreshMissingAsync(freshness)` und `RefreshStaleAsync` (Z. 106–116). Keine externen Aufrufer der Triplik mehr (Repo-weite Suche: nur noch interne Verweise). Die Anlass→Schritt-Zuordnung liegt nicht mehr in `HomePage`.

### Unverändert gebliebene Pflichten

- Fachliche Refresh-Schritte unangetastet: `FavoriteMonitorViewModel`, `StopMonitorViewModel`, `JourneySearchViewModel`, `RefreshLoop` nicht im Diff.
- `RunBackgroundAsync`: einzige Änderung ist der Aufruf `RefreshStaleAsync` → `RefreshForOccasionAsync(ForegroundReturn)` — identische Abbildung, Semantik gleichwertig; 12 bestehende `RefreshLifecycleTests_Background`-Tests unverändert grün.
- `RefreshLoop`-Steuerung bleibt bei den Pages (`ReconcileRefreshLoops`/`ReconcileRefreshLoop` in HomePage/DeparturePage); `ResumeCompleted` ersetzt exakt das frühere finally-Reconcile (gefiltert per `args.Area`).
- Fehler→Title-Pfad via `RefreshFailed`-Event: HomePage mappt Startup→„Abfahrten konnten nicht aktualisiert werden", CardsAdded→„Abfahrten konnten nicht geladen werden", sonst→„Aktualisierung fehlgeschlagen" — exakt die früheren Titel. Monitor/Journeys setzen „Aktualisierung fehlgeschlagen". Event nur bei noch sichtbarem Bereich (`visible.ContainsKey`).

### Scope-Ausschlüsse

- `SearchPage`, `StopSearchPage`, `MapPage`, `LocationPickerPage` nicht im Diff; ihre eigenen Foreground-Cancel-Pfade (Endpoint-Lookups, Kacheln) sind unberührt und bleiben wirksam.

### Event-Leaks / Cancel-Pfade

- Lifecycle→`foreground`/`favorites`-Subscriptions im Konstruktor: Singleton gegen Singleton — kein Leak.
- Seiten-Abos (`RefreshFailed`, `ResumeCompleted`) in `OnAppearing`/`OnDisappearing` symmetrisch. Kein verlorener Cancel-Pfad: jeder frühere `CancelPending`/`resume.Cancel` ist durch `CancelArea` abgedeckt.
- Keine Regression festgestellt. Marginalverbesserungen ohne Nebenwirkung: CardsAdded während des initialen Ladevorfalls entfällt (vom anschließenden Startup-Anlass abgedeckt — vermeidet den früher möglichen Doppelabruf); Fehler eines veralteten Startup-Laufs setzen den Titel nur, wenn der Bereich sichtbar ist.

### Tests / Nachweise

- 5 neue Testklassen vorhanden und grün: `_Occasions` (6), `_Foreground` (4), `_Overlap` (3), `_Cancellation` (2), `_Ownership` (3).
- Eigener Verifikationslauf: `dotnet test FlowNRW.Tests -c Release` → **330/330 bestanden, 0 Fehler**.
- E2E: `test-results.md` in Commit `acfed72` belegt realen Lauf von `WindowsLifecycleUiTests.ps1` gegen den UiTest-Fixture-Build mit allen Assertions PASS (Gate, Overlap, Abbruch, Journey-Identität); Artefakte workflowgemäß in `7a25305` entfernt, via `git show acfed72:<pfad>` einsehbar.
- Doku `docs/help/monitorintervalle/ablauf-technisch.md` aktualisiert und konsistent mit dem Code.

## Verdict

**vollständig erfüllt**
