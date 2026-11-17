# Tests

Die Anforderung ist ein Dokumentations-/Infrastruktur-Feature ohne C#-Änderungen; es existieren keine seitenbezogenen Tests. Dieses Dokument belegt den projektweiten Test-Ausgangszustand vor der Umsetzung.

## Test-Ausgangszustand vor der Umsetzung

- **Zeitpunkt:** 2026-11-16, ca. 19:20–19:31 Uhr +0100 (18:20–18:31 UTC)
- **Branch und Commit-ID:** `task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode`, HEAD = `17662551a4c206e25803e27a20b8a2f8a671084a` (identisch mit `origin/staging`-Spitze; Basisbranch `staging`)
- **Uncommittete Änderungen im getesteten Stand:** nur das neue, untracked Feature-Verzeichnis `docs/features/task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode/` (`requirement.md`, `todo.md`); keine Änderungen an Produktivcode oder Tests
- **Testumgebung:** Windows (Git Bash), .NET SDK `10.0.403`, Node.js `v24.15.0`
- **Ermittelte Testsuiten und Quellen der Testbefehle:**
  - `FlowNRW.Tests` (xUnit, `net10.0`, testet ausschließlich `FlowNRW.Core`) — Befehl aus `README.md` Zeilen 67 und 128–129 sowie CI-Gate „build & test" (`docs/CI-CD.md` Zeile 97): `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj`
  - Release-Skripttests (Node `node:test`) — `package.json` Script `test:release-version`, CI-Job `release-scripts-test`: `npm run test:release-version`
  - Windows-UI-Tests `tests/WindowsJourneyUiTests/` (PowerShell/UIA gegen `UiTest`-Fixture-Build) — lokal nicht ausgeführt, siehe „Testlücken"

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|---|---|---|---|---|---|---|---|
| 1 | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release` | Repo-Wurzel | 0 | 300 | 0 | 0 | Konsolenausgabe (Erstlauf mit Build, Dauer 441 ms Testlauf) |
| 2 | `npm run test:release-version` | Repo-Wurzel | 0 | 26 | 0 | 0 | Konsolenausgabe (Erstlauf, 26 ✔-Zeilen) |
| 3 (Nachweis-Wiederholung) | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-build` | Repo-Wurzel | 0 | 300 | 0 | 0 | [dotnet-test-flownrw-tests.log](test-results/dotnet-test-flownrw-tests.log) |
| 4 (Nachweis-Wiederholung) | `npm run test:release-version` | Repo-Wurzel | 0 | 26 | 0 | 0 | [npm-test-release-version.log](test-results/npm-test-release-version.log) |

Die Wiederholungsläufe 3/4 dienten der persistenten Sicherung der Ausgaben; Ergebnis identisch zu den Erstläufen.

### Nachgewiesene bestehende Testfehler

Keine. Beide ausgeführten Suiten bestanden vollständig; es wurde kein Testfehler nachgewiesen.

Hinweis (Dokumentationsstand, kein Fehlschlag): `README.md` Zeile 132 nennt „307 Core-Tests"; der aktuelle Lauf zählt 300. `README.md` Zeile 21 nennt „106 Tests" für einen historischen Fahrplanauskunft-Stand.

### Testlücken und Ausführungsprobleme

- **`tests/WindowsJourneyUiTests/` nicht ausgeführt:** Fünf PowerShell-Treiber (`WindowsJourneyUiTests.ps1`, `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsDepartureCacheUiTests.ps1`, `WindowsDesignUiTests.ps1`) erfordern laut `tests/WindowsJourneyUiTests/README.md` eine interaktive Windows-Desktopsitzung, Windows PowerShell 5.1, UIAutomationClient und einen `UiTest`-Fixture-Build (`dotnet build FlowNRW/FlowNRW.csproj -c UiTest`, erfordert MAUI-Workload). Kein Ausführungsnachweis für den aktuellen Commit vorhanden.
- **CI-Gates lokal nicht wiederholt:** `dotnet format --verify-no-changes --severity error`, Security-Scan (NuGet) und `dotnet build FlowNRW.sln -c Release` (inkl. MAUI-App, erfordert `maui-windows`-Workload) sind Quality Gates aus `pr-staging-ci.yml`/`staging-ci.yml`, keine Tests — hier nicht ausgeführt.
- **Seitenbezogene Tests:** nicht vorhanden (kein Seitenquellbestand). Ein automatisierter Check der künftigen Seite (z. B. Link-/Build-Check) existiert nicht.

