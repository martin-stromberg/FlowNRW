# Anforderungsübersetzung – konfigurierbare bundesweite und regionale Fahrplanauskunft

## Fachliche Zusammenfassung

Die C#-ÖPNV-App erhält eine unabhängig von der späteren Oberfläche nutzbare, plattformunabhängige Datenversorgung. Fachliche Services lösen bundesweit Adressen, Haltestellen und Koordinaten auf, liefern nahe Haltestellen sowie Verbindungen und Abfahrten und konsolidieren bei mindestens einem Start- oder Zielpunkt in NRW bevorzugt regionale Echtzeitdaten. Antworten externer Anbieter werden in ein einheitliches fachliches Modell überführt; fehlende Daten, Warnungen und Anbieterausfälle führen zu nachvollziehbaren Fallbacks und niemals zu erfundenen Fahrten.

Dieser Schritt liefert die Auskunftsgrundlage vor der UI. Er enthält keine fachliche UI-Änderung und hat deshalb keine native UI-Abnahme als Lieferumfang. Echte API-Proben und deterministische Service-/Modelltests weisen die Datenversorgung nach.

Die Eingabe- und Auswahlfunktionen sind fachlich eigenständige Anforderungen: Adressen, Haltestellen und Koordinaten müssen als Start- und Zielpunkte identifizierbar sein; nahe Haltestellen müssen geliefert werden. Eine Auswahl darf nicht auf die Eingabe interner Anbieterkennungen reduziert werden.

## Betroffene Klassen und Komponenten

Die konkreten Klassen- und Methodennamen werden in der Bestandsaufnahme und Planung aus den vorhandenen Projektkonventionen abgeleitet. Aus der Anforderung ergeben sich voraussichtlich folgende Komponenten:

- **Plattformunabhängiger Fachkern:** normalisierte Modelle für Adressen, Haltestellen, Koordinaten, nahe Haltestellen, Verbindungen, Teilstrecken, Umstiege, Fußwege, Linien, Betreiber, Geometrien sowie Abfahrten und Echtzeitstatus.
- **Routing- und Auflösungsservices:** fachliche Services für bundesweite Adress-, Haltestellen- und Koordinatenauflösung, nahe Haltestellen und nächste Verbindungen.
- **Abfahrts- und Echtzeitservices:** fachliche Services für Abfahrten, Soll-/Ist-Zeiten, Verspätungen, Ausfälle und Gleis-/Steigangaben sowie für die regionale Echtzeitkonsolidierung.
- **Provideradapter und Konfiguration:** ein konfigurierbarer bundesweiter Adapter, zunächst mit Prüfung von `db-rest`, sowie ein regionaler VRR-EFA-Adapter. Der punktuell für Berlin nachgewiesene VRR-Entwicklungszugang kann als mögliche bundesweite Rückfallebene geprüft werden; daraus wird keine Produktivfreigabe oder flächendeckende Abdeckung abgeleitet.
- **Normalisierung und Konsolidierung:** Zuordnung von Fahrt und Haltestelle, regionale Priorisierung, bundesweiter Fallback sowie Bereitstellung von Quelle, Datenalter und fehlender Echtzeit an nachgelagerte Komponenten.
- **Technische Infrastruktur:** begrenzter Cache, HTTP-Aufrufsteuerung mit Abbruch und begrenzten Wiederholungen, datensparsame Fehler- und Laufzeitdiagnose sowie sichere Endpunkt- und gegebenenfalls Geheimniskonfiguration.
- **Tests:** deterministische Service- und Modelltests einschließlich Providerantworten, Routing, regionaler Priorität, Fallback, Echtzeit- und Abfahrtszuständen, Cache und Fehlerfällen; ergänzend echte API-Proben für eine NRW-Abfahrt und eine bundesweite Verbindung.
- **UI:** In diesem Schritt keine fachlichen UI-Komponenten. Die Datenmodelle und Services müssen jedoch so nutzbar sein, dass die spätere Oberfläche Quelle, Datenalter und unbekannte Echtzeit darstellen kann.

