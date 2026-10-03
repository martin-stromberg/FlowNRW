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
