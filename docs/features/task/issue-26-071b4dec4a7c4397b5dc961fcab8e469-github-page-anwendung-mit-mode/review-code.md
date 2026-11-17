# Code-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`,
**vierte (finale) Iteration** nach den Fixes zu `review-code.3.md` und den neuesten
Usability-Textkorrekturen (Hero-CTA „Releases ansehen", Feature-Karten ohne
„Statuschips"/„explizite Entfernungssortierung", Release-Panel „vorhandene
Desktop-Anwendung"). Basisbranch `staging`; die ungetrackten
Implementierungsartefakte (`site/`, `.github/workflows/pages.yml`) wurden
vollständig gelesen, die Doku-Dateien als Diff gegen `staging` geprüft.

## Ergebnis

**Status:** Befunde vorhanden

Alle Befunde aus Iteration 3 sind behoben (Nachweis unten); sämtliche
Implementierungs- und Konsistenzprüfungen wurden am aktuellen Stand erneut
durchgeführt. Ein verbleibender Layout-Randfall betrifft die Anker-Navigation
unter dem sticky Header.

## Befunde

### site/styles.css (+ site/index.html)

- **Randfall/Korrektheit (Layout)** — Fehlende Kompensation des sticky Headers
  bei der internen Anker-Navigation. `.site-header` ist `position: sticky` und
  misst ca. 58 px Höhe (Desktop) bzw. ca. 100 px im Breakpoint ≤ 640 px (dort
  bricht `.nav` per `flex-wrap` auf zwei Zeilen um: `nav-links` belegt eine
  eigene Zeile, `gap: 16px` als Row-Gap). Weder `scroll-margin-top` auf den
  Ankerzielen noch `scroll-padding-top` auf `html` ist gesetzt. Die Sektionen
  `#features`, `#screenshots`, `#release`, `#links` beginnen nur mit
  `padding: 48px 0` (`styles.css:309–311`): Beim Anspringen liegt der
  `h2`-Anfang bei 48 px — Desktop werden ~10 px der Überschrift vom Header
  verdeckt, mobil wird die komplette Überschrift (48 + ~32 px < ~100 px) vom
  umgebrochenen Header überdeckt. Bei `main#top` ist der Hero-`h1` mobil
  ebenfalls betroffen (`.hero`-Padding 40 px < 100 px Headerhöhe). Damit ist
  die Kopfnavigation auf kleinen Viewports funktional gebrochen: Der Nutzer
  landet auf einem Abschnitt, dessen Titel unsichtbar hinter der Navigation
  liegt.

  Empfehlung: `scroll-padding-top` auf `html` oder `scroll-margin-top` auf den
  Ankerzielen ergänzen, z. B. `html { scroll-padding-top: 72px; }` und im
  `@media (max-width: 640px)`-Block einen größeren Wert (~116 px) — dann
  springen die Überschriften frei unter dem sticky Header an.

## Behobene Befunde aus Iteration 3 (verifiziert)

- `test-results.md:33` dokumentiert jetzt die Actions-Reihenfolge
  `checkout@v7` → `configure-pages@v6` → `upload-pages-artifact@v5` →
  `deploy-pages@v5` — deckt sich mit `.github/workflows/pages.yml:43,46,49,55`.
- `test-results.md:46` dokumentiert „CSS-Klammerbilanz (80 Paare)" —
  verifiziert: `site/styles.css` enthält exakt 80× `{` und 80× `}`.

## Neueste Textkorrekturen (verifiziert)

- Hero-CTA „Releases ansehen" (`index.html:49`), identischer CTA im
  Release-Panel (`index.html:124`) — Usability-Befund „Hero-Button ‚Releases‘
  für Laien unklar" behoben.
- Feature-Karten ohne „Statuschips"/„explizite Entfernungssortierung"
  (`grep -riE "statuschip|entfernungssort" site/` → 0 Treffer): Abfahrtsmonitor
  nennt Verspätungen/Ausfälle/Gleiswechsel direkt (`index.html:69`), Favoriten
  „nach Entfernung sortierbar" (`index.html:73`), NRW-Echtzeitpriorität
  „Ersatzdaten" statt „Fallbacks" (`index.html:85`), Haltestellenkarte
  „Kartenverläufe zu gefundenen Verbindungen" statt „gelieferten
  Verbindungsverläufen" (`index.html:77`).
- Release-Panel nennt jetzt „Die aktuellen Vorabversionen für die vorhandene
  Desktop-Anwendung" (`index.html:120–123`) — schließt die Usability-Lücke
  „Plattform der Vorabversionen unklar" ohne Windows-Nennung.
- `test-results.md` wurde um eine Checkzeile zu diesen Textkorrekturen ergänzt
  (Abschnitt 4); die Tasks-Datei enthält keine veralteten Formulierungen und
  spiegelt den Stand bereits wider.

## Positiv festgehaltene Prüfpunkte (keine Befunde)

- **Anwenderfestlegung erfüllt:** `grep -riE "windows|win-x64|msix|\.exe|
  installer" site/` → 0 Treffer; iOS ist die einzige genannte Zielplattform
  (Chip „In Entwicklung", `index.html:127–128`); Release-Verweis
  plattformneutral auf die `releases`-Übersicht (`index.html:49,124`).
- **Workflow `pages.yml`:** YAML-Parse bestanden; `"on"` gequotet;
  `permissions` minimal (`contents: read`, `pages: write`, `id-token: write`);
  `concurrency: group pages, cancel-in-progress: false`; `environment:
  github-pages` mit `page_url`-Output; `timeout-minutes: 10`; Trigger `push`
  auf `main` mit `paths`-Filter plus `workflow_dispatch`; Step-Kette
  `checkout@v7` → `configure-pages@v6` → `upload-pages-artifact@v5`
  (`path: site`) → `deploy-pages@v5` (`id: deployment`) → `curl`-Smoke-Check
  mit `--fail --retry 5 --retry-delay 10 --retry-all-errors` und
  `grep -q "FlowNRW"` (bash-`-e`-Default lässt den Schritt korrekt
  fehlschlagen).
- **Relative Pfade/Base-Pfad:** Sämtliche internen Referenzen relativ
  (`./styles.css`, `./theme.js`, `./assets/…`, Font-URLs `./assets/fonts/…`) —
  funktioniert unter `/FlowNRW/`; keine Root-Pfade.
- **Asset-Referenzen erneut vermessen:** 4 Screenshots je 430×900 px und
  md5-identische Kopien aus `docs/help/design/verification/icon-matrix/`
  (`light|dark-430-900-100-favorite-cache-seed-search-form` →
  `verbindungssuche-hell|-dunkel`, `…-settings-success-keyboard` →
  `aktualisierung-hell|-dunkel`); `flownrw.png` 657×619 und md5-identisch zu
  `FlowNRW/Resources/AppIcon/flownrw.png`; alle 4 Fonts md5-identisch zu
  `FlowNRW/Resources/Fonts/`; deklarierte `width`/`height` (32×30, 220×207)
  seitenverhältnis-korrekt; alle externen Linkziele im Repo vorhanden
  (`LICENSE`, `docs/`, `docs/help/index.md`).
- **Sichere externe Links:** ausschließlich `https://` auf
  `github.com/martin-stromberg/FlowNRW` und `polyformproject.org`; kein
  `target="_blank"` → kein `rel="noopener"` nötig; keine externen
  Skripte/CDNs/Tracker; `og:image`/`og:url` absolut.
- **HTML/CSS/JS-Qualität:** `lang="de"`, semantische Struktur, saubere
  Heading-Hierarchie; `alt`-Texte, `aria-label`/`aria-pressed`/`aria-hidden`
  am Umschalter; `hidden`-Button als Progressive-Enhancement-Fallback inkl.
  notwendiger `.theme-toggle[hidden]`-Regel (Spezifitäts-Override korrekt);
  `loading="lazy"` in der Galerie; `theme-color`-Metas hell/dunkel;
  `prefers-reduced-motion` beachtet; `localStorage` try/catch-gesichert;
  `data-theme` nur bei gespeicherter Wahl, `matchMedia`-`change`-Listener mit
  `addListener`-Fallback; keine toten Funktionen in `theme.js`.
- **Dokumentation konsistent:** `docs/CI-CD.md` (Workflow-Tabellenzeile,
  Einrichtungspunkt 6 mit korrektem `gh api`-Befehl, Abschnitt „Projektseite"
  inkl. Screenshot-Kuratierungsweg und Fehlersuche-Eintrag), `README.md`-Badge
  (korrekte shields.io-Escapes) und `site/`-Tabellenzeile, `changes.log`- und
  `RELEASE_NOTES.md`-Einträge (de/en, „Release-Übersicht"/„release overview")
  decken sich mit der Implementierung. `plan.md`/`review.md` enthalten
  weiterhin Planungszeitpunkt-Formulierungen (`releases/latest`,
  „Statuschips") — historische Planungsartefakte, kein Nachpflegebedarf.

## Geprüfte Dateien

- `.github/workflows/pages.yml`
- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/` (Asset-Referenzen, Bildmaße und Provenienz per md5/PNG-Header
  verifiziert: `flownrw.png` 657×619, 4 Screenshots je 430×900, 4 Fonts)
- `README.md` (Diff gegen `staging`)
- `changes.log` (Diff gegen `staging`)
- `docs/CI-CD.md` (Diff gegen `staging`)
- `docs/RELEASE_NOTES.md` (Diff gegen `staging`)
- `docs/features/task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode/`
  (`requirement.md`, `test-results.md`, `review*.md`, `…-tasks.md` —
  Konsistenz gegen Implementierung geprüft)
