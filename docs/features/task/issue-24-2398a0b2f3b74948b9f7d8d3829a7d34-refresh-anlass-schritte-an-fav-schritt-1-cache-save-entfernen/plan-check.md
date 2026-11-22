# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| Voll-Ersetzungs-Schreibvorgang (`SaveAsync`) am öffentlichen Vertrag nicht mehr erreichbar (entfernt oder verintern) | Vollständige Entfernung aus `IDepartureCacheStore` (Z. 13–17 inkl. XML-Doku) und `JsonDepartureCacheStore` (Z. 28–39); Entscheidung gegen „verintern" im Plan begründet (kein `InternalsVisibleTo` im Repo — verifiziert, kein Treffer außerhalb von docs) | Akzeptanzszenario 2: `dotnet build` mit `TreatWarningsAsErrors` + `rg "SaveAsync"` — jeder Aufruf wäre Kompilierfehler | Abgedeckt |
| Bestehende Tests am Voll-Ersetzungs-Pfad und Prüfdoubles anpassen | Test-Umbau `DepartureCacheStoreTests` (alle 9 `SaveAsync`-Aufrufstellen Z. 17, 48, 54, 72, 86–91, 105, 125 den 6 gelisteten Testfällen zugeordnet — verifiziert) plus Entfernen des Members aus beiden `MemoryDepartureCache`-Fakes (`FavoriteDepartureCacheIntegrationTests.cs` Z. 221, `ConnectionFavoriteTests.cs` Z. 134 — verifiziert; kein Aufruf der Fake-Member vorhanden) | Betroffene-Tests-Tabelle listet alle sechs Testfälle und beide Fakes einzeln | Abgedeckt |
| Über `SaveAsync` charakterisierte Eigenschaften (Rundreise, Integrität, Validierung) über den produktiven Upsert-Pfad abdecken | Seeding via sequentieller `UpsertAsync`-Aufrufe; Schreibversuche auf korrupte/übergroße Datei via `UpsertAsync` (führt ebenfalls `ReadAsync` vor dem Schreiben aus — verifiziert Z. 48); Duplikat-Validierung ist auf dem Schreibpfad konstruktionsbedingt unerreichbar und wird korrekt auf den Lesepfad (`ReadAsync` → `Validate` → `InvalidDataException` Z. 151, verifiziert) verlagert | Umgebaute Tests `RoundTrip_*`, `Invalid/OversizedExistingFileCannotBeOverwritten`, `Upsert_InvalidEntriesAreRejected`; neuer Test `Load_DuplicateIdentitiesAreRejected` | Abgedeckt |
| Einzel-Ändern, Einzel-Entfernen und Orphan-Bereinigung bleiben unverändert verfügbar und getestet | Kein produktiver `SaveAsync`-Aufrufer existiert (verifiziert: `FavoriteHomeViewModel` nutzt nur `LoadAsync` Z. 77, `RemoveOrphansAsync` Z. 79, `RemoveAsync` Z. 230, `UpsertAsync` Z. 317; `StopMonitorViewModel` nur `LoadAsync` Z. 366); verbleibende Member unverändert | `RemoveOrphans_KeepsOnlyCurrentFavoriteKeys`, `Upsert_ReplacesOnlyMatchingBoard` und die Integrations-Tests bleiben bestehen | Abgedeckt |
| Alle Tests grün; `dotnet format`, Build mit `TreatWarningsAsErrors` bestehen | Umsetzungsreihenfolge Schritt 4 (Verifikation) | `dotnet format --severity error`, Build, `dotnet test` | Abgedeckt |
| Nicht-Ziel: kein Umbau des produktiven Schreibpfads | `UpsertAsync` und alle privaten Helfer bleiben unverändert (Plan Programmablauf 2 + Änderungen); verifiziert: alle Helfer (`Validate`, `AssertSingle`, `ReadAsync`, `WriteAsync`, `gate`) werden nach Entfernen weiterhin benutzt — kein toter Code | — | Abgedeckt |
| Nicht-Ziel: keine Änderung an Dateiformat/Persistenz-Limits | Explizit in Scope-Klärung und Änderungen ausgeschlossen | — | Abgedeckt |

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Favoriten-Board aus Cache wiederherstellen / per Refresh persistieren / Favorit entfernen | Keiner geplant | Nicht erforderlich mit Begründung — die Änderung entfernt einen ausschließlich testseitig genutzten API-Member und berührt keinen UI-/Nutzerfluss; der produktive Fluss ist bereits durch `FavoriteDepartureCacheIntegrationTests`/`ConnectionFavoriteTests` abgedeckt. Verifiziert: UI-Test-Fixtures (`tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`) implementieren `IDepartureCacheStore` nicht und referenzieren `SaveAsync` nur auf Fremdverträgen (`IReadOnlyList<Stop>`, `int`) |

## Hinweise

- Alle `SaveAsync`-Fundstellen am `IDepartureCacheStore`-Vertrag wurden per Grep gegen den echten Code verifiziert: Interface, Implementierung, `DepartureCacheStoreTests` (9 Aufrufe) und genau zwei `IDepartureCacheStore`-Implementierer in Tests. Es existieren keine weiteren Implementierer oder Aufrufer — die Test-Umbau-Liste des Plans ist vollständig.
- Seiteneffekt-Analyse im Plan benennt korrekt die gleichnamigen, nicht betroffenen `SaveAsync`-Member anderer Verträge (`IFavoriteStore`, `IConnectionFavoriteStore`, `IRefreshSettingsStore`, `MapTileService`-privat, `RefreshSettingsViewModel`, UI-Test-Dekorateure) — verifiziert, alle gefundenen Treffer außerhalb des Cache-Vertrags gehören diesen an.
- Kleinigkeit ohne Lücken-Charakter: Der Plan-Text zu `RoundTrip_*` ist intern leicht redundant formuliert („unsortiert-gesicherte Prüfung" vs. „Reihenfolge bleibt erhalten"); beide Varianten sind korrekt, da `UpsertAsync` via `Append` anfügt.
