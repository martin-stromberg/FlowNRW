# Tasks: Hintergrundaktualisierung und Wiederaufnahme

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---|---|---|---|
| 1 | Frische | RefreshFreshness mit injizierbarer Uhr und Cache-TTL anlegen | Erledigt | RefreshFreshnessTests_Boundaries |
| 2 | Modelle | StopMonitorViewModel frischeabhängig/extern abbrechbar aktualisieren | Erledigt | WindowsLifecycleUiTests: frischer/veralteter Einzelmonitor |
| 3 | Modelle | FavoriteMonitorViewModel frischeabhängig/extern abbrechbar aktualisieren | Erledigt | ResumeMonitorTests_Cancellation |
| 4 | Modelle | FavoriteHomeViewModel begrenzten Aktualisierungsbatch ergänzen | Erledigt | RefreshLifecycleTests_Background |
| 5 | Modelle | JourneySearchViewModel Resume ohne Navigation, Fehlererhalt und eindeutigen Fahrtabgleich ergänzen | Erledigt | JourneyResumeTests_Selection |
| 6 | Koordination | RefreshLifecycle mit Budget, Single-Flight, Aus und Handoff anlegen | Erledigt | RefreshLifecycleTests_Background |
| 7 | UI | App-Fensterereignisse mit Lifecycle verbinden | Erledigt | WindowsLifecycleUiTests |
| 8 | UI | HomePage Resume und Abbruch anbinden | Erledigt | WindowsLifecycleUiTests: Favoriten |
| 9 | UI | DeparturePage Resume und Abbruch anbinden | Erledigt | WindowsLifecycleUiTests: Einzelmonitor |
| 10 | UI | ResultsPage dynamische Ergebnisse und Resume anbinden | Erledigt | WindowsLifecycleUiTests: Ergebnisliste |
| 11 | UI | JourneyDetailPage dynamische Details, eindeutige Auswahl und Resume anbinden | Erledigt | WindowsLifecycleUiTests: Detailauswahl |
| 12 | UI | Such-/Karten-/Standortarbeit bei Deaktivierung abbrechen | Erledigt | WindowsJourneyUiTests: Routing/Maps/Locations; finale Kartenregression maps-retry.txt bestanden |
| 13 | UI | RefreshSettingsPage Hintergrundgrenzen erläutern | Erledigt | UiTest-Build; schmaler Screenshot geprüft |
| 14 | DI | Lifecycle-/Frische-/Plattformdienste registrieren | Erledigt | UiTest-Build und gestartete native Anwendung |
| 15 | iOS | IosBackgroundRefresh mit Registrierung, Planung, Expiration und genau einmaligem Abschluss implementieren | Erledigt | iOS Compile-Target bestanden; native iOS-Ausführung beim Nutzer |
| 16 | iOS | AppDelegate/Info.plist Taskkennung und fetch konfigurieren | Erledigt | iOS Compile-Target und statischer Kennungsabgleich |
| 17 | Tests | Frischegrenzen und Uhr deterministisch prüfen | Erledigt | RefreshFreshnessTests_Boundaries |
| 18 | Tests | Backgroundbegrenzung, Cancellation/Handoff, Fehler und Datenschutz prüfen | Erledigt | RefreshLifecycleTests_Background (einschließlich 20-Sekunden-Deadline) |
| 19 | Tests | Monitor-/Favoriten-Resume und Busy-/Revisionsschutz prüfen | Erledigt | ResumeMonitorTests_Cancellation und native Lifecycleprüfung |
| 20 | Tests | Journey-Resume, Fehlererhalt, fehlende/mehrdeutige Fahrt und Navigation prüfen | Erledigt | JourneyResumeTests_Selection |
| 21 | Fixtures | Alte/frische/fehlerhafte/späte Daten und Requestzähler ergänzen | Erledigt | WindowsLifecycleUiTests |
| 22 | E2E | WindowsLifecycleUiTests für Monitor, Favoriten, Ergebnisse/Details und Resume durchführen | Erledigt | docs/help/monitorintervalle/verification-lifecycle/index.md |
| 23 | E2E | WindowsJourneyUiTests alle integrierten Modi durchführen | Erledigt | artifacts/step8-ui: routing, monitors, maps, locations-retry, favorites jeweils bestanden |
| 24 | E2E | WindowsRefreshUiTests an Frischeprüfung anpassen und nativ durchführen | Erledigt | docs/help/monitorintervalle/verification-lifecycle/index.md |
| 25 | Prüfungen | Release/UiTest, Core/Coverage, Format/XML, Release-Skripte prüfen | Erledigt | docs/help/monitorintervalle/verification-lifecycle/index.md |
| 26 | Dokumentation | Hilfe, README, Release Notes, Plattformquellen und iOS-Gerätecheckliste aktualisieren | Erledigt | docs/help/monitorintervalle/verification-lifecycle/index.md |
