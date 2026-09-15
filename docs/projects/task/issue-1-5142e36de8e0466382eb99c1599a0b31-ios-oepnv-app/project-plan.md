# Projektplan – iOS ÖPNV-App

## Übersicht

Stand: 2026-09-15. Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`.

Ziel ist eine nutzbare C#/.NET-MAUI-App für iOS, die bundesweite Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten anbietet. Sie ersetzt die Standardvorlage, verwendet MVVM und bleibt unter Visual Studio entwickelbar. Windows bleibt als bestehende Release- und Testplattform in GitHub Actions erhalten.

Betroffen sind App-Einstieg und Plattformkonfiguration, Navigation und Gestaltung, Routing, Haltestellen und Standort, Monitore und Favoriten, Karten, Datenanbieter und Echtzeit, technische Speicherung, Hintergrundaktualisierung, Diagnose sowie Tests. Grundlage sind [requirement.md](requirement.md), [inventory.md](inventory.md), `issue.md` und die HTML-/Markdown-Referenzen aus `design-draft.zip`.

## Grobe Vorgehensentscheidungen

1. **Acht kleine Lieferungen:** Schritt 1 bleibt unverändert. Die offenen Pakete werden in manuelle Verbindungssuche, manuelle Haltestellensuche/Monitor, Karte, GPS-Nutzung, Favoriten, Vordergrundintervalle und Hintergrund/Wiederaufnahme getrennt. Nach jedem UI-Schritt steht eine eigenständig begutachtbare Funktion bereit. Vorhandenen Datenkern und gültige Nachweise weiterverwenden; keine erneute Providerimplementierung je UI-Schritt. Bestehende Branchzuordnungen 1–4 bleiben erhalten; neue Schritte 5–8 werden angehängt. Begründung und AK-Überleitung: [package-review.md](package-review.md).
2. **Design am Kernumfang ausrichten:** Native, minimalistische Navigation für Startseite/Monitore, Verbindungen und Haltestellen/Umgebung. Verbindungsdetails sind eine Unterseite. Blaue Interaktionsfarbe, gruppierte Karten, Linienkennzeichnungen sowie unterscheidbare Soll-/Ist-/Ausfallzustände übernehmen; Zusatzfunktionen des Entwurfs wie Tickets, aktive Reisebegleitung, Wagenreihung, Sharing-Angebote oder Mikrofoneingabe werden nicht als Anforderungen übernommen. Die unbrauchbaren Screen-PNGs sind keine Abnahmevorlage.
3. **Konfigurierbare reale Datenversorgung:** Bundesweite Routingversorgung und regionaler NRW-EFA-Adapter werden tatsächlich implementiert; EFA ist die gewählte Ausprägung der geforderten EFA/TRIAS-Anbindung, ein zweites gleichartiges Protokoll ist nicht erforderlich. Endpunkte und gegebenenfalls Zugangsdaten bleiben austauschbar. VRR bietet einen Entwicklungszugang; dessen Nutzung ist keine produktive Freigabe. db.transport.rest ist eine weitere bundesweite Entwicklungsoption, darf nach einer einzelnen HTTP-503-Probe nicht als garantiert erreichbar vorausgesetzt werden. Der VRR-Testzugang lieferte bereits Berliner Suchtreffer und für Berlin Hbf → Alexanderplatz über XML_TRIP_REQUEST2 zwei Verbindungen (HTTP 200, Warnung BROKER -10015 itp-monomodal); dies ermöglicht weitere bundesweite Integrationsprüfungen, belegt aber keine vollständige Deutschlandabdeckung. Lifecycle prüft die konkreten Felder und tatsächliche Abdeckung und wählt eine nachweisbar nutzbare bundesweite Konfiguration. Produktive Nutzungsbedingungen und Freigaben werden dokumentiert, nicht erfunden. Mockdaten dürfen ausschließlich ausdrücklich erkennbare Tests versorgen.
4. **Regionale Priorität und konservative Konsolidierung:** Mindestens ein Start-/Zielpunkt in NRW löst die regionale Priorität aus. Die Gebietsentscheidung muss NRW statt nur VRR oder einer groben NRW-umfassenden Rechteckfläche abbilden. Regional fehlende Ergebnisse werden durch die bundesweite Versorgung ergänzt. Echtzeit wird nur bei fachlich eindeutiger Zuordnung derselben Fahrt/Haltestelle übernommen; fehlende Werte bleiben unbekannt, Soll und Ist sowie Datenalter erkennbar.
5. **Datensparsame, robuste Nutzung:** Standort erst mit Betriebssystemberechtigung und nur für angeforderte Funktionen; manuelle Nutzung bleibt möglich. Keine persistente Standort- oder Adresshistorie. Technische Favoriten, Einstellungen und begrenzte Haltestellen-/Fachdaten-Caches sind zulässig; sensible Anfrageinhalte und Zugangsdaten fehlen in Logs. Sichere API-Verbindungen, Abbruch veralteter Anfragen, gemeinsame Abrufe und begrenzte Wiederholungen vermeiden unnötige Netzlast.
6. **Aktualisierung entsprechend Plattformregeln:** Im Vordergrund gelten konfigurierbare Intervalle; iOS-Hintergrundaktualisierung wird mit unterstützten Plattformmechanismen und Aktualisierung beim Wiederaufnehmen umgesetzt. Kein garantiertes Dauerintervall für suspendierte Apps. Standardwerte, sichere Grenzen und Cache-Lebensdauern legt Lifecycle begründet und dokumentiert fest.
7. **Erweiterbarkeit ohne Scheinprodukte:** Anbieter- und fachliche Servicegrenzen erlauben weitere Verbünde und spätere Sharing-/Push-Funktionen. Keine aktuelle Sharing-Integration, Push-Zustellung, App-Store-Veröffentlichung oder neues automatisiertes Deployment. Bestehende Windows-Releaseabläufe bleiben erhalten; kein iOS-Job in GitHub Actions erforderlich.
8. **Verbindlich bestätigte Abnahmestrategie:** Der Nutzer bestätigt: „Ja, Teste alle UI-Abläufe unter Windows soweit es möglich ist. iOS muss erst einmal bei mir liegen.“ Alle UI-Abläufe sind deshalb unter Windows soweit technisch möglich tatsächlich zu testen, automatisiert als native UI-E2E. Reine ViewModel-Tests ersetzen keine UI-Ausführung. Ernsthafte Versuche, Resultate und je nicht ausführbarem Windows-Fluss die konkrete technische Grenze werden dokumentiert; nicht ausgeführte Tests gelten nicht als bestanden. Native iOS-Abnahme einschließlich Build, Gerät/Simulator und plattformspezifischem Hintergrundverhalten liegt zunächst beim Nutzer und ist kein lokaler Abschlussblocker. iOS-Target und Plattformintegration sind weiterhin zu implementieren und im Code zu prüfen; eine ausführbare manuelle iOS-Prüfanleitung wird übergeben. Die bestätigte Prüfaufteilung bedarf keiner erneuten Rückfrage. Repository-Konventionen einschließlich XML-Dokumentation, Warnungen als Fehler, Format, Core-Tests und mindestens 70 % Zeilenabdeckung bleiben maßgeblich.

9. **IIS-Begutachtung:** Die vorhandene Website „ÖPNV“ ist für manuelle Zwischenlieferungen ausdrücklich freigegeben. Nach abgenommenen UI-Schritten einen geprüften Windows-ZIP-Stand mit Startanleitung, Commit/Version, Umfang und Grenzen bereitstellen. IIS verteilt die native App und Begutachtungsinformationen; es entsteht keine Browserportierung oder Deploymentautomatisierung. Vor Installation Site-Zuordnung, absoluten Zielpfad und Zugriff prüfen, fremde Inhalte bewahren. Verzeichnis `D:\Dashboard\ÖPNV`: eigene Begutachtungsseite ist installiert; `http://localhost/%C3%96PNV/` liefert HTTP 200 und den erwarteten Inhalt, Quell-/Zielhash stimmen überein (Nachweis: `docs/review/README.md`). Ein Windows-App-Paket folgt erst mit bedienbarer UI. Konkrete Installationsgrenzen dokumentieren, keine erfolgreiche Veröffentlichung behaupten.

