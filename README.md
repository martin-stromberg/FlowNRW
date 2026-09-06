# FlowNRW

.NET 10 MAUI-Anwendung (aktuell Windows). Erste Version: `0.0.1`.

## Projektstruktur

| Projekt | Inhalt |
| --- | --- |
| `FlowNRW/` | MAUI-App (`net10.0-windows10.0.19041.0`, win-x64, unpackaged) |
| `FlowNRW.Core/` | Plattformunabhängige Logik (`net10.0`), von der App referenziert |
| `FlowNRW.Tests/` | xUnit-Tests für `FlowNRW.Core` (`net10.0`, laufen auch unter Linux) |
| `.github/` | CI/CD-Workflows und Composite Actions, siehe [CI-CD.md](CI-CD.md) |
| `.githooks/` | Lokale Git-Hooks (Branch-Schutz, XML-Doc-, Stub- und Format-Checks) |
| `scripts/` | Node-Skripte für Release-Version und Update-Manifest (`update.json`) |

Die Version wird zentral in `Directory.Build.props` gepflegt (`0.0.1`) und in CI über
`-p:Version=X.Y.Z[-rc.N]` überschrieben (semantic-release, Conventional Commits).

## Voraussetzungen

- Windows 10 (Build 17763+) / Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MAUI-Workload: `dotnet workload install maui-windows`
- Visual Studio 2026 mit ".NET Multi-platform App UI development" **oder** VS Code mit der
  .NET-MAUI-Extension
- Node.js 22 (nur für die Release-Skripte / `npm run test:release-version`)

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

Release-Build wie in CI (self-contained, unpackaged):

```powershell
dotnet publish FlowNRW/FlowNRW.csproj -c Release -f net10.0-windows10.0.19041.0 -r win-x64 --self-contained -p:Version=0.0.1 -o publish/win-x64
```

## Entwicklung

- Branches: `main` (stabile Releases) und `staging` (Integration, Release Candidates).
  Feature-Branches werden per PR nach `staging` gemerged; Details in
  [CONTRIBUTING.md](CONTRIBUTING.md) und [CI-CD.md](CI-CD.md).
- Commits folgen [Conventional Commits](https://www.conventionalcommits.org/)
  (`feat:` → Minor, `fix:` → Patch, `!`/`BREAKING CHANGE` → Major).
- Formatierung: `dotnet format FlowNRW.sln --verify-no-changes --severity error`
- XML-Dokumentation ist für alle öffentlichen Member Pflicht (`CS1591` ist ein Fehler).

## Roadmap

- Android- und iOS-Targets ergänzen (`TargetFrameworks` in `FlowNRW/FlowNRW.csproj`,
  `Platforms/Android`, `Platforms/iOS`, Runtime-Einträge in
  `scripts/generate-update-manifest.mjs` / `scripts/resolve-release-version.mjs` und
  in `.github/actions/build-and-package/action.yml`).
