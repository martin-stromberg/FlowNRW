# Anforderung – GitHub Page: Anwendung mit modernem Stil präsentieren

**Kundenanforderung (Original, GitHub Issue #26):** „Bitte erstelle für dieses Repository eine GitHub Page, die die Anwendung mit modernem Stil präsentiert. Kontext: FlowNRW ist eine .NET-MAUI-Anwendung für bundesweite ÖPNV-Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten (Windows, iOS-Plattformbasis). Anforderungen: Statische Seite, über GitHub Pages ausgeliefert — z. B. aus einem `docs/`-Ordner oder einem GitHub-Actions-Deployment passend zu den bestehenden CI/CD-Workflows; modernes, responsives Design mit klarer Typografie und konsistentem Stil; helle und dunkle Darstellung; an das Erscheinungsbild der Anwendung angelehnt; Inhalte: prägnante Vorstellung der Anwendung, zentrale Features, Screenshots, Download-/Release-Verweise, Verweise auf Repository und Dokumentation; Inhalte und Screenshots werden aus dem Repository heraus gepflegt, damit die Seite mit der Anwendung mitwachsen kann. Akzeptanzkriterien: Die Page ist unter `https://martin-stromberg.github.io/FlowNRW/` öffentlich erreichbar; das Design ist responsiv, modern und präsentiert die Anwendung ansprechend; Zweck, zentrale Features und Download-/Release-Verweis sind auf der Page vorhanden; die Aktualisierung der Seite (Workflow bzw. Pflege) ist im Repository dokumentiert."

**Korrektur/Annahmen bei der Übersetzung:** Der Issue-Kommentar „Bitte warte 2 Stunden und erledige dann diese Aufgabe" ist ein prozessuale Steueranweisung an den Workflow, kein fachlicher Bestandteil der Anforderung, und wird hier nicht übersetzt. „An das Erscheinungsbild der Anwendung angelehnt" wird als Anlehnung an das verbindliche Token-System der App (Farben aus `FlowNRW/Resources/Styles/Colors.xaml`, Typografie Inter/JetBrains Mono, Iconografie Material Symbols Rounded — dokumentiert in `docs/design/acceptance.md` und `docs/help/design/`) interpretiert.

## Fachliche Zusammenfassung

Für das Repository soll eine statische Projektseite entstehen, die über GitHub Pages unter `https://martin-stromberg.github.io/FlowNRW/` öffentlich ausgeliefert wird. Die Seite präsentiert FlowNRW (Zweck: bundesweite ÖPNV-Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten; Plattformen: Windows, iOS-Basis) mit einem modernen, responsiven Design in heller und dunkler Darstellung, angelehnt an das bestehende App-Designsystem. Seiteninhalte und Screenshots liegen versioniert im Repository; die Auslieferung erfolgt über einen zum bestehenden CI/CD-Muster (`.github/workflows/`, Dokumentation in `docs/CI-CD.md`) passenden Mechanismus, und die Pflege der Seite ist im Repository dokumentiert.

## Fachlicher Scope

- **Verarbeitungseinheit:** Die statische Projektseite des Repositories als Ganzes — also das gebündelte Ergebnis aus Seitenquellen (Markup/Styles/Inhalte), referenzierten Screenshot-Assets und dem Auslieferungslauf (Pages-Build/Deploy). Zum Scope gehören Inhalt, Gestaltung, Deployment-Anbindung und die Doku der Pflege; nicht dazu gehören Änderungen an der Anwendung selbst oder an deren Datenlogik.
- **Auslösende Ereignisse:** Fachlich relevant ist der Merge/Push geänderter Seiteninhalte auf den veröffentlichenden Branch (projektüblich: Feature-Branch → `staging` → `main`; welcher Branch veröffentlicht, ist zu klären, siehe Offene Fragen) sowie ggf. ein manueller Workflow-Trigger. Für den Besucher ist das auslösende Ereignis der Aufruf der öffentlichen URL.
- **Erwartete Wirkung:**
  - Öffentlich erreichbare Seite unter `https://martin-stromberg.github.io/FlowNRW/` (Projektseite, kein Custom Domain).
  - Die Seite enthält: prägnante Vorstellung der Anwendung (Zweck), die zentralen Features, Screenshots der Anwendung, Download-/Release-Verweise sowie Verweise auf Repository und Dokumentation.
  - Darstellung ist responsiv (mobil und Desktop) und in heller wie dunkler Variante verfügbar; der Stil ist konsistent, typografisch klar und an das App-Erscheinungsbild angelehnt.
  - Inhaltspflege erfolgt ausschließlich über Änderungen im Repository; der Aktualisierungsweg (Workflow bzw. Pflegeanleitung) ist im Repo dokumentiert.
- **Umgang mit fehlenden Objekten:**
  - **GitHub-Pages-Aktivierung:** Es existiert noch keine Pages-Konfiguration/kein Pages-Workflow im Repository (`.github/workflows/` enthält keinen Pages-Workflow). Pages muss einmalig aktiviert werden — je nach gewählter Variante in den Repo-Einstellungen oder implizit durch einen Actions-Deploy-Workflow (`actions/configure-pages`/`deploy-pages`). Ob ein manueller Einrichtungsschritt durch den Maintainer nötig ist, ist zu klären (siehe Offene Fragen).
  - **Screenshot-Assets:** Bereits vorhandene, repo-seitig gepflegte Screenshots existieren u. a. unter `docs/help/design/verification/final-matrix/`, `docs/help/design/verification/icon-matrix/` sowie `native-*.png` in den `docs/help/*/verification/`-Ordnern. Diese können wiederverwendet oder als Ausgangsbasis für einen kuratierten Satz dienen; neue Aufnahmen wären zu erzeugen, wenn die vorhandenen nicht präsentationsreif sind.
  - **Veröffentlichungsziel-URL:** Die Ziel-URL ist durch das Repository (`martin-stromberg/FlowNRW`) vorgegeben; es ist nichts zu erzeugen, aber der Base-Pfad `/FlowNRW/` muss in der Seite berücksichtigt werden (relative Pfade).
  - **`docs/`-Konflikt:** Der Ordner `docs/` ist bereits durch die Projektdokumentation belegt. Wird die klassische Variante „Publish aus `/docs` eines Branchs" gewählt, würde die gesamte Doku-Struktur mit veröffentlicht; dagegen spricht, dass dort `.md`-Dateien, Arbeitsverzeichnisse (`docs/features/task/`) und große Screenshot-Matrizen liegen. Dieser Punkt ist bei der Variantenwahl zu berücksichtigen.
- **Scope-Ausschlüsse:**
  - Keine dynamischen/serverseitigen Funktionen — die Anforderung verlangt eine statische Seite; keine Live-Fahrplandaten, keine Backend-Anbindung.
  - Keine Custom Domain, keine zusätzlichen Subdomains; Ziel ist ausschließlich die genannte GitHub-Pages-URL.
  - Kein Store-/MSIX-/Installer-Angebot auf der Seite; „Download-/Release-Verweise" meinen Verweise auf die GitHub-Releases (`release.yml` erzeugt `FlowNRW-vX.Y.Z-win-x64.zip`; ein stabiler Verweis wie `releases/latest` liegt nahe, ist aber nicht vorgeschrieben).
  - Keine mehrsprachige Seite gefordert — die Projektdokumentation ist deutsch; Sprache der Seite als Annahme deutsch, zur Sicherheit in den Offenen Fragen vermerkt.
  - Keine Analytics-/Tracking-, Cookie-, Newsletter- oder Kontaktfunktionen angefordert.
  - Keine Änderung der App, der Release-Pipeline-Ziele oder der bestehenden Quality Gates; die Seite ist ein additives Artefakt.
  - Die iOS-Plattform ist nur „Plattformbasis" ohne Geräteabnahme — die Seite soll den Funktionsstand nicht überzeichnen (Annahme: Darstellung orientiert sich am in README/`docs/help/` dokumentierten, geprüften Stand).

## Betroffene Klassen und Komponenten

Es handelt sich um ein Dokumentations-/Infrastruktur-Feature ohne Änderungen an C#-Klassen. Voraussichtlich betroffene bzw. neu zu erstellende Artefakte:

- **Seitenquellen (neu):** Statische Seitendateien (HTML/CSS, ggf. Assets) an einem noch festzulegenden Ort — z. B. ein eigener Ordner (wie `site/` oder `docs/page/`) oder die klassische `/docs`-Variante. Ort und Aufbau sind Teil der Planung, nicht dieser Übersetzung.
- **Screenshot-Assets (neu oder wiederverwendet):** Kuratierter Bildsatz für die Seite; Kandidaten liegen unter `docs/help/design/verification/final-matrix/`, `docs/help/design/verification/icon-matrix/` und `docs/help/*/verification/native-*.png`.
- **Workflow (neu oder Erweiterung):** Deployment-Anbindung passend zum bestehenden Muster in `.github/workflows/` — entweder ein dedizierter Pages-Deploy-Workflow (z. B. `pages.yml` mit `actions/upload-pages-artifact` + `actions/deploy-pages`) oder die Repo-Einstellung „Deploy from branch". Integration in die bestehende Branch-Strategie (`staging` → `main`) ist zu beachten.
- **`docs/CI-CD.md`:** Ergänzung um den Pages-Workflow in der Workflow-Tabelle und ggf. um den Einrichtungsschritt in „Einmalige Repo-Einrichtung" (Akzeptanzkriterium „Aktualisierung ist dokumentiert").
- **`docs/help/`:** Ggf. neuer Hilfebereich zur Pflege der Seite (das Projekt pflegt Feature-Dokumentation strukturiert unter `docs/help/<thema>/`, vgl. `docs/help/design/`).
- **`README.md`:** Ggf. Badge/Verweis auf die Page (nicht explizit gefordert; naheliegend).
- **App-Designquellen (nur lesend):** `FlowNRW/Resources/Styles/Colors.xaml` (Farb-Tokens: `Primary #0058BC`, `Canvas*`/`Card*`/`Ink*`/`Muted*`/`Outline*` je hell/dunkel), `docs/design/acceptance.md`, `docs/help/design/` als verbindliche Referenz für das Erscheinungsbild; Schriften Inter (SIL OFL) und JetBrains Mono (OFL) sowie Material Symbols Rounded (Apache 2.0) mit Lizenznachweisen unter `docs/design/licenses/`.
- **Tests:** Keine Unit-Test-Auswirkungen (`FlowNRW.Tests` testet `FlowNRW.Core`). Ob eine automatisierte Prüfung der Seite (z. B. Link-Check, Build-Check im PR-Workflow) sinnvoll ist, bleibt der Planung überlassen.

