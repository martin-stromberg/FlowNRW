# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK1: Reale konfigurierbare bundesweite Suche/Routing, NRW-Abfahrten, Eingabearten/Nähe und Live-Proben | DbRestProvider und EfaProvider für Search/Nearby/Trip/Departures, Fachservices, transparenter Entwicklungsfallback; Umsetzung 4–8 | Mapper-/Fallbacktests und gesonderte echte NRW-Abfahrt sowie bundesweite Verbindung, gegebenenfalls über EFA | Abgedeckt |
| AK2: Gelieferte Verbindungs-/Abfahrtsfelder, unbekannte Werte, Zeitzonen/Tageswechsel | Gemeinsame Modelle und beide Mapper; Umsetzung 2/4/5 | JourneyTimesAndLegs, HandlesJourneyAcrossMidnight, MapsDbRestDepartures, EfaMapper_NormalizesExplicitTimezoneAndDepartureStates und RapidJSON-Fixtures decken Felder, Tageswechsel, Zeitzone, Ausfall und unbekannte Echtzeit ab | Abgedeckt |
| AK3: NRW-Priorität, bundesweiter Fallback, eindeutige Fahrt-/Haltestellenkonsolidierung und Metadaten | Echtes Grenzpolygon, Orchestrator und Konsolidierer; DHID ausdrücklich nur Haltestellenidentität, zusätzliche eindeutige Fahrtprüfung erforderlich | NRW-Innen-/Außen-/Randpunkte, Priorität bei mindestens einem NRW-Punkt, partielle/leere/fehlerhafte Antworten, eindeutige und mehrdeutige Zuordnung sowie DoesNotMergeDifferentTripsWithSameDhid | Abgedeckt |
| AK4: Begrenzter Cache, optimierte Abrufe, Cancellation/Retry, HTTPS und Datenschutz | HTTP-Gateway, Cache und Diagnose mit konkreten Grenzen, Umsetzung 3/7 | TTL/Größe/Stale, Deduplizierung, Cancellation, Retry und Logtests; Tasks 38/39 sichern Timeout und Sicherheitsverhalten | Abgedeckt |
| AK5: Deterministische Tests und Repository-Prüfungen, Windows-CI | Umsetzung 8/10, Tasks 32–41 und 44/45; mindestens 70 % Core-Zeilenabdeckung | Fachliche Tests zu AK1–4, fünf unveränderte Counterfälle, Format, Warnungen als Fehler, Core-Coverage und Windows-Prüfungen | Abgedeckt |
| AK6: Erweiterbare Grenzen, dauerhafte Dokumentation, begründete Werte und konkrete Zugangssperren | Interfaces/Optionsobjekte, Blockadestrategie und Umsetzung 9 für docs/help/fahrplanauskunft/ | Explizite Dokumentationsabnahme für Ergänzung weiterer Verbünde und spätere Sharing-/Push-Grenzen, Tasks 42/43; Live-/Fixture-/Produktivgrenzen werden getrennt dokumentiert | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| AK1–6: Datenversorgung ohne fachliche UI | Keine nativen UI-E2E; echte HTTP-Proben und deterministische Adapter-/Service-/Modelltests | Nicht erforderlich mit Begründung: Der ursprüngliche Projektschritt schließt fachliche UI-Änderungen und native UI-Abnahme ausdrücklich aus. UI-Flüsse folgen in abhängigen Entwicklungsschritten. |

## Fehlende oder unvollständige Planbestandteile

## Hinweise

Die Nachprüfung verwendet die bereits vollständig gelesene Feature-Anforderung, inventory.md einschließlich logic.md/models.md/tests.md und die ursprüngliche vollständige Schritt-1-Anforderung im Projektplan. Aktueller plan.md und die Tasks-Datei wurden erneut vollständig gelesen. Prüfbasis ist `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`, aktiver Schritt-Branch dessen Suffix `-schritt-1-fahrplanauskunft`.

Alle im archivierten Bericht plan-check.1.md genannten Lücken sind im aktuellen Plan geschlossen: konkrete Zeit-/Ausfalltests, zusätzliche Fahrtidentität auch bei gemeinsamer DHID und dauerhafte Erweiterungsdokumentation mit Ausgabepfad sowie Abnahme. Die Interfacebenennung IEfaProvider ist vereinheitlicht.

Reale Providerfelder, Live-Erreichbarkeit und NRW-Polygon werden als technische Umsetzungsvoraussetzungen beschafft und geprüft. Der EFA-Entwicklungsfallback bei db-rest-Ausfall bleibt zulässig; daraus werden keine Produktionsfreigabe oder flächendeckende Versorgung abgeleitet. Ein zwingend fehlender Zugang ist als konkrete Blockade zu dokumentieren.

Der Plan berücksichtigt FlowNRW.Core, FlowNRW.Tests und FlowNRW/MauiProgram. Windows-Test-/Releasekonfiguration bleibt erhalten; iOS-CI und neue Deployment-Automation sind ausgeschlossen. Mindestens 70 % Core-Zeilenabdeckung ist als auszuführender Nachweis festgelegt. Der positive Status bestätigt Planvollständigkeit und behauptet weder bereits ausgeführte Tests noch erfolgreiche Live-Proben oder implementiertes Verhalten.
