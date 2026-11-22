# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Geprüfte Dateien

Liste aller geprüften Dateien:
- `FlowNRW.Core/Favorites/DepartureBoardCache.cs` (neu)
- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW/MauiProgram.cs`
- `FlowNRW.Tests/DepartureBoardCacheTests.cs` (neu)
- `FlowNRW.Tests/ConnectionFavoriteTests.cs`
- `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`
- `tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1`

Zweiter Lauf. Delta seit `review-code.1.md`: (1) explizite Array-Casts `(T[])[...]` in `UiTestFixtureServices.cs` als CsWinRT1032-Workaround — die Collection Expressions erhalten damit denselben konkreten Zieltyp (`T[]`) wie zuvor per Target-Typing; alle Ziel-Properties sind `IReadOnlyList<T>` (`ProviderResult.Items`/`Warnings`, `Journey.Legs`/`Transfers`, `GeoGeometry.Coordinates`), Laufzeitsemantik identisch. Verbleibendes ungecastetes `[]` in `DeparturesAsync` (Z. 52) ist korrekt: Der Ternary-Partner `lines` ist bereits `StopEvent[]`, das Collection Expression targetet damit nie das WinRT-projizierte Interface. (2) `-MonitorStale`-Block in `WindowsDepartureCacheUiTests.ps1` um Nicht-Favorit-Szenario erweitert: Favoriten-Datei auf `[]`, Restart, Öffnen über die Suche; `Find 'Departure0'` vor dem 700-ms-Fixture-Delay (`DeparturesAsync`, Z. 38) ist race-frei, `AwaitText 'Departure0' 'RE 1'` belegt danach die Live-Anzeige. Reale UI-Ausführung bestanden.

Stichproben-Verifikation des ersten Laufs: `DepartureBoardCache` (Membership-Gate vor Cache-Lesung, `gate`-Release im `finally`, `Project` mit Expiry-Filter und Stale-Markierung, `DepartureCacheKey`-Record-Equality in `IDepartureCacheStore.cs:36`), `StopMonitorViewModel.OpenStopAsync` (Restore-Block mit `request`-CTS, `ReferenceEquals`-Guard im `finally`, unveränderter `version`-/`SelectedStop`-Guard), `FavoriteHomeViewModel.LoadAsync` (`boards!` innerhalb des `cache is not null`-Guards konstruktionsbedingt non-null, `RemoveOrphansAsync` unverändert), `FavoriteMonitorViewModel.Restore` (Linieninventar auch ohne Board, Semantik zum alten `Restore` äquivalent).
