# Usability-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### index.html (Abschnitt „Release & Plattform")

- **Erreichbarkeit** — Die Anforderung verlangt einen verständlichen Download-/Release-Verweis. Die Plattform-Liste nennt ausschließlich „iOS – In Entwicklung"; auf welchem Gerät die aktuell angebotenen Vorabversionen laufen, sagt die Seite nirgends (die Festlegung „Windows nicht nennen" ist dabei korrekt eingehalten). Eine nicht-technische Besucherin, die „Releases ansehen" klickt, landet auf `FlowNRW-vX.Y.Z-win-x64.zip`-Artefakten, kann aber nicht einordnen, ob die App auf ihrem Gerät läuft — ein iPhone-Nutzer könnte eine Datei laden, die er nicht verwenden kann. Die Fenster-Screenshots deuten den Desktop-Betrieb nur implizit an.

  Empfehlung: Im Release-Panel einen klaren Plattform-Eintrag für die vorhandene Version ergänzen, ohne Windows zu nennen — z. B. einen zweiten Listeneintrag „Desktop" mit Chip „Vorabversion" oder einen Satz wie „Die Vorabversionen laufen derzeit als Desktop-Anwendung." So bleibt die Anwenderfestlegung gewahrt und die Download-Erwartung ist eindeutig.

### index.html (Feature-Karten)

- **Erreichbarkeit** — Einzelne Formulierungen setzen Fachvokabular voraus, das eine Laiin nicht sicher deuten kann: „Fallbacks und Datenalter bleiben sichtbar" (Karte „NRW-Echtzeitpriorität") und „gelieferten Verbindungsverläufen" (Karte „Haltestellenkarte"). „Fallback" ist ein unübersetzter technischer Begriff; „geliefert" passt nicht zu etwas, das die Karte anzeigt. Der verlangte Zweck („zentrale Features prägnant vorstellen") wird durch solche Stellen für Laien schwerer erfassbar.

  Empfehlung: Umgangssprachlich umformulieren, z. B. „Wenn Echtzeitdaten fehlen, bleiben die zuletzt bekannten Fahrten sichtbar — inklusive Hinweis auf das Datenalter" bzw. „angezeigten Verbindungsverläufen". Begriffe wie „Statuschips" oder „Koordinate" sind im Kontext („für Verspätung, Ausfall und Gleiswechsel") noch verständlich und können bleiben.

## Geprüfte Interaktionen

Liste der aus der Anforderung geprüften Benutzerinteraktionen:

- Seite aufrufen und Zweck der Anwendung erfassen (Hero + Feature-Karten) → Befund vorhanden (Fachvokabular in zwei Karten)
- Zwischen heller und dunkler Darstellung wechseln → unauffällig (manueller Umschalter mit `aria-pressed`/Persistenz via `theme.js`, zusätzlich `prefers-color-scheme`; ohne JS bleibt der Schalter versteckt statt wirkungslos — saubere Progressive Enhancement)
- Screenshots der Anwendung ansehen → unauffällig (vier saubere Motive, je hell/dunkel, echte App-Ansichten ohne Fixture-/Fehlerzustände, Alt-Texte und Captions vorhanden)
- Zum Download-/Release-Angebot navigieren → Befund vorhanden (Ziel existiert und enthält reale Releases; aktuelle Zielplattform der Vorabversionen wird nicht benannt — siehe Befund Release-Panel)
- Verweise auf Repository und Dokumentation nutzen → unauffällig (alle vier Verweisziele existieren: Repo, `docs/`, `docs/help/index.md`, `LICENSE`)
- Über die Kopfnavigation zu Abschnitten springen → unauffällig (alle Anker `#features`, `#screenshots`, `#release`, `#links`, `#top` vorhanden; mobil umgebrochen statt abgeschnitten)
- Plattform-/Vorabversions-Kommunikation verstehen → teilweise Befund (Festlegungen korrekt umgesetzt: „Windows" wird nicht genannt, iOS als Zielplattform mit „In Entwicklung" gekennzeichnet, „Vorabversionen" ehrlich — nur Pre-Releases vorhanden; Lücke: Plattform der bestehenden Vorabversionen unklar)
- Seite ohne JavaScript nutzen → unauffällig (vollständig les- und navigierbar; Systemfarbschema greift weiterhin)

## Geprüfte Dateien

Liste aller geprüften UI-Dateien:

- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/screenshots/verbindungssuche-hell.png`
- `site/assets/screenshots/verbindungssuche-dunkel.png`
- `site/assets/screenshots/aktualisierung-hell.png`
- `site/assets/screenshots/aktualisierung-dunkel.png`
- `site/assets/flownrw.png`
