# Favoriten und Startseitenmonitore

## Entscheidungen und Ablauf

- Kleine IFavoriteStore-Schnittstelle und JSON-Dateispeicher im Core: geordnete Liste vollständiger technischer Stops, Duplikatschlüssel Source+Id (keine namensbasierte Verschmelzung), maximal100 Einträge, begrenzte Dateigröße, atomare Tempdatei-Ersetzung. Fehler überschreiben vorhandene Datei nicht blind. Sourcegenerated JSON statt Reflection. Keine aktuelle Position, Suchhistorie oder Abfahrten persistieren.
- FavoriteHomeViewModel lädt gespeicherte Stops, erzeugt pro Favorit einen unabhängigen Departure-Service und Monitorzustand. Startseite zeigt Name, bekannte/ungekannte Entfernung, nächste Abfahrten, Quelle und Fehlerstatus. Pro Karte manueller Refresh, gleichzeitiger Doppelklick löst keine Doppelabfrage aus. Seitenabgang bricht ausstehende Arbeit ab.
- Favorit toggeln im bestehenden Monitor über vollständigen SelectedStop; Home erlaubt Entfernen. Persistenz erfolgreich abwarten, bevor Erfolg gezeigt wird. Wiederholtes Hinzufügen erzeugt keinen zweiten Eintrag; entfernte Karten verschwinden und brechen ihren Request ab.
- Home ist echter Shell-Einstieg mit Verbindungssuche und Haltestellensuche als klaren Aktionen. Favoritenmonitor öffnet über explizite validierte Favoritenauswahl einen Einzelmonitor, ohne die bisherige Listen-/Kartenmitgliedschaft zu lockern. Favoritenkarte verwendet eine aktuelle Snapshotmenge und deren Mitgliedschaft. Bestehende Navigationsabläufe erhalten.
- Expliziter Button Entfernungen aktualisieren ruft bestehenden Standortservice auf, berechnet Luftlinie aus bekannten Koordinaten (Haversine), sortiert aufsteigend, unbekannte am Ende und stabile Reihenfolge bei Gleichstand. Bei Standortfehler Entfernung als unbekannt/stabile gespeicherte Reihenfolge; keine fiktive Null. Position nur flüchtig.
- MAUI-DI übergibt Storepfad und unabhängige Services. UiTest nutzt getrennte Persistenzdatei mit dokumentiertem Reset nur für Testdaten; zweiter Prozessstart verwendet dieselbe Datei. Release-/Nutzerfavoriten nie löschen.

## Dateien und Reihenfolge

1. Core Favorites: IFavoriteStore, JsonFavoriteStore, FavoriteHomeViewModel und Karten-/Monitorintegration; Tests für Datei-/Fehler-/Identitäts-/Distanz-/Concurrencyverhalten.
2. Native HomePage und Favoritenaktion DeparturePage, DI/Shellnavigation; bestehende Navigation auf vollständige aktuelle Favoriten begrenzen.
3. Native Harness: neuer Home-Einstieg, Favoriten-Hinzufügen/Entfernen/Neustart/Sortierung/Fehler und Regression.
4. Getrennte Reviews, gesamte Solution Release/UiTest, Corecoverage mindestens70%, Format/XML, native E2E, Hilfe/iOS-Anleitung/README/ReleaseNotes, Commit und unabhängige Projektabnahme.

## Verbindliche Tests

Core: technische Stopidentität über Dateineustart, Duplikate, atomarer Fehlererhalt, ungültige/überdimensionierte Datei, keine private Position gespeichert; Entfernung bekannte/unbekannte/Gleichstand/Standortfehler; unabhängige Refreshes, keine Doppelabrufe, Fehler behalten Daten, Entfernen/Seitenabgang verwerfen alte Antworten; veraltete Favoritenauswahl navigiert nicht.

Native Windows: leerer echter Home-Einstieg → Haltestellensuche → Monitor → Favorit hinzufügen → Home mit Abfahrten; zweiter Appstart erhält Favorit; Duplikat vermeiden/Entfernen/Leerzustand; mehrere Favoriten nach expliziter Fixture-Position sortieren, ohne Position stabile Ordnung; einzelne Refreshfehler behindern andere Karte nicht; Monitor/Karte/Verbindungssuche und Rücknavigation; schmale Ansicht und Tastatur. Bestehende Routing-/Monitor-/Karten-/Standortläufe über neuen Home-Einstieg erneut ausführen. Keine VM-Aufrufe als UI-Nachweis.

Zusätzliche native Pflichtfälle: UiTest-Speicherfehler beim Hinzufügen und Entfernen muss sichtbar bleiben, ohne erfolgreichen Speichervorgang vorzutäuschen; nach Wiederherstellung erneut speichern/entfernen und zweiten Prozessstart prüfen. Langsamer Refresh auf Karte A lässt Karte B bedienbar und unabhängig aktualisierbar; Doppelklick auf A startet nur einen Abruf (UiTest-Aufrufzähler). Die Fehlersteuerung und Zähler sind ausschließlich im UiTest-Build enthalten.

## Nicht-Ziele und offene Punkte

Automatische Intervalle folgen in Schritt7, Hintergrund in Schritt8. Keine neuen Pakete, keine iOS-CI, kein Deployment/IIS. Native iOS-Geräteprüfung bleibt beim Nutzer. Keine offenen fachlichen Fragen.
