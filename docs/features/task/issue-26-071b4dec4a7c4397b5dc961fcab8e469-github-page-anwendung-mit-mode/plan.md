# Umsetzungsplan: GitHub Page – Anwendung mit modernem Stil präsentieren (Issue #26)

## Übersicht

Eine statische, buildfreie Projektseite unter einem neuen Quellordner `site/` im
Repository-Wurzelverzeichnis präsentiert FlowNRW (Zweck, zentrale Features,
kuratierte Screenshots, Release-/Repo-/Doku-Verweise) mit dem bestehenden
Design-Token-System der App in heller und dunkler Darstellung. Die Auslieferung
erfolgt per GitHub-Actions-Deployment über einen neuen Workflow
`.github/workflows/pages.yml` nach `https://martin-stromberg.github.io/FlowNRW/`;
die Pflege- und Einrichtungsdoku liegt in `docs/CI-CD.md`. Keine Änderungen an
`FlowNRW`, `FlowNRW.Core` oder `FlowNRW.Tests`.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Die statische Projektseite als Ganzes: Seitenquellen unter `site/` (HTML/CSS/JS), kuratierte Assets (Icon, Fonts, Screenshots) und der Deploy-Lauf `pages.yml` |
| Auslösende Ereignisse | Push auf `main`, der `site/**` oder den Workflow ändert (erreicht `main` über die projektübliche Kette Feature-Branch → `staging` → Promotion-PR); manueller `workflow_dispatch`. Für den Besucher: Aufruf der öffentlichen URL |
| Erwartete Wirkung | Öffentlich erreichbare, responsive Seite unter `https://martin-stromberg.github.io/FlowNRW/` mit Zweck, zentralen Features, Screenshots, plattformneutralem Release-Verweis (`releases/latest`), Plattformhinweis iOS mit Status „In Entwicklung" und Verweisen auf Repository/Dokumentation — hell und dunkel, an das App-Design angelehnt; Windows wird auf der Seite nicht als Ziel-/Nutzerplattform genannt |
| Umgang mit fehlenden Objekten | **Pages-Site-Konfiguration** fehlt → einmalig durch den Maintainer erzeugen (Settings → Pages → Source „GitHub Actions" oder `gh api -X POST repos/martin-stromberg/FlowNRW/pages -f build_type=workflow`); `GITHUB_TOKEN` kann das nicht (Repo-Administration), daher dokumentierter Pflichtschritt statt Automatismus. **`github-pages`-Environment** fehlt → wird vom ersten `deploy-pages`-Lauf automatisch angelegt. **Quellordner `site/`** fehlt → in dieser Umsetzung neu erzeugen. **Präsentationsfähige Screenshots** → kuratieren: Auswahl aus `docs/help/design/verification/icon-matrix/` (neuester Designstand) nach `site/assets/screenshots/` kopieren; `native-*.png` nicht verwenden (sichtbare Test-Fixture-Felder wie „monitor-sequence", „UI-Fixture", „KARTENFIXTURE"); Neuaufnahmen nur als spätere Option. **Ziel-URL/Base-Pfad `/FlowNRW/`** → vorgegeben, nichts zu erzeugen; interne Referenzen relativ halten |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Keine dynamischen/serverseitigen Funktionen — keine Live-Fahrplandaten, kein Backend | Anforderung verlangt eine statische Seite | bestätigt (2026-11-16) |
| 2 | Keine Custom Domain / Subdomains; ausschließlich die Projekt-URL | Anforderung nennt nur die GitHub-Pages-URL | bestätigt (2026-11-16) |
| 3 | Kein Store-/MSIX-/Installer-Angebot; „Download" meint Verweis auf GitHub-Releases; der Verweis wird plattformneutral geframed (kein „Windows-Download" auf der Seite) | Release-Pipeline liefert `FlowNRW-vX.Y.Z-win-x64.zip`; stabile URL `releases/latest`; Windows ist nur Entwicklungsplattform und darf nicht als Zielplattform erscheinen (Anwenderfestlegung 2026-11-16) | bestätigt (2026-11-16) |
| 4 | Keine Mehrsprachigkeit — Seite auf Deutsch | Gesamte Projektdokumentation ist deutsch | bestätigt (2026-11-16) |
| 5 | Kein Analytics/Tracking, keine Cookies, kein Newsletter, kein Kontaktformular | Nicht angefordert; datensparsame statische Seite | bestätigt (2026-11-16) |
| 6 | Keine Änderung an App, Release-Pipeline oder Quality Gates — additives Artefakt | Scope der Anforderung ist die Präsentationsseite | bestätigt (2026-11-16) |
| 7 | Windows wird auf der Seite nicht als Ziel-/Nutzerplattform genannt; iOS wird als Zielplattform mit Status „In Entwicklung" dargestellt | Anwenderfestlegung 2026-11-16: Windows ist nur die Entwicklungsplattform, iOS ist das Ziel; der Status „In Entwicklung" darf gezeigt werden und vermeidet die Überzeichnung des Funktionsstands | bestätigt (2026-11-16) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|------------------|------------|
| Quellordner | `site/` im Repository-Root | `docs/` ist mit ~31 MB Doku, Arbeitsverzeichnissen (`docs/features/task/`) und Screenshot-Matrizen belegt — eine klassische „Deploy from branch aus `/docs`"-Quelle würde all das öffentlich ausliefern; `site/` ist sauber getrennt und wird als eigene Zeile in der README-Projektstruktur dokumentiert |
| Deployment-Mechanismus | GitHub-Actions-Deployment (`actions/configure-pages` → `actions/upload-pages-artifact` → `actions/deploy-pages`) statt „Deploy from branch" | Harmoniert mit dem bestehenden `.github/workflows/`-Muster; das deployte Artefakt enthält ausschließlich `site/`; kein Jekyll, kein `.nojekyll` nötig (Artefakt-Deployment liefert Dateien unverändert aus) |
| Veröffentlichender Branch | `main` — Push-Trigger mit `paths`-Filter (`site/**`, Workflow-Datei), zusätzlich `workflow_dispatch` | `main` trägt die stabilen Releases, auf die die Seite verlinkt (`releases/latest`); der Go-Live folgt der projektüblichen Promotion `staging` → `main`; kein RC-/Integrationsstand in der Öffentlichkeit |
| Build/Tooling | Buildfrei: statisches HTML/CSS/JS, kein Generator, keine neuen npm-/NuGet-Abhängigkeiten | Anforderung „statische Seite"; Inhalte werden direkt im Repo gepflegt; kein Build-Step im Workflow nötig |
| Seitenumfang | Eine Landingpage (`site/index.html`) | Die Anforderung verlangt eine prägnante Präsentation, keine Dokumentations-Site; Doku-Verweis verlinkt auf `docs/` im Repo |
| Plattformdarstellung | iOS als Zielplattform mit Status „In Entwicklung"; Windows wird auf der Seite nicht als Ziel-/Nutzerplattform genannt | Anwenderfestlegung 2026-11-16: Windows ist nur die Entwicklungsplattform, iOS ist das Ziel; der Status „In Entwicklung" darf gezeigt werden. Der Release-Verweis (`releases/latest`) bleibt wie vorgeschlagen, wird aber plattformneutral geframed — kein „Windows-Download" |
| Hell/Dunkel | `prefers-color-scheme` als Default plus manueller Umschalter mit `localStorage`-Persistenz | Anforderung verlangt beide Darstellungen ohne Mechanismus-Vorgabe; System-Default entspricht erwarteter UX, Umschalter gibt Kontrolle |
| Farb-/Typo-Tokens | CSS-Custom-Properties gespiegelt aus `Colors.xaml` (Canvas/Card/Ink/Muted/Outline/Primary/Semantik je hell+dunkel); selbst gehostete Schriften Inter (Regular/SemiBold/Bold) und JetBrains Mono (Bold) als Kopien aus `FlowNRW/Resources/Fonts/` | „An das Erscheinungsbild angelehnt" heißt verbindliches Token-System (Kontrastwerte in `Colors.xaml`/`acceptance.md` dokumentiert); SIL OFL/Apache-2.0 erlauben Web-Einbettung; keine externen Font-CDNs (Datensparsamkeit, Offline-Fähigkeit) |
| Screenshots | Kuratierter Satz (~6–8) aus `docs/help/design/verification/icon-matrix/` (Stand 14./15.10.2026, finales Icon-/Font-Design), kopiert mit sprechenden Dateinamen | `icon-matrix` zeigt den aktuellen, präsentationsfähigen Stand ohne sichtbare Testfelder; `native-*.png` und `final-matrix` enthalten sichtbare Fixture-Elemente bzw. älteres Design; Kopien entkoppeln die Seite von den Nachweisdateinamen |
| Pages-Aktivierung | Dokumentierter einmaliger Maintainer-Schritt, kein `enablement`-Automatismus im Workflow | `actions/configure-pages`-`enablement` erfordert PAT/GitHub-App — `GITHUB_TOKEN` darf keine Repo-Administration; der Schritt gehört konventionsgemäß in `docs/CI-CD.md` „Einmalige Repo-Einrichtung (GitHub)" |
| Pflege-Doku-Ort | `docs/CI-CD.md` (Workflow-Tabelle, Einrichtung, Update-Weg, Fehlersuche) | Präzedenz Issue #20: `docs/help/` ist App-Anwenderhilfe; Repo-Automatisierung/-Infrastruktur wird in `docs/CI-CD.md` dokumentiert |