## Entwicklungsschritte

### Schritt 1: Konfigurierbare bundesweite und regionale Fahrplanauskunft

**Kundenanforderung:** Stelle die unabhängig von der späteren Oberfläche nutzbare Datenversorgung der C#-ÖPNV-App bereit. Fachliche Services müssen bundesweit Adressen, Haltestellen und Koordinaten auflösen, nahe Haltestellen liefern, nächste Verbindungen und Abfahrten abrufen und NRW-Echtzeit bevorzugt konsolidieren. Die tatsächlichen externen Antworten werden einheitlich normalisiert. Bei fehlenden Daten oder Anbieterausfall bestehen nachvollziehbare Fallbacks statt erfundener Fahrten. Diese lieferbare Auskunftsgrundlage wird vor der UI implementiert und durch echte API-Proben sowie deterministische Service-/Modelltests geprüft.

**Betroffene Bereiche:** Plattformunabhängiger Fachkern, Routing-, Haltestellen-, Abfahrts- und Echtzeitservices, Providerkonfiguration, Cache, Diagnose, Datenschutz, Unit-/Integrationstests.

**Abhängigkeiten:** Keine.

**Verbindliche Rahmenbedingungen:** C#, DI-fähige getrennte Services und ausschließlich asynchrone, abbrechbare HTTP-Aufrufe. Konfigurierbarer bundesweiter Adapter, zunächst db-rest prüfen, sowie regionaler VRR-EFA-Adapter. Den punktuell für Berlin Routing und Haltestellensuche nachgewiesenen VRR-Entwicklungszugang als mögliche bundesweite Rückfallebene prüfen; Produktionsfreigaben und flächendeckende Verfügbarkeit nicht behaupten. Endpunkte und gegebenenfalls Geheimnisse austauschbar halten. NRW-Priorität gilt bei mindestens einem Start-/Zielpunkt in NRW und darf nicht nur VRR oder ein grobes Rechteck abbilden. Windows-Release und Tests in GitHub Actions bleiben erhalten, keine iOS-CI oder neues automatisiertes Deployment. Dieser Schritt ändert keine fachliche UI und benötigt deshalb keine native UI-Abnahme; UI-Lieferung und native UI-E2E folgen ab Schritt 2.

