# Code-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`,
**dritte Iteration** nach den Fixes zu `review-code.2.md`. Basisbranch `staging`.
Die Implementierungsartefakte (`site/`, `.github/workflows/pages.yml`) sind ungetracked
und wurden vollständig gelesen; die Doku-Dateien wurden als Diff gegen `staging` geprüft.
Die Screenshot-Galerie wurde seit Iteration 2 neu kuratiert (4 statt 9 Motive) und
daher vollständig erneut verifiziert.

## Ergebnis

**Status:** Befunde vorhanden

Der eine Befund aus Iteration 2 ist behoben (Nachweis unten); alle Implementierungs-
Prüfpunkte bestehen erneut. Verbleibend sind zwei veraltete Prüfwerte im Testprotokoll
`test-results.md` — gleiche Kategorie wie der Iteration-2-Befund (dokumentierte
Prüfung, die gegen den aktuellen Stand nicht mehr zutrifft), diesmal in anderen Zeilen.

## Befunde

### docs/features/task/…/test-results.md

- **Veraltete/inkonsistente Dokumentationsangabe** — Zwei protokollierte Prüfwerte
  stimmen nicht mehr mit der aktuellen Implementierung überein:
  - `test-results.md:33` dokumentiert die Actions-Reihenfolge als
    `configure-pages@v5` → `upload-pages-artifact@v4` → `deploy-pages@v4`. Der
    Workflow verwendet seit dem Iteration-1-Fix `configure-pages@v6`,
    `upload-pages-artifact@v5` und `deploy-pages@v5`
    (`.github/workflows/pages.yml:46,49,55`). Die Zeile behauptet „Bestanden" für
    eine Reihenfolge, die so nicht mehr existiert.
  - `test-results.md:46` dokumentiert „CSS-Klammerbilanz (79 Paare)". Die aktuelle
    `site/styles.css` enthält 80 Klammerpaare (verifiziert: 80× `{`, 80× `}`) —
    die Zahl driftete mit dem Iteration-1-Fix (Aufteilung der gemeinsamen
    `.card-grid, .screenshot-grid`-Regel in zwei Regeln).

  Empfehlung: Zeile 33 auf `configure-pages@v6` → `upload-pages-artifact@v5` →
  `deploy-pages@v5` aktualisieren und Zeile 46 auf 80 Paare nachziehen (oder
  Änderungsvermerk ergänzen), analog zur bereits erfolgten Korrektur der
  Release-Verweis-Zeile.

## Behobener Befund aus Iteration 2 (verifiziert)

- `changes.log:5` nennt jetzt „die plattformneutrale Release-Übersicht `releases`"
  — deckt sich mit der Implementierung (`site/index.html:49,123` →
  `https://github.com/martin-stromberg/FlowNRW/releases`).
- `test-results.md:69` dokumentiert jetzt „Release-Verweis plattformneutral auf
  die Release-Übersicht `releases`" — behoben.
- `docs/RELEASE_NOTES.md` (de/en) sagt „Release-Übersicht"/„release overview" —
  konsistent, wie in Iteration 2 empfohlen.

## Positiv festgehaltene Prüfpunkte (keine Befunde)

- **Anwenderfestlegung erfüllt:** `grep -riE "windows|win-x64|msix|\.exe|installer"
  site/` → 0 Treffer; iOS ist die einzige genannte Zielplattform mit Chip
  „In Entwicklung" (`index.html:124–133`); Release-Verweis plattformneutral auf
  die `releases`-Übersicht. Windows-Nennungen in `docs/CI-CD.md` betreffen die
  Maintainer-Doku bzw. den existierenden Pfad
  `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1` — zulässig.
- **Workflow-Syntax/Berechtigungen:** YAML-Parse bestanden; `"on"` gequotet;
  `permissions` minimal (`contents: read`, `pages: write`, `id-token: write`);
  `concurrency: group pages, cancel-in-progress: false`; `environment:
  github-pages` mit `page_url`-Output; `timeout-minutes: 10`; Trigger `push` auf
  `main` mit `paths`-Filter plus `workflow_dispatch`; Step-Kette korrekt.
  Action-Majors `checkout@v7`, `configure-pages@v6`, `upload-pages-artifact@v5`,
  `deploy-pages@v5` — konsistent zu den übrigen Repo-Workflows
  (Vergleich per `grep "uses: actions/"`: durchgehend v5–v7).
