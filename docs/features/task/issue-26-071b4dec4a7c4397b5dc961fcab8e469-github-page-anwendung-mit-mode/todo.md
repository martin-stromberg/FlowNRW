# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode`

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode/` |
| [x] | – | Einstiegspunkt ermitteln | – |
| [x] | 3 | Anforderung übersetzen (Unteragent) | `requirement.md` |
| [x] | 4 | Bestandsaufnahme (Unteragent) | `inventory.md`, `inventory/` |
| [x] | 5 | Umsetzungsplanung (Unteragent) | `plan.md` |
| [x] | 5a | Offene Punkte prüfen und ggf. Planung wiederholen | `plan.md` (aktualisiert) |
| [x] | 5b | Plan gegen Anforderung und Testbedarf prüfen (Unteragent) | `plan-check.md` |
| [x] | 5c | Planungscommit | – |
| [x] | 6 | Implementierung (Unteragent) | Codeänderungen (`site/`, `pages.yml`, Doku) |
| [x] | 7 | Plan-Review (Unteragent, bedingt) | `review.md` — Vollständig umgesetzt (Post-Merge-Go-Live als dokumentierter Folgeschritt) |
| [x] | 8 | Usability-Review (Unteragent, bei UI-Änderungen) | `review-usability.md` — 4 Iterationen, alle Befunde behoben |
| [x] | 9 | Code-Review (Unteragent) | `review-code.md` — 4 Iterationen, letzter Befund (Sticky-Header-Ankerkompensation) behoben |
| [x] | 10 | Tests ausführen (Unteragent) | `test-results.md` — 326/326; Go-Live-E2E als `Nicht durchführbar` vor Merge dokumentiert |
| [x] | – | Iteration oder Abschluss entscheiden | Iterationen 1–4 abgeschlossen; alle Review-Befunde behoben |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | n/a — Go-Live-Folgeschritte in `test-results.md`/`docs/CI-CD.md` dokumentiert |
| [x] | 12 | Dokumentation erstellen (Unteragent) | `docs/CI-CD.md` (Pages-Abschnitt inkl. Aktivierung/Update/Troubleshooting) |
| [x] | 12b | README aktualisieren (Unteragent) | `README.md` (Badge/Link + `site/`-Strukturzeile) |
| [x] | 12c | Release Notes aktualisieren (Unteragent) | `docs/RELEASE_NOTES.md`, `changes.log` |
| [x] | – | Feature-Pfad-Bereinigung | Doppeltes UUID-Verzeichnis (mit Bindestrichen) aufgelöst; alle Artefakte im kanonischen Pfad |
| [ ] | – | Commit durchführen | – |
| [ ] | – | Push + PR gegen `staging` + Checks überwachen | – |
