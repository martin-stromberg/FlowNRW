# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine.

## Prüfung der Korrekturen

Beide technischen Befunde aus `review-code.1.md` sind behoben:

- Die Transport-Catch-Blöcke im HTTP-Gateway setzen den zuvor empfangenen Status zurück. Body-Timeout und Body-IOException nach HTTP 200 gelangen damit korrekt in die begrenzte Retryentscheidung und behalten `timeout` beziehungsweise `transport` als Endfehler. Die zwei neuen Regressionen prüfen Fehlercode und zwei Versuche bei einem erlaubten Retry.
- Validierungsfehler durchlaufen jetzt in Search-, Routing- und Departure-Service ebenfalls `Latest`. Die drei neuen Regressionen beginnen eine offene gültige Anfrage, liefern anschließend ungültige Eingaben und prüfen, dass die alte Anfrage nach ihrem Abschluss abgebrochen zurückkehrt.

Die zusammenhängende Änderung `ProviderOrchestrator.ResolveRegion` wurde einschließlich ihrer Aufrufer und vier Regressionen gelesen. Auflösung erfolgt vor der Providerwahl, bereits vorhandene Koordinaten bleiben maßgeblich, ausgewählte Stopidentitäten müssen übereinstimmen, und nicht eindeutig auflösbare Regionen erhalten eine Warnung. Cancellation sowie Suchcache und geteilte Anfragen bleiben über die vorhandenen Aufrufpfade erhalten. Kein zusätzlicher konkreter technischer Befund im geprüften Korrekturumfang.

## Prüfnachweise

Diff-Basis unverändert: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Git-Status einschließlich ungetrackter Quellen berücksichtigt. Dies ist die gezielte Nachprüfung der Korrekturrunde; die vollständige Erstprüfung und deren Dateiliste sind in `review-code.1.md` erhalten.

Gelesener Nachweis: `docs/help/fahrplanauskunft/verification/iteration2-checks.md`, mit neun Rot-Grün-Regressionen, 98 erfolgreichen Gesamttests, 541/552 Core-Zeilen (98,00 %), Windows-Build ohne Warnungen/Fehler sowie erfolgreichen Format- und XML-Dokumentationsprüfungen. Diese Ausführungen stammen aus der Korrekturrunde; keine erneuten Tests, Builds oder Liveproben durch diesen Prüfagenten.

## Geprüfte Dateien

- `FlowNRW.Core/Transit/TransitHttpGateway.cs`
- `FlowNRW.Core/Transit/StopSearchService.cs`
- `FlowNRW.Core/Transit/RoutingService.cs`
- `FlowNRW.Core/Transit/DepartureService.cs`
- `FlowNRW.Core/Transit/ProviderOrchestrator.cs`
- `FlowNRW.Tests/TransitHttpGatewayTests_BodyFailures.cs`
- `FlowNRW.Tests/TransitServiceTests_InvalidReplacement.cs`
- `FlowNRW.Tests/ProviderOrchestratorTests_RegionResolution.cs`
- `FlowNRW.Tests/TransitTestSupport.cs`