**Akzeptanzkriterien:**

1. Reale konfigurierbare Adapter ermöglichen bundesweite Ortssuche/Routing und NRW-EFA-Abfahrts-/Echtzeitabrufe über fachliche Services. Adresse, Haltestelle und Koordinate sowie nahe Haltestellen sind unterstützt. Konkrete Live-Proben für NRW-Abfahrten und eine bundesweite Verbindung sind mit Datum, Ergebnis und Datenquellengrenzen dokumentiert; Fixtures sind kein Live-Nachweis.
2. Normalisierte Ergebnisse enthalten nächste Fahrten, Umstiege, Fußwege, Linien, Betreiber und vorhandene Geometrie; Abfahrten enthalten Soll/Ist, Verspätung, Ausfall und Gleis/Steig, soweit geliefert. Fehlende Felder bleiben unbekannt. Zeitzonen und Tageswechsel sind korrekt behandelt.
3. Regionale Auswahl, bundesweiter Fallback und eindeutige Fahrt-/Haltestellenzuordnung konsolidieren NRW-Echtzeit ohne fremde Fahrten zu vermischen. Leere/partielle Antworten, HTTP-/Providerfehler und Warnungen sind verarbeitet; Quelle, Datenalter und fehlende Echtzeit stehen der UI zur Verfügung.
4. Begrenzter technischer Cache und optimierte Abrufe verhindern unnötige Netzlast. Abbruch, begrenzte Wiederholungen und datensparsame API-Fehler-/Laufzeitdiagnose sind vorhanden. HTTPS wird verwendet; keine persistente personenbezogene Adress-/Standorthistorie oder Geheimnisse in Logs. Cachegrenzen und sichere Konfiguration sind dokumentiert.
5. Service-/Modelltests prüfen Providerantworten, regionale Priorität, Fallback, Konsolidierung, Abfahrtszustände, Routing, Cache und Fehler deterministisch. Repository-Prüfungen einschließlich Format, Warnungen als Fehler und Core-Zeilenabdeckung werden ausgeführt. Bestehende Windows-Test-/Releasekonfiguration bleibt funktionsfähig.
6. Anbieter-/Servicegrenzen und dauerhafte Dokumentation ermöglichen weitere Verbünde und spätere Sharing-/Push-Erweiterungen ohne aktuelle Scheinimplementierung. Technische Werte legt Lifecycle begründet fest. Ergibt die API-Prüfung einen zwingenden fehlenden Zugang, wird die konkrete Blockade gemeldet, nicht eine vollständige Datenversorgung behauptet.

