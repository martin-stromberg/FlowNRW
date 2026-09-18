# Aktueller Standort und nahe Haltestellen

Auf der Verbindungssuche lässt sich in der Start- und Zielkarte jeweils **Aktuellen Standort verwenden** wählen. Die App fragt die Betriebssystemberechtigung bei Bedarf an und ermittelt einmalig eine Position. Anschließend kann die Verbindung wie mit einem manuell gewählten Endpunkt gesucht werden. Eine fehlgeschlagene Standortabfrage erhält eine bereits abgeschlossene Auswahl; eine neue manuelle Eingabe ersetzt sie.

Unter **Haltestelle suchen** lädt **Haltestellen in meiner Nähe** die Umgebung der angeforderten Position. Die Ergebnisbuttons und die Kartenliste öffnen den Abfahrtsmonitor derselben Haltestelle. Entfernungen stammen aus der Anbieterantwort; fehlende oder ungültige Werte werden als unbekannt angezeigt. Eine fehlende Haltestellenposition verhindert nur deren Kartenmarker.

Bei verweigerter Berechtigung, deaktiviertem Standortdienst, Zeitüberschreitung oder fehlender Position zeigt die App einen verständlichen Hinweis. Manuelle Adresse, Haltestelle und Koordinate bleiben verfügbar. Berechtigungen können in den Systemeinstellungen überprüft werden; die App verändert diese Einstellungen nicht selbst.

## Datenschutz und Grenzen

Ein Standortzugriff erfolgt nur nach einer ausdrücklichen Aktion. Es gibt kein kontinuierliches Tracking und keine gespeicherte Bewegungshistorie. Für Routing oder Nahbereich wird die Position an die bereits konfigurierten Fahrplandienste übergeben; für Karten gelten die vorhandenen Kartenanbieter und technischen Cachegrenzen. Fehlermeldungen enthalten keine Koordinaten oder Betriebssystem-Exceptiontexte.

Die Genauigkeit hängt von Gerät und Betriebssystem ab; eine Position muss nicht von einem GPS-Sensor stammen. iOS kann ausdrücklich eine ungefähre Position liefern. Der Adapter akzeptiert höchstens zwei Minuten alte Positionen und keine Zeitstempel mehr als eine Minute in der Zukunft. Die Positionsabfrage ist auf 15 Sekunden begrenzt; die Zeit für eine Nutzerentscheidung im Berechtigungsdialog zählt nicht dazu.

Siehe [iOS-Geräteprüfung](ios-pruefung.md). Testnachweise werden unter `verification/` abgelegt; simulierte Standortzustände sind vom tatsächlichen Betriebssystemversuch zu unterscheiden.
