# Test- und Nachweisstand

## Bestand

Die Inventur wurde am 2026-09-09 auf Commit `0d7eb4b` erstellt. Der Arbeitsbaum enthält fremde Änderungen an `.gitignore` und Projekttracking sowie das untracked `design-draft.zip`; diese Bestandsaufnahme ändert daran nichts.

Die vorhandenen Nachweise werden gemäß Auftrag wiederverwendet, ohne Routinechecks erneut auszuführen:

- [iteration2-checks.md](../../../../help/fahrplanauskunft/verification/iteration2-checks.md): Windows-Releasebuild Exitcode 0, 0 Fehler/0 Warnungen, Format/XML-Prüfung sowie .NET-Tests 98/98 erfolgreich und 98% Core-Coverage (541/552 Zeilen).
- [run-tests-2026-09-09.md](../../../../help/fahrplanauskunft/verification/run-tests-2026-09-09.md): Node-Release-Testlauf 26/26 erfolgreich.

## Relevante vorhandene Tests

- `FlowNRW.Tests/ProviderOrchestratorTests_Fallback.cs`: regionale Priorität, nationale Nutzung außerhalb NRW, leere/fehlerhafte Primärantworten und Fallbackstatus.
- `FlowNRW.Tests/ProviderOrchestratorTests_Concurrency.cs`: gemeinsame Requests, Cancellation und stale Cache-Fallback.
- `FlowNRW.Tests/RealtimeConsolidatorTests_Identity.cs`: DHID-/Provider-ID-Zuordnung, Fahrtkonflikte, Mehrdeutigkeit und Name/Entfernung.
- `FlowNRW.Tests/RoutingServiceTests_NextJourneys.cs`: zeitliche Auswahl der nächsten Journey.

## Abdeckungslücke

Es fehlt ein deterministischer Rot-Grün-Fall, in dem regionale A mit Warnung und nationale A+B zu A+B führt. Dasselbe gilt für Abfahrten mit zusätzlichem nationalem Event. Ergänzend müssen Deduplizierung, eindeutige Fahrtidentität, zeitliche Sortierung, `MaxResults` und Erhalt der bestehenden Fehler-/Fallbackantworten in diesen beiden Mergepfaden geprüft werden. Tests wurden in dieser Inventur nicht geändert oder ausgeführt.

