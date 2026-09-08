# Tasks: konfigurierbare bundesweite und regionale Fahrplanauskunft

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Datenmodell | `Address` anlegen | Offen | — |
| 2 | Datenmodell | `GeoCoordinate` mit Bereichsvalidierung anlegen | Offen | — |
| 3 | Datenmodell | `Stop` und `NearbyStopResult` anlegen | Offen | — |
| 4 | Datenmodell | `Journey`, `JourneyLeg`, `WalkingSegment` und `Transfer` anlegen | Offen | — |
| 5 | Datenmodell | `Line`, `Operator`, `GeoGeometry` und `TripIdentity` anlegen | Offen | — |
| 6 | Datenmodell | `StopEvent` und `RealtimeStatus` anlegen | Offen | — |
| 7 | Datenmodell | `ProviderResult<T>` und gemeinsame Quellen-/Datenalterswerte anlegen | Offen | — |
| 8 | Konfiguration | `TransitProviderOptions` und sichere Endpointoptionen anlegen | Offen | — |
| 9 | Konfiguration | `TransitCacheOptions` und HTTP-Timeout-/Retryoptionen anlegen | Offen | — |
| 10 | Konfiguration | Offizielle GeoBasis-NRW-Verwaltungsgrenze als lizenzkonformes Polygon beschaffen, versionieren und bei Fehlen fail-fast behandeln | Offen | — |
| 11 | Interfaces | `ITransitProvider`, `IRoutingService`, `IStopSearchService` und `IDepartureService` anlegen | Offen | — |
| 12 | Interfaces | `IEfaProvider`, `IProviderOrchestrator`, `IRealtimeConsolidator` und `INrwRegionClassifier` anlegen | Offen | — |
| 13 | Interfaces | `ITransitCache`, `ITransitHttpGateway` und `ITransitDiagnostics` anlegen | Offen | — |
| 14 | Infrastruktur | DI-fähiges `HttpClient`-Gateway mit HTTPS-Prüfung und URL-Encoding implementieren | Offen | — |
| 15 | Infrastruktur | Cancellation, Timeout und begrenzte `RetryPolicy` implementieren | Offen | — |
| 16 | Infrastruktur | Bounded-TTL-`MemoryTransitCache` implementieren | Offen | — |
| 17 | Infrastruktur | Datensparsame `TransitDiagnostics` implementieren | Offen | — |
| 18 | Adapter | `DbRestProvider` für Locations und Nearby implementieren | Offen | — |
| 19 | Adapter | `DbRestProvider` für Journeys und Departures implementieren | Offen | — |
| 20 | Adapter | `DbRestResponseMapper` für dokumentierte db-rest-JSON-Felder implementieren | Offen | — |
| 21 | Adapter | `EfaProvider` vollständig für Suche, Nearby, Routing und RapidJSON-Abfahrten/Echtzeit implementieren | Offen | — |
| 22 | Adapter | `EfaResponseMapper` anhand Schema, Fixture und Liveantwort implementieren | Offen | — |
| 23 | Adapter | db-rest-503 dokumentieren und EFA-Fallback für Search/Nearby/Trip deterministisch ausführen | Offen | — |
| 24 | Logik | `NrwRegionClassifier` mit echter Grenzgeometrie implementieren | Offen | — |
| 25 | Logik | `StopSearchService` implementieren | Offen | — |
| 26 | Logik | `RoutingService` mit Normalisierung und Fallback implementieren | Offen | — |
| 27 | Logik | `DepartureService` implementieren | Offen | — |
| 28 | Logik | `ProviderOrchestrator` mit NRW-Priorität und bundesweitem EFA-Fallback implementieren | Offen | — |
| 29 | Logik | Cross-Provider-Konsolidierung über DHID bzw. eindeutige räumliche/Namens-/Linien-/Betreiber-/Richtungs-/Sollzeit-Kandidaten implementieren | Offen | — |
| 30 | Integration | Fachliche Services und Provider in `MauiProgram` registrieren | Offen | — |
| 31 | Integration | Entwicklungsoption für alternativen EFA-Endpunkt transparent konfigurieren | Offen | — |
| 32 | Tests | Modell- und Validierungstests anlegen | Offen | — |
| 33 | Tests | db-rest-Mapper- und Zeitnormalisierungstests anlegen | Offen | — |
| 34 | Tests | EFA-Mapper-, Warnungs- und Search/Nearby/Trip-Fallbacktests anlegen | Offen | — |
| 35 | Tests | NRW-Klassifikations- und Providerprioritätstests anlegen | Offen | — |
| 36 | Tests | Cross-Provider-Konsolidierungs- und Mehrdeutigkeits-Tests anlegen | Offen | — |
| 37 | Tests | Fallback-, Fehler- und partielle-Antwort-Tests anlegen | Offen | — |
| 38 | Tests | Cache-, Cancellation-, Timeout- und Retrytests mit konkreten Grenzwerten anlegen | Offen | — |
| 39 | Tests | Datensparsame Diagnose- und Sicherheitsverhalten testen | Offen | — |
| 40 | Tests | Anonymisierte db-rest-/EFA-Fixtures, db-rest-503 und Live-Proben dokumentieren | Offen | — |
| 41 | Tests | Core-Coverage auf mindestens 70 % Zeilenabdeckung bringen | Offen | — |
| 42 | Dokumentation | Dauerhafte Erweiterungs- und Betriebsdokumentation unter `docs/help/fahrplanauskunft/` erstellen | Offen | — |
| 43 | Dokumentation | Abnahme der Provider-/Service-Erweiterungsgrenzen und späteren Sharing-/Push-Anbindung dokumentieren | Offen | — |
| 44 | Qualität | Format- und Warnungen-als-Fehler-Prüfungen ausführen | Offen | — |
| 45 | Qualität | Bestehende Windows-Test-/Releasekonfiguration verifizieren | Offen | — |
