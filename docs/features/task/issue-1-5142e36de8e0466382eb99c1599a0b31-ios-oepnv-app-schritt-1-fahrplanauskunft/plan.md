# Umsetzungsplan: konfigurierbare bundesweite und regionale Fahrplanauskunft

## Übersicht

Der vorhandene `FlowNRW.Core`-Fachkern wird um eine plattformunabhängige Datenversorgung für Ortssuche, nahe Haltestellen, Routing, Abfahrten und NRW-Echtzeit erweitert. Konfigurierbare Gateway-Adapter für `db.transport.rest` und den VRR-EFA-RapidJSON-Testzugang normalisieren ihre Antworten in gemeinsame Fachmodelle; ein begrenzter In-Memory-Cache, Abbruch/Wiederholung, datensparsame Diagnose und nachvollziehbare Fallbacks werden ergänzt. Der Schritt enthält keine UI-Änderung, erhält die Windows-CI und weist die sechs Akzeptanzkriterien mit deterministischen Tests, Live-Proben und mindestens 70 % Core-Zeilenabdeckung nach.

## Abdeckung der Akzeptanzkriterien

| AK | Geplante Umsetzung und Nachweis |
|----|--------------------------------|
| 1 | `DbRestProvider`, `EfaProvider`, Ortssuche/Routing/Abfahrten, echte NRW- und bundesweite Live-Proben sowie dokumentierte Datenquellengrenzen; db-rest-503 wird als Grenze und EFA als vollständiger Entwicklungsfallback dokumentiert. |
| 2 | Gemeinsame Journey-/Leg-/StopEvent-Modelle, ISO-8601- und EFA-Zeitnormalisierung, Umstiege/Fußwege/Linien/Betreiber/Geometrien und unbekannte optionale Werte. |
| 3 | `NrwRegionClassifier`, `ProviderOrchestrator` und `RealtimeConsolidator` mit eindeutiger Fahrt-/Haltestellenzuordnung, Fallback, Warnungen, Quelle und Datenalter. |
| 4 | Bounded-TTL-Cache, deduplizierte Abrufe, Cancellation, Retry/Timeout, HTTPS und datensparsame Diagnose ohne Adress-/Standorthistorie oder Geheimnisse. |
| 5 | Deterministische Provider-, Modell-, Routing-, Prioritäts-, Fallback-, Konsolidierungs-, Abfahrts-, Cache- und Fehler-Tests; Format/Warnungen-als-Fehler, Core-Coverage ≥70 % und bestehende Windows-Konfiguration. |
| 6 | Austauschbare Provider-/Servicegrenzen, dokumentierte alternative EFA-Entwicklungsoption, begründete technische Werte und konkrete Blockademeldung bei fehlendem Zugang. |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|------------------|------------|
| Provideranbindung | `DbRestProvider` und vollständig konfigurierbarer `EfaProvider` hinter gemeinsamen Service-Interfaces | `db-rest` wird wegen der dokumentierten/zu dokumentierenden 503-Grenze nicht als einzige Versorgung vorausgesetzt; EFA deckt Suche, Nähe, Routing und Abfahrten als Entwicklungsfallback ab. |
| Fachliche Normalisierung | Gemeinsame immutable Datenmodelle als Value Objects mit explizit unbekannten optionalen Werten | Fehlende Anbieterfelder werden nicht erfunden und können Quelle/Datenalter bis zur späteren UI transportieren. |
| HTTP-Ausführung | Asynchroner, DI-fähiger `HttpClient`-Gateway mit `CancellationToken`, begrenztem Timeout und begrenzter Wiederholung | Nutzt vorhandene .NET-/MAUI-Infrastruktur ohne zusätzliche Bibliothek und verhindert überholte bzw. endlose Aufrufe. |
| JSON-Verarbeitung | `System.Text.Json` mit provider-spezifischen DTOs und Mappern | Ist im Ziel-Framework verfügbar; die Antwortformate von `db.transport.rest` und VRR RapidJSON bleiben getrennt testbar. |
| NRW-Erkennung | Expliziter, versionierter NRW-Grenzdatensatz aus BKG/Geoportal-NRW bzw. DHID `state05` plus Point-in-Polygon-Klassifikation | Die Anforderung verlangt die tatsächliche NRW-Grenze; VRR-Raum und grobe Rechtecke sind fachlich unzureichend. |
| Cache | Begrenzter In-Memory-TTL-Cache im Fachservice | Schritt 1 benötigt technischen Cache ohne personenbezogene Persistenz oder Datenbankmigration; Cachegrenzen bleiben konfigurierbar. |
| Konsolidierung | `RealtimeConsolidator` mit mehrstufiger Cross-Provider-Kandidatenregel: DHID, sonst räumliche/Namensauflösung und anschließend Linie/Betreiber/Richtung/Sollzeit/Haltestelle | Providerinterne `TripId`/`StopId` und Quellen sind nicht gleich; nur ein eindeutiger Kandidat darf gemergt werden, Mehrdeutigkeiten bleiben unverbunden. |
| Bundesweite EFA-Option | EFA-Adapter als konfigurierbare Entwicklungsoption dokumentieren und für Search/Nearby/Trip/Departures als Fallback ausführbar machen | Der VRR-Testzugang ist belegt; eine bundesweite EFA-Abdeckung oder Produktivfreigabe ist nicht belegt und wird nicht behauptet. |
| Geheimnisse und Logs | Endpunkte/optionale Zugangsdaten über Optionsobjekte; Werte nie in Logs oder Exceptions | Erfüllt HTTPS- und Datenschutzvorgaben bei austauschbarer Konfiguration. |

