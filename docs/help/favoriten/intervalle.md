# Automatische Aktualisierung

Über **Aktualisierung einstellen** wird das Intervall für sichtbare Einzel- und Favoritenmonitore gewählt. Möglich sind **Aus**, 30, 60, 120 und 300 Sekunden; der Standard beträgt 60 Sekunden. Freie Werte werden nicht angenommen, damit die Anbieterlast begrenzt bleibt.

Die Auswahl wird erst mit **Speichern** wirksam und lokal als kleine Einstellung abgelegt. Eine beschädigte oder ungültige Datei aktiviert den sicheren Standard von 60 Sekunden; die Einstellung kann anschließend ausdrücklich ersetzt werden. Das Speichern ändert das wirksame Intervall erst nach erfolgreichem Schreiben.

Die erste automatische Aktualisierung erfolgt nach dem vollständigen Intervall. Nach einer Aktualisierung beginnt die Wartezeit erneut; es gibt keine Aufholserie. Manuelles Aktualisieren bleibt möglich und startet während eines laufenden Abrufs keine zweite Anfrage. Jede Favoritentafel besitzt ihre eigene Schleife.

Automatische Aktualisierung läuft nur bei sichtbarer Seite und aktivem App-Fenster im Vordergrund. Beim Seitenwechsel, beim Verlust des Fensterfokus oder bei **Aus** werden Wartezeit und ausstehender Abruf beendet. Beim Zurückkehren wird genau eine neue Schleife aufgebaut. Hintergrundaktualisierung und Wiederaufnahme über den App-Lebenszyklus folgen in Schritt 8.
