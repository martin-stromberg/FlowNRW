# Tasks: GitHub Page – Anwendung mit modernem Stil präsentieren

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Seitenquellen | `site/index.html` anlegen (Header/Nav mit Theme-Umschalter, Hero mit CTAs, Feature-Karten, Screenshot-Galerie, Release-/Plattformhinweis: plattformneutraler Release-Verweis `releases/latest`, iOS als Zielplattform „In Entwicklung", keine Windows-Nennung; Verweise, Footer; `lang="de"`, Meta/OG-Tags, Favicon, relative interne Pfade) | Offen | — |
| 2 | Seitenquellen | `site/styles.css` anlegen (CSS-Custom-Properties aus `Colors.xaml`-Tokens hell/dunkel, responsives Layout, Typografie-Hierarchie, `@font-face`-Deklarationen, Kontraste ≥ 4,5:1) | Offen | — |
| 3 | Seitenquellen | `site/theme.js` anlegen (`prefers-color-scheme`-Default + manueller Umschalter mit `localStorage`-Persistenz via `data-theme`) | Offen | — |
| 4 | Assets | Fonts aus `FlowNRW/Resources/Fonts/` nach `site/assets/fonts/` kopieren (`Inter-Regular.ttf`, `Inter-SemiBold.ttf`, `Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf`) | Offen | — |
| 5 | Assets | App-Icon/Favicon nach `site/assets/` kopieren (`FlowNRW/Resources/AppIcon/flownrw.png` oder `docs/design/reference/flownrw_icon.png`) | Offen | — |
| 6 | Assets | Kuratierten Screenshot-Satz (~6–8, mind. hell + dunkel) aus `docs/help/design/verification/icon-matrix/` mit sprechenden Namen nach `site/assets/screenshots/` kopieren | Offen | — |
| 7 | Workflow | `.github/workflows/pages.yml` anlegen (`push: main` mit `paths`-Filter + `workflow_dispatch`; `pages`/`id-token`-Permissions; Job `deploy` mit `environment: github-pages`; checkout → configure-pages → upload-pages-artifact (`path: site`) → deploy-pages → `curl`-Smoke-Check) | Offen | — |
| 8 | Dokumentation | `docs/CI-CD.md` ergänzen: Workflow-Tabellenzeile `pages.yml`, Einrichtungspunkt „Pages-Quelle: GitHub Actions" (Settings-Weg + `gh api`-Alternativbefehl), Abschnitt „Projektseite (GitHub Pages)" (Quellordner, Update-Weg, Screenshot-Kuratierung, manueller Redeploy), Fehlersuche-Eintrag | Offen | — |
| 9 | Dokumentation | `README.md` ergänzen: Verweis auf `https://martin-stromberg.github.io/FlowNRW/` im Kopf + `site/`-Zeile in der Projektstruktur-Tabelle | Offen | — |
| 10 | Dokumentation | `changes.log` und `docs/RELEASE_NOTES.md` um datierte Einträge zur Projektseite ergänzen | Offen | — |
| 11 | Verifikation | `pages.yml` mit YAML-Parser laden; Trigger, Permissions, `environment` und Actions-Reihenfolge gegen den Plan prüfen | Offen | — |
| 12 | Verifikation | `site/` lokal per `python -m http.server`/`npx serve` prüfen: Layout schmal/breit, beide Themes, Umschalter-Persistenz, alle Links/Bildpfade | Offen | — |
| 13 | Verifikation | Go-Live-Abnahme post-merge: Maintainer setzt Pages-Quelle „GitHub Actions" (einmalig), `pages.yml`-Lauf auf `main` beobachten, öffentliche URL auf Inhalte/Themes/Responsivität prüfen | Offen | — |
