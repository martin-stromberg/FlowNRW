# Tasks: Einheitlicher Einstieg zum Wiederherstellen persistierter Favoriten-Boards

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Fassade | `RestoredDepartureBoard`-Record (`Result`, `Lines`) mit XML-Doku in `FlowNRW.Core/Favorites/DepartureBoardCache.cs` anlegen | Offen | — |
| 2 | Fassade | `DepartureBoardCache` anlegen: Ctor `(IDepartureCacheStore, IFavoriteStore, RefreshFreshness)`, `gate`-Semaphore, `RestoreBoardAsync` (Membership → gebündelte Lesung → `Project`), `RestoreBoardsAsync` (Keys-Filter → `Project`), interne `static` Hilfen `EffectiveTime`/`LineName`/`SortLines` | Offen | `DepartureBoardCacheTests` |
| 3 | ViewModel | `FavoriteMonitorViewModel.Restore` auf `internal bool Restore(RestoredDepartureBoard)` umstellen; private `EffectiveTime`/`LineName`/`SortedLines` entfernen und Aufrufstellen auf Fassaden-Hilfen umleiten | Offen | `FavoriteDepartureCacheIntegrationTests` |
| 4 | ViewModel | `FavoriteHomeViewModel`: Ctor-Parameter `DepartureBoardCache? boards` mit Auto-Default aus `cache`/`store`/`freshness`; `LoadAsync`-Restore-Block auf `RestoreBoardsAsync` umstellen (`RemoveOrphansAsync` unverändert) | Offen | `FavoriteDepartureCacheIntegrationTests` |
| 5 | ViewModel | `StopMonitorViewModel`: Felder/Ctor-Parameter `favoriteStore`/`departureCache`/`freshness` durch `DepartureBoardCache? boards` ersetzen | Offen | Build + `StopMonitorTests` |
| 6 | ViewModel | `StopMonitorViewModel.ReadPersistentFavoriteCacheAsync` entfernen; `OpenStopAsync` nutzt `boards.RestoreBoardAsync(stop, token)` mit `request`-CTS, OCE-/Fehlerbehandlung und unverändertem `version`-Guard | Offen | `ConnectionFavoriteTests` |
| 7 | ViewModel | Private `EffectiveTime` in `StopMonitorViewModel` entfernen; Aufrufstellen in `OpenStopAsync` und `RefreshCoreAsync` auf `DepartureBoardCache.EffectiveTime` umleiten (Filterprädikate unverändert) | Offen | `StopMonitorTests` unverändert grün |
| 8 | DI | `DepartureBoardCache`-Singleton in `MauiProgram` registrieren; `StopMonitorViewModel`- und `FavoriteHomeViewModel`-Fabriken verdrahten | Offen | Build |
| 9 | Tests | `ConnectionFavoriteTests.StopSearch_UsesPersistentFavoriteBoardBeforeRefresh`: Ctor-Argumente auf `DepartureBoardCache`-Verpackung umstellen | Offen | selbst |
| 10 | Tests | `ConnectionFavoriteTests.StopSearch_DoesNotDisplayStalePersistentBoard` auf einheitliche Semantik umstellen (stale Board wird angezeigt und `IsStale`-markiert, dann Live-Ersatz) | Offen | selbst |
| 11 | Tests | Neue `DepartureBoardCacheTests.cs` anlegen: Membership-Verweigerung, fehlender Eintrag, zeitlose/abgelaufene Items, Stale-Markierung, Linieninventar ohne Board, Keys-Filter des Bulk-Pfads, Cancellation | Offen | selbst |
| 12 | Tests | Neuen Monitor-Test `StopSearch_PersistentBoardForNonFavoriteIsIgnored` in `ConnectionFavoriteTests` anlegen | Offen | selbst |
| 13 | E2E | `WindowsDepartureCacheUiTests.ps1` um Monitor-Pfad-Stale-Nachweis erweitern (geseedete `departure-cache.json` mit altem `RetrievedAt`; Öffnen via Stopsuche; `Departure0`/`MonitorNotice`; Live-Ersatz) | Offen | Skriptlauf |
| 14 | Verifikation | `dotnet format --severity error`, Build mit `TreatWarningsAsErrors`, `dotnet test` (≥ 70 % Coverage), UI-Testlauf | Offen | — |
