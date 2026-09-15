# Relevante Logik

## ProviderOrchestrator

`FlowNRW.Core/Transit/ProviderOrchestrator.cs` implementiert `RouteAsync` und `DeparturesAsync`. Bei regionaler Priorität lädt `Execute` die regionale Primärantwort und die nationale Sekundärantwort. Warnungen oder fehlende Primärdaten bestimmen weiterhin `IsFallback` und die Warnungscodes. Bei zwei Datenmengen werden die privaten Mergefunktionen aufgerufen.

`MergeJourneys` sammelt derzeit sekundäre StopEvents und aktualisiert nur gleich positionierte regionale Legs. Die sekundäre Journey-Menge selbst wird nicht in das Ergebnis aufgenommen. `MergeEvents` ruft den Consolidator zweimal auf; damit werden passende regionale Werte aktualisiert, aber keine neuen sekundären Events erhalten.

## RealtimeConsolidator

`FlowNRW.Core/Transit/RealtimeConsolidator.cs` ordnet regionale und geplante Events über Stop- und Fahrtmerkmale konservativ zu. DHID, Provider-IDs, normalisierte Namen/Koordinaten sowie Linie, Betreiber und Richtung werden berücksichtigt; Zeitfenster und Eindeutigkeit verhindern mehrdeutige Zuordnung. Das Ergebnis iteriert jedoch nur über die geplante Eingabemenge. Fremde oder nicht eindeutig zuordenbare Fahrten werden deshalb nicht vermischt, zusätzliche eindeutige nationale Elemente aber auch nicht ergänzt.

Cache, Cancellation und bestehende Fallbacksemantik liegen außerhalb der Merge-Lücke und sind durch die vorhandene Orchestrator-Logik abgedeckt.

