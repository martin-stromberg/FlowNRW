# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler (ein Abnahmeszenario `Nicht durchführbar` — Eskalation an den Anwender über Issue-Kommentar)

## Fehlgeschlagene Tests

Keine.

## E2E-Abdeckung

| Szenario | Test / Testklasse | Ergebnis |
|----------|-------------------|----------|
| Cron-Lauf erzeugt Issue mit Erwähnung und Typ-Label | Reale Abnahme auf GitHub (post-merge/post-promotion) | Nicht durchführbar — siehe unten |
| Lauf bei vorhandenem offenem Issue erzeugt kein Duplikat | `gh issue list`-Queries (real, read-only) + post-merge Beobachtung | Teilweise bestanden (Query-Logik real verifiziert) |
| `gh issue list`-Abfrage liefert valide Ergebnisse | Lokaler `gh`-Lauf gegen `martin-stromberg/FlowNRW` | Bestanden |

## Reale End-to-End-Abnahme

Die Anforderung berührt einen externen Dienst (GitHub Issues API) — reale
Abnahme grundsätzlich erforderlich.

| Szenario | Durchgeführt über | Ergebnis |
|----------|-------------------|----------|
| Happy Path: Cron/Dispatch erzeugt Issue | Geplant: `workflow_dispatch` bzw. erster Cron-Lauf auf `main` | Nicht durchführbar — `schedule` und `workflow_dispatch` wirken erst, wenn die Workflow-Datei auf dem Default-Branch (`main`) liegt; vor Merge/Promotion technisch nicht auslösbar. Eskalation an den Anwender per Issue-Kommentar (manuelle Abnahme oder dokumentierter Verzicht) |
| Duplikat-Abfrage (Label + Titel-Fallback) | Vier reale `gh issue list`-Aufrufe gegen das Repository (read-only) | Bestanden — valide JSON-Ergebnisse (`[]`, derzeit keine offenen Review-Issues) |
| YAML- und Skript-Validität | `yaml.safe_load` auf allen neuen Dateien; PowerShell-Parser auf dem `run`-Block der Composite Action | Bestanden |

Begründung „Nicht durchführbar" (Happy Path): GitHub führt `schedule`-Trigger
nur auf dem Default-Branch aus und registriert `workflow_dispatch` ebenfalls
erst dort. Der früheste reale Nachweis ist nach dem Merge der Promotion
`staging` → `main` möglich (manueller Dispatch oder der nächste fällige
Cron-Lauf, z. B. die wöchentliche NuGet-Prüfung am folgenden Montag).
Entscheidung beim Anwender: manuelle Abnahme nach Promotion oder
dokumentierter Verzicht — angefragt als Kommentar in Issue #20.

## Zusammenfassung

- Gesamt: 300
- Bestanden: 300
- Fehlgeschlagen: 0
- Übersprungen: 0

Befehle (jeweils im Repo-Root):
`dotnet build FlowNRW.sln --nologo -v q` → 0 Warnungen, 0 Fehler;
`dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj --no-build` → 300/300.

Zusätzlich: YAML-Parse von `review-architecture-security.yml`,
`review-nuget-weekly.yml`, `review-compliance-yearly.yml` und
`.github/actions/create-review-issue/action.yml` erfolgreich
(Trigger-, Permissions- und Step-Felder geprüft); PowerShell-Syntaxprüfung des
`run`-Blocks ohne Fehler; vier reale `gh issue list`-Queries erfolgreich.

## Testabdeckung

**Abdeckung:** Nicht messbar — der Diff enthält keine C#-Quelldateien
(ausschließlich `.github/**` und `docs/**`); die `FlowNRW.Core`-Abdeckung ist
durch die Änderung unverändert (Baselinemessung siehe `inventory/tests.md`).

## Fehlende Tests

Keine — für die neuen Dateien existiert kein passendes Testmedium im Projekt
(reine Workflow-YAML/Markdown); die gegebenenfalls anwendbaren Prüfungen
(YAML-Parse, pwsh-Syntax, reale API-Query) wurden durchgeführt.
