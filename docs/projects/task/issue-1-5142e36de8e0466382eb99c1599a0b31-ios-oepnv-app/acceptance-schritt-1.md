# Abnahmeprüfung – Entwicklungsschritt 1

## Ergebnis

**Status:** Anforderung vollständig erfüllt

Abnahmedurchlauf 2 am 2026-09-15. Geprüft ist der noch nicht committete Korrekturstand gegenüber `0d7eb4b` auf `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft`. Projekt-Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Der erste Bericht wurde als `acceptance-schritt-1.1.md` archiviert.

## Abweichungen

Keine verbleibenden Abweichungen aus der gezielt erneut geprüften AK-3-Korrektur. Die unveränderten übrigen Kriterien behalten die Quellenprüfung und Nachweise der ersten Abnahme; die identische ursprüngliche Schrittanforderung bleibt maßgeblich.

## Hinweise

Die unabhängige Nachprüfung umfasste den tatsächlichen Working-Tree-Diff von `ProviderOrchestrator`, die nun intern wiederverwendbaren Identitätsprädikate aus `RealtimeConsolidator`, die Verdrahtung des Ergebnislimits in `MauiProgram`, acht neue Fälle in `ProviderOrchestratorTests_Union` sowie fünf angepasste Testfälle der bisherigen Fallback-/Regionauflösungstests. Keine Quellenkorrektur, kein Commit, kein Branchwechsel und keine neue externe Recherche durch den Prüfer.

| Geprüftes Verhalten | Ergebnis |
|---|---|
| Regionale Fahrt A, bundesweit A und B | Routing und Abfahrten ergänzen B, statt sie zu verwerfen. |
| Eindeutig gleiche Fahrt A | `Unmatched` entfernt nur beidseitig eindeutige Doppelungen. Regionale Echtzeit hat weiterhin Vorrang; fehlende Werte können ergänzt werden. |
| Mehrdeutige oder fremde Fahrten | Kandidaten bleiben getrennt erhalten; keine erzwungene Zusammenführung. |
| Verbindungen mit mehreren Abschnitten | `SameJourney` prüft sämtliche Abschnitte und Fußweg-Endpunkte; ein gleicher erster Abschnitt reicht nicht zur Deduplizierung. |
| Struktur und Zusatzinformationen | Regionale Geometrie/Transfers bleiben erhalten; zusätzliche bundesweite Verbindungen werden unverändert aufgenommen. |
| Anreicherung | `EnrichJourney` arbeitet nur mit eindeutig zugeordneten ganzen Verbindungen; `EnrichEvent` erhält regionale Werte und ergänzt fehlende Angaben. |
| Reihenfolge und Begrenzung | Vereinigung wird zeitlich sortiert und auf das konfigurierte Ergebnislimit begrenzt; MAUI reicht die vorhandenen Provideroptionen weiter. |
| Regressionserwartungen | Angepasste bisherige Tests erwarten beide nicht eindeutig zuordenbaren Mengen und prüfen weiterhin regionale Auswahl. |

Der gelesene lokale Beleg `docs/help/fahrplanauskunft/verification/union-correction-green.txt` weist die beiden Kernregressionen mit 2 erfolgreichen Tests und ohne Fehler/Überspringungen aus. Der koordinierende Agent meldet für den korrigierten Gesamtstand **106 erfolgreiche Tests mit Coverage-Erfassung**; dies wird als übernommener Ausführungsnachweis, nicht als vom Prüfer erneut ausgeführter Testlauf behandelt. Er ergänzt den dauerhaften Gesamtbeleg. Der bisherige Stand mit 98 Tests, 98 % Core-Abdeckung und Windows-Build ohne Warnungen/Fehler bleibt historischer Nachweis der ersten Abnahme und wird nicht als aktueller Korrektur-Build ausgegeben. Format-/Buildprüfung und Abschlusscommit sichert der koordinierende Workflow für diesen Stand.

Bekannte Grenzen bleiben unverändert: db-rest-Ausfälle und die eingeschränkte EFA-Entwicklungsversorgung sind dokumentiert, punktuelle bundesweite Liveproben garantieren keine flächendeckende Produktionsversorgung. Schritt 1 enthält keine fachliche UI; native Windows-UI-Prüfungen folgen mit der UI. Native iOS-Abnahme liegt gemäß Nutzerentscheidung zunächst beim Nutzer. Diese Abnahme gilt für den geprüften semantischen Code; weitere semantische Änderungen machen eine betroffene Nachprüfung erforderlich.
