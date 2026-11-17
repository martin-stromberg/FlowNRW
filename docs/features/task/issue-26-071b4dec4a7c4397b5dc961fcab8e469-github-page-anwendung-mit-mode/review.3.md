# Plan-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`,
**dritte Iteration** nach den Fixes zu `review-code.2.md` und
`review-usability.2.md` (geändert: `site/index.html`, `site/styles.css`,
Screenshot-Satz reduziert auf 4 saubere Motive, `changes.log`,
`docs/RELEASE_NOTES.md`, `test-results.md`, Tasks-Datei; `site/theme.js`,
`.github/workflows/pages.yml`, `README.md`, `docs/CI-CD.md` unverändert)
gegen `plan.md`, `requirement.md`, `inventory.md` und `inventory/`. Keine
Änderungen an `FlowNRW`, `FlowNRW.Core`, `FlowNRW.Tests` oder sonstigem
Quellcode — Scope eingehalten.

## Ergebnis

**Status:** Offene Aufgaben vorhanden

Alle repo-seitig umsetzbaren Planelemente sind vollständig implementiert und
verifiziert; die Befunde aus Code- und Usability-Review der zweiten Iteration
sind nachvollziehbar behoben. Die einzige offene Aufgabe ist der per Plan
ausdrücklich post-merge anstehende Maintainer-Schritt (Umsetzungsreihenfolge
Schritt 7, Tasks #13) — keine Implementierungslücke, sondern ein durch den
Merge blockierter Prozessschritt.

## Umgesetzte Planelemente

- [x] `site/index.html` (Landingpage, `lang="de"`) — vorhanden; Header/Nav mit
  Theme-Umschalter (`hidden` im Markup, Progressive Enhancement via `theme.js`)
  und Anker-Links (`#features`, `#screenshots`, `#release`, `#links` — alle
  Ziel-IDs vorhanden), Hero mit App-Icon, Zwecktext („bundesweite
  ÖPNV-Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten") und CTAs
  auf `releases` + Repository, sechs Feature-Karten exakt gemäß Plan
  (Verbindungssuche, Abfahrtsmonitor mit Statuschips, Favoriten,
  Haltestellenkarte inkl. Listenalternative, automatische Monitorintervalle,
  NRW-Echtzeitpriorität), Screenshot-Galerie mit vier Figuren,
  Release-/Plattform-Abschnitt mit **neutraler Vorabversions-Kommunikation**
  („Die aktuellen Vorabversionen stehen auf GitHub bereit.") und
  iOS-„In Entwicklung"-Chip samt Hinweistext — **ohne jede Windows-Nennung**
  (`grep -ri "windows\|win-x64\|msix\|\.exe\|installer" site/` → 0 Treffer,
  eigenständig verifiziert), Verweise auf Repository, `docs/`,
  `docs/help/index.md`, Lizenz; Footer mit PolyForm-Noncommercial-,
  Font-Lizenz- (Inter/JetBrains Mono, SIL OFL 1.1) und
  OpenStreetMap-Attribution; Meta-/OG-Tags (`og:image` absolut),
  `theme-color` mit `prefers-color-scheme`-Media-Varianten, Favicon;
  ausschließlich relative interne Pfade (`./assets/…`, `./styles.css`,
  `./theme.js` — kein `src="/…"`/`href="/…"`, kein hart verdrahteter
  Base-Pfad `/FlowNRW/`); externe Links nur als `<a href>` auf
  `github.com/martin-stromberg/FlowNRW` und `polyformproject.org`; keine
  externen Skripte/CDNs/Tracker.
- [x] `site/styles.css` — vorhanden; Farb-Tokens aus
  `FlowNRW/Resources/Styles/Colors.xaml` (Primary `#0058BC`/`#A5C9FF`,
  on-primary `#FFFFFF`/`#242424`, Secondary `#E8F0FC`/`#2C2C2E`, Canvas
  `#FAF9FE`/`#000000`, Card `#FFFFFF`/`#1C1C1E`, Ink `#1A1B1F`/`#F5F5F7`,
  Muted `#414755`/`#C5C7D0`, Outline `#E3E2E7`/`#414149`, GroupStrong
  `#EEEDF3`/`#2C2C2E`, Success/Warning/Error je hell+dunkel); `@font-face`
  für alle vier Fonts; helle Darstellung via `:root`, dunkle via
  `[data-theme="dark"]` plus `prefers-color-scheme`-Fallback
  (`:root:not([data-theme="light"])` — No-JS-Systemfolge gewahrt);
  responsives Layout (Breakpoints 900 px/640 px); `.screenshot-grid` an den
  reduzierten Satz angepasst (4 → 3 → 2 Spalten); `prefers-reduced-motion`
  beachtet.
- [x] `site/theme.js` — vorhanden (`node --check` bestanden); setzt
  `data-theme` nur bei gespeicherter Nutzerwahl (`localStorage`,
  `flownrw-theme`), entfernt es sonst; `change`-Listener auf `matchMedia`
  (inkl. `addListener`-Fallback) — OS-Theme-Wechsel greift live; Umschalter
  aktualisiert `aria-pressed`/Label und wird erst bei aktiver Verdrahtung
  eingeblendet (`button.hidden = false`). Ohne JS bleibt die
  `prefers-color-scheme`-Darstellung voll wirksam — sauberes No-JS-Verhalten.
- [x] `site/assets/flownrw.png` (App-Icon/Favicon) — byteidentisch zu
  `FlowNRW/Resources/AppIcon/flownrw.png` (md5 `67ad0bb1…`, erneut verifiziert).
- [x] `site/assets/fonts/` — alle vier TTFs (`Inter-Regular`, `Inter-SemiBold`,
  `Inter-Bold`, `JetBrainsMono-Bold`) byteidentisch zu
  `FlowNRW/Resources/Fonts/` (md5-Abgleich, erneut verifiziert).
- [x] `site/assets/screenshots/` — **reduzierter, sauberer Satz: 4 kuratierte
  PNGs** (je 2× hell + 2× dunkel, 430×900 px), alle per md5-Abgleich
  byteidentische Kopien aus `docs/help/design/verification/icon-matrix/`:
  `verbindungssuche-{hell,dunkel}` = `…-search-form`,
  `aktualisierung-{hell,dunkel}` = `…-settings-success-keyboard`. Alle vier
  Bilder visuell geprüft: **keine Warn-/Fehler-/Fallback-Zustände** (kein
  „Daten möglicherweise unvollständig", kein „Basiskarte geladen", kein
  „Fällt aus"-Banner), keine Fixture-Texte („UI-Fixture", „monitor-sequence",
  „KARTENFIXTURE"), keine `failure-`/`*-provider-error`/`*-offline`/
  `*-loading`/`*-empty-*`-Namen, kein `native-*.png`; Bildunterschriften und
  `alt`-Texte passen zum Bildinhalt. Alle in `review-usability.2.md`
  beanstandeten Motive (`verbindungsdetails-*`, `haltestellensuche-hell`,
  `haltestellenliste-*`, `abfahrtsmonitor-*`) wurden entfernt — Usability-Befund
  behoben.
- [x] `.github/workflows/pages.yml` — vorhanden; YAML-Parse bestanden;
  `name` + englischer Header-Kommentar inkl. Pages-Aktivierungs-Voraussetzung;
  Trigger `push` auf `main` mit `paths: [site/**, .github/workflows/pages.yml]`
  plus `workflow_dispatch`; `permissions: contents: read, pages: write,
  id-token: write`; `concurrency: group "pages", cancel-in-progress: false`;
  Job `deploy` auf `ubuntu-latest` (`timeout-minutes: 10`) mit
  `environment: github-pages` und `url: ${{ steps.deployment.outputs.page_url }}`;
  Step-Reihenfolge `actions/checkout@v7` → `actions/configure-pages@v6` →
  `actions/upload-pages-artifact@v5` (`path: site`) → `actions/deploy-pages@v5`
  (`id: deployment`) → Smoke-Step (`curl --fail --retry 5 --retry-delay 10
  --retry-all-errors` + Marker „FlowNRW" per `grep -q`).
- [x] `docs/CI-CD.md` — alle vier geplanten Ergänzungen unverändert vorhanden:
  Workflow-Tabellenzeile `pages.yml`, Einrichtungspunkt 6 „Pages-Quelle:
  GitHub Actions" (Settings-Weg + `gh api`-Alternativbefehl +
  `GITHUB_TOKEN`-Hinweis), Abschnitt „Projektseite (GitHub Pages)"
  (Quellordner, Deploy-Kette, Update-Weg, Screenshot-Kuratierung inkl.
  Ausschluss von Fixture-/Fehler-/Leerzuständen, manueller Redeploy),
  Fehlersuche-Eintrag „Get Pages site failed".
- [x] `README.md` — Badge/Verweis auf
  `https://martin-stromberg.github.io/FlowNRW/` im Kopf und `site/`-Zeile in
  der Projektstruktur-Tabelle (unverändert).
- [x] `changes.log` — datierter Eintrag „2026-11-16 — Statische Projektseite
  für GitHub Pages"; **Iteration-3-Fix verifiziert:** Release-Verweis jetzt
  korrekt als „plattformneutrale Release-Übersicht `releases`" beschrieben
  (Code-Review-Befund aus Iteration 2 behoben).
- [x] `docs/RELEASE_NOTES.md` — Einträge in „What's New" und „Neuerungen",
  jetzt präzisiert auf „release overview"/„Release-Übersicht" (optionale
  Empfehlung aus `review-code.2.md` mit umgesetzt).
- [x] Validierungsregeln — Screenshot-Auswahl (s. o.; die beanstandeten
  Degradierungs-Motive sind ersetzt), interne Referenzen relativ / externe
  absolut (eigenständig per Grep verifiziert), Inhaltsaussagen (kein Windows,
  iOS „In Entwicklung", neutrale „Vorabversionen"-Formulierung — kein
  „aktuelle Releases"-Framing mehr), keine externen Abhängigkeiten (Fonts
  lokal), Kontrast-Tokens aus dokumentierter `Colors.xaml`-Quelle.
- [x] Tasks-Datei (`…-tasks.md`) — aktuell: Aufgaben 1–12 `Erledigt` mit
  Testnachweisen (Task-6-Nachweis auf den 4-Bilder-Satz und die neuen
  Quellmotive aktualisiert), Task 13 (Go-Live-Abnahme) `Offen`.
- [x] Review-Fixes Iteration 2 — Code-Befund (veraltete `releases/latest`-
  Angaben in `changes.log`/`test-results.md`) behoben; beide Usability-Befunde
  (Vorabversions-/iOS-Kommunikation, degradierte Screenshot-Motive) behoben —
  siehe Einzelnachweise oben und Hinweise unten.

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
  einmalig anklicken (bedingter Usability-Hinweis, löst sich erst durch die
  Promotion auf).

## Hinweise

- **Keine Nachimplementierung erforderlich.** Der Status „Offene Aufgaben
  vorhanden" spiegelt ausschließlich den planmäßig post-merge liegenden
  Go-Live-Schritt; es existiert keine Code-/Dokumentationslücke.
- **Screenshot-Anzahl 4 statt „~6–8" im Planwortlaut:** Der Satz unterschreitet
  die im Plan genannte Richtgröße — bewusste Konsequenz der Kuratierung nach
  dem Usability-Befund (nur Motive ohne Warn-/Fallback-Zustände sind
  präsentationsfähig). Die harte Mindestanforderung des Plans (mind. ein
  helles und ein dunkles Motiv) ist erfüllt; die Tasks-Datei wurde bereits auf
  „mind. hell + dunkel" angeglichen. Optional: Planwortlaut `~6–8` beim
  nächsten Anfassen des Plans an den kuratierten Vierer-Satz anpassen oder den
  Satz später um saubere Neuaufnahmen erweitern (in `docs/CI-CD.md` als
  Update-Weg dokumentiert).
- **iOS-Satz „iOS ist die Zielplattform von FlowNRW" bewusst beibehalten:**
  Die Usability-Empfehlung, die Formulierung zu relativieren (z. B. „weitere
  Zielplattform"), wurde nicht übernommen — sie würde eine weitere
  Zielplattform implizieren und damit der Anwenderfestlegung (iOS = die
  Zielplattform, Windows wird nicht genannt) widersprechen. Die eigentliche
  Kernverbesserung des Befunds ist umgesetzt: „aktuelle Releases" →
  „aktuelle Vorabversionen" (kein falsches Stable-Versprechen mehr). Die
  verbleibende Mehrdeutigkeit, auf welcher Plattform die Vorabversionen
  laufen, ist der Windows-Nennungssperre inhärent und plan-/festlegungskonform.
- **Release-Verweisziel `releases` statt `releases/latest`:** seit Iteration 2
  dokumentierte, funktional gleichwertige Abweichung vom Planwortlaut; jetzt
  konsistent in `index.html`, `changes.log`, `docs/RELEASE_NOTES.md` und
  `test-results.md:69` beschrieben. `plan.md`/Tasks-Textbausteine, die
  `releases/latest` nennen, könnten beim nächsten Anfassen angeglichen werden.
- **Kleinigkeit in `test-results.md:33`:** die Testprotokoll-Tabelle listet
  noch `configure-pages@v5`/`upload-pages-artifact@v4`/`deploy-pages@v4`,
  während der Workflow seit Iteration 2 `@v6`/`@v5`/`@v5` nutzt — veralteter
  Protokolleintrag im Feature-Verzeichnis, keine Auswirkung auf die
  Implementierung; kann beim nächsten Durchlauf nachgezogen werden.
- **Galerie-Abdeckung:** Vier Feature-Karten (Favoriten, Haltestellenkarte,
  Abfahrtsmonitor, NRW-Echtzeitpriorität) stehen ohne Screenshot da — im Plan
  nur exemplarisch genannt, kein Verstoß. Bei späteren sauberen
  `icon-matrix`-Läufen ergänzbar.
