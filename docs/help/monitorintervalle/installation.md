# Speicherung und Geräteprüfung

## Technische Konfiguration

Es sind keine neuen Pakete oder Server nötig. `MauiProgram` registriert Einstellung, Frischeprüfung und Lebenszykluskoordination. Reguläre Builds speichern `refresh-settings.json` in `FileSystem.AppDataDirectory`. Inhalt ist ausschließlich eine Zahl: `0`, `30`, `60`, `120` oder `300`. Die Datei wird über die sichtbare Einstellung geändert; keine manuelle Migration ist erforderlich. Das iOS-Target enthält `UIBackgroundModes=fetch` und `BGTaskSchedulerPermittedIdentifiers=de.martinstromberg.flownrw.refresh`; eine dauerhafte Standortberechtigung wird nicht angefordert.

Nur der `UiTest`-Build verwendet `FLOWNRW_UI_TEST_REFRESH_SETTINGS` oder ersatzweise `FileSystem.CacheDirectory/UiTest/refresh-settings.json`. Dieser isolierte Pfad darf für Testneustarts wiederverwendet werden. Release-Einstellungen und Favoritendateien nicht für Tests löschen. Test-Fehlersteuerungen gehören ausschließlich zum Fixture-Build. Produktive Intervalle werden durch die Fixtures nicht beschleunigt.

## Manuelle iOS-Prüfanleitung für den Nutzer

Native iOS-Ausführung ist hier nicht als bestanden behauptet. Auf einem tatsächlich gebauten iOS-Gerät oder Simulator prüfen und Geräte-/OS-/Buildstand sowie Resultat festhalten:

1. Ohne bisherige Intervalleinstellung die Startseite öffnen: **Aktualisierung einstellen** zeigt **60 Sekunden** und den Standardhinweis. Ein vorhandener gespeicherter Wert darf nicht durch den Standard ersetzt werden.
2. **30 Sekunden** wählen und **Speichern**. Zum Einzelmonitor zurückkehren, die App im Vordergrund halten und nach dem initialen Abruf mindestens ein volles Intervall plus Antwortzeit abwarten. Einen neuen Datenstand beziehungsweise **automatisch aktualisiert** prüfen.
3. **Aktualisieren** bedienen. Während bereits geladen wird, darf keine zweite parallele Anfrage entstehen; nach Abschluss bleibt die Aktion erneut bedienbar.
4. Mindestens zwei Favoriten auf der Startseite prüfen. Ein langsamer oder fehlgeschlagener Monitor darf die Bedienung und Aktualisierung der anderen Tafel nicht blockieren. Fehler müssen letzte Daten und ihren Datenstand erkennbar lassen.
5. **Aus** speichern. Auf Startseite und Einzelmonitor über mehr als 30 Sekunden prüfen, dass keine automatische Aktualisierung erfolgt; **Aktualisieren** muss weiterhin funktionieren. Anschließend **2 Minuten** beziehungsweise **5 Minuten** speichern und die Anzeige des wirksamen Werts prüfen.
6. App vollständig schließen und neu starten: zuletzt erfolgreich gespeicherter Wert bleibt erhalten. Einen Entwurf ohne **Speichern** verlassen: wirksamer Wert bleibt unverändert.
7. Zwischen Monitor, Einstellungen und Suche wechseln. Zurücknavigation startet keine mehrfachen Aktualisierungen. In eine andere App wechseln und zurückkehren: Bei eingeschalteter Automatik werden veraltete Daten sofort erneuert; anschließend beginnt die volle Intervallwartezeit. Innerhalb der 30-Sekunden-Frischefrist darf keine zusätzliche Anfrage entstehen. Ein gesperrtes oder suspendiertes Gerät ist kein Nachweis eines garantierten Hintergrundintervalls.
8. Mit vorübergehend fehlender Datenverbindung einen Refreshfehler prüfen. Letzte bekannte Daten und Quelle/Datenstand bleiben erkennbar; bei wiederhergestellter Verbindung manuell erneut versuchen. Keine echten persönlichen Standortdaten in Prüfberichten oder Screenshots festhalten.
9. Auswahl und **Speichern** mit VoiceOver sowie größerer Schrift bedienen. Sichtbare Rückmeldung, Fokus und vollständige Texte prüfen; Gerätebegrenzungen konkret dokumentieren.

