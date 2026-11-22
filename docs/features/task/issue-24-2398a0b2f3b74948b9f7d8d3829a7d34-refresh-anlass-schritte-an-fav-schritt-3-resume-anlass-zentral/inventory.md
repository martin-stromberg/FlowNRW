# Bestandsaufnahme – Schritt 3: Vordergrund-Aktualisierung und Anlass-Zuordnung

Stand: Basisbranch `task/issue-24-…-refresh-anlass-schritte-an-fav` (nach Schritt 2).

## Zentrale Stelle

- `FlowNRW.Core/Refresh/RefreshLifecycle.cs` — sealed, hält `ForegroundState`,
  `RefreshSettingsViewModel`, `FavoriteHomeViewModel`, `RefreshFreshness`,
  `TimeProvider`. Bietet `SetActive(bool)` (beendet Hintergrund-Ownership,
  `favorites.CancelPending()`) und `RunBackgroundAsync` (bounded 20 s, gate
  `IntervalSeconds == 0`, `favorites.RefreshStaleAsync`). Kennt heute **nur** die
  Favoriten-Karten. Singleton via `MauiProgram`.
- `FlowNRW.Core/Refresh/ForegroundState.cs` — `ObservableObject`, `IsActive`
  (PropertyChanged-getrieben; alle Pages abonnieren).
- `FlowNRW.Core/Refresh/RefreshSettingsViewModel.cs` — `IsLoaded`,
  `IntervalSeconds`, `LoadAsync`, `Changed`-Event.
- `FlowNRW.Core/Refresh/RefreshFreshness.cs` — `IsStale(Result)`.
- `FlowNRW.Core/Refresh/RefreshLoop.cs` — pro-Karte/Monitor Intervall-Schleife.

## Die vier Resume-Kompositionen (Fundstellen B2)

### `FlowNRW/HomePage.cs` (geschützt)

- Felder: `resume` (CTS), `active`, `appearance`-Version, `refreshLoops`-Dictionary.
- `ForegroundChanged` (Z. ~141–160): bei `!IsActive` → `resume.Cancel`,
  `ReconcileRefreshLoops`, `model.CancelPending`. Bei `IsActive`: Gate
  `!active || !settings.IsLoaded || settings.IntervalSeconds == 0 || resume is not null`
  → `model.RefreshStaleAsync(freshness, source.Token)`, `finally` mit
  `ReferenceEquals(resume, source)` → `resume = null; ReconcileRefreshLoops()`.
- Anlass-Zuordnung (B4): `OnAppearing` → `RefreshAtStartupAsync(version)`;
  Cards-Changed (Z. ~70) → `RefreshMissingAsync()` (Guard
  `active && foreground.IsActive && resume is null`); Foreground → `RefreshStaleAsync`.
- `OnDisappearing`: `resume.Cancel(); resume = null;` + `CancelPending` + Loops stoppen.

### `FlowNRW/DeparturePage.cs` (geschützt)

- Gleiche Struktur mit `resume`-CTS; Gate identisch; ruft
  `model.RefreshIfStaleAsync(freshness, source.Token)` (`StopMonitorViewModel`);
  `ReconcileRefreshLoop` (singular) im `finally`; Hidden-Pfad: `resume.Cancel`,
  `refreshLoop.Stop()`, `model.CancelPending()`.

### `FlowNRW/JourneyDetailPage.cs` (ungeschützt)

- `ForegroundChanged`: `!IsActive` → `model.Session.CancelPending()`; `IsActive` →
  `ResumeAsync()`. `ResumeAsync` (Z. ~95): Gate
  `!active || !foreground.IsActive || !settings.IsLoaded || settings.IntervalSeconds == 0`
  → `model.Session.RefreshIfStaleAsync(freshness)` — **kein** Overlap-Guard, **kein**
  CancellationToken, **kein** Reconcile. `OnAppearing` ruft ebenfalls `ResumeAsync`.

