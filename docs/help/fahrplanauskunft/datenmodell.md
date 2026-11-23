← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — Datenmodell

## Fachliche Modelle

| Modell | Inhalt |
|--------|--------|
| `Address` | Name, optionale `GeoCoordinate` und optionaler `Stop`. |
| `GeoCoordinate` | Endliche WGS84-Breite und -Länge. |
| `Stop` | Provider-ID, Name, Quelle, optionale DHID und Koordinate. |
| `NearbyStopResult` | `Stop` und optionale Entfernung in Metern. |
| `Journey` | Metadaten und eine Folge von Teilstrecken (`Legs`). Die Zeit- und Geometriedaten liegen an den jeweiligen Teilstrecken. |
| `JourneyLeg` | Fahrtabschnitt oder Fußweg mit Abfahrts-/Ankunftsereignis und optionaler Geometrie. |
| `StopEvent` | `TripIdentity` und `RealtimeStatus`. |
| `TripIdentity` | Quelle, providerinterne Fahrtkennung, optionale geteilte Fahrtkennung, Stop, Linie, Betreiber, Richtung und Sollzeit. |
| `RealtimeStatus` | Istzeit, Verspätung, Ausfall, Steig/Gleis, Quelle und Abrufzeit. |
| `ProviderResult<T>` | Ergebnisliste, Quelle, Warnungen, Fehlercode, Abrufzeit sowie `IsFallback`- und `IsStale`-Markierung. |

## Beziehungen

Eine `Journey` enthält mehrere `JourneyLeg`-Objekte. Ein `JourneyLeg` referenziert Abfahrts- und Ankunfts-`StopEvent`-Objekte. `RealtimeConsolidator` ergänzt ein Ereignis nur, wenn Stop- und Fahrtidentität eindeutig übereinstimmen. Es gibt keine Datenbankentitäten und keine Migration; der Cache liegt ausschließlich im Arbeitsspeicher.
