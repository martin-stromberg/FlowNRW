# Abnahme – Schritt 2: Einheitlicher Restore-Einstieg persistierter Favoriten-Boards

- **Schritt-Branch:** `task/issue-24-2398a0b2f3b74948b9f7d8d3829a7d34-refresh-anlass-schritte-an-fav-schritt-2-restore-einstieg`
- **Basisbranch:** `task/issue-24-2398a0b2f3b74948b9f7d8d3829a7d34-refresh-anlass-schritte-an-fav`
- **Anforderung:** Issue #21 (Review-Befund B1 aus Issue #19)
- **Geprüfter Diff:** `git diff <basis>...HEAD` (10 Dateien, +430/−86)
- **Unabhängige Nachprüfung:** `dotnet test FlowNRW.Tests -c Release` → **312/312 bestanden** (eigener Lauf während der Abnahme)

## Geprüfte Punkte

### 1. Gemeinsame Fassade (`DepartureBoardCache` + `RestoredDepartureBoard`) — erfüllt

`FlowNRW.Core/Favorites/DepartureBoardCache.cs` (neu, 107 Zeilen): `public sealed class DepartureBoardCache` mit Ctor `(IDepartureCacheStore, IFavoriteStore, RefreshFreshness?)`, `SemaphoreSlim gate` für gebündelte Lesung, `RestoreBoardAsync` (Monitor-Einzelpfad), `RestoreBoardsAsync` (Home-Bulk-Pfad), interne `static` Hilfen `EffectiveTime`/`LineName`/`SortLines`; `public sealed record RestoredDepartureBoard { Result, Lines }` (Z. 100–107). Beide Konsumenten nutzen ausschließlich die Fassade; die doppelte Dateilese-Komposition (`ReadPersistentFavoriteCacheAsync`, Home-`LoadAsync`-Block mit eigener Filterung) ist entfernt.

### 2. Membership-Prüfung vor Cache-Lesung + Bulk-Variante — erfüllt

- `RestoreBoardAsync` (Z. 31–41): `favorites.LoadAsync` → Identitätsvergleich `(Source, Id)` → erst danach `store.LoadAsync`. Test `RestoreBoardAsync_NonFavoriteReturnsNullWithoutBoard` belegt `cache.Loads == 0` bei Nicht-Favorit.
- `RestoreBoardsAsync` (Z. 46–61): projiziert nur Einträge, deren `Key` in den übergebenen `keys` liegt; die Keys stammen in `FavoriteHomeViewModel.LoadAsync` (Z. 80) aus `savedOrder`, d. h. den gerade aus dem Favoriten-Store geladenen Identitäten — Membership ist per Konstruktion gegeben (Test belegt `favorites.Loads == 0` im Bulk-Pfad, plankonform).
- Monitor-E2E: `StopSearch_PersistentBoardForNonFavoriteIsIgnored` und das native Szenario (Favoriten-Datei `[]` bei vorhandenem Cache-Eintrag → kein `Departure0`).

### 3. Stale-Boards angezeigt und markiert — erfüllt

`Project` (Z. 71–73): `IsStale = entry.Result.IsStale || freshness.IsStale(entry.Result)`; das Board wird zurückgegeben statt verworfen. Monitor zeigt es via `HasDataNotice` (`DataNotice.cs` Z. 14: `IsStale` → `MonitorNotice`). Vorheriges Verwerfen (`!freshness.IsStale` in `ReadPersistentFavoriteCacheAsync`) ist entfernt — Angleichung an das Home-Verhalten. Belegt durch `RestoreBoardAsync_StaleBoardIsReturnedMarked`, `StopSearch_DisplaysStalePersistentBoardMarked` und den `-MonitorStale`-UI-Lauf.

### 4. Zeitlose/abgelaufene Abfahrten verworfen; Linieninventar erhalten — erfüllt

`Project` (Z. 67–70): `EffectiveTime(item) is { } time && time >= now` verwirft zeitlose (`ActualTime`/`PlannedTime`+`Delay` beide leer) und abgelaufene Items, sortiert aufsteigend. `retained.Length == 0` → `Result = null`, `Lines` bleibt (gespeichert oder aus Items abgeleitet). `FavoriteMonitorViewModel.Restore` (Z. 87–96) setzt `Lines` vor der Null-Prüfung. Belegt durch `..._DropsTimelessAndExpiredItems` und `..._OnlyExpiredKeepsLineInventory` (gespeichertes `S1` bzw. abgeleitetes `RE 9` ohne Board).