### `FlowNRW/ResultsPage.cs` (ungeschützt)

- Identisch zu JourneyDetailPage: `ResumeAsync` (Z. ~93) mit gleichem Gate,
  `model.Session.RefreshIfStaleAsync(freshness)` ohne Overlap/CTS/Reconcile.

## Anlass-Schritte am `FavoriteHomeViewModel` (B4)

- `RefreshMissingAsync()` (Z. 103): `Task.WhenAll` über Karten mit `IsStale(Result)
  && !IsBusy` → `card.RefreshAsync()`.
- `RefreshAtStartupAsync()` (Z. 107): `Task.WhenAll` über `!IsBusy`-Karten →
  `card.RefreshAutomaticallyAsync()`.
- `RefreshStaleAsync(RefreshFreshness, CancellationToken)` (Z. 156): wird auch von
  `RefreshLifecycle.RunBackgroundAsync` genutzt — **nicht** entfernen, Fassade drumherum.
- Aufrufer nur `HomePage` (Zuordnung OnAppearing/Cards-Changed/Foreground) und
  `RefreshLifecycle` (Hintergrund).

## Weitere aktualisierbare Bereiche / Modelle

- `StopMonitorViewModel.RefreshIfStaleAsync(freshness, ct)` — Monitor-Resume.
- `Session.RefreshIfStaleAsync(freshness)` — `model.Session` an JourneyDetail/Results
  (`JourneyResumeTests_*`, `ResumeMonitorTests_*` testen es direkt).
- `SearchPage`/`StopSearchPage`: nur Hidden-Cancel, kein Resume-Refresh.

## Tests / Infrastruktur

- `FlowNRW.Tests/RefreshLifecycleTests_Background.cs` — Fixture-Zeile 159:
  `internal RefreshLifecycle Lifecycle { get; }`; deckt `RunBackgroundAsync`/
  `SetActive`-Pfade ab.
- `JourneyResumeTests_Selection.cs`, `ResumeMonitorTests_Cancellation.cs` —
  `RefreshIfStaleAsync`-Semantik auf Modellebene.
- Testklassen-Konvention: thematisch gesplittet (`*_Background`, `*_Selection`,
  `*_Cancellation` — siehe Skill `test-class-structure`); neue Lebenszyklus-Tests als
  eigene Klassen (z. B. `RefreshLifecycleTests_Foreground`).
- DI: `RefreshLifecycle` Singleton in `MauiProgram` (Z. ~108 ff.); Pages erhalten
  `ForegroundState`/`settings`/`freshness` per Konstruktor. Zentrale Stelle braucht
  Zugriff auf `StopMonitorViewModel`-/Session-Refreshables — Registrierungspunkt
  klären (z. B. `IRefreshable`-Registrierung oder ViewModel-Injektion ins Lifecycle).

## Konventionen / Risiken

- Pages leben in `FlowNRW/` (MAUI-Projekt), `RefreshLifecycle` in `FlowNRW.Core` —
  Core darf nicht auf `Microsoft.Maui`-Typen verweisen; die Zentralisierung muss
  über Core-abstrahierte Schnittstellen laufen (Page meldet visible/hidden,
  Lifecycle ruft Core-ViewModel-Methoden).
- Async-void-Eventhandler `ForegroundChanged` → Fehlerbehandlung `Title = …` heute
  page-seitig; beim Zentralisieren muss ein gleichwertiger Fehlerpfad bleiben.
- `resume`-CTS-Guard `ReferenceEquals`-Muster bereits etabliert (Schritt 2,
  `StopMonitorViewModel.OpenStopAsync`) — wiederverwenden.
- `RefreshLifecycle` ist Singleton: überlappende Resume-Requests mehrerer sichtbarer
  Seiten sind möglich (Shell-Navigation) → Overlap-Semantik pro Bereich oder global
  klären; Kandidat: Registrierung genau eines aktiven Refreshables je Page
  („visible"-Meldung ersetzt vorherige).
