# Tasks: Korrektur partieller Providerergebnisse

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---|---|---|---|
| 1 | Tests | Rote Regression für Routing A + A/B mit zusätzlicher Journey B und regionaler Echtzeit für A anlegen | Fertig | union-correction-checks.md |
| 2 | Tests | Rote Regression für Abfahrten A + A/B mit zusätzlichem nationalem Event anlegen | Fertig | union-correction-checks.md |
| 3 | Logik | `MergeJourneys` um konservative Union, eindeutige Fahrtidentität, Erhalt von Legs/Transfers/Geometrie, Sortierung und `MaxResults` ergänzen | Fertig | union-correction-checks.md |
| 4 | Logik | `MergeEvents` um Event-Union, regionale Echtzeitpriorität, Ambiguitätsbehandlung, Sortierung und `MaxResults` ergänzen | Fertig | union-correction-checks.md |
| 5 | Logik | `RealtimeConsolidator` so erweitern, dass eindeutige sekundäre Events erhalten bleiben und DHID allein keine Fahrt dedupliziert | Fertig | union-correction-checks.md |
| 6 | Tests | Grüne A/A+B-Regressionen sowie doppelte, fremde und mehrdeutige Fahrtidentitäten prüfen | Fertig | union-correction-checks.md |
| 7 | Tests | Zeitliche Sortierung und `MaxResults` für Routing und Abfahrten prüfen | Fertig | union-correction-checks.md |
| 8 | Tests | Leere, fehlerhafte und warnende Providerantworten sowie bestehende Fallbacksemantik regressionsprüfen | Fertig | union-correction-checks.md |
| 9 | Dokumentation | Bestehende Fahrplanauskunft-Help-Dokumente um die Union-/Identitätsregel und Grenzen ergänzen | Fertig | union-correction-checks.md |
| 10 | Dokumentation | Vorhandene README-/Release-Notes-Verweise zur Datenunion gezielt aktualisieren | Fertig | union-correction-checks.md |

