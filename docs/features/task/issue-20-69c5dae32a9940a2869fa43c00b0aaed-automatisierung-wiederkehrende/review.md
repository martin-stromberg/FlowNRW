# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] `.github/workflows/review-architecture-security.yml` — angelegt (Cron `0 5 1 * *`, `workflow_dispatch`, `permissions`, `concurrency`, Composite-Aufruf mit Label `automated-review-architecture`, Präfix `Architektur- und Sicherheitsreview`, Format `yyyy-MM`)
- [x] `.github/workflows/review-nuget-weekly.yml` — angelegt (Cron `0 5 * * 1`, Label `automated-review-nuget`, Präfix `NuGet-Prüfung`, Format `yyyy-MM-dd`)
- [x] `.github/workflows/review-compliance-yearly.yml` — angelegt (Cron `0 5 1 1 *`, Label `automated-review-compliance`, Präfix `Compliance- und Copyright-Review`, Format `yyyy`)
- [x] `.github/actions/create-review-issue/action.yml` — angelegt (Inputs `label`, `title-prefix`, `title-date-format`, `body-file`, `gh-token`; Steps Label-Anlage + Duplikat-Prüfung/Issue-Erzeugung)
- [x] `.github/review-bodies/architecture-security.md` — Body wörtlich aus der Anforderung inkl. `@softwarewerkstatt`
- [x] `.github/review-bodies/nuget-weekly.md` — Body wörtlich aus der Anforderung inkl. `@softwarewerkstatt` und PR-Anweisung
- [x] `.github/review-bodies/compliance-yearly.md` — Body wörtlich aus der Anforderung inkl. `@softwarewerkstatt`
- [x] Duplikat-Prüfung: Label-Query primär, `in:title`-Präfix-Suche als Fallback — in `action.yml` umgesetzt
- [x] YAML-Parse-Verifikation aller neuen YAML-Dateien — durchgeführt (python `yaml.safe_load`, alle valide)
- [x] Reale Duplikat-Abfrage via `gh issue list` — ausgeführt, vier Queries liefern valide `[]`-Ergebnisse
- [x] pwsh-Syntaxprüfung des Run-Blocks — `Parser::ParseFile` ohne Fehler
- [x] Dokumentation — siehe `docs/CI-CD.md`, `docs/help/automatisierte-reviews/`, `changes.log`, `docs/RELEASE_NOTES.md`

## Offene Aufgaben

Keine.

## Hinweise

- Die Plattform-Endwirkung (Cron erzeugt Issue) ist wie geplant erst nach
  Promotion auf `main` nachweisbar; als `Nicht durchführbar`-Szenario in
  `test-results.md` mit Eskalation an den Anwender dokumentiert.
