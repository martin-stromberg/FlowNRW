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

Schritt1 fertig, vollständig ohne Konflikte in Basisbranch integriert; integrierten Schrittbranch sicher gelöscht. Schritt2 Verbindungssuche In Arbeit, Klärungsrunden0/Abnahmerunden0. 1 von8 fertig.

2026-09-15: Schritt2 fortgesetzt. Bestandsaufnahme abgeschlossen; kompakte Detailplanung läuft. Native Windows-UI-Automation unter PowerShell5.1 erfolgreich vorgeprüft (Vorlagenbutton per InvokePattern bedient, sichtbare Änderung bestätigt), Nachweis docs/help/verbindungssuche/verification/windows-uia-preflight.md. IIS-Seite erneut HTTP200 und identische Quell-/Zielprüfsumme. Noch keine fachliche UI-Abnahme oder neue App-Lieferung.

16.09.2026: Schritt2 Funktionsumfang implementiert, 118 Coretests und 97,01% Coverage, native Windows-Fixtures inklusive Fehler-/Race-/Navigationsflüssen grün. Echte native NRW- und bundesweite Verbindungssuche erfolgreich. Schmalansicht-Textumbruch behoben und erneut visuell/nativ geprüft. Unabhängiges Planreview vollständig, unabhängiges Usabilityreview ohne Befunde. Code-Review läuft; Dokumentation, Lifecycleabschluss, Projektabnahme und IIS-ZIP folgen. Kein Merge erfolgt.
16.09.2026: Nutzer streicht ausdrücklich lokale IIS-Präsentation. Kein Paket kopiert oder veröffentlicht; begonnener lokaler Publish endete erfolgreich, wird nicht weiterverfolgt. A18/K13 und IIS-Anteile aller Schritte entfallen, Funktionsumfang/Tests unverändert. Lifecycle2 abgeschlossen Commit7adb9bc, Berichte25d6f30. Projektabnahmeagent am Nutzungslimit ausgefallen; gezielte Plananpassungsprüfung und fachliche Abnahme werden gemäß Skillfallback lokal getrennt ausgeführt.
Schritt2 Projektabnahmerunde1 für Produkt7adb9bc vollständig erfüllt im vom Nutzer aktualisierten Umfang; lokaler Skillfallback nach dokumentiertem Ausfall des separaten Abnahmeagenten. Kein IIS-Kriterium verbleibt. Merge vorbereitet. Klärungsrunden0.
Schritt2 konfliktfrei in Basisbranch integriert; integrierten Branch sicher gelöscht. Schritt2 Fertig, Schritt3 In Arbeit. Schritt3 Klärungsrunden0/Abnahmerunden0. Keine IIS-Arbeiten mehr.

17.09.2026: Schritt3 Lifecycle abgeschlossen (726fe42), Reviews archiviert ab80f06. 126 Tests/96,93% Coverage, native Monitor-/Routing-/Live-Prüfungen und schmale Ansicht erfolgreich. Projektabnahme Runde1 erfüllt; separater Abnahmeagent usagebedingt ausgefallen, getrennte lokale Abnahme gemäß Skillfallback. Keine fachlichen Klärungsrunden. Merge freigegeben.

17.09.2026: Schritt3 konfliktfrei integriert; Schrittbranch sicher gelöscht. Schritt4 In Arbeit, Klärungsrunden0/Abnahmerunden0. 3 von8 fertig.

17.09.2026: Schritt4 Lifecycle abgeschlossen (65179fa), Reviews archiviert d6ca720. 138 Tests/94,09% Coverage, native Karten-/Routing-/Monitorflüsse, echte NRW-Kartenprobe und schmale Ansicht erfolgreich. Projektabnahme Runde1 erfüllt; getrennte lokale Abnahme gemäß dokumentiertem Agentenlimit-Fallback. Keine fachlichen Klärungsrunden. Merge freigegeben.

17.09.2026: Schritt4 konfliktfrei integriert, Schrittbranch sicher gelöscht. Schritt5 Standort In Arbeit, Klärungsrunden0/Abnahmerunden0. 4 von8 fertig.

