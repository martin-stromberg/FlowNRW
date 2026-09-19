# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine konkreten Produktfehler im geprüften Stand gefunden.

## Geprüfte Dateien

- `FlowNRW.Core/Favorites/IFavoriteStore.cs`
- `FlowNRW.Core/Favorites/JsonFavoriteStore.cs`
- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- `FlowNRW.Core/Maps/MapViewModel.cs`
- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/AppShell.xaml.cs`
- `FlowNRW/MauiProgram.cs`
- `FlowNRW.Tests/FavoriteTests.cs`

## Hinweise

Unabhängige statische Prüfung am 19.09.2026. Technische Stopidentität, Namespace-Deduplizierung, begrenztes Sourcegenerated JSON, Übernahme erst nach erfolgreicher Speicherung, Tempdatei-Ersetzung samt Fehlererhalt, Entfernungssortierung und referenzbasierte Favoriten-/Kartenauswahl geprüft. Keine zusätzliche aktuelle Positions- oder Abfahrtenpersistenz.

DI erzeugt durch die transient registrierten IDepartureService-Instanzen tatsächlich einen eigenen Service pro Karte. IsBusy und Commandschutz verhindern wiederholte gleichzeitige Abrufe derselben Karte. Entfernen/Seitenabgang invalidieren Revisionsstände; verspätete Antworten werden verworfen. Bestehende manuelle Karten-/Suchmitgliedschaft wird durch den separaten Favorite-Übernahmeweg nicht gelockert. Native Home-Seitenaktivität verhindert nachträgliche initiale Abfragen nach Seitenabgang.

Diese Codeprüfung ist kein ausgeführter Windows-/iOS-Testnachweis. Zwei noch fehlende ausdrücklich geplante Core-Testfälle sind in review.md aufgeführt; die aktuelle native Ausführung und abschließenden Checks werden separat fertiggestellt. Keine Produktdateien geändert.
