# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

Die zweite unabhängige Testphase wurde anhand des dauerhaften Nachweises [iteration2-checks.md](../../../help/fahrplanauskunft/verification/iteration2-checks.md) übernommen. Die vorherige Fassung ist als `test-results.1.md` archiviert. Der Nachweis stammt vom 9. September 2026 und passt zum aktuellen Transit-Quellfingerprint.

## Fehlgeschlagene Tests

Keine.

## Rot-Grün-Regressionen

Alle neun zuvor roten Regressionstests bestanden nach der Korrektur:

- 2 Tests für Body-Timeout/IOException nach HTTP-200-Headern; je zwei Versuche bei `MaxRetries=1`.
- 3 Tests für ungültige Folgeeingaben, die ältere Search-/Route-/Departure-Anfragen ablösen.
- 4 Tests für NRW-Namen-/ID-Auflösung, Providerwahl und `region-unknown` bei ungeklärter Region.

Diese 9 Fälle sind keine verbliebenen Testfehler.

## E2E-Abdeckung

Für Schritt 1 sind keine nativen E2E-Szenarien geplant: Der Schritt enthält keine fachliche UI. Die Abnahme basiert auf Core-/Provider-Tests und den bereits dokumentierten Serviceproben.

## Zusammenfassung

| Lauf | Gesamt | Bestanden | Fehlgeschlagen | Übersprungen | Exitcode | Herkunft |
|------|--------|-----------|---------------|-------------|----------|----------|
| Korrekturrunde 2: vollständige Core-Tests | 98 | 98 | 0 | 0 | 0 | [iteration2-checks.md](../../../help/fahrplanauskunft/verification/iteration2-checks.md) |
| Windows-Solution-Build | — | — | — | — | 0 | 0 Warnungen, 0 Fehler; gleicher Nachweis |
| Core-Format | — | — | — | — | 0 | gleicher Nachweis |
| Test-Format | — | — | — | — | 0 | gleicher Nachweis |
| XML-Dokumentationshook | 76 Dateien | 76 | 0 | 0 | 0 | gleicher Nachweis |
| Release-Scripts | 26 | 26 | 0 | 0 | 0 | Unverändert aus erster Testphase; `test-results.1.md` |

## Testabdeckung

**Abdeckung:** 98,00 % (541/552 Core-Zeilen)

Quelle: [coverage-iteration2.cobertura.xml](../../../help/fahrplanauskunft/verification/coverage-iteration2.cobertura.xml), ohne Include-/Exclude-Filter; einziges Paket `FlowNRW.Core`. Die geforderte Core-Abdeckung von mindestens 70 % ist erfüllt.

| Datei | Abdeckung |
|-------|-----------|
| `FlowNRW.Core/Transit/EfaProvider.cs` | 60 % |

## Fehlende Tests

Quelle: `Coverage-Daten`.

- `FlowNRW.Core/Transit/EfaProvider.cs` — einzelne Providerpfade bleiben unter 80 %; die Gesamt-Coverage erfüllt die Projektschwelle.

## Provenienz und Fingerprint

- Repository: `D:\Repositories\softwareschmiede\5142e36d-e8e0-4663-82eb-99c1599a0b31`
- Branch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft`
- HEAD: `1a106a57e748e4cc72fdb38bee9e5442a25d83c8`
- Umgebung: Windows, .NET SDK `10.0.400`.
- SHA-256-Aggregat der 57 Transit-Core-/Transit-Testquelldateien: `67005F44924A6A57A6B9B900424A9CF2A39DEF2CAA7CD28CB138851A497D9F38`.
- Detaillierter Nachweis: [iteration2-checks.md](../../../help/fahrplanauskunft/verification/iteration2-checks.md).
