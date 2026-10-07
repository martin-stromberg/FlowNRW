# Code Review – Navigation und Verbindungsfavoriten

**Nachprüfung:** 04.10.2026
**Umfang:** aktueller Arbeitsbaum gegen `requirement-navigation-favorites.md` und offene Punkte aus `requirement-departure-ux.md`
**Status:** Beide zuletzt geprüften Änderungen sind geschlossen. Als Freigabepunkt bleibt der erfolgreiche native UI-Nachweis offen.

## Nachverfolgung früherer Codebefunde

- **Linienübersicht:** `HomePage` nutzt jetzt `TransitVisuals.Badge`; der Summary-Container ist sichtbar, wenn eingeklappt, und verschwindet beim Aufklappen.
- **Favoritencache beim Suchmonitor:** `StopMonitorViewModel` erhält `IFavoriteStore` und `IDepartureCacheStore`; bei Cachemiss wird der persistente Cache nur für explizit gespeicherte Haltestellen geprüft. Versionsprüfung schützt das Ergebnis vor verzögerten Cacheantworten.
- **Teilantwort im Suchmonitor:** `RefreshCoreAsync` fordert jetzt `IsComplete` für die Übernahme. Bei Fehlern/Warnungen bleibt das vorherige `Result` stehen.
- **Maximales Ereignislimit:** `DepartureService` vergleicht jetzt mit `> MaxResults`; ein neuer Test belegt den vollständigen Bestand exakt am Limit.
- **Storefehler auf Ergebnisseite:** `ResultsPage.OnAppearing` behandelt den Fehler von `ConnectionFavoritesViewModel.LoadAsync`.
- **Widerspruch bei partieller Antwort, behoben:** `FavoriteMonitorViewModel.RefreshCoreAsync` übernimmt nur noch vollständige Antworten. Der aktualisierte Integrationstest prüft, dass ein vorheriger vollständiger `Result`-Bestand bei einer Teilantwort erhalten bleibt.
- **Frische im Suchmonitor, behoben:** `StopMonitorViewModel.ReadPersistentFavoriteCacheAsync` prüft jetzt `RefreshFreshness.IsStale` auf dem geladenen Cacheeintrag.
- **Frische bei Startseitenwiederherstellung, behoben:** `FavoriteHomeViewModel` übergibt die konfigurierte `RefreshFreshness` an `FavoriteMonitorViewModel.Restore`. Dort werden neben vergangenen Ereignissen auch alte oder zukunftsdatierte Providerantworten verworfen. `LoadAsync_DoesNotRestoreStaleBoardWithFutureDeparture` deckt einen abgelaufenen Cache mit noch zukünftiger Abfahrt ab.

## Verbleibende Codebefunde

### Niedrig – Striktes JSON-Lesen blockiert Verbindungfavoriten bei Erweiterung

`JsonConnectionFavoriteStore` konfiguriert `UnmappedMemberHandling.Disallow`; jedes unbekannte Feld führt beim Laden zum Fehler der gesamten Liste. Die atomare Dateiablage und Längen-/Anzahlgrenzen begrenzen Risiken, und `ResultsPage`/`SearchPage` fangen jetzt Ladefehler ab. Vorwärtskompatibilität und Wiederherstellung bleiben aber ungeprüft; Tests für unbekannte Felder, unbekannte Version, beschädigte Datei, Schreibfehler und den Erhalt gültiger Einträge fehlen.

## Test- und Abdeckungslücken

- In `test-results-navigation-favorites.md` ist der native Journey-UI-Prozess weiterhin als hängen geblieben und manuell beendet dokumentiert. Speichern/Entfernen eines Verbindungsfavoriten, Neustart, Auswahl mit erhaltenen Zeitparametern, Swap und Provideraufrufszähler sind damit nicht ausgeführt nachgewiesen.
- Die Cache-Navigation hat keinen nativen Nachweis für gespeicherten Cache beim Suchaufruf, Rückkehr mit Trefferliste, verspätete Antwort A nach Auswahl B, Cacheleseabbruch und Nichtfavorit ohne JSON-Schreibzugriff.
- Die erweiterten Abfahrtscache-UI-Skripte sind vorhanden, aber das Testprotokoll dokumentiert keinen erfolgreichen nativen Lauf dieser Skripte.
- Kein UI-Test prüft derzeit sichtbar farbige Summary-Badges, Ausblenden im geöffneten Zustand, Zeiten je Linie, Screenreader-Semantik und Touch.
- Für die Detailbereinigung fehlen aktualisierte UI-/Präsentationstests der Kerninformationen bei mehrteiliger Fahrt, Umstieg, Fußweg und fehlenden Daten.

## Security und Robustheit

Die Connection-Favorite-Datei ist größenbegrenzt (256 KiB), auf 100 Verbindungen limitiert, validiert Stop-IDs/Namen/Quellen und wird über temporäre Datei atomar ersetzt. Werte aus Stop-Identitäten werden nicht als Dateipfade benutzt. Ich sehe im aktuellen Diff keine direkte Pfadmanipulation oder unbeschränkte JSON-Ressource. Der offene Robustheitspunkt ist das strikte Formatverhalten: Erweiterte Dateien machen die ganze Favoritenliste unzugänglich, auch wenn der Fehler inzwischen auf den betroffenen Seiten abgefangen wird.

## Reviewentscheidung

**Codeprüfung der beiden zuletzt gemeldeten Befunde bestanden.** Die Favoritenkarte behält bei unvollständigen Antworten ihren letzten vollständigen Bestand. Beide persistenten Cache-Einstiege (Startseite und Suchmonitor) wenden `RefreshFreshness` an; ein deterministischer Integrationstest deckt den Startseitenfall ab. Es verbleiben keine Codeblocker aus diesen beiden Punkten. Der native UI-Nachweis für Verbindungfavoriten und Such-/Cache-Navigation ist weiterhin offen. In dieser gezielten Nachprüfung wurden keine Tests oder Builds gestartet.
