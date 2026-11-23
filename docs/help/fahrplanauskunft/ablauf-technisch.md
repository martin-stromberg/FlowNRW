← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — Technischer Ablauf

## Ablauf

1. `MauiProgram.CreateMauiApp` liest und validiert `TransitProviderOptions` und `TransitCacheOptions` und registriert die Services.
2. `StopSearchService`, `RoutingService` oder `DepartureService` validiert die Eingabe und übergibt sie mit einem `CancellationToken` an `ProviderOrchestrator`.
3. Der Orchestrator löst fehlende Koordinaten über `SearchAsync` auf und klassifiziert NRW über `NrwRegionClassifier`.
4. Der ausgewählte `DbRestProvider` oder `EfaProvider` ruft den passenden externen Dienst über `TransitHttpGateway.GetAsync` ab.
5. Der jeweilige Mapper normalisiert JSON/RapidJSON in die Core-Modelle.
6. `ProviderOrchestrator` teilt identische laufende Anfragen, nutzt `MemoryTransitCache`, ruft bei Bedarf den zweiten Provider auf und markiert Fallback-/Stale-Ergebnisse.
7. `RealtimeConsolidator` ergänzt Echtzeit nur bei eindeutiger Stop- und Fahrtzuordnung.
8. Das `ProviderResult<T>` wird mit Quelle, Warnungen, Abrufzeit und Fehlerstatus an den aufrufenden Service zurückgegeben.

## Fehlerbehandlung

Ungültige Eingaben werden vor dem Netzwerkzugriff abgewiesen. Timeout, Transportfehler, HTTP-Fehler, Providerwarnungen und unaufgelöste Regionen werden als technische bzw. fachliche Statuswerte zurückgegeben. Cancellation wird weitergereicht und beendet überholte Ergebnisse. Diagnose schreibt nur bereinigte technische Metadaten.
