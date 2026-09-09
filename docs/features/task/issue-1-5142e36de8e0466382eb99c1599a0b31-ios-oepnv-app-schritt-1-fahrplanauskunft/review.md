# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

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
- [x] `ProviderOrchestrator` — Deduplizierung, individuelle Abbruchverantwortung, Cache, regionaler Vorrang für vorhandene NRW-Koordinaten, ergänzende Konsolidierung und Fehler-/Leer-/Warnungsfallback vorhanden. Orte ohne Koordinaten werden jetzt vor der Entscheidung aufgelöst.
- [x] `StopSearchService`, `RoutingService`, `DepartureService` — Eingabevalidierung und Verdrängung älterer Anfragen vorhanden; Routing filtert vergangene Fahrten und sortiert kommende Ergebnisse.
- [x] `MauiProgram.CreateMauiApp` — Optionslesen und DI-Komposition der tatsächlichen Fachservices, Adapter, Mapper, Cache, Diagnose und HTTP-Gateway vorhanden. Keine fachliche UI-Änderung.
- [x] Testklassen unter `FlowNRW.Tests/Transit/` und `FlowNRW.Tests/` — deterministische Mapper-, Adapter-, Modell-, Service-, Cache-, Sicherheits-, Retry-, Cancellation-, NRW-, Fallback- und Konsolidierungstests vorhanden. `AdapterGateway`, `TransitTestProvider`, `TransitTestClock`, `TransitTestHandler` und gespeicherte EFA-Antworten stellen die Testhilfen bereit.
- [x] Bestehende fünf `ClickCounterTests` — erhalten. Nachweis der Korrekturrunde vom 9. September: 98 erfolgreiche Tests, keine Fehler/Skips; Cobertura tatsächlich gelesen: 541/552 Core-Zeilen = 98,00 %.
- [x] Format-/XML-/Buildnachweise — dokumentiert; Windows-Build 0 Fehler/0 Warnungen, bestehende GitHub-Workflows unverändert. Kein neuer Build oder Liveabruf in diesem Review.
- [x] Echte Fachserviceproben — vier normalisierte Ausgaben mit 5 Suchtreffern, 5 Nahbereichshaltestellen, 5 NRW-Abfahrten und 2 bundesweiten Verbindungen vorhanden. db-rest-503/Timeout und EFA-Entwicklungsgrenzen ausdrücklich dokumentiert; Fixtures werden nicht als neue Liveabrufe ausgegeben.

- [x] `ProviderOrchestrator.ResolveRegion` — ergänzt: Vor Routing und Abfahrten werden nicht koordinierte Namen/Stopidentitäten aufgelöst. Nur ein eindeutiger Treffer mit Koordinate wird übernommen; ausgewählte Stops müssen nach Namespace/ID oder DHID übereinstimmen. Unaufgelöste Regionen erhalten `region-unknown`. Die nachfolgende Providerwahl berücksichtigt die gewonnene NRW-Koordinate.
- [x] `ProviderOrchestratorTests_RegionResolution` — vier Regressionen zu Namen, ID-only-Routing, Abfahrtsstop und ungeklärter Region vorhanden; sie beobachten regionales Ergebnis bzw. explizite Warnung.
- [x] `TransitHttpGateway` — Fehler beim Lesen des Bodys nach HTTP 200 bleiben Timeout/Transportfehler; Status wird vor Retryentscheidung zurückgesetzt. Zwei neue Bodyfehler-Regressionen vorhanden.
- [x] Alle drei Fachservices — ungültige Folgeeingaben durchlaufen ebenfalls Latest und verdrängen ältere Anfragen. Drei neue Regressionen beobachten den Abbruch alter Ergebnisse nach Validierungsfehler.

## Offene Aufgaben

## Hinweise

Die dauerhafte Betriebs-/Erweiterungsdokumentation ist nach gezielter Nachprüfung ebenfalls abgenommen; Tasks 42/43 sind Erledigt. architektur.md beschreibt nun korrekt den Austausch des nationalen Providers über die DI-Factory sowie die notwendigen Änderungen an Auswahl-, Regions-, Fallbacklogik und Konstruktor/DI für zusätzliche Verbünde. Automatische Providerentdeckung wird nicht behauptet. Sharing-/Push-Anschlüsse sind als spätere Erweiterungen gekennzeichnet. datenmodell.md ordnet Zeiten/Geometrie den Legs und IsStale dem ProviderResult zu. installation.md unterscheidet tatsächlich geladene Konfigurationsschlüssel von MaxSearchLength als programmatischer Option und der eingebetteten NRW-Grenzversion. API-, Entwicklungs-/Produktivgrenzen und vorhandene Testnachweise sind dokumentiert; keine gelieferte UI wird behauptet. Die drei konkreten Dokumentationsbefunde sind damit geschlossen. Der Status umfasst nun Code, Integration, Tests und Dokumentation des Plans; eine abschließende fachliche Projektabnahme bleibt Aufgabe des übergeordneten Workflows.

Die funktionalen Befunde aus review.1.md sind geschlossen. Alle korrigierten Fachquellen und neuen Regressionen wurden vollständig gelesen; die ursprüngliche vollständige Quellprüfung bleibt für unveränderte Komponenten die Grundlage. Keine zusätzliche fachliche Codeabweichung in der gezielten Nachprüfung festgestellt. Die tatsächlich vorhandene Struktur FlowNRW.Core/Transit, FlowNRW.Tests und FlowNRW/MauiProgram bleibt maßgeblich; untracked Dateien sind berücksichtigt.

Nachweis `docs/help/fahrplanauskunft/verification/iteration2-checks.md`: neun zunächst fehlschlagende Regressionen nach Korrektur grün; Gesamtsuite 98 erfolgreich, 0 Fehler, 0 übersprungen. Aktueller Cobertura-Rohreport gelesen: 541/552 Core-Zeilen = 98,00 %. Windows-Solution-Build nach Korrektur mit 0 Warnungen/0 Fehlern, Formatprüfungen und XML-Hook erfolgreich. Keine neuen Tests oder Liveproben durch den Prüfer; die dokumentierten echten Serviceproben vom 8. September bleiben als solche gekennzeichnet.

Review vom 9. September 2026 auf Schritt-Branch task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft, Diff-Basis der Projektbranch ohne Schritt-Suffix. Ausschließlich review.md und die Tasks-Datei aktualisiert. Keine Quellen geändert, keine Commits oder Branchwechsel.

