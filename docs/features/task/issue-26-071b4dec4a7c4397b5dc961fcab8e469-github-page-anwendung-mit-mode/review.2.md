# Plan-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`,
**zweite Iteration** nach den Review-Fixes zu `review-code.1.md` und
`review-usability.1.md` (geändert: `site/index.html`, `site/styles.css`,
`site/theme.js`, `.github/workflows/pages.yml`, Screenshot-Satz; Doku-Diffs
`README.md`, `changes.log`, `docs/CI-CD.md`, `docs/RELEASE_NOTES.md` unverändert)
gegen `plan.md`, `requirement.md`, `inventory.md` und `inventory/`. Keine
Änderungen an `FlowNRW`, `FlowNRW.Core`, `FlowNRW.Tests` oder sonstigem
Quellcode — Scope eingehalten.

## Ergebnis

**Status:** Offene Aufgaben vorhanden

Alle repo-seitig umsetzbaren Planelemente sind vollständig implementiert und
verifiziert; sämtliche Befunde aus Code- und Usability-Review der ersten
Iteration sind nachvollziehbar behoben. Die einzige offene Aufgabe ist der per
Plan ausdrücklich post-merge anstehende Maintainer-Schritt
(Umsetzungsreihenfolge Schritt 7, Tasks #13) — keine Implementierungslücke,
sondern ein durch den Merge blockierter Prozessschritt.

## Umgesetzte Planelemente

- [x] `site/index.html` (Landingpage, `lang="de"`) — angelegt; Header/Nav mit
  Theme-Umschalter (jetzt `hidden` im Markup, wird erst per `theme.js`
  eingeblendet — No-JS-Befund behoben) und Anker-Links (`#features`,
  `#screenshots`, `#release`, `#links`), Hero mit App-Icon, Zwecktext und CTAs
  auf `releases` + Repository, sechs Feature-Karten exakt gemäß Plan
  (Verbindungssuche, Abfahrtsmonitor mit korrigiertem Satz „Bei Fehlern bleiben
  die letzten bekannten Daten sichtbar.", Favoriten, Haltestellenkarte inkl.
  Listenalternative, automatische Monitorintervalle, NRW-Echtzeitpriorität),
  Screenshot-Galerie mit neun Figuren, Release-/Plattform-Abschnitt mit
  neutraler Aussage zu den vorhandenen Pre-releases („Die aktuellen Releases
  und Vorabversionen stehen auf GitHub bereit. Eine iOS-Veröffentlichung ist
  noch nicht verfügbar.") und iOS-„In Entwicklung"-Chip **ohne jede
  Windows-Nennung** (`grep -ri "windows\|win-x64\|msix\|\.exe\|installer" site/`
  → 0 Treffer), Verweise auf Repository, `docs/`, `docs/help/index.md`, Lizenz;
  Footer mit PolyForm-Noncommercial-, Font-Lizenz- (nur Inter/JetBrains Mono —
  ungenutzte Material-Symbols-Nennung entfernt) und
  OpenStreetMap-Attribution; Meta-/OG-Tags (`og:image` jetzt absolut),
  `theme-color` jetzt mit `prefers-color-scheme`-Media-Varianten hell/dunkel,
  Favicon; ausschließlich relative interne Pfade (`./assets/…`, `./styles.css`,
  `./theme.js`), kein hart verdrahteter Base-Pfad; keine externen
  Skripte/CDNs/Tracker. Bildmaße korrigiert: Icon `width/height` jetzt 32×30
  bzw. 220×207 passend zum realen Seitenverhältnis 657×619.
- [x] `site/styles.css` — vorhanden; alle Farb-Tokens gegen
  `FlowNRW/Resources/Styles/Colors.xaml` verifiziert (Primary `#0058BC`/`#A5C9FF`,
  on-primary `#FFFFFF`/`#242424`, Secondary `#E8F0FC`/`#2C2C2E`, Canvas
  `#FAF9FE`/`#000000`, Card `#FFFFFF`/`#1C1C1E`, Ink `#1A1B1F`/`#F5F5F7`, Muted
  `#414755`/`#C5C7D0`, Outline `#E3E2E7`/`#414149`, GroupStrong
  `#EEEDF3`/`#2C2C2E`, Success/Warning/Error je hell+dunkel); `@font-face` für
  alle vier Fonts; responsives Karten-/Galerieraster mit Breakpoints (900 px,
  640 px); redundante Selektor-Regel im 640-px-Block entfernt (`.card-grid` und
  `.screenshot-grid` jetzt getrennt deklariert); `.theme-toggle[hidden]`-Regel
  ergänzt; `prefers-reduced-motion` unverändert beachtet.
- [x] `site/theme.js` — vorhanden (`node --check` bestanden); setzt
  `data-theme` nur noch bei gespeicherter Nutzerwahl und entfernt das Attribut
  sonst (`applyStored`), zusätzlich `change`-Listener auf `matchMedia` — der
  eingefrorene Systemthema-Wechsel aus Iteration 1 ist behoben, OS-Wechsel
  greift jetzt live; Umschalter persistiert in `localStorage`
  (`flownrw-theme`), aktualisiert `aria-pressed`/Label und blendet den Button
  erst bei aktiver Verdrahtung ein (`button.hidden = false`).
- [x] `site/assets/flownrw.png` (App-Icon/Favicon) — byteidentisch zu
  `FlowNRW/Resources/AppIcon/flownrw.png` (md5 `67ad0bb1…`, unverändert).
- [x] `site/assets/fonts/` — alle vier TTFs (`Inter-Regular`, `Inter-SemiBold`,
  `Inter-Bold`, `JetBrainsMono-Bold`) byteidentisch zu
  `FlowNRW/Resources/Fonts/` (md5-Abgleich, unverändert).
- [x] `site/assets/screenshots/` — **ersetzter Satz: 9 kuratierte PNGs**
  (4× hell, 4× dunkel, 1 weitere helle Aufnahme), alle per Hash-Abgleich
  byteidentische Kopien aus `docs/help/design/verification/icon-matrix/`
  (`stop-monitor-cached`, `map-native-list`, `stop-list[-returned]`,
  `journey-details`, `search-form` je hell/dunkel); die beanstandeten Motive
  wurden ausgetauscht: `haltestellenkarte-*` (leere Basiskarte ohne Kacheln)
  → `haltestellenliste-*` (native Listenansicht mit echten Treffern, deckt sich
  mit der geänderten Feature-Karten-Formulierung „alternativ als Liste
  bedienbar") und `verbindungssuche-hell` (identische Start-/Ziel-Koordinaten)
  → `search-form`-Motive ohne befüllte Koordinatenfelder; visuell geprüft:
  keine Fixture-Texte („UI-Fixture", „monitor-sequence", „KARTENFIXTURE"),
  keine `failure-`/`*-provider-error`/`*-offline`/`*-loading`/`*-empty-*`-Namen,
  kein `native-*.png`.
- [x] `.github/workflows/pages.yml` — vorhanden; YAML-Parse bestanden;
  `name` + englischer Header-Kommentar inkl. Pages-Aktivierungs-Voraussetzung;
  Trigger `push` auf `main` mit `paths: [site/**, .github/workflows/pages.yml]`
  plus `workflow_dispatch`; `permissions: contents: read, pages: write,
  id-token: write`; `concurrency: group "pages", cancel-in-progress: false`;
  Job `deploy` auf `ubuntu-latest` mit `environment: github-pages` und
  `url: ${{ steps.deployment.outputs.page_url }}`; Step-Reihenfolge
  `actions/checkout@v7` → `actions/configure-pages@v6` →
  `actions/upload-pages-artifact@v5` (`path: site`) → `actions/deploy-pages@v5`
  (`id: deployment`) → Smoke-Step (`curl --fail --retry 5 --retry-delay 10
  --retry-all-errors` + Marker „FlowNRW"). Action-Majors auf den aktuellen Stand
  angehoben (Befund aus Iteration 1 behoben) — konsistent zu den übrigen
  Repo-Workflows.
- [x] `docs/CI-CD.md` — alle vier geplanten Ergänzungen unverändert vorhanden:
  Workflow-Tabellenzeile `pages.yml`, Einrichtungspunkt 6 „Pages-Quelle:
  GitHub Actions" (Settings-Weg + `gh api`-Alternativbefehl +
  `GITHUB_TOKEN`-Hinweis), Abschnitt „Projektseite (GitHub Pages)" (Quellordner,
  Deploy-Kette, Update-Weg, Screenshot-Kuratierung, manueller Redeploy),
  Fehlersuche-Eintrag „Get Pages site failed".
- [x] `README.md` — Badge/Verweis auf
  `https://martin-stromberg.github.io/FlowNRW/` im Kopf und `site/`-Zeile in
  der Projektstruktur-Tabelle (unverändert).
- [x] `changes.log` — datierter Eintrag „2026-11-16 — Statische Projektseite
  für GitHub Pages" (unverändert).
- [x] `docs/RELEASE_NOTES.md` — Einträge in „What's New" und „Neuerungen"
  (unverändert).
- [x] Validierungsregeln — Screenshot-Auswahl (s. o.), interne Referenzen
  relativ / externe absolut (`src="http`/`href="http` nur in `<a>` und
  OG-Tags; keine `src="/…"`/`href="/…"`/`url("/…")`), Inhaltsaussagen (kein
  Windows, iOS „In Entwicklung", neutrale Pre-release-Aussage), keine externen
  Abhängigkeiten (Fonts lokal), Kontrast-Tokens aus dokumentierter
  `Colors.xaml`-Quelle.
- [x] Review-Fixes Iteration 1 — alle 7 Code-Review-Befunde und alle
  umsetzbaren Usability-Befunde behoben (siehe Einzelnachweise oben);
  der bedingte Usability-Hinweis zu `main`-Pfaden in den Verweisen bleibt
  bewusst unverändert (löst sich erst durch die Promotion `staging` → `main`
  auf, siehe Hinweise).

## Offene Aufgaben

- [ ] `Umsetzungsreihenfolge Schritt 7: Einmalige Pages-Aktivierung und
  Go-Live-Abnahme` — nicht ausführbar vor dem Merge: Voraussetzungen sind
  Merge nach `staging`, Promotion-Merge nach `main` und Maintainer-Rechte.
  Auszuführen: Pages-Quelle „GitHub Actions" setzen (Settings → Pages oder
  `gh api -X POST repos/martin-stromberg/FlowNRW/pages -f build_type=workflow`),
  `pages.yml`-Lauf auf `main` beobachten (ggf. `workflow_dispatch`), öffentliche
  URL `https://martin-stromberg.github.io/FlowNRW/` auf Inhalte, Themes und
  Responsivität prüfen — dabei die drei `main`-pfadbezogenen Verweise
  (`tree/main/docs`, `blob/main/docs/help/index.md`, `blob/main/LICENSE`)
  einmalig anklicken (Usability-Befund Iteration 1, bedingt).

## Hinweise

- **Keine Nachimplementierung erforderlich.** Der Status „Offene Aufgaben
  vorhanden" spiegelt ausschließlich den planmäßig post-merge liegenden
  Go-Live-Schritt; es existiert keine Code-/Dokumentationslücke.
- **Release-Verweis weicht bewusst vom Planwortlaut ab:** Plan und Tasks nennen
  `releases/latest`; die Seite verlinkt nach dem Usability-Fix auf
  `releases` (Übersicht). Funktional gleichwertig — ohne stabiles Release
  leitet `releases/latest` ohnehin auf die Übersicht weiter — und die
  Anforderung „plattformneutraler Download-/Release-Verweis" ist erfüllt. Die
  Formulierungen in `changes.log` („plattformneutrale `releases/latest`") und
  `docs/RELEASE_NOTES.md` („latest release"/„aktuelles Release") beschreiben
  den ursprünglichen Planstand und könnten beim nächsten Anfassen der Einträge
  an die tatsächliche Verlinkung angeglichen werden — kein Verstoß, da beide
  Ziele auf dieselbe Seite auflösen.
- **Screenshot-Anzahl 9 statt „~6–8":** marginal über der Planangabe, fachlich
  unproblematisch (deckt vier Motive sauber hell+dunkel ab); Testnachweis in
  der Tasks-Datei wurde auf 9 PNGs aktualisiert.
- **Verweise auf `main`-Pfade:** `tree/main/docs`,
  `blob/main/docs/help/index.md` und `blob/main/LICENSE` liefern erst nach
  der Promotion Inhalte — da `pages.yml` ohnehin nur auf `main` deployt,
  ist das zeitlich konsistent; Go-Live-Abnahme enthält den Link-Check
  (siehe Offene Aufgaben).
- **`hidden`-Theme-Umschalter:** Bei deaktiviertem JavaScript bleibt die
  `prefers-color-scheme`-Darstellung wirksam, der wirkungslose Schalter
  erscheint nicht — Progressive Enhancement sauber umgesetzt.
