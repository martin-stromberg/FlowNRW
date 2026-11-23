# Bestandsaufnahme: Restore-/Resume-Pfade (Issues #21–#24, Befunde B1–B4)

Quelle: Explore-Subagent, Beleg durch direkte Dateilektüre. Die Aufgabe ist dokumentiert in
`requirement.md` (+ `todo.md`); offene Frage dort: Fassaden-Ort (`FavoriteHomeViewModel`
vs. dedizierter `DepartureBoardCache`-Service).

## 1. RESTORE-PFAD (B1 / Issue #21)

Es gibt **drei** divergierende Wege, wie ein Board in den Monitor kommt — nicht nur zwei.

### 1.1 Home-Pfad: `FavoriteHomeViewModel.LoadAsync` + `FavoriteMonitorViewModel.Restore`

`FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`, `LoadAsync` (Z. 60–97):

```csharp
var stops = await store.LoadAsync(cancellationToken)...
var unique = stops.DistinctBy(stop => (stop.Source, stop.Id)).ToArray();
if (unique.Length > 100 || unique.Any(...)) throw new InvalidDataException(...);
savedOrder.AddRange(unique.Select(CreateCard));
if (cache is not null)
{
    var keys = savedOrder.Select(card => new DepartureCacheKey(card.Stop.Source, card.Stop.Id)).ToArray();
    var cached = await cache.LoadAsync(cancellationToken)...;
    await cache.RemoveOrphansAsync(keys, cancellationToken)...;
    var byKey = cached.Where(entry => keys.Contains(entry.Key)).ToDictionary(entry => entry.Key);
    var now = DateTimeOffset.Now;
    foreach (var card in savedOrder)
        if (byKey.TryGetValue(new DepartureCacheKey(card.Stop.Source, card.Stop.Id), out var entry))
            card.Restore(entry, now, freshness);
}
```

- Favoritenzugehörigkeit implizit: nur Einträge, deren `DepartureCacheKey` zu einer
  gespeicherten Karte passt, werden angewendet (`byKey` aus `keys`).
- Einmalig: läuft unter `persistence`-Semaphore, `loaded`-Flag (Z. 19, 65, 91) verhindert
  Wiederholung; Fehler → Status-Text, Liste bleibt retrybar.
- Orphan-Bereinigung (`RemoveOrphansAsync`, Z. 79) ist hier verankert.

`FavoriteMonitorViewModel.Restore` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
Z. 89–102, `internal`):

```csharp
if (entry.Key.Source != Stop.Source || entry.Key.Id != Stop.Id) return false;
var storedLines = entry.Lines ?? [];
Lines = SortedLines(storedLines.Count > 0 ? storedLines : entry.Result.Items.Select(LineName));
var retained = entry.Result.Items.Where(item => EffectiveTime(item) is { } time && time >= now)
    .OrderBy(item => EffectiveTime(item)!.Value).ToArray();
if (retained.Length == 0) { RefreshBindings(); return false; }
Result = entry.Result with { Items = retained, IsStale = entry.Result.IsStale || freshness.IsStale(entry.Result) };
LastAttempt = null;
Status = "Letzter Stand wird aktualisiert …";
```

- Stale-Semantik: **markieren-und-anzeigen** — `IsStale` wird am Result gesetzt, Board
  bleibt sichtbar.
- Item-Filter: `EffectiveTime(item) is { } time && time >= now` → zeitlose Items werden
  verworfen.
- Board ohne Zukunfts-Items → `false`, `Result` bleibt `null`, `Lines` wird gesetzt
  (Linieninventar überlebt; Test `LoadAsync_KeepsCacheBoardWithOnlyExpiredDeparturesForItsLineInventory`).

`EffectiveTime` (Z. 184–185): `item.Realtime.ActualTime ?? (PlannedTime + Delay)`.

### 1.2 Monitor-Pfad: `StopMonitorViewModel`

`OpenStopAsync(Stop stop, ProviderResult<StopEvent>? retained = null)` (Z. 220–249):

```csharp
var cached = retained ?? ReadSession(key);
var version = revision;
if (cached is null) cached = await ReadPersistentFavoriteCacheAsync(stop, version);
if (version != revision || SelectedStop != stop) { opening = false; return; }
Result = cached is null ? null : cached with
{
    Items = cached.Items.Where(item => EffectiveTime(item) is { } time && time >= DateTimeOffset.Now)
        .OrderBy(item => EffectiveTime(item)!.Value).ToArray()
};
```

