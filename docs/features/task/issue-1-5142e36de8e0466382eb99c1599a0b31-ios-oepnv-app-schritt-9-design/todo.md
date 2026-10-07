# Aufgabenliste – Visuelle Integration

| Status | Schritt | Beschreibung | Artefakt |
|---|---|---|---|
| [x] | 1 | Branch prüfen | Schritt9 |
| [x] | 2 | Verzeichnis/Tracking | dieses Dokument |
| [x] | 3 | Anforderung übernehmen | requirement.md |
| [x] | 4 | Bestandsaufnahme | inventory.md |
| [x] | 5 | Planung | plan.md |
| [x] | 5a | Offene Punkte | plan.md |
| [x] | 5b | Planprüfung | plan-check.md |
| [x] | 5c | Planungscommit | Git: 6780866 |
| [x] | 6 | Implementierung in kleinen prüfbaren Abschnitten | Code |
| [x] | 7 | Planreview | review.md |
| [x] | 8 | Separate visuelle/Usability-Prüfung | review-usability.md, visual-review-oct3.md, visual-recheck-root-oct3.md |
| [x] | 9 | Codereview | review-code.md |
| [x] | 10 | Native Tests, Screenshotmatrix, Core/Build | test-results.md |
| [x] | 10a | Tester-Korrekturen K1–K7 umsetzen und erneut prüfen | correction-tasks.md |
| [x] | 10b | Aktualisierten Stitch-Entwurf erfassen, Nearby-Drilldown korrigieren und Kernseiten visuell vereinheitlichen | requirement-draft-refresh.md, draft-refresh-plan.md |
| [x] | 10c | Code-Review des aktualisierten Gesamtdiffs | review-draft-refresh.md |
| [x] | 10d | Nativen Windows-Designmatrixlauf für die aktualisierte Oberfläche in Hell/Dunkel durchführen, einschließlich Nearby-Drilldown-Screenshot | test-results-visual-final.md (Commit 97b940f, vier Matrizen PASS) |
| [ ] | 10e | iOS-Geräteabnahme für Safe Areas, Systemthema, Dynamic Type und VoiceOver durchführen | test-results-draft-refresh.md |
| [x] | 10f | Favoriten-Abfahrtcache unter Windows nativ starten: Cacheanzeige vor verzögerter Anbieterantwort und anschließende Ersetzung prüfen | artifacts/step9-visual-final/cache-refresh (Commit 97b940f) |
| [ ] | 12 | Dokumentation/README/Release Notes | docs/help/design |
| [ ] | – | Abschlusscommit und Projektabnahme | Git |

26.09.2026: Ausfälle beider Schritt8-Prüfer am Nutzungslimit bekannt. Anforderung und Bestandsaufnahme als getrennte lokale Phasen gemäß Skillfallback; kein weiterer identischer erfolgloser Delegationsversuch für jede Datei. Bestehenden geprüften Datenkern wiederverwenden. Kein Gesamtabschluss vor visueller Abnahme.
26.09.2026: Abschnitt1 begonnen: semantische Flächen-/Textrollen, drei Shell-Kernbereiche, DepartureCardView und erste Such-/Ergebniskarten umgesetzt. XAML XML wohlgeformt. Native UiTest-/Release-Build und Commit sind wegen des erreichten automatischen Freigabelimits noch offen; kein Prüfergebnis daraus behaupten.
26.09.2026: Timeline-Karten aus echten JourneyLegs ergänzt. Der Windows-MAUI-Build bleibt wegen des Freigabelimits nicht ausführbar; keine Build-/UI-Abnahme behauptet.

01.10.2026: Nach Serverneustart Branch, Arbeitsstand und Prozesse geprüft. Planungscommit 6780866 vorhanden; kein App-/Buildprozess aktiv. Windows-UiTest-Build inzwischen mit 0 Warnungen und 0 Fehlern erfolgreich. Implementierung wird wieder durch separaten Lifecycle-Unteragenten fortgesetzt. Native Regression und separate visuelle Abnahme weiterhin offen.

03.10.2026: Separater Implementierer korrigierte die fünf Codebefunde sowie Such-/Listenhierarchie. Separater visueller Prüfer lieferte `visual-review-oct3.md`; neue Bilder werden anschließend vom unbeteiligten Projektkoordinator nachgeprüft. Implementierungs-/Testagent und abschließender Codeprüfer fielen am Nutzungslimit aus (Meldung 12:37 PM, keine Zeitzone genannt). Fortsetzung nach ausdrücklichem Nutzerauftrag als lokaler Skillfallback, keine unabhängige technische Freigabe behauptet.

03.10.2026: Ungültiger ScreenCopy-Versuch und versehentlich nicht ausgeführter PowerShell-Scriptblock erkannt; ausschließlich echte PrintWindow-Erfassung der eigenen HWND verwendet und mit view_image geprüft. Native Schaltergröße zunächst nur52×40 trotz Mindesthöhe; tatsächliche Vergrößerung ergibt62×48, streng geprüft. UiTestbuild0/0; korrigierter kompletter dunkler150%-Matrixlauf erfolgreich einschließlich sichtbarer Fußweg-/Bus-/Umstiegsfolge und Departure.Line-Fallback. Breite Matrix und finale Regression noch in Arbeit.

03.10.2026: Coretests 253/253 bestanden; Coverage 93,21 % Zeilen und 80,23 % Zweige. Routing-, Monitor-, Standort- und Designregressionen sowie die revidierte Bildnachprüfung bestanden. `maps-final.log` blieb leer und wird nicht als Nachweis verwendet; der vollständige Kartenlauf ist unter `maps-oct2-accessible.log` belegt. Der Release-Solution-Build scheiterte in der Sandbox am Zugriff auf das Windows-SDK-Verzeichnis; ein erneuter privilegierter Build war wegen des automatischen Nutzungslimits nicht möglich. Schritt10 bleibt offen, bis diese Grenzen und der erneute Lifecycle-Regressionstest geklärt sind.