Bestehende Windows-Test- und Releasekomponenten in GitHub Actions bleiben funktionsfähig. Eine iOS-CI oder ein neues automatisiertes Deployment gehört nicht zu diesem Schritt.

## Implementierungsansatz

Die Datenversorgung wird im vorhandenen C#-Fachkern als getrennte, per Dependency Injection registrierbare Services aufgebaut. Alle externen Aufrufe erfolgen asynchron, abbrechbar und über HTTPS. Die Providerantworten werden zunächst in Anbieteradapter-spezifischen Strukturen verarbeitet und anschließend in gemeinsame fachliche Modelle normalisiert.

Die Providergrenzen bleiben austauschbar. Für bundesweite Ortssuche und Routing wird ein konfigurierbarer Adapter vorgesehen; `db-rest` ist zunächst zu prüfen. Für regionale Abfahrten und Echtzeit wird der VRR-EFA-Adapter verwendet. Der VRR-Entwicklungszugang ist nur als Entwicklungsoption zu behandeln. Einzelne erfolgreiche Proben belegen weder dauerhafte Verfügbarkeit noch vollständige bundesweite Abdeckung. Eine zwingende fehlende Zugangsvoraussetzung wird als konkrete Blockade dokumentiert, statt eine vollständige Datenversorgung zu behaupten.

Mindestens ein Start- oder Zielpunkt in NRW aktiviert die regionale Priorität. Das Gebiet muss NRW fachlich abbilden und darf nicht lediglich mit dem VRR-Raum oder einem groben Rechteck gleichgesetzt werden. Regionale Ergebnisse werden mit bundesweiten Daten nur bei eindeutiger Fahrt- und Haltestellenzuordnung konsolidiert, damit keine fremden Fahrten vermischt werden. Bei leeren oder partiellen Antworten, HTTP- und Providerfehlern sowie Warnungen werden der regionale Rückfall, bundesweite Daten oder gekennzeichnete fehlende Werte nachvollziehbar bereitgestellt.

Normalisierte Verbindungen enthalten, soweit vom Anbieter geliefert, nächste Fahrten, Umstiege, Fußwege, Linien, Betreiber und Geometrie. Normalisierte Abfahrten enthalten Soll-/Ist-Zeit, Verspätung, Ausfall und Gleis/Steig. Nicht gelieferte Werte bleiben unbekannt. Zeitzonen und Tageswechsel werden bei der Normalisierung fachlich korrekt berücksichtigt.

Ein begrenzter technischer Cache und zusammengefasste bzw. optimierte Abrufe reduzieren Netzlast. Veraltete oder überholte Aufrufe können abgebrochen werden; Wiederholungen bleiben begrenzt. Logs enthalten weder Geheimnisse noch eine persistente personenbezogene Adress- oder Standorthistorie. Cachegrenzen und sichere Konfigurationswerte werden dauerhaft dokumentiert. Anbieter- und Servicegrenzen ermöglichen später weitere Verkehrsverbünde sowie Sharing- und Push-Erweiterungen, ohne diese Funktionen in diesem Schritt vorzutäuschen.

## Konfiguration

Die Konfiguration umfasst mindestens:

- austauschbare Endpunkte für den bundesweiten Adapter und den regionalen VRR-EFA-Adapter,
- gegebenenfalls erforderliche Zugangsdaten bzw. Geheimnisse in einer sicheren, nicht protokollierten Konfiguration,
- sichere Cachegrenzen und Cachelebensdauer,
- begrenzte Wiederholungs- und Abbruchparameter,
- die fachliche Auswahl und Priorisierung der regionalen Versorgung.

Die konkreten Werte, Endpunktfreigaben, Nutzungsbedingungen und Anbieterfelder sind noch zu prüfen und werden in der Planung begründet festgelegt. Produktionsfreigaben oder eine vollständige Datenabdeckung dürfen aus Entwicklungszugängen oder einzelnen Live-Proben nicht abgeleitet werden.

