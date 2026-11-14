# Umsetzungsplan: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

## Übersicht

Drei neue GitHub-Actions-Workflows unter `.github/workflows/` erzeugen
zeitgesteuert (Cron) automatisch GitHub-Issues mit Erwähnung von
`@softwarewerkstatt`. Jeder Workflow prüft vor dem Anlegen, ob bereits ein
offenes Issue desselben Typs existiert, und verzichtet dann auf die Neuanlage.
Zusätzlich ist jeder Workflow manuell per `workflow_dispatch` auslösbar.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Ein wiederkehrender Prüfzyklus je Review-Typ (monatlich / wöchentlich / jährlich); pro Zyklus genau ein Issue |
| Auslösende Ereignisse | Cron-Zeitplan des Workflows; manuell `workflow_dispatch`. Sichtbare Bereitstellung (Push/PR) frühestens 23:12 UTC am 2026-11-14 gemäß Issue-Kommentar |
| Erwartete Wirkung | Neues Issue im Tracker mit aussagekräftigem Titel, Prüfauftrag im Body, `@softwarewerkstatt`-Erwähnung und Typ-Label — oder kein neues Issue bei vorhandenem offenem Vorgänger |
| Umgang mit fehlenden Objekten | Offenes Issue desselben Typs fehlt → erzeugen; vorhanden → wiederverwenden (kein neues Issue); Typ-Label fehlt → vor Verwendung per `gh label create --force` anlegen |

### Scope-Ausschlüsse

Keine.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|------------------|------------|
| Anzahl Workflow-Dateien | Drei separate Dateien | Akzeptanzkriterium „Für jede Prüfung existiert eine separate GitHub Action"; unabhängige Zeitpläne und Fehlerisolierung |
| Issue-Erzeugung | `gh`-CLI mit `GH_TOKEN: ${{ github.token }}` in `pwsh`-Steps | Entspricht dem etablierten Muster aus `staging-to-main-promotion.yml`/`sync-staging-with-main.yml`; keine neue Abhängigkeit |
| Duplikat-Erkennung | Typ-Label (`automated-review-*`) als primäres Kriterium, Titel-Präfix-Suche als Fallback | Label überlebt Titeländerungen; Titel-Suche fängt manuell/ohne Label angelegte Vorgänger ab; beides auf dem vorhandenen `gh ... list --state open`-Muster aufbauend |
| Runner | `ubuntu-latest` | Reine GitHub-API-Automatisierung, kein Build nötig — wie Promotion-/Backmerge-Workflows |
| `concurrency` | Eine Gruppe je Workflow, `cancel-in-progress: false` | Kein Lauf darf verworfen werden (sonst könnte ein Zyklus ausfallen) — gleiche Begründung wie bei den Automations-Workflows |
| Berechtigungen | `issues: write` auf Workflow-Ebene | Minimale nötige Berechtigung; kein Checkout erforderlich |
| Cron-Zeitpunkte | Monatlich `0 5 1 * *`; wöchentlich `0 5 * * 1` (Mo); jährlich `0 5 1 1 *` | „Erster Tag des Monats"/„wöchentlich"/„1. Januar" wörtlich; 05:00 UTC versetzt zum bestehenden Security-Scan (Mo 04:00 UTC) |
| Issue-Titel | Stabiler deutscher Präfix plus Zeitraum-Suffix (`– YYYY-MM` / `– YYYY-MM-DD` / `– YYYY`) | Aussagekräftig und pro Zyklus eindeutig; der Präfix bleibt für die Duplikat-Suche stabil |
| Issue-Body | Wörtlich aus der Anforderung als pwsh-Here-String im Workflow | Vollständig im Repo dokumentiert, keine externe Datei nötig |

## Programmabläufe

### Zyklische Issue-Erzeugung (alle drei Workflows identisch aufgebaut)

1. Cron-Trigger (oder manueller Dispatch) startet den Job auf `ubuntu-latest`.
2. Step „Ensure label exists": `gh label create <typ-label> --force` legt das
   Typ-Label idempotent an.
3. Step „Create issue": `gh issue list --state open --label <typ-label>` liefert
   offene Issues desselben Typs; ist das Ergebnis leer, wird zusätzlich
   `gh issue list --state open --search 'in:title "<präfix>"'` geprüft.