Verbindliche Standardwerte für den ersten Implementierungsstand sind: HTTP-Timeout `10 s`, höchstens `1` Wiederholung, maximal `256` Cacheeinträge, Stop-/Stammdaten-TTL `24 h`, Echtzeit-TTL `30 s`, als veraltet markierter Echtzeit-Cache höchstens `5 min` und maximal `100` normalisierte Antwortobjekte je Abruf. Diese Werte begrenzen Wartezeit, Netzlast und Speicher; sie werden konfigurierbar dokumentiert und durch Tests abgesichert.

Dokumentierte Quellen für die Feld- und Endpointplanung sind die [db.transport.rest-v6-Dokumentation](https://v6.db.transport.rest/api.html) und die [offizielle OpenData-ÖPNV/VRR-API-Seite](https://www.opendata-oepnv.de/ht/de/api). Die db-Dokumentation beschreibt unter anderem `locations`, `locations/nearby`, `journeys` und `stops/:id/departures` mit ISO-8601-Datumswerten sowie `plannedWhen`, `when`, `delay` und `platform`; die VRR-Seite stellt EFA-RapidJSON/JSON-Dokumentation und einen Abfahrtsmonitor-Testzugang bereit. Tatsächliche Antwortfelder werden zusätzlich durch gespeicherte anonymisierte Fixtures und Live-Proben verifiziert.

## Programmabläufe

### Ortssuche und nahe Haltestellen

1. Ein fachlicher Service validiert Suchtext bzw. Koordinaten und erzeugt eine abbrechbare Anfrage.
2. Der konfigurierte Provideradapter ruft `locations` bzw. `locations/nearby` ab; bei db-rest-503, Timeout oder nicht verwendbarer Antwort führt `ProviderOrchestrator` dieselbe Suche/Nähe über `EfaProvider` aus.
3. Der Mapper überführt Adressen, Haltestellen und Koordinaten in gemeinsame Modelle; Anbieter-IDs bleiben als technische Identifikatoren erhalten.
4. Leere, partielle oder fehlerhafte Antworten werden als Ergebniszustand mit Quelle/Fehlerdiagnose zurückgegeben und nicht durch erfundene Treffer ersetzt.

Beteiligte Klassen/Komponenten: `IStopSearchService`, `DbRestProvider`, `DbRestResponseMapper`, `Stop`, `Address`, `GeoCoordinate`, `NearbyStopResult`, `ProviderResult`.

### Verbindungssuche und Normalisierung

1. `RoutingService` nimmt zwei fachliche Orte als Adresse, Haltestelle oder Koordinate entgegen und bestimmt die Provideranfrage.
2. Der bevorzugte bundesweite Gatewayadapter ruft `journeys` mit URL-kodierten Parametern und den erforderlichen Ergebnisoptionen ab; bei HTTP 503 bzw. Providerfehler nutzt der Orchestrator den EFA-Entwicklungsadapter auch für Suche, Nähe und Routing.
3. `DbRestResponseMapper` bzw. `EfaResponseMapper` normalisieren Fahrten, Teilstrecken, Umstiege, Fußwege, Linien, Betreiber, Geometrie und Zeitwerte einschließlich Zeitzone/Tageswechsel.
4. `RoutingService` sortiert nächste Ergebnisse fachlich nach Abfahrts-/Ankunftszeit und reicht Quelle, Datenalter sowie unbekannte optionale Werte weiter.
5. Bei leerer Antwort, Warnung oder Providerfehler wird der definierte Fallback ausgeführt; die Antwort bleibt als Fallback bzw. Fehlerzustand erkennbar.

Beteiligte Klassen/Komponenten: `IRoutingService`, `RoutingService`, `DbRestProvider`, `EfaProvider`, `DbRestResponseMapper`, `EfaResponseMapper`, `Journey`, `JourneyLeg`, `Transfer`, `WalkingSegment`, `Line`, `Operator`, `GeoGeometry`.

### Regionale Priorität und NRW-Echtzeitkonsolidierung

1. `INrwRegionClassifier` klassifiziert jeden Start-/Zielpunkt anhand des dokumentierten Grenzdatensatzes.
2. Sobald mindestens ein Punkt in NRW liegt, ruft `ProviderOrchestrator` den konfigurierten EFA-Adapter bevorzugt ab; außerhalb NRW bleibt der bundesweite Adapter primär, mit EFA als transparenter Fallbackoption.
3. `EfaProvider` sendet die konfigurierten EFA-RapidJSON-Abfragen für Ortssuche, nahe Haltestellen, Routing und Abfahrten/Echtzeit; `EfaResponseMapper` normalisiert die tatsächlich gelieferten Felder.
4. `RealtimeConsolidator` verwendet eine gemeinsame DHID ausschließlich zur Haltestellenzuordnung. Für die Fahrtzuordnung muss zusätzlich eine eindeutige Fahrtkennung vorliegen oder der Kandidat muss anhand von Linie, Betreiber, Richtung und Sollzeit eindeutig sein; bei fehlender DHID werden Haltestellen über räumliche Nähe und normalisierten Namen bestimmt.
5. Kein eindeutiger Kandidat, widersprüchliche IDs oder Abweichung über die festgelegte Toleranz führen zu keinem Merge; der Datensatz bleibt mit Quelle/Datenalter getrennt bzw. Echtzeit unbekannt. Quelle, Datenalter und Warnungen werden im `ProviderResult` erhalten.

Beteiligte Klassen/Komponenten: `INrwRegionClassifier`, `NrwRegionClassifier`, `IProviderOrchestrator`, `ProviderOrchestrator`, `IEfaProvider`, `EfaProvider`, `RealtimeConsolidator`, `StopEvent`, `RealtimeStatus`, `TripIdentity`.

### Cache, Abbruch, Wiederholung und Diagnose

1. `MemoryTransitCache` prüft vor dem Netzwerkabruf den typisierten Cache-Schlüssel aus Anfrage, Provider und relevanten Optionen.
2. Frische Treffer werden zurückgegeben; abgelaufene Einträge werden entfernt oder als gekennzeichnete Fallbackdaten verwendet, ohne sie als aktuell auszugeben.
3. Jeder HTTP-Aufruf erhält ein verknüpftes `CancellationToken`, ein konfiguriertes Timeout und höchstens die konfigurierte Zahl an Wiederholungen für geeignete transienten Fehler.
4. Beim Abbruch oder beim Eintreffen einer neueren Anfrage wird der überholte Vorgang beendet und nicht als aktuelles Ergebnis veröffentlicht.
5. `TransitDiagnostics` protokolliert nur Provider, Dauer, Statusklasse, Ergebnisgröße und technische Fehlercodes; URL-Parameter mit Adressen, Antwortinhalte, Zugangsdaten und Standortverläufe werden ausgeschlossen.

Beteiligte Klassen/Komponenten: `ITransitCache`, `MemoryTransitCache`, `RetryPolicy`, `TransitDiagnostics`, `ITransitHttpGateway`.

### Live-Proben und Nachweisführung

1. Eine reproduzierbare Probe für `db.transport.rest` dokumentiert Ortssuche und eine bundesweite Verbindung mit Datum, Endpoint, HTTP-Ergebnis, normalisiertem Ergebnis und Datenquellengrenzen; ein HTTP-503 wird als aktuelle Providergrenze festgehalten.
2. Eine reproduzierbare EFA-Probe dokumentiert einen NRW-Abfahrtsabruf und, falls db-rest 503 bleibt, eine bundesweite Verbindung über den EFA-Entwicklungsadapter mit Datum, Endpoint, HTTP-Ergebnis, gelieferten Feldern und Datenalter.
3. Probeantworten werden ohne Zugangsdaten oder personenbezogene Anfrageinhalte als anonymisierte Fixtures für deterministische Mapper- und Konsolidierungstests gespeichert.
4. Die Dokumentation unterscheidet Live-Nachweise, Fixtures, Providerwarnungen und nicht belegte Abdeckung.

Beteiligte Klassen/Komponenten: Provideradapter, Mapper, `ProviderProbe`-Dokumentation und Core-Testfixtures.

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `Address` | Datenmodellklasse | Normalisierte Adresse als Ortseingabe. |
| `GeoCoordinate` | Datenmodellklasse | Breitengrad/Längengrad mit validierter Darstellung. |
| `Stop` | Datenmodellklasse | Normalisierte Haltestelle mit Quelle und Anbieterkennungen. |
| `NearbyStopResult` | Datenmodellklasse | Haltestelle und gelieferte Nähe-/Distanzinformation. |
| `Journey` | Datenmodellklasse | Normalisierte Verbindung mit Zeit, Teilstrecken und Metadaten. |
| `JourneyLeg` | Datenmodellklasse | Ein Fahrt-, Fuß- oder Umstiegsabschnitt. |
| `WalkingSegment` | Datenmodellklasse | Fußwegabschnitt mit vorhandener Dauer/Geometrie. |
| `Transfer` | Datenmodellklasse | Normalisierter Umstieg. |
| `Line` | Datenmodellklasse | Linie einschließlich vorhandener Kennzeichnung. |
| `Operator` | Datenmodellklasse | Betreiberinformation. |
| `GeoGeometry` | Datenmodellklasse | Vom Anbieter gelieferte Geometrie. |
| `StopEvent` | Datenmodellklasse | Normalisierte Abfahrt bzw. Ankunft mit Soll-/Istwerten. |
| `RealtimeStatus` | Datenmodellklasse | Verspätungs-, Ausfall-, Plattform- und Datenaltersstatus. |
| `TripIdentity` | Datenmodellklasse | Eindeutiger Zuordnungsschlüssel von Fahrt und Haltestelle. |
| `ProviderResult<T>` | Datenmodellklasse | Daten, Quelle, Datenalter, Warnungen und Fallbackstatus. |
| `TransitProviderOptions` | Konfigurationsklasse | Endpunkte, Zeitüberschreitungen, Wiederholungen und Provideroptionen. |
| `TransitCacheOptions` | Konfigurationsklasse | Größen- und TTL-Grenzen des technischen Caches. |
| `RetryPolicy` | Infrastrukturklasse | Begrenzte Wiederholungsentscheidung für HTTP-Aufrufe. |
| `MemoryTransitCache` | Infrastrukturklasse | Begrenzter, typisierter In-Memory-Cache. |
| `TransitDiagnostics` | Infrastrukturklasse | Datensparsame Laufzeit- und Fehlerdiagnose. |
| `DbRestProvider` | Gateway/Adapter | Zugriff auf `db.transport.rest` für Locations, Nearby, Journeys und Departures. |
| `EfaProvider` | Gateway/Adapter | Vollständiger EFA-Entwicklungsadapter für Suche, nahe Haltestellen, Routing und Abfahrts-/Echtzeitabrufe. |
| `DbRestResponseMapper` | Mapper | Mapping der dokumentierten db-rest-JSON-Strukturen. |
| `EfaResponseMapper` | Mapper | Mapping der tatsächlich geprüften EFA-RapidJSON-Antworten. |
| `RealtimeConsolidator` | Service | Eindeutige regionale/bundesweite Soll-/Ist-Konsolidierung. |
| `NrwRegionClassifier` | Service | Klassifikation anhand des echten NRW-Grenzdatensatzes. |
| `ProviderOrchestrator` | Service | Auswahl, Priorisierung und Fallback der Provider. |
| `RoutingService` | Service | Fachliche Verbindungssuche und Normalisierung. |
| `StopSearchService` | Service | Fachliche Adress-/Haltestellen-/Koordinatenauflösung. |
| `DepartureService` | Service | Fachlicher Abfahrtsabruf. |

Erforderliche Contracts werden als Interfaces angelegt: `IRoutingService`, `IStopSearchService`, `IDepartureService`, `ITransitProvider`, `IEfaProvider`, `IProviderOrchestrator`, `IRealtimeConsolidator`, `INrwRegionClassifier`, `ITransitCache`, `ITransitHttpGateway` und `ITransitDiagnostics`. Provider-spezifische DTOs bleiben intern zu den Adaptern. Ein separater `JourneyResponseMapper`, `CachedTransitDataProvider` oder `TransitRequestOptions` wird nicht zusätzlich angelegt: Journey-Mapping gehört in `DbRestResponseMapper`/`EfaResponseMapper`, Cache in `MemoryTransitCache`, Anfrageparameter in den jeweiligen Service-/Optionsmodellen.

## Änderungen an bestehenden Klassen

### `MauiProgram` (DI-Komposition)

- **Neue Registrierungen:** Optionsobjekte, `HttpClient`-Gateway, Fachservice-Interfaces, Provideradapter, Mapper, Cache und Diagnose.
- **Geänderte Methoden:** `CreateMauiApp` behält App-/Font-/Logging-Konfiguration und ergänzt ausschließlich die fachliche DI-Komposition.

### `FlowNRW.Core.csproj` (Projektkonfiguration)

- **Geänderte Konfiguration:** Core bleibt bei `net10.0`, Nullable, impliziten Usings, XML-Dokumentation und Warnungen als Fehler. Zusätzliche NuGet-Pakete werden nur aufgenommen, wenn die vorhandene .NET-/MAUI-Infrastruktur die benötigte Funktion nicht bereitstellt.

### `FlowNRW.Tests.csproj` (Testkonfiguration)

- **Geänderte Konfiguration:** Coverage-Sammlung und Testausführung werden für die neuen Core-Tests genutzt; die vorhandenen xUnit-/Coverlet-Pakete bleiben bestehen.

### `ClickCounter` / `ClickCounterTests`

- Keine fachliche Änderung geplant. Die fünf vorhandenen Counter-Testfälle müssen weiterhin bestehen.

## Datenbankmigrationen

Keine. Der Schritt verwendet fachliche HTTP-Daten und einen begrenzten In-Memory-Cache. Eine persistente personenbezogene Adress-/Standorthistorie wird nicht eingeführt.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `GeoCoordinate` | Breitengrad liegt in `[-90,90]`, Längengrad in `[-180,180]`; finite Zahlen erforderlich. | Ungültige Eingabe wird als fachlicher Validierungsfehler zurückgegeben; kein HTTP-Aufruf. |
| `Address` / Suchtext | Nicht leer, nach Trim innerhalb einer begrenzten konfigurierten Länge. | Ungültige Suche wird abgewiesen; sensible Rohwerte nicht loggen. |
| `Stop`-Kennung | Nur vom Provider gelieferte, nicht leere Kennung verwenden. | Treffer wird verworfen oder als unvollständig markiert; keine erfundene Kennung. |
| Zeitwerte | ISO-8601-Offset bzw. dokumentiertes EFA-Zeitformat mit expliziter Zeitzone normalisieren. | Antwortfeld bleibt unbekannt und erzeugt eine Warnung; keine lokale Zeitannahme. |
| Providerantwort | HTTP-Erfolg, erwartetes JSON/RapidJSON und notwendige Identitätsfelder prüfen. | Leere/partielle Antwort, Warnung oder Providerfehler wird als `ProviderResult`-Fehler/Fallback behandelt. |
| NRW-Klassifikation | Punkt-in-Polygon gegen den versionierten NRW-Grenzdatensatz. | Außerhalb/unklar löst keine regionale Priorität aus; Unklarheit wird diagnostisch erfasst. |

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `TransitProviders:DbRest:BaseUrl` | URI | `https://v6.db.transport.rest` als dokumentierte Entwicklungsoption | Bundesweite Locations-, Nearby-, Journey- und Departure-Anfragen. |
| `TransitProviders:Efa:BaseUrl` | URI | VRR-OpenService-Testendpoint nach dokumentierter Konfiguration | Vollständige EFA-Entwicklungsfallbacks für Suche, Nähe, Routing und Abfahrten. |
| `TransitProviders:Efa:Format` | Enum/String | `rapidJSON` | Auswahl des geprüften EFA-Antwortformats. |
| `TransitProviders:Efa:FallbackBaseUrl` | URI/optional | leer/deaktiviert | Transparente deutschlandweite EFA-Entwicklungsoption; keine behauptete Abdeckung. |
| `TransitHttp:Timeout` | Dauer | `00:00:10` | Abbruch lang laufender Aufrufe. |
| `TransitHttp:MaxRetries` | Ganzzahl | `1` | Begrenzte Wiederholung transienter Fehler. |
| `TransitCache:MaxEntries` | Ganzzahl | `256` | Speichergrenze des In-Memory-Caches. |
| `TransitCache:StopTimeToLive` | Dauer | `24:00:00` | Frischegrenze für Stop-/Stammdaten. |
| `TransitCache:RealtimeTimeToLive` | Dauer | `00:00:30` | Frischegrenze für Echtzeit. |
| `TransitCache:MaxStaleAge` | Dauer | `00:05:00` | Maximale Kennzeichnung eines veralteten Fallbacks. |
| `TransitProvider:MaxResults` | Ganzzahl | `100` | Antwortlimit gegen unkontrollierte Datenmenge. |
| `TransitRegion:NrwBoundaryVersion` | String | versionierter Ressourcenstand | Nachvollziehbare NRW-Grenzklassifikation. |

Endpunkte, mögliche Zugangsdaten und Produktivfreigaben werden nicht in den Quelltext oder Logs geschrieben. Konkrete Standardwerte für Timeout, Wiederholungen und TTL werden nach Messung und Sicherheitsprüfung im Implementierungsschritt dokumentiert.

## Seiteneffekte und Risiken

- **Providerverfügbarkeit:** Einzelne Live-Proben belegen keine dauerhafte oder flächendeckende Versorgung. Adapter müssen Fehler/Fallback sichtbar machen; eine zwingende Zugangssperre wird als Blockade gemeldet.
- **Anbieteridentitäten:** Unterschiedliche Fahrt-/Haltestellenkennungen können Konsolidierung verhindern. Ohne eindeutige Zuordnung bleibt Echtzeit unbekannt.
- **NRW-Grenzdatensatz:** Der Datensatz wird in einem frühen Vorbereitungsschritt aus einer dokumentierten BKG-/Geoportal-NRW-Quelle oder, wo vorhanden, DHID `state05` plus echtem Polygon als lizenzkonformes Bundeslandpolygon beschafft, versioniert und mit Quelle dokumentiert. Fehlt die Ressource, bricht die Implementierung der regionalen Priorität kontrolliert ab; ein grobes Rechteck ist unzulässig.
- **Rate Limits und Netzlast:** `db.transport.rest` dokumentiert Rate Limits; Cache, Deduplication, Timeout und begrenzte Wiederholungen sind deshalb notwendig.
- **Bestehende Vorlage:** `MainPage` verwendet `ClickCounter`; die Vorlage bleibt in Schritt 1 unverändert, damit keine UI-Scope-Erweiterung entsteht.
- **CI:** Bestehende Windows-Release-/Testjobs dürfen durch Core-Änderungen nicht brechen; iOS-CI und automatisiertes Deployment werden nicht ergänzt.

## Umsetzungsreihenfolge

1. **Projekt- und Konfigurationsgrundlage anlegen**
   - Voraussetzungen: Bestehendes `FlowNRW.Core`- und `FlowNRW.Tests`-Projekt, .NET 10 SDK, vorhandene xUnit-/Coverlet-Pakete. Keine neuen Bibliotheken erforderlich.
   - Beschreibung: BKG-/Geoportal-NRW-Verwaltungsgrenzen bzw. DHID-`state05`-Zuordnung als lizenzkonformes Bundeslandpolygon beschaffen, Quelle/Version dokumentieren; bei fehlender Ressource fail-fast abbrechen. Danach Optionsmodelle, gemeinsame Fehler-/Quellen-/Datenalterswerte und XML-dokumentierte Interfaces anlegen.
2. **Gemeinsame Datenmodelle und Validierung implementieren**
   - Voraussetzungen: Schritt 1 Interfaces/Optionsobjekte.
   - Beschreibung: Value Objects für Orte, Haltestellen, Journeys, Legs, Abfahrten, Identitäten und Providerergebnisse samt Validierungsregeln implementieren.
3. **HTTP-Gateway, Diagnose und Cache implementieren**
   - Voraussetzungen: Gemeinsame Modelle und Optionsobjekte.
   - Beschreibung: DI-fähiges `HttpClient`-Gateway, sichere Header/HTTPS-Prüfung, Cancellation, Timeout, begrenzte Wiederholung, datensparsame Diagnose und bounded TTL-Cache implementieren.
4. **`db.transport.rest`-Adapter implementieren**
   - Voraussetzungen: HTTP-Gateway, Modelle und Mapper-Verträge.
   - Beschreibung: `locations`, `locations/nearby`, `journeys` und `stops/:id/departures` mit URL-Encoding, dokumentierten Parametern und provider-spezifischen DTOs anbinden; ISO-8601-Zeitwerte und dokumentierte Felder mappen.
5. **Vollständigen EFA-Adapter implementieren**
   - Voraussetzungen: HTTP-Gateway, Abfahrtsmodelle und RapidJSON-Fixture-Schema.
   - Beschreibung: Konfigurierbare EFA-RapidJSON-Abfragen für Ortssuche, nahe Haltestellen, Routing und Abfahrts-/Echtzeitabruf mit Stopfinder-/Abfahrtsidentitäten, Soll-/Ist-/Verspätungs-/Ausfall-/Gleisfeldern und Warnungen abbilden. Eine alternative deutschlandweite EFA-Basis-URL bleibt als Entwicklungsoption transparent konfigurierbar. Die db-rest-503-Probe wird als Providergrenze dokumentiert.
6. **Fachservices und NRW-Priorisierung implementieren**
   - Voraussetzungen: Beide Adapter, NRW-Klassifikator, Cache und gemeinsame Modelle.
   - Beschreibung: `StopSearchService`, `RoutingService`, `DepartureService`, `ProviderOrchestrator` und `RealtimeConsolidator` verbinden; regionale Priorität ab mindestens einem NRW-Punkt, eindeutige Zuordnung und nachvollziehbarer Fallback umsetzen.
7. **DI und sichere Konfiguration integrieren**
   - Voraussetzungen: Fertige Adapter und Services.
   - Beschreibung: Alle Contracts in `MauiProgram` registrieren, Endpunkte/Optionen laden, Geheimnisse nicht protokollieren und Windows-Komposition ohne iOS-CI-Anforderung erhalten.
8. **Deterministische Tests und Live-Proben erstellen**
   - Voraussetzungen: Implementierte Core-Komponenten und anonymisierte Providerfixtures.
   - Beschreibung: Mapper-, Service-, Validierungs-, NRW-, Cache-, Abbruch-, Wiederholungs-, Fehler- und Konsolidierungstests schreiben; echte NRW-Abfahrts- und bundesweite Routing-/Ortssuchproben mit Datum und Grenzen dokumentieren.
9. **Dauerhafte Erweiterungs- und Betriebsdokumentation erstellen**
   - Voraussetzungen: Provider- und Servicegrenzen, Konfigurationsschema und Live-/Fixture-Nachweise aus den vorherigen Schritten.
   - Beschreibung: `docs/help/fahrplanauskunft/` mit Provider-Erweiterung, Feld-/ID-Zuordnung, NRW-/EFA-/db-rest-Grenzen, Cache-/Retry-Werten, späteren Sharing-/Push-Erweiterungspunkten und Entwicklungs-/Produktivgrenzen anlegen. Die Abnahme prüft, dass eine weitere Verbundquelle anhand dieser Anleitung ergänzt werden kann und aktuelle Sharing-/Push-Funktionen nicht als geliefert erscheinen.
10. **Qualitäts- und Coverage-Nachweis ausführen**
   - Voraussetzungen: Tests und Providerproben.
   - Beschreibung: Format, Build mit Warnungen als Fehler, Core-Tests, Coverage (mindestens 70 % Zeilen) und bestehende Windows-CI-relevante Prüfungen ausführen; fehlende Providerzugänge als konkrete Blockade dokumentieren.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `GeoCoordinate_RejectsOutOfRangeValues` | `TransitModelTests` | Koordinatenvalidierung. |
| `ProviderMapper_MapsDbRestLocations` | `DbRestResponseMapperTests` | Adress-/Haltestellen-/Koordinatenmapping aus JSON. |
| `ProviderMapper_MapsDbRestJourneyTimesAndLegs` | `DbRestResponseMapperTests` | Journey, Fußweg, Umstieg, Linie, Betreiber, Geometrie und ISO-8601-Zeitwerte. |
| `ProviderMapper_HandlesJourneyAcrossMidnight` | `DbRestResponseMapperTests` | Fahrtzeiten über Mitternacht mit korrekter lokaler Zeitzone und Tageswechsel. |
| `ProviderMapper_MapsDbRestDepartures` | `DbRestResponseMapperTests` | `plannedWhen`, `when`, `delay`, `platform` und unbekannte Felder. |
| `EfaMapper_NormalizesExplicitTimezoneAndDepartureStates` | `EfaResponseMapperTests` | EFA-Zeitzone, Ausfall, fehlende Echtzeit und die Unterscheidung „unbekannt“ gegenüber pünktlich. |
| `ProviderMapper_MapsEfaRapidJson` | `EfaResponseMapperTests` | Tatsächlich geprüfte EFA-RapidJSON-Felder, Stop-/Trip-Identitäten und Warnungen. |
| `ProviderOrchestrator_PrioritizesNrwWhenEitherPointIsInsideBoundary` | `ProviderOrchestratorTests` | NRW-Priorität mit mindestens einem NRW-Punkt. |
| `NrwClassifier_DoesNotUseVrrOrBoundingBox` | `NrwRegionClassifierTests` | Echte Grenzklassifikation an Innen-/Außen-/Randpunkten. |
| `RealtimeConsolidator_MergesOnlyMatchingTripAndStop` | `RealtimeConsolidatorTests` | Eindeutige Zuordnung; fremde/mehrdeutige Fahrten bleiben ungemischt. |
| `RealtimeConsolidator_DoesNotMergeDifferentTripsWithSameDhid` | `RealtimeConsolidatorTests` | Gemeinsame DHID identifiziert nur die Haltestelle; unterschiedliche Fahrten werden ohne eindeutige Fahrtkriterien nicht gemergt. |
| `ProviderOrchestrator_UsesFallbackForEmptyPartialAndHttpFailure` | `ProviderOrchestratorTests` | Leere/partielle Antwort, Warnung, HTTP-Fehler und Fallbackstatus. |
| `TransitCache_ExpiresAndBoundsEntries` | `MemoryTransitCacheTests` | TTL und Maximalgröße. |
| `TransitCache_ReturnsStaleOnlyWithinFiveMinutesAndMarksIt` | `MemoryTransitCacheTests` | Stale-Fallback bis `5 min`, danach kein aktueller Treffer. |
| `ProviderOrchestrator_DeduplicatesIdenticalRequests` | `ProviderOrchestratorTests` | Gemeinsamer Abruf für identische Provider-/Anfrage-Schlüssel. |
| `TransitHttp_CancellationStopsOverdueRequest` | `TransitHttpGatewayTests` | Weitergabe des `CancellationToken` und kein veraltetes Ergebnis. |
| `RetryPolicy_RetriesOnlyBoundedTransientFailures` | `RetryPolicyTests` | Begrenzte Wiederholungen und nicht wiederholbare Fehler. |
| `Diagnostics_DoesNotLogQuerySecretsOrAddresses` | `TransitDiagnosticsTests` | Datensparsame Logs. |
| `LiveDbRestProbe_IsDocumented` | `ProviderProbeDocumentationTests` | Fixture/Metadaten für eine echte bundesweite Probe, sofern Endpoint erreichbar. |
| `LiveEfaProbe_IsDocumented` | `ProviderProbeDocumentationTests` | Fixture/Metadaten für eine echte NRW-Abfahrtsprobe. |

Zusätzliche Fixture-Hilfsmethoden erzeugen ausschließlich anonymisierte, deterministische Providerantworten; sie ersetzen die geforderten Live-Proben nicht. Die Tests müssen die Core-Zeilenabdeckung auf mindestens 70 % bringen.

### Betroffene bestehende Tests

Keine. `ClickCounterTests` bleibt unverändert und muss weiterhin mit 5 Testfällen bestehen.

### E2E-Tests (primärer Funktionsnachweis)

Der Schritt 1 verändert keine fachliche UI und enthält keine Benutzerinteraktion. Deshalb sind für diesen Schritt keine nativen UI-E2E-Tests erforderlich. Die fachliche Funktionsgrenze wird durch echte HTTP-Proben plus deterministische Adapter-/Service-/Modelltests nachgewiesen; UI-E2E für Verbindungssuche und Abfahrtsmonitor beginnt im abhängigen Schritt 2.

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Nicht anwendbar | Keine UI in Schritt 1 | — | — | Es gibt in diesem Schritt keinen Nutzerfluss; Core-/Providernachweise sind der passende Nachweis. |

Welche bestehenden E2E-Tests müssen angepasst werden? Keine vorhanden bzw. keine in diesem Schritt betroffen.

## Offene Punkte

Keine. Die verbleibenden Prüfungen (db-rest-503-Nachweis, EFA-Felder und Identitäten, GeoBasis-NRW-Ressource, Live-Erreichbarkeit und gegebenenfalls fehlende Produktivfreigabe) sind als konkrete Umsetzungsschritte bzw. Risiken in den Abschnitten „Programmabläufe“, „Umsetzungsreihenfolge“ und „Seiteneffekte und Risiken“ festgelegt. Ein tatsächlich fehlender Zugang wird im Implementierungsnachweis als konkrete Blockade ausgewiesen.
