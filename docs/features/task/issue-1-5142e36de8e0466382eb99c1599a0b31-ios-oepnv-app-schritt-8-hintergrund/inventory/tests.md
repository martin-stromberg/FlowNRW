# Tests und Test-Ausgangszustand

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): `2026-09-25 12:24:29–12:24:32 +02:00` für Core-Tests; Release-Skript-Test im selben Bestandsaufnahmeabschnitt am `2026-09-25`.
- Branch und Commit-ID: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-8-hintergrund`, `d8c606d033f1d0943fe4858e827dc77e5ad5bd94`.
- Uncommittete Dateien vor diesem Inventory: `design-draft.zip` und das Feature-Verzeichnis mit der vorhandenen `requirement.md`; Produktivcode, Tests und Testkonfiguration waren unverändert. Während des Baseline-Laufs wurden nur Testausgaben unter `inventory/test-results/` erzeugt.
- Testumgebung: Windows PowerShell 7.6.6; .NET SDK 10.0.401, .NET Runtime 10.0.12; Node.js v24.15.0, npm 11.12.1 (CI pinnt Node.js 22; der lokale Release-Skriptlauf fand daher mit einer abweichenden Node-Hauptversion statt). Betriebssystemversion konnte wegen verweigertem CIM-Zugriff nicht ermittelt werden. Das Testlog bestätigt Windows-Pfade und 64-bit .NET.
- Ermittelte Testsuiten und Quellen: `FlowNRW.Tests/FlowNRW.Tests.csproj` ist die plattformunabhängige xUnit-Suite; `.github/workflows/pr-staging-ci.yml` führt sie in Release-Konfiguration aus und sammelt TRX/Coverage. Dasselbe Workflowfile führt `npm run test:release-version` für Release-Skripte aus. Manuelle native Windows UI-E2E sind in `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1` und `WindowsRefreshUiTests.ps1` implementiert und in den Schritt-7-Verifikationsunterlagen dokumentiert, jedoch kein eigener CI-Job. Format, Securityscan und Release-Solution-Build sind zusätzliche Workflow-Gates.
- Es gibt keine iOS-Hintergrund- oder Resume-Testklasse. Native iOS-Testausführung wurde im Ausgangslauf nicht unternommen; die Projekt-Bestandsaufnahme weist für diese Umgebung keine lokale Apple-Werkzeugkette/Simulatorverbindung nach.

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Core-Unit-Tests | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-build --no-restore --logger 'trx;LogFileName=baseline-core.trx' --logger 'console;verbosity=normal' --results-directory docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-8-hintergrund/inventory/test-results` (kein Filter) | Repositorywurzel | 0 | 210 | 0 | 0 | [Konsolenausgabe](test-results/baseline-core.txt), [TRX](test-results/baseline-core.trx), [Exit/Umgebung](test-results/baseline-core-metadata.txt) |
| Release-Skript-Tests | `npm.cmd run test:release-version` | Repositorywurzel | 0 | 26 | 0 | 0 | [Konsolenausgabe](test-results/baseline-release-scripts.txt), [Exitcode](test-results/baseline-release-scripts-exit.txt) |
| Vollständiger Windows-Releasebuild, Coverage und native UI-Regressionen (bestehender Schritt-7-Nachweis; nicht in dieser Phase erneut ausgeführt) | Siehe Kommandos in der verlinkten Verifikationsdokumentation | Repositorywurzel im dokumentierten Lauf | dokumentiert erfolgreich | Core 210; native UI-Suiten erfolgreich laut Einzelnachweisen | 0 im dokumentierten finalen Corelauf | 0 im dokumentierten finalen Corelauf | [Abschlussübersicht](../../../../help/monitorintervalle/verification/index.md), darin verlinkte Release-, Core-, Coverage-, Routing-, Monitor-, Favoriten- und Intervallnachweise |

Der vorhandene Schritt-7-Nachweis stammt vom Produktivcode-Stand `3c062ed8f0da8f766637d2d05e31f2a3b89fe76b`. Ein direkter Git-Treevergleich für `FlowNRW`, `FlowNRW.Core`, `FlowNRW.Tests`, `tests` und `.github` gegen den aktuellen `HEAD` ergab keine Unterschiede. Der Stand berichtet Windows-Release-Solutionbuild ohne Warnungen/Fehler, 210 Coretests ohne Fehler/Überspringungen und 92,77 % Core-Zeilenabdeckung sowie erfolgreiche native Refresh-, Favoriten-, Routing- und Einzelmonitorprüfungen. Die nativen Läufe werden deshalb als vorheriger codegleicher Nachweis referenziert, nicht als erneute Ausführung dieser Bestandsaufnahme.

### Nachgewiesene bestehende Testfehler

Keine. Beide in dieser Bestandsaufnahme ausgeführten Suiten bestanden ohne fehlgeschlagene oder übersprungene Tests. Auch der referenzierte Schritt-7-Abschlussbericht weist im finalen Corelauf keine Fehler aus. Frühere, damals fehlgeschlagene UI-Versuche und ihre Korrekturen sind in der dortigen Fehlerhistorie dokumentiert; sie gelten nicht als Fehler des aktuellen Ausgangslaufs.

### Testlücken und Ausführungsprobleme

