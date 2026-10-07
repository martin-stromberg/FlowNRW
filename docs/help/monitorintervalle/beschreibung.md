# Automatische Aktualisierung verwenden

Auf der Startseite oder im Abfahrtsmonitor **Aktualisierung einstellen** öffnen. Auf **Automatische Aktualisierung** zeigt **Aktualisierungsintervall** die verfügbaren Werte:

| Auswahl | Wirkung |
|---|---|
| Aus | Keine automatische Aktualisierung; **Aktualisieren** bleibt verfügbar. |
| 30 Sekunden | Kürzeste Wartezeit zwischen automatischen Abrufen. |
| 60 Sekunden | Standard beim ersten Start. |
| 2 Minuten | Entspricht 120 Sekunden. |
| 5 Minuten | Entspricht 300 Sekunden. |

Eine Auswahl wird erst durch **Speichern** wirksam. Nach **Einstellung gespeichert.** gilt sie für Einzelmonitore und Favoritentafeln und bleibt nach einem App-Neustart erhalten. Ohne Speichern bleibt das bisherige Intervall aktiv. Die Anzeige **Automatische Aktualisierung: …** nennt immer den wirksamen Wert.

Nach Rückkehr zum Monitor beginnt zunächst die volle Wartezeit. Ist eine Aktualisierung fertig, beginnt das Intervall erneut. Ein langsamer Abruf führt deshalb zu einem längeren Abstand; die App holt keine verpassten Abrufe in einer Serie nach. Erfolgreiche Abrufe mit Abfahrten sind als **automatisch aktualisiert** beziehungsweise **manuell aktualisiert** bezeichnet.

Jede Favoritentafel aktualisiert unabhängig. **Aktualisieren** bleibt auch bei ausgeschalteter Automatik nutzbar; während der jeweilige Monitor bereits lädt, startet kein zweiter Abruf. Das erste Öffnen eines Monitors beziehungsweise einer noch nicht geladenen Favoritentafel lädt deren Abfahrten unabhängig von **Aus**.

## Vordergrund und Navigation

Die Intervallabrufe laufen nur auf der geöffneten Monitor- oder Startseite im aktiven App-Fenster. Der Wechsel zur Suche, zu Einstellungen oder in ein anderes Fenster beendet sie. Auch ein noch sichtbares Windows-Fenster ist nach dem Wechsel in eine andere Anwendung nicht mehr aktiv.

Beim erneuten Aktivieren prüft die App die angezeigten Abfahrten oder Verbindungen: Frische Daten bleiben erhalten, veraltete Daten werden bei eingeschalteter Automatik sofort erneuert. Danach beginnt für Monitore wieder die volle Intervallwartezeit. **Aus** verhindert auch diese automatische Erneuerung. Die Standortfreigabe wird dabei nicht erneut angefordert.

In den Verbindungsdetails bleibt dieselbe Fahrt nur ausgewählt, wenn sie eindeutig wiedergefunden wurde. Andernfalls erscheint **Die gewählte Verbindung ist nicht mehr eindeutig bestätigt. Bitte in der Ergebnisliste neu auswählen.** Über **Zurück** eine aktuelle Verbindung wählen. Fehler behalten die letzten bekannten Daten und kennzeichnen sie entsprechend.

Auf iOS kann das System gespeicherte Favoriten gelegentlich im Hintergrund aktualisieren. Zeitpunkt und Ausführung sind nicht garantiert; ein gesperrtes Gerät wird nicht im gewählten Vordergrundtakt aktualisiert. **Aus** unterbindet diese automatischen Datenabrufe ebenfalls. Quelle und Datenstand bleiben für die Beurteilung der Angaben entscheidend.

## Wenn etwas nicht funktioniert

**Einstellung konnte nicht gespeichert werden:** Der bisherige Wert bleibt wirksam. Die Auswahl erneut speichern; erst die Erfolgsbestätigung zählt. Bei einem Ladefehler verwendet die App den Standard von 60 Sekunden und meldet ihn ausdrücklich. Eine Auswahl und **Speichern** ersetzen die fehlerhafte Einstellung.

**Aktualisierung fehlgeschlagen:** Bereits geladene Abfahrten bleiben als letzte bekannte Daten sichtbar. Quelle und Datenstand beachten; alte Angaben sind keine aktuelle Echtzeitbestätigung. **Aktualisieren** erneut versuchen. Bei eingeschalteter Automatik folgt der nächste Versuch erst nach dem Intervall. Ein erfolgreiches Ergebnis ohne nächste Abfahrten ersetzt die frühere Liste.

**Es passiert nach dem Speichern zunächst nichts:** Zum Monitor zurückkehren, das App-Fenster aktiv halten und das vollständige Intervall abwarten. Unterwegs in Einstellungen oder Suche werden die alten Monitore nicht weiter automatisch geladen.