Speicherfehler können im isolierten Windows-Fixturebetrieb gezielt erzeugt werden; dies ersetzt keinen iOS-Dateisystemnachweis. Auf einem regulären iOS-Gerät keine künstliche Beschädigung privater App-Daten allein für diesen Test verlangen. Nicht ausführbare Fälle mit Ursache als nicht ausgeführt kennzeichnen.

## Zusätzliche iOS-Lebenszyklusabnahme

Diese Fälle sind noch vom Nutzer auf Apple-Hardware auszuführen. Der erfolgreiche C#-Compile-Target unter Windows ist kein signierter iOS-Build und kein Beleg für einen ausgelieferten Hintergrundtask.

| Prüfung | Erwartung und Nachweis |
|---|---|
| Nativer Build und Start | Regulären iOS-Build auf unterstütztem Mac erstellen, signieren und starten. Registrierung vor Ende des Starts und identische Taskkennung in Plist/Adapter prüfen. Geräte-, OS- und Commitstand festhalten. |
| OS-Hintergrundaktualisierung erlaubt | Zwei Favoriten speichern, Automatik einschalten, App in den Hintergrund schicken. In einer Entwicklungssitzung den registrierten Task nach Apples Verfahren auslösen; Eintritt, Abschluss und Anzahl Provideraufrufe ohne Positionsdaten protokollieren. Keine Ausführung genau nach 15 Minuten verlangen. |
| OS-Hintergrundaktualisierung gesperrt | In iOS die Hintergrundaktualisierung deaktivieren. App darf nicht abstürzen oder fortlaufend planen; Vordergrund und manuelle Aktionen bleiben nutzbar. |
| Einstellung Aus | Task bei gespeicherter Einstellung Aus auslösen: keine Favoriten-Providerabrufe und kein Standortzugriff. Manuell im Vordergrund weiterhin laden. |
| Ablauf und Zeitbudget | Mit langsamer Testdatenquelle OS-Expiration auslösen und separat das interne 20-Sekunden-Budget ablaufen lassen. Jeweils genau eine Abschlussmeldung, beendete Arbeit und keine Übernahme einer späteren Antwort prüfen. |
| Wechsel während Hintergrundarbeit | Während eines langsamen Tasks App aktivieren. Hintergrundrequest wird zuerst abgebrochen; sichtbare veraltete Daten werden erneuert. Wiederholter Wechsel darf keine doppelten Timer erzeugen. |
| Verbindungen und Auswahl | Ergebnisliste und Details nach mehr als 30 Sekunden wieder aktivieren. Seite bleibt geöffnet. Eindeutig wiedergefundene Fahrt bleibt ausgewählt; fehlende/mehrdeutige Identität fordert neue Auswahl. Offline bleiben letzte Daten mit Fehlerhinweis erhalten. |
| Standort und Navigation | Standortabfrage beginnen und App deaktivieren; späte Antwort darf keine Auswahl oder Navigation auslösen. Resume fordert keinen neuen Standort an. |
| Prozessende | App neu starten: Favoriten/Einstellung bleiben gespeichert, Echtzeitdaten werden neu geladen. Keine gespeicherten Bewegungs-, Such- oder Abfahrtsverläufe entstehen. |

Ein Simulatorlauf ersetzt nicht die Prüfung der tatsächlichen OS-Planung auf einem Gerät. Anbieterausfälle, ausgebliebene OS-Ausführung und nicht verfügbare Testinstrumente getrennt als solche dokumentieren.

Technische Referenzen: [Apple: Using background tasks to update your app](https://developer.apple.com/documentation/uikit/using-background-tasks-to-update-your-app), [.NET: BGTaskScheduler](https://learn.microsoft.com/en-us/dotnet/api/backgroundtasks.bgtaskscheduler), [.NET: BGAppRefreshTaskRequest](https://learn.microsoft.com/en-us/dotnet/api/backgroundtasks.bgapprefreshtaskrequest).
