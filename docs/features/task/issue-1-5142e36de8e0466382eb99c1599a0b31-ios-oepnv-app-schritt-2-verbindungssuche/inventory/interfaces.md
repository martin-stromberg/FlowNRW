# Interfaces

| Contract | Datei | Zweck |
|---|---|---|
| `IRoutingService` | `FlowNRW.Core/Transit/IRoutingService.cs` | `RouteAsync(Address, Address, DateTimeOffset, CancellationToken)` für abbrechbare Verbindungssuche. |
| `IStopSearchService` | `FlowNRW.Core/Transit/IStopSearchService.cs` | `SearchAsync(string, CancellationToken)` und `NearbyAsync(GeoCoordinate, CancellationToken)` für Trefferauflösung bzw. nahe Haltestellen. |
| `IProviderOrchestrator` | `FlowNRW.Core/Transit/IProviderOrchestrator.cs` | Provider-Contract für zentralisierte Auswahl, Fallback und Konsolidierung. |
| `ITransitProvider` | `FlowNRW.Core/Transit/ITransitProvider.cs` | Gemeinsame Suche, Routing, Abfahrten und Nearby-Operationen samt `Name`. |
| `IEfaProvider` | `FlowNRW.Core/Transit/IEfaProvider.cs` | Regionaler EFA-Provider als `ITransitProvider`. |
| `INrwRegionClassifier` | `FlowNRW.Core/Transit/INrwRegionClassifier.cs` | `IsInNrw(GeoCoordinate?)`. |
| `IRealtimeConsolidator` | `FlowNRW.Core/Transit/IRealtimeConsolidator.cs` | Eindeutige Ereigniskonsolidierung. |
| `ITransitCache` | `FlowNRW.Core/Transit/ITransitCache.cs` | Typisierter Cache mit explizitem Stale-Lesen. |
| `ITransitHttpGateway` | `FlowNRW.Core/Transit/ITransitHttpGateway.cs` | Begrenzter JSON-Abruf mit sicheren Diagnosen. |

`IDepartureService` ergänzt die vorhandene Abfahrtsfunktion; für die manuelle Verbindungssuche ist `IRoutingService` der direkte Einstieg.
