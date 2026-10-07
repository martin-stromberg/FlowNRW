# Technischer Ablauf der Monitorintervalle

## Einstellung und Speicherung

`RefreshSettingsViewModel` trennt `SelectedSeconds` als Entwurf von `IntervalSeconds` als wirksamem Wert. Gültig sind ausschließlich 0, 30, 60, 120 und 300; 0 bedeutet Aus. `LoadAsync` löst einmalig gespeicherte Werte oder den sicheren Standard 60 auf. Fehler werden ohne externe Exceptiontexte angezeigt. `IsLoaded` verhindert, dass Seiten vor dem Laden kurzzeitig einen falschen Standardtimer starten.

`SaveAsync` wartet auf `IRefreshSettingsStore.SaveAsync`. Erst danach setzt es den wirksamen Wert und meldet `Changed`. Bei Speicherfehler bleibt der vorherige Wert erhalten. `JsonRefreshSettingsStore` speichert eine einzelne JSON-Zahl, liest höchstens 64 Bytes und ersetzt die Datei über eine temporäre Datei im selben Verzeichnis atomar. Ungültige Dateien werden beim Lesen nicht überschrieben; ein ausdrückliches Speichern darf sie ersetzen. Keine Favoriten, Abfahrten oder Positionen werden in dieser Datei gespeichert.

60 Sekunden sind der Standard. Die Mindestwahl 30 Sekunden entspricht der vorhandenen Echtzeit-Cachefrist und begrenzt häufige Abrufe. Die Obergrenze 300 Sekunden hält die Auswahl überschaubar. Diese Werte garantieren weder Netzantwortzeiten noch die Aktualität der vom Anbieter gelieferten Daten.

## Sichtbarkeit und Taktung

`App.CreateWindow` überträgt `Activated`, `Deactivated`, `Stopped`, `Resumed` und `Destroying` über `RefreshLifecycle.SetActive` in `ForegroundState.IsActive`. Vor der Aktivierung wird laufende Hintergrundarbeit abgebrochen. `HomePage` und `DeparturePage` kombinieren Fensteraktivität, Seitenaktivität und `IsLoaded`. Auf der Startseite existiert eine eigene `RefreshLoop` pro aktueller Favoritenkarte; entfernte Karten verlieren ihre Schleife. Seitenabgang entfernt Abonnements und beendet ausstehende Arbeit.

`RefreshLoop.Start` ist bei unverändertem Intervall idempotent. Der erste Tick folgt nach einem vollständigen `Task.Delay`; nach jedem abgeschlossenen oder fehlgeschlagenen Update wartet die Schleife erneut vollständig. Intervallwechsel beendet die alte Schleife. `Stop` cancelt den Delay und invalidiert über `CancelPending` den laufenden Monitorrequest. Revisionen verhindern die Übernahme verspäteter Antworten. Aus startet keinen Timer. Fortsetzungen bleiben auf dem aufrufenden UI-Kontext; es gibt kein `Task.Run` für ViewModels.

`RefreshAutomaticallyAsync` und `RefreshAsync` verwenden denselben Busy-Schutz in `StopMonitorViewModel` beziehungsweise `FavoriteMonitorViewModel`. Ein paralleler manueller Aufruf startet deshalb keinen zweiten Abruf desselben Monitors. Die Favoriten behalten ihre unabhängigen Departure-Serviceinstanzen. Der bestehende `ProviderOrchestrator` fasst exakt gleiche Requests zusammen; unterschiedliche Abfahrtszeitstempel sind unterschiedliche Schlüssel und werden hier nicht pauschal als zusammengefasste Abrufe bezeichnet.

## Fehler und Grenzen

Der Monitor behält bei einem Fehler das letzte erfolgreiche Ergebnis einschließlich dessen Metadaten; ein erfolgreiches leeres Ergebnis ersetzt es. Provider-Retry-, HTTP-Timeout-, Cache- und Diagnosegrenzen bleiben unverändert. Die Intervallschleife selbst wiederholt erst nach der nächsten Wartezeit und erzeugt keinen Aufholstau.

Windows-Fensterdeaktivierung ist ein tatsächlicher Zustandswechsel, auch wenn das Fenster sichtbar bleibt.

## Wiederaufnahme

`RefreshFreshness` verwendet `TransitCacheOptions.RealtimeTimeToLive` (standardmäßig 30 Sekunden). Fehlende, als veraltet markierte, fehlgeschlagene oder zukünftig datierte Resultate benötigen einen neuen Abruf. Die sichtbare Seite aktualisiert über `RefreshIfStaleAsync` beziehungsweise `RefreshStaleAsync`. Metadaten werden auch ohne Abruf erneut bekanntgegeben. Bei Aus entfallen automatische Resume-Abrufe. Monitorintervalle beginnen erst nach Abschluss des Resume-Pfads; Busy-Sperren und Revisionen bleiben gemeinsam mit manuellen Aktionen wirksam.

`JourneySearchViewModel` erneuert die vorhandenen Suchendpunkte ohne Navigation. Eine Auswahl bleibt nur erhalten, wenn alle Abschnittsereignisse anhand Provider, Fahrt, Haltestellennamespace/-kennung und geplanter Zeit genau einmal zugeordnet werden können. Fehlende Identitäten, einschließlich nicht eindeutig identifizierbarer Fußwege, führen konservativ zur erneuten Auswahl. Providerfehler behalten das vorherige Ergebnis. Die Karte bleibt ein Snapshot der beim Öffnen gewählten Geometrie; Resume lädt ihre Basiskacheln erneut, erzeugt aber keine neue Fahrtzuordnung. Aktuelle Verbindungsdetails erhält man auf der Detailseite.

## Begrenzte iOS-Hintergrundarbeit

`IosBackgroundRefresh.Register` registriert `de.martinstromberg.flownrw.refresh` vor Abschluss von `FinishedLaunching`. `Info.plist` enthält diese Kennung und den Hintergrundmodus `fetch`. Beim Eintritt in den Hintergrund wird eine bestehende Anfrage derselben Kennung ersetzt; `EarliestBeginDate` liegt 15 Minuten später. Das ist weder ein Takt noch eine Ausführungsgarantie.

Der Callback läuft auf dem UI-Kontext. `RefreshLifecycle.RunBackgroundAsync` erlaubt nur einen Lauf bei inaktiver App, lädt die gespeicherte Einstellung und erneuert bei eingeschalteter Automatik veraltete Favoriten in Vierergruppen. Die bestehende Obergrenze von 100 Favoriten bleibt erhalten. Der komplette Lauf ist auf 20 Sekunden begrenzt; OS-Expiration und erneute Aktivierung brechen früher ab. `WaitAsync` und Revisionsprüfungen verhindern, dass nicht kooperative Provider den Abschluss blockieren oder verspätet neue Daten überschreiben. Der Adapter meldet den Abschluss einmal und plant eine spätere Anfrage.

Es gibt keine Hintergrund-GPS-Abfrage, Navigation oder zusätzliche Persistenz von Echtzeitdaten. Ergebnisse leben in der bestehenden Sitzung; nach Prozessende müssen sie erneut geladen werden. Native iOS-Ausführung ist separat anhand der [Gerätecheckliste](installation.md) zu prüfen.
