# Übersetzte Anforderung: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

Quelle: Issue #20 `martin-stromberg/FlowNRW` + Issue-Kommentar
(„@softwarewerkstatt warte bitte eine Stunde und dann füge bitte diese Prüfung hinzu",
2026-11-14 22:12 UTC — die sichtbare Bereitstellung erfolgt daher nicht vor 23:12 UTC).

## Fachliche Zusammenfassung

Das Repository erhält drei zeitgesteuerte GitHub-Actions-Workflows, die automatisch
GitHub-Issues erzeugen. Jedes Issue erwähnt den Account `@softwarewerkstatt`, damit die
automatische Analyse- und Bearbeitungskette der Anwendung angestoßen wird. Vor dem
Anlegen prüft jeder Workflow, ob bereits ein offenes Issue desselben Typs existiert,
und verzichtet in diesem Fall auf eine Neuanlage (Duplikat-Schutz).

## Fachlicher Scope

- **Verarbeitungseinheit:** Ein wiederkehrender Prüfzyklus je Review-Typ
  (monatliches Architektur- und Sicherheitsreview, wöchentliche NuGet-Prüfung,
  jährliches Compliance- und Copyright-Review). Pro Zyklus wird genau ein
  GitHub-Issue erzeugt.
- **Auslösende Ereignisse:** Cron-Zeitplan des jeweiligen Workflows
  (`schedule`-Trigger); zusätzlich manueller Start über `workflow_dispatch`
  zur jederzeitigen Ausführung. Der Kommentar verlangt, dass die sichtbare
  Bereitstellung (Push/PR) frühestens eine Stunde nach dem Kommentar
  (23:12 UTC) erfolgt.
- **Erwartete Wirkung:** Zum geplanten Zeitpunkt erscheint im Issue-Tracker ein
  neues Issue mit aussagekräftigem Titel, dem geforderten Prüfauftrag im Body und
  der Erwähnung `@softwarewerkstatt`. Existiert bereits ein offenes Issue
  desselben Typs, bleibt der Tracker unverändert.
- **Umgang mit fehlenden Objekten:**
  - Offenes Issue desselben Typs fehlt → erzeugen.
  - Offenes Issue desselben Typs vorhanden → wiederverwenden (kein neues Issue).
  - Der Workflow selbst existiert nur als Datei im Repo und wird durch den Merge
    aktiviert — kein weiteres Objekt nötig.
- **Scope-Ausschlüsse:** Keine — die Anforderung beschreibt die Erzeugung der
  Issues vollständig; die inhaltliche Bearbeitung der erzeugten Issues ist nicht
  Teil dieser Anforderung.

## Betroffene Klassen und Komponenten

- Neue Workflow-Dateien unter `.github/workflows/` (drei Dateien, eine je Prüfung).
- Keine C#-Klassen, keine UI-Komponenten, keine Datenmodelländerungen.
- Dokumentation unter `docs/` (CI-CD.md, ggf. Hilfebereich), `changes.log`,
  `docs/RELEASE_NOTES.md`.
- Tests: keine neuen .NET-Tests möglich/nötig (reine Workflow-YAML); Nachweis über
  YAML-Validierung und realen Test der Duplikat-Abfrage gegen die GitHub-API.

## Implementierungsansatz

- Drei separate Workflow-Dateien nach dem Muster von `security-scan.yml`
  (Header-Kommentar, `schedule` + `workflow_dispatch`, `concurrency`-Block,
  `windows-latest`-Runner, `pwsh`-Shells).
- Issue-Erzeugung über die auf GitHub-Runnern vorinstallierte `gh`-CLI mit
  `GITHUB_TOKEN` (`permissions: issues: write` im Job).
- Duplikat-Prüfung über `gh issue list --search` auf offene Issues mit dem
  gleichen Titelmuster; nur bei leerem Ergebnis `gh issue create`.
- Issue-Texte wörtlich aus der Anforderung übernehmen, einschließlich der
  `@softwarewerkstatt`-Erwähnung und der jeweiligen Prüflisten.

## Konfiguration

Keine Anwendungskonfiguration nötig. Zeitpunkte sind als Cron-Ausdrücke in den
Workflow-Dateien fest verdrahtet (wie bei `security-scan.yml`).

## Offene Fragen

Keine. Der Kommentar „diese Prüfung" bezieht sich auf die im Issue beschriebenen
Prüfungen; das Issue verlangt alle drei Zyklen als Akzeptanzkriterium, daher werden
alle drei Workflows umgesetzt.