`ReadPersistentFavoriteCacheAsync` (Z. 359–371):

```csharp
if (favoriteStore is null || departureCache is null) return null;
var favorites = await favoriteStore.LoadAsync();                       // kein CancellationToken
if (version != revision || !favorites.Any(item => item.Source == stop.Source && item.Id == stop.Id)) return null;
var entry = (await departureCache.LoadAsync()).FirstOrDefault(item => item.Source == stop.Source && item.StopId == stop.Id);
return version == revision && entry is not null && !freshness.IsStale(entry.Result) ? entry.Result : null;
```

- Favoritenzugehörigkeit explizit, teuer: bei jedem `OpenStopAsync` werden Favoriten-Datei
  und Cache-Datei komplett gelesen, ohne CancellationToken, ohne gemeinsame Sperre mit
  `UpsertAsync`-Schreibern.
- Stale-Semantik: **verwerfen** (`!freshness.IsStale(...)` → `null`) — Gegensatz zum
  Home-Pfad.
- Item-Filter identisch zum Home-Pfad (zeitlose Items verworfen, Z. 234), ABER
  `RefreshCoreAsync` (Z. 291–295) nutzt umgekehrte Semantik:
  `EffectiveTime(item) is not { } time || time >= started` — zeitlose Items bleiben und
  werden nach `DateTimeOffset.MaxValue` sortiert. Zwei Filtersemantiken in einer Klasse.

Dritter Weg — Session-Boards (Z. 23–25, 336–357): In-Memory-LRU (`sessionBoards`, max 100,
`Order`-Zähler). `ReadSession`/`StoreSession`; `OpenFavoriteAsync(home, card)` (Z. 217–218)
übergibt `card.Result` als `retained` und umgeht den Persistenz-Pfad komplett.

Ctor `StopMonitorViewModel` (Z. 37–38):
`IStopSearchService search, IDepartureService departures, IDepartureNavigation navigation, int maxSearchLength, ICurrentLocationService? location, IStopSearchService? nearbySearch, IFavoriteStore? favoriteStore, IDepartureCacheStore? departureCache, RefreshFreshness? freshness`
— Fallback `new RefreshFreshness(new TransitCacheOptions())` (Z. 46), wie
`FavoriteHomeViewModel` Z. 36.

### 1.3 `IDepartureCacheStore` und Aufrufer

`FlowNRW.Core/Favorites/IDepartureCacheStore.cs` — Member:

| Member | Signatur | Produktive Aufrufer |
|---|---|---|
| `LoadAsync` | `Task<IReadOnlyList<DepartureCacheEntry>>` | `FavoriteHomeViewModel` Z. 77; `StopMonitorViewModel` Z. 366 |
| `SaveAsync` | `Task` (Voll-Ersetzung) | **kein produktiver Aufruf** — nur Tests (`DepartureCacheStoreTests.cs` Z. 17, 48, 54, 72, 86–91, 105, 125; Fakes in `FavoriteDepartureCacheIntegrationTests.cs` Z. 221, `ConnectionFavoriteTests.cs` Z. 134). Kein produktiver `SaveAsync`-Aufruf in `FlowNRW/`/`FlowNRW.Core/` |
| `UpsertAsync` | `Task` | `FavoriteHomeViewModel.PersistAsync` Z. 317 |
| `RemoveAsync` | `Task` | `MutateAsync` Z. 230 (Favorit-Entfernung) |
| `RemoveOrphansAsync` | `Task` | `LoadAsync` Z. 79 |

`DepartureCacheKey` (Z. 42): `record (string Source, string Id)`. `DepartureCacheEntry`
(Z. 45–65): `Source`, `StopId`, `Result`, `Lines`, abgeleitetes `Key`.

Implementierung `JsonDepartureCacheStore` (Z. 8–208): Pfad per Ctor; Limits 512 KiB /
100 Entries / 100 Events; `SemaphoreSlim gate`; atomarer Temp-File-Write (`WriteThrough` +
`File.Move(..., overwrite: true)`); Validierung verwirft fehlerhafte/stale Results und
entfernt Stop-Koordinaten (`WithoutCoordinate`, Z. 178–184); Source-Gen-JSON
(`DepartureCacheJsonContext`, `UnmappedMemberHandling.Disallow`).

