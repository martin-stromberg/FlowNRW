# Usability-Review

## Ergebnis

**Status:** Befunde vorhanden

Zwei geringfügige Befunde zur Beschriftung/Verständlichkeit; keine blockierenden Probleme. Die Seite erfüllt die Anforderung aus Endanwendersicht: Zweck, Features, Screenshots, Release-Verweis und Verweise auf Repository/Dokumentation sind ohne Vorwissen erreichbar, alle Aktionen sind verdrahtet, die Darstellung ist responsiv und in hell/dunkel verfügbar.

**Bewertung der Festlegungen:**
- **„Desktop-Anwendung" (Release-Abschnitt, index.html Zeile 121–123):** Die neutrale Formulierung erfüllt die Anwenderfestlegung — ein Abgleich bestätigt, dass „Windows" (ebenso „.NET"/„MAUI") auf der gesamten Seite nicht genannt wird. Für Laien ist „Desktop-Anwendung" verständlich („Programm für den Computer"). Restrisiko: macOS-/Linux-Besucher können die Plattform nicht aus dem Seitentext ableiten und könnten Kompatibilität annehmen; das eigentliche Release-Asset (`win-x64.zip`) auf der verlinkten GitHub-Seite klärt dies jedoch auf. Als Befund nicht gewertet, da bewusste Festlegung.
- **Vereinfachte Feature-Texte:** Überwiegend gelungene Laien-Sprache, die den dokumentierten Funktionsstand korrekt und ohne Überzeichnung abbildet. Zwei Formulierungen bleiben fachsprachlich (siehe Befund 2).
- **iOS-Darstellung:** Plattform-Liste enthält ausschließlich „iOS — In Entwicklung" plus erläuternden Satz; entspricht exakt der Festlegung (iOS = Zielplattform mit Status „In Entwicklung").

## Befunde

### index.html (Hero-CTA „Releases")

- **Erreichbarkeit** — Die Anforderung verlangt einen Download-/Release-Verweis, den ein nicht-technischer Besucher erkennt. Der primäre Button im Hero heißt nur „Releases" — für Laien kein geläufiger Begriff; die Zielseite ist zudem eine technische GitHub-Release-Liste mit ZIP-Assets. Im Release-Abschnitt ist dieselbe Aktion als „Releases ansehen" bereits verständlicher beschriftet.

  Empfehlung: Hero-Button konsistent zu „Releases ansehen" umbenennen oder als „Download (GitHub-Releases)" beschriften, damit das Handlungsziel ohne GitHub-Vorwissen erkennbar ist.

### index.html (Feature-Karten „Favoriten" und „Abfahrtsmonitor")

- **Erreichbarkeit** — Die Anforderung verlangt eine prägnante Vorstellung der zentralen Features in verständlicher Sprache. Zwei Formulierungen bleiben fachsprachlich bzw. sperrig: „expliziter Entfernungssortierung" (Favoriten-Karte, Zeile 73) — „explizit" ist hier Entwicklerjargon, der Satz ist schwer parsbar; „Echtzeit-Statuschips" (Abfahrtsmonitor-Karte, Zeile 69) — „Chips" ist ein Design-/UI-Fachbegriff, den Laien nicht kennen (die Beispiele „Verspätung, Ausfall, Gleiswechsel" federn das teilweise ab).

  Empfehlung: „…Haltestellen mit eigenen Abfahrtstafeln, die sich nach Entfernung sortieren lassen" bzw. „Echtzeit-Statushinweise für Verspätung, Ausfall und Gleiswechsel".

## Geprüfte Interaktionen

Liste der aus der Anforderung geprüften Benutzerinteraktionen:
- Zweck der Anwendung erfassen (Hero-Text) → unauffällig (klare Ein-Satz-Vorstellung)
- Zentrale Features überblicken (Feature-Karten) → Befund vorhanden (zwei fachsprachliche Formulierungen, siehe Befunde)
- Screenshots der Anwendung ansehen → unauffällig (reale App-Aufnahmen 430×900, hell/dunkel, sprechende Alt-Texte und Bildunterschriften; die sichtbare Fenster-Titelleiste der Screenshots korrespondiert konsistent mit „Desktop-Anwendung")
- Download-/Release-Verweis nutzen → Befund vorhanden (Hero-Button „Releases" für Laien wenig aussagekräftig; Linkziel funktionsfähig)
- Repository- und Dokumentations-Verweise nutzen → unauffällig (Nav-Anker „Verweise", vier klar beschriftete externe Links auf Repository, `docs/`, `docs/help/`, Lizenz)
- Zwischen heller und dunkler Darstellung wechseln → unauffällig (Systemerkennung via `prefers-color-scheme` plus manueller Umschalter mit `localStorage`; Umschalter ist `hidden` und wird erst per `theme.js` eingeblendet — Progressive Enhancement, kein toter Button ohne JS; `aria-pressed` und Label-Wechsel Hell/Dunkel korrekt)
- Seite mobil verwenden (responsiv) → unauffällig (Breakpoints 900/640 px, Karten 3→2→1 Spalten, Screenshots 4→3→2, Navigation bricht in zweite Zeile um — bei vier Links vertretbar)
- Plattformstand einordnen (Release & Plattform) → unauffällig (entspricht Festlegung: Desktop neutral benannt, iOS mit Chip „In Entwicklung"; Festlegungsbedingte Unschärfe, dass die einzig genannte Plattform noch nicht ladenbar ist, wird durch den erläuternden Satz abgefedert)
- Interne Kennungen / Suche / Auswahlaufgaben → nicht zutreffend (statische Seite ohne Eingaben; keine internen Werte erforderlich)

## Geprüfte Dateien

Liste aller geprüften UI-Dateien:
- `site/index.html`
- `site/styles.css`
- `site/theme.js`
- `site/assets/screenshots/verbindungssuche-hell.png`, `verbindungssuche-dunkel.png`, `aktualisierung-hell.png`, `aktualisierung-dunkel.png` (stichprobenartige Sichtprüfung)
