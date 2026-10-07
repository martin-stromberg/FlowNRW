# Offene Aufgaben

Aktualisiert am: 05.10.2026 (Nacht)

Zweiter iOS-Gerätedurchgang durch den Nutzer: Abfahrten laden jetzt (degradierte Antworten werden angezeigt statt als Fehler), aber der Abruf meldet zwischendurch weiterhin Fehlschläge — vom Nutzer vorläufig als akzeptabel bewertet, Ursachenanalyse über das neue Diagnoseprotokoll. Weitere Befunde behoben: persistierte Cache-Boards werden nach Neustart wiederhergestellt (vorher nur Linien), der Session-Cache nimmt degradierte fehlerfreie Antworten auf (erneut geöffnete Station zeigt vorherige Abfahrten), und die Detailansicht zeigt bei gelöschter Auswahl den konkreten Sitzungsstatus.

Neue Funktion implementiert: `AppLog` Diagnoseprotokoll — in den Einstellungen aktivierbar, sendbar via Mail an mstromberg84+flow@gmail.com. Bitte beim nächsten Gerätetest aktivieren und bei Abfahrtsfehlern das Protokoll senden — es enthält HTTP-Status/Dauer je Anfrage und Monitorergebnis-Markierungen (error/fallback/stale/warnings).

- [ ] iOS-Geräteabnahme auf dem neuen Build wiederholen: insbesondere Cache-Checkpunkte (Abfahrten sichtbar vor Refresh-Abschluss, erneutes Öffnen derselben Station) und 'Protokoll senden' bei Fehlern.
- [ ] Native Refresh-/Lifecycle-/Journey-Regressionen und eine Designmatrix auf dem neuen Build nachlaufen lassen (Nacht-Runner `artifacts/night-runner.ps1`).
- [ ] Dokumentation/README/Release Notes finalisieren (Schritt 12).
- [ ] Danach Projektabnahme, Abschlusscommit und Merge vorbereiten; Schritt 9 bleibt „In Arbeit".
