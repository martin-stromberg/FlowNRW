# CI/CD-Workflows, Releases und GitHub-Pages-Optionen

Bestand der Automatisierung in `.github/workflows/` und `.github/actions/`, soweit für eine GitHub-Pages-Auslieferung und Release-/Download-Verweise relevant.

## Vorhandene Workflows

| Datei | Trigger | Jobs | Zweck |
|---|---|---|---|
| `verify-pr-source.yml` | `pull_request` → `main` | `verify-source` | Bricht ab, wenn der PR-Quellbranch nicht `staging` ist |
| `pr-staging-ci.yml` | `pull_request` → `staging` | `detect-backmerge`, `static-checks`, `build-and-test`, `release-scripts-test`, `back-merge-skip` | Quality Gates für Feature-PRs; Back-Merge-PRs aus `main` werden erkannt und übersprungen |
| `staging-ci.yml` (Anzeigename „Pre-Release") | `push` → `staging` | `detect-backmerge`, `static-checks`, `build-and-test`, `release-scripts-test`, `version`, `package-ios`, `prerelease`, `back-merge-skip` | Quality Gates + RC-Prerelease `vX.Y.Z-rc.N` |
| `staging-to-main-promotion.yml` | `workflow_run` von „Pre-Release" (`completed`, `staging`) | `promote` | Draft-PR `staging` → `main` (Label `automated-promotion`); nur bei Erfolg |
| `release.yml` | `push` → `main`, Tag `v*.*.*` | `release` | Stabiles Release inkl. Asset-Reparatur über `scripts/resolve-release-version.mjs` |
| `sync-staging-with-main.yml` | `push` → `main` | `backmerge` | PR `main` → `staging` (Label `automated-backmerge`), Merge-Commit erforderlich |
| `security-scan.yml` | `schedule` Mo 04:00 UTC, `workflow_dispatch` | `security-scan` | NuGet-Vulnerability-Scan (`windows-latest`) |
| `review-architecture-security.yml` | `schedule` `0 5 1 * *`, `workflow_dispatch` | `create-issue` | Monatliches Review-Issue |
| `review-nuget-weekly.yml` | `schedule` `0 5 * * 1`, `workflow_dispatch` | `create-issue` | Wöchentliches Review-Issue |
| `review-compliance-yearly.yml` | `schedule` `0 5 1 1 *`, `workflow_dispatch` | `create-issue` | Jährliches Review-Issue |

Hinweise aus den Workflow-Köpfen:

- `schedule` und `workflow_dispatch` der Review-Workflows wirken erst, wenn die Dateien auf dem Default-Branch `main` liegen (`review-*.yml`, Kopfkommentare).
- `staging-to-main-promotion.yml` referenziert den **Anzeigenamen** „Pre-Release" von `staging-ci.yml` — beide müssen synchron geändert werden.
- Genutzte Actions-Versionen (Beispiele): `actions/checkout@v7`, `actions/setup-dotnet@v6` (`.NET 10.0.x`), `actions/setup-node@v7` (Node 22). Permissions werden je Workflow minimal gesetzt (`contents: read`, `checks: write`, `pull-requests: write` u. a.).
- **Kein Pages-Workflow:** `actions/configure-pages`, `actions/upload-pages-artifact` und `actions/deploy-pages` kommen im gesamten `.github/`-Bestand nicht vor.

## Composite Actions (`.github/actions/`)

| Action | Zweck |
|---|---|
| `setup-maui-workload` | `dotnet workload install maui-windows` auf Windows-Runnern |
| `security-scan` | `dotnet list package --vulnerable --include-transitive`, schlägt bei Funden fehl |
| `build-and-package` | Publish `FlowNRW/FlowNRW.csproj` (win-x64, self-contained, unpackaged, `-p:Version`), erzeugt `FlowNRW-v<version>-win-x64.zip`, `release-metadata.json` und `update.json` (via `scripts/generate-update-manifest.mjs`) |
| `package-ios` | iOS-Paketschritt in `staging-ci.yml` (Job `package-ios`) |
| `create-review-issue` | Idempotente Erstellung geplanter Review-Issues inkl. Label und Duplikatschutz; Issue-Texte in `.github/review-bodies/` |

## Release- und Download-Verweise

- **Release-Assets** (aus `.github/actions/build-and-package/action.yml` und `release.yml`):
  `FlowNRW-vX.Y.Z-win-x64.zip` (Windows-Paket) und `update.json` (Update-Manifest mit Version, Release Notes, Asset-URL/SHA-256/Größe). Windows-only; kein Store-/MSIX-Angebot.
- **Versionierung:** semantic-release mit Conventional Commits (`release.config.js`); `feat` → Minor, `fix` → Patch; `docs`/`refactor`/`chore` erzeugen kein Release. Stable Tags `vX.Y.Z` auf `main`, RCs `vX.Y.Z-rc.N` auf `staging`.
- **Stabile Download-URL:** GitHub stellt `https://github.com/martin-stromberg/FlowNRW/releases/latest` als Redirect auf das neueste stabile Release bereit; die Releases-Übersicht liegt unter `.../releases`.
- **Dokumentation:** `docs/CI-CD.md` enthält die Workflow-Tabelle (Zeilen 38–49), die Quality-Gate-Tabelle (Zeilen 92–101) und den Abschnitt „Einmalige Repo-Einrichtung (GitHub)" (Zeilen 117–130) — dort sind bisherige manuelle Settings-Schritte dokumentiert (Actions-PR-Erlaubnis, Branch Protection, Dependabot `.github/dependabot.yml`).

## GitHub-Pages-Ausgangslage (API-geprüft, 2026-11-16)

- `gh api repos/martin-stromberg/FlowNRW/pages` → **404 Not Found**: Es existiert keine Pages-Site-Konfiguration.
- `gh api repos/martin-stromberg/FlowNRW` → `has_pages: false`, `visibility: public`, `default_branch: main`.
- Damit fehlt jede Voraussetzung für Auslieferung: weder eine „Deploy from branch"-Quelle noch ein Actions-basiertes Pages-Deployment ist eingerichtet.
- Im Repository existiert kein Seitenquellordner (`site/`, `docs/page/` o. ä. nicht vorhanden) und keine Jekyll-/Static-Site-Generator-Konfiguration. `.gitignore` enthält keine site-/jekyll-bezogenen Einträge.

## Branch-Strategie (veröffentlichungsrelevant)

- Veröffentlichungskette: Feature-Branch → PR gegen `staging` → Merge → automatischer RC → Draft-Promotion-PR → manueller Merge nach `main`.
- `main` ist Default-Branch und trägt stabile Releases; `staging` ist Integrationsbranch mit RC-Prereleases.
- Direkte Commits auf `main`/`staging` werden lokal durch `.githooks/pre-commit` blockiert; serverseitig durch Branch Protection (Required Checks, ≥1 Approval, laut `docs/CI-CD.md` Zeilen 124–128).
