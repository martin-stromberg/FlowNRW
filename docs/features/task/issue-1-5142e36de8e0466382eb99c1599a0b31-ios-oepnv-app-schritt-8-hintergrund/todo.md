# Aufgabenliste – Anforderungsbearbeitung

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch ermitteln | – |
| [x] | 2 | Verzeichnis vorbereiten | – |
| [x] | – | Einstieg: Schritt 3 | – |
| [x] | 3 | Anforderung übersetzen | requirement.md |
| [x] | 4 | Bestandsaufnahme | inventory.md |
| [x] | 5 | Planung | plan.md |
| [x] | 5a | Offene Punkte prüfen | plan.md |
| [x] | 5b | Planprüfung | plan-check.md |
| [x] | 5c | Planungscommit | – |
| [x] | 6 | Implementierung | Code |
| [x] | 7 | Planreview | review.md |
| [x] | 8 | Usabilityreview | review-usability.md |
| [x] | 9 | Codereview | review-code.md |
| [x] | 10 | Tests | test-results.md |
| [x] | – | Iteration entscheiden | – |
| [x] | 11 | Folgeaufgaben falls nötig | continue.md |
| [x] | 12 | Dokumentation | docs/help |
| [x] | 12b | README | README.md |
| [x] | 12c | Release Notes | docs/RELEASE_NOTES.md |
| [x] | – | Feature-Verzeichnis löschen | – |
| [x] | – | Commit durchführen | – |

25.09.2026: Bestandsaufnahme abgeschlossen; 210 Core- und 26 Release-Skripttests bestanden. Delegierte Agenten danach am Nutzungslimit ausgefallen; Planung und getrennte lokale Planprüfung gemäß Skillfallback abgeschlossen und committed. Keine fachlichen Rückfragen. Implementierung begonnen: Frischeregel, abbrechbare Monitorpfade und begrenzter Favoritenbatch/Lifecycle-Kern. Zwischenprüfung 221 Coretests bestanden; noch keine UI-/iOS-Anbindung oder finale Abnahme dieses Schritts.


26.09.2026: Fortsetzung: UI-/iOS-Anbindung implementiert; iOS Compile-Target (kein nativer Build) und UiTest-Build erfolgreich. 235 Coretests bestanden, einschließlich deterministischer Deadline, leerer und ungültiger Favoriten. Native Routing-, Monitor-, Karten-, Standort- und Favoritenregression bestanden. Standortprüfung wartete zuvor nicht auf Resize-Layout; begrenztes Layout-Warten ergänzt, Größenassertion beibehalten und kompletter Modus erneut bestanden. Eigener Lifecyclelauf zuvor bestanden; nach zwei Guards erneut auszuführen. Echte Intervallregression läuft. Separater Review-Agent am Nutzungslimit ausgefallen; lokale Prüfung gemäß Skillfallback, keine unabhängige Abnahme behauptet. Review, finale Prüfungen, Dokumentation und Commit weiterhin offen.

26.09.2026: Finale Tests und Dokumentation abgeschlossen: 236 Coretests/93,15 Prozent, Release/UiTest und iOS Compile-Target 0/0, Format/XML erfolgreich, 26 Release-Skripttests. Alle fünf nativen Regressionen, echter Intervalllauf und erweiterter Lifecyclelauf erfolgreich; Layout-/Navigationswartefehler mit erhaltenen Assertions korrigiert, Fehlversuche dauerhaft archiviert. Reviews lokal gemäß Ausweichregel; keine unabhängige Abnahme behauptet. Keine continue.md nötig. Produktcommit und Projektabnahme folgen; iOS-Geräteprüfung und Schritt9 bleiben getrennt.