### Schritt 2: Manuelle bundesweite Verbindungssuche mit NRW-Echtzeit

**Kundenanforderung:** Ersetze die MAUI-Vorlage durch eine unter Visual Studio entwickelbare Verbindungssuche mit Start und Ziel als manuell eingegebene Adresse, Haltestelle oder Koordinate. Zeige nächste bundesweite Verbindungen mit Details und bevorzugter NRW-Echtzeit. Richte die iOS-Plattformbasis ein und liefere früh eine bedienbare Windows-App zur Begutachtung. Automatische GPS-Übernahme folgt in Schritt 5.

**Betroffene Bereiche:** iOS/Windows-Basis, Eingabe, Verbindungsliste/Details, MVVM, UI-Tests.

**Abhängigkeiten:** 1.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. Vorlagenbegrüßung, Counter und zugehörige Logik sind entfernt. iOS-Target, Einstieg und Plattformdateien sind eingerichtet, Windows bleibt baubar. Navigation führt zur Suche und nutzbaren Ergebnissen/Details ohne unfertige Menüziele; Visual-Studio-Entwicklung ist dokumentiert.
2. Adresse, Haltestelle und gültige Koordinate funktionieren jeweils als Start und Ziel. Mehrdeutige Treffer sind auswählbar, ungültige Eingaben sowie Lade-/Leer-/Fehlerzustände verständlich. Veraltete Anfragen überschreiben keine neuen Ergebnisse. Manuelle Suche benötigt keine Standortberechtigung.
3. Zeitlich sinnvoll geordnete Verbindungen zeigen Umstiege, Fußwege, Linien, Betreiber und gelieferte Soll-/Ist-Zeiten einschließlich Tageswechsel/Zeitzone. Fehlende Echtzeit, Ausfall, Quelle und veralteter Cache bleiben nachvollziehbar; fehlende Werte werden nicht erfunden.
4. Die UI integriert reale bundesweite/regionale Services. Tests belegen NRW-Priorität für einen/beide Endpunkte, außerhalb NRW, ID-Zuordnung, Ergänzung eindeutiger Fahrten, Warnungen und leere/fehlerhafte Rückfälle ohne Fahrtvermischung. Bundesweite und NRW-Live-Verbindungsproben sind separat mit Datum/Ergebnis/Grenzen dokumentiert; Fixtures sind kein Live-Nachweis.
5. Native Windows-UI-E2E bedienen alle manuellen Eingabearten, Trefferwahl, Ergebnisse, Details, Rücknavigation und Fehlerzustände. Eingabe-/Zustandslogik ist deterministisch geprüft. Sichere Konfiguration, Datenquellen, Cache/Datenschutz, Plattformgrenzen und manuelle iOS-Prüfung sind dokumentiert. Der erste abgenommene Windows-Stand wird über IIS begutachtbar bereitgestellt.