Schreibpfad: `FavoriteMonitorViewModel.RefreshCoreAsync` persistiert nur bei
`IsComplete(result)` (`ErrorCode is null && Warnings.Count == 0 && !IsFallback &&
!IsStale`, Z. 136–147) über `persist`-Delegaten → `FavoriteHomeViewModel.PersistAsync`
(Z. 311–320) → `cache.UpsertAsync` unter `persistence`-Semaphore, nur wenn `Contains(card)`.

DI: `MauiProgram.cs` Z. 145–154 — Singleton `JsonDepartureCacheStore` unter
`FileSystem.AppDataDirectory/departure-cache.json`; injiziert in `StopMonitorViewModel`
(Z. 105) und `FavoriteHomeViewModel` (Z. 155–157). `RefreshFreshness` Singleton (Z. 109)
mit `RealtimeTimeToLive` (Default 30 s, `MauiProgram` Z. 56).

**Keine Fassade vorhanden** — Freshness-Bewertung, Key-Ableitung, Membership-Prüfung und
Item-Filterung sind in beide ViewModels dupliziert (`EffectiveTime` sogar dreimal inkl.
`HomePage.RenderFavoriteSummary` Z. 269).

## 2. RESUME-GATE (B2 / Issue #22) + Anlass-Triplik (B4 / Issue #24)

Die Regel `IsLoaded && IntervalSeconds != 0` existiert an **fünf** Stellen; nur zwei haben
Overlap-Guard + Loop-Reconcile.

### 2.1 Kompositionen pro Page

| Page | Gate | Overlap-Guard | Refresh-Aufruf | Loop-Reconcile | CancellationToken |
|---|---|---|---|---|---|
| `HomePage.ForegroundChanged` (Z. 141–165) | `active && settings.IsLoaded && IntervalSeconds != 0` (Z. 151) | `resume is not null` + `resume`-CTS, `ReferenceEquals`-Check im finally | `model.RefreshStaleAsync(freshness, source.Token)` | `ReconcileRefreshLoops()` im finally (Z. 162) | ja |
| `DeparturePage.ForegroundChanged` (Z. 178–202) | identisch (Z. 188) | identisch | `model.RefreshIfStaleAsync(freshness, source.Token)` | `ReconcileRefreshLoop()` (Z. 199, 204–208) | ja |
| `JourneyDetailPage.ResumeAsync` (Z. 95–100) | `active && foreground.IsActive && IsLoaded && IntervalSeconds != 0` | kein Guard, kein CTS | `model.Session.RefreshIfStaleAsync(freshness)` | entfällt | nein |
| `ResultsPage.ResumeAsync` (Z. 93–98) | identisch | kein Guard, kein CTS | `model.Session.RefreshIfStaleAsync(freshness)` | entfällt | nein |
| `RefreshLifecycle.RunBackgroundAsync` (Z. 46–67) | `settings.IntervalSeconds == 0` nach `LoadAsync` (Z. 56) | `background is not null` Single-Flight + 20-s-Deadline über `TimeProvider`-CTS | `favorites.RefreshStaleAsync(freshness, source.Token)` | n/a | ja |

Weitere Divergenzen:

- Deaktivierungs-Pfade: Home/Departure canceln `resume` + Loops + `model.CancelPending()`;
  Journey/Results rufen nur `Session.CancelPending()` (Z. 91 bzw. Z. 89).
- `HomePage.OnAppearing` (Z. 102–120) hat eigenen Anlass-Pfad: `settings.LoadAsync` →
  `model.LoadAsync` → `RefreshAtStartupAsync(version)` + `RefreshMissingAsync` über
  Cards-Changed-Trigger (Z. 70) — die B4-Anlass-Triplik (`RefreshMissingAsync` Z. 101,
  `RefreshAtStartupAsync` Z. 105, `RefreshStaleAsync` Z. 154–168 in
  `FavoriteHomeViewModel`).
- `JourneyDetailPage`/`ResultsPage` rufen `ResumeAsync` auch am Ende von `OnAppearing`
  (Z. 70–71 bzw. 67–68) — derselbe Einstieg dient zwei Anlässen.
