# Tests — Ausgangszustand

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): 2026-11-14 22:18 UTC
- Branch und Commit-ID: `task/issue-20-69c5dae32a9940a2869fa43c00b0aaed-automatisierung-wiederkehrende` @ `15276be`
- Uncommittete Änderungen im getesteten Stand: keine (nur untracked Lifecycle-Artefakte unter `docs/features/…`, nicht Teil des Builds)
- Testumgebung und Runtime-/SDK-Versionen: Windows 11, .NET SDK 10.0.403 (Workloads inkl. android/maui installiert), xUnit
- Ermittelte Testsuiten und Quellen der Testbefehle:
  - `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj` (xUnit, laut README/CI die projektübliche Suite; `staging-ci.yml` führt zusätzlich `npm run test:release-version` aus)
  - `npm run test:release-version` (Node-Skript-Tests für Release-Versionierung, siehe `package.json`)

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| 1 | `dotnet build FlowNRW.sln --nologo -v q` | Repo-Root | 0 | Build ohne Warnungen/Fehler | – | – | [Log](test-results/dotnet-test-baseline.log) |
| 2 | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj --no-build --logger "console;verbosity=normal"` | Repo-Root | 0 | 300 | 0 | 0 | [Log](test-results/dotnet-test-baseline.log) |

### Nachgewiesene bestehende Testfehler

Keine — der dokumentierte Ausgangslauf war vollständig erfolgreich.

### Testlücken und Ausführungsprobleme

- `npm run test:release-version` wurde in dieser Bestandsaufnahme nicht
  ausgeführt (nicht betroffen von der Anforderung; keine Workflow-/YAML-Tests
  vorhanden). Die Suite existiert und bleibt unberührt.
- Die PowerShell-UI-Testskripte unter `tests/WindowsJourneyUiTests/` erfordern
  einen `UiTest`-Build der MAUI-App und wurden nicht ausgeführt — für diese
  Anforderung ohne UI-Bezug nicht relevant.

## Testklassen

Die Suite `FlowNRW.Tests` enthält u. a. `FavoriteTests`,
`TransitHttpGatewayTests_*`, `RefreshSettingsTests_*`, `MapTests` — für diese
Anforderung (reine Workflow-YAML) existiert keine passende Testklasse und es
werden keine neuen .NET-Tests benötigt.

## Hilfsmethoden

Keine für diese Anforderung relevanten Hilfsmethoden vorhanden.