## Implementierungsansatz

- **Variante wählen (zulässig lt. Anforderung):** (a) klassisches „Deploy from branch" aus einem Ordner oder (b) GitHub-Actions-Deployment per `actions/deploy-pages` — letzteres passt zum bestehenden Workflow-Muster und erlaubt einen separaten Quellordner ohne `docs/`-Konflikt. Entscheidung offen (siehe Offene Fragen); die Anforderung nennt beide Wege als Beispiele.
- **Design anlehnen, nicht kopieren:** Farb- und Typo-Tokens der App (`Colors.xaml`, Inter/JetBrains Mono, Statussemantik hell/dunkel mit den dokumentierten Kontrastwerten) als CSS-Custom-Properties übernehmen; helle und dunkle Darstellung z. B. über `prefers-color-scheme` und/oder Umschalter — Mechanismus nicht vorgegeben.
- **Inhalte aus dem Repo:** Zweck/Features aus README und `docs/help/` prägnant zusammenfassen; Screenshots als versionierte Assets einbinden; Verweise auf GitHub-Releases, Repository und Dokumentation. Base-Pfad `/FlowNRW/` beachten.
- **Dokumentation der Pflege:** Den gewählten Aktualisierungsweg (welcher Branch, welcher Ordner, welcher Trigger) in `docs/CI-CD.md` und ggf. `docs/help/` festhalten — verbindliches Akzeptanzkriterium.
- **Abhängigkeiten:** Keine Änderung an `FlowNRW`, `FlowNRW.Core`, `FlowNRW.Tests`; Abhängigkeit zur Repo-Einrichtung (Pages-Aktivierung, ggf. `github-pages`-Environment, Branch-Protection-Erfordernisse) und zur Staging→main-Promotion, falls die Veröffentlichung erst ab `main` wirken soll.

