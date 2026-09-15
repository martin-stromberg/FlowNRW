# Abnahmeprüfung – Entwicklungsschritt 1

## Ergebnis

**Status:** Abweichungen gefunden

Geprüft am 2026-09-09: Commit `0d7eb4b` auf dem Schrittbranch `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft`, gegen Basisbranch `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Grundlage sind die vollständige Schrittbeschreibung und ihre sechs Akzeptanzkriterien in `project-plan.md` sowie `requirement.md`.

## Abweichungen

- [ ] **AK 3: Bundesweite zusätzliche Fahrten fehlen bei partiellen regionalen Antworten.** `FlowNRW.Core/Transit/ProviderOrchestrator.cs:136–149` ruft bei Warnungen den zweiten Provider ab und markiert `partial-primary`, übergibt aber beide Mengen an einen Merge, der ausschließlich die Primärmenge zurückgibt. `MergeJourneys` (Zeile 167, insbesondere 170) iteriert nur regionale Verbindungen; `MergeEvents` (Zeile 164) ruft zweimal `RealtimeConsolidator.Consolidate` auf, dessen `scheduled.Select` (`RealtimeConsolidator.cs:9`) ebenfalls niemals zusätzliche Ereignisse aufnimmt. Konkreter Fall: EFA liefert Fahrt A mit einer Teilantwortwarnung, die bundesweite Quelle liefert A und eine andere nächste Fahrt B. B verschwindet aus dem Ergebnis, obwohl sie die regional fehlenden Ergebnisse ergänzen soll. Das betrifft Routing und Abfahrten. Erwartet ist eine konservativ konsolidierte Ergänzung fehlender bundesweiter Ergebnisse unter Erhalt regionaler Echtzeitpriorität und ohne Vermischen verschiedener Fahrten. Die vorhandenen Tests prüfen leere/fehlerhafte Rückfälle und partielle **Suche**, aber nicht diese partielle Routing-/Abfahrtsmenge; ein entsprechender deterministischer Regressionstest fehlt.

## Hinweise

| Kriterium | Quellenprüfung und Nachweise |
|---|---|
| AK 1 | Reale `DbRestProvider`-/`EfaProvider`-Adapter unterstützen Ortssuche, Koordinatenrouting, Nahbereich, Verbindungen und Abfahrten. Fachservices und DI sind in `MauiProgram.cs` verbunden. `service-live-2026-09-08.jsonl` belegt reale NRW-Abfahrten und Berlin–Hamburg einschließlich Anschlussfußweg über EFA sowie den ausgefallenen db-rest-Zugriff. |
| AK 2 | Mapper und Modelle enthalten Fahrtabschnitte, Fußwege, Umstiege, Betreiber, Linien, Geometrie und nullable Echtzeitfelder. Offset-/Tageswechsel-, Ausfall-, Geometrie- und Plattformtests wurden in den tatsächlichen Tests geprüft. |
| AK 3 | NRW-MultiPolygon statt Rechteck, eindeutige Regionauflösung, konservative Fahrt-/Haltestellenzuordnung, technische Warnungen, Quelle und Zeitstempel sind umgesetzt. Die oben genannte Ergänzung fehlender Ergebnisse bleibt offen. |
| AK 4 | Begrenzter Memory-Cache, gemeinsame laufende Abrufe, Cancellation, HTTPS-Prüfung, deaktivierte Redirects, Antwortgrößenlimit, Timeout, begrenzte Retries und datensparsame Diagnose sind im Code vorhanden. Konfiguration und Lebensdauern sind dokumentiert. |
| AK 5 | Der aktuelle Nachweis `docs/help/fahrplanauskunft/verification/iteration2-checks.md` dokumentiert 98 bestandene Tests, 0 übersprungene Tests, 541/552 Core-Zeilen (98,00 %), Windows-Releasebuild ohne Warnungen/Fehler, Format und XML-Prüfung. Der Cobertura-Rohreport liegt bei. `run-tests-2026-09-09.md` dokumentiert zusätzlich 26 bestandene Node-Releaseversionstests; seine älteren 89 .NET-Tests sind durch den Korrekturrundennachweis ersetzt. Der Diff verändert keine `.github/workflows`-Datei. Die Testabdeckung der oben genannten Teilantwortlücke ist zu ergänzen. |
| AK 6 | Getrennte Provider-, Fachservice-, Transport-, Cache- und Konsolidierungsgrenzen sowie dauerhafte Betriebs-/Erweiterungsdokumentation sind vorhanden. Keine aktuelle Sharing-/Push-Scheinoberfläche. |

Die Prüfung umfasste den Basisbranch-Diff, tatsächliche Transitquellen, MAUI-Komposition, einschlägige Service-/Mapper-/Identitätstests und dauerhafte Live-/Testbelege. Es wurden keine Quellen korrigiert und keine Routineprüfungen erneut ausgeführt. Der Befund folgt direkt aus dem deterministischen Mengenverhalten der beiden Mergefunktionen; er setzt keinen erreichbaren externen Anbieter voraus.

db-rest-503/Timeout und die eingeschränkte, nicht produktiv freigegebene EFA-Entwicklungsversorgung sind dokumentierte Betriebsgrenzen und werden nicht als zusätzliche Abweichung behandelt. Punktuelle bundesweite Liveproben sind kein Nachweis flächendeckender Verfügbarkeit. Schritt 1 liefert Services und verändert keine fachliche UI; native UI-Abnahme gehört zu den Folgeschritten. Windows-Test und -Release bleiben erforderlich, native iOS-Abnahme liegt gemäß Nutzerentscheidung zunächst beim Nutzer.
