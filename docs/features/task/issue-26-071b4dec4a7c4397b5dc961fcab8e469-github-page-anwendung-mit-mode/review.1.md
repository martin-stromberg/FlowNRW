# Plan-Review

Geprüft: uncommittete Implementierung auf `task/issue-26-…-github-page-anwendung-mit-mode`
(geändert: `README.md`, `changes.log`, `docs/CI-CD.md`, `docs/RELEASE_NOTES.md`,
Feature-Doku; neu: `.github/workflows/pages.yml`, `site/`) gegen `plan.md`,
`requirement.md`, `inventory.md` und `inventory/`. Keine Änderungen an
`FlowNRW`, `FlowNRW.Core`, `FlowNRW.Tests` oder sonstigem Quellcode — Scope
eingehalten.

## Ergebnis

**Status:** Offene Aufgaben vorhanden

Alle repo-seitig umsetzbaren Planelemente sind vollständig implementiert und
verifiziert. Die einzige offene Aufgabe ist der per Plan ausdrücklich
post-merge anstehende Maintainer-Schritt (Umsetzungsreihenfolge Schritt 7,
Tasks #13) — keine Implementierungslücke, sondern ein durch den Merge
blockierter Prozessschritt.

## Umgesetzte Planelemente

- [x] `site/index.html` (Landingpage, `lang="de"`) — angelegt; Header/Nav mit
  Theme-Umschalter und Anker-Links (`#features`, `#screenshots`, `#release`,
  `#links`), Hero mit App-Icon, Zwecktext und CTAs `releases/latest` +
  Repository, sechs Feature-Karten exakt gemäß Plan (Verbindungssuche,
  Abfahrtsmonitor, Favoriten, Haltestellenkarte, automatische
  Monitorintervalle, NRW-Echtzeitpriorität), Screenshot-Galerie mit allen acht
  Assets, Release-/Plattform-Abschnitt plattformneutral mit iOS „In
  Entwicklung"-Chip und **ohne jede Windows-Nennung** (`grep -ri windows site/`
  → keine Treffer, auch kein MSIX/Installer-Framing), Verweise auf Repository,
  `docs/`, `docs/help/index.md`, Lizenz; Footer mit PolyForm-Noncommercial-,
  Font-Lizenz- und OpenStreetMap-Attribution; Meta-/OG-Tags, `theme-color`,
  Favicon; ausschließlich relative interne Pfade (`./assets/…`, `./styles.css`,
  `./theme.js`), kein hart verdrahteter Base-Pfad `/FlowNRW/`; keine externen
  Skripte/CDNs/Tracker.
- [x] `site/styles.css` — angelegt; alle Farb-Tokens stichprobenartig gegen
  `FlowNRW/Resources/Styles/Colors.xaml` verifiziert (Primary `#0058BC`/`#A5C9FF`,
  Canvas `#FAF9FE`/`#000000`, Card `#FFFFFF`/`#1C1C1E`, Ink `#1A1B1F`/`#F5F5F7`,
  Muted `#414755`/`#C5C7D0`, Outline `#E3E2E7`/`#414149`, GroupStrong
  `#EEEDF3`/`#2C2C2E`, Success/Warning/Error je hell+dunkel); `@font-face` für
  alle vier Fonts; responsives Karten-/Galerieraster mit Breakpoints (900 px,
  640 px); Typografie-Hierarchie; `prefers-reduced-motion`-Beachtung.
- [x] `site/theme.js` — angelegt; Initialzustand aus `localStorage`
  (`flownrw-theme`) mit `prefers-color-scheme`-Fallback, setzt `data-theme`
  auf `<html>`, persistiert die Umschalterwahl, aktualisiert `aria-pressed`
  und Button-Label.
- [x] `site/assets/flownrw.png` (App-Icon/Favicon) — byteidentisch zu
  `FlowNRW/Resources/AppIcon/flownrw.png` (md5 `67ad0bb1…`).
- [x] `site/assets/fonts/` — alle vier TTFs (`Inter-Regular`, `Inter-SemiBold`,
  `Inter-Bold`, `JetBrainsMono-Bold`) byteidentisch zu
  `FlowNRW/Resources/Fonts/` (md5-Abgleich).
- [x] `site/assets/screenshots/` — 8 kuratierte PNGs (4× hell, 3× dunkel,
  1 weitere helle Aufnahme) mit sprechenden Namen; Hash-Abgleich bestätigt
  byteidentische Kopien aus `docs/help/design/verification/icon-matrix/`
  (`…-stop-monitor-cached`, `…-map-stations`, `…-stop-list[-returned]`,
  `…-journey-details`, `…-search-selections`); kein `native-*.png`; keine
  Dateinamen mit `failure-`, `*-provider-error`, `*-offline`, `*-loading`,
  `*-empty-*`; Stichproben visuell frei von Fixture-Feldern („UI-Fixture",
  „monitor-sequence", „KARTENFIXTURE") — gezeigt werden reale
  Haltestellen-/Verbindungsdaten (Essen/Düsseldorf Hbf).