### Schritt 3: Haltestellensuche und manuell aktualisierter Abfahrtsmonitor

**Kundenanforderung:** Ergänze die Verbindungssuche um eine bundesweite manuelle Haltestellensuche. Die gewählte Haltestelle öffnet einen Monitor mit realen nächsten Abfahrten und manueller Aktualisierung. GPS, Favoriten und automatische Intervalle folgen getrennt in Schritten 5–7.

**Betroffene Bereiche:** Haltestellensuche, Monitor, Echtzeitstatus, Navigation, UI-Tests.

**Abhängigkeiten:** 2.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. Bundesweit gesuchte Haltestellen sind eindeutig auswählbar und öffnen den richtigen Monitor. Lade-, Leer-, Fehlerzustände und Rücknavigation funktionieren ohne Standortzugriff.
2. Abfahrten zeigen Linie/Ziel, Soll-/Ist-Zeiten, Verspätung, Ausfall und Gleis/Steig einschließlich Änderungen. Fehlende Echtzeit bleibt von pünktlicher Echtzeit unterscheidbar; Ausfälle erscheinen nicht als regulär fahrend.
3. NRW-Echtzeit und bundesweite Ergänzungen werden aus dem vorhandenen Datenkern korrekt angezeigt. Leere/partielle Antworten, Mehrdeutigkeiten, Anbieterfehler, Quelle und Datenalter sind nachvollziehbar. Manuelle Aktualisierung blockiert die UI nicht; bei Fehlern bleiben letzte bekannte Daten erkennbar statt kommentarlos zu verschwinden. Identische Abrufe werden zusammengefasst.
4. Automatisierte Tests prüfen Soll-/Ist-Anzeige, Ausfall/Gleiswechsel, Fehler/Fallback und manuelle Aktualisierung. Native Windows-UI-E2E bedienen Suche → Monitor, Aktualisieren und Fehlerzustände; Routing bleibt funktionsfähig. Ein NRW-Live-Abfahrtsabruf wird separat mit Ergebnis und Grenzen dokumentiert.

### Schritt 4: Interaktive Haltestellenkarte und gelieferte Linienverläufe

**Kundenanforderung:** Ergänze eine interaktive Karte für manuell gesuchte Haltestellen und tatsächlich gelieferte Linien-/Verbindungsverläufe. Stationsauswahl öffnet ihren Monitor. Die Karte funktioniert zunächst in einer manuell gewählten Umgebung; GPS-Nahbereich folgt in Schritt 5.

**Betroffene Bereiche:** Karte, Stationsauswahl, Geometrie, Details/Monitor, UI-Tests.

**Abhängigkeiten:** 2, 3.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. Gesuchte Stationen stehen an tatsächlichen Koordinaten auf der interaktiven Karte; Auswahl öffnet den richtigen Monitor. Eine zugängliche Listenalternative bietet denselben Weg. Ohne Standort bleibt eine manuell gewählte Umgebung bedienbar.
2. Gelieferte Linien-/Verbindungsgeometrien sind zur gewählten Verbindung/Linie korrekt zugeordnet und auch aus Verbindungsdetails erreichbar. Fehlende Geometrie ist kenntlich statt erfunden. Quelle, Attribution und sichere technische Kartenkonfiguration sind dokumentiert.
3. Die technische Kartenlösung berücksichtigt iOS und Windows. Lade-/Offline-/Anbieterfehler und veraltete Daten sind verständlich; Cache-, Netzwerk- und Datenschutzgrenzen gelten auch für Kartenabrufe. Keine Bewegungshistorie wird gespeichert.
4. Automatisierte Tests prüfen Geometriezuordnung und Stationsauswahl. Native Windows-UI-E2E bedienen Umgebung → Karte → Monitor, Listenalternative, Details → Geometrie, Rücknavigation und Fehlerzustände; Routing/Monitor bleiben nutzbar.

