# FlowNRW

.NET 10 MAUI-Anwendung mit geprüfter Windows-Oberfläche und iOS-Plattformbasis. Erste Version: `0.0.1`.

[![PR CI für Staging](https://img.shields.io/github/actions/workflow/status/martin-stromberg/FlowNRW/pr-staging-ci.yml?label=PR%20CI%20f%C3%BCr%20Staging)](https://github.com/martin-stromberg/FlowNRW/actions/workflows/pr-staging-ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)

FlowNRW ist eine C#/.NET-MAUI-Anwendung für bundesweite Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten. Die manuelle Verbindungssuche führt von Start-/Zielwahl über Ergebnisse zu Verbindungsdetails. Native iOS-Abnahme steht beim Nutzer aus.

## Aktueller Funktionsstand

- Startseite mit dauerhaft gespeicherten Favoriten, unabhängigen Abfahrtstafeln und expliziter Entfernungssortierung. [Bedienung und Speicherung](docs/help/favoriten/index.md).
- Vordergrundmonitore können mit Aus/30/60/120/300 Sekunden automatisch aktualisiert werden (Standard 60). Die Auswahl wird erst nach erfolgreichem Speichern wirksam; Navigation und Fensterdeaktivierung stoppen automatische Abrufe. Manuelle Aktualisierung bleibt verfügbar. [Bedienung, Speicherung und iOS-Prüfung](docs/help/monitorintervalle/index.md).
- Wiederaufnahme erneuert veraltete sichtbare Abfahrten und Verbindungen ohne Seitensprung. iOS-Hintergrundaktualisierung für Favoriten ist registriert und zeitlich begrenzt; ihre native Geräteabnahme steht aus. **Aus** deaktiviert auch automatische Resume-/Hintergrundabrufe. [Ablauf und Grenzen](docs/help/monitorintervalle/ablauf-technisch.md).

- Adress-, Haltestellen- und Koordinatenauflösung einschließlich naher Haltestellen.
- Verbindungs- und Abfahrtsservices mit Umstiegen, Fußwegen, Linien, Betreibern, Geometrien und verfügbaren Soll-/Ist-Daten.
- Konfigurierbare `db.transport.rest`- und EFA-Adapter mit NRW-Priorität, Fallback, Cache, Abbruch und begrenzten Wiederholungen.
- Konservative Echtzeitkonsolidierung: Haltestellen- und Fahrtidentität müssen eindeutig zusammenpassen; fehlende Werte bleiben unbekannt.
- Regionale Teilergebnisse werden um zusätzliche bundesweite Fahrten ergänzt; eindeutige Treffer behalten regionale Echtzeitpriorität. [106 Tests und Windows-Build geprüft](docs/help/fahrplanauskunft/verification/union-correction-checks.md).
- Native Suche mit Adress-/Haltestellentreffern und Koordinaten, Ergebnissen und Details; Lade-/Fehler-/Leerzustände, Quellen und Datenalter sind sichtbar. [Bedienung](docs/help/verbindungssuche/beschreibung.md).
- Native Haltestellensuche und manueller Abfahrtsmonitor mit Echtzeitstatus, Gleiswechseln und erhaltenen Daten bei Aktualisierungsfehlern. [Bedienung](docs/help/abfahrten/beschreibung.md).
- Interaktive Haltestellenkarte mit nativer Listenalternative und gelieferten Verbindungsverläufen. [Bedienung und Kartenkonfiguration](docs/help/karte/beschreibung.md).
- Expliziter aktueller Standort als Start/Ziel und nahe Haltestellen in Liste/Karte; Fehler erhalten die manuelle Nutzung. [Bedienung und iOS-Prüfung](docs/help/standort/index.md). Native Windows-Fixtures und echter Windows-Standortversuch erfolgreich.
- iOS-Einstieg und Zielplattform vorhanden; [Visual-Studio-/iOS-Einrichtung und manuelle Prüfliste](docs/help/verbindungssuche/installation.md).

## Projektstruktur

| Projekt | Inhalt |
| --- | --- |
| `FlowNRW/` | MAUI-App: Windows (win-x64, unpackaged), iOS auf macOS oder mit `EnableIos=true` |
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

Unter Windows startet die Favoritenübersicht. „Verbindung suchen“ öffnet die Start-/Zielsuche; „Haltestelle hinzufügen“ führt zu Suche und Abfahrtsmonitor. Über „Aktualisierung einstellen“ lässt sich das gemeinsame Vordergrundintervall speichern. Für Verbindungen Start und Ziel suchen und je einen Treffer auswählen oder Koordinaten übernehmen; „Verbindungen suchen“ öffnet Ergebnisse und anschließend Details. Debug und Release verwenden reale Provider. Manuelle Suche benötigt keine Standortfreigabe.

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
| `FlowNRW/` | MAUI-Komposition, native Suche/Ergebnisse/Details und Windows-/iOS-Einstiege. |
| `FlowNRW.Tests/` | xUnit-Tests für Core, Providerverhalten und Präsentationszustände. |
| `docs/help/fahrplanauskunft/` | Technische API-, Betriebs-, Probe- und Verifikationsdokumentation. |

Der Datenfluss ist `Service → ProviderOrchestrator → DbRestProvider/EfaProvider → TransitHttpGateway → Mapper → ProviderResult`. NRW wird über die amtlich dokumentierte Polygonressource klassifiziert; Fallbacks und Echtzeitkonsolidierung bleiben gekennzeichnet.

## Tests

Core-Tests und Coverage:

```powershell
dotnet build FlowNRW.sln -c Release -p:TreatWarningsAsErrors=true
dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-build --collect:"XPlat Code Coverage"
```

Die historische [Verifikation der Verbindungssuche vom 16.09.2026](docs/help/verbindungssuche/verification/checks-2026-09-16.md) dokumentiert den damaligen Build-/Teststand und separate echte Bedienproben. Nachweise gelten jeweils für den dort genannten Stand; sie sind keine Abnahme späterer Funktionen. [UI-Harness ausführen](tests/WindowsJourneyUiTests/README.md). `UiTest` ist ein isolierter Fixture-Build; nur reguläre Releaseartefakte ausliefern. Die native iOS-Geräteprüfung bleibt beim Nutzer. Die bestehenden Release-Skripttests laufen mit `npm run test:release-version`.

## CI/CD

Die vorhandene GitHub-Actions-Konfiguration behält Windows-Build/Release sowie Core-Tests und Coverage bei. Eine iOS-CI- oder iOS-Deployment-Pipeline wird in diesem Projektabschnitt nicht eingeführt.

## Roadmap

- Native iOS-Ausführung anhand der Prüfliste durch den Nutzer; die Plattformbasis ist implementiert.
- Hintergrundaktualisierung und Wiederaufnahme sind implementiert; finale Schritt-8-Abnahme und native iOS-Geräteprüfung stehen noch aus.
- Die abschließende Gestaltung und visuelle Abnahme anhand des gelieferten Entwurfs folgen verbindlich in Schritt 9. [Design-Abnahmekriterien](docs/design/acceptance.md).

Weitere Verkehrsverbünde sowie spätere Sharing-/Push-Funktionen bleiben Erweiterungspunkte. Sie sind in der technischen Dokumentation beschrieben, aber nicht als aktuelle Produktfunktion implementiert.

## Changelog

Änderungen stehen in [`changes.log`](changes.log) und in der [technischen Dokumentation](docs/help/fahrplanauskunft/index.md).
