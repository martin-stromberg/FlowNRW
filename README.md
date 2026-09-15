# FlowNRW

.NET 10 MAUI-Anwendung (aktuell Windows). Erste Version: `0.0.1`.

[![PR CI für Staging](https://img.shields.io/github/actions/workflow/status/martin-stromberg/FlowNRW/pr-staging-ci.yml?label=PR%20CI%20f%C3%BCr%20Staging)](https://github.com/martin-stromberg/FlowNRW/actions/workflows/pr-staging-ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)

FlowNRW ist eine C#/.NET-MAUI-Anwendung für bundesweite Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten. Der aktuelle Schritt liefert dafür den plattformunabhängigen Core-Datenkern; eine neue fachliche UI und native iOS-Abnahme gehören zum folgenden Ausbauschritt.

## Aktueller Funktionsstand

- Adress-, Haltestellen- und Koordinatenauflösung einschließlich naher Haltestellen.
- Verbindungs- und Abfahrtsservices mit Umstiegen, Fußwegen, Linien, Betreibern, Geometrien und verfügbaren Soll-/Ist-Daten.
- Konfigurierbare `db.transport.rest`- und EFA-Adapter mit NRW-Priorität, Fallback, Cache, Abbruch und begrenzten Wiederholungen.
- Konservative Echtzeitkonsolidierung: Haltestellen- und Fahrtidentität müssen eindeutig zusammenpassen; fehlende Werte bleiben unbekannt.
- Regionale Teilergebnisse werden um zusätzliche bundesweite Fahrten ergänzt; eindeutige Treffer behalten regionale Echtzeitpriorität. [106 Tests und Windows-Build geprüft](docs/help/fahrplanauskunft/verification/union-correction-checks.md).
- Keine neue Benutzeroberfläche in diesem Schritt. Technische Details stehen in der [Fahrplanauskunft-Dokumentation](docs/help/fahrplanauskunft/index.md).

## Projektstruktur

| Projekt | Inhalt |
| --- | --- |
| `FlowNRW/` | MAUI-App (`net10.0-windows10.0.19041.0`, win-x64, unpackaged) |
| `FlowNRW.Core/` | Plattformunabhängige Logik (`net10.0`), von der App referenziert |
| `FlowNRW.Tests/` | xUnit-Tests für `FlowNRW.Core` (`net10.0`, laufen auch unter Linux) |
| `.github/` | CI/CD-Workflows und Composite Actions, siehe [docs/CI-CD.md](docs/CI-CD.md) |
| `.githooks/` | Lokale Git-Hooks (Branch-Schutz, XML-Doc-, Stub- und Format-Checks) |
| `scripts/` | Node-Skripte für Release-Version und Update-Manifest (`update.json`) |
| `docs/` | Projektdokumentation (u. a. [CI-CD.md](docs/CI-CD.md)) |

Die Version wird zentral in `Directory.Build.props` gepflegt (`0.0.1`) und in CI über
`-p:Version=X.Y.Z[-rc.N]` überschrieben (semantic-release, Conventional Commits).

## Voraussetzungen

- Windows 10 (Build 17763+) / Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MAUI-Workload: `dotnet workload install maui-windows`
- Visual Studio 2026 mit ".NET Multi-platform App UI development" **oder** VS Code mit der
  .NET-MAUI-Extension
- Node.js 22 (nur für die Release-Skripte / `npm run test:release-version`)

Die fachliche Provider- und Cachekonfiguration mit sicheren Standardwerten ist in [Installation und Konfiguration](docs/help/fahrplanauskunft/installation.md) beschrieben.

`FlowNRW.Core` und `FlowNRW.Tests` lassen sich ohne MAUI-Workload auch unter Linux/macOS
bauen und testen.

## Erste Schritte

```bash
git clone https://github.com/martin-stromberg/FlowNRW.git
cd FlowNRW
./.githooks/install-hooks.sh        # Windows: .githooks\install-hooks.cmd

dotnet restore FlowNRW.sln
dotnet build FlowNRW.sln -c Debug
dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj
```

App starten (Windows):

```powershell
dotnet build FlowNRW/FlowNRW.csproj -t:Run -f net10.0-windows10.0.19041.0
```

oder `FlowNRW.sln` in Visual Studio öffnen, `FlowNRW` als Startprojekt und
"Windows Machine" als Ziel wählen, F5.

Die Windows-App ist weiterhin die lokale Plattform für Build und Entwicklung. Die fachlichen Fahrplanauskunft-Services werden über Dependency Injection bereitgestellt und können von späteren Views verwendet werden; Schritt 1 liefert noch keinen neuen fachlichen UI-Ablauf.

Release-Build wie in CI (self-contained, unpackaged):

```powershell
dotnet publish FlowNRW/FlowNRW.csproj -c Release -f net10.0-windows10.0.19041.0 -r win-x64 --self-contained -p:Version=0.0.1 -o publish/win-x64
```

## Entwicklung

- Branches: `main` (stabile Releases) und `staging` (Integration, Release Candidates).
  Feature-Branches werden per PR nach `staging` gemerged; Details in
  [CONTRIBUTING.md](CONTRIBUTING.md) und [docs/CI-CD.md](docs/CI-CD.md).
- Commits folgen [Conventional Commits](https://www.conventionalcommits.org/)
  (`feat:` → Minor, `fix:` → Patch, `!`/`BREAKING CHANGE` → Major).
- Formatierung: `dotnet format FlowNRW.sln --verify-no-changes --severity error`
- XML-Dokumentation ist für alle öffentlichen Member Pflicht (`CS1591` ist ein Fehler).

## Konfiguration der Fahrplandaten

| Parameter | Standardwert | Zweck |
|-----------|--------------|-------|
| `TransitProviders:DbRest:BaseUrl` | `https://v6.db.transport.rest/` | Bundesweiter Provider für Suche, Nearby, Journeys und Departures. |
| `TransitProviders:Efa:BaseUrl` | `https://openservice-test.vrr.de/openservice/` | EFA-Entwicklungsprovider und NRW-Rückfall. |
| `TransitHttp:Timeout` | `00:00:10` | Zeitlimit pro Versuch. |
| `TransitHttp:MaxRetries` | `1` | Begrenzte zusätzliche Versuche. |
| `TransitCache:MaxEntries` | `256` | Maximale flüchtige Cacheeinträge. |
| `TransitCache:StopTimeToLive` | `1.00:00:00` | Frische von Stop-/Adressdaten. |
| `TransitCache:RealtimeTimeToLive` | `00:00:30` | Frische von Verbindungen und Echtzeit. |
| `TransitCache:MaxStaleAge` | `00:05:00` | Maximales Alter eines markierten Echtzeit-Rückfalls. |

Nur absolute HTTPS-Endpunkte ohne Benutzerinformationen, Query oder Fragment sind zulässig. Zugangsdaten werden nicht in der README, im Quelltext oder in Logs dokumentiert.

## Architektur

| Projekt/Bereich | Aufgabe |
|----------------|---------|
| `FlowNRW.Core/Transit/` | Modelle, Provideradapter, Orchestrator, Normalisierung, Cache und Diagnose. |
| `FlowNRW/` | MAUI-Komposition und bestehende Windows-App. |
| `FlowNRW.Tests/` | xUnit-Tests für Core und Providerverhalten. |
| `docs/help/fahrplanauskunft/` | Technische API-, Betriebs-, Probe- und Verifikationsdokumentation. |

Der Datenfluss ist `Service → ProviderOrchestrator → DbRestProvider/EfaProvider → TransitHttpGateway → Mapper → ProviderResult`. NRW wird über die amtlich dokumentierte Polygonressource klassifiziert; Fallbacks und Echtzeitkonsolidierung bleiben gekennzeichnet.

## Tests

Core-Tests und Coverage:

```powershell
dotnet build FlowNRW.sln -c Release -p:TreatWarningsAsErrors=true
dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-build --collect:"XPlat Code Coverage"
```

Die aktuelle Verifikation weist 98 von 98 Tests, 0 Fehler, 0 Überspringungen und 98,00 % Core-Zeilenabdeckung (541/552) nach. Der Windows-Solution-Build meldet 0 Warnungen und 0 Fehler. Die Release-Skripte werden mit `npm run test:release-version` geprüft; der dokumentierte Lauf bestand mit 26 von 26 Tests. Einzelheiten und Providerproben stehen in [test-results.md](docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft/test-results.md) und [iteration2-checks.md](docs/help/fahrplanauskunft/verification/iteration2-checks.md).

## CI/CD

Die vorhandene GitHub-Actions-Konfiguration behält Windows-Build/Release sowie Core-Tests und Coverage bei. Eine iOS-CI- oder iOS-Deployment-Pipeline wird in diesem Projektabschnitt nicht eingeführt.

## Roadmap

- Die fachliche UI für Verbindungssuche und Abfahrtsmonitor auf dem vorhandenen Datenkern ergänzen; iOS-Zielplattform und native Abnahme folgen in Schritt 2 (`TargetFrameworks` in `FlowNRW/FlowNRW.csproj`,
  `Platforms/Android`, `Platforms/iOS`, Runtime-Einträge in
  `scripts/generate-update-manifest.mjs` / `scripts/resolve-release-version.mjs` und
  in `.github/actions/build-and-package/action.yml`).

Weitere Verkehrsverbünde sowie spätere Sharing-/Push-Funktionen bleiben Erweiterungspunkte. Sie sind in der technischen Dokumentation beschrieben, aber nicht als aktuelle Produktfunktion implementiert.

## Changelog

Änderungen der Fahrplanauskunft stehen in [`changes.log`](changes.log) und in der [technischen Dokumentation](docs/help/fahrplanauskunft/index.md).
