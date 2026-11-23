# Plan-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`
(**vierte/finale Iteration** nach den Fixes zu `review-code.3.md` und
`review-usability.3.md` — geändert seit Iteration 3: `site/index.html`
Release-Panel jetzt „Vorabversionen für die vorhandene **Desktop-Anwendung**",
Feature-Karten ohne Fachvokabular, `test-results.md`-Prüfwerte nachgezogen)
gegen `plan.md`, `requirement.md`, `inventory.md` und `inventory/`. Alle
Nachweise in diesem Review wurden eigenständig neu erhoben (Grep, md5-Abgleich,
YAML-/Syntax-Parse, Referenz-Auflösung, Bildsichtung). Keine Änderungen an
`FlowNRW`, `FlowNRW.Core`, `FlowNRW.Tests` oder sonstigem Quellcode —
Scope-Ausschluss 6 eingehalten (`git status`: nur `site/`, `pages.yml`, Doku).

## Ergebnis

**Status:** Vollständig umgesetzt

Alle repo-seitig umsetzbaren Planelemente sind vollständig implementiert,
verifiziert und dokumentiert. Der einzige verbleibende Planpunkt —
Umsetzungsreihenfolge Schritt 7 (einmalige Pages-Aktivierung und
Go-Live-Abnahme an der öffentlichen URL) — ist per Plan ausdrücklich ein
Post-Merge-/Post-Promotion-Schritt des Maintainers und vor dem Merge technisch
nicht ausführbar (Deploy triggert ausschließlich auf `main`; die
Pages-Quell-Einstellung erfordert Repo-Administration). Er ist in
`docs/CI-CD.md` (Einrichtungspunkt 6 + Fehlersuche) und in der Tasks-Datei
(Aufgabe 13, `Offen`) dokumentiert — siehe Hinweise.

## Umgesetzte Planelemente

