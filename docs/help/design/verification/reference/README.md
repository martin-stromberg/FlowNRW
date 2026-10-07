# Lokal gerenderte Original-Designreferenzen

Erstellt am 01.10.2026 aus den unveränderten vier `docs/design/reference/*/code.html` mit Chrome 154.0.8037.58 unter Windows. Dies sind **HTML-Referenzrenderings, keine Screenshots der nativen FlowNRW-App** und keine visuelle Abnahme der Umsetzung.

Je Referenz liegen zwei PNGs vor: 430 × 900 und 1024 × 768 CSS-Pixel bei DPR 1 (entspricht PNG-Pixeln), helle Darstellung, Standardskalierung. Der Viewport wird ausdrücklich per Chromium Device Metrics gesetzt; die Mindestbreite eines Desktopfensters beeinflusst ihn nicht. `render-metadata.json` dokumentiert die tatsächlich ausgelesenen Werte, Browser-Version, Datum, Schriftstatus und Bildladezustände.

| Original | Schmal | Breit |
|---|---|---|
| Abfahrten | [430 × 900](abfahrtsmonitor_live-430x900.png) | [1024 × 768](abfahrtsmonitor_live-1024x768.png) |
| Verbindungssuche | [430 × 900](verbindungssuche-430x900.png) | [1024 × 768](verbindungssuche-1024x768.png) |
| Fahrtdetails | [430 × 900](fahrtbegleiter_detail-430x900.png) | [1024 × 768](fahrtbegleiter_detail-1024x768.png) |
| Haltestellenkarte | [430 × 900](umgebungskarte_stationen-430x900.png) | [1024 × 768](umgebungskarte_stationen-1024x768.png) |

## Reproduktion

Vom Repository-Stamm aus `node docs/help/design/verification/reference/render.mjs` ausführen. Benötigt aktuelles Node.js mit eingebautem WebSocket und Chrome am im Skript angegebenen Standardpfad. Chrome läuft unsichtbar im Headless-Modus mit isoliertem Profil; keine native App wird gestartet. Das Skript wartet pro Seite fünf Sekunden und anschließend auf `document.fonts.ready`, nimmt den initialen Viewport auf und beendet seinen Browser. Generierte Profilverzeichnisse sind temporär und gehören nicht in Git.

Der verfügbare In-App-Browser wurde entsprechend Browser-Skill zuerst verbunden, meldete jedoch `Browser is not available: iab`. Deshalb erfolgte die lokale Referenzaufnahme mit installiertem Chrome.

## Grenzen und externe Assets

Die Originale laden Tailwind von `cdn.tailwindcss.com`, Schriftarten/Symbole von Google Fonts und Bilder von `lh3.googleusercontent.com`. Diese Abhängigkeiten wurden für die Aufnahme unverändert zugelassen; sie sind keine Produktabhängigkeiten. Bei dieser Aufnahme meldeten alle HTML-Bilder erfolgreiche Ladung, alle Schriftensätze waren geladen, der berechnete Hintergrund war korrekt `rgb(250, 249, 254)` und die verwendete Textschrift `Inter`. Spätere Offline-Aufnahmen können abweichen. Keine Ersatzbilder und keine 28-Byte-Platzhalter wurden verwendet.

Die Originale enthalten animierte bzw. zeitgesteuerte Beispielzustände; Countdownwerte sind daher nicht pixelgenau reproduzierbar. Screenshots zeigen den oberen Viewport, keine vollständige Scrollseite. Manche ursprünglichen HTML-Inhalte laufen bei schmaler Breite um oder werden gekürzt; die Referenzen wurden dafür nicht verändert.

Für den Produktvergleich gelten die Konfliktentscheidungen in `docs/design/acceptance.md`: insbesondere drei statt vier Kernbereiche, keine erfundene Auslastung, kein aktiver Fahrtbegleiter und keine dekorativen Aktionen ohne Funktion. Die Originalbilder zeigen solche ausgeschlossenen Entwurfselemente weiterhin bewusst als unveränderte Quellen.