- `HomePage`-Loops sind pro Karte (`Dictionary<FavoriteMonitorViewModel, RefreshLoop>`,
  Z. 24, 167–185); `DeparturePage` hat einen einzelnen `RefreshLoop` (Z. 17, 40).

### 2.2 `RefreshLifecycle`

`FlowNRW.Core/Refresh/RefreshLifecycle.cs` (68 Zeilen):

- Ctor (Z. 21–22): `ForegroundState foreground`, `RefreshSettingsViewModel settings`,
  `FavoriteHomeViewModel favorites`, `RefreshFreshness freshness`,
  `TimeProvider? clock` (Default `TimeProvider.System`).
- `SetActive(bool)` (Z. 33–41): bricht laufenden Hintergrund-CTS ab +
  `favorites.CancelPending()` vor `foreground.IsActive = active` (Handoff-Reihenfolge
  durch Test abgesichert).
- `RunBackgroundAsync(CancellationToken)` (Z. 46–67): Single-Flight (`background is not
  null`), 20-s-Deadline via `new CancellationTokenSource(TimeSpan.FromSeconds(20), clock)`,
  linked token; Gate `IntervalSeconds == 0` → `favorites.RefreshStaleAsync`.
- **Der Lifecycle kennt aktuell nur `FavoriteHomeViewModel` als Refreshable** —
  `StopMonitorViewModel` und `JourneySearchViewModel` sind nicht registriert. Eine
  zentrale `ResumeForegroundAsync` bräuchte Registrierung/Injektion weiterer Refreshables
  (alle drei sind DI-Singletons: `MauiProgram` Z. 101, 90, 110).
- Singleton (`MauiProgram` Z. 110), injiziert in `App` (`App.xaml.cs` Z. 29). Aufgerufen:
  `SetActive` aus Window-Events (`App.xaml.cs` Z. 57–61); `RunBackgroundAsync` aus
  iOS-`BGTaskScheduler`-Handler (`Platforms/iOS/IosBackgroundRefresh.cs` Z. 52).

### 2.3 Wer ruft Resume auf?

- `foreground.PropertyChanged`-Handler (`ForegroundChanged`) — `ForegroundState`
  (`Refresh/ForegroundState.cs`) wird ausschließlich über `RefreshLifecycle.SetActive` aus
  `App.CreateWindow` gesteuert.
- `OnAppearing` (Journey/Results) bzw. `SettingsChanged` → nur `ReconcileRefreshLoop(s)`.
- „Session" bei Results/Detail = `JourneySearchViewModel` (`ResultsViewModel.Session`,
  `JourneyDetailViewModel.Session` — dünne Projektionen);
  `JourneySearchViewModel.RefreshIfStaleAsync` (Z. 165–202) hat eigene IsBusy-/Revision-
  Guards, aber keinen Schutz gegen überlappende Resume-Aufrufe der Page.

## 3. TESTS

Konventionen: xunit (`[Fact]`/`[Theory]`, xunit 2.9.3), handgeschriebene Fakes, kein
Mock-Framework. Geteilte Test-Doubles: `ControlledDepartureService` (`StopMonitorTests.cs`
Z. 265), `ControlledSearchService` (`PresentationTests.cs` Z. 267), `ControlledLocation`
(`LocationTests.cs` Z. 286), `DepartureTestNavigation` (`StopMonitorTests.cs` Z. 280),
`ControlledRefreshSettingsStore` (`RefreshSettingsTests_Persistence.cs` Z. 121),
`ControlledRefreshDelay` (`RefreshTests.cs` Z. 54), `TransitTestClock`
(`TransitTestSupport.cs` Z. 37). Naming: `Method_Scenario_Expectation`, Dateien
`<Area>Tests_<Aspect>.cs`.