### Schritt 5: Aktueller Standort und nahe Haltestellen

**Kundenanforderung:** Ergänze die manuellen Abläufe um den aktuellen GPS-Standort als Start oder Ziel sowie GPS-nahe Haltestellen in Liste und Karte. Nach Betriebssystemzustimmung können Nutzer ihre Umgebung erkunden und den gewählten Monitor öffnen; ohne Standort bleiben manuelle Abläufe verfügbar.

**Betroffene Bereiche:** Standortberechtigung, Routing-Eingabe, Nahbereich, Umgebungskarte, UI-Tests.

**Abhängigkeiten:** 2, 3, 4.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. Aktueller Standort ist nach Berechtigung als Start oder Ziel nutzbar. iOS-Berechtigungsbeschreibungen und Windows-Standortintegration sind vorhanden. Ablehnung, Entzug, Timeout und fehlende Position erzeugen verständliche Zustände und verhindern keine manuelle Suche.
2. GPS-nahe Stationen erscheinen passend zur tatsächlichen Position in Liste/Karte. Beide Auswahlwege öffnen den richtigen Monitor; eine manuell gewählte Umgebung bleibt möglich.
3. Standort dient nur angeforderten Funktionen, ohne persistente Bewegungs-/Adress-/Suchhistorie und sensible Logs. Veraltete Standort-/Suchergebnisse überschreiben keine neuere Auswahl. Unbekannte Positionen/Entfernungen werden nicht erfunden.
4. Deterministische Tests prüfen Berechtigungs-/Positionsfehler und Nahbereichszuordnung. Native Windows-UI-E2E bedienen GPS als Start/Ziel, Nahbereich → Monitor/Karte sowie Ablehnungs-/Fehlerwege soweit möglich. Tatsächliche Betriebssystemversuche und Fixture-Anteile bleiben getrennt; iOS-Prüfanleitung umfasst Berechtigung/Entzug.

### Schritt 6: Favoriten und nach Entfernung sortierte Startseitenmonitore

**Kundenanforderung:** Ermögliche technische Haltestellenfavoriten und zeige ihre Abfahrtsmonitore auf der Startseite. Bei erlaubtem verfügbarem Standort werden sie nach Entfernung sortiert. Manuelle Aktualisierung bleibt verfügbar; automatische Intervalle folgen in Schritt 7.

**Betroffene Bereiche:** Favoritenpersistenz, Startseite, Entfernungssortierung, UI-Tests.

**Abhängigkeiten:** 3, 5.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. Favoriten lassen sich in der Stations-/Monitorbedienung hinzufügen und entfernen, vermeiden Duplikate und überleben Neustarts. Nur erforderliche technische Stationsdaten werden gespeichert.
2. Die Startseite zeigt Favoritenmonitore aufsteigend nach Entfernung bei verfügbarem erlaubtem Standort. Ohne Standort gilt eine stabile Reihenfolge ohne erfundene Entfernung. Leere Favoriten bieten einen verständlichen Weg zur Haltestellensuche.
3. Monitore erhalten Echtzeit-/Fehler-/Cachezustände und manuelle Aktualisierung. Identische Abrufe werden vermieden; einzelne Ladezustände blockieren nicht die gesamte Startseite. Navigation zu Monitor, Routing und Karte bleibt konsistent.
4. Automatisierte Tests prüfen Persistenz, Duplikate, Entfernungssortierung und fehlenden Standort. Native Windows-UI-E2E bedienen Hinzufügen → Startseite, Neustart/Persistenz, Entfernen, Leerzustand, Sortierung und Navigation.

### Schritt 7: Konfigurierbare automatische Monitoraktualisierung

