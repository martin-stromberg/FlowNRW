# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine.

## Geprüfte Dateien

Basisbranch: `staging` (Upstream-Basis des Task-Branches, explizit aus dem
Ereignis-Kontext). Geänderte/neue Quelldateien:

- `.github/workflows/review-architecture-security.yml`
- `.github/workflows/review-nuget-weekly.yml`
- `.github/workflows/review-compliance-yearly.yml`
- `.github/actions/create-review-issue/action.yml`
- `.github/review-bodies/architecture-security.md`
- `.github/review-bodies/nuget-weekly.md`
- `.github/review-bodies/compliance-yearly.md`

## Prüfnotizen

Anwendbare Kriterien (YAML/Shell statt C#):

- **Doppelter Code:** Label-Anlage, Duplikat-Prüfung und Issue-Erzeugung liegen
  genau einmal in der Composite Action — entspricht der im Repo dokumentierten
  Konvention (vgl. Kommentar in `security-scan.yml`). Die drei Workflows
  unterscheiden sich nur in `name`, Cron, Kommentar und Inputs.
- **Namenskonventionen:** Datei- und Label-Namen folgen den bestehenden
  (`automated-promotion`/`automated-backmerge` → `automated-review-*`;
  kebab-case Workflow-Dateinamen).
- **Fehlerbehandlung:** `@(...)` um `ConvertFrom-Json` erzwingt Array-Semantik
  auch bei genau einem Treffer; leere Ergebnisse (`[]`) werden korrekt als
  Count 0 behandelt. `gh`-Aufrufe brechen bei Fehlern den Step ab
  (Exit-Code ≠ 0 → Step/Job failed, sichtbar im Actions-Log).
- **Kopplung:** Der Token wird explizit als Input übergeben statt implizit im
  Action-Context auf `github.token` zuzugreifen — robuster und prüfbar.
- **Toter Code:** Keine ungenutzten Inputs, Steps oder Dateien; jede
  Body-Datei wird von genau einem Workflow referenziert.
- **Randfälle:** Die Titel-Fallback-Suche nutzt `in:title` mit dem stabilen
  Präfix; das Datums-Suffix kann die Suche nicht verfehlen lassen, da der
  Präfix konstant bleibt.
- **Sicherheit:** Kein Secret im Klartext; ausschließlich `GITHUB_TOKEN` mit
  minimalen Scopes (`contents: read`, `issues: write`).