| Typ | Testdatei(en) | Inhalt |
|---|---|---|
| `FavoriteHomeViewModel` + `FavoriteMonitorViewModel` | `FavoriteTests.cs` (12 Tests) | Favoriten-Mutation, Validierung, Identität, Persistenz |
| | `FavoriteDepartureCacheIntegrationTests.cs` (6 Tests) | Restore-Semantik: Board-Restore vor Live-Refresh, Expired-Items verwerfen, Stale markiert statt verworfen (Z. 70–92), Linieninventar ohne Board, Orphan-/Removal-Cleanup, Lines nur aus Complete-Responses |
| | `RefreshMonitorTests_Concurrency.cs`, `ResumeMonitorTests_Cancellation.cs` | Busy-Guards, Loop-Stop, `RefreshIfStaleAsync`-Frische/Cancellation |
| `StopMonitorViewModel` | `StopMonitorTests.cs` (12 Tests) | Open/Refresh/Retain/Notice — kein Test für persistenten Pfad hier |
| | `ConnectionFavoriteTests.cs` Z. 66–106 | `StopSearch_UsesPersistentFavoriteBoardBeforeRefresh` + `StopSearch_DoesNotDisplayStalePersistentBoard` — pinnen die Monitor-Semantik (stale ⇒ nicht anzeigen); bei Harmonisierung anzupassen |
| | `RefreshMonitorTests_Concurrency.cs` Z. 29–44 | Busy-Guard des Monitor-Pfads |
| `RefreshLifecycle` | `RefreshLifecycleTests_Background.cs` (12 Tests + `LifecycleFixture`) | Deadline, Disabled-Gate, Single-Flight, Batching (Chunk 4), SetActive-Handoff |
| `IDepartureCacheStore` | `DepartureCacheStoreTests.cs` (6 Tests) | Roundtrip, Integrität, Validierung, Orphans, Upsert — charakterisiert `SaveAsync`, das produktiv nie läuft (B3) |
| `JourneySearchViewModel` (Resume) | `JourneyResumeTests_Selection.cs` | `RefreshIfStaleAsync`, Selektions-Erhalt |
| `RefreshLoop`/`RefreshFreshness`/Settings | `RefreshTests.cs`, `RefreshLoopTests_Scheduling.cs`, `RefreshFreshnessTests_Boundaries.cs`, `RefreshSettingsTests_Persistence.cs` | Scheduling, Grenzen, Persistenz |

UI-Test-Fixtures (`tests/WindowsJourneyUiTests`, `UiTestLocationServices` in
`UiTestFixtureServices.cs` Z. 176) via `#if UI_TEST_FIXTURES`-Schalter in den Pages.

## 4. KONVENTIONEN

- Kein CommunityToolkit.Mvvm. Eigenes `ObservableObject`
  (`Presentation/ObservableObject.cs`, `INotifyPropertyChanged` +
  `Notify([CallerMemberName])`), eigene `RelayCommand`/`AsyncRelayCommand`.
- ViewModels: `sealed`, Ctor-Injektion mit optionalen Parametern + Defaults
  (`freshness ?? new RefreshFreshness(new TransitCacheOptions())`), `private readonly`
  camelCase-Felder, Batch-`Notify` in `RefreshBindings()`/`Refresh()`.
- Async-/Robustheits-Muster: `IsBusy`-Dup-Guard + `long revision` gegen verspätete
  Antworten + `CancellationTokenSource`; Exceptions → deutsche Status-Strings, keine
  Exception-Details nach außen; Feuer-und-vergiss nur als `_ = …` oder `async void`
  Lifecycle/Event-Handler; `WaitAsync(token)` auf Provider-Aufrufen.
- Persistenz: atomare Temp-File-Writes, `SemaphoreSlim`-Gates, Bounded-Validierung
  (Limits, `InvalidDataException`).
- DI: `Microsoft.Extensions.DependencyInjection` in `MauiProgram.cs` — ViewModels/Stores
  als Singletons (teils Fabrik-Lambdas), Pages transient.
- Regeln: `CONTRIBUTING.md` — Staging-Workflow (PRs gegen `staging`), Conventional Commits,
  `dotnet format --severity error`, `TreatWarningsAsErrors`, CS1591 = Fehler → XML-Doku auf
  allen öffentlichen Membern, keine Stubs, ≥ 70 % Line-Coverage, Logik nach
  `FlowNRW.Core`. Keine `AGENTS.md`/`CLAUDE.md`; `.editorconfig` vorhanden; technische
  Doku zum Refresh in `docs/help/monitorintervalle/ablauf-technisch.md`.

## 5. FASSADEN-OPTION für `RestoreBoardAsync(Stop, RefreshFreshness)`