## Konfiguration

Keine Laufzeit-/Anwendungskonfiguration betroffen. Relevante „Konfiguration" liegt auf Repository-Ebene: Pages-Quelle bzw. Deploy-Workflow, veröffentlichender Branch und ggf. das `github-pages`-Deployment-Environment. Die Wahl ist im Repository zu dokumentieren (Akzeptanzkriterium).

## Offene Fragen

1. **Deployment-Variante und Quellordner:** Actions-Deployment aus einem eigenen Ordner (empfohlene Annahme wegen des belegten `docs/`-Ordners) oder klassische `/docs`-Veröffentlichung? Falls Actions-Deployment: muss Pages vorab in den Repo-Einstellungen auf „GitHub Actions" umgestellt werden — ist dieser manuelle Schritt durch den Maintainer akzeptiert/einzuplanen?
2. **Veröffentlichender Branch:** Soll die Seite bereits aus `staging` oder erst nach Promotion aus `main` deployed werden? Hintergrund: Die bestehende Strategie hält `main` für stabile Releases; die Seite „wächst mit der Anwendung" — unklar, ob sie den Funktionsstand von `staging` oder `main` spiegeln soll.
3. **Sprache der Seite:** Deutsch (passend zur gesamten Projektdokumentation) — bestätigt?
4. **Screenshots:** Reicht ein kuratierter Satz aus den vorhandenen Verifikations-Screenshots (`final-matrix`, `icon-matrix`, `native-*`), oder sind neue, gezielt für die Präsentation aufgenommene Screenshots gewünscht? Die vorhandenen Matrizen enthalten teils Fixture-/Fehlerzustände, die nicht präsentationsgeeignet sein dürften.
5. **Hell/Dunkel-Umsetzung:** Soll die Seite der Systemeinstellung folgen (`prefers-color-scheme`), einen manuellen Umschalter bieten oder beides? Die Anforderung verlangt beide Darstellungen, lässt den Mechanismus offen.
6. **Download-Verweis-Ziel:** Verweis auf „latest stable Release" (`releases/latest`), zusätzlich auf RC-Prereleases von `staging`, oder nur allgemein auf die Releases-Seite?
7. **Einmalige Einrichtung:** Reicht es, den notwendigen einmaligen GitHub-Settings-Schritt (Pages aktivieren) als dokumentierte Voraussetzung zu liefern, oder soll die Umsetzung ihn — soweit möglich — selbst auslösen (z. B. per `gh api`)? Die öffentliche Erreichbarkeit als Akzeptanzkriterium hängt davon ab.
8. **Umfang der Darstellung:** Soll die Seite den geprüften Windows-Stand zeigen und iOS als „in Vorbereitung/Plattformbasis" kennzeichnen, oder iOS gar nicht erwähnen? (Vermeidung von Überzeichnung des Funktionsstands.)