- [x] `site/index.html` (Landingpage, `lang="de"`) — angelegt. Header/Nav mit
  Icon, Anker-Links und Theme-Umschalter (`hidden` im Markup, Progressive
  Enhancement via `theme.js`); Hero mit App-Icon, Zwecktext („bundesweite
  ÖPNV-Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten") und CTAs
  auf die Release-Übersicht + Repository; sechs Feature-Karten exakt gemäß Plan
  (Verbindungssuche mit Umstiegen/Fußwegen, Abfahrtsmonitor mit
  Echtzeit-Statuschips, Favoriten mit Abfahrtstafeln/Entfernungssortierung,
  Haltestellenkarte + nahe Haltestellen, automatische Monitorintervalle,
  NRW-Echtzeitpriorität mit bundesweiter Ergänzung); Screenshot-Galerie (4
  Figuren mit `alt`-Texten, `figcaption`, `loading="lazy"`);
  Release-/Plattform-Abschnitt: „Vorabversionen für die vorhandene
  **Desktop-Anwendung**" (neutral, ohne Windows-Nennung), iOS als Zielplattform
  mit Chip „In Entwicklung" samt Hinweistext; Verweise auf Repository, `docs/`,
  `docs/help/index.md`, Lizenz; Footer mit PolyForm-Noncommercial-,
  Font-Lizenz- (Inter/JetBrains Mono, SIL OFL 1.1) und
  OpenStreetMap-Attribution; Meta-/OG-Tags (`og:image` absolut),
  `theme-color` mit `prefers-color-scheme`-Varianten, Favicon.
- [x] `site/styles.css` — angelegt. Alle CSS-Custom-Properties stichproben- und
  wertgenau gegen `FlowNRW/Resources/Styles/Colors.xaml` verifiziert (Primary
  `#0058BC`/`#A5C9FF`, Canvas `#FAF9FE`/`#000000`, Card `#FFFFFF`/`#1C1C1E`,
  Ink `#1A1B1F`/`#F5F5F7`, Muted `#414755`/`#C5C7D0`, Outline
  `#E3E2E7`/`#414149`, GroupStrong `#EEEDF3`/`#2C2C2E`, Success/Warning/Error
  je hell+dunkel); hell via `:root`, dunkel via `[data-theme="dark"]` plus
  `prefers-color-scheme`-Fallback (`:root:not([data-theme="light"])`);
  4× `@font-face` (Pfade auflösbar); responsives Layout (Breakpoints
  900 px/640 px, Karten-/Screenshot-Raster); `prefers-reduced-motion` beachtet;
  Kontrast-Tokens aus der dokumentierten `Colors.xaml`-Quelle (≥ 4,5:1).
- [x] `site/theme.js` — angelegt (`node --check` bestanden). Initialzustand aus
  `localStorage` (`flownrw-theme`) bzw. `prefers-color-scheme`; setzt/entfernt
  `data-theme` auf `<html>`; persistiert die Wahl (try/catch-gesichert);
  `matchMedia`-`change`-Listener (inkl. `addListener`-Fallback) für live
  Systemwechsel; Umschalter mit `aria-pressed`/Label-Update, wird erst bei
  aktiver Verdrahtung eingeblendet — sauberes No-JS-Verhalten.
- [x] `site/assets/flownrw.png` (App-Icon/Favicon) — vorhanden; md5-Abgleich
  byteidentisch zu `FlowNRW/Resources/AppIcon/flownrw.png`.
- [x] `site/assets/fonts/` — alle vier TTFs (`Inter-Regular`, `Inter-SemiBold`,
  `Inter-Bold`, `JetBrainsMono-Bold`) md5-byteidentisch zu
  `FlowNRW/Resources/Fonts/`; lokal gehostet, keine Font-CDNs.
- [x] `site/assets/screenshots/` — reduzierter, sauberer Satz: 4 kuratierte
  PNGs (je hell + dunkel, 430×900), alle md5-byteidentische Kopien aus
  `docs/help/design/verification/icon-matrix/` mit sprechenden Namen
  (`verbindungssuche-{hell,dunkel}` ← `…-search-form`,
  `aktualisierung-{hell,dunkel}` ← `…-settings-success-keyboard`). Alle vier
  Bilder visuell gesichtet: echte App-Ansichten, **keine** Fixture-Texte
  („UI-Fixture", „monitor-sequence", „KARTENFIXTURE"), **keine**
  Warn-/Fehler-/Fallback-/Leerzustände; Dateinamen frei von `failure-*`,
  `*-provider-error`, `*-offline`, `*-loading`, `*-empty-*`; kein
  `native-*.png` verwendet. Erfüllt die harte Plananforderung (mind. je ein
  helles und dunkles Motiv).
- [x] `.github/workflows/pages.yml` — angelegt; `yaml.safe_load` bestanden.
  `name:` + englischer Header-Kommentar inkl. Pages-Aktivierungs-Voraussetzung;
  Trigger `push` auf `main` mit `paths: ["site/**",
  ".github/workflows/pages.yml"]` plus `workflow_dispatch`;
  `permissions: contents: read, pages: write, id-token: write`;
  `concurrency: group "pages", cancel-in-progress: false`; ein Job `deploy`
  auf `ubuntu-latest` mit `environment: name: github-pages, url: ${{
  steps.deployment.outputs.page_url }}`; Step-Kette `actions/checkout@v7` →
  `actions/configure-pages@v6` → `actions/upload-pages-artifact@v5`
  (`path: site`) → `actions/deploy-pages@v5` (`id: deployment`) → Smoke-Step
  (`curl --fail --retry 5 --retry-delay 10 --retry-all-errors` auf `page_url`
  + Marker-String „FlowNRW" per `grep -q`).
- [x] `docs/CI-CD.md` — alle vier geplanten Ergänzungen verifiziert (Diff):
  Workflow-Tabellenzeile `pages.yml` | „Push auf `main` (`site/**`), manuell" |
  „Deploy der Projektseite nach GitHub Pages"; Einrichtungspunkt 6
  „Pages-Quelle: GitHub Actions" (Settings-Weg + `gh api -X POST
  repos/martin-stromberg/FlowNRW/pages -f build_type=workflow` +
  `GITHUB_TOKEN`-Hinweis); Abschnitt „Projektseite (GitHub Pages)" (Quellordner
  `site/`, Deploy-Kette, Update-Weg über PR → `staging` → Promotion → `main`,
  Screenshot-Kuratierung inkl. Ausschlusskriterien, manueller Redeploy per
  `workflow_dispatch`); Fehlersuche-Eintrag „Get Pages site failed".
- [x] `README.md` — Projektseiten-Badge/Verweis auf
  `https://martin-stromberg.github.io/FlowNRW/` im Kopfbereich und `site/`-Zeile
  in der Projektstruktur-Tabelle (Diff verifiziert).
- [x] `changes.log` — datierter Eintrag „2026-11-16 — Statische Projektseite
  für GitHub Pages" (Seite, Workflow, Plattformdarstellung, einmalige
  Voraussetzung).
- [x] `docs/RELEASE_NOTES.md` — Einträge in „What's New" und „Neuerungen",
  konsistent zur Implementierung („release overview"/„Release-Übersicht").
- [x] Validierungsregeln — alle eigenständig verifiziert:
  - **Screenshot-Auswahl:** s. o. — sauber, keine Testartefakte.
  - **Interne Referenzen:** ausschließlich relativ (`./assets/…`,
    `./styles.css`, `./theme.js`, `url("./assets/fonts/…")`); kein
    `src="/…"`/`href="/…"`/`url("/…")`, kein hart verdrahteter Base-Pfad
    `/FlowNRW/`; alle 9 HTML-Referenzen, 5 Ankerziele und 4 CSS-`url()`-Pfade
    per Skript aufgelöst — keine fehlenden Dateien/Targets.
  - **Inhaltsaussagen:** `grep -ri "windows|win-x64|msix" site/` → 0 Treffer;
    iOS einzige genannte Zielplattform mit Status „In Entwicklung";
    Release-Framing plattformneutral („Desktop-Anwendung", keine
    Windows-Voraussetzung, kein „Windows-Download").
  - **Externe Abhängigkeiten:** keine externen Skripte/Styles/Bilder/CDNs/
    Tracker — externe URLs nur als `<a href>` (github.com,
    polyformproject.org) und in Meta-Tags; Fonts lokal.
  - **Kontraste:** Token-Werte wertidentisch aus `Colors.xaml` (dort mit
    Kontrastnachweis dokumentiert).
- [x] Tasks-Datei (`…-tasks.md`) — aktuell: Aufgaben 1–12 `Erledigt` mit
  Testnachweisen, Aufgabe 13 (Go-Live-Abnahme post-merge) `Offen`.
- [x] Review-Fixes Iteration 3 — `test-results.md:33` (Actions-Versionen jetzt
  `@v6`/`@v5`/`@v5`) und `:46` (80 Klammerpaare) nachgezogen;
  Usability-Befunde behoben: Plattform der Vorabversionen jetzt neutral als
  „Desktop-Anwendung" benannt (Variante aus der Review-Empfehlung),
  Fachvokabular ersetzt („Ersatzdaten" statt „Fallbacks", „Kartenverläufe zu
  gefundenen Verbindungen" statt „gelieferte Verbindungsverläufe").

## Hinweise

- **Post-Merge-Schritt (kein Implementierungslücke):** Umsetzungsreihenfolge
  Schritt 7 — Maintainer setzt nach dem Promotion-Merge die Pages-Quelle auf
  „GitHub Actions" (Settings → Pages oder `gh api -X POST
  repos/martin-stromberg/FlowNRW/pages -f build_type=workflow`), beobachtet den
  `pages.yml`-Lauf auf `main` (ggf. `workflow_dispatch`) und nimmt
  `https://martin-stromberg.github.io/FlowNRW/` ab (Inhalte, Themes,
  Responsivität). Vor dem Merge technisch nicht ausführbar; in `docs/CI-CD.md`
  und Tasks-Datei (#13) dokumentiert.
- **Dokumentierte Planabweichung `releases` statt `releases/latest`:** Die
  Seite verweist auf die Release-Übersicht statt auf `releases/latest` —
  funktional gleichwertig, seit Iteration 2 begründet (nur Vorabversionen
  vorhanden) und konsistent in `index.html`, `changes.log`,
  `docs/RELEASE_NOTES.md`, Tasks-Datei und `test-results.md` beschrieben.
  `plan.md` nennt noch `releases/latest` (Planungsstand); beim nächsten
  Anfassen des Plans angleichbar.
- **Screenshot-Anzahl 4 statt Richtgröße „~6–8":** bewusste Konsequenz der
  Kuratierung (nur fixture-/degradierungsfreie Motive sind präsentationsfähig);
  Plan-Mindestanforderung (hell + dunkel) erfüllt, Tasks-Datei angeglichen.
  Erweiterung über spätere saubere `icon-matrix`-Läufe ist in `docs/CI-CD.md`
  als Update-Weg dokumentiert.
- **Galerie-Abdeckung:** Vier Feature-Karten stehen ohne Screenshot da — im
  Plan nur exemplarisch genannt, kein Verstoß.
