# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Arbeitsmodus

26.09.2026: Lokale technische Prüfung gemäß Lifecycle-Ausweichregel. Der separate Review-Agent ist am Nutzungslimit ausgefallen. Diese Prüfung ist keine unabhängige Agentenabnahme. Basis ist der Projektbranch task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app einschließlich der noch uncommitteten Schritt-8-Dateien.

## Geprüfte Aspekte

- Gemeinsame Busy-/Revisionssperren für manuelle, automatische und Resume-Abrufe; veraltete Antworten dürfen aktuelle Sitzungen nicht überschreiben.
- Hintergrundlauf: Einzelbesitz, 20-Sekunden-Deadline, OS-Token, Vierergruppen, Abbruch vor Vordergrundmeldung, keine GPS-/Navigations-/Persistenzaufrufe.
- Fehler behalten Ergebnis und dessen Quelle. Fahrtabgleich akzeptiert nur eindeutige vollständige Ereignisidentität; unvollständige Fußwegidentität wird konservativ nicht bestätigt.
- Seitenabos werden entfernt; dynamische Ergebnis-/Favoritenbuttons trennen Commands vor dem Entfernen. Keine RaiseUiActionRequested-Aufrufe vorhanden.
- iOS-Kennung und Plist stimmen überein; Registrierung vor Launch-Abschluss, MainQueue-Callback, einmaliger Abschluss und erneute opportunistische Planung. C#-Compile-Nachweis ersetzt keine native iOS-Ausführung.
- Test-Fixtures bleiben auf UiTest beschränkt; Windows-Workflows sind unverändert.

## Behobene Beobachtungen

- HomePage konnte nach einem noch laufenden initialen Laden trotz zwischenzeitlicher Fensterdeaktivierung neue Abfahrten starten. Vor RefreshMissingAsync wird nun die Fensteraktivität geprüft.
- MapPage nahm nach Pause noch WebView-Nachrichten an. Nach dem Ready-Handshake werden Nachrichten bei abgebrochenem Token oder inaktivem Fenster verworfen.
- Der schmale Umgebungstest las unmittelbar nach Resize alte Layoutmaße. Er wartet nun maximal fünf Sekunden auf eine positive Breite bis 430 Pixel; danach bleibt die Assertion strikt. Vollständiger Standortlauf bestanden.
- Deadline sowie leere/ungültige Favoritenlisten waren nicht direkt abgesichert; drei zusätzliche Tests bestehen (235 Coretests insgesamt zum Zwischenstand).

## Geprüfte Dateien

- FlowNRW.Core/Refresh/RefreshFreshness.cs und RefreshLifecycle.cs
- FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs und FavoriteMonitorViewModel.cs
- FlowNRW.Core/Presentation/StopMonitorViewModel.cs und JourneySearchViewModel.cs
- FlowNRW/App.xaml.cs, MauiProgram.cs, HomePage.cs, DeparturePage.cs
- FlowNRW/ResultsPage.cs, JourneyDetailPage.cs, SearchPage.cs, StopSearchPage.cs, MapPage.cs, RefreshSettingsPage.cs
- FlowNRW/Platforms/iOS/AppDelegate.cs, IosBackgroundRefresh.cs und Info.plist
- FlowNRW.Tests/RefreshFreshnessTests_Boundaries.cs, ResumeMonitorTests_Cancellation.cs, JourneyResumeTests_Selection.cs und RefreshLifecycleTests_Background.cs
- tests/WindowsJourneyUiTests/UiTestFixtureServices.cs, WindowsJourneyUiTests.ps1, WindowsRefreshUiTests.ps1 und WindowsLifecycleUiTests.ps1

Finale Testberichte bleiben eine separate Phase; dieses Review behauptet keine noch laufenden E2E als bestanden.