**Kundenanforderung:** Ergänze eine lokal gespeicherte Intervall-Einstellung für automatische Aktualisierung aktiver Einzel- und Favoritenmonitore im Vordergrund. Manuelle Aktualisierung bleibt möglich. Suspendierung und iOS-Hintergrundbetrieb folgen in Schritt 8.

**Betroffene Bereiche:** Intervall-Einstellung, Vordergrundaktualisierung, Netzlast, UI-Tests.

**Abhängigkeiten:** 3, 6.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. Das eingestellte Intervall wirkt tatsächlich auf aktive Einzel-/Startseitenmonitore und überlebt Neustarts. Standard und sichere Grenzen sind begründet dokumentiert; ungültige Werte werden verhindert.
2. Navigation und Intervallwechsel beenden überholte Aktualisierung. Identische Abrufe werden zusammengefasst; begrenzte Wiederholungen verhindern Anfragefluten. Inaktive Ansichten betreiben keine unnötigen Schleifen; Aktualisierung ist asynchron und abbrechbar.
3. Manuelle Aktualisierung bleibt verfügbar. Fehler machen letzte bekannte Daten und ihr Alter erkennbar, statt Daten kommentarlos zu löschen. Cache und Logs bleiben begrenzt und datensparsam.
4. Deterministische Tests prüfen Taktung, Intervallwechsel, Abbruch, gemeinsame Abrufe und Fehler. Native Windows-UI-E2E bedienen Einstellungen, sichtbar wirksame automatische Aktualisierung, manuelle Aktualisierung und Navigation ohne doppelte Schleifen; bestehende Routing-/Favoriten-/Monitorabläufe bleiben nutzbar.

### Schritt 8: Hintergrundaktualisierung und Wiederaufnahme der vollständigen App

**Kundenanforderung:** Vervollständige Echtzeitaktualisierung über den App-Lebenszyklus mit tatsächlich angebundenen unterstützten iOS-Hintergrundmechanismen und frischen Daten beim Wiederaufnehmen. Prüfe die einzeln gelieferten Kernabläufe gemeinsam und übergib den vollständigen Windows-Begutachtungsstand und die native iOS-Abnahmecheckliste.

**Betroffene Bereiche:** iOS-Lebenszyklus, Hintergrundarbeit, integrierte Abnahme, Betriebsdokumentation.

**Abhängigkeiten:** 2, 3, 4, 5, 6, 7.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Nach fachlicher Abnahme Windows-ZIP mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitstellen, Download und lokalen Start prüfen. Site-Zuordnung/Zielpfad vor Installation prüfen; konkrete Zugriffsgrenzen dokumentieren. IIS hostet Paket/Unterlagen, die App läuft nativ. Keine weiteren Veröffentlichungen.

**Akzeptanzkriterien:**

