# Tests und Ausgangsnachweis

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt des vorhandenen Nachweises: 2026-09-15 (Europe/Berlin).
- Branch/Commit: `task/issue-1-5142e36de8e0466382eb99c1599a0b31` / `be7bb34bd4c825806c72d1f7a8b6965b64de3656`; Arbeitsstand mit den im übergeordneten Nachweis genannten Dokumentations-/Arbeitsdateien.
- Umgebung: Windows 10 (`10.0.26200`), .NET SDK `10.0.401`; iOS-Workload vorhanden, keine lokale Xcode-/Appium-Ausführung.
- Maßgeblicher Nachweis: [union-correction-checks.md](../../../../help/fahrplanauskunft/verification/union-correction-checks.md). Die dort verlinkten Reports und Logs bleiben die Testausgabe; für diese Bestandsaufnahme wurden sie nicht neu erzeugt.
- Ermittelte Quellen: `FlowNRW.Tests/FlowNRW.Tests.csproj`, `.github/workflows/pr-staging-ci.yml`, `docs/CI-CD.md`.

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|---|---|---|---:|---:|---:|---:|---|
| Bestehender Core-/Coverage-Lauf | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true --collect:"XPlat Code Coverage" --results-directory D:/temp/flownrw-union-final` | Repositorywurzel | 0 | 106 | 0 | 0 | [Nachweis](../../../../help/fahrplanauskunft/verification/union-correction-checks.md), [Coverage](../../../../help/fahrplanauskunft/verification/coverage-union.cobertura.xml) |
| Bestehender Windows-Build | `dotnet build FlowNRW.sln -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true` | Repositorywurzel | 0 | n/a | 0 | n/a | [Nachweis](../../../../help/fahrplanauskunft/verification/union-correction-checks.md) |

Gesamte Core-Assembly: 570/578 Zeilen, 98,61 % Coverage. Zusätzlich ist der XML-Dokumentationshook mit 77 Dateien erfolgreich nachgewiesen; Release-Skripttests (26) blieben laut Nachweis gültig.

### Nachgewiesene bestehende Testfehler

Im maßgeblichen Abschlusslauf keine. Der Nachweis dokumentiert frühere rote Union-Proben und einen zwischenzeitlichen gezielten Fehlschlag, die vor dem 106/106-Abschlusslauf korrigiert wurden; sie sind kein Fehler des getesteten Abschlussstands.

### Testlücken und Ausführungsprobleme

Es gibt keine native iOS-Ausführung in diesem Nachweis: `xcodebuild`, `xcrun` und `appium` fehlen lokal, und ein Mac-/Simulatorzugang ist nicht belegt. Native Windows-UI-E2E sind im bestehenden Projekt noch nicht ausgeführt; im lokalen NuGet-Cache sind jedoch `flaui.core` und `flaui.uia3` (FlaUI 5.0) vorhanden. Fixture- und Serviceproben belegen keine dauerhafte Live-Abdeckung aller Provider.

## Testklassen und Hilfsmethoden

Featurebezogene vorhandene Tests liegen in `FlowNRW.Tests/Transit`, unter anderem für `RoutingService`, Validierung, Anfrageabbruch, Provider-Orchestrator (Region, Fallback, Union, Parallelität), Realtime-Konsolidierung, Mapper, Cache, Gateway und Modelle. Die vollständige Klassen-/Methodenliste ist im bestehenden Nachweis und den Quelldateien verfügbar; eine unveränderte Vollrecherche wurde gemäß Nutzeranweisung nicht wiederholt.
