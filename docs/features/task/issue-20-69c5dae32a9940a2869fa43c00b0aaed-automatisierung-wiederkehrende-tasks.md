# Tasks: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Workflows | `.github/workflows/review-architecture-security.yml` anlegen (Cron `0 5 1 * *`, Label `automated-review-architecture`) | Offen | — |
| 2 | Workflows | `.github/workflows/review-nuget-weekly.yml` anlegen (Cron `0 5 * * 1`, Label `automated-review-nuget`) | Offen | — |
| 3 | Workflows | `.github/workflows/review-compliance-yearly.yml` anlegen (Cron `0 5 1 1 *`, Label `automated-review-compliance`) | Offen | — |
| 4 | Verifikation | YAML-Parse aller drei Workflows + Cron-Felder prüfen | Offen | — |
| 5 | Verifikation | Reale Duplikat-Abfrage via `gh issue list` gegen das Repo ausführen | Offen | — |
| 6 | Dokumentation | `docs/CI-CD.md` Workflow-Tabelle erweitern | Offen | — |
| 7 | Dokumentation | Neuen Hilfebereich unter `docs/help/` anlegen inkl. `docs/help/index.md`-Eintrag | Offen | — |
| 8 | Dokumentation | `changes.log` und `docs/RELEASE_NOTES.md` aktualisieren; README prüfen | Offen | — |
