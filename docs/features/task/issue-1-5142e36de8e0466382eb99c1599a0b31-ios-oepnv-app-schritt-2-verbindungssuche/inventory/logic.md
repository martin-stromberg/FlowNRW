# Logik

## `StopSearchService`
Datei: `FlowNRW.Core/Transit/StopSearchService.cs`

`SearchAsync` validiert nichtleere Suchtexte bis `MaxSearchLength` und ruft den Provider abbrechbar auf. `NearbyAsync` reicht Koordinaten weiter. Beide Operationen verwenden eine gemeinsame „neuere Anfrage verdrängt ältere Anfrage“-Logik.

## `RoutingService`
Datei: `FlowNRW.Core/Transit/RoutingService.cs`

`RouteAsync` validiert `Address`-Endpunkte, ruft den Orchestrator abbrechbar auf, entfernt Fahrten vor dem angefragten Zeitpunkt, sortiert nach tatsächlicher bzw. geplanter Abfahrt und Ankunft und begrenzt auf `MaxResults`. Ältere laufende Anfragen werden abgebrochen.

## `ProviderOrchestrator`
Datei: `FlowNRW.Core/Transit/ProviderOrchestrator.cs`

`SearchAsync`, `NearbyAsync`, `RouteAsync` und `DeparturesAsync` wählen den regionalen Provider bei NRW-Bezug. `ResolveRegion` löst nicht koordinierte Endpunkte zunächst über die Suche auf. `Run` nutzt begrenzten typisierten Cache und teilt identische laufende Abrufe. `Execute` ergänzt oder ersetzt bei Fallbackbedarf mit dem zweiten Provider und markiert Warnungen/Stale-Daten. `MergeJourneys` und `MergeEvents` führen nur eindeutig zuordenbare Ergebnisse zusammen; regionale Echtzeitwerte bleiben erhalten.

## Weitere beteiligte Logik

`DbRestProvider`, `EfaProvider` und die jeweiligen Response-Mapper normalisieren Providerantworten. `NrwRegionClassifier` klassifiziert Koordinaten anhand der eingebetteten NRW-Grenze. `RealtimeConsolidator` gleicht Ereignisse konservativ ab. `TransitHttpGateway`, `RetryPolicy`, `MemoryTransitCache` und `TransitDiagnostics` kapseln sichere HTTP-Abrufe, begrenzte Wiederholungen, Cache und datensparsame Diagnose.
