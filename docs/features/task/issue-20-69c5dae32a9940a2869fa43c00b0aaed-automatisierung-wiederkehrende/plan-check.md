# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| Für jede Prüfung existiert eine separate GitHub Action | Drei Workflow-Dateien (Umsetzungsreihenfolge 1–3) | YAML-Parse prüft Existenz und Triggerfelder | Abgedeckt |
| Ausführung zeitgesteuert über Cron-Trigger | `schedule` je Workflow: `0 5 1 * *`, `0 5 * * 1`, `0 5 1 1 *` | YAML-Parse + Cron-Feldprüfung | Abgedeckt |
| Erzeugte Issues verwenden aussagekräftige Titel | Präfix + Zeitraum-Suffix je Typ | Reale Abnahme post-merge; Titel-Regel als Validierung | Abgedeckt |
| `@softwarewerkstatt` wird automatisch im Issue erwähnt | Erste Zeile jedes Issue-Bodys | Reale Abnahme post-merge; Validierungsregel im Plan | Abgedeckt |
| Offene Issues desselben Typs werden vor Erstellung geprüft | Label-Check primär, Titel-Präfix-Fallback | Reale `gh issue list`-Query vor Merge (read-only) + Szenario 2/4 | Abgedeckt |
| Lösung vollständig innerhalb des Repositories dokumentiert | Schritt 5: `docs/CI-CD.md`, `docs/help/`, `changes.log`, `RELEASE_NOTES.md` | Review-Artefakte + Diff | Abgedeckt |

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Cron-Lauf erzeugt Issue (Happy Path) | Reale Abnahme auf GitHub nach Merge/Promotion auf `main` | Lücke vor Merge — technisch nicht früher möglich; als `Nicht durchführbar`-Szenario mit Anwender-Entscheidung vorgesehen |
| Duplikat-Vermeidung | Lokaler realer `gh issue list`-Lauf (read-only) + post-merge Beobachtung | Teilabgedeckt vor Merge (Query real verifizierbar) |
| App-seitiger Benutzerfluss | Nicht erforderlich — Anforderung berührt keine App-UI | Nicht erforderlich mit Begründung |

## Hinweise

- Der Plan markiert die Plattform-Abnahme (Cron → Issue) bewusst als
  post-merge/post-promotion-Szenario; die Eskalation an den Anwender erfolgt
  über einen Issue-Kommentar gemäß Lifecycle-Schritt 10.
- Scope-Klärung ist vollständig gefüllt; keine Scope-Ausschlüsse, keine offenen
  Punkte vorhanden.