1. iOS-Hintergrundaktualisierung ist einschließlich Registrierung/Konfiguration und begrenzter Ausführung angebunden. Beim Wiederaufnehmen werden veraltete Echtzeitdaten erneuert. Kein permanentes Hintergrundintervall für suspendierte Apps wird versprochen; die UI erklärt Datenalter.
2. Vordergrundintervalle, Hintergrundarbeit, Wiederaufnahme und Navigation erzeugen keine doppelten Abrufschleifen. Abbruch, Offline-/Providerfehler, Cachegrenzen und Standortablehnung/-entzug funktionieren ohne personenbezogene Historie oder sensible Logs.
3. Alle Kernfunktionen sind konsistent navigierbar und gemäß Designkern gestaltet. Beschriftungen, Kontraste, Touch-Ziele, Textskalierung und nicht rein farbliche Echtzeit-/Ausfallzustände sind am tatsächlichen UI geprüft. Keine Vorlage oder unimplementierte Zusatzproduktansicht verbleibt.
4. Automatisierte Tests prüfen Wiederaufnahme, Koordination, Fehler und Datenschutzgrenzen. Native Windows-UI-E2E führen integrierte Verbindungssuche/Details, Haltestellensuche, GPS, Karte → Monitor, Favoriten, Einstellungen und Lade-/Leer-/Fehlerzustände soweit möglich tatsächlich aus; Wiederaufnahme wird unter Windows geprüft. Nicht ausführbare Flüsse erhalten konkrete Ursache und Versuchsnachweis.
5. Die integrierte App besteht verfügbare lokale Format-/Build-/Testprüfungen nach Repository-Konventionen. Windows-Release-/Test-Workflows bleiben funktionsfähig und unabhängig von iOS-Builds. iOS-Target/Plattformcode werden geprüft; native iOS-Build-/Geräte-/Simulator-/UI-/Hintergrundtests bleiben beim Nutzer und werden nicht als ausgeführt behauptet.
6. Dauerhafte Dokumentation deckt Visual Studio, Plattformvoraussetzungen, Provider-/Kartenkonfiguration und Produktionsfreigaben, Favoriten/Intervalle, Cache/Datenschutz, Hintergrundgrenzen, Testnachweise und konkrete manuelle iOS-Abnahme ab. Erweiterungsgrenzen für Verbünde, Sharing und optionale Pushs sind beschrieben. Vollständiger IIS-Begutachtungsstand enthält Paket, Startanleitung, Commit/Umfang und Grenzen; Deploymentautomatisierung bleibt gesonderte spätere Aufgabe.

## Übersichtstabelle

| # | Titel | Abhängigkeiten | Betroffene Bereiche |
|---|---|---|---|
| 1 | Konfigurierbare bundesweite und regionale Fahrplanauskunft | Keine | Fachkern, Provider, Normalisierung, Echtzeit, Cache, Diagnose, Tests |
| 2 | Manuelle bundesweite Verbindungssuche mit NRW-Echtzeit | 1 | iOS/Windows-Basis, Eingabe, Verbindungsliste/Details, MVVM, UI-Tests |
| 3 | Haltestellensuche und manuell aktualisierter Abfahrtsmonitor | 2 | Haltestellensuche, Monitor, Echtzeitstatus, Navigation, UI-Tests |
| 4 | Interaktive Haltestellenkarte und gelieferte Linienverläufe | 2, 3 | Karte, Stationsauswahl, Geometrie, Details/Monitor, UI-Tests |
| 5 | Aktueller Standort und nahe Haltestellen | 2, 3, 4 | Standortberechtigung, Routing-Eingabe, Nahbereich, Umgebungskarte, UI-Tests |
| 6 | Favoriten und nach Entfernung sortierte Startseitenmonitore | 3, 5 | Favoritenpersistenz, Startseite, Entfernungssortierung, UI-Tests |
| 7 | Konfigurierbare automatische Monitoraktualisierung | 3, 6 | Intervall-Einstellung, Vordergrundaktualisierung, Netzlast, UI-Tests |
| 8 | Hintergrundaktualisierung und Wiederaufnahme der vollständigen App | 2, 3, 4, 5, 6, 7 | iOS-Lebenszyklus, Hintergrundarbeit, integrierte Abnahme, Betriebsdokumentation |

## Offene Punkte

Keine fachlichen Entscheidungsfragen.

Windows-UI-Prüfung soweit möglich, native iOS-Abnahme beim Nutzer und Nutzung der vorbereiteten IIS-Website sind autorisiert. Die statische Begutachtungsseite ist unter `http://localhost/%C3%96PNV/` mit HTTP 200 und identischem Quell-/Zielhash geprüft. Die spätere externe Stakeholder-URL wird separat geklärt; die lokale Entwicklung ist dadurch nicht blockiert. Ein konkreter technischer Blocker wird dokumentiert und verhindert keine unabhängige Entwicklung. Provider-Produktivfreigaben bleiben vor späterem Produktivbetrieb gesondert erforderlich. Die bisherige Planprüfung ist wegen des Neuschnitts ungültig und wird unabhängig erneuert.
