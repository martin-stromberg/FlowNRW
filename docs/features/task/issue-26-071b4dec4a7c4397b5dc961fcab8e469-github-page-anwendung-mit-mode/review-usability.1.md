# Usability-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### site/index.html (Abschnitt „Release & Plattform")

- **Irreführende Aussage** — Die Seite behauptet: „Das jeweils aktuelle stabile Release steht auf GitHub bereit". Tatsächlich existiert im Repository kein stabiles Release — alle Releases sind als Pre-release markierte Release-Kandidaten (`v1.0.0-rc.1` … `v1.0.0-rc.11`). `releases/latest` leitet in diesem Fall nur auf die allgemeine Release-Übersicht weiter; ein Laie klickt auf „Aktuelles Release" und sieht ausschließlich „Pre-release"-Badges — direkter Widerspruch zum Wort „stabil".

  Empfehlung: Text neutral formulieren, z. B. „Die aktuellen Releases und Vorabversionen stehen auf GitHub bereit", und den Button z. B. auf `https://github.com/martin-stromberg/FlowNRW/releases` zielen.

- **Irreführende Aussage** — Die Seite nennt als einzige Plattform „iOS — In Entwicklung" (gemäß Anwenderfestlegung: Windows nicht nennen, iOS Zielplattform mit Status „In Entwicklung"). Die verlinkten Release-Artefakte sind jedoch plattformspezifische Desktop-Pakete (`FlowNRW-vX.Y.Z-win-x64.zip`), und sämtliche Screenshots zeigen ein Desktop-Fenster mit klassischer Fensterleiste. Eine nicht-technische Besucherin kann sich nicht erklären, für welches Gerät das herunterladbare Paket gedacht ist und weshalb die gezeigte App bereits als Desktop-Fenster läuft, obwohl nur iOS „in Entwicklung" genannt wird. Die Festlegung selbst wird eingehalten — die entstehende Verständnislücke ist aber ein echtes Usability-Problem, das eine Produktentscheidung braucht.

  Empfehlung: Klären, ob der Bezug des Release-Pakets zur tatsächlich ausgelieferten Plattform ohne die Nennung von Windows kenntlich gemacht werden darf (z. B. neutral als „aktuelles Desktop-Paket" oder Hinweis „eine iOS-Version ist noch nicht verfügbar"); andernfalls zumindest die Formulierung so wählen, dass nicht der Eindruck entsteht, es gebe bereits ein installierbares iOS-Release.

### site/index.html (Feature-Karte „Abfahrtsmonitor")

- **Verständlichkeit** — Der Satz „Fehler erhalten die letzten bekannten Daten." ist grammatisch falsch und liest sich für Laien missverständlich („Fehler erhalten Daten"). Gemeint ist: Bei Aktualisierungsfehlern bleiben die letzten bekannten Daten sichtbar.

  Empfehlung: Korrigieren zu z. B. „Bei Fehlern bleiben die letzten bekannten Daten sichtbar."

### site/index.html (Abschnitt „Verweise")

- **Wirkungslose Aktion (bedingt)** — Drei der vier Verweise zeigen auf `main`-Pfade: `tree/main/docs`, `blob/main/docs/help/index.md`, `blob/main/LICENSE`. Der Branch `main` enthält aktuell nur `.gitignore` und `README.md` (alle Inhalte liegen auf `staging`); alle drei URLs liefern daher heute GitHub-404-Seiten — für Besucher Sackgassen. Da `pages.yml` nur bei Pushes auf `main` deployed und `site/` erst über die Promotion (feature → staging → main) auf `main` landet, sollten die Links zum Go-Live-Zeitpunkt auflösen — vorausgesetzt, die Promotion bringt `LICENSE` und `docs/` mit. Das ist plausibel, aber nicht verifiziert.

  Empfehlung: Nach dem ersten produktiven Deploy einmalig alle „Verweise" anklicken bzw. Link-Smoke-Check ergänzen; falls die Promotion die Pfade nicht mitliefert, Links auf die tatsächlich veröffentlichende Branch-Struktur ausrichten.

### site/index.html (Abschnitt „Screenshots")

- **Präsentationsqualität** — `haltestellenkarte-hell.png` und `haltestellenkarte-dunkel.png` zeigen eine nahezu leere Basiskarte: nur Hintergrundgitter, zwei nummerierte Marker und den Hinweis „Basiskarte geladen", aber keine geladenen Kartenkacheln (keine Straßen, Ortsnamen o. Ä.). Als Werbematerial wirkt die interaktive Karte damit defekt/unfertig und untergräbt die Feature-Aussage „Interaktive Karte". Zusätzlich zeigt `verbindungssuche-hell.png` für Start „Essen Hauptbahnhof" und Ziel „Düsseldorf Hauptbahnhof" identische Koordinaten „51.45, 7.01" — bei genauem Hinsehen erkennbar unfertig/Fixture-artig.

  Empfehlung: Karten-Screenshots mit vollständig geladenen Kacheln neu aufnehmen (oder stattdessen die Listenansicht zeigen); Suche-Screenshot mit konsistenten, plausiblen Werten neu aufnehmen.

