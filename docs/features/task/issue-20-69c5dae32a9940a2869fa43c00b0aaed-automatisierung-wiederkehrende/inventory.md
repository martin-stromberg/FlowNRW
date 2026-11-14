# Bestandsaufnahme: Automatisierung wiederkehrender Reviews (Issue #20)

Analysiert wurden die CI/CD-Struktur des Repos (`.github/workflows/`,
`.github/actions/`) sowie die Dokumentationskonventionen, bezogen auf die
Anforderung, drei zeitgesteuerte Issue-Erzeugungs-Workflows einzuführen.

## Zusammenfassung

- Es existieren bereits Automations-Workflows, die per `gh`-CLI GitHub-Objekte
  anlegen (`staging-to-main-promotion.yml`, `sync-staging-with-main.yml`) —
  einschließlich eines etablierten Duplikat-Musters (`gh ... list --state open`
  + Count-Prüfung) und Label-Anlage per `gh label create --force`.
- `security-scan.yml` zeigt das Muster für `schedule`-Cron-Trigger plus
  `workflow_dispatch` und `concurrency`-Block.
- Es gibt noch keinen Workflow, der Issues erzeugt; die Berechtigung
  `issues: write` wird bereits in zwei Workflows verwendet.
- Es existiert kein `src/`-Verzeichnis — produktiver Code liegt in `FlowNRW/`,
  `FlowNRW.Core/`; für diese Anforderung werden ausschließlich YAML-Dateien und
  Dokumentation angelegt, keine C#-Artefakte.
- Test-Ausgangszustand: Solution-Build 0 Warnungen/0 Fehler,
  `dotnet test FlowNRW.Tests` 300/300 bestanden — siehe
  [Tests](inventory/tests.md).

## Details

- [Workflows und CI-Konventionen](inventory/workflows.md)
- [Tests](inventory/tests.md)