## Akzeptanzkriterien

1. Reale, konfigurierbare Adapter ermöglichen über fachliche Services bundesweite Ortssuche und Routing sowie NRW-EFA-Abfahrts- und Echtzeitabrufe. Adressen, Haltestellen und Koordinaten sowie nahe Haltestellen werden unterstützt. Eine konkrete Live-Probe für NRW-Abfahrten und eine bundesweite Verbindung wird jeweils mit Datum, Ergebnis und Grenzen der Datenquelle dokumentiert; Fixtures gelten nicht als Live-Nachweis.
2. Normalisierte Ergebnisse enthalten nächste Fahrten, Umstiege, Fußwege, Linien, Betreiber und vorhandene Geometrie. Abfahrten enthalten Soll-/Ist-Zeit, Verspätung, Ausfall und Gleis/Steig, soweit der Anbieter dies liefert. Fehlende Felder bleiben unbekannt; Zeitzonen und Tageswechsel werden korrekt verarbeitet.
3. Regionale Auswahl, bundesweiter Fallback und eindeutige Fahrt-/Haltestellenzuordnung konsolidieren NRW-Echtzeit, ohne fremde Fahrten zu vermischen. Leere oder partielle Antworten, HTTP-/Providerfehler und Warnungen werden verarbeitet. Quelle, Datenalter und fehlende Echtzeit bleiben für die spätere UI verfügbar.
4. Ein begrenzter technischer Cache und optimierte Abrufe vermeiden unnötige Netzlast. Abbruch, begrenzte Wiederholungen und datensparsame API-Fehler-/Laufzeitdiagnose sind vorhanden. HTTPS wird verwendet; eine persistente personenbezogene Adress-/Standorthistorie und Geheimnisse in Logs gibt es nicht. Cachegrenzen und sichere Konfiguration sind dokumentiert.
5. Service- und Modelltests prüfen Providerantworten, regionale Priorität, Fallback, Konsolidierung, Abfahrtszustände, Routing, Cache und Fehler deterministisch. Repository-Prüfungen einschließlich Format, Warnungen als Fehler und Core-Zeilenabdeckung werden ausgeführt. Die bestehende Windows-Test- und Releasekonfiguration bleibt funktionsfähig.
6. Anbieter- und Servicegrenzen sowie dauerhafte Dokumentation ermöglichen weitere Verbünde und spätere Sharing-/Push-Erweiterungen ohne aktuelle Scheinimplementierung. Technische Werte legt der Lifecycle begründet fest. Ergibt die API-Prüfung einen zwingenden fehlenden Zugang, wird die konkrete Blockade gemeldet und keine vollständige Datenversorgung behauptet.

## Offene Fragen

- Welche konkrete bundesweite Routing- und Ortssuche ist nach Live-Prüfung dauerhaft nutzbar, und welche Nutzungsbedingungen bzw. produktiven Freigaben gelten für die gewählten Endpunkte?
- Welche Anbieterfelder und stabilen Kennungen erlauben die eindeutige Zuordnung derselben Fahrt und Haltestelle zwischen bundesweiten und regionalen Antworten?
- Welche sicheren Standardwerte und Grenzen werden für Cachelebensdauer, Wiederholungen und Abbruch festgelegt?
- Welche konkrete Datenquelle liefert Geometrien und welche Geometriefelder sind tatsächlich verfügbar?
- Falls ein erforderlicher Zugang fehlt: Welche konkrete Blockade muss vor der Implementierung aufgelöst werden?

Die Fragen betreffen die technische Ausgestaltung und werden in der Umsetzungsplanung anhand von Bestandsaufnahme und API-Prüfung beantwortet. Die Vorgaben zu Windows-Release und -Tests, zum Verzicht auf iOS-CI sowie zur fehlenden UI-Abnahme in diesem Schritt sind bereits festgelegt.
