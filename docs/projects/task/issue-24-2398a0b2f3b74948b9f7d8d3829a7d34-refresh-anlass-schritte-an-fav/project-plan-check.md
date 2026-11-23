# Projektplan-Gegenprüfung

## Ergebnis

**Status:** Projektplan vollständig

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderungspunkt | Abgedeckt durch Schritt(e) | Status |
|--------------------|-----------------------------|--------|
| A/B1: Ein einziger Einstiegspunkt für „persistiertes Favoriten-Board wiederherstellen" (einzige Cache-Lesefassade, ggf. dedizierter Service) | 2 | Abgedeckt |
| A/B1: Ablauf Favoritenzugehörigkeit prüfen → Cache lesen → Freshness bewerten → Items filtern → anzeigen | 2 | Abgedeckt |
| A/B1: Einheitliche Stale-Semantik „markieren-und-anzeigen" (Home-Verhalten) | 2 (+ Vorgabe in Vorgehensentscheidung) | Abgedeckt |
| A/B1: Einheitliche Item-Filterung, zeitlose Items konsistent behandelt (verworfen) | 2 (+ Vorgabe in Vorgehensentscheidung) | Abgedeckt |
| A/B1: Gebündelter Dateizugriff statt wiederholtem Lesen pro Monitor-Öffnen (inkl. fehlender Abbruchmöglichkeit/Sperre im Befund) | 2 | Abgedeckt |
| A/B1: Monitor-Pfad nutzt denselben Einstieg; Eigenkompositionen inkl. duplizierter Ermittlung der wirksamen Abfahrtszeit entfallen | 2 | Abgedeckt |
| A/B1: Testanpassung der gepinnten alten Monitor-Semantik | 2 | Abgedeckt |
| B/B2: Zentraler Einstieg für Vordergrund-Rückkehr am Aktualisierungs-Lebenszyklus mit Gate „geladen und Intervall aktiv" | 3 | Abgedeckt |
| B/B2: Zentraler Schutz gegen überlappende Ausführung (Overlap-Guard) inkl. Abbruchmöglichkeit; kein divergierender Guard mehr | 3 | Abgedeckt |
| B/B2: Delegation an registrierte aktualisierbare Bereiche (Haltestellen-Monitor, Verbindungssuche/-details anbinden) | 3 | Abgedeckt |
| B/B2: Vier Seiten melden nur noch „sichtbar/verdeckt", komponieren keine Sequenz selbst; Verdecken bricht laufende Aktualisierungen ab | 3 | Abgedeckt |
| C/B3: Ungenutzter Voll-Ersetzungs-Schreibvorgang aus der Cache-Ablage-Schnittstelle entfernen/verintern | 1 | Abgedeckt |
| C/B3: Zugehörige Tests und Prüfdoubles anpassen; relevante Eigenschaften über produktiven Einfüge-/Aktualisierungspfad abdecken | 1 | Abgedeckt |
| D/B4: Anlass-Zuordnung (Anwendungsstart, neue Favoriten-Karten, Vordergrund-Rückkehr) zentral gebündelt; Trigger-Triplik entfällt, kein Aufrufer kennt Anlass-Regeln | 3 (+ Vorgehensentscheidung „Anlass-Zuordnung") | Abgedeckt |
| Betroffener Bereich: Favoriten-Start-Übersicht und Karten-ViewModel | 2, 3 | Abgedeckt |
| Betroffener Bereich: Cache-Ablage-Schnittstelle (+ Implementierung) | 1, 2 | Abgedeckt |
| Betroffener Bereich: Haltestellen-Monitor | 2, 3 | Abgedeckt |
| Betroffener Bereich: Aktualisierungs-Lebenszyklus | 3 | Abgedeckt |
| Betroffener Bereich: Vier Anzeige-Seiten (Start, Abfahrt, Details, Ergebnisse) | 3 | Abgedeckt |
| Betroffener Bereich: Testprojekt / Testabdeckung der Persistenz, Wiederherstellung, Lebenszyklus | 1, 2, 3 | Abgedeckt |
| Nicht-Ziel: Kein neues Refresh-Verhalten, kein UI-Umbau | Vorgehensentscheidung „Umfang" + Schritt 3 (Schritte/Loops/Hintergrund unverändert) | Abgedeckt |
| Nicht-Ziel: Fachliche Refresh-Schritte bleiben erhalten, nur Komposition zentralisiert | 3 (explizit) | Abgedeckt |
| Nicht-Ziel: Keine CI/Release-Änderungen | Kein Schritt plant solche Änderungen; Umfangsbegrenzung greift | Abgedeckt |

## Abhängigkeitsprüfung

| Prüfpunkt | Befund |
|-----------|--------|
| Zyklen in Abhängigkeiten | Keine gefunden (lineare Kette 1 → 2 → 3; alle Verweise auf frühere Schrittnummern auflösbar) |
| Reihenfolge konsistent zu inhaltlichen Abhängigkeiten | Ja — Schritt 2 baut sinnvoll auf der in Schritt 1 verschlankten Cache-Schnittstelle auf; Schritt 3 bindet u. a. den in Schritt 2 umgestellten Monitor-Pfad an den zentralen Einstieg an und profitiert fachlich von der geschaffenen Fassade |

## Hinweise

- Prozess-Anforderungen des Auftrags (PR gegen `staging`, der Issues #21–#24 per Closing-Tags schließt; Status-Dokumentation in den vier Issues) sind bewusst keinem einzelnen Entwicklungsschritt zugeordnet und beim Abschluss der Abarbeitung sicherzustellen.
- Die offene Frage zum Fassaden-Ort der Wiederherstellung ist über die Vorgehensentscheidung (Empfehlung: dedizierter Lese-Dienst in der Kernlogik) ausreichend adressiert; die konkrete Wahl trifft Schritt 2.
- Die Annahme zu zeitlosen Fahrten (verwerfen, Home-Semantik) ist im Plan als Vorgehensentscheidung festgelegt und konsistent zur Anforderung.