- **Smoke-Check robust:** `curl --fail --retry 5 --retry-delay 10
  --retry-all-errors` plus `grep -q "FlowNRW"` lässt den Schritt bei HTTP- oder
  Marker-Fehler korrekt fehlschlagen (bash-`-e`-Default).
- **Relative Pfade/Base-Pfad:** Sämtliche internen Referenzen relativ
  (`./styles.css`, `./theme.js`, `./assets/…`, Font-URLs `./assets/fonts/…`) —
  funktioniert unter `/FlowNRW/`; keine Root-Pfade vorhanden.
- **Asset-Referenzen (nach Neukuratierung erneut vermessen):** alle 4
  referenzierten Screenshots existieren, je 430×900 px (PNG-Header) und
  byteidentische Kopien aus `docs/help/design/verification/icon-matrix/`
  (md5-Abgleich: `search-form` + `settings-success-keyboard`, je hell/dunkel);
  `flownrw.png` 657×619, deklarierte Maße 32×30 / 220×207 seitenverhältnis-
  korrekt; alle 4 Fonts existieren; alle Linkziele im Repo vorhanden
  (`LICENSE`, `docs/help/index.md`, `docs/`).
- **Sichere externe Links:** ausschließlich `https://` auf
  `github.com/martin-stromberg/FlowNRW` und `polyformproject.org`; kein
  `target="_blank"` → kein `rel="noopener"` nötig; keine externen
  Skripte/CDNs/Tracker. README-Badge nutzt korrekte shields.io-Escapes
  (`martin--stromberg`).
- **HTML/CSS/JS-Qualität:** `lang="de"`, saubere Heading-Hierarchie, semantische
  Struktur; alle 5 Ankerziele vorhanden; `alt`-Texte, `aria-label`/`aria-pressed`
  am Umschalter, `hidden`-Fallback als Progressive Enhancement;
  `loading="lazy"` in der Galerie; `prefers-reduced-motion` beachtet;
  `data-theme` nur bei gespeicherter Wahl, `matchMedia`-`change`-Listener mit
  `addListener`-Fallback; `localStorage` try/catch-gesichert. CSS-Klammerbilanz
  ausgeglichen (80/80), keine redundanten Regeln.
- **Dokumentation konsistent:** `docs/CI-CD.md` (Workflow-Tabellenzeile,
  Einrichtungspunkt 6 mit korrektem `gh api`-Befehl, Abschnitt „Projektseite"
  inkl. Screenshot-Kuratierungsweg, Fehlersuche-Eintrag), `README.md`-Badge und
  `site/`-Tabellenzeile sowie `changes.log`-/`RELEASE_NOTES.md`-Einträge decken
  sich mit der Implementierung. Hinweis ohne Befundcharakter: `plan.md` enthält
  weiterhin `releases/latest`-Formulierungen — Planungsdokument, Stand zum
  Planungszeitpunkt, vom Usability-Fix überholt; keine Nachpflege nötig.

## Geprüfte Dateien

- `.github/workflows/pages.yml`
- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/` (Asset-Referenzen, Bildmaße und Provenienz verifiziert:
  `flownrw.png` 657×619, 4 Screenshots je 430×900 als md5-identische Kopien aus
  `icon-matrix/`, 4 Font-Dateien)
- `README.md` (Diff gegen `staging`)
- `changes.log` (Diff gegen `staging`)
- `docs/CI-CD.md` (Diff gegen `staging`)
- `docs/RELEASE_NOTES.md` (Diff gegen `staging`)
- `docs/features/task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode/`
  (`requirement.md`, `test-results.md`, `…-tasks.md` — Konsistenz gegen
  Implementierung geprüft)