- Der aktuelle Baseline-Corelauf nutzte `--no-build --no-restore`, da vorhandene Release-Buildartefakte und der codegleiche vorherige vollständige Releasebuild dokumentiert sind. Er testete die 210 Corefälle; er ist kein neuer Buildnachweis.
- Die Release-Skript-Suite lief mit vorhandenen Node-Abhängigkeiten; sie meldete 26/26 bestanden. `npm.cmd --version` meldete 11.12.1.
- Native Windows UI-Skripte wurden in dieser Phase nicht nochmals gestartet. Ihr vorheriger erfolgreicher Lauf ist separat verlinkt und sein getesteter Quellbaum ist nachweislich inhaltlich gleich. Diese Referenz ist kein iOS-Hintergrundtest.
- Native iOS-Build-, Simulator-, Geräte-, UI- und Suspend/Resume-Tests wurden nicht ausgeführt. Die Abnahme erfolgt wie vereinbart durch den Nutzer. Aus dem Nichtlauf wird kein bestandener Test abgeleitet.
- `Get-CimInstance Win32_OperatingSystem` verweigerte den Zugriff auf die Betriebssystemkennung; die Windows-Version bleibt unbekannt. Dies hinderte beide Testsuiten nicht.
- Git gab bei Status-/Diff-Aufrufen Warnungen über nicht lesbare globale Ignore-Konfiguration `C:\Users\Martin\.config\git\ignore` aus. Die im Workspace sichtbare Statusausgabe war trotzdem verfügbar; kein Testsuitefehler.

## Testklassen

### `RefreshLoopTests_Scheduling`
Datei: `FlowNRW.Tests/RefreshLoopTests_Scheduling.cs`

- `IntervalChangeRejectsOldDelay` — verwirft verspätete Wartezeit nach Intervallwechsel.
- `FullDelayBeginsAfterCompletion` — beginnt die nächste Wartezeit erst nach Ende eines langsamen Abrufs.
- `IndependentLoopsProgressSeparately` — getrennte Monitore blockieren einander nicht.
- `RefreshFailureWaitsForNextInterval` und `FailedDelayDoesNotCreateRetryStorm` — Fehler und verzögerter nächster Versuch.
- `OffAndReactivationHaveSingleOwnership` — idempotentes Stoppen, Deaktivieren und Wiederstart.
- `ForegroundStateNotifiesOnlyTransitions` — Zustandsänderungen nur bei tatsächlichem Wechsel.

### `RefreshMonitorTests_Concurrency`
Datei: `FlowNRW.Tests/RefreshMonitorTests_Concurrency.cs`

- `FavoriteManualAndAutomaticPathsShareBusyGuard` und `StopMonitorManualAndAutomaticPathsShareBusyGuard` — manueller und automatischer Abruf teilen Busy-Schutz.
- `LoopStopCancelsInflightAndRejectsLateCardResult` — Stoppen bricht Request ab und weist späte Antwort zurück.
- `AutomaticFailureRetainsDataAndRecoveryWaits` — Fehler erhält bisherige Daten und vermeidet sofortige Wiederholschleife.

### `RefreshSettingsTests_Persistence`
Datei: `FlowNRW.Tests/RefreshSettingsTests_Persistence.cs`

- `SupportedChoicesSurviveRestart` — persistierte Intervalle über neue Store-/ViewModel-Instanz.
- `CorruptSettingsUseSafeDefaultWithoutOverwriting` und `MissingFileResolvesDefaultAndLoadsOnce` — ungültige/fehlende Einstellungen.
- `CancelledSavePreservesPreviousBytes`, `FailedReplacementPreservesTargetAndCleansTemporaryFile`, `FailedDraftSaveCanBeRetriedWithoutPrematureChange` — Abbruch und Speicherfehler.
- `InvalidDraftIsRejectedBeforeSave` — nicht erlaubte Intervalle.

### `RefreshTests`
Datei: `FlowNRW.Tests/RefreshTests.cs`

- `SettingsRoundTripAndInvalidValuesStayOut` — Standard, Speichern und Validierung.
- `LoopIsDelayedAndIdempotent` — verzögerter Start und doppelstartfeste Schleife.
- `StopRejectsLateWork` — abgebrochene Arbeit wird invalidiert.

### `StopMonitorTests` und `FavoriteTests`
Dateien: `FlowNRW.Tests/StopMonitorTests.cs`, `FlowNRW.Tests/FavoriteTests.cs`

Diese Suites prüfen angrenzend manuelle/automatische Busy-Sperren, Ergebnisbeibehaltung, verspätete Antworten, unabhängige Favoritenabrufe und Favoritenpersistenz.

## Hilfsmethoden

### `ControlledRefreshDelay`
Datei: `FlowNRW.Tests/RefreshTests.cs`

- `WaitAsync(TimeSpan, CancellationToken)` — speichert Intervall, Token und kontrollierbare `TaskCompletionSource`, damit Timing ohne reale Wartezeiten geprüft wird.

### `ControlledDepartureService`, `ControlledSearchService`, `ControlledRefreshSettingsStore`
Dateien: `FlowNRW.Tests/RefreshMonitorTests_Concurrency.cs`, `FlowNRW.Tests/RefreshSettingsTests_Persistence.cs` sowie gemeinsame Testhilfen.

- Kontrollieren manuell den Abschluss von Providerrequests, Cancellation-Token und Settings-Saves für Konkurrenz-/Verspätungsprüfungen.

### `RefreshTestFiles`
Datei: `FlowNRW.Tests/RefreshSettingsTests_Persistence.cs`

- Stellt temporäre Dateipfade bereit und entfernt sie nach dem Test.

### Native Windows UI Harnesses
Dateien: `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`, `tests/WindowsJourneyUiTests/WindowsRefreshUiTests.ps1`

- Starten die echte MAUI-Windows-App mit isolierten `UI_TEST_FIXTURES`-Services und steuern sie über Windows UI Automation. Der allgemeine Runner bietet Flüsse für Routing, Monitore, Karten, Standort und Favoriten; der Refreshrunner prüft Intervallsteuerung, Fensteraktivierung, Timer und Favoritenzyklen mit echten Zeitintervallen.

