# Release Notes

## Important Notes Before Update

- Local iOS store deployments now require a Mac with release Xcode 27 or later plus `Microsoft.iOS.Sdk` 27.x and `Microsoft.NET.Sdk.Maui` `10.0.110` or later; the deployment script enforces this toolchain before the build (`-SkipToolchainCheck` opts out of the warning-level check for `build`/`simulator`/`device`).
- The included `db.transport.rest` and EFA endpoints are development/data-source options; productive public use still requires provider-specific availability, terms and approvals.

## What's New

- iOS packages are now built with the iOS 27 SDK (Xcode 27.2), ahead of Apple's April 2027 requirement (ITMS-90725): the `package-ios` job runs on the `xcode-27` runner image and pins `Microsoft.iOS.Sdk` `27.2.10000` / `Microsoft.NET.Sdk.Maui` `10.0.110`; a real TestFlight upload was accepted by Apple as VALID.
- The app adopts the UIScene lifecycle required by Xcode 27 (scene-level delegate), declares en/de localizations and ships a privacy manifest; local iOS deployments check the Mac toolchain before building.
- Fixed: a selected journey stays selected after an automatic refresh even for connections with walking legs; the detail page no longer duplicates the status text.
- New static project site under `site/` deployed to GitHub Pages (`https://martin-stromberg.github.io/FlowNRW/`) via the `pages.yml` workflow: purpose, key features, curated screenshots and links to the release overview, repository and documentation, in light and dark presentation matching the app design.
- Scheduled GitHub Actions now open recurring review issues for @softwarewerkstatt: monthly architecture/security review, weekly NuGet check and yearly compliance review, each skipped while a previous open issue of the same type exists.
- Journey endpoints can now be picked directly on an OpenStreetMap map (map button next to the input field, "Übernehmen" resolves the tapped position to an address) instead of entering coordinates; the endpoint search form is more compact.
- Fixed map position picking and hardened base-map initialization: "Übernehmen" stays disabled when the map fails to load, with a manual retry.
- Removed redundant in-page titles; each page shows its title only in the navigation bar.
- Native FlowNRW design integration with persistent departures, connections and stops areas, structured departure and journey cards, accessible map/list alternatives, light/dark semantic surfaces and Windows visual regression evidence. A first real-device iOS pass is accepted by the user; sporadic departure fetch errors remain under observation via the diagnostic protocol.
- Opt-in diagnostic protocol in settings: HTTP calls, monitor outcomes and lifecycle events are recorded locally and can be sent to the maintainer by e-mail ("Protokoll senden").
- Journey details show plain local times with "Vortag"/"Folgetag" day markers instead of technical UTC offsets; transfers render between the affected legs.
- Cached departure boards restore their times on restart (marked stale and refreshed immediately), and reopened stop monitors reuse session data during refresh.

- Refresh stale visible departures and journeys on resume without navigation; bounded opportunistic iOS favorite refresh, disabled together with automatic refresh. Native iOS acceptance remains pending.
- Persistent favorite stops on the home screen, independently refreshable departure boards, explicit distance sorting and validated monitor/map navigation.
- Configurable foreground refresh intervals (off, 30, 60, 120 or 300 seconds) with persisted settings, cancellation on navigation/window deactivation and independent favorite loops.

- Explicit foreground location for journey endpoints and nearby stops, shared list/map selections and cancellation of superseded requests. Native Windows fixture tests and the authorized real-location probe pass; iOS device acceptance remains with the user.

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

- Für lokale iOS-Store-Deployments ist auf dem Build-Mac jetzt Release-Xcode ≥ 27 mit `Microsoft.iOS.Sdk` 27.x und `Microsoft.NET.Sdk.Maui` ≥ `10.0.110` erforderlich; das Deployment-Skript erzwingt diese Werkzeugkette vor dem Build (`-SkipToolchainCheck` hebt die Warnprüfung für `build`/`simulator`/`device` auf).
- Die enthaltenen `db.transport.rest`- und EFA-Endpunkte sind Entwicklungs-/Datenquellenoptionen; für einen produktiven öffentlichen Betrieb müssen Verfügbarkeit, Nutzungsbedingungen und Freigaben des jeweiligen Anbieters geklärt sein.

## Neuerungen

