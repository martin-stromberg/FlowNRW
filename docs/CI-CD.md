# CI/CD und Branch-Strategie

Pipeline und Git-Strategie für FlowNRW, nach dem Muster aus
[Pattern-Collection/CI-Workflows](https://github.com/martin-stromberg/Pattern-Collection)
bzw. [FinanceManager](https://github.com/martin-stromberg/FinanceManager), angepasst an eine
Windows-only MAUI-App.

## Branch-Strategie

```
Feature-Branch
    ↓ Pull Request gegen staging
    ├→ verify-pr-source.yml      (nur bei PRs gegen main: Quelle muss staging sein)
    ├→ pr-staging-ci.yml         (static checks, build & test, release-scripts test)
    ↓ Merge nach staging
    ├→ staging-ci.yml "Pre-Release"
    │    ├→ dieselben Checks wie im PR
    │    ├→ version:   semantic-release --dry-run → X.Y.Z, RC-Nummer → vX.Y.Z-rc.N
    │    └→ prerelease: build-and-package → GitHub-Prerelease mit ZIP + update.json
    ├→ staging-to-main-promotion.yml (nach erfolgreichem Pre-Release)
    │    └→ Draft-PR staging → main, falls main hinter staging liegt
    ↓ Maintainer merged Promotion-PR
    ├→ release.yml
    │    ├→ resolve-release-version.mjs (semantic-release --dry-run oder vX.Y.Z-Tag)
    │    ├→ Release-Gate: dotnet test FlowNRW.Tests
    │    ├→ build-and-package → FlowNRW-vX.Y.Z-win-x64.zip + update.json
    │    └→ GitHub Release (semantic-release bzw. gh release create)
    └→ sync-staging-with-main.yml
         └→ PR main → staging ("Create a merge commit"), damit der Release-Tag auf
            staging erreichbar bleibt und die nächste RC-Berechnung stimmt
```

Tags: stabile Releases `vX.Y.Z`, Release Candidates `vX.Y.Z-rc.N`. Die Version im Repo
(`Directory.Build.props`, `0.0.1`) ist nur der lokale Default; CI setzt `-p:Version`.

## Workflows

| Datei | Trigger | Aufgabe |
| --- | --- | --- |
| `verify-pr-source.yml` | PR gegen `main` | Bricht ab, wenn der Quellbranch nicht `staging` ist |
| `pr-staging-ci.yml` | PR gegen `staging` | Quality Gates (siehe unten); Back-Merge-PRs aus `main` werden erkannt und übersprungen |
| `staging-ci.yml` ("Pre-Release") | Push auf `staging` | Quality Gates + RC-Prerelease |
| `staging-to-main-promotion.yml` | `workflow_run` von "Pre-Release" | Draft-PR `staging` → `main` (Label `automated-promotion`) |
| `release.yml` | Push auf `main`, Tag `v*.*.*` | Stabiles Release inkl. Asset-Reparatur |
| `sync-staging-with-main.yml` | Push auf `main` | PR `main` → `staging` (Label `automated-backmerge`) |
| `security-scan.yml` | wöchentlich Mo 04:00 UTC, manuell | NuGet-Vulnerability-Scan der Solution |
| `review-architecture-security.yml` | monatlich am 1., 05:00 UTC, manuell | Issue für das Architektur- und Sicherheitsreview (Label `automated-review-architecture`) |
| `review-nuget-weekly.yml` | wöchentlich Mo 05:00 UTC, manuell | Issue für die NuGet-Paketprüfung (Label `automated-review-nuget`) |
| `review-compliance-yearly.yml` | jährlich am 1. Januar, 05:00 UTC, manuell | Issue für das Compliance- und Copyright-Review (Label `automated-review-compliance`) |

Composite Actions (`.github/actions/`):

- `setup-maui-workload`: `dotnet workload install maui-windows` (Windows-Runner).
- `create-review-issue`: Legt das Typ-Label an, prüft auf offene Issues desselben
  Typs (Label, Fallback Titel-Präfix) und erzeugt das Review-Issue nur, wenn
  keines offen ist (siehe unten).
- `security-scan`: `dotnet list package --vulnerable --include-transitive`, schlägt bei Funden fehl.
- `build-and-package`: Publish `FlowNRW/FlowNRW.csproj` (win-x64, self-contained, unpackaged,
  `-p:Version`), ZIP `FlowNRW-v<version>-win-x64.zip`, `release-metadata.json` im Publish-Output,
  `update.json` via `scripts/generate-update-manifest.mjs`.

## Geplante Review-Issues (Issue #20)

Drei Zeitplan-Workflows erzeugen wiederkehrende Review-Issues, die
`@softwarewerkstatt` erwähnen und damit die automatische Analyse anstoßen:

| Workflow | Zeitplan (UTC) | Typ-Label | Issue-Titel |
| --- | --- | --- | --- |
| `review-architecture-security.yml` | `0 5 1 * *` (monatlich am 1.) | `automated-review-architecture` | `Architektur- und Sicherheitsreview – <JJJJ-MM>` |
| `review-nuget-weekly.yml` | `0 5 * * 1` (montags) | `automated-review-nuget` | `NuGet-Prüfung – <JJJJ-MM-TT>` |
| `review-compliance-yearly.yml` | `0 5 1 1 *` (1. Januar) | `automated-review-compliance` | `Compliance- und Copyright-Review – <JJJJ>` |

Gemeinsame Logik liegt in der Composite Action `.github/actions/create-review-issue`:

1. `gh label create <typ-label> --force` legt das Typ-Label idempotent an.
2. `gh issue list --state open --label <typ-label>` sucht offene Issues desselben
   Typs; ist das Ergebnis leer, greift die Fallback-Suche
   `gh issue list --state open --search 'in:title "<präfix>"'` — so werden auch
   Issues erkannt, denen das Label fehlt.
3. Nur bei leerem Ergebnis legt `gh issue create` ein neues Issue mit dem
   Typ-Label und dem Body aus `.github/review-bodies/` an. Andernfalls endet der
   Lauf mit einem Log-Hinweis — es entstehen keine Duplikate aus dem
   vorherigen Zyklus.

Die Issue-Texte stehen wörtlich in `.github/review-bodies/` und können dort
angepasst werden. Alle drei Workflows lassen sich zusätzlich manuell über
`workflow_dispatch` starten. Wichtig: `schedule` und `workflow_dispatch`
funktionieren erst, wenn die Workflow-Dateien auf dem Default-Branch (`main`)
vorhanden sind — die Automatisierung wird also erst mit der Promotion
`staging` → `main` aktiv.

## Quality Gates (pr-staging-ci.yml / staging-ci.yml)

| Job | Runner | Gate |
| --- | --- | --- |
| static checks | windows-latest | 1. `dotnet format --verify-no-changes --severity error`<br>2. Security-Scan<br>3. `dotnet build FlowNRW.sln -c Release -p:TreatWarningsAsErrors=true` (inkl. MAUI-App) |
| build & test | ubuntu-latest | 4. `dotnet test FlowNRW.Tests` mit Coverage, **mindestens 70 % Line Coverage** (ReportGenerator) |
| release scripts | ubuntu-latest | `npm ci && npm run test:release-version` |

Die MAUI-App selbst kann nur auf Windows gebaut werden; Tests laufen ausschließlich gegen
`FlowNRW.Core`, deshalb ist "build & test" schnell und plattformunabhängig.

## Releases

- **Version:** semantic-release mit `conventionalcommits` (`release.config.js`).
  `feat` → Minor, `fix` → Patch, Breaking → Major; `docs`/`refactor`/`chore` erzeugen kein Release.
- **RC auf staging:** `staging-ci.yml` ermittelt X.Y.Z per `--dry-run --branches staging` und
  hängt `-rc.N` an (N = nächste freie Nummer der vorhandenen `vX.Y.Z-rc.*`-Tags).
- **Asset-Reparatur:** `scripts/resolve-release-version.mjs` erkennt ein vorhandenes Release
  ohne vollständige Assets (`FlowNRW-vX.Y.Z-win-x64.zip`, `update.json`) und lädt sie nach,
  statt ein Duplikat zu erzeugen. Prereleases werden nie repariert.
- **Manuelles Release:** Tag `vX.Y.Z` pushen → `release.yml` baut und veröffentlicht mit
  `gh release create`.
- **update.json:** Version, Release Notes, pro Asset Plattform/RID/URL/SHA-256/Größe; Basis für
  einen späteren In-App-Updater.

## Einmalige Repo-Einrichtung (GitHub)

1. Branch `staging` von `main` anlegen (Default-Branch kann `main` bleiben).
2. Labels `automated-promotion` und `automated-backmerge` werden von den Workflows bei Bedarf
   angelegt (benötigt `pull-requests: write`, ist gesetzt).
3. Settings → Actions → General → "Allow GitHub Actions to create and approve pull requests"
   aktivieren (sonst können Promotion-/Back-Merge-PRs nicht erstellt werden).
4. Branch Protection:
   - `staging`: Required checks `static checks`, `build & test`, `release scripts`;
     mindestens 1 Approval; kein Direct Push.
   - `main`: Required check `verify-source` (Workflow "Verify PR Source"); mindestens 1 Approval; Branch muss aktuell
     sein; kein Direct Push.
5. Dependabot (`.github/dependabot.yml`) öffnet wöchentliche PRs gegen `staging` für NuGet,
   npm und GitHub Actions.

## Fehlersuche

- **Promotion-PR wird nicht erstellt:** `staging-to-main-promotion.yml` reagiert auf den
  Display-Namen `Pre-Release` von `staging-ci.yml`; beide müssen zusammen geändert werden.
  Außerdem Punkt 3 der Einrichtung prüfen.
- **Coverage < 70 %:** Neue Logik nach `FlowNRW.Core` verschieben und in `FlowNRW.Tests`
  abdecken; die MAUI-App zählt nicht zur Coverage.
- **RC-Version niedriger als letztes Release:** Der Back-Merge-PR `main` → `staging` wurde
  nicht (oder per Squash) gemerged; mit "Create a merge commit" nachholen.
- **Release nur teilweise veröffentlicht:** Erneuten Push auf `main` (oder Workflow-Rerun)
  auslösen; `resolve-release-version.mjs` lädt fehlende Assets zum bestehenden Tag nach.
- **MAUI-Build in CI schlägt beim Workload-Install fehl:** `setup-maui-workload/action.yml`
  prüfen; das Workload muss zur `dotnet-version` (`10.0.x`) passen.