## Programmabläufe

### Erstdeployment (Go-Live)

1. **Einmalig:** Maintainer stellt in den Repo-Einstellungen Pages → „Build and
   deployment" → Source auf **„GitHub Actions"** (oder führt
   `gh api -X POST repos/martin-stromberg/FlowNRW/pages -f build_type=workflow`
   aus). Dokumentiert in `docs/CI-CD.md` „Einmalige Repo-Einrichtung".
2. Der Feature-PR wird nach `staging` gemerged; `staging-ci.yml` läuft regulär
   (die Quality Gates betreffen keinen .NET-Code der Seite); anschließend erzeugt
   `staging-to-main-promotion.yml` den Draft-Promotion-PR `staging` → `main`.
3. Der Maintainer merged den Promotion-PR → der Push auf `main` enthält
   `site/**` und `pages.yml` → `pages.yml` wird durch den `paths`-Filter
   ausgelöst.
4. Job `deploy` (auf `ubuntu-latest`, Environment `github-pages`):
   `actions/checkout` → `actions/configure-pages` →
   `actions/upload-pages-artifact` (Pfad `site/`) → `actions/deploy-pages`
   (liefert `page_url`); das `github-pages`-Environment wird beim ersten Lauf
   automatisch angelegt.
5. Smoke-Step: `curl --fail --retry …` gegen `page_url` prüft HTTP 200 und
   einen Marker-String („FlowNRW") im ausgelieferten HTML.
6. Sichtbare Wirkung: `https://martin-stromberg.github.io/FlowNRW/` zeigt die
   Landingpage.

Beteiligte Komponenten: `.github/workflows/pages.yml`, `site/`, GitHub Pages
(`github-pages`-Environment), `staging-to-main-promotion.yml` (liefert den
Push auf `main`).

### Inhalts- und Screenshot-Aktualisierung

1. Inhalte/Markup/Styles unter `site/` auf einem Feature-Branch ändern → PR
   gegen `staging` → Promotion → Merge nach `main`.
2. Push auf `main` mit `site/**`-Änderung → `pages.yml` deployt die neue
   Artefaktversion unter derselben URL (idempotent).
3. Screenshots: neue Aufnahmen (z. B. neuer `icon-matrix`-Lauf über
   `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`) → kuratieren → nach
   `site/assets/screenshots/` kopieren → gleicher PR-Weg.

Beteiligte Komponenten: `site/`, `pages.yml`, PR-/Promotion-Kette.

### Blockierter Deploy ohne Pages-Aktivierung (Fehlerfall)

1. Push auf `main` erfolgt, bevor die Pages-Quelle auf „GitHub Actions" steht.
2. `actions/configure-pages` schlägt mit „Get Pages site failed … Not Found"
   fehl → Workflow-Lauf rot, sichtbare Annotation.
3. Wiederaufnahme: Maintainer setzt die Quelle (siehe `docs/CI-CD.md`,
   Fehlersuche) und startet den Workflow per `workflow_dispatch` erneut —
   kein halbfertiger Zustand wird veröffentlicht.

## Neue Klassen

Keine — es werden ausschließlich statische Dateien, ein YAML-Workflow und
Dokumentation angelegt.

Neue Dateien:

| Datei | Zweck |
|-------|-------|
| `site/index.html` | Landingpage auf Deutsch: Header/Nav mit Theme-Umschalter, Hero (App-Icon, Zweck, CTA `releases/latest` + Repository), Feature-Karten, Screenshot-Galerie, Release-/Plattformhinweis (plattformneutraler Release-Verweis, iOS als Zielplattform „In Entwicklung", keine Windows-Nennung), Verweise (Repo, `docs/`, Lizenz), Footer (PolyForm-Lizenz, Font-Lizenzen, OSM-Attribution); `lang="de"`, Meta/OG-Tags, Favicon, ausschließlich relative interne Pfade |
| `site/styles.css` | CSS-Custom-Properties aus den `Colors.xaml`-Tokens für `:root`/`[data-theme="dark"]`, responsives Layout (Kartenraster, Breakpoints), Typografie-Hierarchie, `@font-face`-Deklarationen |
| `site/theme.js` | Theme-Umschalter: Initialzustand aus `localStorage` bzw. `prefers-color-scheme`, setzt `data-theme` auf `<html>`, persistiert die Wahl |
| `site/assets/` | `flownrw.png`/`flownrw_icon.png` (App-Icon aus `FlowNRW/Resources/AppIcon/` bzw. `docs/design/reference/`) als Logo/Favicon |
| `site/assets/fonts/` | Kopien von `Inter-Regular.ttf`, `Inter-SemiBold.ttf`, `Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf` aus `FlowNRW/Resources/Fonts/` |
| `site/assets/screenshots/` | Kuratierter Satz (~6–8 PNGs, mindestens ein helles und ein dunkles Motiv) aus `docs/help/design/verification/icon-matrix/` mit sprechenden Namen (z. B. `abfahrten-hell.png`, `verbindungen-dunkel.png`, `haltestellenkarte.png`) |
| `.github/workflows/pages.yml` | Deploy-Workflow (siehe Umsetzungsreihenfolge Schritt 3) |

## Änderungen an bestehenden Klassen

Keine C#-Änderungen. Dokumentationsänderungen:

### `docs/CI-CD.md` (Dokumentation)

- **Workflow-Tabelle:** neue Zeile `pages.yml` | „Push auf `main` (`site/**`), manuell" | „Deploy der Projektseite nach GitHub Pages".
- **„Einmalige Repo-Einrichtung (GitHub)":** neuer Punkt — Pages-Quelle auf
  „GitHub Actions" stellen (Settings-Weg und `gh api`-Alternativbefehl);
  Hinweis, dass `GITHUB_TOKEN` das nicht automatisieren kann.
- **Neuer Abschnitt „Projektseite (GitHub Pages)":** Quellordner `site/`,
  Update-Weg (PR → `staging` → Promotion → `main` → Auto-Deploy),
  Screenshot-Kuratierung (Quelle `icon-matrix/`, Ziel `site/assets/screenshots/`),
  manueller Redeploy per `workflow_dispatch`.
- **„Fehlersuche":** neuer Eintrag — `configure-pages` schlägt mit
  „Get Pages site failed" fehl → Pages-Quelle prüfen/setzen, danach
  `workflow_dispatch`.

### `README.md` (Dokumentation)

- **Neuer Verweis:** Link auf `https://martin-stromberg.github.io/FlowNRW/`
  (Badge oder Textzeile im Kopfbereich).
- **Projektstruktur-Tabelle:** neue Zeile `site/` | „Statische GitHub-Page
  (Präsentationsseite), Deploy via `pages.yml`".

### `changes.log` / `docs/RELEASE_NOTES.md` (Dokumentation)

- Datierter `changes.log`-Eintrag über die neue Projektseite und den
  Deploy-Workflow; `RELEASE_NOTES.md`-Eintrag nach Konvention der letzten
  Infrastruktur-Features (vgl. Eintrag vom 2026-11-14).

## Datenbankmigrationen

Keine.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| Screenshot-Auswahl | Nur präsentationsfähige Motive: keine `failure-*`, `*-provider-error`, `*-offline`, `*-loading`, `*-empty-*`-Namen; keine Bilder mit sichtbaren Test-/Fixture-Feldern („UI-Fixture", „monitor-sequence", „KARTENFIXTURE") | Öffentliche Seite zeigt Testartefakte |
| Interne Referenzen | Ausschließlich relative Pfade (`./assets/…`); externe Links absolut (`https://github.com/martin-stromberg/FlowNRW/…`); Base-Pfad `/FlowNRW/` nicht hart verdrahten | Assets/Anker unter der Projekt-URL nicht erreichbar |
| Inhaltsaussagen | Kein ungeprüfter Funktionsstand: iOS als Zielplattform mit Status „In Entwicklung"; Windows wird nirgends auf der Seite als Ziel-/Nutzerplattform genannt; Release-Hinweis plattformneutral auf `releases/latest` (kein „Windows-Download"-Framing, keine Windows-Voraussetzung) | Windows-Nennung würde eine falsche Zielplattform suggerieren; überzeichneter iOS-Stand → Vertrauensverlust |
| Externe Abhängigkeiten | Keine externen Skripte/CDNs/Tracker; Fonts lokal aus `site/assets/fonts/` | Datensparsamkeits-/Statik-Verstoß, Offline-Bruch |
| Kontraste | Text ≥ 4,5:1, bedienrelevante Konturen ≥ 3:1 in beiden Themes (Token-Werte aus `Colors.xaml` sind dafür dokumentiert) | Verstoß gegen die verbindlichen Abnahmekriterien |

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| Pages „Build and deployment"-Quelle | Repo-Setting (einmalig manuell) | — | „GitHub Actions" als Quelle; Voraussetzung für `deploy-pages` |
| `github-pages`-Environment | GitHub-Environment | automatisch beim ersten Deploy | Deployment-Ziel; keine Schutzregeln vorgesehen |

## Seiteneffekte und Risiken

- **Quality Gates unberührt:** `site/` und `pages.yml` enthalten keinen .NET-Code;
  `dotnet format`, `FlowNRW.Tests` und der MAUI-Build sind unverändert —
  `pr-staging-ci.yml`/`staging-ci.yml` laufen regulär durch.
- **Go-Live erst auf `main`:** Der erste Deploy erfolgt erst nach dem
  Promotion-Merge (plus einmaliger Pages-Aktivierung). Auf `staging` allein
  passiert sichtbar nichts — bewusst, siehe Designentscheidung.
- **`paths`-Filter:** Pushes auf `main` ohne `site/`-Änderung lösen kein
  Redeploy aus (gewollt); `workflow_dispatch` bleibt als manueller Weg.
- **Conventional Commits:** Änderung ohne App-Code → `docs:`/`ci:`/`chore:`-Commit
  erzeugt kein Release; konsistent, da nichts an der App geändert wird.
- **Repo-Wachstum:** ~6–8 kuratierte PNGs plus ~2 MB Font-Kopien — moderat,
  gegenüber den ~23 MB bestehender Matrizen vernachlässigbar.
- **`staging-to-main-promotion.yml`-Kopplung:** Keine Änderung nötig — der
  Deploy folgt passiv dem vorhandenen Promotion-Mechanismus.

## Umsetzungsreihenfolge

1. **`site/`-Seitenquellen anlegen**
   - Voraussetzungen: Keine
   - Beschreibung: `site/index.html`, `site/styles.css`, `site/theme.js`
     erstellen. Sektionen: Header/Nav (Icon, Name, Anker-Links, Theme-Umschalter),
     Hero (Zweck: „bundesweite ÖPNV-Verbindungen und Abfahrten mit bevorzugten
     NRW-Echtzeitdaten", CTAs → `releases/latest` + Repository), Feature-Karten
     (Verbindungssuche mit Umstiegen/Fußwegen, Abfahrtsmonitor mit
     Echtzeit-Statuschips, Favoriten mit Abfahrtstafeln/Entfernungssortierung,
     Haltestellenkarte + nahe Haltestellen, automatische Monitorintervalle,
     NRW-Echtzeitpriorität mit bundesweiter Ergänzung), Screenshot-Galerie,
     Release-/Plattform-Abschnitt (plattformneutraler Release-Verweis
     `releases/latest` — kein „Windows-Download"-Framing; iOS als Zielplattform
     mit Status „In Entwicklung"; Windows wird auf der Seite nicht genannt),
     Verweise (Repository, `docs/`/`docs/help/index.md`,
     Lizenz), Footer (PolyForm-Noncommercial-Hinweis, Font-Lizenzen,
     OpenStreetMap-Attribution). Tokens aus `Colors.xaml` als
     CSS-Custom-Properties (hell via `:root`, dunkel via `data-theme="dark"` +
     `prefers-color-scheme`-Fallback), Kontraste ≥ 4,5:1.

2. **Assets bereitstellen**
   - Voraussetzungen: Schritt 1 (Ordnerstruktur und Referenzen stehen)
   - Beschreibung: Fonts aus `FlowNRW/Resources/Fonts/` nach
     `site/assets/fonts/` kopieren; App-Icon (`FlowNRW/Resources/AppIcon/flownrw.png`
     oder `docs/design/reference/flownrw_icon.png`) und Favicon nach
     `site/assets/`; kuratierten Screenshot-Satz aus
     `docs/help/design/verification/icon-matrix/` auswählen (mind. je ein
     helles/dunkles Motiv; Favoriten-Home, Verbindungsergebnisse,
     Verbindungsdetails, Haltestellenkarte, Abfahrtsmonitor) und unter
     sprechenden Namen nach `site/assets/screenshots/` kopieren.

3. **Deploy-Workflow `.github/workflows/pages.yml` anlegen**
   - Voraussetzungen: Schritt 1 (`site/` liegt im Upload-Pfad)
   - Beschreibung: Workflow nach Repo-Konvention (englischer Header-Kommentar,
     sprechender `name:`). Trigger: `push` auf `main` mit
     `paths: ['site/**', '.github/workflows/pages.yml']` plus
     `workflow_dispatch`. `permissions: contents: read, pages: write,
     id-token: write`; `concurrency: group "pages", cancel-in-progress: false`.
     Ein Job `deploy` auf `ubuntu-latest` mit `environment: name: github-pages,
     url: ${{ steps.deployment.outputs.page_url }}`; Steps:
     `actions/checkout` → `actions/configure-pages` →
     `actions/upload-pages-artifact` (`path: site`) → `actions/deploy-pages`
     (`id: deployment`) → Smoke-Step (`curl --fail --retry 5 --retry-delay 10`
     auf `page_url`, Marker-String „FlowNRW" im HTML prüfen).

4. **`docs/CI-CD.md` ergänzen**
   - Voraussetzungen: Schritt 3
   - Beschreibung: Workflow-Tabellenzeile, Einrichtungspunkt (Pages-Quelle),
     Abschnitt „Projektseite (GitHub Pages)", Fehlersuche-Eintrag — siehe
     Abschnitt „Änderungen an bestehenden Klassen".

5. **README, `changes.log`, `docs/RELEASE_NOTES.md` ergänzen**
   - Voraussetzungen: Schritte 1–4
   - Beschreibung: Page-Verweis im README-Kopf, `site/`-Zeile in der
     Projektstruktur-Tabelle, datierte Einträge.

6. **Lokale Verifikation**
   - Voraussetzungen: Schritte 1–3
   - Beschreibung: `site/` per `python -m http.server`/`npx serve` lokal
     ausliefern und prüfen: Layout schmal (~430 px) und breit (~1280 px),
     beide Themes, Umschalter-Persistenz, alle Links/Anker/Bildpfade.
     `pages.yml` mit YAML-Parser laden; Trigger, Permissions,
     Actions-Reihenfolge und `environment` gegen die Anforderung prüfen.

7. **Einmalige Pages-Aktivierung und Go-Live-Abnahme (Maintainer, post-merge)**
   - Voraussetzungen: Merge nach `staging`, Promotion-Merge nach `main`,
     Maintainer-Rechte auf dem Repository
   - Beschreibung: Pages-Quelle „GitHub Actions" setzen, `pages.yml`-Lauf auf
     `main` beobachten (ggf. `workflow_dispatch`), öffentliche URL
     `https://martin-stromberg.github.io/FlowNRW/` aufrufen: Inhalte
     (Zweck/Features/Download-Verweis), Themes und Responsivität prüfen.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| YAML-Parse `pages.yml` | (Skript-/Workflow-Prüfung, kein Testprojekt) | Valides YAML; Trigger (`push: main` + `paths` + `workflow_dispatch`), Permissions (`pages`/`id-token`), `environment: github-pages`, Actions-Reihenfolge |
| Lokale Render-/Link-Prüfung | (manueller Preview-Lauf) | Seite rendert ohne Fehler in schmaler/breiter Ansicht; beide Themes; Bildpfade und externe Links erreichbar |
| Deploy-Smoke-Check | (Step in `pages.yml`) | `curl`-Prüfung auf `page_url`: HTTP 200 + Marker-String → automatisierter Nachweis „Seite ausgeliefert" im selben Lauf |

### Betroffene bestehende Tests

Keine. `FlowNRW.Tests` (xUnit, testet `FlowNRW.Core`) und die
Release-Skripttests bleiben unverändert; keine C#- oder Skriptänderung.

### E2E-Tests (primärer Funktionsnachweis)

Die Anforderung betrifft keinen Benutzerfluss innerhalb der App — kein UI-, API-
oder Interaktionsablauf in `FlowNRW`. Der „Benutzerfluss" ist ein
GitHub-Plattformablauf (Push → Workflow → Pages-Deployment) plus der
Besucher-Abruf der öffentlichen URL. Repo-seitig ist dafür kein automatisierter
E2E-Test einführbar, ohne eine neue Test-Toolchain einzuführen; der
Funktionsnachweis erfolgt über den eingebauten Deploy-Smoke-Check und die reale
Abnahme nach dem Merge/Promotion. Vor dem Merge ist der Ablauf technisch nicht
ausführbar (Deploy nur auf `main`, Pages-Aktivierung erforderlich).

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Pflicht | Erster Deploy nach Promotion liefert die Seite unter `https://martin-stromberg.github.io/FlowNRW/` aus | Reale Abnahme post-merge (Actions-Log + Browser) | Öffentliche Erreichbarkeit, vollständige Inhalte | Wirkung liegt auf der GitHub-Plattform, nicht im Code |
| Pflicht | Deploy-Smoke-Check schlägt fehl, wenn die Seite nicht ausgeliefert wird | Step in `pages.yml` | Öffentliche Erreichbarkeit (automatisiert) | Integrierter Nachweis direkt nach dem Deploy |
| Pflicht | Besucher sieht responsives Layout und beide Themes | Reale Abnahme post-merge (Browser, schmal/breit, System-Theme + Umschalter) | Responsivität, helle/dunkle Darstellung | Nur real darstellbar |
| Optional | `workflow_dispatch` löst Redeploy ohne Push aus | Reale Abnahme post-merge | Manueller Update-Weg dokumentiert | Pflege-Szenario |

Bestehende E2E-Tests (Windows-UIA-Skripte unter `tests/WindowsJourneyUiTests/`)
sind nicht betroffen — keine App-Änderung.

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path Go-Live | Maintainer merged Promotion-PR `staging` → `main` | Pages-Quelle bereits auf „GitHub Actions" gesetzt; `site/` + `pages.yml` sind im Merge enthalten; kein `github-pages`-Environment vorhanden | Push auf `main` → `pages.yml` → checkout → configure-pages → upload-pages-artifact → deploy-pages (legt Environment an) → Smoke-Check | `https://martin-stromberg.github.io/FlowNRW/` zeigt die Landingpage mit Zweck, Features, Screenshots, plattformneutralem Release-Verweis und iOS-Hinweis „In Entwicklung" (keine Windows-Nennung) | Actions-Log (grün, `page_url`) + realer URL-Abruf |
| 2 | Fehlende Voraussetzung: Pages nicht aktiviert | Push auf `main` (oder `workflow_dispatch`) | Pages-Quelle steht nicht auf „GitHub Actions" | `configure-pages` schlägt mit „Get Pages site failed" fehl | Workflow-Lauf rot mit verständlicher Annotation; keine Teil-Veröffentlichung; Behebung in `docs/CI-CD.md` beschrieben; danach `workflow_dispatch` → Szenario 1 | Actions-Log + Maintainer-Dokumentation |
| 3 | Inhaltsaktualisierung | Änderung unter `site/` auf Feature-Branch | Seite ist live (Szenario 1) | PR → `staging` → Promotion → Merge nach `main` → `pages.yml` deployt neue Artefaktversion | Aktualisierter Inhalt unter derselben URL | Actions-Log + URL-Abruf |
| 4 | Besucher-Darstellung | Aufruf der URL auf Smartphone und Desktop | Seite ist live | Seite laden; System-Theme hell bzw. dunkel; Umschalter betätigen; Breite ändern | Responsive Umbrüche; helle/dunkle Darstellung konsistent; Umschalterwahl bleibt nach Reload erhalten | Reale Abnahme (Browser) |

## Offene Punkte

Keine. Alle zuvor offenen Punkte wurden durch die Anwenderantwort vom
2026-11-16 geklärt und sind im Plan eingearbeitet: Sprache Deutsch,
kuratierter Screenshot-Satz aus `icon-matrix/`, dokumentierte einmalige
Pages-Aktivierung durch den Maintainer, veröffentlichender Branch `main`
sowie Plattformdarstellung iOS „In Entwicklung" ohne Windows-Nennung.
