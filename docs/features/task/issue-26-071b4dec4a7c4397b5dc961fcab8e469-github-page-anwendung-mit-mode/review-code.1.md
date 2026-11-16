# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### site/index.html

- **Fehlerbehandlung / Korrektheit externer Metadaten** — Zeile 12: `og:image` ist relativ angegeben (`content="./assets/flownrw.png"`). Open-Graph-Scraper (Social-Media-/Messenger-Vorschauen) verlangen absolute URLs; ein relativer Pfad wird nicht aufgelöst, die Link-Vorschau erscheint ohne Bild. `og:url` in Zeile 11 ist korrekt absolut.

  Empfehlung: `content="https://martin-stromberg.github.io/FlowNRW/assets/flownrw.png"` verwenden.

- **Falsche Deklaration / Layout-Stabilität** — Zeilen 22 und 53: `width`/`height` deklarieren quadratische Maße (`32×32`, `220×220`), das Asset `site/assets/flownrw.png` ist aber 657×619 px (nicht quadratisch). Browser leiten aus den Attributen ein falsches Seitenverhältnis für die Platzhalter-Berechnung ab → kleiner Layout-Shift beim Laden und faktisch falsche Angabe. Die gerenderte Darstellung ist durch CSS `height: auto` korrekt, nur die Deklaration stimmt nicht.

  Empfehlung: Attribute ans reale Seitenverhältnis anpassen (z. B. `width="220" height="208"` bzw. `width="32" height="30"`) oder ein quadratisch beschnittenes Icon-Asset verwenden.

- **Toter/falscher Lizenzhinweis** — Zeile 174: Der Footer nennt „Icon-Font Material Symbols Rounded (Apache License 2.0)", die Seite lädt oder verwendet diese Icon-Font aber nirgends (kein `@font-face`, kein `<link>`, keine Icon-Klassen). Die Nennung ist irreführend.

  Empfehlung: Den Material-Symbols-Teilsatz aus dem Footer entfernen (Inter, JetBrains Mono und OSM-Attribution bleiben korrekt).

- **Inkonsistente Darstellung in dunkler Stufe** — Zeile 13: `<meta name="theme-color" content="#0058BC">` ist statisch; Browser-Chrome bleibt in dunkler Darstellung hell-blau, während die Seite schwarzen Canvas (`--canvas: #000000` im Dark-Block) zeigt.

  Empfehlung: Zusätzlich `<meta name="theme-color" media="(prefers-color-scheme: dark)" content="#000000">` eintragen (oder den hellen Wert auf `media="(prefers-color-scheme: light)"` einschränken).

### site/styles.css

- **Toter Code / redundante Regel** — Zeilen 477–484: Im `@media (max-width: 640px)`-Block setzt `.card-grid, .screenshot-grid { grid-template-columns: 1fr; }` und unmittelbar danach `.screenshot-grid { grid-template-columns: repeat(2, 1fr); }`. Die erste Deklaration ist für `.screenshot-grid` wirkungslos — verwirrend und toter Code.

  Empfehlung: `.screenshot-grid` aus dem gemeinsamen Selektor entfernen (`grid-template-columns: 1fr` nur für `.card-grid`).

### site/theme.js

- **Randfall: Systemthema-Wechsel wird eingefroren** — Zeile 26 (`applyStored()`): Liegt keine gespeicherte Nutzerwahl vor, wird `data-theme` trotzdem auf den aktuellen Systemwert gesetzt. Damit ist das Theme anschließend statisch gepinnt; ein OS-Wechsel hell↔dunkel bei geöffneter Seite wird nicht mehr übernommen — obwohl der Nutzer nie manuell umgeschaltet hat (die `@media (prefers-color-scheme)`-Regeln werden durch das gesetzte Attribut ausgehebelt).

  Empfehlung: `data-theme` nur bei gespeicherter Wahl setzen und sonst das Attribut entfernen (Media-Query-Fallback greift), oder einen `change`-Listener auf das `matchMedia`-Objekt registrieren, der nur aktiv ist, solange keine gespeicherte Wahl existiert.

### .github/workflows/pages.yml

