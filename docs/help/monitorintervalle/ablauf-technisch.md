# Technischer Ablauf der Monitorintervalle

## Einstellung und Speicherung

`RefreshSettingsViewModel` trennt `SelectedSeconds` als Entwurf von `IntervalSeconds` als wirksamem Wert. Gültig sind ausschließlich 0, 30, 60, 120 und 300; 0 bedeutet Aus. `LoadAsync` löst einmalig gespeicherte Werte oder den sicheren Standard 60 auf. Fehler werden ohne externe Exceptiontexte angezeigt. `IsLoaded` verhindert, dass Seiten vor dem Laden kurzzeitig einen falschen Standardtimer starten.

`SaveAsync` wartet auf `IRefreshSettingsStore.SaveAsync`. Erst danach setzt es den wirksamen Wert und meldet `Changed`. Bei Speicherfehler bleibt der vorherige Wert erhalten. `JsonRefreshSettingsStore` speichert eine einzelne JSON-Zahl, liest höchstens 64 Bytes und ersetzt die Datei über eine temporäre Datei im selben Verzeichnis atomar. Ungültige Dateien werden beim Lesen nicht überschrieben; ein ausdrückliches Speichern darf sie ersetzen. Keine Favoriten, Abfahrten oder Positionen werden in dieser Datei gespeichert.

60 Sekunden sind der Standard. Die Mindestwahl 30 Sekunden entspricht der vorhandenen Echtzeit-Cachefrist und begrenzt häufige Abrufe. Die Obergrenze 300 Sekunden hält die Auswahl überschaubar. Diese Werte garantieren weder Netzantwortzeiten noch die Aktualität der vom Anbieter gelieferten Daten.

## Sichtbarkeit und Taktung

`App.CreateWindow` überträgt `Activated`, `Deactivated` und `Destroying` in `ForegroundState.IsActive`. `HomePage` und `DeparturePage` kombinieren Fensteraktivität, Seitenaktivität und `IsLoaded`. Auf der Startseite existiert eine eigene `RefreshLoop` pro aktueller Favoritenkarte; entfernte Karten verlieren ihre Schleife. Seitenabgang entfernt Abonnements und beendet ausstehende Arbeit.

`RefreshLoop.Start` ist bei unverändertem Intervall idempotent. Der erste Tick folgt nach einem vollständigen `Task.Delay`; nach jedem abgeschlossenen oder fehlgeschlagenen Update wartet die Schleife erneut vollständig. Intervallwechsel beendet die alte Schleife. `Stop` cancelt den Delay und invalidiert über `CancelPending` den laufenden Monitorrequest. Revisionen verhindern die Übernahme verspäteter Antworten. Aus startet keinen Timer. Fortsetzungen bleiben auf dem aufrufenden UI-Kontext; es gibt kein `Task.Run` für ViewModels.

`RefreshAutomaticallyAsync` und `RefreshAsync` verwenden denselben Busy-Schutz in `StopMonitorViewModel` beziehungsweise `FavoriteMonitorViewModel`. Ein paralleler manueller Aufruf startet deshalb keinen zweiten Abruf desselben Monitors. Die Favoriten behalten ihre unabhängigen Departure-Serviceinstanzen. Der bestehende `ProviderOrchestrator` fasst exakt gleiche Requests zusammen; unterschiedliche Abfahrtszeitstempel sind unterschiedliche Schlüssel und werden hier nicht pauschal als zusammengefasste Abrufe bezeichnet.

## Fehler und Grenzen

Der Monitor behält bei einem Fehler das letzte erfolgreiche Ergebnis einschließlich dessen Metadaten; ein erfolgreiches leeres Ergebnis ersetzt es. Provider-Retry-, HTTP-Timeout-, Cache- und Diagnosegrenzen bleiben unverändert. Die Intervallschleife selbst wiederholt erst nach der nächsten Wartezeit und erzeugt keinen Aufholstau.

Dies ist ausschließlich Vordergrundsteuerung. Die Anbindung von Suspendierung/Wiederaufnahme und unterstützten iOS-Hintergrundmechanismen folgt gesondert. Windows-Fensterdeaktivierung ist ein tatsächlicher Zustandswechsel, auch wenn das Fenster sichtbar bleibt.