### 5. `OpenStopAsync`: Abbruch + Refresh-Anstoß — erfüllt

`StopMonitorViewModel.cs` Z. 224–235, 253: `using var source`, `request = source` (kündbar durch `CancelPending`, Z. 330), `catch (OperationCanceledException)` mit `opening`-Freigabe, `catch (Exception)` → `cached` bleibt `null`, `finally { if (ReferenceEquals(request, source)) request = null; }`; `version`-/`SelectedStop`-Guard (Z. 236) unverändert; `if (version == revision) await RefreshAsync()` stößt den Live-Refresh an. `CancellationToken` wird bis in `favorites.LoadAsync`/`store.LoadAsync` durchgereicht (`.WaitAsync(ct)` doppelt abgesichert). Test `RestoreBoardAsync_HonorsCancellation` belegt Abbruch vor jedem Speicherzugriff.

### 6. Scope-Ausschlüsse — eingehalten

- `RefreshCoreAsync`-Live-Filterung unverändert: `is not { } time || time >= started` (Z. 298–299) — zeitlose Live-Items bleiben erhalten, nur Helper-Delegation auf `DepartureBoardCache.EffectiveTime`.
- `HomePage.RenderFavoriteSummary` nicht im Diff — unverändert.
- `RemoveOrphansAsync` weiterhin im Home-Ladevorgang (`FavoriteHomeViewModel.cs` Z. 83).
- `IDepartureCacheStore`-Schreibpfade (`UpsertAsync`/`RemoveAsync`) unangetastet.

### 7. Tests — erfüllt und unabhängig verifiziert

- `FlowNRW.Tests/DepartureBoardCacheTests.cs`: exakt die 7 geforderten Fälle (Nicht-Favorit ohne Cache-Zugriff, fehlender Eintrag, zeitlos/abgelaufen, Stale-Markierung, Linieninventar ohne Board, Keys-Filter/Cancel des Bulk-Pfads, Cancellation vor Speicherzugriff).
- `FlowNRW.Tests/ConnectionFavoriteTests.cs`: `StopSearch_UsesPersistentFavoriteBoardBeforeRefresh` auf Fassade umgestellt; `StopSearch_DisplaysStalePersistentBoardMarked` (Stale-Markiert-Fall mit `TransitTestClock`); `StopSearch_PersistentBoardForNonFavoriteIsIgnored` (Nicht-Favorit-Fall) neu.
- `tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1`: `-MonitorStale`-Schalter mit Stale-Szenario (`RetrievedAt`-Patch, `Departure0`/`MonitorNotice`, Fehler-Retention, Live-Ersatz) und Nicht-Favorit-Szenario; realer Lauf dokumentiert und bestanden (`test-results.md` in Commit `846a020`, alle PASS).
- Eigener Testlauf: 312/312 grün.

### 8. Enabling-Fix — erfüllt, ohne Funktionsänderung

`tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`: explizite Array-Casts `(string[])[]`, `(Address[])[]`, `(Journey[])[]` etc. gegen CsWinRT1032; rein syntaktisch, keine Semantikänderung der Fixture-Daten.

### 9. DI-Verdrahtung — erfüllt

`MauiProgram.cs` Z. 155–156: `DepartureBoardCache`-Singleton mit geteilter `RefreshFreshness`; `StopMonitorViewModel`-Fabrik (Z. 105) und `FavoriteHomeViewModel`-Fabrik (Z. 157–160) auf die Fassade umgestellt. Alle `new StopMonitorViewModel`-Aufrufstellen kompilieren gegen die neue Signatur (optionale Parameter entfernt/ersetzt; nur `ConnectionFavoriteTests` nutzte die Named-Args — angepasst).

## Festgestellte Abweichungen / Lücken

Keine fachlichen Mängel. Beiläufige Beobachtungen ohne Mangelcharakter:

- `DepartureBoardCache.Project` nutzt `DateTimeOffset.Now` (nicht die `TransitTestClock` der injizierten `freshness`); die Stale-Bewertung selbst läuft deterministisch über `freshness.IsStale`. Entspricht dem Plan und dem bisherigen Home-Verhalten.
- Der Monitor-Pfad verwertet nur `RestoredDepartureBoard.Result`; `Lines` werden dort nicht benötigt (Monitor zeigt kein Linieninventar) — kein Funktionsverlust gegenüber dem alten `ReadPersistentFavoriteCacheAsync`.

## Verdict

**vollständig erfüllt**
