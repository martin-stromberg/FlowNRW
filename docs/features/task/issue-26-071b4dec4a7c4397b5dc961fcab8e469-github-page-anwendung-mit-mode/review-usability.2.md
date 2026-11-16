# Usability-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### index.html (Release-&-Plattform-Sektion)

- **Erreichbarkeit** — Die Release-Kommunikation erzeugt für eine Laiin einen unaufgelösten Widerspruch: Der Text nennt „aktuelle Releases und Vorabversionen" und verlinkt auf GitHub-Releases, doch die einzige in der Plattformliste genannte Plattform ist „iOS" mit dem Chip „In Entwicklung" – ergänzt durch den Satz „iOS ist die Zielplattform von FlowNRW; die native Umsetzung befindet sich derzeit in Entwicklung". Eine Besucherin kann daraus nicht ableiten, auf welchem Gerät bzw. für welche Plattform die heute verfügbaren Downloads überhaupt laufen; „iOS ist die Zielplattform" liest sich sogar so, als gäbe es noch gar keine nutzbare Plattform – im direkten Widerspruch zum beworbenen Download. Zusätzlich weckt „aktuelle Releases" die Erwartung einer stabilen Version, obwohl auf GitHub ausschließlich Vorabversionen (Pre-releases) liegen.

  Empfehlung: Wortlaut so formulieren, dass die Vorabversionen als solche erkennbar sind und iOS als zusätzliche kommende Plattform erscheint, ohne Windows zu nennen – z. B.: „Die aktuellen Vorabversionen stehen auf GitHub bereit. Eine iOS-Version befindet sich derzeit in Entwicklung." Den Satz „iOS ist die Zielplattform von FlowNRW" ersetzen oder relativieren (z. B. „…als weitere Zielplattform"), damit nicht der Eindruck entsteht, die einzige Plattform sei unfertig.

### index.html (Screenshot-Auswahl)

- **Verständlichkeit** — Mehrere der eingebundenen Screenshots zeigen Warn-, Fehler- bzw. Fallback-Zustände, die eine nicht-technische Besucherin als „die App ist kaputt/unfertig" lesen kann: `verbindungsdetails-hell.png` (vermutlich auch die dunkle Variante) zeigt den Banner „Daten möglicherweise unvollständig" und „Fährt fällt aus"; `haltestellensuche-hell.png` zeigt ebenfalls „Daten möglicherweise unvollständig"; `haltestellenliste-dunkel.png` zeigt den Fallback-Hinweis „Basiskarte geladen." – also gerade nicht die beworbene interaktive Karte, sondern deren Ersatzdarstellung. Die Anforderung selbst warnt explizit, dass die vorhandenen Verifikations-Screenshots teils Fixture-/Fehlerzustände enthalten, die nicht präsentationsgeeignet sind.

  Empfehlung: Kuratieren bzw. neu aufnehmen — Screenshots bevorzugen, die Normalzustände ohne Warnbanner zeigen. Der Statuschip „Fährt fällt aus" im Abfahrtsmonitor kann als Beleg des Echtzeit-Features durchgehen; die Banner „Daten möglicherweise unvollständig" und der Zustand „Basiskarte geladen" (statt echter Karte) sollten jedoch durch Aufnahmen ohne Degradierung ersetzt werden, insbesondere da die Feature-Karte „Haltestellenkarte" eine interaktive Karte verspricht.

## Geprüfte Interaktionen

Liste der aus der Anforderung geprüften Benutzerinteraktionen:
- Seite aufrufen und Zweck der Anwendung verstehen (Hero mit prägnanter Vorstellung) → unauffällig; „native App" ist leichtes Fachwort, im Kontext aber verständlich.
- Zentrale Features ansehen (sechs Karten: Verbindungssuche, Abfahrtsmonitor, Favoriten, Haltestellenkarte, Automatische Aktualisierung, NRW-Echtzeitpriorität) → unauffällig; einzelne Begriffe wie „Statuschips" und „Fallbacks" sind Jargon-restig, aber die Kernaussagen sind laienverständlich.
- Screenshots der Anwendung ansehen (Galerie, hell/dunkel, mit Bildunterschriften und Alt-Texten) → Befund vorhanden (Fehler-/Fallback-Zustände in mehreren Bildern).
- Download-/Release-Verweis folgen (Buttons „Releases auf GitHub" im Hero und in der Release-Sektion) → Befund vorhanden (Link wirkt, aber die Plattform-/Versionseinordnung bleibt widersprüchlich; siehe oben). Anwenderfestlegung formal erfüllt: „Windows" wird nirgends genannt, iOS ist als „In Entwicklung" ausgewiesen.
- Verweise auf Repository und Dokumentation folgen (Linkliste „Verweise": Repository, `docs/`, `docs/help/index.md`, Lizenz) → unauffällig; alle Ziele existieren (`docs/help/index.md`, `LICENSE` geprüft), Lizenzname wird für Laien ausgeschrieben.
- Über die Navigation zwischen Abschnitten wechseln (Ankerlinks Features/Screenshots/Release/Verweise sowie Brand-Link nach `#top`) → unauffällig; alle Ankerziele vorhanden, auf Mobilgeräten bricht die Navigation sauber in eine zweite Zeile um.
- Zwischen heller und dunkler Darstellung wechseln → unauffällig; Umschalter ist per Progressive Enhancement nur mit aktivem JS sichtbar (`hidden` + `theme.js`), ohne JS folgt die Seite der Systemeinstellung (`prefers-color-scheme`) — beide Darstellungen bleiben erreichbar, gespeicherte Wahl via `localStorage`, `aria-pressed` gepflegt.
- Seite ohne JavaScript nutzen → unauffällig; sämtliche Inhalte, Links und Anker funktionieren rein statisch, kein JS-Pflichtelement.
- Seite auf verschiedenen Gerätegrößen nutzen (Responsivität) → unauffällig; Breakpoints bei 900/640 px, Grids schrumpfen auf 2 bzw. 1 Spalte, Bilder skalieren (`max-width: 100%`).

## Geprüfte Dateien

Liste aller geprüften UI-Dateien:
- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/screenshots/abfahrtsmonitor-hell.png`
- `site/assets/screenshots/abfahrtsmonitor-dunkel.png`
- `site/assets/screenshots/verbindungssuche-hell.png`
- `site/assets/screenshots/verbindungssuche-dunkel.png`
- `site/assets/screenshots/verbindungsdetails-hell.png`
- `site/assets/screenshots/verbindungsdetails-dunkel.png`
- `site/assets/screenshots/haltestellenliste-hell.png`
- `site/assets/screenshots/haltestellenliste-dunkel.png`
- `site/assets/screenshots/haltestellensuche-hell.png`
- `site/assets/flownrw.png`
