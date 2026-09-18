# Aufgabenliste – Standort

Branch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-5-standort`
Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`

| Status | Schritt | Artefakt |
|---|---|---|
| [x] | Branch und Verzeichnis prüfen | Featureverzeichnis |
| [x] | Einstiegspunkt bestimmen | Neuer Schritt 5 |
| [x] | Anforderung übernehmen | requirement.md |
| [x] | Bestandsaufnahme | inventory.md |
| [x] | Planung | plan.md |
| [x] | Offene Punkte prüfen | Keine |
| [x] | Unabhängige Planprüfung | plan-check.md |
| [x] | Planungscommit | ef228c9 |
| [x] | Implementierung | Code |
| [x] | Planreview | review.md, vollständig umgesetzt |
| [x] | Usabilityreview | review-usability.md |
| [x] | Codereview | review-code.md |
| [x] | Tests einschließlich nativer UI/OS-Probe | test-results.md |
| [x] | Iteration/Abschluss entscheiden | Alle Prüfungen grün, unabhängige Abnahme erfüllt |
| [x] | Folgeaufgaben prüfen, bei Bedarf festhalten | continue-done.md |
| [x] | Dauerhafte Hilfe/iOS-Anleitung | docs/help |
| [x] | README aktualisieren | README.md |
| [x] | Release Notes aktualisieren | docs/RELEASE_NOTES.md |
| [x] | Feature-Artefakte nach vollständigem Abschluss archivieren/entfernen | Archivierungscommit vor Entfernung |
| [x] | Abschlusscommit | Produkt 5b96d81, Abschluss nach Archivierung |

## Fortsetzung 18.09.2026

Der Stand vom 17.09. war trotz erfolgreichem Build und 138 bestehenden Tests noch unvollständig: Nearby-Kandidaten waren nicht an die sichtbare Liste/Karte angeschlossen, echte Cancellation und neue Standorttests fehlten. Kein Implementierungsabschluss oder fachliche Abnahme. Die damaligen Implementierungsagenten brachen am Nutzungslimit ab; der lokale Zwischenstand wird jetzt vervollständigt. Planungscommit und unabhängige Planprüfung bleiben gültig. Core und neue Coretests sowie native Fixture-/UIA-Tests werden in getrennten Dateibereichen delegiert; Appintegration erfolgt lokal. Die vorangegangene automatische Ablehnung des Commitversuchs betraf das damalige Approval-Kontingent; es wurde kein Commit behauptet.
