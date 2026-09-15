# Projekttracking – iOS ÖPNV App

Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`
Issue: #1 (keine Veröffentlichung beauftragt)

- [x] Branchprüfung
- [x] Verzeichnis/Tracking
- [x] Einstiegspunkt: neues Projekt, issue.md
- [x] Anforderung
- [x] Bestandsaufnahme
- [x] Projektplan
- [x] Offene Punkte
- [x] Planprüfung
- [x] Planungscommit
- [x] Entwicklungsschritte-Tracking
- [ ] Entwicklungsschleife
- [ ] Aufräumen
- [ ] Abschlusscommit

## Ausgangszustand

Vorhandene fremde Änderungen: .gitignore modifiziert; design-draft.zip unversioniert, als Eingabe autorisiert. Nicht ungefragt committen.
Lifecycle-Einstieg und lifecycle.md gelesen. Keine vorhandenen Projektartefakte. rg fehlt; PowerShell-Dateisuche als Ersatz.



Nutzerergänzung: Windows-Release und Tests in GitHub Actions erhalten; iOS-CI und neues automatisiertes Deployment nicht im Umfang. Anforderung/Bestandsaufnahme angepasst. Testumfang Windows-UI versus native iOS-Abnahme angefragt. Kein zusätzliches Plan-/Todo-Werkzeug verfügbar.


Planprüfung 1 abgeschlossen: fachliche Abdeckung vollständig; Status Projektplan lückenhaft ausschließlich wegen offener nativer Abnahmestrategie. blocked.md/status.html angelegt. Kein Planungscommit, keine Implementierung. Sicherungscommit hält die offene Planung fest.

2026-09-07: Nutzer bestätigt Windows-UI-Prüfung aller Abläufe soweit möglich. Native iOS-Abnahme liegt vorerst beim Nutzer. Planungsblockade damit gelöst; keine erneute Zustimmung nötig. Plan wird aktualisiert und erneut geprüft.


2026-09-08: Fortsetzung nach Nutzungslimit. Windows-Basisbuild Release mit TreatWarningsAsErrors=true erfolgreich, 0 Warnungen/Fehler. Aktualisierte Anforderung und Plan vorhanden; unabhängige erneute Prüfung läuft.



Schritt 1 In Arbeit; Lifecycle-Klärungsrunden 0, Abnahmerunden 0. Planungscommit b2454f5.

2026-09-09: Lifecycle Schritt1 erfolgreich, Abschlusscommit 0d7eb4b, Reviewhistorie ecc25c8. 98 .NET-/26Node-Tests,98%Corecoverage,Windowsbuild0/0. Plan-/Code-/Dokureviews grün. Projekt-Abnahmerunde1 gestartet; Lifecycle-Klärungsrunden0 (Usage-Unterbrechungen keine fachlichen Klärungsrunden).

Projekt-Abnahmerunde1: Abweichungen gefunden (AK3 zusätzliche bundesweite Fahrten bei regionaler Teilantwort verworfen). Gezielte Lifecycle-Korrekturanforderung auf demselben Branch gestartet; Nachbesserung1 von maximal2, keine Mergefreigabe. Ursprüngliche Lifecycle-Artefakte nicht rekonstruiert.

2026-09-15: Nutzer fordert kleinere Lieferpakete und IIS-Zwischenstände. Neuschnitt8 Schritte, Schritt1 unverändert; unabhängige neue Planprüfung vor Schritten2–8 erforderlich. IIS-Begutachtungsseite unter http://localhost/%C3%96PNV/ installiert und HTTP/Hash-geprüft, docs/review/README.md. Externe URL optional angefragt. Keine native UI-Lieferung vor Schritt2.

2026-09-15: Acht-Schritte-Neuschnitt unabhängig vollständig geprüft; Tracking synchronisiert, bestehende Branchzuordnungen1–4 bewahrt. Schritt1 Projektabnahmerunde2 erfüllt; Korrekturcommit1eba9c3,106Tests98,61%CoreWindows0/0. Merge vorbereitet. Agents durch Usage-Limit zeitweise ausgefallen; lokale Korrektur gemäß Skillfallback, unabhängige Abnahme anschließend wieder möglich.