18.09.2026: Schritt5 implementiert; 165 Coretests/94,42% Abdeckung, Release und UiTest0/0, native Standort-/Routing-/Monitor-/Karten-Fixtures erfolgreich. Echte Windows-OS-Probe vor Start von automatischer Freigabeprüfung wegen sensibler Positionsübermittlung abgelehnt. Schritt5 Blockiert bis expliziter Freigabe; kein Merge/keine Projektabnahme. Unabhängige Adaptervorprüfung korrigiert, Gesamtprüfungen lokal nach erneutem Agentenlimit. Details blocked.md und Feature-continue.md.

18.09.2026: Nutzer genehmigt tatsächlichen Standortabruf und Datenübermittlung ausdrücklich. Release-Live-Test erfolgreich: Windows liefert Position, reale nahe Haltestelle öffnet Monitor. Keine privaten Positionsdaten gespeichert. Blockade erledigt. Unabhängige Projektabnahme Runde1 für 5b96d81 vollständig erfüllt; keine Abweichungen, Klärungsrunden0. Archivierung und Merge freigegeben.

18.09.2026: Schritt5 mit Abschluss ff060d5 konfliktfrei in den Projektbasisbranch integriert. Standortblockade entfernt, 5 von8 fertig. Schritt6 Favoriten/Startseitenmonitore In Arbeit, Klärungsrunden0/Abnahmerunden0.

19.09.2026: Schritt6 Produktcommit 1edca4b, unabhängige Projektabnahme Runde1 vollständig erfüllt. 173 Coretests/92,57 % Coverage, native Favoriten mit vier Prozessstarts sowie alle Routing-/Monitor-/Karten-/Standortregressionen bestanden. Release/UiTest ohne Warnungen und Fehler, Format/XML bestanden. Keine fachlichen Klärungsrunden. Reviews über Git-Historie gesichert, dauerhafte Nachweise unter docs/help/favoriten/verification; Lifecycleabschluss und Merge freigegeben.

19.09.2026: Schritt6 mit Abschluss71690db konfliktfrei integriert, integrierten Schrittbranch sicher gelöscht. 6 von8 fertig. Schritt7 Monitorintervalle In Arbeit, Klärungsrunden0/Abnahmerunden0.
## Designklarstellung – 24.09.2026

6 von9 Schritten fachlich fertig; Schritt7 bleibt In Arbeit, Schritte8 und9 Offen. HTML-/Markdown-Draft war in Entscheidung2 bereits berücksichtigt, jedoch ohne konkrete visuelle Gesamtabnahme. Nutzerbestätigung präzisiert die finale Erscheinung. docs/design/acceptance.md legt Tokens, Konfliktentscheidungen und eine tatsächliche Screenshotmatrix fest; Schritt9 ist verpflichtendes Abschlusskriterium nach8. Keine bisherigen fachlichen Abnahmen aufgehoben.

- [x] Tatsächliche Designreferenzen und Kernumfang abgeglichen
- [x] Visuelle Abnahmekriterien und Schritt9 geplant
- [x] Aktualisierten Projektplan einschließlich Designumfang unabhängig prüfen
- [ ] Schritt9 implementieren und unabhängig visuell/fachlich abnehmen

Gesamtabschluss/Aufräumen erst nach erfülltem Schritt9. Die unabhängige Prüfung des erweiterten Plans ist vollständig; siehe project-plan-check.md und archivierten Vorgänger project-plan-check.4.md.

25.09.2026: Schritt7 mit Produktfix 3c062ed und unabhängiger Projektabnahme Runde1 vollständig erfüllt. 210 Coretests/92,77 % Abdeckung, Release/UiTest ohne Warnungen und Fehler; native Intervalle, Favoriten, Routing und Monitore bestanden. Fehler beim Entfernen ladender Favoriten durch verwaiste WinUI-Commandbindungen behoben und nativ nachgeprüft. Format/XML/Diff bestanden. Dauerhafte Nachweise unter docs/help/monitorintervalle/verification. Keine fachlichen Klärungsrunden; Abschluss und Merge freigegeben.

25.09.2026: Schritt7 mit Abschluss 34b0b76 konfliktfrei integriert. 7 von9 Schritten fertig. Schritt8 Hintergrundaktualisierung/Wiederaufnahme In Arbeit, Klärungsrunden0/Abnahmerunden0. Visuelle Gesamtabnahme bleibt Schritt9.
