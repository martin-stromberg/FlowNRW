← [Zur Übersicht](index.md)

# Karte — Fehlerhilfe

## „Basiskarte wird geladen …" bleibt stehen

Das war der unter iOS gemeldete Befund. Der Ladepfad ist jetzt doppelt abgesichert; der Status darf nicht dauerhaft in diesem Zustand verharren:

- **Kachel-Fortschritt (15 s, `map.js`):** Meldet eine einzelne Kachel 15 Sekunden lang keinen Fortschritt (keine Antwort, kein Entladen, keine neue Anfrageaktivität), wird sie mit `cancel` am nativen Gateway abgebrochen und als Kachelfehler beendet. Jeder tatsächliche Fortschritt verlängert die Frist — langsame, aber laufende Ladevorgänge bleiben unberührt.
- **Gesamtlauf (15 s Stillstand, `map.js`):** Ohne jeglichen Kachelfortschritt oder ausbleibendes `load`-Ereignis meldet die Webseite einmalig `failed`.
- **Nativer Watchdog (30 s, `MapPage`):** Trifft nach `init` weder `loaded` noch `failed` ein, wechselt der Status zu „Basiskarte konnte nicht geladen werden. Haltestellenliste und Zurück bleiben nutzbar." und offene Kachelanfragen werden abgebrochen.
- **Verworfene Kachelanfragen:** Überschreitet die ausstehende Zahl 64 oder kommt eine doppelte Kennung, wird die Anfrage ausdrücklich als Kachelfehler beantwortet statt still verworfen — die Webseite wartet nicht ewig auf eine nie kommende Antwort.
- **Verspätete Meldungen:** `loaded`/`failed` ohne die aktuelle Lade-Generation (`loadId`) werden ignoriert; ein erneutes Laden oder eine neue Sitzung kann den Status nicht nachträglich verfälschen.

Haltestellenliste und Zurück bleiben in jedem Fall bedienbar; **Karte erneut laden / auf Auswahl zentrieren** startet eine neue Lade-Generation.

## „Basiskarte fehlt oder ist veraltet. Haltestellen und gelieferte Verläufe bleiben nutzbar."

Mindestens eine Kachel wurde als Fehler abgeschlossen — Gateway-Fehler, verworfene Anfrage oder Kachel-Timeout. Die Basiskarte kann Lücken zeigen; Marker, Liste und gelieferte Verläufe sind davon unberührt. Netzverbindung prüfen und **Karte erneut laden** wählen; bei anhaltenden Fehlern den Kartenanbieter (`Map:TileUrl`) und die Erreichbarkeit prüfen.

## „Basiskarte konnte nicht geladen werden. Haltestellenliste und Zurück bleiben nutzbar."

Der native 30-Sekunden-Watchdog hat ohne `loaded`-/`failed`-Meldung zugeschlagen — typischerweise eine gestörte WebView-Bridge, ein nie startender Kachellauf oder ein komplett blockierter Netzweg. **Karte erneut laden** startet frisch; wiederholt sich der Fall, liegt ein strukturelles Problem vor → Diagnose unten.

## Diagnose über das AppLog-Protokoll

`MapPage` schreibt den Ladeweg in das optionale Diagnoseprotokoll `AppLog`: `init` mit Generation und Marker-/Segmentzahl, `ready`, `loaded`/`failed` mit ausstehender Kachelzahl, ignorierte verspätete Meldungen (`ignored stale`), Kachelfehler (`tile error`), verworfene Anfragen (`tile dropped`) und den Watchdog-Eintrag (`no loaded/failed within 30 s`).

1. In der App über **Aktualisierung einstellen** die Seite **Automatische Aktualisierung** öffnen und dort im Abschnitt **Diagnose** den Schalter **Protokollierung** einschalten.
2. Den Kartenfehler reproduzieren.
3. Über **Protokoll senden** die Protokolldatei per E-Mail teilen.

Einträge der Kategorie `map` zeigen, ob `ready` ankam (Bridge funktioniert), ob `init` gesendet wurde und wo der Lauf endete (Kachelfehler, Verwurf, Watchdog). Die Protokollierung ist standardmäßig aus und bleibt lokal; sie enthält Sitzungskürzel, Zähler und Fehlertypen, keine Koordinaten der Markerinhalte als Freitext.

## iOS-Verifikation ausstehend

Der ursprüngliche Hänger ist unter Windows nicht reproduzierbar (Windows-Nachweise zeigen „Basiskarte geladen."); die genaue iOS-Ursache ist im Code nicht eingrenzbar. Watchdog, Kachel-Timeouts und `AppLog`-Diagnose sind die eingebaute Absicherung und das Werkzeug zur Felddiagnose — eine Geräte-/Simulatorprüfung (z. B. Safari-Web-Inspector an der HybridWebView/WKWebView plus ausgewertetem `AppLog`) steht noch aus und ist für die endgültige Abnahme erforderlich.
