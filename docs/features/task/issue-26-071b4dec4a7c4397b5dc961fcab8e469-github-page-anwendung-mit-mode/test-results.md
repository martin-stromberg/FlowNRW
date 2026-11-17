# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

**Hinweis zum Abschluss:** Ein geplantes reales Abnahmeszenario ist
`Nicht durchführbar` (Go-Live-/Besucher-Abnahme an der öffentlichen
GitHub-Pages-URL — vor Merge/Promotion technisch unmöglich, siehe Abschnitt
„Reale End-to-End-Abnahme"). Gemäß Lifecycle-Regel gilt `Nicht durchführbar`
nicht als Erfolg; das Szenario bleibt offen und erfordert eine
Anwender-Entscheidung (manuelle Abnahme nach dem Go-Live oder dokumentierter
Verzicht). Alle lokal durchführbaren Prüfungen und alle bestehenden
automatisierten Tests sind bestanden.

## Fehlgeschlagene Tests

Keine. (Das offene Abnahmeszenario ist unter „Reale End-to-End-Abnahme" als
`Nicht durchführbar` geführt, nicht als Testfehler.)

## Feature-Prüfungen (statisch und lokal, uncommittete Umsetzung)

### 1. Workflow `.github/workflows/pages.yml`

| Prüfung | Ergebnis |
|---------|----------|
| YAML-Parse (`yaml.safe_load`, PyYAML) | Bestanden — valides YAML |
| Trigger `push` auf `main` mit `paths: ["site/**", ".github/workflows/pages.yml"]` | Bestanden |
| Trigger `workflow_dispatch` | Bestanden (vorhanden) |
| `permissions: contents: read, pages: write, id-token: write` | Bestanden |
| `concurrency: group "pages", cancel-in-progress: false` | Bestanden |
| Ein Job `deploy` auf `ubuntu-latest`, `environment: github-pages` mit `url: ${{ steps.deployment.outputs.page_url }}` | Bestanden |
| Actions-Reihenfolge `checkout@v7` → `configure-pages@v6` → `upload-pages-artifact@v5` (`path: site`) → `deploy-pages@v5` (`id: deployment`) | Bestanden |
| Smoke-Step: `curl --fail --retry 5 --retry-delay 10 --retry-all-errors` auf `page_url`, Marker-String „FlowNRW" | Bestanden (inkl. `--retry-all-errors` — Hinweis aus `plan-check.md` umgesetzt) |
| Einmalige Pages-Aktivierung dokumentiert (Header-Kommentar + `docs/CI-CD.md`) | Bestanden |

### 2. HTML-/JS-/CSS-Syntax und interne Referenzen unter `site/`

| Prüfung | Ergebnis |
|---------|----------|
| `node --check site/theme.js` | Bestanden |
| HTML-Parse (`html.parser`, Tag-Balance) | Bestanden — keine ungeschlossenen/verwaisten Tags |
| Interne Referenzen (`src`/`href` mit `./…`) — 9 Referenzen | Bestanden — alle Dateien vorhanden |
| Anker-Links (`#top`, `#features`, `#screenshots`, `#release`, `#links`) | Bestanden — alle Ziel-IDs vorhanden |
| CSS `url()`-Referenzen (4 Fonts) | Bestanden — alle vorhanden |
| CSS-Klammerbilanz (80 Paare) | Bestanden |
| Keine hart verdrahteten Base-Pfade (`href="/…"`, `src="/…"`, `url("/…")`) | Bestanden — ausschließlich relative interne Pfade |
| Keine externen Skripte/Styles/Bilder (Datensparsamkeit, Offline-Fähigkeit) | Bestanden — externe URLs nur als `<a href>` (github.com, polyformproject.org) und `og:url` |
| `lang="de"`, Meta-/OG-Tags, Favicon, `viewport` | Bestanden |

### 3. Lokale Auslieferung (`python -m http.server`, `http://127.0.0.1:8317/`)

| Ressource | HTTP |
|-----------|------|
| `index.html`, `styles.css`, `theme.js` | 200 / 200 / 200 |
| `assets/flownrw.png` (657×619) | 200 |
| 4 Fonts (`Inter-{Regular,SemiBold,Bold}.ttf`, `JetBrainsMono-Bold.ttf`) | je 200 |
| 4 Screenshots (`assets/screenshots/*.png`, je 430×900) | je 200 |
| Marker-String „FlowNRW" im ausgelieferten HTML (spiegelt den Workflow-Smoke-Check) | Bestanden |

Summe: 12 referenzierte Ressourcen, alle HTTP 200, keine fehlenden Dateien.

### 4. Inhalts- und Sichtprüfung (Anwenderfestlegung)

| Prüfung | Ergebnis |
|---------|----------|
| Kein „Windows"-Text unter `site/` (`grep -ri "windows\|win-x64\|msix\|\.exe"` → 0 Treffer) | Bestanden |
| iOS als Zielplattform mit Status „In Entwicklung" (`index.html`: Chip „In Entwicklung", Hinweistext „iOS ist die Zielplattform … befindet sich derzeit in Entwicklung") | Bestanden |
| Release-Verweis plattformneutral auf die Release-Übersicht `releases` (kein „Windows-Download"-Framing) | Bestanden |
| Screenshot-Kuratierung: 4 PNGs, hell+dunkel, keine `failure-*`/`*-empty-*`/`*-offline`/`*-loading`-Namen | Bestanden |
| Sichtprüfung aller 4 Screenshots: echte App-Inhalte ohne Degradierungs-/Fallback-Zustände (Verbindungssuche, Einstellungen „Automatische Aktualisierung"), keine sichtbaren Fixture-Texte („UI-Fixture", „monitor-sequence", „KARTENFIXTURE") | Bestanden |
| Design-Anlehnung: Tokens aus `Colors.xaml` als Custom Properties (hell `:root`, dunkel `data-theme="dark"` + `prefers-color-scheme`), selbst gehostete Fonts | Bestanden (Quellcode-Sichtung; Browser-Renderprüfung entfällt headless) |
| Theme-Umschalter: `localStorage`-Persistenz + `prefers-color-scheme`-Default (`theme.js`-Sichtung) | Bestanden (logisch nachvollzogen; reale Browser-Bedienung post-merge) |
| Textkorrekturen (Usability-Iterationen): Hero-CTA und Release-CTA „Releases ansehen" (`index.html`); Feature-Karten ohne „Statuschips"/„explizite Entfernungssortierung" — Abfahrtsmonitor nennt Verspätungen/Ausfälle/Gleiswechsel direkt, Favoriten „nach Entfernung sortierbar", NRW-Echtzeit „Ersatzdaten" statt „Fallbacks", Karte „Kartenverläufe zu gefundenen Verbindungen"; Release-Panel nennt „vorhandene Desktop-Anwendung" (kein Windows) | Bestanden (Quellcode-Sichtung + `grep` auf entfernte Formulierungen → 0 Treffer) |

## E2E-Abdeckung

Geplante E2E-Szenarien aus `plan.md` Abschnitt „E2E-Tests" / `plan-check.md` Abschnitt „E2E-Abdeckung":

| Szenario | Test / Testklasse | Ergebnis |
|----------|-------------------|----------|
| (Pflicht) Erster Deploy nach Promotion liefert die Seite unter `https://martin-stromberg.github.io/FlowNRW/` aus | Reale Abnahme post-merge (Actions-Log + Browser) | Nicht ausgeführt — vor Merge/Promotion technisch nicht möglich (Deploy nur auf `main`, Pages-Aktivierung ausstehend, URL existiert noch nicht) |
| (Pflicht) Deploy-Smoke-Check schlägt fehl, wenn die Seite nicht ausgeliefert wird | Smoke-Step in `pages.yml` (`curl` + Marker) | Statisch verifiziert (Step vorhanden und korrekt); Lauf selbst erst post-merge ausführbar |
| (Pflicht) Besucher sieht responsives Layout und beide Themes | Reale Abnahme post-merge (Browser) | Lokal soweit möglich verifiziert (Auslieferung + Ressourcen + Code-Sichtung); reale Browser-Abnahme an öffentlicher URL post-merge offen |
| (Optional) `workflow_dispatch` löst Redeploy ohne Push aus | Reale Abnahme post-merge | Nicht ausgeführt — Workflow existiert noch nicht auf `main` |

## Reale End-to-End-Abnahme

Erforderlich (Benutzeroberfläche und externe Dienste/GitHub-Plattform betroffen).

| Szenario | Durchgeführt über | Ergebnis |
|----------|-------------------|----------|
| Lokale Auslieferung der Seite inkl. aller referenzierten Ressourcen (lokale Ersatzabnahme des Besucherflusses) | `python -m http.server 8317` in `site/`, HTTP-Prüfung aller 12 Referenzen + Marker-Check | Bestanden |
| Szenario 1 (Happy Path Go-Live): Promotion-Merge → `pages.yml`-Deploy → öffentliche URL zeigt Landingpage | — | Nicht durchführbar — Deploy triggert ausschließlich auf `main`; die Promotion `staging` → `main` und die einmalige Pages-Aktivierung (Maintainer-Schritt) liegen nach diesem Testlauf. Die öffentliche URL `https://martin-stromberg.github.io/FlowNRW/` existiert vor dem Go-Live nicht und kann technisch nicht geprüft werden. |
| Szenario 2 (Pages nicht aktiviert → verständlicher Fehler + Wiederaufnahme) | — | Nicht durchführbar — erfordert einen realen Workflow-Lauf auf `main`; Voraussetzung (Workflow auf `main` vorhanden) ist vor dem Merge nicht gegeben. |
| Szenario 3 (Inhaltsaktualisierung über PR-/Promotion-Kette) | — | Nicht durchführbar — setzt eine bereits live geschaltete Seite voraus. |
| Szenario 4 (Besucher-Darstellung: responsiv, hell/dunkel, Umschalter-Persistenz) | Lokal: `http.server` + Code-/Asset-Sichtung | Nicht durchführbar für die öffentliche URL — in dieser Umgebung steht kein Browser mit Rendering-/Persistenz-Abnahme für die Ziel-URL zur Verfügung; lokale Auslieferung und statische Verifikation der Mechanismen (`prefers-color-scheme`, `data-theme`, `localStorage`, Breakpoints) bestanden. |

**Begründung/Eskalation:** Die öffentliche GitHub-Pages-URL kann vor Merge und
Promotion technisch nicht geprüft werden — `pages.yml` deployt nur von `main`
(paths-gefiltert), und die Pages-Quelle „GitHub Actions" ist ein dokumentierter,
einmaliger Maintainer-Schritt nach dem Merge. Diese Szenarien sind bewusst als
reale Abnahme post-merge geplant (`plan.md` Umsetzungsreihenfolge Schritt 7).
Entscheidung beim Anwender: manuelle Abnahme nach Go-Live
(`Bestanden (manuell, Datum)`) oder dokumentierter Verzicht.

## Zusammenfassung

- Gesamt: 326
- Bestanden: 326
- Fehlgeschlagen: 0
- Übersprungen: 0

Befehle (jeweils im Repo-Root):

- `npm run test:release-version` → 26/26 bestanden
  (`node --test scripts/resolve-release-version.test.mjs scripts/generate-update-manifest.test.mjs`)
- `dotnet build FlowNRW.sln --nologo -v q` → 0 Warnungen, 0 Fehler (~14 s)
- `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj --no-build --logger "console;verbosity=normal"` → 300/300 bestanden

Zusätzlich (nicht in der Testzahl enthalten, siehe „Feature-Prüfungen"):
YAML-Parse `pages.yml`, HTML-Parse `site/index.html`, `node --check site/theme.js`,
interne Referenz-/Anker-Prüfung, lokale HTTP-Auslieferung mit 12× HTTP 200,
Inhaltsprüfung (kein Windows-Text, iOS „In Entwicklung"), Sichtprüfung aller
4 Screenshots.

`dotnet test` war ohne hohen Mehraufwand möglich (Solution-Build ~14 s, reiner
`net10.0`-Testlauf ~2 s) und wurde daher real ausgeführt statt auf die
Baseline (300/300 aus Issue #20) zu verweisen — Ergebnis unverändert 300/300.

## Testabdeckung

**Abdeckung:** Nicht messbar

Begründung: Die Umsetzung enthält keinen C#-/JS-Produktivcode mit
Test-Toolchain — `site/` ist bewusst buildfrei (statisches HTML/CSS/JS ohne
Generator, `plan.md` Designentscheidung „Build/Tooling"). Für `FlowNRW.Tests`
wurde kein Coverage-Lauf durchgeführt, da keinerlei .NET-Code geändert wurde
(`git status`: nur `site/`, `.github/workflows/pages.yml`, Doku); die
Bestehensrate der 300 Tests ohne Änderung am getesteten Code ist der
relevante Regressionsnachweis.

## Fehlende Tests

Keine relevanten Lücken. Für `site/` existiert bewusst kein automatisierter
Test (`plan.md` begründet: kein Benutzerfluss in der App, Plattformablauf vor
Merge nicht ausführbar, neue Toolchain nicht gerechtfertigt); der
Funktionsnachweis erfolgt über den im Workflow eingebauten Deploy-Smoke-Check
und die reale Abnahme post-merge. `pages.yml` wurde per YAML-Parse und
inhaltlicher Prüfung verifiziert.