Was ein gemeinsamer Einstieg leisten muss:

1. Membership-Check gegen `IFavoriteStore` bzw. die geladene `savedOrder`-Liste — einmalig
   statt pro `OpenStopAsync`.
2. Einmaliges/bündiges Lesen der Cache-Datei (heute: Home 1× in `LoadAsync`, Monitor liest
   beide Dateien bei jedem Öffnen, ohne CT, ohne Sperre gegenüber `UpsertAsync`).
3. Eine Stale-Semantik — „markieren-und-anzeigen" (requirement.md legt das fest);
   `ConnectionFavoriteTests` Z. 88–106 pinnen die alte Monitor-Semantik, anzupassen.
4. Eine Item-Filterung — heute drei Varianten. Rückgabetyp mindestens
   `{ ProviderResult<StopEvent> Result (gefiltert, IsStale gesetzt), IReadOnlyList<string> Lines }`
   — `DepartureCacheEntry` hat bereits Result+Lines.
5. Orphan-Bereinigung bleibt Home-`LoadAsync`-Verantwortung.

Bewertung der Orte:

**Dedizierter Service (z. B. `DepartureBoardCache` in `FlowNRW.Core/Favorites`) —
empfohlen:**

- Beide Konsumenten bräuchten nur den Service; `StopMonitorViewModel` würde
  `favoriteStore` + `departureCache` + `freshness` (drei Ctor-Parameter) durch eine
  Abhängigkeit ersetzen.
- `FavoriteHomeViewModel` bleibt Besitzer der Karten, delegiert aber das „Entry →
  anzeigbares Board"-Mapping; der Service könnte den aktuellen Kartenstand (`card.Result`)
  als bevorzugte Quelle liefern und die Datei nur als Fallback lesen.
- Testbar ohne `FavoriteHomeViewModel`-Konstruktion; passt zur Konvention
  „plattformunabhängige Logik nach `FlowNRW.Core`" und zum Store-Muster (Interface +
  Json-Impl + Test-Double).
- Die dreifache `EffectiveTime`-Duplikation fällt als private/statische Hilfe des Service
  zusammen.

**`FavoriteHomeViewModel` als Fassaden-Ort — möglich, aber schwächer:**

- Pro: besitzt bereits `store` + `cache` + `freshness` + `persistence`-Semaphore +
  `IsFavorite`/`savedOrder`; `LoadAsync` läuft einmalig; `OpenFavoriteAsync` nutzt schon
  `card.Result`.
- Contra: Der Monitor müsste die komplette Home-Session kennen (Standort, Nearby,
  Karten-Lifecycle) — die Abhängigkeit existiert heute bewusst nicht. `LoadAsync`-basierte
  Cache-Sicht ist einmalig/starr; die Komposition bliebe halb im VM — der Befund würde nur
  teilweise gelöst.

**Hinweis für B2/B4:** Für `ResumeForegroundAsync` am `RefreshLifecycle` fehlt jede
Registrierungsinfrastruktur — der Lifecycle kennt nur `FavoriteHomeViewModel`.
`StopMonitorViewModel` (Singleton, `MauiProgram` Z. 101) und `JourneySearchViewModel`
(Singleton, Z. 90) müssten injiziert oder über ein `Register`-API angebunden werden; die
Page-seitigen `RefreshLoop`s und das `active`-Flag bleiben Page-Wissen, das ein
„sichtbar/verdeckt"-Signal braucht (Reconcile-Trigger), auch wenn Gate + Overlap-Guard +
Refresh-Aufruf zentralisiert werden.

## Handlungshinweise aus der Bestandsaufnahme

- `SaveAsync`-Entfernung (B3) erfordert Anpassung der sechs
  `DepartureCacheStoreTests`-Methoden und der `IDepartureCacheStore`-Test-Fakes
  (`FavoriteDepartureCacheIntegrationTests` Z. 221, `ConnectionFavoriteTests` Z. 134).
- `ConnectionFavoriteTests` Z. 66–106 pinnen die alte Monitor-Stale-Semantik — bei
  Harmonisierung auf „markieren-und-anzeigen" anzupassen.
- `JourneySearchViewModel`/`StopMonitorViewModel` brauchen eine Anbindung an
  `RefreshLifecycle` für die zentrale Resume-Fassade.
