# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-30-0d24300145ad41dfb5d3500f06b5e040-nachricht-von-apple`

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/task/issue-30-0d24300145ad41dfb5d3500f06b5e040-nachricht-von-apple/` |
| [x] | – | Einstiegspunkt ermitteln | – |
| [x] | 3 | Anforderung übersetzen (Unteragent) | `requirement.md` |
| [x] | 4 | Bestandsaufnahme (Unteragent) | `inventory.md`, `inventory/` |
| [x] | 5 | Umsetzungsplanung (Unteragent) | `plan.md` |
| [x] | 5a | Offene Punkte prüfen und ggf. Planung wiederholen | `plan.md` (aktualisiert) |
| [x] | 5b | Plan gegen Anforderung und Testbedarf prüfen (Unteragent) | `plan-check.md` |
| [x] | 5c | Planungscommit | – |
| [x] | 6 | Implementierung (Unteragent) | Codeänderungen; inkl. Plan-Korrektur 2026-11-20: SDK-Band bleibt `10.0.1xx`, MAUI-Pin `10.0.110` (`manifest-10.0.400` existiert nicht, `10.0.120` existiert nicht); Plan-Erweiterung: UIScene-Migration, `PrivacyInfo.xcprivacy`, `CFBundleLocalizations`, `EnableIos`, Slash-Normalisierung |
| [x] | 7 | Plan-Review (Unteragent, bedingt) | `review.md` — Vollständig umgesetzt; Plan-Erweiterungs-Änderungen durch Code-Review abgedeckt |
| [x] | 8 | Usability-Review (Unteragent, bei UI-Änderungen) | `review-usability.md` — nicht anwendbar (keine UI-Änderungen; UIScene = Plattform-Lifecycle, kein UI) |
| [x] | 9 | Code-Review (Unteragent) | `review-code.md` — Keine Befunde (Rotationen .1–.4) |
| [x] | 10 | Tests ausführen (Unteragent) | `test-results.md` — 329/329 Tests + externe `store`-Abnahme dokumentiert |
| [x] | – | Iteration oder Abschluss entscheiden | konvergiert nach Plan-Korrektur + Re-Reviews |
| [x] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | entfällt — kein Schleifenabbruch; offene anwendergebundene Abnahmen in `test-results.md` dokumentiert |
| [x] | 12 | Dokumentation erstellen (Unteragent) | `changes.log`, `docs/help/verbindungssuche/installation.md`, `docs/help/monitorintervalle/` |
| [x] | 12b | README aktualisieren (Unteragent) | `README.md` |
| [x] | 12c | Release Notes aktualisieren (Unteragent) | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen | offen — anwendergebundene Abnahmen (TestFlight-Gerätetest, nächster CI-Lauf) stehen noch aus |
| [ ] | – | Commit durchführen | offen |
