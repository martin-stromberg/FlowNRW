# Plan-Gegenprüfung

Geprüft: `plan.md` gegen `requirement.md` (Issue #26), `inventory.md` und die Detaildokumente unter `inventory/`. Stichproben gegen das Repository bestätigt: Fonts unter `FlowNRW/Resources/Fonts/` vorhanden (Inter-Regular/-SemiBold/-Bold, JetBrainsMono-Bold), App-Icon `FlowNRW/Resources/AppIcon/flownrw.png` und `docs/design/reference/flownrw_icon.png` vorhanden, `icon-matrix/` mit ~226 PNGs belegt, `docs/CI-CD.md` enthält die Abschnitte „Workflows", „Einmalige Repo-Einrichtung (GitHub)" und „Fehlersuche", `.github/workflows/` enthält keinen Pages-Workflow (neue Datei `pages.yml` kollidiert nicht).

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| Page öffentlich erreichbar unter `https://martin-stromberg.github.io/FlowNRW/` | Actions-Deployment via neuer `pages.yml` (`configure-pages` → `upload-pages-artifact` → `deploy-pages`), Trigger `push` auf `main` + `workflow_dispatch`; einmalige Pages-Aktivierung als dokumentierter Maintainer-Schritt; Fehlerfall „Pages nicht aktiviert" mit Wiederaufnahmeweg (Programmabläufe, Umsetzungsreihenfolge Schritte 3 und 7) | Deploy-Smoke-Check im Workflow (`curl` auf `page_url`: HTTP 200 + Marker-String) plus reale Go-Live-Abnahme post-merge; Akzeptanzszenarien 1 und 2 | Abgedeckt |
| Design responsiv, modern, präsentiert die Anwendung ansprechend | `site/styles.css` mit responsiven Karten/Breakpoints, CSS-Custom-Properties aus `Colors.xaml`, Typografie-Hierarchie; einseitige Landingpage mit Hero, Feature-Karten, Galerie (Designentscheidungen, Schritt 1) | Lokale Render-/Link-Prüfung schmal (~430 px)/breit (~1280 px) in Schritt 6; Besucher-Szenario 4 | Abgedeckt |
| Helle und dunkle Darstellung | `prefers-color-scheme`-Default plus manueller Umschalter mit `localStorage`-Persistenz (`site/theme.js`, `data-theme="dark"`) | Lokale Prüfung beider Themes inkl. Umschalter-Persistenz (Schritt 6); Akzeptanzszenario 4 | Abgedeckt |
| Anlehnung an das Erscheinungsbild der Anwendung | Verbindliche Tokens aus `Colors.xaml` gespiegelt; selbst gehostete Font-Kopien Inter/JetBrains Mono aus `FlowNRW/Resources/Fonts/`; Kontrastregeln ≥ 4,5:1 als Validierungsregel | Lokale Verifikation beider Themes; Kontrast-Validierungsregel | Abgedeckt |
| Zweck, zentrale Features, Download-/Release-Verweis auf der Seite | Hero mit Zwecktext, sechs Feature-Karten aus README/`docs/help/`-Stand, plattformneutraler Release-Verweis `releases/latest` (Schritt 1, `index.html`-Beschreibung) | Lokale Link-Prüfung; Akzeptanzszenario 1 (sichtbare Wirkung nennt Inhalte explizit) | Abgedeckt |
| Screenshots der Anwendung | Kuratierter Satz (~6–8, hell+dunkel) aus `docs/help/design/verification/icon-matrix/` nach `site/assets/screenshots/`; Ausschlussregeln für Fixture-/Fehlerzustände (Schritt 2, Validierungsregeln) | Validierungsregel Screenshot-Auswahl; lokale Bildpfad-Prüfung (Schritt 6) | Abgedeckt |
| Verweise auf Repository und Dokumentation | Verweis-Sektion und Footer in `index.html` (Repo, `docs/`/`docs/help/index.md`, Lizenz) | Lokale Link-Prüfung (Schritt 6); Akzeptanzszenario 1 | Abgedeckt |
| Inhalte/Screenshots aus dem Repository gepflegt; Aktualisierungsweg dokumentiert | Versionierte Seitenquellen unter `site/`; Update-Weg PR → `staging` → Promotion → `main` → Auto-Deploy; `docs/CI-CD.md` erhält Workflow-Tabellenzeile, Einrichtungspunkt, Abschnitt „Projektseite (GitHub Pages)", Fehlersuche-Eintrag; README-Strukturzeile + Page-Verweis | Akzeptanzszenario 3 (Inhaltsaktualisierung); Programmablauf „Inhalts- und Screenshot-Aktualisierung" | Abgedeckt |
| Anwenderfestlegung: Windows nicht auf der Seite nennen; iOS Zielplattform mit Status „In Entwicklung" | Durchgängig festgelegt: Scope-Klärung „Erwartete Wirkung", Scope-Ausschluss 7, Designentscheidung „Plattformdarstellung", `index.html`-Beschreibung, Schritt 1, Validierungsregel „Inhaltsaussagen" (inkl. plattformneutralem Release-Framing), Akzeptanzszenario 1 | Validierungsregel + sichtbare Wirkung in Szenario 1; lokale Inhaltskontrolle in Schritt 6 | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

Entfällt — Status `Plan vollständig`.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Plattformfluss: Merge nach `main` → Deploy → Seite unter öffentlicher URL erreichbar | Deploy-Smoke-Check als Step in `pages.yml` (HTTP 200 + Marker auf `page_url`) plus reale Abnahme post-merge (Akzeptanzszenario 1) | Abgedeckt |
| Fehlerfall: Deploy ohne Pages-Aktivierung → verständlicher Fehler + Wiederaufnahme | Akzeptanzszenario 2 (`configure-pages`-Fehler, roter Lauf, `workflow_dispatch` nach Einrichtung) — vor dem Merge nicht automatisierbar, als reale Abnahme geplant | Abgedeckt |
| Besucherfluss: Seite laden, responsiv, hell/dunkel, Umschalter-Persistenz | Reale Abnahme post-merge (Szenario 4) + lokale Verifikation (Schritt 6); Plan begründet nachvollziehbar, warum kein repo-seitiger automatisierter E2E-Test eingeführt wird (kein Benutzerfluss in der App, Plattformablauf vor Merge nicht ausführbar, neue Test-Toolchain wäre nötig; die Anforderung lässt automatisierte Seiten-Checks ausdrücklich der Planung überlassen) | Nicht erforderlich mit Begründung |
| Inhaltsaktualisierung über PR-/Promotion-Kette | Akzeptanzszenario 3 als realer Ablauf; der Mechanismus (paths-gefilterter Push-Trigger, idempotenter Redeploy) ist im Workflow verankert | Abgedeckt |

## Fehlende oder unvollständige Planbestandteile

Entfällt — Status `Plan vollständig`.

Einzelprüfung der Pflichtbausteine: `Scope-Klärung` vollständig gefüllt (Verarbeitungseinheit, auslösende Ereignisse, erwartete Wirkung, Umgang mit fehlenden Objekten). Alle sieben Scope-Ausschlüsse tragen Begründung und Status `bestätigt (2026-11-16)` — kein Ausschluss mit Status `offen`. `Akzeptanzszenarien` enthält einen vollständigen Happy Path (Szenario 1: Auslöser Promotion-Merge → Deploy-Kette → sichtbare öffentliche Seite) samt benanntem Ausgangszustand, ein Szenario für fehlende Voraussetzung (Szenario 2), Aktualisierung (3) und Besucher-Darstellung (4). Wartesituationen (blockierter Deploy, auto-angelegtes `github-pages`-Environment) haben Zustände und Wiederaufnahmewege. Alle acht Offenen Fragen der Anforderung sind durch die Anwenderantwort vom 2026-11-16 aufgelöst und in Designentscheidungen/Ausschlüsse eingearbeitet (Deployment-Variante site/+Actions, Branch `main`, Sprache Deutsch, Screenshot-Kuratierung, Hell/Dunkel-Mechanismus, `releases/latest` plattformneutral, dokumentierte Pages-Aktivierung, Plattformdarstellung). Betroffene bestehende Tests: korrekt als „keine" benannt (keine C#-/Skriptänderung); notwendige Assets/„Fixtures" (Fonts, Icon, kuratierte PNGs mit Ausschlussregeln) sind benannt. Umsetzungsreihenfolge ist ausführbar mit Voraussetzungen je Schritt; Maintainer-Schritt ist als post-merge-Voraussetzung für Go-Live explizit.

## Hinweise

Optionale Hinweise zur Nachplanung (keine Lücken):

- **Smoke-Check-Robustheit:** `curl --retry` wiederholt standardmäßig keine HTTP-404-Antworten; bei der allerersten Pages-Auslieferung kann die CDN-Propagation einige Minuten dauern. In der Umsetzung empfiehlt sich `--retry-all-errors` bzw. ein großzügigeres Retry-Fenster oder ein toleranterer Check — sonst kann der Smoke-Step den ersten Lauf trotz erfolgreichem Deploy rot markieren. Der dokumentierte Wiederaufnahmeweg (`workflow_dispatch`) bleibt Fallback.
- **Screenshot-Kuratierung:** `icon-matrix/` enthält auch Fixture-/Leerzustände (Dateinamen z. B. `*-favorite-cache-seed-empty-favorites.png`, `*-empty-*`). Die Validierungsregel deckt `*-empty-*` bereits ab; bei der Kuratierung zusätzlich auf Motive mit echten Inhalten achten (Favoriten/Verbindungen mit Beispieldaten statt Leerzustände).
- **README-Verweis vor Go-Live:** Der README-Link auf die Page zeigt bis zum ersten erfolgreichen Deploy auf `main` ins Leere — zeitlich unkritisch, da der Merge ohnehin den Deploy auslöst; ggf. bewusst als Teil des Go-Live einordnen.
- **Optionale Vertiefung:** Ein pre-merge automatisierter Check der statischen Seite (HTML-/Link-Prüfung in `pr-staging-ci.yml`) wäre ohne neue Toolchain denkbar, ist aber nicht gefordert — die Anforderung lässt automatisierte Seiten-Checks der Planung frei, und der Plan begründet den Verzicht nachvollziehbar.
