# Tests — Ausgangszustand und Bestand

## Test-Ausgangszustand vor der Umsetzung

- **Zeitpunkt:** 2026-11-20, ca. 20:19–20:45 Uhr CET (UTC+1) / 19:19–19:45 UTC
- **Branch und Commit-ID:** `task/issue-30-0d24300145ad41dfb5d3500f06b5e040-nachricht-von-apple` @ `deda1be2e46226d5c38e66da9bc293c575451966` („fix: Verbindungsauswahl nach Aktualisierung auch mit Fußweg erhalten (#29)"; identisch mit `origin/staging`/`staging`, Tag `v1.0.0-rc.13`)
- **Uncommittete Änderungen im getesteten Stand:** Nur untracked `docs/features/task/issue-30-0d24300145ad41dfb5d3500f06b5e040-nachricht-von-apple/` (Anforderungs- und Bestandsaufnahme-Artefakte dieser Aufgabe). Keine Änderungen an Code, Tests oder Build-/CI-Konfiguration.
- **Testumgebung und Runtime-/SDK-Versionen:**
  - Windows 11 (OS 10.0.26300), Git Bash
  - Installierte .NET SDKs: `10.0.404`, `11.0.100`. Kein `global.json` im Repo → Resolver nutzt `11.0.100`.
  - Testlaufzeit: `Microsoft.NETCore.App 10.0.15` (xUnit-VSTest-Adapter meldet „.NET 10.0.15")
  - Installierte Workloads (per MSI via VS 18.11.12402.416): `ios` `26.5.11720-net11-p6`, `maccatalyst` `26.5.11720-net11-p6`, `maui-windows` `11.0.0-preview.6.26360.8`, `android` `37.0.0-preview.6.59` — jeweils `11.0.100-preview.6`-Manifestband
  - Node `v24.15.0`, npm `11.10.0` (Hinweis: `package.json` deklariert `engines.node = 22.x` — Abweichung, `npm ci`/`node --test` liefen dennoch durch)
- **Ermittelte Testsuiten und Quellen der Testbefehle:**
  1. `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj` (xUnit, net10.0; Coverage+trx in CI) — Quelle: `.github/workflows/staging-ci.yml` Job `build-and-test` (Z. 114–120), `pr-staging-ci.yml` (Z. 122–128), `release.yml` Release-Gate (Z. 84–94)
  2. `npm run test:release-version` (`node --test` auf `scripts/resolve-release-version.test.mjs` + `scripts/generate-update-manifest.test.mjs`) — Quelle: `package.json` (Z. 9), `.github/workflows/staging-ci.yml` Job `release-scripts-test` (Z. 160–180)
  3. `dotnet build FlowNRW.sln -c Release` (Solution-Build inkl. MAUI-App; in CI Gate 3 mit `TreatWarningsAsErrors`) — Quelle: `staging-ci.yml` (Z. 86–89)

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| 1 | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --logger "trx;LogFileName=test-results.trx" --logger "console;verbosity=normal"` | Repo-Root | 0 | 303 | 0 | 0 | [Log](test-results/dotnet-test-flownrw-tests.log), [TRX](test-results/flownrw-tests-results.trx) |
| 2 | `npm ci` + `npm run test:release-version` | Repo-Root | 0 | 26 | 0 | 0 | [Log](test-results/npm-test-release-version.log) |
| 3 | `dotnet build FlowNRW.sln -c Release` | Repo-Root | 0 | — (Build, kein Testlauf; 0 Warnungen, 0 Fehler) | — | — | [Log](test-results/dotnet-build-solution.log) |
| 4 | `dotnet test FlowNRW.sln -c Release` | Repo-Root | 0 | 303 | 0 | 0 | [Log](test-results/dotnet-test-solution.log) |

**Anmerkungen:**

- Lauf 1 und 4 testen dieselbe Testassembly (`FlowNRW.Tests.dll`); die MAUI-App (`FlowNRW.csproj`) enthält keine Tests und wird von `dotnet test` nur gebaut (net10.0-windows10.0.19041.0, win-x64).
- Lauf 3 baute alle drei Projekte erfolgreich — die lokale `maui-windows`-Workload (net11-Preview-Packs) reicht für den `net10.0-windows`-Build aus. Der `net10.0-ios`-Teil wurde auf Windows **nicht** gebaut (TargetFrameworks-Bedingung `IsOSPlatform('osx')`/`EnableIos`, `FlowNRW.csproj` Z. 11).
- Die Testergebnisdateien enthalten keine Zugangsdaten oder Geheimnisse.

### Nachgewiesene bestehende Testfehler

**Keine.** Alle 303 .NET-Tests und alle 26 Node-Tests bestanden im Ausgangslauf. Es wurden keine Testfehler nachgewiesen.

### Testlücken und Ausführungsprobleme

- **iOS-Paketierungspfad nicht lokal ausführbar:** Der von der Anforderung betroffene Job `package-ios` (`.github/workflows/staging-ci.yml` Z. 256–287) und die Action `.github/actions/package-ios/action.yml` benötigen einen macOS-Runner, die `maui-ios`/`ios`-Workloads sowie Signing- und App-Store-Connect-Secrets (`vars.IOS_SIGNING_ENABLED`, `secrets.IOS_*`). Auf Windows nicht ausführbar — die Umstellung kann lokal nicht verifiziert werden, nur im CI-Lauf (Build-Log: `dotnet workload list`, Xcode-Version) und anhand der Apple-Antwortmail.
- **`net10.0-ios` nicht baubar auf Windows:** `dotnet build FlowNRW.sln` baut die App nur für `net10.0-windows10.0.19041.0` (siehe Lauf 3). Kein Fehler, sondern projektseitige Bedingung.
- **Keine Tests für die CI-/Build-Artefakte:** Für `action.yml`, `staging-ci.yml` und `iOS-Deployment.ps1` existieren keine automatisierten Tests (YAML wird nicht lokal validiert).
- **`tests/WindowsJourneyUiTests/` nicht ausgeführt:** Fünf PowerShell-UI-Testskripte (`WindowsDesignUiTests.ps1`, `WindowsJourneyUiTests.ps1`, `WindowsDepartureCacheUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsRefreshUiTests.ps1` + `UiTestFixtureServices.cs`). Sie sind nicht Teil der CI-Gates, benötigen eine laufende App-Instanz (`UiTest`-Konfiguration) und Interaktion — für diese Bestandsaufnahme nicht ausgeführt. Umfang: gesamte UI-Testsuite.
- **Statische Gates nicht ausgeführt:** `dotnet format --verify-no-changes` und `security-scan` (`staging-ci.yml` Z. 73–85) sind statische Prüfungen, keine Tests — nicht ausgeführt.
- **Coverage-Gate nicht geprüft:** Die CI erzwingt ≥ 70 % Line Coverage (Gate 5). Lokal wurde kein Coverage-Lauf mit `--collect:"XPlat Code Coverage"` + ReportGenerator durchgeführt; der reine Testlauf (Lauf 1) ist grün.
- **Node-Versionsabweichung:** Lokal Node 24.15.0 vs. deklariert `engines.node = 22.x` (package.json) — keine Auswirkung auf das Ergebnis, dokumentiert als Umgebungsabweichung zur CI (CI nutzt `actions/setup-node@v7` mit `node-version: '22'`).

## Testklassen

Keine Testklassen decken den betroffenen Bereich (iOS-Paketierung) ab. Der vollständige Testbestand betrifft ausschließlich `FlowNRW.Core` (`FlowNRW.Tests`, 303 Tests, xUnit, net10.0) sowie die Release-Skripte (Node, 26 Tests). Auflistung der vorhandenen `FlowNRW.Tests`-Testdateien (alle grün im Ausgangslauf):

- `ConnectionFavoriteTests.cs`, `DepartureCacheStoreTests.cs`, `DeparturePresentationTests_Badges.cs`, `DepartureStatusTests.cs`, `FavoriteDepartureCacheIntegrationTests.cs`, `FavoriteTests.cs`, `JourneyResumeTests_Selection.cs`, `LocationTests.cs`, `MapTests.cs`, `MemoryTransitCacheTests_Lifetime.cs`, `NrwRegionClassifierTests_Boundary.cs`, `PresentationTests.cs`, `ProviderOrchestratorTests_Concurrency.cs`, `ProviderOrchestratorTests_Fallback.cs`, `ProviderOrchestratorTests_RegionResolution.cs`, `ProviderOrchestratorTests_Union.cs`, `RealtimeConsolidatorTests_Identity.cs`, `RefreshFreshnessTests_Boundaries.cs`, `RefreshLifecycleTests_Background.cs`, `RefreshLoopTests_Scheduling.cs`, `RefreshMonitorTests_Concurrency.cs`, `RefreshSettingsTests_Persistence.cs`, `RefreshTests.cs`, `ResumeMonitorTests_Cancellation.cs`, `RetryPolicyTests_Bounds.cs`, `RoutingServiceTests_NextJourneys.cs`, `StopMonitorTests.cs`, `TransitHttpGatewayTests_BodyFailures.cs`, `TransitHttpGatewayTests_Execution.cs`, `TransitHttpGatewayTests_Security.cs`, `TransitModelTests_Validation.cs`, `TransitServiceTests_InvalidReplacement.cs`, `TransitServiceTests_Validation.cs`, `Transit/` (weitere Transit-Testdateien, u. a. `EfaProviderTests_Requests`, `EfaResponseMapperTests_Journeys` laut Testlog)

Node-Tests: `scripts/resolve-release-version.test.mjs`, `scripts/generate-update-manifest.test.mjs` (26 Tests via `node --test`).

## Hilfsmethoden

### `TransitTestSupport` (`FlowNRW.Tests/TransitTestSupport.cs`)

- Testdaten/Hilfsfunktionen für die Transit-Testklassen (Fixture-Aufbau, gemeinsame Provider-/Modell-Helder). Details für diese Anforderung nicht relevant — kein Bezug zur iOS-Pipeline.

### `UiTestFixtureServices` (`tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`)

- Wird in der `UiTest`-Konfiguration in `FlowNRW.csproj` eingebunden (Z. 6–8, `Compile Include` + `UI_TEST_FIXTURES`-Konstante) und stellt Fixture-Dienste für die Windows-UI-Testskripte bereit.
