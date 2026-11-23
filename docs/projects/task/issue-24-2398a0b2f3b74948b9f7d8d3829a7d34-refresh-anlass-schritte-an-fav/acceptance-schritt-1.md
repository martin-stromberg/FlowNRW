# Abnahmeprüfung – Entwicklungsschritt 1

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Abweichungen

Keine.

## Hinweise

**Geprüfter Diff** (`git diff task/...-refresh-anlass-schritte-an-fav...task/...-schritt-1-cache-save-entfernen`):
- `FlowNRW.Core/Favorites/IDepartureCacheStore.cs` — `SaveAsync`-Member inkl. XML-Doku entfernt; übrige Member `LoadAsync`, `UpsertAsync`, `RemoveAsync`, `RemoveOrphansAsync` unverändert (Signaturen identisch).
- `FlowNRW.Core/Favorites/JsonDepartureCacheStore.cs` — nur die `SaveAsync`-Methode entfernt; private Helfer `Validate`, `AssertSingle`, `ReadAsync`, `WriteAsync` und `gate` unverändert.
- `FlowNRW.Tests/DepartureCacheStoreTests.cs` — alle bisherigen `SaveAsync`-Seedings auf `UpsertAsync` umgestellt; `Save_InvalidEntriesAreRejected` zu `Upsert_InvalidEntriesAreRejected` umgebaut; neuer Test `Load_DuplicateIdentitiesAreRejected` (Z. 97–116).
- `FlowNRW.Tests/ConnectionFavoriteTests.cs` (Z. 131 ff.) und `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs` (Z. 217 ff.) — `SaveAsync` aus den `MemoryDepartureCache`-Prüfdoubles entfernt.
- Zusätzlich nur die Schritt-Tracking-Datei `docs/features/task/...-schritt-1-cache-save-entfernen-tasks.md` hinzugefügt. Keine weiteren Produktiv- oder Vertragsänderungen.

**Nebenbedingungen:**
- `SaveAsync` auf `IDepartureCacheStore` nicht mehr erreichbar: Grep nach `SaveAsync` im gesamten Repo zeigt nur noch Fremdverträge (`IFavoriteStore`-Fakes mit `SaveAsync(IReadOnlyList<Stop>)` in `ConnectionFavoriteTests.cs` Z. 128, `FavoriteDepartureCacheIntegrationTests.cs` Z. 214, `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs` Z. 269; `IRefreshSettingsStore.SaveAsync(int)` in `RefreshTests.cs`/`RefreshSettingsTests_Persistence.cs`; `RefreshSettingsViewModel.SaveAsync` in `FlowNRW/RefreshSettingsPage.cs` Z. 35) sowie Doku-/Coverage-Artefakte — kein Bezug zum Abfahrts-Cache.
- Produktive Aufrufer nutzen ausschließlich die verbleibenden Zugriffswege: `FavoriteHomeViewModel.cs` Z. 77 (`LoadAsync`), Z. 79 (`RemoveOrphansAsync`), Z. 230 (`RemoveAsync`), Z. 317 (`UpsertAsync`); `StopMonitorViewModel.cs` Z. 366 (`LoadAsync`); DI-Registrierung `MauiProgram.cs` Z. 145. Einzige `IDepartureCacheStore`-Implementierungen: `JsonDepartureCacheStore` plus zwei Test-Fakes — alle ohne `SaveAsync`.
- Eigenschaftsabdeckung über den produktiven Pfad gesichert:
  - Rundreise: `RoundTrip_SeparatesProviderKeysAndDropsCoordinates` (Upsert-Seeding + `LoadAsync`, Koordinaten-Verwerfung weiter geprüft).
  - Integrität: `InvalidExistingFileCannotBeOverwritten` (Theory: malformed + `[]`) und `OversizedExistingFileCannotBeOverwritten` — fehlerhafte/übergroße Datei kann weder gelesen noch via `UpsertAsync` überschrieben werden.
  - Validierung: `Upsert_InvalidEntriesAreRejected` — `ErrorCode`-Ergebnis und leere `Source` werden vor Persistenz abgewiesen.
  - Duplikat-Prüfung: `Load_DuplicateIdentitiesAreRejected` — fremdgeschriebene Datei mit doppeltem `DepartureCacheKey` wird beim Lesen abgewiesen; der `Validate`-Duplikatcheck liegt auf dem Lesepfad, über den alle Schreibvorgänge zwingend laufen (`UpsertAsync` liest vor dem Schreiben; `UpsertAsync` kann konstruktionsbedingt selbst keine Duplikate erzeugen).
  - Einzel-Entfernen und Orphan-Bereinigung: `RemoveOrphans_KeepsOnlyCurrentFavoriteKeys` deckt weiter `RemoveOrphansAsync` und `RemoveAsync` ab; `Upsert_ReplacesOnlyMatchingBoard` deckt das Ersetzen eines Einzeleintrags ab.

**Testnachweis:**
- `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release` → 304/304 bestanden, 0 Fehler, 0 übersprungen.
- `--filter "FullyQualifiedName~DepartureCache"` → 15/15 bestanden.

**Einschränkungen:**
- Verifikation beschränkt auf `FlowNRW.Tests` (Release); das separate UI-Testprojekt `tests/WindowsJourneyUiTests` wurde nicht ausgeführt — es enthält nachweislich kein `IDepartureCacheStore`-Double und keinen Bezug zum entfernten Cache-`SaveAsync`.
- Format-/Warnings-Prüfung (`dotnet format`) nicht selbst ausgeführt; der Release-Build lief fehlerfrei durch (Testprojekt mit `TreatWarningsAsErrors`-Kontext kompiliert).
