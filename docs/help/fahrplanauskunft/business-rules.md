← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — Fachliche Regeln

## Regionale NRW-Priorität

**Bedingung:** Mindestens ein aufgelöster Start- oder Zielpunkt liegt im amtlichen NRW-MultiPolygon.

**Verhalten:** Der EFA-Provider wird bevorzugt; der zweite Provider ergänzt oder übernimmt bei leerer, fehlerhafter oder warnender Antwort. Eine grobe Boundingbox und der VRR-Raum gelten nicht als NRW-Grenze.

**Umsetzung:** `NrwRegionClassifier.IsInNrw`, `ProviderOrchestrator.RouteAsync` und `ProviderOrchestrator.DeparturesAsync`.

## Konservative Echtzeitzuordnung

Eine DHID identifiziert nur die Haltestelle. Eine Fahrt darf zusätzlich nur über eine eindeutige gemeinsame Fahrtkennung oder über Linie, Betreiber, Richtung und Sollzeit zusammengeführt werden. Mehrere Kandidaten, widersprüchliche Fahrtkennungen oder fehlende Pflichtmerkmale führen zu keinem Merge.

**Umsetzung:** `RealtimeConsolidator.Consolidate`.

## Unbekannte Werte

Nicht gelieferte Istzeiten, Verspätungen, Ausfälle und Steige/Gleise bleiben unbekannt. Pünktlichkeit wird nicht aus dem Fehlen eines Echtzeitwerts abgeleitet. Warnungen und Datenalter werden an die aufrufende Schicht weitergegeben.

## Zeit und Tageswechsel

Zeitwerte werden mit ihrem Offset verarbeitet. Ergebnisse vor dem angefragten Zeitpunkt werden herausgefiltert; Tageswechsel bleiben durch `DateTimeOffset` erhalten. Zeitwerte ohne belastbare Zeitzone werden nicht stillschweigend mit der Maschinenzeitzone interpretiert.

## Rückfälle und Stale-Daten

Ein Providerfehler oder eine leere Antwort aktiviert den zweiten Provider. Ein gespeicherter Treffer darf nur innerhalb seiner TTL verwendet werden; ein älterer Echtzeitwert wird als `IsStale`/`IsFallback` markiert und nach fünf Minuten verworfen.
