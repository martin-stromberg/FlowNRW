# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

- [x] `RestoredDepartureBoard` (public sealed record in `FlowNRW.Core/Favorites/DepartureBoardCache.cs`, Z. 100–107) — angelegt; `Result` (`ProviderResult<StopEvent>?`) und `Lines` (`IReadOnlyList<string>`) mit XML-Doku
- [x] `DepartureBoardCache` (public sealed class, `DepartureBoardCache.cs` Z. 7–97) — angelegt; Ctor `(IDepartureCacheStore, IFavoriteStore, RefreshFreshness? = null)` mit Default `new RefreshFreshness(new TransitCacheOptions())`; `SemaphoreSlim gate`; XML-Doku auf allen öffentlichen Membern
- [x] Methode `RestoreBoardAsync(Stop, CancellationToken)` in `DepartureBoardCache` (Z. 29–41) — Membership-Prüfung via `favorites.LoadAsync`, gebündelte Lesung unter `gate` mit CT, Eintragssuche per `(Source, StopId)`, Projektion via `Project`
- [x] Methode `RestoreBoardsAsync(IReadOnlyCollection<DepartureCacheKey>, CancellationToken)` in `DepartureBoardCache` (Z. 47–61) — einmalige Cache-Lesung, projiziert nur Einträge deren `Key` in `keys` liegt, liefert `IReadOnlyDictionary`
- [x] Private `Project(DepartureCacheEntry)` (Z. 63–76) — `now` einmalig; `Lines` aus `entry.Lines` bzw. `LineName`-Ableitung; `retained` filtert zeitlose/vergangene Items, sortiert; `retained.Length == 0` → `Result = null` mit `Lines`; sonst `IsStale`-Markierung (`entry.Result.IsStale || freshness.IsStale`)
- [x] Interne `static` Hilfen `EffectiveTime`, `LineName`, `SortLines` in `DepartureBoardCache` (Z. 81–96)
- [x] Feld `boards` + Ctor-Parameter `DepartureBoardCache? boards = null` in `FavoriteHomeViewModel` mit Auto-Default `boards ?? (cache is null ? null : new DepartureBoardCache(cache, store, this.freshness))` (Z. 16, 33–40)
- [x] `FavoriteHomeViewModel.LoadAsync`-Restore-Block (Z. 78–88) — `boards.RestoreBoardsAsync(keys, ct)` + `card.Restore(board)` innerhalb des `cache is not null`-Guards; `RemoveOrphansAsync` unverändert im selben Block
- [x] `FavoriteMonitorViewModel.Restore` auf `internal bool Restore(RestoredDepartureBoard)` umgestellt (Z. 84–93) — Karten-Zustand (`Lines`, `Result`, `LastAttempt`, `Status`, `RefreshBindings`); Mapping entfernt
- [x] Private `EffectiveTime`/`LineName`/`SortedLines` in `FavoriteMonitorViewModel` entfernt; Aufrufstellen in `RefreshCoreAsync`/`EnrichLines` auf `DepartureBoardCache.*` umgeleitet
- [x] `StopMonitorViewModel`: Felder `favoriteStore`/`departureCache`/`freshness` entfernt, `private readonly DepartureBoardCache? boards` neu; Ctor-Parameter ersetzt inkl. XML-Doku (Z. 14, 32–40)
- [x] `ReadPersistentFavoriteCacheAsync` und privates `EffectiveTime` in `StopMonitorViewModel` entfernt
- [x] `StopMonitorViewModel.OpenStopAsync` (Z. 224–240) — `using var source`, `request = source`, `boards.RestoreBoardAsync(stop, source.Token)`; `catch (OperationCanceledException)` → Öffnen abgebrochen; `catch (Exception)` → `cached` bleibt `null`; `finally { if (ReferenceEquals(request, source)) request = null; }`; `version`-Guard unverändert
- [x] `RefreshCoreAsync`-Aufrufstellen auf `DepartureBoardCache.EffectiveTime` umgeleitet, Filterprädikat unverändert (umgekehrte Semantik `is not { } time ||` bleibt — Scope-Ausschluss 3 eingehalten)
- [x] `MauiProgram`: `DepartureBoardCache`-Singleton mit expliziter 3-Argument-Konstruktion (geteilter `RefreshFreshness`) registriert (Z. 155–156); `StopMonitorViewModel`-Fabrik (Z. 105) und `FavoriteHomeViewModel`-Fabrik (Z. 157–160) auf Fassade verdrahtet
- [x] `FlowNRW.Tests/DepartureBoardCacheTests.cs` neu — alle 7 geplanten Tests vorhanden (`RestoreBoardAsync_NonFavoriteReturnsNullWithoutBoard`, `..._FavoriteWithoutEntryReturnsNull`, `..._DropsTimelessAndExpiredItems`, `..._StaleBoardIsReturnedMarked`, `..._OnlyExpiredKeepsLineInventory`, `RestoreBoardsAsync_ProjectsOnlyGivenKeys`, `RestoreBoardAsync_HonorsCancellation`) plus Fakes `RecordingFavorites`/`RecordingDepartureCache` mit Aufrufzählern
- [x] `ConnectionFavoriteTests.StopSearch_UsesPersistentFavoriteBoardBeforeRefresh` auf `boards:`-Verpackung (2-Argument-Fassade) umgestellt
- [x] `ConnectionFavoriteTests.StopSearch_DoesNotDisplayStalePersistentBoard` → `StopSearch_DisplaysStalePersistentBoardMarked` umgestellt (3-Argument-Fassade mit `TransitTestClock`-Freshness; Asserts `Source == "stale"`, `IsStale`, Live-Ersatz)
- [x] `ConnectionFavoriteTests.StopSearch_PersistentBoardForNonFavoriteIsIgnored` neu angelegt
- [x] `tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1` um Pflicht-Schalter `-MonitorStale` erweitert — `RetrievedAt`-Patch, Öffnen via Stopsuche, `Departure0`-/`MonitorNotice`-Nachweis, Live-Ersatz

## Offene Aufgaben

- [ ] E2E-Szenario „Nicht-Favorit mit zufällig gleich gelagertem Cache-Eintrag zeigt kein persistiertes Board" (Plan, E2E-Tabelle, Priorität „Empfohlen") — fehlt vollständig: Im `-MonitorStale`-Block von `WindowsDepartureCacheUiTests.ps1` ist nur das Pflicht-Szenario implementiert; der Membership-Nachweis über die UI fehlt.
- [ ] UI-Testlauf (Plan, Umsetzungsreihenfolge Schritt 8 „Verifikation") — ausstehend: Das erweiterte Skript wurde noch nicht gegen einen MAUI-Windows-Build ausgeführt. `dotnet test FlowNRW.Tests` ist vollständig grün (312/312), Build ohne Warnungen/Fehler.

## Hinweise

- Gesamte Code-Umsetzung liegt als uncommittete Änderungen im Working Tree (keine Commits auf dem Branch außer Doku) — beim Commit beachten, dass `DepartureBoardCache.cs` und `DepartureBoardCacheTests.cs` untracked sind.
- `DepartureBoardCache.Project` verwendet `DateTimeOffset.Now` (nicht die `TransitTestClock` der injizierten `freshness`) — entspricht dem Plan („`now` einmalig pro Aufruf"); die Stale-Bewertung läuft deterministisch über `freshness.IsStale`.
- Der `FavoriteHomeViewModel`-Restore-Aufruf nutzt `boards!` — Null-Sicherheit ist durch den Auto-Default im selben `cache is not null`-Block gegeben (plankonform).