- iOS-Pakete werden vor Apples Frist April 2027 (ITMS-90725) bereits mit dem iOS-27-SDK (Xcode 27.2) gebaut: Der `package-ios`-Job läuft auf dem `xcode-27`-Runner-Image und pinnt `Microsoft.iOS.Sdk` `27.2.10000` / `Microsoft.NET.Sdk.Maui` `10.0.110`; ein echter TestFlight-Upload wurde von Apple als VALID angenommen.
- Die App nutzt den von Xcode 27 geforderten UIScene-Lebenszyklus (Scene-Ebene), deklariert die Lokalisierungen en/de und enthält ein Datenschutz-Manifest; lokale iOS-Deployments prüfen die Mac-Werkzeugkette vor dem Build.
- Behoben: Eine gewählte Verbindung bleibt nach automatischer Aktualisierung auch bei Fußwegabschnitten ausgewählt; die Detailseite zeigt die Statusmeldung nicht mehr doppelt.
- Neue statische Projektseite unter `site/`, per `pages.yml`-Workflow nach GitHub Pages ausgeliefert (`https://martin-stromberg.github.io/FlowNRW/`): Zweck, zentrale Features, kuratierte Screenshots sowie Verweise auf die Release-Übersicht, Repository und Dokumentation — hell und dunkel, an das App-Design angelehnt.
- Zeitgesteuerte GitHub Actions legen wiederkehrende Review-Issues für @softwarewerkstatt an: monatliches Architektur-/Sicherheitsreview, wöchentliche NuGet-Prüfung und jährliches Compliance-Review — jeweils nur, wenn kein offenes Issue desselben Typs existiert.
- Start und Ziel lassen sich direkt auf einer OpenStreetMap-Karte antippen (Karten-Schaltfläche neben dem Eingabefeld; „Übernehmen" löst den Punkt in eine Adresse auf) statt Koordinaten einzugeben; die Suchmaske ist verdichtet.
- Karten-Positionsauswahl repariert und Karteninitialisierung gehärtet: „Übernehmen" bleibt bei Ladefehlern deaktiviert, ein manuelles Neuladen ist möglich.
- Überflüssige In-Page-Überschriften entfernt; jede Seite zeigt ihren Titel nur noch in der Navigationsleiste.
- Native FlowNRW-Designintegration mit persistenten Abfahrts-, Verbindungs- und Haltestellenbereichen, strukturierten Abfahrts- und Verbindungskarten, barrierefreien Karten-/Listenalternativen, hellen/dunklen semantischen Oberflächen und Windows-Visual-Regression-Nachweisen. Eine erste iOS-Geräteprüfung ist vom Nutzer abgenommen; sporadische Abfahrtsfehler bleiben über das Diagnoseprotokoll beobachtbar.
- Optionales Diagnoseprotokoll in den Einstellungen: HTTP-Aufrufe, Monitorergebnisse und Lifecycle-Ereignisse werden lokal aufgezeichnet und können per E-Mail an den Maintainer gesendet werden („Protokoll senden").
- Verbindungsdetails zeigen lokale Zeiten mit „Vortag"/„Folgetag"-Markierung statt UTC-Offsets; Umstiege erscheinen zwischen den betroffenen Fahrtabschnitten.
- Gespeicherte Abfahrtsboards zeigen beim Neustart wieder ihre Zeiten (als veraltet markiert und sofort aktualisiert); erneut geöffnete Haltestellen nutzen Sitzungsdaten während der Aktualisierung.
- Veraltete sichtbare Abfahrten und Verbindungen bei Wiederaufnahme ohne Seitensprung erneuern; begrenzte opportunistische iOS-Favoritenaktualisierung, gemeinsam mit der Automatik abschaltbar. Native iOS-Abnahme bleibt offen.
- Dauerhaft gespeicherte Haltestellenfavoriten auf der Startseite, unabhängig aktualisierbare Abfahrtstafeln, explizite Entfernungssortierung und validierte Monitor-/Kartennavigation.
- Konfigurierbare Vordergrundintervalle (aus, 30, 60, 120 oder 300 Sekunden) mit gespeicherter Einstellung, Abbruch bei Navigation/Fensterdeaktivierung und unabhängigen Favoritenschleifen.

- Expliziter Standort für Start/Ziel und nahe Haltestellen mit gemeinsamer Listen-/Kartenauswahl und Abbruch veralteter Anfragen. Native Windows-Fixturetests und freigegebene echte Standortprobe bestanden; iOS-Geräteabnahme bleibt beim Nutzer.

- Interaktive Haltestellenkarte und gelieferte Verbindungsverläufe mit nativer Listenalternative, Kartenfehlerbehandlung und begrenztem HTTPS-Kachelcache.

- Manuelle Haltestellenmonitore mit Ausfall, Verspätung und Gleiswechsel; bei Aktualisierungsfehlern bleiben letzte bekannte Daten und Quelle erhalten.

- Native manuelle Verbindungssuche mit Adress-/Haltestellenwahl, Koordinaten, Ergebnissen, Details und erhaltenem Kontext bei Rücknavigation.
- Sichtbare Lade-/Fehler-/Leerzustände, Quelle und Datenalter, Ersatzquelle sowie unbekannte Echtzeit.
- iOS-Plattformbasis und Einrichtungs-/Prüfanleitung; native iOS-Abnahme beim Nutzer noch offen.

- Fehlende bundesweite Verbindungen und Abfahrten bei regionalen Teilergebnissen ergänzt; eindeutige Fahrten zusammengeführt, regionale Echtzeit priorisiert und finale Ergebnismenge sortiert/begrenzt.

- Asynchrone, abbrechbare Fahrplandienste für Adress-/Haltestellensuche, nahe Haltestellen, Verbindungen und Abfahrten mit normalisierten Providerergebnissen ergänzt.
- NRW-bevorzugte EFA-Daten, konservative Echtzeitkonsolidierung, transparente Fallback-/Stale-Zustände, begrenzten Speichercache und datensparsame Diagnose ergänzt.
- Konfigurierbare `db.transport.rest`- und EFA-Adapter mit HTTPS-Prüfung, begrenzten Wiederholungen, Antwortlimits und dokumentierten Live-Probegrenzen ergänzt.
