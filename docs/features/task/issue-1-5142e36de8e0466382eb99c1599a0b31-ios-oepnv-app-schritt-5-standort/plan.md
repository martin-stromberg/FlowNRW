# Umsetzungsplan: Aktueller Standort und nahe Haltestellen
## Übersicht
Explizite Standortaktionen ergänzen die vorhandenen Start-/Zielkarten und die Haltestellensuche. Der bestehende Datenkern liefert die Umgebung; dieselben vollständigen Haltestellen öffnen aus Liste und Karte den Monitor. Manuelle Nutzung bleibt bei jedem Standortfehler möglich.
## Designentscheidungen
| Bereich | Ansatz | Begründung |
|---|---|---|
| OS-Grenze | Kleines Core-Interface `ICurrentLocationService` mit Ergebnis `LocationResult`; MAUI-Adapter im App-Projekt | Core testbar ohne MAUI, keine weitere Standortarchitektur. |
| Abfrage | Einmalige aktuelle Abfrage nach Buttondruck, Medium-Genauigkeit und 15 Sekunden Positions-Timeout | Ausreichender Startwert für Umgebung; kein Tracking und kein versteckter Zugriff. Berechtigungsdialog ist von Sensorwartezeit zu unterscheiden. |
| Zustände | Erfolg, abgelehnt/eingeschränkt, deaktiviert, nicht unterstützt, Timeout, keine Position, Fehler; Cancellation separat | Verständliche Wiederholung/manuelle Alternative, keine fremden Exceptiontexte im UI/Log. |
| Umgebung | Aktive Ergebnismenge im vorhandenen `StopMonitorViewModel`; eigene Nearby-Serviceinstanz | Vermeidet zweite Monitor-/Kartenidentität und gegenseitiges Abbrechen unabhängiger Suchfelder. |
| Datenschutz | Nur flüchtiger UI-Zustand und vorhandene begrenzte technische Caches | Keine neue Speicherung, Logs oder Bewegungshistorie. |
## Programmabläufe
### Standort als Start oder Ziel
1. Nutzer drückt „Aktuellen Standort verwenden“ in der betreffenden vorhandenen Endpunktkarte. Bisherige abgeschlossene Auswahl bleibt bei Fehlschlag erhalten; Status zeigt laufende Ermittlung.
2. Der Adapter prüft bei jeder expliziten Anfrage die aktuelle Berechtigung und fordert, wenn zulässig, `LocationWhenInUse` im UI-Kontext an. Ablehnung oder Entzug wird verständlich gemeldet, erneute OS-Abfrage nicht durch einen eigenen Endlosdialog ersetzt. Hinweis auf Systemeinstellungen genügt; keine automatische Änderung von OS-Einstellungen.
3. Bei Zustimmung ruft er `GetLocationAsync` mit Timeout und Cancellation auf. `null`, deaktivierter Dienst und fehlende Unterstützung sind eigene Zustände. Koordinate, Zeitpunkt und verfügbare Genauigkeit werden validiert; reduzierte Genauigkeit darf verwendet werden, wird als solche kenntlich, falls vom OS geliefert. Keine Garantie eines GPS-Sensors behaupten.
4. Nur die noch aktuelle Anfrage übernimmt einen vollständigen `Address` mit Name „Aktueller Standort“ und Koordinate über einen expliziten Endpunkt-Übernahmeweg. `Changed` invalidiert alte Verbindungsergebnisse wie bisher. Kein Reverse-Geocoding nötig.
5. Nutzer wählt den anderen Endpunkt und sucht normal Verbindungen. Textänderung, Koordinatenmoduswechsel, andere Auswahl, erneute Aktion und Verlassen der Seite invalidieren laufende Standortantworten. Start und Ziel sind unabhängig. Fehler überschreiben keine neuere Auswahl.
### Nahe Haltestellen
1. Nutzer drückt „Haltestellen in meiner Nähe“ auf `StopSearchPage`; Position wird wie oben explizit ermittelt. Währenddessen bleiben manuelle Eingabe und Navigation möglich.
2. Bei gültiger aktueller Position ruft der unabhängige Such-Scope `NearbyAsync` auf. Revision/Cancellation umfasst die gesamte Kette Position → Providerantwort.
3. Vollständige identifizierbare Stops werden kontrolliert als aktive Kandidaten übernommen; Quelle, Alter, Warnungen und optional gelieferte Entfernung bleiben erhalten. Verständliche Leer-/Fehlerzustände; ältere Ergebnisse werden nicht als neue Umgebung ausgegeben. Bei Fehlern erhaltene Daten sind ausdrücklich als vorherige Ergebnisse gekennzeichnet.
4. Bestehende Klartextbuttons öffnen den passenden Monitor. „Haltestellen auf Karte zeigen“ übergibt dieselbe aktive Kandidatenmenge und Metadaten an `MapViewModel`; Kartenmarker und zugängliche Kartenliste öffnen denselben Monitor. Fehlende Position verhindert nur Marker, nicht Listenwahl.
5. Manuelle Suche oder Eingabe invalidiert laufende Umgebungsermittlung; neue Umgebung invalidiert alte Kartenwahl über vorhandene Identitätsprüfung. Rücknavigation erhält abgeschlossene aktuelle Auswahl, startet aber keinen Standortzugriff.
## Neue Klassen
| Klasse | Typ | Zweck |
|---|---|---|
| `ICurrentLocationService` | Core-Interface | Einzelabfrage mit Cancellation. |
| `LocationResult` / `LocationStatus` | Core-Ergebnis / Enum | Koordinate, optional Genauigkeit/Zeitpunkt und fachlicher Fehlerstatus. |
| `MauiCurrentLocationService` | App-Adapter | MAUI-Berechtigungen und aktuelle Geolocation; keine sensiblen Logs. |
| `LocationTests` | Tests | Kontrollierte verspätete Antworten und Zustandsübergänge. |
Hilfslogik für Wiederholungen/Revisionen in bestehende ViewModels einfügen; keine allgemeinen Eventbusse, Repositories oder Hintergrunddienste.
## Änderungen an bestehenden Klassen
- `EndpointViewModel`: Standortcommand, lesbarer Standortstatus und Busy-Zustand; optionaler DI-Adapter; kontrollierte Koordinatenübernahme. `Invalidate`, `CancelPending` und bestehende Auswahl invalidieren auch Standortanfragen. Handübernahme darf nicht durch eigene `Changed`-Behandlung wieder gelöscht werden.
- `StopMonitorViewModel`: Nearby-Command/-Status, Ergebnis und flüchtige aktive Suchart; Projektion vollständiger Nearby-Stops als `Address`. `Stops`, Suchmetadaten und `OpenAsync` verwenden nur aktuelle gültige Kandidaten. Auf Änderungen manueller Eingabe reagieren; erneute und verlassene Umgebung abbrechen. Eigene Lookup-Metadaten nicht für GPS-Resultate ausgeben.
- `MapViewModel`: `ShowStops` liest aktive Metadaten/Kandidaten des Monitors. Session-/Kandidatenvalidierung erhalten. Nur bei Bedarf vorhandene Kartenorientierung um gültigen Umgebungsmittelpunkt ergänzen; keine erfundenen Stationspunkte.
- `SearchPage`: vorhandene Endpunktkarten um beschriftete Standortbuttons/Status mit stabilen AutomationIds ergänzen; Texte skalierbar, WordWrap und Touch-Ziele wie Bestand. Manuelle Controls bleiben erreichbar.
- `StopSearchPage`: Nearby-Button, Status/Metadaten und Entfernung im vorhandenen Ergebnislistenmuster; OnDisappearing invalidiert ausstehende Standort/Nearby-Arbeit, nicht fertige Ergebnisse.
- `MauiProgram`: echten Adapter registrieren und in Endpunkte/Monitor injizieren; Testadapter ausschließlich `UI_TEST_FIXTURES`.
- `Platforms/iOS/Info.plist`: deutscher Nutzungsgrund für Start/Ziel und nahe Haltestellen. Keine Always-/Hintergrundberechtigung. Windows-Packaging und GitHub Actions unverändert.
- `UiTestFixtureServices` und native Testskripte: steuerbare Standortzustände ohne echte Positionsdaten; Testauswahl nur im UiTest-Build, kein Release-Testmenü.
## Datenbankmigrationen
Keine.
## Validierungsregeln
| Objekt | Regel | Fehlerfall |
|---|---|---|
| Position | Endliche WGS84-Werte, gültige Breite/Länge; vorhandener Zeitstempel nicht als Zukunft/alte LastKnown-Antwort beschönigen | Keine nutzbare aktuelle Position melden. |
| Genauigkeit | Nur vorhandene endliche nichtnegative Angaben anzeigen | Unbekannt lassen. |
| Entfernung | Nur gelieferte endliche nichtnegative Werte anzeigen | „Entfernung unbekannt“, keine Null erfinden. |
| Kandidat | Stop-ID vorhanden, vollständige Identität und Mitgliedschaft in aktueller Ergebnismenge | Veraltete Auswahl ignorieren. |
| Ergebnis | Revision stimmt und Cancellation nicht erfolgt | Keine UI-Übernahme oder Navigation. |
## Konfigurationsänderungen
Kein neues Nutzer-Einstellungsmenü. Adapter-Standard: Medium, 15 Sekunden, keine temporäre Vollgenauigkeitsanforderung. iOS-Plist wie oben; bestehende Nearby-Radius-/Provider-/Cachekonfiguration weiterverwenden.
## Seiteneffekte und Risiken
`EndpointViewModel.Changed` beeinflusst Routinginvalidierung. `StopMonitorViewModel.Stops` beeinflusst Listen-, Karten- und Monitorauswahl. Alte Karten dürfen neue Ergebnismengen nicht umgehen. Native Seitenabgänge dürfen fertige Kandidaten nicht löschen. OS-Berechtigungen können zwischen zwei Anfragen wechseln; deshalb nicht dauerhaft cachen. Windows kann ohne Sensor oder bei deaktiviertem Dienst keine Position liefern; Fixture-Erfolg ist kein OS-Erfolgsnachweis.
## Umsetzungsreihenfolge
1. Core-Vertrag/Ergebnis und MAUI-Adapter samt iOS-Beschreibung anlegen. Voraussetzung: vorhandene MAUI- und Core-Projekte, keine zusätzlichen Pakete.
2. Endpunktintegration und deterministische Übernahme-/Fehler-/Revisionsprüfungen. Voraussetzung: Schritt 1 und vorhandener Endpoint-/Command-Unterbau.
3. Nearby-Kette, aktive Kandidaten und Kartenmetadaten integrieren. Voraussetzung: Schritt 1, vorhandenes `NearbyAsync`, Monitor und Karte.
4. Bestehende Seiten/DI ergänzen, Fixture-Adapter und native Standorttests bauen. Voraussetzung: Schritte 2–3 und bestehende UIA-Infrastruktur.
5. Native Windows-Flüsse und echte OS-Probe, Reviews, Dokumentation und abschließende Checks. Voraussetzung: lauffähiger UiTest- und Release-Build. iOS-Anleitung getrennt übergeben.
## Tests
### Neue deterministische Tests
`LocationTests`: Erfolg Start/Ziel unabhängig; verweigert/entzogen/deaktiviert/Timeout/null/unsupported/Exception; alte Position nach Texteingabe oder Kandidatenwahl ignoriert; Wechsel und Seitenabgang; erneute Aktion nach Abbruch. Nearby erhält exakte Fixturekoordinate; komplette Stop-ID bleibt erhalten; Quelle/Fallback/Alter und bekannte/unbekannte Entfernung korrekt; leer/Fehler mit verständlicher Datenzuordnung; verspätete alte Standort- und Providerantworten überschreiben weder neue manuelle Suche noch neue Umgebung; Karten-/Listenwahl validiert aktuelle Identität.
### Betroffene bestehende Tests
Endpoint-/JourneySearch-, Monitor- und Karten-Tests bei Konstruktoränderungen anpassen, bestehende fachliche Erwartungen erhalten. Gesamte Core-Suite mit mindestens 70 % Zeilenabdeckung, Format, XML-Dokumentationsprüfung und Windows Release-Build mit Warnungen als Fehler.
### Native Windows-E2E (primärer Funktionsnachweis)
Erweiterung unter `tests/WindowsJourneyUiTests`; stabile AutomationIds, echte Click-/Input-/Navigation-Aktionen. Fixture-Flüsse im nativen UiTest-Build:
| Pflichtszenario | Sichtbarer Nachweis | AK |
|---|---|---|
| Startseite ohne Standortaktion | Keine Standortanfrage; manuelle Suche weiterhin erfolgreich | 1, 3 |
| Standort als Start, danach als Ziel | Auswahl „Aktueller Standort“, Verbindungsergebnisse, Details und Zurück; manuelle Alternative je Seite | 1 |
| Verweigert, nach Erfolg entzogen, deaktiviert, Timeout, keine Position | Verständlicher Status, Busy endet, erneute Aktion möglich, manuelle Suche gelingt | 1, 4 |
| Umgebung → Liste → Monitor → Zurück | Gewählte vollständige Haltestelle, Abfahrten und Metadaten stimmen | 2 |
| Umgebung → Karte → Marker/Listenalternative → Monitor | Beide Wege öffnen denselben aktuellen Stop; Position fehlt nur beim entsprechenden Marker | 2 |
| Nearby leer/Providerfehler | Verständlicher Status, alte Daten nicht als neue Umgebung bezeichnet; manuelle Suche bleibt nutzbar | 2, 4 |
| Langsame Position → manuelle Änderung / neue Auswahl | Späte Antwort überschreibt weder Endpunkt noch manuelle Stop-Ergebnisse | 3 |
| Langsames Nearby → neue Umgebung / Zurück | Kein fremder Ergebniswechsel, keine verspätete Navigation; erneutes Öffnen funktioniert | 3 |
| Schmale Darstellung/Tastatur/Zurück | Buttons erreichbar, Texte umbrechen, keine abgeschnittenen Hinweise | 1, 2 |
Bestehende Routing-, Monitor- und Karten-E2E wegen gemeinsamer Kandidaten/Controls gezielt als Regression ausführen. Der echte Release-Build muss separat die Windows-OS-Abfrage durch tatsächlichen Standortbutton bedienen. Berechtigungs-/Dienstzustand und Erfolg bzw. konkrete technische Grenze ohne genaue Position protokollieren; bei Erfolg Nearby über reale Provider prüfen. Betriebssystemeinstellungen nicht stillschweigend ändern. Keine echten Koordinaten, Wohnortableitungen oder sensitive Screenshots in dauerhaften Protokollen. Simulierte Ablehnung/Entzug explizit als Fixture kennzeichnen, tatsächliche OS-Dialoge separat. Nicht ausführbare OS-Wege erst nach ernsthaftem Versuch mit Grund als nicht ausgeführt festhalten, niemals als PASS.
Native iOS-Build/Geräteprüfung liegt beim Nutzer: Erstzustimmung, Ablehnung, Entzug in Einstellungen, reduzierte Genauigkeit, Dienst aus, fehlende Position, Start/Ziel/Nearby und manuelle Rückfälle mit konkreter Anleitung. Lokal iOS-Code/Plist prüfen; Windows-CI bleibt erhalten.
## Dokumentation
Dauerhilfe Standort/Datenschutz und Fehlerbehebung, Herkunft von Distanz/Genauigkeit, keine Historie, Plattformgrenzen und iOS-Prüfanleitung ergänzen. README/Release Notes/changes.log nach verifizierter Implementierung aktualisieren. OS-Probe, Fixture-Nachweise und Testgrenzen getrennt ablegen. Kein IIS und kein Deployment.
## Offene Punkte
Keine.
