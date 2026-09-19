# Bestand Monitorintervalle

Schritt6 ist integriert und unabhängig abgenommen. Ausgangslauf im Unterordner inventory: 173 Coretests und 26 Release-Skripttests bestanden; native Windows-Nachweise des vorherigen Schritts sind historische Ausgangsnachweise, keine erneute Ausführung.

StopMonitorViewModel und FavoriteMonitorViewModel besitzen RefreshAsync, IsBusy, Revision und CancelPending. Manuelle Commands und direkte Aufrufe teilen denselben Busy-Schutz. Jede Favoritenkarte erhält einen eigenen transienten DepartureService. ProviderOrchestrator fasst exakt identische Requests zusammen; departure ist Teil des Schlüssels, deshalb keine Behauptung einer Zusammenfassung verschiedener Zeitstempel.

HomePage lädt Favoriten einmal, lädt fehlende Tafeln beim Erscheinen und bricht beim Verlassen ab. DeparturePage abonniert Modelländerungen beim Erscheinen und cancelt beim Verlassen. OpenStopAsync erfasst seine Revision vor Navigation und lädt erst danach; erste Timeraktivierung darf diese Revision nicht invalidieren. App.CreateWindow liefert bisher nur Window(shell), keine Vordergrundzustandsanbindung. Keine Settings-/Intervallpersistenz vorhanden.

JsonFavoriteStore bietet begrenzte atomare Persistenz als lokales Muster. Keine neuen Pakete erforderlich; Task.Delay und injizierbare Verzögerung genügen für deterministische Tests. UI-Harness enthält echte Prozessneustarts, unabhängige Fixturezähler, sichere Fokussteuerung und schmale Ansichten. Historische Regressionen erwarten manuelle Statusformulierungen; Testsettings müssen isoliert und bei alten Modi ausgeschaltet sein.

Lücken: gespeicherte Intervalleinstellung, Settings-UI, testbare Scheduler, Seiten- und Vordergrundaktivität, wahrheitsgemäßer Aktualisierungsstatus, native Tests realer Taktung. Anbieter-/Cache-/Retrygrenzen bleiben bestehen.

Plattformprüfung: Microsoft dokumentiert Activated/Deactivated auch unter Windows; Deactivated kann bei weiterhin sichtbarem Fenster auftreten. Für automatische Vordergrundmonitore wird bewusst Fokusaktivität verwendet. [MAUI-10-Lebenszyklus](https://learn.microsoft.com/de-de/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-10.0), geprüft19.09.2026. Stopped/Resumed und native iOS-Hintergrundarbeit bleiben Gegenstand vonSchritt8.
