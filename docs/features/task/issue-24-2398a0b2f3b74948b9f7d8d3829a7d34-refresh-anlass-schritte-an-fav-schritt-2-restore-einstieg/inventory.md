# Bestandsaufnahme: Schritt 2 – Einheitlicher Restore-Einstieg

Vollständige Detail-Bestandsaufnahme: `docs/projects/task/issue-24-2398a0b2f3b74948b9f7d8d3829a7d34-refresh-anlass-schritte-an-fav/inventory.md` (Abschnitte 1, 3–5). Hier die entscheidungsrelevante Kondensation.

## Die drei Restore-Wege (Ist)

1. **Home-Pfad**: `FavoriteHomeViewModel.LoadAsync` (Z. 60–97) lädt Favoriten einmal
   unter `persistence`-Semaphore (`loaded`-Flag), liest `cache.LoadAsync` einmal, ruft
   `RemoveOrphansAsync`, verteilt Einträge per `DepartureCacheKey`-Dict an
   `card.Restore(entry, now, freshness)`.
   `FavoriteMonitorViewModel.Restore` (Z. 89–102, `internal`): Key-Abgleich, `Lines` aus
   `entry.Lines` oder Result-Items; Filter `EffectiveTime(item) is { } time && time >= now`
   (zeitlose verworfen); bei 0 Rest-Items `return false` aber `Lines` bleibt; sonst
   `Result = entry.Result with { Items = retained, IsStale = entry.Result.IsStale || freshness.IsStale(entry.Result) }`,
   Status „Letzter Stand wird aktualisiert …" → **Stale: anzeigen+markieren**.
2. **Monitor-Pfad**: `StopMonitorViewModel.OpenStopAsync` (Z. 220–249) →
   `ReadPersistentFavoriteCacheAsync` (Z. 359–371): liest `favoriteStore.LoadAsync()` UND
   `departureCache.LoadAsync()` bei jedem Öffnen, **ohne CancellationToken**, ohne
   gemeinsame Sperre; Membership-Check explizit; `freshness.IsStale(entry.Result)` →
   `null` (**Stale: verwerfen**); Filter in `OpenStopAsync` Z. 234 identisch zum Home-Pfad.
3. **Session-Pfad**: `sessionBoards`-LRU (max 100) bzw. `retained`-Parameter via
   `OpenFavoriteAsync` — umgeht Persistenz komplett; bleibt unverändert.

## Divergenzen, die die Fassade lösen muss

- Stale-Semantik: anzeigen+markieren (Home) vs. verwerfen (Monitor) → Ziel: anzeigen+markieren.
- Dateizugriff: einmalig+CT+Semaphore (Home) vs. pro-Öffnen+ohne CT+ohne Sperre (Monitor).
- `EffectiveTime` dreifach dupliziert (`FavoriteMonitorViewModel`, `StopMonitorViewModel`,
  `HomePage.RenderFavoriteSummary` Z. 269 — letztere ist Live-Darstellung, Fassaden-Scope
  prüfen).
- `RefreshCoreAsync` (StopMonitor Z. 291–295) hat umgekehrte Filtersemantik (behält
  zeitlose Items) — ist Live-Refresh-Pfad, **nicht** Teil des Restore-Scopes laut
  Anforderung.

## Typen/Fundstellen

- `DepartureCacheKey` = `record (string Source, string Id)`; `DepartureCacheEntry`
  { Source, StopId, Result, Lines, Key abgeleitet }.
- `RefreshFreshness.IsStale(result)`; `ProviderResult<StopEvent>` mit `Items`,
  `IsStale` (record, `with`-tauglich).
- Ctor `StopMonitorViewModel` (Z. 37–38): `..., IFavoriteStore? favoriteStore, IDepartureCacheStore? departureCache, RefreshFreshness? freshness` —
  drei Parameter könnten durch eine Fassaden-Abhängigkeit ersetzt werden (optional
  beibehalten für Testbarkeit, Defaults vorhanden).
- DI: `MauiProgram` Z. 105 (StopMonitor), Z. 145–154 (JsonDepartureCacheStore),
  Z. 155–157 (FavoriteHomeViewModel), Z. 109 (RefreshFreshness Singleton).
- `FavoriteMonitorViewModel.Restore` ist `internal` — ohne `InternalsVisibleTo` nur über
  die Assembly erreichbar; Tests greifen darauf offenbar zu (FavoriteDepartureCacheIntegrationTests)
  — prüfen wie (ggf. gleiche Assembly oder public API).

## Tests (Ist)

- `FavoriteDepartureCacheIntegrationTests` (6 Tests): pinnt Home-Restore-Semantik
  (stale ⇒ angezeigt+markiert, expired ⇒ verworfen, Linieninventar ohne Board,
  Orphan-/Removal-Cleanup).
- `ConnectionFavoriteTests` Z. 66–106: `StopSearch_UsesPersistentFavoriteBoardBeforeRefresh`
  + `StopSearch_DoesNotDisplayStalePersistentBoard` — pinnt Monitor-Semantik
  (stale ⇒ nicht anzeigen) → bei Harmonisierung anzupassen.
- `StopMonitorTests` (12): kein persistenter-Pfad-Test.
- Test-Doubles: `MemoryDepartureCache` (×2), handgeschriebene Fakes, xunit.

## Schreibpfad (unverändert)

`FavoriteMonitorViewModel.RefreshCoreAsync` persistiert bei `IsComplete` via
`persist`-Delegat → `FavoriteHomeViewModel.PersistAsync` → `cache.UpsertAsync` unter
`persistence`-Semaphore.
