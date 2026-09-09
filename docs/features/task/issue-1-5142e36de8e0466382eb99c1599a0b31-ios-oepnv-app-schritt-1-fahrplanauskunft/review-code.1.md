# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### TransitHttpGateway.cs (TransitHttpGateway)

- **Fehlerbehandlung (P2)** — In `GetAsync` bleibt `status` nach erfolgreichen Response-Headern auf HTTP 200 gesetzt, wenn `ReadAsStreamAsync` oder `ReadAsync` danach einen Timeout bzw. eine `IOException` auslöst. Die Catch-Blöcke setzen lediglich `code`. `ShouldRetry(status, ...)` lehnt deshalb den Retry ab, und der abschließende Fehler lautet `http-200` statt `timeout` oder `transport`. Reproduktion: Handler liefert sofort 200 mit einem Stream, dessen Read bis zur Token-Cancellation wartet; `MaxRetries = 1`. Aktuell nur ein Versuch und Fehler `http-200`. Derselbe Fehler tritt bei einem während des Body-Lesens abbrechenden Stream auf.

  Empfehlung: Transport-/Timeoutfehler unabhängig vom zuvor empfangenen erfolgreichen HTTP-Status klassifizieren und an die Retryentscheidung übergeben; den konkreten Fehlercode erhalten. Je einen Regressionstest für Body-Timeout und Body-IOException nach erfolgreichen Headern ergänzen (Versuchsanzahl und Fehlercode prüfen).

### StopSearchService.cs, RoutingService.cs, DepartureService.cs

- **Cancellation / veraltete Ergebnisse (P2)** — Die Eingabevalidierung kehrt vor `Latest` zurück. Eine neuere ungültige Eingabe löst daher keine Cancellation der laufenden älteren Anfrage aus. Reproduktion mit dem vorhandenen `TransitTestProvider`: `SearchAsync("Berlin")` über eine noch offene TaskCompletionSource starten; anschließend `SearchAsync("")` aufrufen; danach die erste Quelle erfolgreich abschließen. Die alte Suche kehrt erfolgreich mit Berlin zurück, obwohl die neueste Eingabe bereits leer ist. Der vorhandene Test `SearchAsync_NewerRequest_SuppressesOlderResult` deckt nur zwei gültige Eingaben ab. Entsprechendes passiert beim Leeren eines Routenendpunkts bzw. einer ungültigen Haltestelle.

  Empfehlung: Jede neue nicht bereits vom Aufrufer abgebrochene Serviceanfrage muss die vorige Anfrage ablösen, auch wenn ihre eigene Validierung ein Fehlerergebnis liefert. Validierung unter dieselbe Latest-Request-Verwaltung ziehen oder die Ablösung davor sicher ausführen. Regressionen für gültige laufende Anfrage gefolgt von ungültiger neuer Eingabe ergänzen und sicherstellen, dass die alte Anfrage keine erfolgreichen Ergebnisse mehr liefert.

## Prüfnachweise und Abgrenzung

Explizite Diff-Basis: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Geänderte Dateien wurden über Git-Diff und Git-Status sowie die vollständige Dateiliste der ungetrackten Transit-Quellen ermittelt. Die nachfolgend aufgeführten C#-Dateien wurden vollständig gelesen. Die Befunde ergeben sich unmittelbar aus den beschriebenen Kontrollflüssen; in diesem Review wurden keine neuen Tests oder Liveproben ausgeführt. Die vorhandenen Tests prüfen Header-Timeouts und gültige Folgeanfragen, nicht die oben genannten Randfälle. Die bekannte fachliche Frage zur NRW-Priorität bei unaufgelösten Namen/IDs bleibt im separaten fachlichen Review.

## Geprüfte Dateien

