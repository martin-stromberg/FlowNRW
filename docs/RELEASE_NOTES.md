# Release Notes

## Important Notes Before Update

- The included `db.transport.rest` and EFA endpoints are development/data-source options; productive public use still requires provider-specific availability, terms and approvals.

## What's New

- Interactive stop maps and supplied journey geometries with native list access, manual map recovery and bounded HTTPS tile caching.

- Manual stop departure boards with cancellation, delay and platform changes; failed refreshes retain the last known data and provenance.

- Native manual journey search with address/stop selection, coordinates, results, itinerary details and retained back-navigation context.
- Visible loading/error/empty states, source and data age, fallback and unknown realtime information.
- iOS platform foundation and setup/checklist; native iOS acceptance remains pending with the user.

- Fixed missing national journeys and departures in partial regional results; merge uniquely matching trips, preserve regional realtime and apply the final sorted result limit.

- Added asynchronous, cancellable transit services for address/stop search, nearby stops, journeys and departures with normalized provider results.
- Added NRW-aware EFA prioritization, conservative realtime consolidation, transparent fallback/stale states, bounded memory caching and privacy-preserving diagnostics.
- Added configurable `db.transport.rest` and EFA adapters with HTTPS validation, bounded retries, response limits and documented live-probe boundaries.

## Wichtige Hinweise vor dem Update

- Die enthaltenen `db.transport.rest`- und EFA-Endpunkte sind Entwicklungs-/Datenquellenoptionen; für einen produktiven öffentlichen Betrieb müssen Verfügbarkeit, Nutzungsbedingungen und Freigaben des jeweiligen Anbieters geklärt sein.

## Neuerungen

- Interaktive Haltestellenkarte und gelieferte Verbindungsverläufe mit nativer Listenalternative, Kartenfehlerbehandlung und begrenztem HTTPS-Kachelcache.

- Manuelle Haltestellenmonitore mit Ausfall, Verspätung und Gleiswechsel; bei Aktualisierungsfehlern bleiben letzte bekannte Daten und Quelle erhalten.

- Native manuelle Verbindungssuche mit Adress-/Haltestellenwahl, Koordinaten, Ergebnissen, Details und erhaltenem Kontext bei Rücknavigation.
- Sichtbare Lade-/Fehler-/Leerzustände, Quelle und Datenalter, Ersatzquelle sowie unbekannte Echtzeit.
- iOS-Plattformbasis und Einrichtungs-/Prüfanleitung; native iOS-Abnahme beim Nutzer noch offen.

- Fehlende bundesweite Verbindungen und Abfahrten bei regionalen Teilergebnissen ergänzt; eindeutige Fahrten zusammengeführt, regionale Echtzeit priorisiert und finale Ergebnismenge sortiert/begrenzt.

- Asynchrone, abbrechbare Fahrplandienste für Adress-/Haltestellensuche, nahe Haltestellen, Verbindungen und Abfahrten mit normalisierten Providerergebnissen ergänzt.
- NRW-bevorzugte EFA-Daten, konservative Echtzeitkonsolidierung, transparente Fallback-/Stale-Zustände, begrenzten Speichercache und datensparsame Diagnose ergänzt.
- Konfigurierbare `db.transport.rest`- und EFA-Adapter mit HTTPS-Prüfung, begrenzten Wiederholungen, Antwortlimits und dokumentierten Live-Probegrenzen ergänzt.
