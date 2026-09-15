# Modelle

Die für Schritt 2 relevanten normalisierten Modelle liegen unter `FlowNRW.Core/Transit`.

| Typ | Relevante Eigenschaften / Zweck |
|---|---|
| `Address` | `Name`, optionale `Coordinate` und optionale fachliche `Stop`-Identität; Such- und Routing-Endpunkt. |
| `GeoCoordinate` | Validierte finite WGS84-`Latitude`/`Longitude`; Konstruktor weist Werte außerhalb -90..90 bzw. -180..180 zurück. |
| `Stop` | `Id`, `Name`, `Source`, optional `Dhid` und `Coordinate`; Provider-/Trefferidentität. |
| `Journey` | Optionale `Id`, geordnete `Legs`, `Transfers`. |
| `JourneyLeg` | `Departure`, `Arrival`, optionale `Line`, `Walking`, `Geometry`. |
| `StopEvent` | `Identity`, optional `PlannedTime`, `Realtime`, optionale `Line`. |
| `TripIdentity` | Provider-/Trip-/Shared-IDs, Stop, Linie, Betreiber, Richtung und Sollzeit; Grundlage konservativer Zuordnung. |
| `RealtimeStatus` | Optionale Istzeit, Verspätung, Ausfall, Bahnsteige sowie Quelle und Abrufzeitpunkt; `null` bedeutet unbekannt. |
| `ProviderResult<T>` | Items, Quelle, Abrufzeit, Warn-/Fehlercodes, Fallback-/Stale-Markierung und `HasData`; erfindet keine Werte. |

Ergänzend liefern `Line`, `Operator`, `Transfer`, `WalkingSegment` und `GeoGeometry` die in der Anforderung genannten Linien-, Betreiber-, Umstiegs-, Fußweg- und Geometriedaten.