### site/index.html + site/theme.js (Theme-Umschalter)

- **Wirkungslose Aktion (Randfall)** — Der Button „Darstellung" ist ausschließlich per `theme.js` verdrahtet. Ist JavaScript deaktiviert oder wird das Skript blockiert/nicht geladen, bleibt ein sichtbarer, klickbar wirkender Schalter ohne jede Wirkung zurück (die reine `prefers-color-scheme`-Darstellung greift zwar weiter, der Schalter tut dann aber nichts).

  Empfehlung: Schalter progressive-enhancement-mäßig erst per JS sichtbar machen (im Markup `hidden`, per `theme.js` einblenden) oder einen `<noscript>`-Hinweis ergänzen.

### site/index.html (Metadaten/Footer, kleinere Punkte)

- **Verständlichkeit** — `og:image` ist relativ angegeben (`./assets/flownrw.png`); Social-Media-Crawler benötigen eine absolute URL, sodass beim Teilen der Seite kein Vorschaubild erscheint. Zudem nennt der Footer „Icon-Font Material Symbols Rounded (Apache License 2.0)", obwohl die Seite diese Schrift nirgends einbindet (das Theme-Icon ist reines CSS) — der Lizenznachweis behauptet eine Nutzung, die nicht stattfindet.

  Empfehlung: `og:image` absolut setzen (`https://martin-stromberg.github.io/FlowNRW/assets/flownrw.png`); den Font-Nachweis entweder entfernen oder nur führen, wenn die Icon-Schrift tatsächlich eingebunden wird.

## Geprüfte Interaktionen

Liste der aus der Anforderung geprüften Benutzerinteraktionen:

- Seitenaufruf und Erfassen des Anwendungszwecks (Hero, Titel, Meta-Beschreibung) → unauffällig
- Navigation über Kopfzeilen-Anker (Features, Screenshots, Release, Verweise, Marken-Link „#top") → unauffällig (alle Ziele vorhanden)
- Hell-/Dunkel-Darstellung via Systemeinstellung (`prefers-color-scheme`) → unauffällig
- Hell-/Dunkel-Umschalter (Klick, Persistenz via localStorage, `aria-pressed`, Labelwechsel) → grundsätzlich wirksam verdrahtet; Befund im No-JS-Randfall
- Screenshots ansehen (8 Assets, alle vorhanden, 430×900 passend zu width/height, Lazy-Loading, Alt-Texte) → Befund (leere Karte, doppelte Koordinaten)
- Download-/Release-Verweis anklicken → Befund („stabiles Release" existiert nicht; Plattformbezug des Pakets unklar; Link führt technisch nicht ins Leere, sondern auf die Release-Übersicht mit nur Vorabversionen)
- Verweise auf Repository, Dokumentation, Feature-Doku und Lizenz → bedingter Befund (drei Links heute 404, auflösend erst nach staging→main-Promotion)
- Lesbarkeit/Verständlichkeit der Inhalte inkl. Anwenderfestlegung (Windows nicht genannt; iOS als Zielplattform „In Entwicklung") → Festlegung eingehalten; Befund zum Sprachfehler und zur resultierenden Plattform-Verständnislücke
- Responsivität (Breakpoints 900/640 px, umbrechende Navigation, Grid-Umschaltungen) → unauffällig, soweit statisch prüfbar

## Geprüfte Dateien

Liste aller geprüften UI-Dateien:

- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/screenshots/abfahrtsmonitor-hell.png`
- `site/assets/screenshots/abfahrtsmonitor-dunkel.png`
- `site/assets/screenshots/verbindungssuche-hell.png`
- `site/assets/screenshots/verbindungsdetails-hell.png`
- `site/assets/screenshots/verbindungsdetails-dunkel.png`
- `site/assets/screenshots/haltestellenkarte-hell.png`
- `site/assets/screenshots/haltestellenkarte-dunkel.png`
- `site/assets/screenshots/haltestellensuche-hell.png`
- `site/assets/flownrw.png`
- `site/assets/fonts/` (Inter Regular/SemiBold/Bold, JetBrains Mono Bold — Einbindung geprüft)

Kontextprüfung außerhalb von `site/` (nur zur Bewertung der verlinkten Ziele, keine Implementierungsdateien geändert): `.github/workflows/pages.yml`, `README.md`, `LICENSE`, `docs/help/index.md`, Remote-Branchzustand `main`/`staging`, Release-Liste (`gh release list`).
