# Abnahmeprüfung – Entwicklungsschritt 2

## Ergebnis

**Status:** Anforderung vollständig erfüllt

Prüfstand: Produktcommit `7adb9bca9cb07f8027cf22bb833893d197e55be6`, verglichen mit dem Projekt-Basisbranch. Maßgeblich ist die Schrittanforderung einschließlich des ausdrücklichen Nutzerwunsches vom 16.09.2026: lokale IIS-Präsentation und zugehörige Paketbereitstellung entfallen.

## Abweichungen

Keine verbleibenden Abweichungen zum aktuellen Umfang.

## Abgleich

| Kriterium | Geprüfte Umsetzung und Nachweise |
|---|---|
| 1 – Fachlicher Einstieg und Plattformbasis | Counter/MainPage und Counterlogik entfernt; `SearchPage`, `ResultsPage`, `JourneyDetailPage` mit DI/Shell angebunden. iOS-Target, AppDelegate, Program und Info.plist vorhanden, Windows-Eigenschaften konditioniert. Windowsbuild erfolgreich, Visual-Studio-/iOS-Anleitung vorhanden. |
| 2 – Manuelle Endpunkte und Zustände | `EndpointViewModel` erhält vollständige Address-/Stop-Identität, validiert Koordinaten und verwendet getrennte Suchdienste. Native Tests bedienen beide Eingabearten und Koordinaten für beide Felder, Mehrdeutigkeit, Fehler und Invalidierung. Revisionsschutz ist auf Modell- und nativer UI-Ebene geprüft; keine Standortabfrage. |
| 3 – Ergebnisse und Details | `JourneyPresentation` und native Seiten zeigen Linien, Betreiber, Fußwege, Umstiege, Soll/Ist, unbekannte Werte, Ausfall, Quelle, Datenalter und Cachezustand. Deutsche Ortszeit mit Datum/Offset; Mitternacht und Ausfälle im Fixture-Test, echte Details im Live-Lauf. Schmalansicht nach WordWrap-Korrektur visuell geprüft. |
| 4 – Reale Provideranbindung | Reguläre DI verwendet unveränderten abgenommenen Providerkern mit NRW-Priorität, konservativer Identitätszuordnung und nationaler Ergänzung. Bestehende Transit-Tests bleiben enthalten. Reale native NRW- und Berlin–Hamburg-Suche am 16.09. erfolgreich; Ausfall der nationalen Quelle und EFA-Fallback werden transparent dokumentiert. |
| 5 – Native Tests und Dokumentation | 118 erfolgreiche Coretests, 715/737 Zeilen = 97,01 %; vollständiger nativer Windows-Fixturelauf einschließlich alter Routingantwort, Fehlererholung und Rücknavigation erfolgreich. Releasebuild 0 Warnungen/Fehler, Solution-Format und XML-Dokucheck erfolgreich. Hilfe, Konfiguration/Datenschutzverweise und konkrete manuelle iOS-Prüfliste vorhanden. Keine Änderungen unter `.github`. |

## Hinweise

Nachweise: `docs/help/verbindungssuche/verification/checks-2026-09-16.md`, Rohcoverage, nativer UI-Testlog, Vorher-/Nachher-Schmalbilder und `native-live-2026-09-16.md`. Planung und unabhängige Plan-/Usability-/Code-Reviews sind in Commit `25d6f30` archiviert. Native iOS-Ausführung verbleibt vereinbarungsgemäß beim Nutzer und wird nicht als bestanden behauptet.

Die separate Projektabnahme wurde delegiert, der Agent brach jedoch am Nutzungslimit vor einem Ergebnis ab. Der Hauptagent führte den fachlichen Abgleich gemäß dokumentiertem Skill-Fallback lokal aus, einschließlich direkter Quellprüfung und eigener tatsächlicher nativer Live-Bedienung. Diese abschließende Projektabnahme wird daher ausdrücklich nicht als unabhängiges Unteragentenreview bezeichnet. Die zuvor unabhängigen drei Lifecycle-Reviews sind vorhanden und ohne Befunde.

Die entfallene IIS-Lieferung ist weder als erfolgreich durchgeführt noch als offene Produktabweichung erfasst. Der begonnene lokale Publish war erfolgreich, wurde aber nicht auf IIS installiert und wird nicht weiterverfolgt. Keine neue Veröffentlichung, iOS-CI oder Deploymentautomatisierung.
