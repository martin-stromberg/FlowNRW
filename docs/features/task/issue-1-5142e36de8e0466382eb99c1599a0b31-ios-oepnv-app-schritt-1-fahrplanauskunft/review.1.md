# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

- [x] `Address`, `GeoCoordinate`, `Stop`, `NearbyStopResult` — vorhanden; Namen, Koordinaten, Provider-/DHID-Kennungen und optionale Entfernung abgebildet; endliche WGS84-Grenzen validiert.
- [x] `Journey`, `JourneyLeg`, `WalkingSegment`, `Transfer`, `Line`, `Operator`, `GeoGeometry` — vorhanden; Teilstrecken, Fußwege, Umstiege, Betreiber und gelieferte Geometrien normalisiert.
- [x] `StopEvent`, `RealtimeStatus`, `TripIdentity`, `ProviderResult<T>` — vorhanden; Soll-/Istzeit, Verspätung, Ausfall, Soll-/Istplattform, Quellen, Abrufzeit, Warnungen, Fehler und Fallback-/Stale-Status verfügbar; unbekannte Felder nullable.
- [x] Sämtliche geplanten fachlichen Interfaces — vorhanden, einschließlich `ITransitProvider`, `IEfaProvider`, `IRoutingService`, `IStopSearchService`, `IDepartureService`, Orchestrator-, Klassifikator-, Konsolidierungs-, Cache-, Gateway- und Diagnoseverträge.
- [x] `TransitProviderOptions`, `TransitCacheOptions` — sichere Endpunkte, Ergebnis-/Suchlimits, Timeout, Wiederholungsgrenze und Cachegrenzen mit Validierung vorhanden.
- [x] `DbRestProvider`, `EfaProvider` — `SearchAsync`, `NearbyAsync`, `RouteAsync`, `DeparturesAsync` mit asynchronen, abbrechbaren echten HTTP-Anfragen vorhanden; fremde IDs werden nicht blind übertragen; alternative EFA-Basisadresse tatsächlich verwendet.
- [x] `DbRestResponseMapper`, `EfaResponseMapper`, `TransitJson` — Search/Nearby/Journey/Departure-Normalisierung vorhanden; providerbezogene JSON-Strukturen direkt über JsonElement statt zusätzlicher DTO-Klassen gelesen. Dies liefert die geplante getrennte Adapter-Normalisierung ohne funktionslose Platzhalter.
- [x] `TransitHttpGateway.GetAsync`, `RetryPolicy.ShouldRetry`, `TransitDiagnostics.Record` — HTTPS-Prüfung, begrenztes Timeout, Abbruch, Wiederholung, begrenzte Antwortgröße und datensparsame Diagnose umgesetzt.
- [x] `MemoryTransitCache.Get/Set` — typisierter, begrenzter In-Memory-Cache mit TTL und höchstens fünf Minuten altem markiertem Echtzeitfallback vorhanden.
- [x] `NrwRegionClassifier.IsInNrw` — eingebettetes, versioniertes amtliches NRW-MultiPolygon, Löcher und Grenzpunkte berücksichtigt; echte Landesgrenze statt VRR oder Rechteck. Ressource und Herkunftsdokumentation vorhanden.
- [x] `RealtimeConsolidator.Consolidate` — eindeutige Stop- und Fahrtzuordnung, DHID allein nicht ausreichend; Mehrdeutigkeiten, widersprüchliche Fahrtkennungen und nicht passende Richtungen werden nicht zusammengeführt.
- [x] `ProviderOrchestrator` — Deduplizierung, individuelle Abbruchverantwortung, Cache, regionaler Vorrang für vorhandene NRW-Koordinaten, ergänzende Konsolidierung und Fehler-/Leer-/Warnungsfallback vorhanden. Einschränkung für Orte ohne Koordinaten siehe unten.
- [x] `StopSearchService`, `RoutingService`, `DepartureService` — Eingabevalidierung und Verdrängung älterer Anfragen vorhanden; Routing filtert vergangene Fahrten und sortiert kommende Ergebnisse.
- [x] `MauiProgram.CreateMauiApp` — Optionslesen und DI-Komposition der tatsächlichen Fachservices, Adapter, Mapper, Cache, Diagnose und HTTP-Gateway vorhanden. Keine fachliche UI-Änderung.
- [x] Testklassen unter `FlowNRW.Tests/Transit/` und `FlowNRW.Tests/` — deterministische Mapper-, Adapter-, Modell-, Service-, Cache-, Sicherheits-, Retry-, Cancellation-, NRW-, Fallback- und Konsolidierungstests vorhanden. `AdapterGateway`, `TransitTestProvider`, `TransitTestClock`, `TransitTestHandler` und gespeicherte EFA-Antworten stellen die Testhilfen bereit.
- [x] Bestehende fünf `ClickCounterTests` — erhalten. Nachweis vom 8. September: 89 erfolgreiche Tests, keine Fehler/Skips; Cobertura tatsächlich gelesen: 497/528 Core-Zeilen = 94,12 %.
- [x] Format-/XML-/Buildnachweise — dokumentiert; Windows-Build 0 Fehler/0 Warnungen, bestehende GitHub-Workflows unverändert. Kein neuer Build oder Liveabruf in diesem Review.
- [x] Echte Fachserviceproben — vier normalisierte Ausgaben mit 5 Suchtreffern, 5 Nahbereichshaltestellen, 5 NRW-Abfahrten und 2 bundesweiten Verbindungen vorhanden. db-rest-503/Timeout und EFA-Entwicklungsgrenzen ausdrücklich dokumentiert; Fixtures werden nicht als neue Liveabrufe ausgegeben.

