# Speicherung und Geräteprüfung

Die Release-App speichert höchstens 100 Favoriten in `favorites.json` im plattformeigenen AppData-Verzeichnis. Gespeichert werden ausschließlich technische Haltestellen-ID, Anbieter, Name, DHID und gegebenenfalls Haltestellenkoordinaten. Abfahrten, Suchhistorie und aktuelle Nutzerposition werden nicht persistiert. Identische IDs verschiedener Anbieter bleiben getrennt.

Die Datei ist auf 256 KiB begrenzt und wird über eine temporäre Datei atomar ersetzt. Eine beschädigte oder zu große vorhandene Datei wird nicht ungeprüft überschrieben. Lade- und Speicherfehler erscheinen in der Oberfläche.

Native Windows-Tests verwenden ausschließlich den `UiTest`-Build und eine separate Datei über `FLOWNRW_UI_TEST_FAVORITES`. Der Harness erzeugt einen eigenen Testpfad und verwendet ihn für vier echte Prozessstarts; Release-Favoriten werden nicht verändert. Szenariofelder und Abrufzähler sind nur im Testbuild enthalten.

Für die noch ausstehende iOS-Geräteprüfung:

1. Leere Startseite öffnen, Station suchen, im Monitor speichern und zur Startseite zurückkehren.
2. App vollständig beenden und neu starten; Haltestelle und technische Zuordnung müssen erhalten bleiben.
3. Mehrere Stationen speichern; einzelne Tafeln aktualisieren, Monitor/Karte öffnen und zurückkehren.
4. Standort erlauben und Entfernungen aktualisieren; anschließend fehlende Berechtigung und fehlende Position prüfen.
5. Favorit entfernen und App neu starten; entfernte Station bleibt entfernt.
6. Schmale Darstellung, Scrollen, VoiceOver und große Systemschrift auf dem Gerät prüfen.

Windows bleibt Release- und Testziel der GitHub Actions. iOS-CI und automatisiertes Deployment sind kein Bestandteil dieses Schritts.
