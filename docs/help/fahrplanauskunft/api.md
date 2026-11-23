← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — API

## Übersicht

Die öffentliche Core-Schnittstelle besteht aus asynchronen Services und Providerverträgen. Alle Methoden akzeptieren ein `CancellationToken` und liefern `ProviderResult<T>` mit Daten, Quelle, Warnungen, Fehlercode, Abrufzeit sowie Fallback-/Stale-Status.

## Authentifizierung und Transport

Die Entwicklungsendpunkte benötigen gemäß den vorhandenen Proben keinen Schlüssel. Der Gateway akzeptiert ausschließlich HTTPS, deaktiviert automatische Redirects und begrenzt Antwortgröße, Timeout und Wiederholungen. Produktive Zugangsdaten oder Freigaben sind providerseitig separat zu klären und werden nicht protokolliert.

## Core-Methoden

| Vertrag | Methode | Zweck |
|---------|---------|-------|
| `IStopSearchService` | `SearchAsync(string, CancellationToken)` | Adressen und Haltestellen anhand von Text suchen. |
| `IStopSearchService` | `NearbyAsync(GeoCoordinate, CancellationToken)` | Nahe Haltestellen zu WGS84-Koordinaten suchen. |
| `IRoutingService` | `RouteAsync(Address, Address, DateTimeOffset, CancellationToken)` | Kommende Verbindungen zwischen zwei Orten abrufen. |
| `IDepartureService` | `DeparturesAsync(Stop, DateTimeOffset, CancellationToken)` | Abfahrten einer Haltestelle abrufen. |
| `ITransitProvider` | `SearchAsync`, `NearbyAsync`, `RouteAsync`, `DeparturesAsync` | Providerzugriff für alle vier Datenoperationen. |
| `IEfaProvider` | geerbte `ITransitProvider`-Methoden | EFA-Adapter einschließlich Rückfallversorgung. |

## Externe Routen

Der `DbRestProvider` verwendet die dokumentierten Routen `GET /locations`, `GET /locations/nearby`, `GET /journeys` und `GET /stops/{id}/departures`. Der `EfaProvider` verwendet die in den Proben dokumentierten EFA-Routen `XML_STOPFINDER_REQUEST`, `XML_COORD_REQUEST`, `XML_TRIP_REQUEST2` und `XML_DM_REQUEST` mit RapidJSON.

## Rückgaben und Fehler

Providerfelder werden in `Address`, `Stop`, `NearbyStopResult`, `Journey`, `JourneyLeg`, `StopEvent`, `RealtimeStatus`, `Line`, `Operator` und `GeoGeometry` normalisiert. Unbekannte oder nicht gelieferte Werte bleiben leer. HTTP-/Transportfehler, Warnungen, mehrdeutige Auflösungen und `region-unknown` werden in `ProviderResult` sichtbar gemacht; bei zulässigem Alter kann ein Ergebnis als `IsStale` und `IsFallback` markiert werden.

Die vollständigen externen URLs, Feldbeispiele, Antwortgrenzen und echten Ergebnisse stehen in [provider-probes.md](provider-probes.md). Die gespeicherten JSON-Dateien unter `fixtures/` sind Testeingaben und keine neue Livegarantie.
