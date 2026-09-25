# Speicherung und Geräteprüfung

## Technische Konfiguration

Es sind keine neuen Pakete, Server oder Berechtigungen nötig. `MauiProgram` registriert die gemeinsame Einstellung und den Fensterzustand. Reguläre Builds speichern `refresh-settings.json` in `FileSystem.AppDataDirectory`. Inhalt ist ausschließlich eine Zahl: `0`, `30`, `60`, `120` oder `300`. Die Datei wird über die sichtbare Einstellung geändert; keine manuelle Migration ist erforderlich.

Nur der `UiTest`-Build verwendet `FLOWNRW_UI_TEST_REFRESH_SETTINGS` oder ersatzweise `FileSystem.CacheDirectory/UiTest/refresh-settings.json`. Dieser isolierte Pfad darf für Testneustarts wiederverwendet werden. Release-Einstellungen und Favoritendateien nicht für Tests löschen. Test-Fehlersteuerungen gehören ausschließlich zum Fixture-Build. Produktive Intervalle werden durch die Fixtures nicht beschleunigt.

## Manuelle iOS-Prüfanleitung für den Nutzer

Native iOS-Ausführung ist hier nicht als bestanden behauptet. Auf einem tatsächlich gebauten iOS-Gerät oder Simulator prüfen und Geräte-/OS-/Buildstand sowie Resultat festhalten:

1. Ohne bisherige Intervalleinstellung die Startseite öffnen: **Aktualisierung einstellen** zeigt **60 Sekunden** und den Standardhinweis. Ein vorhandener gespeicherter Wert darf nicht durch den Standard ersetzt werden.
2. **30 Sekunden** wählen und **Speichern**. Zum Einzelmonitor zurückkehren, die App im Vordergrund halten und nach dem initialen Abruf mindestens ein volles Intervall plus Antwortzeit abwarten. Einen neuen Datenstand beziehungsweise **automatisch aktualisiert** prüfen.
3. **Aktualisieren** bedienen. Während bereits geladen wird, darf keine zweite parallele Anfrage entstehen; nach Abschluss bleibt die Aktion erneut bedienbar.
4. Mindestens zwei Favoriten auf der Startseite prüfen. Ein langsamer oder fehlgeschlagener Monitor darf die Bedienung und Aktualisierung der anderen Tafel nicht blockieren. Fehler müssen letzte Daten und ihren Datenstand erkennbar lassen.
5. **Aus** speichern. Auf Startseite und Einzelmonitor über mehr als 30 Sekunden prüfen, dass keine automatische Aktualisierung erfolgt; **Aktualisieren** muss weiterhin funktionieren. Anschließend **2 Minuten** beziehungsweise **5 Minuten** speichern und die Anzeige des wirksamen Werts prüfen.
6. App vollständig schließen und neu starten: zuletzt erfolgreich gespeicherter Wert bleibt erhalten. Einen Entwurf ohne **Speichern** verlassen: wirksamer Wert bleibt unverändert.
7. Zwischen Monitor, Einstellungen und Suche wechseln. Zurücknavigation startet keine mehrfachen Aktualisierungen. In eine andere App wechseln und zurückkehren: ab erneuter Aktivierung startet die volle Wartezeit. Ein gesperrtes oder suspendiertes Gerät ist kein Nachweis eines garantierten Hintergrundintervalls.
8. Mit vorübergehend fehlender Datenverbindung einen Refreshfehler prüfen. Letzte bekannte Daten und Quelle/Datenstand bleiben erkennbar; bei wiederhergestellter Verbindung manuell erneut versuchen. Keine echten persönlichen Standortdaten in Prüfberichten oder Screenshots festhalten.
9. Auswahl und **Speichern** mit VoiceOver sowie größerer Schrift bedienen. Sichtbare Rückmeldung, Fokus und vollständige Texte prüfen; Gerätebegrenzungen konkret dokumentieren.

Speicherfehler können im isolierten Windows-Fixturebetrieb gezielt erzeugt werden; dies ersetzt keinen iOS-Dateisystemnachweis. Auf einem regulären iOS-Gerät keine künstliche Beschädigung privater App-Daten allein für diesen Test verlangen. Nicht ausführbare Fälle mit Ursache als nicht ausgeführt kennzeichnen.
