# Native Windows-Liveprüfung der Verbindungssuche

Am 16.09.2026 zwischen 08:39 und 08:44 Uhr Europe/Berlin wurde die reguläre, mit echten Diensten komponierte Windows-App bedient. Kein Fixture-Build und keine direkte ViewModel-Ausführung.

## Getesteter Stand und Verfahren

- Arbeitsstand auf Schritt-2-Branch nach Planungscommit `857d9bd`; Produktänderungen noch nicht committed.
- Programm: `FlowNRW/bin/Release/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe`.
- `FlowNRW.dll`: SHA256 `E30EE9F6EDF68BBEBBD96BF9AFAD428CE4198BFB85E4819849B9B4299E119235`, Buildzeit 15.09.2026 20:41:35 UTC.
- Native Windows PowerShell 5.1, `UIAutomationClient` und `UIAutomationTypes`. Fenster über `ProcessIdProperty`, Controls über `AutomationIdProperty`; Eingabe über `ValuePattern.SetValue`, Buttons über `InvokePattern.Invoke`, sichtbare Texte und Enabled-Zustände aus dem nativen UIA-Baum.
- Die App wurde mit freigegebenem Netzwerkzugriff gestartet und im selben freigegebenen Kontext bedient (PID 28908). Ein erster sandboxgebundener Lauf (PID 30956) zeigte Anbieterfehler; dieser allein hätte keinen Ausfall der öffentlichen Dienste bewiesen. Beide eigens gestarteten Fenster wurden nach der Prüfung geschlossen.

## Tatsächlich bediente Abläufe

1. Suchseite gestartet: Start-/Zielfelder und Koordinatenumschalter vorhanden, kein Counter und kein Standortdialog. `SearchJourneys` war ohne gewählte Endpunkte deaktiviert.
2. `OriginText` mit `Gelsenkirchen Hbf`, `DestinationText` mit `Essen Hbf` befüllt. Beide Suchbuttons ausgelöst; beide Felder zeigten unabhängig `Treffer werden geladen …`.
3. Reale Treffer erschienen. `OriginMatch0` war Gelsenkirchen Hbf (`de:05513:5613`), `DestinationMatch0` Essen Hauptbahnhof (`de:05113:9289`). Die Treffer wurden nativ ausgewählt; beide Auswahltexte zeigten die übernommenen Datensätze. Danach war `SearchJourneys` aktiviert.
4. Verbindungssuche ausgelöst und sichtbaren Ladezustand geprüft. Ergebnis: `3 Verbindungen gefunden.` Quelle EFA, Datenstand 08:41 UTC+02:00, Anbieterwarnung verständlich angezeigt.

| NRW-Ergebnis | Sollabfahrt | Sollankunft | Betreiber |
|---|---|---|---|
| S2 | 08:54 | 09:07 | DB Regio AG NRW |
| RE42 | 09:04 | 09:14 | DB Regio AG NRW |
| RE2 | 09:11 | 09:21 | DB Regio AG NRW |

Alle Zeiten wurden mit Datum 16.09.2026 und UTC+02:00 angezeigt. Die gelieferten Istzeiten entsprachen in dieser Antwort den Sollzeiten; das ist keine allgemeine Pünktlichkeitsaussage.

5. `Journey0` geöffnet. Native Detailabschnitte enthielten S2, Betreiber, Abfahrt Gelsenkirchen Hbf und Ankunft Essen Hauptbahnhof, Soll-/Istzeiten, Bahnsteige 4 bzw. 21, Echtzeitquelle EFA und Datenstand. Unbekannter Ausfallstatus wurde ausdrücklich als unbekannt dargestellt.
6. `NavigationViewBackButton` zweimal bedient: zuerst Ergebnisliste mit drei Fahrten, danach Suche mit erhaltener Gelsenkirchener Auswahl. Änderung von `OriginText` zu `Berlin Hbf` setzte die Auswahl auf `Noch kein Endpunkt ausgewählt.` und deaktivierte die Verbindungssuche.
7. Ziel auf `Hamburg Hbf` geändert, beide Trefferabfragen ausgelöst. Berlin Hauptbahnhof Gleis 1–8 (`OriginMatch0`, `de:11000:900003200`) und der tatsächliche Hamburger Hauptbahnhof (`DestinationMatch6`, `de:02000:10950`) aus den mehrdeutigen Treffern ausgewählt; nicht den zuerst gelisteten Busstop am Hamburger Bahnhof verwendet.
8. Bundesweite Verbindungssuche nativ ausgelöst. Ergebnis: drei geordnete Verbindungen, Quelle EFA, ausdrücklich als Ersatzquelle/Fallback gekennzeichnet, Datenstand 08:43 UTC+02:00, verständliche Anbieterwarnung.

| Bundesweites Ergebnis | Sollabfahrt | Sollankunft | Betreiber |
|---|---|---|---|
| ICE 604 | 09:37 | 11:24 | DB Fernverkehr AG |
| ICE 804 | 10:09 | 12:10 | DB Fernverkehr AG |
| DRF 1234 | 10:19 | 12:21 | FlixTrain |

## Aussagegrenzen

Beide Live-Suchabläufe waren in diesem Lauf erfolgreich. Der separate Service-Lauf vom Vorabend hatte für Berlin–Hamburg keine Verbindung geliefert; die heutige Prüfung ersetzt diesen historischen Befund nicht. Die Nutzung des EFA-Fallbacks bestätigt keine dauerhafte Erreichbarkeit des nationalen Gateways oder flächendeckende Versorgung. Diese Prüfung belegt die tatsächlich bedienten Texteingabe-, Auswahl-, Routing-, Detail- und Rücknavigationsabläufe; deterministische Koordinaten-, Fehler-, Cache- und Race-Szenarien werden zusätzlich im isolierten nativen Fixture-Lauf geprüft. Native iOS-Abnahme verbleibt beim Nutzer.
