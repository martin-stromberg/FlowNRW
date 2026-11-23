# Code-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`,
**zweite Iteration** nach den Fixes zu `review-code.1.md`. Basisbranch `staging`;
Diff-Befehl `git diff --name-only --diff-filter=AM $(git merge-base HEAD staging)`
liefert nur verfolgte Dateien — die neuen, noch ungetrackten Artefakte (`site/`,
`.github/workflows/pages.yml`) wurden zusätzlich vollständig gelesen.

## Ergebnis

**Status:** Befunde vorhanden

Alle sieben Befunde aus Iteration 1 sind nachvollziehbar behoben (Nachweis unten).
Ein neuer, kleiner Befund verbleibt: eine veraltete Dokumentationsangabe zum
Release-Verweisziel.

## Befunde

### changes.log (+ docs/features/task/…/test-results.md)

- **Veraltete/inkonsistente Dokumentationsangabe** — `changes.log:6` behauptet:
  „der Release-Verweis bleibt der plattformneutrale `releases/latest`". Die
  Implementierung verlinkt seit dem Usability-Fix auf
  `https://github.com/martin-stromberg/FlowNRW/releases` (Übersicht), nicht auf
  `releases/latest` (`site/index.html:49,144`). Dasselbe gilt für den
  Testprotokoll-Eintrag `test-results.md:69` („Release-Verweis plattformneutral
  auf `releases/latest` — Bestanden"), der eine Prüfung dokumentiert, die gegen
  den aktuellen Stand nicht mehr zutrifft.

  Empfehlung: In `changes.log` das konkrete Linkziel anpassen (z. B.
  „plattformneutrale Verlinkung auf die Release-Übersicht `releases`") und in
  `test-results.md` die Zeile entsprechend nachtragen bzw. mit
  Änderungsvermerk versehen. `docs/RELEASE_NOTES.md` („latest release" /
  „aktuelles Release") ist unverbindlich genug formuliert und kann bleiben;
  optional ebenfalls auf „release overview"/„Release-Übersicht" präzisieren.

## Behobene Befunde aus Iteration 1 (verifiziert)

- `site/index.html:12` — `og:image` jetzt absolut
  (`https://martin-stromberg.github.io/FlowNRW/assets/flownrw.png`).
- `site/index.html:23,54` — `width`/`height` ans reale Seitenverhältnis
  657×619 angepasst (32×30 bzw. 220×207; Abweichung durch Integer-Rundung
  < 0,5 %, korrekt).
- `site/index.html:178–181` — Material-Symbols-Nennung aus dem Footer
  entfernt; nur noch Inter, JetBrains Mono (SIL OFL 1.1) und
  OpenStreetMap-Attribution.
- `site/index.html:13–14` — `theme-color` jetzt zweifach mit
  `media="(prefers-color-scheme: light|dark)"`.
- `site/styles.css:483–489` — redundante Grid-Regel entfernt; `.card-grid`
  (1fr) und `.screenshot-grid` (2 Spalten) getrennt deklariert.
- `site/theme.js:29–37,65–70,81–85` — `data-theme` wird nur bei gespeicherter
  Nutzerwahl gesetzt, sonst entfernt; zusätzlich `change`-Listener auf
  `matchMedia` (inkl. `addListener`-Fallback) — Systemthema-Wechsel greift
  jetzt live.
- `.github/workflows/pages.yml:46,49,55` — `configure-pages@v6`,
  `upload-pages-artifact@v5`, `deploy-pages@v5`; gegen die Release-Stände
  verifiziert (v6.0.0 / v5.0.0 / v5.0.1 — aktuelle Majors), konsistent zu den
  übrigen Repo-Workflows (`checkout@v7`, `setup-dotnet@v6` u. a.).

## Positiv festgehaltene Prüfpunkte (keine Befunde)

- **Anwenderfestlegung erfüllt:** `grep -ri "windows\|win-x64\|msix|\.exe|
  installer" site/` → 0 Treffer; iOS ist die einzige genannte Zielplattform
  mit Chip „In Entwicklung" (`index.html:145–154`); Release-Verweise sind
  plattformneutral. Windows-Nennungen in `docs/CI-CD.md`/`changes.log`
  betreffen Maintainer-Doku bzw. wörtliche Pfadnamen
  (`tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1` — Datei existiert).
- **Workflow-Syntax/Berechtigungen:** YAML-Parse bestanden; `"on"` gequotet;
  `permissions` minimal und vollständig (`contents: read`, `pages: write`,
  `id-token: write`); `concurrency: group pages, cancel-in-progress: false`
  deploy-sicher; `environment: github-pages` mit `page_url`-Output;
  `timeout-minutes: 10`; Trigger `push` auf `main` mit `paths`-Filter plus
  `workflow_dispatch`; Step-Kette `checkout → configure-pages →
  upload-pages-artifact (path: site) → deploy-pages (id: deployment)` korrekt.
- **Smoke-Check robust:** `curl --fail --retry 5 --retry-delay 10
  --retry-all-errors`, Marker-Prüfung per `grep -q "FlowNRW"` lässt den Schritt
  korrekt fehlschlagen (bash-Default `-e` bei fehlgeschlagenem grep).
- **Relative Pfade/Base-Pfad:** Sämtliche internen Referenzen relativ
  (`./styles.css`, `./theme.js`, `./assets/…`, Font-URLs `./assets/fonts/…`
  im CSS) — funktioniert unter Base-Pfad `/FlowNRW/`; keine
  Root-Pfade (`src="/…"`, `url("/…")`) vorhanden.
- **Asset-Referenzen:** Alle 9 referenzierten Screenshots existieren und sind
  430×900 px (PNG-Header vermessen — stimmt mit `width`/`height` überein);
  `flownrw.png` 657×619; alle 4 Fonts existieren. Neue Screenshot-Namen
  (`haltestellenliste-*`, `haltestellensuche-hell`) stimmen mit dem
  ersetzten kuratierten Satz überein.
- **Sichere externe Links:** Ausschließlich `https://` auf
  `github.com/martin-stromberg/FlowNRW` und `polyformproject.org`; kein
  `target="_blank"` → kein `rel="noopener"` nötig; keine externen
  Skripte/CDNs/Tracker.
- **HTML/CSS/JS-Qualität:** `lang="de"`, saubere Heading-Hierarchie und
  semantische Struktur; alle Ankerziele (`#features`, `#screenshots`,
  `#release`, `#links`, `#top`) vorhanden; `alt`-Texte, `aria-label`/
  `aria-pressed`/`aria-hidden` am Umschalter, `loading="lazy"` in der Galerie,
  `hidden`-Button als Progressive-Enhancement-Fallback,
  `prefers-reduced-motion` beachtet, `localStorage` try/catch-gesichert,
  `matchMedia`-Fallback (`addListener`) für ältere Browser.
- **Dokumentation konsistent:** `docs/CI-CD.md`-Ergänzungen
  (Workflow-Tabellenzeile, Einrichtungspunkt 6 mit korrektem
  `gh api`-Befehl, Abschnitt „Projektseite", Fehlersuche-Eintrag),
  `README.md`-Badge und `site/`-Tabellenzeile sowie der
  `docs/RELEASE_NOTES.md`-Eintrag (de/en) decken sich mit der Implementierung.

## Geprüfte Dateien

- `.github/workflows/pages.yml`
- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/` (Asset-Referenzen und Bildmaße verifiziert: `flownrw.png`
  657×619, 9 Screenshots je 430×900, 4 Font-Dateien)
- `README.md` (Diff gegen `staging`)
- `changes.log` (Diff gegen `staging`)
- `docs/CI-CD.md` (Diff gegen `staging`)
- `docs/RELEASE_NOTES.md` (Diff gegen `staging`)
- `docs/features/task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode/`
  (`requirement.md`, `test-results.md`, `review.md`, `review-usability.md`
  sowie `…-tasks.md` — Kontext/Anwenderfestlegungen, gesichtet)
