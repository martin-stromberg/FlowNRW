# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| 1 Routing A + A/B ergibt A+B mit regionaler Echtzeit für A | Konservative Routing-Union mit vollständigen Journeys | Rote Routingregression vor Korrektur, grüner Nachlauf | Abgedeckt |
| 2 Abfahrtsunion ohne doppelte passende Events | MergeEvents/Consolidator mit regionaler Priorität | Rote Abfahrtsregression vor Korrektur, grüner Nachlauf | Abgedeckt |
| 3 Keine Vermischung fremder/mehrdeutiger Fahrten | Eindeutige Stop-/Fahrtidentität, DHID allein unzureichend | Identity-, Duplikat- und Mehrdeutigkeitstests | Abgedeckt |
| 4 Zeitreihenfolge und MaxResults | Sortierung und Limit beider Mergepfade | Routing-/Abfahrtsgrenztests | Abgedeckt |
| 5 Bestehende Fallback-/Fehler-/Warnungssemantik | Execute-Verträge bleiben erhalten | Bestehende und gezielte Fallbackregressionen | Abgedeckt |
| 6 Deterministische Rot-Grün-Nachweise, bestehende Windows-/Core-/Releaseabläufe | Reihenfolge ausdrücklich Rot → Implementierung → Grün/Regressionen | Plan Schritte 1–3, correction-tasks 1/2 vor 3–5 und Nachlauf 6–8 | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Interne Routing-/Abfahrtsunion ohne UI-Änderung | Deterministische Core-/Orchestratorregressionen | Nicht erforderlich mit Begründung: Keine neue oder geänderte Benutzerinteraktion. |

## Fehlende oder unvollständige Planbestandteile

## Hinweise

Gezielte Nachprüfung des einzigen Restbefunds aus plan-check.1.md am 15. September 2026. Plan und separates Correction-Tracking schreiben jetzt zuerst rote Routing-/Abfahrtsregressionen auf dem fehlerhaften Bestand, anschließend Mergekorrektur und grüne Nachläufe einschließlich Identität, Priorität, Reihenfolge/Limit und Fallbacks vor. Der Befund ist geschlossen.

Die bereits erfolgte fachliche Prüfung gegen Korrekturanforderung, Inventur samt Details und AK3 aus acceptance-schritt-1.md bleibt Grundlage. Keine zusätzliche Planung, Recherche, Quelländerung oder Testausführung. Der Status bestätigt die Planung, nicht bereits erfolgte Implementierung oder bestandene Tests.
