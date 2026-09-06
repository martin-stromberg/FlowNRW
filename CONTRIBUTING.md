# Contributing

## Branch-Workflow (staging / main)

- `main`: stabile Releases. Nur PRs aus `staging` sind erlaubt (`verify-pr-source.yml`).
- `staging`: Integrationsbranch. Jeder Push erzeugt automatisch ein RC-Prerelease
  (`vX.Y.Z-rc.N`).
- Feature-/Fix-Branches (`feat/...`, `fix/...`) zweigen von `staging` ab und werden per PR
  nach `staging` gemerged.
- Direkte Commits/Pushes auf `main` und `staging` werden durch die Git-Hooks blockiert.

Details zu Promotion (`staging` → `main`), Back-Merge (`main` → `staging`) und Releases in
[CI-CD.md](CI-CD.md).

## Git-Hooks

Einmalig nach dem Klonen aktivieren:

```bash
./.githooks/install-hooks.sh        # Windows: .githooks\install-hooks.cmd
```

Der `pre-commit`-Hook blockiert Commits auf `main`/`staging`, prüft die XML-Dokumentation
(`.csproj`-Konfiguration und `<param>`/`<returns>`-Vollständigkeit), warnt vor
`NotImplementedException`/Stub-Membern und ungetesteten Enums und führt `dotnet format
--verify-no-changes` für `FlowNRW.Core` und `FlowNRW.Tests` aus. Der `pre-push`-Hook
blockiert Pushes auf `main`/`staging` und macht die Stub-/Enum-Checks für das gesamte Repo
blockierend. Voraussetzung: `python` (3.x) und das .NET SDK im `PATH`.

## Commits

Conventional Commits, da semantic-release die Version daraus ableitet:

| Präfix | Release |
| --- | --- |
| `feat:` | Minor |
| `fix:` | Patch |
| `feat!:` / `BREAKING CHANGE:` im Footer | Major |
| `docs:`, `refactor:`, `chore:`, `test:`, `ci:` | kein Release |

## Pull Requests

- PRs gehen gegen `staging`. `pr-staging-ci.yml` muss grün sein:
  Format (`dotnet format --severity error`), Security-Scan (verwundbare NuGet-Pakete),
  Build mit `TreatWarningsAsErrors` (inkl. MAUI-App auf Windows), Tests mit mindestens
  70 % Line-Coverage und die Node-Tests der Release-Skripte.
- Öffentliche Member brauchen XML-Dokumentation (`CS1591` ist ein Build-Fehler).
- Kein `NotImplementedException` und keine reinen `throw`-Stubs mergen.
- Plattformunabhängige Logik gehört nach `FlowNRW.Core` und wird in `FlowNRW.Tests`
  getestet; die MAUI-App bleibt möglichst dünn.

## Lokale Checks vor dem Push

```bash
dotnet format FlowNRW.sln --verify-no-changes --severity error   # Windows (Solution enthält MAUI-App)
dotnet build FlowNRW.sln -c Release -p:TreatWarningsAsErrors=true
dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release
npm ci && npm run test:release-version
```

Unter Linux/macOS ohne MAUI-Workload stattdessen die Projekte `FlowNRW.Core` und
`FlowNRW.Tests` einzeln formatieren/bauen/testen.
