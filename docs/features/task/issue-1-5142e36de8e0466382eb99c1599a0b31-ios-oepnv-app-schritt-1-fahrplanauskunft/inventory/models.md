# Relevante Modelle

- `FlowNRW.Core/Transit/Journey.cs` und `JourneyLeg.cs`: Eine Journey enthält Legs; zeitliche und räumliche Detailwerte liegen an den Legs.
- `FlowNRW.Core/Transit/StopEvent.cs`: Abfahrt/Ankunft mit geplanter Zeit, Echtzeitstatus, Linie und `TripIdentity`.
- `FlowNRW.Core/Transit/TripIdentity.cs`: Quelle, Fahrt-IDs, DHID/Stop, Linie, Betreiber, Richtung und Sollzeit für die konservative Zuordnung.
- `FlowNRW.Core/Transit/ProviderResult.cs`: Ergebnisliste, Quelle, Warnungen, Fehler-, Fallback- und Stale-Status.
- `FlowNRW.Core/Transit/TransitProviderOptions.cs`: Bestehendes `MaxResults` als vorgesehene Obergrenze; keine neue Option erforderlich.

