# Tests – Bestandsaufnahme

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): 2026-11-19, lokale Zeit (UTC+1)
- Branch und Commit-ID: `task/issue-28-950a26f4b8ab4e0488c828a12803f266-fehler-bei-verbindungsdetail` @ `fc2c7ae04d141ff9980a2e3e92b9ae8e2e31178b`
- Uncommittete Änderungen im getesteten Stand: nur neue Anforderungsartefakte unter `docs/features/task/issue-28-…/` (untracked), kein Produktivcode
- Testumgebung und Runtime-/SDK-Versionen: Windows, .NET SDK 11.0.100, Ziel `net10.0`, xunit
- Ermittelte Testsuiten und Quellen der Testbefehle: `FlowNRW.Tests` per `CONTRIBUTING.md` (`dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release`); zusätzlich CI-Gates `dotnet format`, statische Analyse (`TreatWarningsAsErrors`), Coverage ≥ 70 % (`.github/workflows/pr-staging-ci.yml`), npm-Release-Skripttests

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Baseline | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release` | Repo-Wurzel | 0 | 300 | 0 | 0 | [baseline.log](test-results/baseline.log) |

### Nachgewiesene bestehende Testfehler

Keine — der Ausgangslauf war vollständig erfolgreich (300/300).

### Testlücken und Ausführungsprobleme

- Die MAUI-App `FlowNRW` (net10.0-windows) wurde im Ausgangslauf nicht gebaut;
  nativ getriebene Windows-UI-Tests unter `tests/WindowsJourneyUiTests/`
  (PowerShell/UIAutomation, benötigen `UI_TEST_FIXTURES`-Build und laufende App)
  wurden nicht ausgeführt.
- `npm run test:release-version` wurde für die Bestandsaufnahme nicht ausgeführt.

## Testklassen

### `JourneyResumeTests_Selection`
- `RefreshIfStale_UniqueJourney_UpdatesSelectionWithoutNavigation` — eindeutige Zuordnung erhält Auswahl
- `RefreshIfStale_UnsafeIdentity_ClearsSelection` (Theory: fehlende/duplizierte Zuordnung) — Auswahl wird aufgehoben, Status „nicht mehr eindeutig"
- `RefreshIfStale_ProviderFailure_PreservesResult` — Fehlerfall behält Daten
- `RefreshIfStale_EndpointChanged_RejectsLateResponse` — verspätete Antwort wird verworfen

### `PresentationTests`
- u. a. `JourneyDetailViewModel.Details` bei ausgewählter Verbindung („Umstiege"-Text)

### `RealtimeConsolidatorTests_Identity`
- Zuordnungsregeln `Matches`/`SameStop` (Konsolidierungskontext)

### `ProviderOrchestratorTests_Union`
- `MergeJourneys`-Zusammenführung inkl. Fußwegabschnitten

## Hilfsmethoden

### Test-Fixtures (in `JourneyResumeTests_Selection`/`PresentationTests`/`TransitTestSupport`)
- `ControlledRoutingService` — warteschlangengesteuertes `IRoutingService` (`Pending[i].SetResult(...)`)
- `RecordingNavigation` — zählt `Results`/`Details`-Navigationen
- `Journey(id)`-Helfer — erzeugt einzelnen Verkehrsabschnitt mit voller `TripIdentity`
- **Lücke:** Kein Fixture für `Journey` mit `Walking != null`-Abschnitt; kein Test für `Details` ohne Auswahl.
