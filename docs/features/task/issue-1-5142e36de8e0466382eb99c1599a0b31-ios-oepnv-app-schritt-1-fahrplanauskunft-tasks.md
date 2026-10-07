# Tasks: konfigurierbare bundesweite und regionale Fahrplanauskunft

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Datenmodell | `Address` anlegen | Erledigt | DbRestResponseMapperTests_Normalization.Search_MixedLocations_PreservesAddressAndStop |
| 2 | Datenmodell | `GeoCoordinate` mit Bereichsvalidierung anlegen | Erledigt | TransitModelTests_Validation.GeoCoordinate_InvalidValues_Throws |
| 3 | Datenmodell | `Stop` und `NearbyStopResult` anlegen | Erledigt | EfaResponseMapperTests_Locations.Nearby_LiveFixture_PreservesDistance |
| 4 | Datenmodell | `Journey`, `JourneyLeg`, `WalkingSegment` und `Transfer` anlegen | Erledigt | EfaResponseMapperTests_Journeys.Journeys_WalkBetweenTransitLegs_ProducesTransfer |
| 5 | Datenmodell | `Line`, `Operator`, `GeoGeometry` und `TripIdentity` anlegen | Erledigt | EfaResponseMapperTests_Journeys.Journeys_BerlinHamburgFixture_PreservesTransportAndGeometry |
| 6 | Datenmodell | `StopEvent` und `RealtimeStatus` anlegen | Erledigt | EfaResponseMapperTests_Journeys.Departures_NrwFixture_NormalizesParentStopAndRealtime |
| 7 | Datenmodell | `ProviderResult<T>` und gemeinsame Quellen-/Datenalterswerte anlegen | Erledigt | MemoryTransitCacheTests_Lifetime.Get_ExpiredRealtime_ReturnsMarkedStaleOnlyWithinLimit |
| 8 | Konfiguration | `TransitProviderOptions` und sichere Endpointoptionen anlegen | Erledigt | TransitModelTests_Validation.ProviderOptions_UnsafeEndpoint_RejectsWithoutEcho |
| 9 | Konfiguration | `TransitCacheOptions` und HTTP-Timeout-/Retryoptionen anlegen | Erledigt | TransitHttpGatewayTests_Execution.GetAsync_Timeout_ReturnsSafeError |
| 10 | Konfiguration | Offizielle GeoBasis-NRW-Verwaltungsgrenze als lizenzkonformes Polygon beschaffen, versionieren und bei Fehlen fail-fast behandeln | Erledigt | NrwRegionClassifierTests_Boundary.IsInNrw_UsesActualStateBoundary |
| 11 | Interfaces | `ITransitProvider`, `IRoutingService`, `IStopSearchService` und `IDepartureService` anlegen | Erledigt | TransitServiceTests_Validation.SearchAsync_EmptySearch_ReturnsValidationError |
| 12 | Interfaces | `IEfaProvider`, `IProviderOrchestrator`, `IRealtimeConsolidator` und `INrwRegionClassifier` anlegen | Erledigt | ProviderOrchestratorTests_Fallback.RouteAsync_EitherPointInNrw_PrioritizesRegional |
| 13 | Interfaces | `ITransitCache`, `ITransitHttpGateway` und `ITransitDiagnostics` anlegen | Erledigt | TransitHttpGatewayTests_Execution.GetAsync_Success_ReturnsPayload |
| 14 | Infrastruktur | DI-fähiges `HttpClient`-Gateway mit HTTPS-Prüfung und URL-Encoding implementieren | Erledigt | TransitHttpGatewayTests_Security.GetAsync_UnsafeUrl_DoesNotSend |
| 15 | Infrastruktur | Cancellation, Timeout und begrenzte `RetryPolicy` implementieren | Erledigt | TransitHttpGatewayTests_Execution; TransitHttpGatewayTests_BodyFailures |
| 16 | Infrastruktur | Bounded-TTL-`MemoryTransitCache` implementieren | Erledigt | MemoryTransitCacheTests_Lifetime.Set_AtCapacity_EvictsOldestEntry |
| 17 | Infrastruktur | Datensparsame `TransitDiagnostics` implementieren | Erledigt | TransitHttpGatewayTests_Security.Record_SensitiveInputs_UsesOnlyAllowlistedMetadata |
| 18 | Adapter | `DbRestProvider` für Locations und Nearby implementieren | Erledigt | DbRestProviderTests_Requests.NearbyAsync_Coordinate_MapsDistance |
| 19 | Adapter | `DbRestProvider` für Journeys und Departures implementieren | Erledigt | DbRestProviderTests_Requests.RouteAsync_Names_ResolvesStops; DeparturesAsync_OwnStop_UsesEscapedId |
| 20 | Adapter | `DbRestResponseMapper` für dokumentierte db-rest-JSON-Felder implementieren | Erledigt | DbRestResponseMapperTests_Normalization |
| 21 | Adapter | `EfaProvider` vollständig für Suche, Nearby, Routing und RapidJSON-Abfahrten/Echtzeit implementieren | Erledigt | EfaProviderTests_Requests |
| 22 | Adapter | `EfaResponseMapper` anhand Schema, Fixture und Liveantwort implementieren | Erledigt | EfaResponseMapperTests_Journeys; EfaResponseMapperTests_Locations |
| 23 | Adapter | db-rest-503 dokumentieren und EFA-Fallback für Search/Nearby/Trip deterministisch ausführen | Erledigt | ProviderOrchestratorTests_Fallback.SearchAsync_UnusablePrimary_ReturnsMarkedFallback; service-live-2026-09-08.jsonl |
| 24 | Logik | `NrwRegionClassifier` mit echter Grenzgeometrie implementieren | Erledigt | NrwRegionClassifierTests_Boundary |
| 25 | Logik | `StopSearchService` implementieren | Erledigt | TransitServiceTests_Validation; TransitServiceTests_InvalidReplacement |
| 26 | Logik | `RoutingService` mit Normalisierung und Fallback implementieren | Erledigt | RoutingServiceTests_NextJourneys; ProviderOrchestratorTests_RegionResolution |
| 27 | Logik | `DepartureService` implementieren | Erledigt | TransitServiceTests_InvalidReplacement; ProviderOrchestratorTests_RegionResolution.DeparturesAsync_UnlocatedNrwStop_UsesRegionalProvider |
| 28 | Logik | `ProviderOrchestrator` mit NRW-Priorität und bundesweitem EFA-Fallback implementieren | Erledigt | ProviderOrchestratorTests_RegionResolution; ProviderOrchestratorTests_Fallback |
| 29 | Logik | Cross-Provider-Konsolidierung über DHID bzw. eindeutige räumliche/Namens-/Linien-/Betreiber-/Richtungs-/Sollzeit-Kandidaten implementieren | Erledigt | RealtimeConsolidatorTests_Identity |
| 30 | Integration | Fachliche Services und Provider in `MauiProgram` registrieren | Erledigt | Kein direkter Test; dokumentierter Windows-Solution-Build und CreateMauiApp-Quellprüfung |
| 31 | Integration | Entwicklungsoption für alternativen EFA-Endpunkt transparent konfigurieren | Erledigt | EfaProviderTests_Requests.SearchAsync_PrimaryUnavailable_CallsFallbackEndpoint |
| 32 | Tests | Modell- und Validierungstests anlegen | Erledigt | TransitModelTests_Validation; TransitServiceTests_Validation |
| 33 | Tests | db-rest-Mapper- und Zeitnormalisierungstests anlegen | Erledigt | DbRestResponseMapperTests_Normalization |
| 34 | Tests | EFA-Mapper-, Warnungs- und Search/Nearby/Trip-Fallbacktests anlegen | Erledigt | EfaResponseMapperTests_Journeys; EfaResponseMapperTests_Locations; ProviderOrchestratorTests_Fallback |
| 35 | Tests | NRW-Klassifikations- und Providerprioritätstests anlegen | Erledigt | NrwRegionClassifierTests_Boundary; ProviderOrchestratorTests_RegionResolution |
| 36 | Tests | Cross-Provider-Konsolidierungs- und Mehrdeutigkeits-Tests anlegen | Erledigt | RealtimeConsolidatorTests_Identity |
| 37 | Tests | Fallback-, Fehler- und partielle-Antwort-Tests anlegen | Erledigt | ProviderOrchestratorTests_Fallback |
| 38 | Tests | Cache-, Cancellation-, Timeout- und Retrytests mit konkreten Grenzwerten anlegen | Erledigt | MemoryTransitCacheTests_Lifetime; TransitHttpGatewayTests_Execution; RetryPolicyTests_Bounds; ProviderOrchestratorTests_Concurrency |
| 39 | Tests | Datensparsame Diagnose- und Sicherheitsverhalten testen | Erledigt | TransitHttpGatewayTests_Security |
| 40 | Tests | Anonymisierte db-rest-/EFA-Fixtures, db-rest-503 und Live-Proben dokumentieren | Erledigt | EfaResponseMapperTests_Journeys; provider-probes.md; service-live-2026-09-08.jsonl |
| 41 | Tests | Core-Coverage auf mindestens 70 % Zeilenabdeckung bringen | Erledigt | verification/coverage-iteration2.cobertura.xml: 541/552 = 98,00 % |
| 42 | Dokumentation | Dauerhafte Erweiterungs- und Betriebsdokumentation unter `docs/help/fahrplanauskunft/` erstellen | Erledigt | Unabhängige Dokumentationsabnahme in review.md; API/Options/MAUI-DI mit architektur.md, datenmodell.md und installation.md abgeglichen |
| 43 | Dokumentation | Abnahme der Provider-/Service-Erweiterungsgrenzen und späteren Sharing-/Push-Anbindung dokumentieren | Erledigt | Unabhängige Dokumentationsabnahme in review.md; API/Options/MAUI-DI mit architektur.md, datenmodell.md und installation.md abgeglichen |
| 44 | Qualität | Format- und Warnungen-als-Fehler-Prüfungen ausführen | Erledigt | verification/iteration2-checks.md: Format, XML-Hook, Warnungen als Fehler |
| 45 | Qualität | Bestehende Windows-Test-/Releasekonfiguration verifizieren | Erledigt | verification/iteration2-checks.md: Windows-Build 0 Warnungen/0 Fehler |
| 46 | Logik/Tests | NRW-Orte ohne mitgelieferte Koordinaten vor Providerwahl eindeutig auflösen bzw. unbekannte Zuordnung ausdrücklich kennzeichnen; Routing-/Abfahrtsregression ergänzen | Erledigt | ProviderOrchestratorTests_RegionResolution (4 Regressionen) |