- **Inkonsistente/veraltete Action-Majors** — Zeilen 46, 49, 55: `actions/configure-pages@v5` (aktuell: v6), `actions/upload-pages-artifact@v4` (aktuell: v5; v4 erzeugt bekannte `DEP0040 punycode`-Deprecation-Warnungen im Log) und `actions/deploy-pages@v4` (aktuell: v5.0.1). Alle anderen Workflows des Repos pinnen durchgehend die aktuellen Majors (`checkout@v7`, `setup-dotnet@v6`, `upload-artifact@v7`); der neue Workflow bricht diese Einheitlichkeit und startet mit Deprecation-Rauschen.

  Empfehlung: Auf `configure-pages@v6`, `upload-pages-artifact@v5` und `deploy-pages@v5` anheben (Dependabot würde es sonst ohnehin nachziehen).

## Positiv festgehaltene Prüfpunkte (keine Befunde)

- **Anwenderfestlegung erfüllt:** `site/` nennt „Windows" nirgends; iOS ist die einzige genannte Zielplattform mit Status „In Entwicklung" (`index.html:140–148`); Release-Verweis plattformneutral auf `releases/latest`. Windows-Nennungen in `docs/CI-CD.md`/`README.md`/`changes.log` betreffen die Maintainer-Doku bzw. wörtliche Pfadnamen (`tests/WindowsJourneyUiTests/…`), nicht die öffentliche Seite — zulässig.
- **Workflow-Syntax/Berechtigungen korrekt:** `"on"`-Schlüssel gequotet (YAML-1.1-Falle vermieden), `permissions` minimal und vollständig (`pages: write`, `id-token: write`, `contents: read`), `concurrency` mit `cancel-in-progress: false` deploy-sicher, `environment: github-pages` mit `page_url`-Output, `timeout-minutes` gesetzt, Trigger auf `main` mit `paths`-Filter plus `workflow_dispatch`. `checkout@v7` konsistent zum Repo-Stand.
- **Smoke-Check robust:** `curl --fail --retry 5 --retry-all-errors`, Kommandosubstitution lässt den Schritt bei HTTP-Fehler korrekt fehlschlagen (`bash -e`).
- **Relative Pfade/Base-Pfad:** Alle internen Referenzen (`styles.css`, `theme.js`, `./assets/…`, Font-URLs im CSS) sind relativ — funktioniert unter dem Base-Pfad `/FlowNRW/`; alle acht referenzierten Screenshots sowie Fonts und Icon existieren, deklarierte Screenshot-Maße (430×900) stimmen mit den Dateien überein.
- **Sichere externe Links:** Ausschließlich `https://`-URLs auf `github.com/martin-stromberg/FlowNRW` und `polyformproject.org`; keine `target="_blank"`-Links, daher kein `rel="noopener"` erforderlich; kein Tracking/keine externen Abhängigkeiten.
- **HTML/CSS/JS-Qualität:** `lang="de"`, semantische Struktur (`header/nav/main/section/figure/footer`), saubere Heading-Hierarchie, `alt`-Texte, `aria-label`/`aria-pressed` am Umschalter, `loading="lazy"` für Galerie, `prefers-reduced-motion` berücksichtigt, No-JS-Fallback via `prefers-color-scheme` vorhanden, `localStorage`-Zugriffe try/catch-gesichert.
- **Dokumentation konsistent:** `docs/CI-CD.md` (Workflow-Tabelle, Einrichtungspunkt 6, Abschnitt „Projektseite", Fehlersuche-Eintrag), `README.md`-Badge und `site/`-Tabellenzeile, `changes.log`- und `docs/RELEASE_NOTES.md`-Einträge (de/en) decken sich mit der Implementierung; der dokumentierte CLI-Schritt `gh api -X POST repos/martin-stromberg/FlowNRW/pages -f build_type=workflow` ist korrekt.

## Geprüfte Dateien

- `.github/workflows/pages.yml`
- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/` (Asset-Referenzen und Bildmaße verifiziert: `flownrw.png` 657×619, 8 Screenshots je 430×900, 4 Font-Dateien)
- `README.md` (Diff gegen `staging`)
- `changes.log` (Diff gegen `staging`)
- `docs/CI-CD.md` (Diff gegen `staging`)
- `docs/RELEASE_NOTES.md` (Diff gegen `staging`)
- `docs/features/task/issue-26-071b4dec4a7c4397b5dc961fcab8e469-github-page-anwendung-mit-mode/requirement.md`, `plan.md`, `todo.md` (Kontext/Anwenderfestlegungen, gesichtet)