- `FlowNRW.Core/Transit/Address.cs`
- `FlowNRW.Core/Transit/DbRestProvider.cs`
- `FlowNRW.Core/Transit/DbRestResponseMapper.cs`
- `FlowNRW.Core/Transit/DepartureService.cs`
- `FlowNRW.Core/Transit/EfaProvider.cs`
- `FlowNRW.Core/Transit/EfaResponseMapper.cs`
- `FlowNRW.Core/Transit/GeoCoordinate.cs`
- `FlowNRW.Core/Transit/GeoGeometry.cs`
- `FlowNRW.Core/Transit/IDepartureService.cs`
- `FlowNRW.Core/Transit/IEfaProvider.cs`
- `FlowNRW.Core/Transit/INrwRegionClassifier.cs`
- `FlowNRW.Core/Transit/IProviderOrchestrator.cs`
- `FlowNRW.Core/Transit/IRealtimeConsolidator.cs`
- `FlowNRW.Core/Transit/IRoutingService.cs`
- `FlowNRW.Core/Transit/IStopSearchService.cs`
- `FlowNRW.Core/Transit/ITransitCache.cs`
- `FlowNRW.Core/Transit/ITransitDiagnostics.cs`
- `FlowNRW.Core/Transit/ITransitHttpGateway.cs`
- `FlowNRW.Core/Transit/ITransitProvider.cs`
- `FlowNRW.Core/Transit/Journey.cs`
- `FlowNRW.Core/Transit/JourneyLeg.cs`
- `FlowNRW.Core/Transit/Line.cs`
- `FlowNRW.Core/Transit/MemoryTransitCache.cs`
- `FlowNRW.Core/Transit/NearbyStopResult.cs`
- `FlowNRW.Core/Transit/NrwRegionClassifier.cs`
- `FlowNRW.Core/Transit/Operator.cs`
- `FlowNRW.Core/Transit/ProviderOrchestrator.cs`
- `FlowNRW.Core/Transit/ProviderResult.cs`
- `FlowNRW.Core/Transit/RealtimeConsolidator.cs`
- `FlowNRW.Core/Transit/RealtimeStatus.cs`
- `FlowNRW.Core/Transit/RetryPolicy.cs`
- `FlowNRW.Core/Transit/RoutingService.cs`
- `FlowNRW.Core/Transit/Stop.cs`
- `FlowNRW.Core/Transit/StopEvent.cs`
- `FlowNRW.Core/Transit/StopSearchService.cs`
- `FlowNRW.Core/Transit/Transfer.cs`
- `FlowNRW.Core/Transit/TransitCacheOptions.cs`
- `FlowNRW.Core/Transit/TransitDiagnostics.cs`
- `FlowNRW.Core/Transit/TransitHttpGateway.cs`
- `FlowNRW.Core/Transit/TransitJson.cs`
- `FlowNRW.Core/Transit/TransitProviderOptions.cs`
- `FlowNRW.Core/Transit/TripIdentity.cs`
- `FlowNRW.Core/Transit/WalkingSegment.cs`
- `FlowNRW.Tests/Transit/AdapterGateway.cs`
- `FlowNRW.Tests/Transit/DbRestProviderTests_Requests.cs`
- `FlowNRW.Tests/Transit/DbRestResponseMapperTests_Normalization.cs`
- `FlowNRW.Tests/Transit/EfaProviderTests_Requests.cs`
- `FlowNRW.Tests/Transit/EfaResponseMapperTests_Journeys.cs`
- `FlowNRW.Tests/Transit/EfaResponseMapperTests_Locations.cs`
- `FlowNRW.Tests/MemoryTransitCacheTests_Lifetime.cs`
- `FlowNRW.Tests/NrwRegionClassifierTests_Boundary.cs`
- `FlowNRW.Tests/ProviderOrchestratorTests_Concurrency.cs`
- `FlowNRW.Tests/ProviderOrchestratorTests_Fallback.cs`
- `FlowNRW.Tests/RealtimeConsolidatorTests_Identity.cs`
- `FlowNRW.Tests/RetryPolicyTests_Bounds.cs`
- `FlowNRW.Tests/RoutingServiceTests_NextJourneys.cs`
- `FlowNRW.Tests/TransitHttpGatewayTests_Execution.cs`
- `FlowNRW.Tests/TransitHttpGatewayTests_Security.cs`
- `FlowNRW.Tests/TransitModelTests_Validation.cs`
- `FlowNRW.Tests/TransitServiceTests_Validation.cs`
- `FlowNRW.Tests/TransitTestSupport.cs`
- `FlowNRW/MauiProgram.cs`
- `FlowNRW.Core/FlowNRW.Core.csproj`
