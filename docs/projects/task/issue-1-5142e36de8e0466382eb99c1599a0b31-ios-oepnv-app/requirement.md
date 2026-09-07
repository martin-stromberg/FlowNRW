# Projektanforderung – iOS ÖPNV-App

Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`  
Issue: #1 · Aufgaben-ID: `5142e36d-e8e0-4663-82eb-99c1599a0b31`  
Quelle: `issue.md`, ergänzender Gestaltungsvorschlag `design-draft.zip`, verbindliche Nutzerergänzung zu Windows-CI vom 2026-09-07  
Stand: 2026-09-07

## Ziel

Die vorhandene .NET-MAUI-Anwendung soll ihre Standardvorlageninhalte durch eine nutzbare iOS-ÖPNV-App ersetzen. Sie liefert bundesweite Verbindungen und Abfahrten und bevorzugt NRW-spezifische Echtzeitdaten. Die Umsetzung erfolgt in C#, mit MVVM, für die Entwicklung in Visual Studio.

## Belegte Anforderungen aus issue.md und Nutzerergänzung

| ID | Anforderung |
|---|---|
| A01 | Standardvorlageninhalte entfernen und die beschriebene App im vorhandenen MAUI-Projekt implementieren; Zielplattform iOS, C#, MVVM, Visual Studio. |
| A02 | Start und Ziel über Adresse, Haltestelle oder GPS-Koordinaten eingeben und die nächsten verfügbaren Verbindungen ausgeben. |
| A03 | Verbindungen mit Umstiegen, Fußwegen, Linien und Betreiberinformationen sowie Echtzeitdaten, soweit verfügbar, anzeigen. |
| A04 | Bei Start/Ziel innerhalb NRW regionale Datenquellen priorisieren. |
| A05 | Haltestellen für Abfahrtsmonitore über Suche oder Karte auswählen; nächste Abfahrten mit Echtzeit, Verspätungen, Ausfällen und Gleis-/Steigangaben darstellen. |
| A06 | Monitore in konfigurierbaren Intervallen aktualisieren. Auf der Startseite die Monitore favorisierter Haltestellen nach Entfernung sortieren. |
| A07 | Haltestellen bundesweit suchen sowie GPS-basierte nahe Haltestellen anzeigen. |
| A08 | Karte zur Darstellung von Haltestellen und Linienverläufen integrieren. |
| A09 | NRW-Echtzeitdaten regional abrufen, bundesweite Daten damit ergänzen und Soll-/Ist-Daten konsolidieren. |
| A10 | Bundesweite Routing-API sowie NRW-spezifische EFA/TRIAS-Endpunkte für Echtzeit und Abfahrten verwenden; Antworten einheitlich normalisieren und bei fehlenden Daten einen Fallback anbieten. |
| A11 | Klare, minimalistische, barrierearme Oberfläche und intuitive Navigation zwischen Routing, Monitor und Haltestellensuche; bereitgestellten Designentwurf berücksichtigen. |
| A12 | Views, ViewModels und Services trennen; Routing-, Abfahrts-, Haltestellen- und Echtzeitservices mit Dependency Injection und ausschließlich asynchronen API-Aufrufen einsetzen. |
| A13 | Haltestellen und häufig genutzte Daten cachen, Netzwerkzugriffe optimieren, API-Ausfälle behandeln und Echtzeitinformationen im Hintergrund aktualisieren. |
| A14 | Ohne Zustimmung keine personenbezogenen Daten speichern; API-Aufrufe über sichere Verbindungen; lokale Speicherung nur technischer Daten, beispielsweise Favoriten. |
| A15 | Erweiterbarkeit für weitere Verkehrsverbünde und Sharing-Dienste sowie optionale Push-Benachrichtigungen für Störungen/Abfahrten vorsehen. |
| A16 | Unit-Tests für Services und Datenmodelle, UI-Tests für Routing und Abfahrtsmonitor sowie Logging von API-Fehlern und Performance-Metriken bereitstellen. |
| A17 | Windows bleibt als Release- und Testplattform in den vorhandenen GitHub Actions erhalten. iOS ist dort laut Nutzer nicht zuverlässig möglich; eine iOS-CI-/Deployment-Pipeline gehört nicht zum Umfang. Automatisiertes Deployment wird später anderweitig implementiert. Die iOS-App bleibt das Produktziel; lokale Testnachweise entfallen dadurch nicht. |

## Betroffene Bereiche

- MAUI-App, iOS-Einstieg, Navigation und Gestaltung.
- Routing einschließlich Eingabeauflösung, Ergebnisliste und Verbindungsdetails.
- Abfahrtsmonitor, Favoriten und lokale technische Einstellungen.
- Haltestellensuche, Standortzugriff und Karten mit Linienverläufen.
- Externe Datenanbieter, NRW-Priorisierung, Normalisierung, Echtzeitkonsolidierung und Fallback.
- Cache, Netzwerk- und Aktualisierungssteuerung, Fehlerzustände und Diagnose.
- Datenschutz, Barrierearmut, Tests und Erweiterungsschnittstellen.
- Bestehende Windows-Release- und Testabläufe in GitHub Actions bleiben erhalten.

## Fachliche Abläufe

1. **Verbindung finden:** Start und Ziel als Adresse, Haltestelle oder Koordinate erfassen, auflösen und bundesweite Verbindungen abrufen; NRW-Quellen passend priorisieren. Nächste Ergebnisse mit Linien, Betreiber, Fußwegen, Umstiegen und verfügbaren Soll-/Ist-Daten darstellen.
2. **Abfahrten betrachten:** Haltestelle suchen oder auf Karte auswählen, kommende Abfahrten abrufen und inklusive Verspätung, Ausfall und Gleis/Steig anzeigen. Aktualisierung erfolgt im eingestellten Intervall.
3. **Favoriten nutzen:** Haltestellen lokal als Favoriten verwalten; Startseite zeigt ihre Abfahrtsmonitore nach Entfernung zum verfügbaren Standort.
4. **Umgebung erkunden:** Mit Standortzugriff nahe Haltestellen auf Karte und in passender Auswahl darstellen; Haltestellen und verfügbare Linienverläufe visualisieren und zur Haltestellenauswahl verwenden.
5. **Daten aktualisieren:** Anbieterantworten normalisieren, regionale Echtzeit mit bundesweiten Soll-Daten konsolidieren, häufige Daten cachen und bei fehlenden Daten/API-Ausfall den Fallback nutzen. Hintergrundaktualisierung berücksichtigt die tatsächlichen iOS-Möglichkeiten.

## Konfiguration

**Explizit gefordert:** konfigurierbares Aktualisierungsintervall für Abfahrtsmonitore; bundesweiter Routing-Anbieter und NRW-EFA/TRIAS-Anbindung; sichere API-Verbindungen; technische lokale Favoritenhaltung.

**Noch nicht konkret festgelegt:** Anbieter, Endpunkt-URLs, erforderliche Zugangsdaten und Nutzungsbedingungen; unterstützte iOS-/MAUI-Versionen; Intervallgrenzen und Standardintervall; Cache-Lebensdauer; Kartenanbieter; genaue Hintergrundaktualisierungsstrategie. Diese Werte sind keine bereits bestätigten Produktvorgaben.

## Akzeptanzkriterien

| ID | Überprüfbares Ergebnis |
|---|---|
| K01 | Die iOS-MAUI-App zeigt anstelle des Vorlageninhalts die ÖPNV-Oberfläche; Views, ViewModels und injizierte Services sind getrennt und API-Aufrufe asynchron (A01, A12). |
| K02 | Eine bundesweite Verbindungssuche funktioniert für Adressen, Haltestellen und Koordinaten; Ergebnisse enthalten nächste Verbindungen mit Umstiegen, Fußwegen, Linien, Betreibern und vorhandenen Echtzeitangaben (A02, A03). |
| K03 | NRW-Start-/Ziel-Anfragen priorisieren die regionale Quelle; regionale Echtzeit ergänzt normalisierte bundesweite Daten, Soll und Ist bleiben unterscheidbar; fehlende regionale Daten führen zum nachvollziehbaren Fallback (A04, A09, A10). |
| K04 | Eine gesuchte oder auf Karte ausgewählte Haltestelle öffnet ihren Monitor; reale Daten mit Verspätung, Ausfall und Gleis-/Steigangaben werden korrekt wiedergegeben, sofern der Anbieter sie liefert (A05). |
| K05 | Das konfigurierte Aktualisierungsintervall wirkt auf die Monitore; Favoriten bleiben lokal erhalten und ihre Startseitenmonitore werden bei verfügbarem Standort nach Entfernung sortiert (A06). |
| K06 | Bundesweite Haltestellensuche und GPS-Nahbereichssuche funktionieren; Karten zeigen Haltestellen und gelieferte Linienverläufe (A07, A08). |
| K07 | Gestaltung und Navigation berücksichtigen den Designentwurf und ermöglichen barrierearme Nutzung der Kernfunktionen (A11). |
| K08 | Cache, optimierte Abrufe, Fehlerbehandlung und Hintergrundaktualisierung sind implementiert und für erfolgreiche, fehlende sowie ausfallende Daten prüfbar (A13). |
| K09 | API-Verbindungen sind abgesichert; technische lokale Daten bleiben vom Speichern personenbezogener Daten ohne Zustimmung getrennt (A14). |
| K10 | Anbieter- und Servicegrenzen ermöglichen weitere Verkehrsverbünde sowie spätere Sharing-/optionale Push-Erweiterungen, ohne diese Integrationen als bereits geliefert auszugeben (A15). |
| K11 | Service-/Modell-Unit-Tests und UI-Tests der Verbindungssuche/des Monitors sowie datensparsame API-Fehler-/Performance-Diagnose sind vorhanden; tatsächliche Testausführung und Plattformgrenzen werden dokumentiert (A16). |
| K12 | GitHub Actions behalten die vorhandenen Windows-Test- und Releaseabläufe funktionsfähig bei. Änderungen zur iOS-App ersetzen oder entfernen diese Abläufe nicht. Eine iOS-CI-/Deployment-Pipeline wird nicht als Lieferung vorausgesetzt; verfügbare lokale Build-/Unit-/UI-Testnachweise und verbleibende Plattformgrenzen werden weiterhin ausgewiesen (A17). |

## Designquelle und Abgrenzung

Das Archiv enthält HTML-Entwürfe für `abfahrtsmonitor_live`, `verbindungssuche`, `fahrtbegleiter_detail` und `umgebungskarte_stationen`, eine App-Flow-Dokumentation, ein Designsystem sowie ein Icon. Die vier Screen-PNGs haben jeweils nur 28 Byte; ihre Verwendbarkeit darf nicht vorausgesetzt werden. HTML und Markdown bleiben auswertbare Referenzen.

Das Design beschreibt iOS-Karten, Linienkennzeichnungen, Soll-/Ist-Zeiten, Statusfarben, eine Stationskarte und Querverbindungen zwischen Suche, Monitor, Karte und Verbindungsdetail. Es enthält darüber hinaus Funktionen, die issue.md nicht als Kernumfang fordert: aktive GPS-Reisebegleitung, Wagenreihung, Umstiegsalarm, Verkehrsmittel-/Fahrradfilter, Deutschlandticket-Gültigkeit, Ausstattung/Sharing-Verfügbarkeit, Live-Fahrzeuge, Tickets und Mikrofon-/QR-Eingabe. Die Flow-Dokumentation nennt vier Tabs einschließlich Fahrtbegleiter; das Designsystem nennt fünf andere Tabs einschließlich Tickets/Favoriten. Diese Widersprüche dürfen keine unbelegte Scope-Erweiterung auslösen.

## Explizite Nicht-Ziele

**Verbindliche Nutzerergänzung:** Eine iOS-CI-/Deployment-Pipeline in GitHub Actions und die Implementierung eines neuen automatisierten Deployments sind Nicht-Ziele dieses Projekts. Das Deployment wird später anderweitig umgesetzt. Die bestehenden Windows-Release- und Testabläufe bleiben bestehen. Diese Abgrenzung hebt die Anforderungen an die iOS-App und verfügbare lokale Tests nicht auf.

issue.md nennt keinen eigenen Nicht-Ziele-Abschnitt. Es fordert insbesondere keine Veröffentlichung, keinen App-Store-Upload und keine tatsächliche Sharing-Integration als aktuelle Kernfunktion; Sharing und optionale Pushs stehen unter Erweiterbarkeit. Aus dieser Einordnung folgt keine Zusage ihrer sofortigen Implementierung. Ein Ausschluss weiterer Funktionen wurde vom Nutzer nicht ausdrücklich bestätigt.

## Annahmen und begründete Vorschläge (nicht als Nutzervorgaben behandeln)

- Der verbindliche Kernumfang ergibt sich aus issue.md. Designreferenzen leiten die Gestaltung und die zu diesem Kern passenden Interaktionen; Verbindungsdetail ist für Umstiege/Fußwege sinnvoll. Zusatzfunktionen und widersprüchliche Tabs benötigen keine stillschweigende Umsetzung.
- „Start/Ziel innerhalb NRW“ wird konservativ als mindestens einer der beiden Punkte in NRW interpretiert; der konkrete Auswahlmechanismus muss bei Planung fachlich begründet werden.
- Standortdaten werden nur mit Betriebssystemberechtigung genutzt. Ohne Standort bleibt manuelle Eingabe bedienbar; eine nicht berechenbare Entfernung wird nicht erfunden.
- Hintergrundaktualisierung ist unter den iOS-Laufzeitregeln umzusetzen; ein dauerhaft garantiertes Intervall bei suspendierter App ist nicht aus der Anforderung abzuleiten.
- Unbekannte Echtzeit-, Betreiber-, Gleis- oder Geometriedaten bleiben erkennbar unbekannt statt durch erfundene Live-Daten ersetzt zu werden.

## Offene Fragen für die Projektplanung

1. **Designumfang – behandelt:** issue.md bestimmt den fachlichen Umfang, das Archiv dient als Gestaltungsvorlage. Passende Verbindungsdetails werden berücksichtigt; zusätzliche Funktionen werden nicht übernommen. Die widersprüchliche Tab-Struktur wird anhand des Kernumfangs aufgelöst. Daraus entsteht keine erforderliche Nutzerentscheidung.
2. **Produktive Datenversorgung:** Welche bundesweiten und NRW-Endpunkte/Zugänge stehen zur Verfügung? Vorschlag: zunächst vorhandene Repository-Konfiguration und dokumentierte Anbieter prüfen; nur bei danach fehlenden zwingenden Zugängen Nutzerentscheidung einholen.
3. **Lokale Abnahmeumgebung:** Sind Mac/Xcode, iOS-Simulator oder Gerät für lokale Build- und UI-Tests verfügbar? Dies zunächst technisch ermitteln; fehlende lokale Möglichkeiten als tatsächliche Prüfgrenze dokumentieren. Eine iOS-Ausführung in GitHub Actions wird gemäß Nutzerergänzung nicht verlangt; Windows-Tests und -Releases dort bleiben erhalten.

Weitere konkrete Implementierungsentscheidungen gehören nach Bestandsaufnahme in den Projektplan bzw. Lifecycle und sind keine zusätzliche Nutzeranforderung.