4. Bei Treffer: Hinweis ins Log („bereits vorhanden"), Workflow endet ohne
   Änderung. Bei keinem Treffer: `gh issue create --title "<präfix> – <zeitraum>"
   --label <typ-label> --body <prüfauftrag>` erzeugt das Issue.
5. Sichtbare Wirkung: offenes Issue mit `@softwarewerkstatt`-Erwähnung im Body.

Beteiligte Komponenten: Workflow-Dateien `review-architecture-security.yml`,
`review-nuget-weekly.yml`, `review-compliance-yearly.yml`; GitHub Issues API via
`gh`.

## Neue Klassen

Keine — es werden ausschließlich YAML-Workflows und Dokumentation angelegt.

Neue Dateien:

| Datei | Zweck |
|-------|-------|
| `.github/workflows/review-architecture-security.yml` | Monatliches Architektur- und Sicherheitsreview (`0 5 1 * *`), Label `automated-review-architecture`, Titel-Präfix `Architektur- und Sicherheitsreview` |
| `.github/workflows/review-nuget-weekly.yml` | Wöchentliche NuGet-Prüfung (`0 5 * * 1`), Label `automated-review-nuget`, Titel-Präfix `NuGet-Prüfung` |
| `.github/workflows/review-compliance-yearly.yml` | Jährliches Compliance- und Copyright-Review (`0 5 1 1 *`), Label `automated-review-compliance`, Titel-Präfix `Compliance- und Copyright-Review` |

## Änderungen an bestehenden Klassen

Keine. Dokumentationsänderungen: `docs/CI-CD.md` (Workflow-Tabelle),
`docs/help/` (neuer Bereich), `changes.log`, `docs/RELEASE_NOTES.md`, ggf.
`README.md` (Projektstruktur-Hinweis).

## Datenbankmigrationen

Keine.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| Cron-Ausdruck | Gültiger 5-Felder-Ausdruck, entspricht gefordertem Intervall | Workflow erscheint nicht bzw. läuft zum falschen Zeitpunkt |
| Issue-Titel | Stabiler Präfix bleibt über Zyklen konstant | Duplikat-Suche verfehlt Vorgänger → Duplikate |
| Issue-Body | Enthält wörtlich `@softwarewerkstatt` und die jeweilige Prüfliste | Erwähnung fehlt → automatische Bearbeitung wird nicht angestoßen |

## Konfigurationsänderungen

Keine — Zeitpunkte und Texte stehen fest in den Workflow-Dateien (Konvention
wie `security-scan.yml`).

## Seiteneffekte und Risiken

- **Issue-Flut:** Fehlerhafte Duplikat-Logik könnte pro Lauf neue Issues
  erzeugen. Gegenmaßnahme: Label- und Titelprüfung getrennt; Lauf real gegen
  die GitHub-API getestet (Read-only-Abfrage lokal verifiziert).
- **Berechtigungen:** `GITHUB_TOKEN` braucht `issues: write`; ist auf
  Workflow-Ebene gesetzt.
- **Schedule-Aktivierung:** `schedule` feuert nur auf dem Default-Branch
  (`main`). Die Workflows werden erst nach Promotion `staging` → `main`
  aktiv; `workflow_dispatch` ebenfalls erst ab Vorhandensein auf `main`.
- Keine bekannten Seiteneffekte auf Code, Tests oder bestehende Workflows.

## Umsetzungsreihenfolge

1. **Workflow `review-architecture-security.yml` anlegen**
   - Voraussetzungen: Keine
   - Beschreibung: Datei nach dem Muster der Bestands-Workflows mit
     Header-Kommentar, `schedule`/`workflow_dispatch`, `permissions`,
     `concurrency`, Label-Step, Duplikat-Prüfung und Issue-Erzeugung
     (Body wörtlich aus der Anforderung).

2. **Workflow `review-nuget-weekly.yml` anlegen**
   - Voraussetzungen: Keine (gleiches Muster wie Schritt 1)
   - Beschreibung: Wie Schritt 1 mit wöchentlichem Cron und NuGet-Body
     (inkl. PR-Anweisung für unbedenkliche Updates).

3. **Workflow `review-compliance-yearly.yml` anlegen**
   - Voraussetzungen: Keine (gleiches Muster wie Schritt 1)
   - Beschreibung: Wie Schritt 1 mit jährlichem Cron und Compliance-Body.

4. **YAML- und Logik-Verifikation**
   - Voraussetzungen: Schritte 1–3
   - Beschreibung: Alle drei Dateien mit einem YAML-Parser laden; Cron-Felder
     gegen das geforderte Intervall prüfen; die Duplikat-Abfrage
     (`gh issue list --state open --label … --search …`) einmal real gegen
     das Repository ausführen (read-only).

5. **Dokumentation ergänzen**
   - Voraussetzungen: Schritte 1–4
   - Beschreibung: `docs/CI-CD.md` Workflow-Tabelle, neuer Bereich unter
     `docs/help/`, `changes.log`, `docs/RELEASE_NOTES.md`, README-Prüfung.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| YAML-Parse der drei Workflows | (Skript-Prüfung, kein Testprojekt) | Dateien sind valides YAML, `schedule`/`workflow_dispatch`/`permissions`/Steps vorhanden |
| Reale Duplikat-Abfrage | (manueller Lauf via `gh`) | `gh issue list --state open --label …` und `--search 'in:title …'` liefern valide Ergebnisse gegen `martin-stromberg/FlowNRW` |

Begründung: Die Änderung enthält keinen .NET-Code; `FlowNRW.Tests` kann
Workflow-YAML nicht abdecken. Bestehende Suite bleibt unverändert.

### Betroffene bestehende Tests

Keine.

### E2E-Tests (primärer Funktionsnachweis)

Die Anforderung betrifft keinen Anwendungs-Benutzerfluss (keine UI, keine
App-Interaktion); der „Benutzerfluss" ist ein GitHub-Plattformablauf
(Cron → Workflow → Issue). E2E im Sinne der Workflow-Engine ist vor dem
Merge technisch nicht ausführbar, da `schedule` und `workflow_dispatch`
nur auf dem Default-Branch (`main`) wirken. Als reale Abnahme im
Rahmen dieser Bearbeitung werden durchgeführt: YAML-Validierung aller drei
Dateien und ein realer Read-only-Lauf der Duplikat-Abfrage gegen das
Repository. Der abschließende Nachweis „Cron erzeugt Issue" ist erst nach
Promotion nach `main` möglich (nächster fälliger Lauf: wöchentliche
NuGet-Prüfung am folgenden Montag bzw. manueller Dispatch auf `main`) —
verbleibt als `Nicht durchführbar`-Szenario zur Entscheidung des Anwenders
(manuelle Abnahme nach Merge/Promotion oder dokumentierter Verzicht).

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Pflicht | Cron-Lauf erzeugt Issue mit Erwähnung und Typ-Label | Reale Abnahme auf GitHub (post-merge) | Zeitgesteuerte Ausführung, Issue-Inhalt, Erwähnung | Sichtbare Wirkung liegt auf der GitHub-Plattform, nicht im Code |
| Pflicht | Lauf bei vorhandenem offenem Issue erzeugt kein Duplikat | Reale Abnahme auf GitHub (post-merge) | Duplikat-Vermeidung | Nur auf Plattform prüfbar |
| Optional | `gh issue list`-Abfrage liefert valide Ergebnisse | Lokaler `gh`-Lauf (read-only) | Duplikat-Vermeidung (Anfragelogik) | Vorab-Verifikation der API-Interaktion |

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path monatlicher Lauf | Cron `0 5 1 * *` oder Dispatch | Kein offenes Review-Issue, Label existiert nicht | Label wird angelegt → Duplikat-Prüfung leer → `gh issue create` | Neues Issue „Architektur- und Sicherheitsreview – YYYY-MM" mit Body und `@softwarewerkstatt` | Reale Abnahme post-merge |
| 2 | Duplikat-Schutz | Cron oder Dispatch | Offenes Issue mit Typ-Label existiert | Duplikat-Prüfung findet Issue → Skip | Kein neues Issue; Log-Hinweis | Reale Abnahme post-merge + lokale Query-Verifikation |
| 3 | Wiederaufnahme nach Warten | Cron (nächster Zyklus) | Vorheriges Issue wurde geschlossen | Duplikat-Prüfung leer → Neuanlage | Neues Issue des nächsten Zyklus | Reale Abnahme post-merge |
| 4 | Label-Fallback | Cron oder Dispatch | Offenes Issue ohne Label, aber mit Titel-Präfix | Titel-Suche findet Issue → Skip | Kein neues Issue | Lokale Query-Verifikation + reale Abnahme post-merge |

## Offene Punkte

Keine.