- [x] `.github/workflows/pages.yml` — angelegt; `name` + englischer
  Header-Kommentar inkl. dokumentierter Pages-Aktivierungs-Voraussetzung;
  Trigger `push` auf `main` mit `paths: [site/**, .github/workflows/pages.yml]`
  plus `workflow_dispatch`; `permissions: contents: read, pages: write,
  id-token: write`; `concurrency: group "pages", cancel-in-progress: false`;
  Job `deploy` auf `ubuntu-latest` mit `environment: github-pages` und
  `url: ${{ steps.deployment.outputs.page_url }}`; Step-Reihenfolge
  `actions/checkout@v7` → `actions/configure-pages@v5` →
  `actions/upload-pages-artifact@v4` (`path: site`) → `actions/deploy-pages@v4`
  (`id: deployment`) → Smoke-Step mit `curl --fail --retry 5 --retry-delay 10
  --retry-all-errors` + Marker-String-Prüfung „FlowNRW"; YAML lädt fehlerfrei
  (Parser-Verifikation durchgeführt); Action-Versionen folgen der
  Repo-Konvention (andere Workflows nutzen `checkout@v7` u. ä.).
- [x] `docs/CI-CD.md` — alle vier geplanten Ergänzungen vorhanden:
  Workflow-Tabellenzeile (`pages.yml` | „Push auf `main` (`site/**`), manuell" |
  „Deploy der Projektseite nach GitHub Pages"), Einrichtungspunkt 6 unter
  „Einmalige Repo-Einrichtung (GitHub)" (Settings-Weg + `gh api`-Alternativbefehl
  + `GITHUB_TOKEN`-Hinweis), neuer Abschnitt „Projektseite (GitHub Pages)"
  (Quellordner, Deploy-Kette, Update-Weg inkl. `workflow_dispatch`,
  Screenshot-Kuratierung aus `icon-matrix/`), Fehlersuche-Eintrag
  „Get Pages site failed" → Quelle setzen + `workflow_dispatch`.
- [x] `README.md` — Badge/Verweis auf `https://martin-stromberg.github.io/FlowNRW/`
  im Kopfbereich und `site/`-Zeile in der Projektstruktur-Tabelle.
- [x] `changes.log` — datierter Eintrag „2026-11-16 — Statische Projektseite
  für GitHub Pages" inkl. Plattformfestlegung und einmaliger Voraussetzung.
- [x] `docs/RELEASE_NOTES.md` — Einträge in beiden Konventionsblöcken
  („What's New" und „Neuerungen") analog zum Infrastruktur-Eintrag vom
  2026-11-14.
- [x] Validierungsregeln — Screenshot-Auswahl (s. o.), interne Referenzen
  relativ / externe absolut, Inhaltsaussagen (kein Windows, iOS „In
  Entwicklung", plattformneutraler `releases/latest`-Verweis), keine externen
  Abhängigkeiten (Fonts lokal, kein CDN/Tracker), Kontrast-Tokens aus der
  dokumentierten `Colors.xaml`-Quelle übernommen.
- [x] Akzeptanzszenarien 1–3 (repo-seitiger Anteil) — Deploy-Kette,
  Fehlerfall-Dokumentation und Update-Weg sind wie geplant verankert;
  Szenario-4-Voraussetzungen (beide Themes, Umschalter, responsive Breakpoints,
  persistierte Wahl) sind implementiert.

## Offene Aufgaben

- [ ] `Umsetzungsreihenfolge Schritt 7: Einmalige Pages-Aktivierung und
  Go-Live-Abnahme` — nicht ausführbar vor dem Merge: Voraussetzungen sind
  Merge nach `staging`, Promotion-Merge nach `main` und Maintainer-Rechte.
  Auszuführen: Pages-Quelle „GitHub Actions" setzen (Settings → Pages oder
  `gh api -X POST repos/martin-stromberg/FlowNRW/pages -f build_type=workflow`),
  `pages.yml`-Lauf auf `main` beobachten (ggf. `workflow_dispatch`), öffentliche
  URL `https://martin-stromberg.github.io/FlowNRW/` auf Inhalte, Themes und
  Responsivität prüfen (Akzeptanzszenarien 1, 3 und 4 real abnehmen).

## Hinweise

- **Keine Nachimplementierung erforderlich.** Der Status „Offene Aufgaben
  vorhanden" spiegelt ausschließlich den planmäßig post-merge liegenden
  Go-Live-Schritt; es existiert keine Code-/Dokumentationslücke. Das Issue
  kann erst nach erfolgreicher Pages-Aktivierung und erstem Deploy als
  fachlich abgenommen gelten.
- **Screenshot-Motivabdeckung:** Der Satz erfüllt alle verbindlichen Kriterien
  (~6–8, hell+dunkel, kuratiert, sprechende Namen, fixture-frei). Die in
  Plan-Schritt 2 exemplarisch genannten Motive „Favoriten-Home" und
  „Verbindungsergebnisse" sind nicht enthalten — stattdessen liegen
  Verbindungssuche-Formular und Haltestellensuche vor; der Feature-Karte
  „Favoriten" steht kein Screenshot gegenüber. Optional ergänzbar
  (`icon-matrix/…-home-favorite-expanded.png`, `…-journey-results.png`),
  nicht gefordert.
- **Smoke-Check-Robustheit:** Der Hinweis aus `plan-check.md` wurde umgesetzt
  (`--retry-all-errors`) — CDN-Propagation beim Erstdeploy führt nicht zu
  einem falschen Negativ.
- **README-Badge zeigt bis zum ersten Deploy ins Leere** — wie in
  `plan-check.md` eingeordnet zeitlich unkritisch, da der Merge den Deploy
  auslöst.
- **Font-Lizenzhinweis im Footer** nennt zusätzlich „Material Symbols Rounded
  (Apache 2.0)", obwohl dieser Font nicht in `site/` gebündelt ist —
  konservative Attribution, kein Verstoß.
