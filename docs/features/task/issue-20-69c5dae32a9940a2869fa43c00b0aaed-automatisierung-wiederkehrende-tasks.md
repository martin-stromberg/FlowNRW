# Tasks: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Workflows | `.github/workflows/review-architecture-security.yml` anlegen (Cron `0 5 1 * *`, Label `automated-review-architecture`) | Erledigt | YAML-Parse + Cron-Feldprüfung |
| 2 | Workflows | `.github/workflows/review-nuget-weekly.yml` anlegen (Cron `0 5 * * 1`, Label `automated-review-nuget`) | Erledigt | YAML-Parse + Cron-Feldprüfung |
| 3 | Workflows | `.github/workflows/review-compliance-yearly.yml` anlegen (Cron `0 5 1 1 *`, Label `automated-review-compliance`) | Erledigt | YAML-Parse + Cron-Feldprüfung |
| 3a | Workflows | Composite Action `.github/actions/create-review-issue/action.yml` anlegen | Erledigt | YAML-Parse + pwsh-Syntaxprüfung |
| 3b | Workflows | Drei Issue-Body-Dateien unter `.github/review-bodies/` anlegen | Erledigt | Inhalt wörtlich gegen Anforderung geprüft |
| 4 | Verifikation | YAML-Parse aller drei Workflows + Cron-Felder prüfen | Erledigt | `yaml.safe_load`, Trigger/Permissions/Steps verifiziert |
| 5 | Verifikation | Reale Duplikat-Abfrage via `gh issue list` gegen das Repo ausführen | Erledigt | 4 Queries, valide `[]`-Ergebnisse |
| 6 | Dokumentation | `docs/CI-CD.md` Workflow-Tabelle erweitern | Erledigt | Diff-Prüfung |
| 7 | Dokumentation | Repo-Dokumentation der Lösung | Erledigt | Abschnitt „Geplante Review-Issues" in `docs/CI-CD.md` (kein `docs/help/`-Eintrag: Bereich ist App-Anwenderhilfe, Feature ist Repo-Automatisierung) |
| 8 | Dokumentation | `changes.log` und `docs/RELEASE_NOTES.md` aktualisieren; README prüfen | Erledigt | `changes.log`-Eintrag + Release-Notes-Bullets; README unverändert (`.github/`-Eintrag deckt Inhalt bereits) |