## Testklassen

### `FlowNRW.Tests` (xUnit, `net10.0`) — Auswahl nach Datei

- `ConnectionFavoriteTests` — Verknüpfung Verbindung/Favorit
- `DepartureCacheStoreTests`, `FavoriteDepartureCacheIntegrationTests` — Abfahrts-/Favoriten-Cache
- `DeparturePresentationTests_Badges`, `DepartureStatusTests`, `PresentationTests` — Präsentationszustände, Statussemantik, Badges
- `FavoriteTests` — Favoritenlogik
- `JourneyResumeTests_Selection`, `RefreshTests`, `RefreshFreshnessTests_Boundaries`, `RefreshLifecycleTests_Background`, `RefreshLoopTests_Scheduling`, `RefreshMonitorTests_Concurrency`, `RefreshSettingsTests_Persistence`, `ResumeMonitorTests_Cancellation`, `StopMonitorTests` — Monitor-/Aktualisierungs-/Wiederaufnahmelogik
- `LocationTests`, `NrwRegionClassifierTests_Boundary` — Standortauflösung, NRW-Regionklassifizierung
- `MapTests` — Kartenlogik
- `MemoryTransitCacheTests_Lifetime` — Cache-Lebensdauer
- `ProviderOrchestratorTests_Concurrency`/`_Fallback`/`_RegionResolution`/`_Union` — Orchestrierung, Fallback, Regionsauflösung, Union regionaler+bundesweiter Ergebnisse
- `RealtimeConsolidatorTests_Identity` — Echtzeitkonsolidierung/Identitätsprüfung
- `RetryPolicyTests_Bounds` — Wiederholungsrichtlinie
- `RoutingServiceTests_NextJourneys` — Verbindungssuche
- `TransitHttpGatewayTests_BodyFailures`/`_Execution`/`_Security` — HTTP-Gateway
- `TransitModelTests_Validation`, `TransitServiceTests_InvalidReplacement`, `TransitServiceTests_Validation` — Modell-/Servicevalidierung
- `Transit/` (Unterordner): `DbRestProviderTests_Requests`, `DbRestResponseMapperTests_Normalization`, `EfaProviderTests_Requests`, `EfaResponseMapperTests_Journeys`, `EfaResponseMapperTests_Locations` — Provideradapter/Mapping

### Release-Skripttests (`node:test`, `scripts/`)

- `resolve-release-version.test.mjs` — Versions-/Tag-Auflösung, Release-Klassifizierung (automatisch/manuell), Asset-Reparatur-Entscheidungen (24 der 26 Tests)
- `generate-update-manifest.test.mjs` — `update.json`-Erzeugung, Asset-URLs, Zeitstempel-Fallback (2 Tests)

## Hilfsmethoden

### `TransitTestSupport` (`FlowNRW.Tests/TransitTestSupport.cs`)

Gemeinsame Testdaten/Fixtures für Transit-Tests.

### `AdapterGateway` (`FlowNRW.Tests/Transit/AdapterGateway.cs`)

Gateway-Hilfe für Provideradapter-Tests.

### `UiTestFixtureServices` (`tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`)

Deterministische Fixture-Services für den `UiTest`-Build der App (synthetische Fahrplan-/Positionsdaten).

### PowerShell-Treiber (`tests/WindowsJourneyUiTests/`)

`WindowsJourneyUiTests.ps1`, `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsDepartureCacheUiTests.ps1`, `WindowsDesignUiTests.ps1` — UIAutomation-basierte native Prüfungen inkl. Screenshot-Erzeugung (`-ScreenshotDirectory`, `-Theme`, `-Width`, `-Height`, `-TextScale`); Quelle der Verifikations-Screenshot-Matrizen unter `docs/help/design/verification/`.
