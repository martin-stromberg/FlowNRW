# Bestandsaufnahme: Schritt 1 – Ungenutzten Schreibvorgang aus der Cache-Ablage entfernen

Kondensierte Sicht aus der Projekt-Bestandsaufnahme
(`docs/projects/task/issue-24-2398a0b2f3b74948b9f7d8d3829a7d34-refresh-anlass-schritte-an-fav/inventory.md`, Abschnitt 1.3 + 3 + 4).

## Schnittstelle `IDepartureCacheStore` (`FlowNRW.Core/Favorites/IDepartureCacheStore.cs`)

| Member | Signatur | Produktive Aufrufer |
|---|---|---|
| `LoadAsync` | `Task<IReadOnlyList<DepartureCacheEntry>>` | `FavoriteHomeViewModel` Z. 77; `StopMonitorViewModel` Z. 366 |
| `SaveAsync` (Z. 17) | `Task` (Voll-Ersetzung) | **keiner** — nur Tests |
| `UpsertAsync` | `Task` | `FavoriteHomeViewModel.PersistAsync` Z. 317 |
| `RemoveAsync` | `Task` | `MutateAsync` Z. 230 |
| `RemoveOrphansAsync` | `Task` | `LoadAsync` Z. 79 |

Grep über `FlowNRW/` und `FlowNRW.Core/`: kein produktiver `SaveAsync`-Aufruf.

## Implementierung `JsonDepartureCacheStore` (`FlowNRW.Core/Favorites/`)

Pfad per Ctor; Limits 512 KiB / 100 Entries / 100 Events; `SemaphoreSlim gate`; atomarer
Temp-File-Write (`WriteThrough` + `File.Move(..., overwrite: true)`); Validierung verwirft
fehlerhafte/stale Results und entfernt Stop-Koordinaten (`WithoutCoordinate`, Z. 178–184);
Source-Gen-JSON (`DepartureCacheJsonContext`, `UnmappedMemberHandling.Disallow`).

## Test-Fundstellen mit `SaveAsync`

- `FlowNRW.Tests/DepartureCacheStoreTests.cs` Z. 17, 48, 54, 72, 86–91, 105, 125 —
  charakterisiert Rundreise/Integrität/Validierung über den ungenutzten Pfad.
- Fake-Implementierungen des Interfaces: `FavoriteDepartureCacheIntegrationTests.cs`
  Z. 221, `ConnectionFavoriteTests.cs` Z. 134 (müssen ggf. angepasst werden, wenn der
  Member entfällt).

## Konventionen

- xunit, handgeschriebene Fakes, Naming `Method_Scenario_Expectation`.
- `CONTRIBUTING.md`: XML-Doku-Pflicht öffentlicher Member, `TreatWarningsAsErrors`,
  `dotnet format --severity error`, keine Stubs, ≥ 70 % Line-Coverage.
- InternalsVisibleTo oder ähnliche Sichtbarkeitstricks: prüfen, ob
  `InternalsVisibleTo(FlowNRW.Tests)` existiert (relevant für die Option „verintern").
