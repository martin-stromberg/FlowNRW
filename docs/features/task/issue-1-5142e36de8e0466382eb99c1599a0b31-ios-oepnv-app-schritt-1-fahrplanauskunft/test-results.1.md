# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

Der vorhandene vollständige .NET-Nachweis wurde aus `docs/help/fahrplanauskunft/verification/implementation-checks.md` übernommen. Er stammt vom 8. September 2026; die dort beschriebene finale Prüfung meldet 89 erfolgreiche, 0 fehlgeschlagene und 0 übersprungene Tests, einen vollständigen Windows-Solution-Build mit 0 Warnungen und 0 Fehlern sowie 94,12 % Core-Zeilenabdeckung. Die aktuelle Quelle wurde am 9. September 2026 per SHA-256-Fingerprint abgeglichen; der Nachweis wurde nicht durch einen redundanten identischen .NET-Lauf ersetzt.

Der zusätzlich ausgeführte Node-Release-Script-Test am 9. September 2026 bestand mit 26 erfolgreichen, 0 fehlgeschlagenen und 0 übersprungenen Tests (Exitcode 0). Die PowerShell-NPM-Wrapperzeile meldete eine Zugriffsberechtigungswarnung auf die globale npm-Datei; anschließend lief der aufgerufene Node-Test vollständig erfolgreich.

Die drei Review-Befunde zu Body-Timeout/IOException und Cancellation nach ungültiger neuer Eingabe sind keine fehlgeschlagenen Tests dieses Laufs. Sie bleiben als separate Code-/Regressionsthemen dokumentiert und werden nicht als 89 Testfehler ausgegeben.

## Fehlgeschlagene Tests

Keine.

## E2E-Abdeckung

Für Schritt 1 sind keine nativen E2E-Szenarien geplant: Der Schritt enthält ausdrücklich keine fachliche UI. Die fachlichen Nachweise sind die vorhandenen Serviceproben, deterministischen Core-/Provider-Tests und der Coverage-Bericht. Native UI-E2E beginnt in den abhängigen UI-Schritten.

## Zusammenfassung

| Lauf | Gesamt | Bestanden | Fehlgeschlagen | Übersprungen | Exitcode | Herkunft |
|------|--------|-----------|---------------|-------------|----------|----------|
| Finale Core-Tests einschließlich Coverage | 89 | 89 | 0 | 0 | 0 | Übernommen aus `verification/implementation-checks.md`, Lauf vom 08.09.2026 |
| Windows-Solution-Build | — | — | — | — | 0 | Übernommen aus `verification/implementation-checks.md`; 0 Warnungen, 0 Fehler |
| Release-Script-Test | 26 | 26 | 0 | 0 | 0 | Ausgeführt am 09.09.2026: `npm run test:release-version` |

## Testabdeckung

**Abdeckung:** 94,12 % (497/528 Core-Zeilen)

Quelle: [Cobertura-Rohreport](../../../help/fahrplanauskunft/verification/coverage.cobertura.xml), erzeugt im dokumentierten finalen Coverage-Lauf. Keine Include-/Exclude-Filter; einziges Paket `FlowNRW.Core`.

| Datei | Abdeckung |
|-------|-----------|
| `FlowNRW.Core/Transit/DepartureService.cs` | 64,28 % (Klassenabdeckung; ein generierter Async-Anteil 0 %) |
| `FlowNRW.Core/Transit/EfaProvider.cs` | 60 % |

Die projektweite Core-Abdeckung liegt damit über der geforderten Schwelle von 70 %. Die Tabelle nennt nur Klassen unter 80 % gemäß Lifecycle-Vorgabe.

## Fehlende Tests

Quelle: `Coverage-Daten`.

- `FlowNRW.Core/Transit/DepartureService.cs` — einzelne Async-/Fehlerpfade bleiben unter 80 %; der Gesamtwert erfüllt die Core-Schwelle.
- `FlowNRW.Core/Transit/EfaProvider.cs` — einzelne Providerpfade bleiben unter 80 %; der Gesamtwert erfüllt die Core-Schwelle.

## Provenienz und Code-Fingerprint

- Repository: `D:\Repositories\softwareschmiede\5142e36d-e8e0-4663-82eb-99c1599a0b31`
- Branch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft`
- HEAD zum Nachweis: `1a106a57e748e4cc72fdb38bee9e5442a25d83c8`
- Umgebung des übernommenen .NET-Laufs: Windows, .NET SDK `10.0.400`, `C:/Program Files/dotnet/dotnet.exe`.
- SHA-256-Fingerprint der 55 Transit-Core-/Transit-Testquelldateien am 09.09.2026: `45F9149C70D0D178BBF1C3BA42424EBD6ABB82AF8050A8A297BFBFB7411752C8`.
- Detaillierte Befehle und Rohreport: [Implementierungsnachweis](../../../help/fahrplanauskunft/verification/implementation-checks.md).
