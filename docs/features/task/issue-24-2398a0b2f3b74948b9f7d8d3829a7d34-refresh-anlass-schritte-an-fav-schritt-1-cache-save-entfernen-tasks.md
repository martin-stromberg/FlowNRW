# Tasks: Ungenutzten Schreibvorgang aus der Cache-Ablage entfernen

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Vertrag | `SaveAsync`-Member inkl. XML-Doku aus `IDepartureCacheStore` entfernen | Erledigt | Build + Grep |
| 2 | Implementierung | `SaveAsync`-Methode aus `JsonDepartureCacheStore` entfernen (private Helfer `Validate`, `AssertSingle`, `ReadAsync`, `WriteAsync`, `gate` unverändert lassen) | Erledigt | Build + `DepartureCacheStoreTests` |
| 3 | Tests | `DepartureCacheStoreTests.RoundTrip_SeparatesProviderKeysAndDropsCoordinates` auf `UpsertAsync`-Seeding umstellen | Erledigt | selbst |
| 4 | Tests | `DepartureCacheStoreTests.InvalidExistingFileCannotBeOverwritten` (Theory) auf `UpsertAsync` als Schreibversuch umstellen | Erledigt | selbst |
| 5 | Tests | `DepartureCacheStoreTests.OversizedExistingFileCannotBeOverwritten` auf `UpsertAsync` als Schreibversuch umstellen | Erledigt | selbst |
| 6 | Tests | `Save_InvalidEntriesAreRejected` zu `Upsert_InvalidEntriesAreRejected` umbauen (ErrorCode- und Leere-Source-Fälle über `UpsertAsync`; Duplikat-Fall entfernen) | Erledigt | `Upsert_InvalidEntriesAreRejected` |
| 7 | Tests | Neuen Test `Load_DuplicateIdentitiesAreRejected` anlegen (handgeschriebene Datei mit doppeltem `DepartureCacheKey` → `LoadAsync` wirft `InvalidDataException`) | Erledigt | `Load_DuplicateIdentitiesAreRejected` |
| 8 | Tests | `RemoveOrphans_KeepsOnlyCurrentFavoriteKeys` auf `UpsertAsync`-Seeding umstellen | Erledigt | selbst |
| 9 | Tests | `Upsert_ReplacesOnlyMatchingBoard` auf `UpsertAsync`-Seeding umstellen | Erledigt | selbst |
| 10 | Tests | `SaveAsync` aus `MemoryDepartureCache` in `FavoriteDepartureCacheIntegrationTests.cs` entfernen | Erledigt | Build |
| 11 | Tests | `SaveAsync` aus `MemoryDepartureCache` in `ConnectionFavoriteTests.cs` entfernen | Erledigt | Build |
| 12 | Verifikation | `dotnet format --severity error`, Build mit `TreatWarningsAsErrors` und `dotnet test` ausführen; `rg "SaveAsync"` auf `IDepartureCacheStore`-Bezüge prüfen | Erledigt | 304/304 grün, format clean, Grep zeigt nur Fremdverträge |
