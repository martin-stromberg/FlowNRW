# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan lückenhaft

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| 1 Routing A + A/B ergibt A+B mit regionaler Echtzeit für A | Routing-Union, eindeutige Leg-/Stop-/Fahrtidentität, Erhalt vollständiger Journey | Routing A+A/B, Correction-Tasks 1/4 | Abgedeckt |
| 2 Abfahrtsunion ohne doppelte passende Events | MergeEvents und Consolidator | Abfahrten A+A/B, Tasks 2/5 | Abgedeckt |
| 3 Keine falsche Zusammenführung fremder/mehrdeutiger Fahrten | DHID allein unzureichend, konservative Eindeutigkeit | Identity-/Duplikattests, Tasks 3/6 | Abgedeckt |
| 4 Zeitreihenfolge und MaxResults | Sortierung und Begrenzung beider Mergepfade | Sortierung/Limit für Routing und Abfahrten, Task 7 | Abgedeckt |
| 5 Bestehende Fallback-/Fehler-/Warnungssemantik | Execute-Vertrag bleibt erhalten | Bestehende Fallback-/Concurrencytests sowie Task 8 | Abgedeckt |
| 6 Deterministische Rot-Grün-Regressionen; Windows-/Core-/Releaseabläufe unverändert | Deterministische Tests vorgesehen, aber erst nach Implementierung | Kein geplanter roter Lauf vor der Korrektur | Lücke |

## Fehlende oder unvollständige Testanforderungen

- [ ] Kriterium 6 verlangt ausdrücklich Rot-Grün-Nachweise. Der Plan ordnet zuerst Implementierung und danach Regressionstests mit Voraussetzung „Unionlogik aus Schritt 1“ an. Die Routing- und Abfahrtsregression A+A/B muss zuerst auf dem fehlerhaften Bestand ausgeführt und als rot dokumentiert werden; anschließend Implementierung und grüner Nachlauf einschließlich Identität, Priorität, Sortierung/Limit und bestehender Fallbacks. Diese Nachweisfolge auch im Correction-Tracking festhalten.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Interne Datenunion, keine neue UI | Deterministische Core-/Orchestratorregressionen | Nicht erforderlich mit Begründung: Kein UI-Fluss wird eingeführt oder geändert. |

## Fehlende oder unvollständige Planbestandteile

- [ ] Umsetzungsreihenfolge für den geforderten Rot-Grün-Nachweis korrigieren; siehe Testanforderung. Keine weitere fachliche Planlücke festgestellt.

## Hinweise

Prüfbasis: neue Korrekturanforderung, inventory.md samt logic.md/models.md/tests.md, vorhandener Korrekturplan, separate correction-tasks.md und der AK3-Befund in acceptance-schritt-1.md. Die Prüfung bleibt auf die konservative Ergänzung partieller Routing-/Abfahrtsmengen begrenzt. Regionale A plus nationale A+B, regionale Echtzeitpriorität, Identitätsschutz, Sortierung/Limit und unveränderte Fallbacks sind fachlich ausreichend beschrieben. Weitere Provider, UI, neue Liveproben oder Produktentscheidungen sind nicht erforderlich. Keine Quellen, Pläne oder Tasks korrigiert.
