# Release Notes

## Important Notes Before Update

- The included `db.transport.rest` and EFA endpoints are development/data-source options; productive public use still requires provider-specific availability, terms and approvals.
- Step 1 delivers the platform-independent transit data core only; no new user interface or native iOS acceptance is included, with UI and iOS work following in Step 2.

## What's New

- Fixed missing national journeys and departures in partial regional results; merge uniquely matching trips, preserve regional realtime and apply the final sorted result limit.

- Added asynchronous, cancellable transit services for address/stop search, nearby stops, journeys and departures with normalized provider results.
- Added NRW-aware EFA prioritization, conservative realtime consolidation, transparent fallback/stale states, bounded memory caching and privacy-preserving diagnostics.
- Added configurable `db.transport.rest` and EFA adapters with HTTPS validation, bounded retries, response limits and documented live-probe boundaries.

## Wichtige Hinweise vor dem Update

- Die enthaltenen `db.transport.rest`- und EFA-Endpunkte sind Entwicklungs-/Datenquellenoptionen; für einen produktiven öffentlichen Betrieb müssen Verfügbarkeit, Nutzungsbedingungen und Freigaben des jeweiligen Anbieters geklärt sein.
- Schritt 1 liefert ausschließlich den plattformunabhängigen Fahrplandatenkern; eine neue Benutzeroberfläche oder native iOS-Abnahme ist nicht enthalten. UI und iOS folgen in Schritt 2.

## Neuerungen

- Fehlende bundesweite Verbindungen und Abfahrten bei regionalen Teilergebnissen ergänzt; eindeutige Fahrten zusammengeführt, regionale Echtzeit priorisiert und finale Ergebnismenge sortiert/begrenzt.

- Asynchrone, abbrechbare Fahrplandienste für Adress-/Haltestellensuche, nahe Haltestellen, Verbindungen und Abfahrten mit normalisierten Providerergebnissen ergänzt.
- NRW-bevorzugte EFA-Daten, konservative Echtzeitkonsolidierung, transparente Fallback-/Stale-Zustände, begrenzten Speichercache und datensparsame Diagnose ergänzt.
- Konfigurierbare `db.transport.rest`- und EFA-Adapter mit HTTPS-Prüfung, begrenzten Wiederholungen, Antwortlimits und dokumentierten Live-Probegrenzen ergänzt.
