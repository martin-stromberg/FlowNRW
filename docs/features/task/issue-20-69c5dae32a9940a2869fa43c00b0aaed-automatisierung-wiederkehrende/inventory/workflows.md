# Workflows und CI-Konventionen

## Vorhandene Workflows (`.github/workflows/`)

| Datei | `name:` | Trigger | Runner | Zweck |
|-------|---------|---------|--------|-------|
| `verify-pr-source.yml` | PR Source Check | PR gegen `main` | – | Quellbranch muss `staging` sein |
| `pr-staging-ci.yml` | PR CI für Staging | PR gegen `staging` | windows-latest | Quality Gates (static checks, build & test, release-scripts test) |
| `staging-ci.yml` | Pre-Release | Push auf `staging` | windows-latest | Quality Gates + RC-Prerelease |
| `staging-to-main-promotion.yml` | Staging to Main Promotion | `workflow_run` von „Pre-Release" | ubuntu-latest | Draft-PR `staging` → `main` |
| `release.yml` | Release | Push auf `main`, Tag `v*.*.*` | windows-latest | Stabiles Release |
| `sync-staging-with-main.yml` | Backmerge Main to Staging | Push auf `main` | ubuntu-latest | PR `main` → `staging` |
| `security-scan.yml` | Security Scan | `schedule` `0 4 * * 1`, `workflow_dispatch` | windows-latest | NuGet-Vulnerability-Scan |

## Beobachtete Konventionen

- **Kommentar-Header:** Jede Workflow-Datei beginnt mit einem erklärenden
  Kommentarblock über Zweck und Besonderheiten.
- **`"on":`-Schreibweise:** Die beiden Automations-Workflows mit `gh`-CLI quoten
  den `on`-Schlüssel (`"on":`); `security-scan.yml` nutzt unquotiertes `on:`.
- **Trigger-Kombination:** `security-scan.yml` kombiniert `schedule` (Cron,
  kommentiert mit Zeitpunkt in UTC) mit `workflow_dispatch` für manuelle Läufe.
- **`concurrency`:** Gruppiert nach Workflow (`security-scan` mit
  `cancel-in-progress: true`; Promotion/Backmerge mit
  `cancel-in-progress: false`, da dort kein Lauf verloren gehen darf).
- **Runner-Wahl:** Reine GitHub-API-Automatisierung läuft auf `ubuntu-latest`
  (Promotion, Backmerge); .NET-Builds/Scans auf `windows-latest`.
- **`gh`-CLI:** Authentifizierung ausschließlich über
  `GH_TOKEN: ${{ github.token }}` als Step-Env; Shell ist durchgehend `pwsh`.
- **Berechtigungen:** Explizite `permissions`-Blöcke auf Workflow-Ebene;
  Promotion/Backmerge nutzen `contents: read`, `pull-requests: write`,
  `issues: write`.
- **Duplikat-Muster:** Vor dem Anlegen eines PRs wird per
  `gh pr list --base ... --state open --json number | ConvertFrom-Json`
  geprüft, ob bereits ein offenes Objekt existiert (`$existing.Count -eq 0`).
- **Labels:** Werden vor Verwendung per `gh label create --force` angelegt
  (`automated-promotion`, `automated-backmerge`).
- **Timeouts:** `timeout-minutes` an allen Jobs (10–15 für Automationsjobs).

## Composite Actions (`.github/actions/`)

`build-and-package`, `package-ios`, `security-scan`, `setup-maui-workload` —
für diese Anforderung nicht relevant (keine Build-/Scan-Logik nötig).

## Fehlende Bausteine

- Kein Workflow erzeugt bisher Issues.
- Kein bestehender Label-Name für Review-Issues; muss neu angelegt werden.