## Offene Aufgaben

- [ ] `ProviderOrchestrator.RouteAsync/DeparturesAsync` — teilweise umgesetzt: In `FlowNRW.Core/Transit/ProviderOrchestrator.cs:51–60` wird regionale Priorität ausschließlich aus bereits mitgelieferten Koordinaten bestimmt, noch bevor die Adapter Namen auflösen. `RoutingService` akzeptiert zugleich reine Ortsnamen und Stop-IDs ohne Koordinaten. Beispiel: Ursprung `Address { Name = "Gelsenkirchen Hbf" }`, Ziel mit Berliner Koordinate, nationaler Adapter liefert nach eindeutiger Namensauflösung eine gültige Verbindung ohne Warnung. `preferRegional` bleibt false; `Execute` ruft wegen Zeile 107 keine regionale Quelle auf. Die im nationalen Adapter gewonnenen NRW-Koordinaten fließen nicht zur Auswahl zurück. Gleiches gilt für Abfahrten an einem NRW-Stop mit ID/Name ohne Coordinate. Die geplante Priorisierung ab einem NRW-Punkt muss auch für diese akzeptierten Eingabeformen nach Auflösung greifen; wirklich ungeklärte Regionszuordnung benötigt die im Plan verlangte Diagnose statt stiller Gleichsetzung mit außerhalb NRW. Betroffene Tasks 26/28/35 und ergänzende Task 46.
- [ ] Regression für diesen Ablauf — vorhandene NRW-Prioritätstests verwenden ausschließlich bereits koordinierte Adressen; Name-/ID-Eingaben mit erfolgreicher nationaler Antwort und anschließend notwendiger NRW-Anreicherung fehlen. Für die Korrektur einen deterministischen Service-/Orchestratornachweis ergänzen, der die regionale Anforderung tatsächlich beobachtet; bloßer Klassifikatortest genügt nicht.
- [ ] Tasks 42/43: Dauerhafte Betriebs-/Erweiterungsdokumentation und deren Abnahme — planmäßig noch in der nachgelagerten Lifecycle-Dokumentationsphase auszuführen. Providerproben, Grenzdokumentation und Implementierungsnachweise sind bereits vorhanden; die vollständige Anleitung zu weiteren Verbünden und Sharing-/Push-Erweiterungsgrenzen fehlt aktuell. Dies ist nachgehaltene Dokumentationsarbeit, keine zusätzliche Codekorrektur.

## Hinweise

Prüfung vom 9. September 2026 auf dem Schritt-Branch `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft`; Diff-Basis ist der gleichnamige Projektbranch ohne `-schritt-1-fahrplanauskunft`. Aktueller Plan und Tasks wurden vollständig gelesen. Alle relevanten C#-Quelldateien des Fachkerns und der Tests sowie MauiProgram wurden vollständig gelesen, einschließlich untracked Dateien, die git diff nicht zeigt. Der tatsächliche Repositoryaufbau ersetzt gemäß Auftrag den generischen src-Pfad des Skills.

Die Aufgabenliste wurde mit tatsächlichen Testnachweisen aktualisiert; Benennungsabweichungen bei äquivalenten Tests gelten nicht als fehlende Implementierung. Nicht neu ausgeführte Prüfungen sind ausdrücklich über `docs/help/fahrplanauskunft/verification/implementation-checks.md` und dessen Rohreport belegt. Der dokumentierte Windows-Build lag vor abschließenden reinen Core-Format-/XML-Anpassungen; danach wurde Core einschließlich Tests erneut kompiliert und geprüft. Native UI-E2E ist in diesem UI-freien Schritt nicht erforderlich.

Die funktionale Abweichung benötigt keine neue Produktentscheidung. Eindeutige Ortsauflösung und regionale Entscheidung sind bereits autorisierter Scope. Eine formale vollständige Planabnahme kann nach Korrektur und späterer Dokumentationsfertigstellung aktualisiert werden. Es wurden keine Quellen korrigiert, keine Commits erstellt und keine Branches gewechselt.
