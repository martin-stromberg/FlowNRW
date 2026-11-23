# Automatische Aktualisierung

Die vollständige [Bedienung und Fehlerhilfe](../monitorintervalle/beschreibung.md), der [technische Ablauf](../monitorintervalle/ablauf-technisch.md) und die [iOS-Prüfanleitung](../monitorintervalle/installation.md) gelten gemeinsam für Favoriten und Einzelmonitore.

Über **Aktualisierung einstellen** wird das Intervall für sichtbare Einzel- und Favoritenmonitore gewählt. Möglich sind **Aus**, **30 Sekunden**, **60 Sekunden**, **2 Minuten** und **5 Minuten**; der Standard beträgt 60 Sekunden. Freie Werte werden nicht angenommen, damit die Anbieterlast begrenzt bleibt.

Die Auswahl wird erst mit **Speichern** wirksam und bleibt nach einem Neustart erhalten. Kann die Einstellung nicht geladen werden, zeigt die App den sicheren Standard von 60 Sekunden und einen Fehlerhinweis. Bei einem Speicherfehler bleibt das bisherige Intervall wirksam; die Aktion kann erneut versucht werden.

Die erste automatische Aktualisierung erfolgt nach dem vollständigen Intervall. Nach einer Aktualisierung beginnt die Wartezeit erneut; es gibt keine Aufholserie. Manuelles Aktualisieren bleibt möglich und startet während eines laufenden Abrufs keine zweite Anfrage. Jede Favoritentafel wird unabhängig aktualisiert.

Automatische Aktualisierung läuft nur bei sichtbarer Seite und aktivem App-Fenster im Vordergrund. Beim Seitenwechsel, beim Verlust des Fensterfokus oder bei **Aus** werden Wartezeit und ausstehender Abruf beendet. Beim Zurückkehren wird genau eine neue Schleife aufgebaut. Hintergrundaktualisierung und Wiederaufnahme über den App-Lebenszyklus folgen in Schritt 8.