03.10.2026: Sieben neue Tester-Korrekturen K1–K7 zu Startseite, Nearby-Anzeige, Favoritenzähler, Suchvorschlägen, Trefferübernahme, kompakter Suche und Routingzeit in `correction-tasks.md` erfasst. Die bisherige Schritt9-Abnahme bleibt bis zur Umsetzung und Prüfung dieser Korrekturen offen.

03.10.2026: Der aktualisierte Stitch-Entwurf ist als Lifecycle-Anforderung, Bestandsaufnahme und Kleinplan dokumentiert. Der Home-Nearby-Drilldown wurde identitätsvalidiert repariert, mit Core-Test und Fixture-Pfad abgesichert; Startseite, Monitor, Suche, Detail und Karte wurden ausschließlich innerhalb des vorhandenen Funktionsumfangs visuell überarbeitet. Core-Tests: 256/256; UiTest-Build: 0 Warnungen/0 Fehler; PowerShell-Syntaxprüfung: bestanden. Der native Windows-Designmatrixlauf für diesen Refresh in Hell/Dunkel und die iOS-Geräteabnahme sind weiterhin ausdrücklich offen.

04.10.2026: Rückmeldung zu überflüssigen Informationstexten umgesetzt: Favoritenzähler, Übersichts-Panel und erfolgreiche Statuswiederholungen auf der Startseite entfernt. Lade-, Leer-, Fehler- und Cachehinweise bleiben kontextgebunden sichtbar. Die Einstellungsseite wurde auf Auswahl, Speichern, effektiven Wert und relevante Rückmeldungen verdichtet; die Karte zeigt keine technische Punktanzahl. Kernprüfungen: 267/267 Core-Tests, UiTest-Build Windows/iOS 0/0, PowerShell-Syntax und Diffprüfung bestanden. Die native Designmatrix bleibt in einer interaktiven Sitzung offen.

05.10.2026: Native Windows-Nachweiskette abgeschlossen auf Commit 97b940f (Build 175DBAB0…095D, sauberer Arbeitsbaum): alle acht nativen Regressionen (journey-*, departure-cache, refresh, lifecycle) und alle vier finalen Designmatrizen (430×900 hell, 430×900 dunkel 150 %, 1024×768 hell/dunkel) plus Cache-während-Refresh-Bildpaar bestanden. Produktfix: Entfernen-Button der verbleibenden Favoritenkarte blieb nach einer Löschung deaktiviert (IsSaving-Race) — in HomePage.cs behoben. Verbleibend: iOS-Geräteabnahme (nutzerseitig), Dokumentation/Release Notes, Projektabschluss.

05.10.2026 (Abend): iOS-Geräteabnahme durch Nutzer teilweise bestanden — Verbindungssuche, Favoritenverwendung, Tausch und Detailaufruf OK; drei Produktbefunde behoben: (1) Abfahrtsabruf lehnte jede degradierte Providerantwort als Fehler ab → nutzbare Antworten zeigen jetzt Daten mit 'unvollständig/veraltet'-Hinweis, komplette Boards bleiben bevorzugt, Persistenz auf vollständige Antworten beschränkt; (2) UTC+02:00-Offsets in Verbindungsdetails → HH:mm mit Vortag/Folgetag-Markierung; (3) Umstiege lagen als Endblock unter der Timeline → jetzt positionsgetreu zwischen den Abschnitten. Core 280/280, UiTest-Build 0/0, journey-default-Regression bestanden. iOS-Wiederholungsabnahme offen.

05.10.2026 (Nacht): Zweiter Gerätedurchgang eingearbeitet — Abfahrten laden jetzt mit Zeiten, aber sporadische Fehleranzeige bleibt als akzeptabel bewertet (Ursachenanalyse via neuem Diagnoseprotokoll). Zwei Cache-Lücken behoben: Restore-Pfad verwarf persistierte Boards älter als TTL (Neustart zeigte nur Linien) und der Session-Cache nahm nur vollständige Antworten auf (erneutes Öffnen zeigte leere Liste). Detail-Fallback zeigt jetzt den Sitzungsstatus statt 'Keine Verbindung ausgewählt'. Neue Funktion: Diagnoseprotokoll mit Aktivierungsschalter und 'Protokoll senden' (E-Mail mit Log-Anhang an mstromberg84+flow@gmail.com). Core 280/280, UiTest-Build 0/0, journey- und departure-cache-Regression grün.

06.10.2026: Nacht-Runner auf Endcommit 782b5a2 vollständig grün — 8/8 Regressionen und 4/4 Matrizen, Manifeste auf sauberem Arbeitsbaum, einheitlicher Build 3FFA3809. iOS-Geräteabnahme vom Nutzer abgenommen (Restpunkt: sporadische Abruffehler, Diagnose via Diagnoseprotokoll). Release Notes aktualisiert.

07.10.2026: Erstes Geräteprotokoll ausgewertet — efa-Abfahrtsabrufe ~700 ms erfolgreich, db-rest /locations pro Karte ~20 s Timeout. Fremd-ID-Regel bleibt erhalten (db.rest darf keine fremden IDs). Retention-Schwelle korrigiert: degradierte frische Antwort ersetzt ein Board, das selbst nicht mehr vollständig ist; komplette Boards bleiben geschützt. Core 280/280, UiTest-Build 0/0.
