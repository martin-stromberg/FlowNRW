# Umsetzungsplan: Ungenutzten Schreibvorgang aus der Cache-Ablage entfernen

## Übersicht

Der Voll-Ersetzungs-Schreibvorgang `SaveAsync` wird aus dem öffentlichen Vertrag
`IDepartureCacheStore` und seiner einzigen produktiven Implementierung
`JsonDepartureCacheStore` vollständig entfernt. Produktiv wird er nie aufgerufen —
ausschließlich `DepartureCacheStoreTests` und zwei `MemoryDepartureCache`-Test-Fakes
referenzieren ihn. Die bisher über `SaveAsync` charakterisierten Eigenschaften
(Rundreise, Integrität, Validierung) werden über den produktiven
`UpsertAsync`-Pfad bzw. den Lesepfad nachgewiesen.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Öffentlicher Vertrag `IDepartureCacheStore` (Ablage für `DepartureCacheEntry`-Bestände) samt Implementierung `JsonDepartureCacheStore` und allen Implementierern des Interface |
| Auslösende Ereignisse | Kein Laufzeitereignis — API-Vertragsänderung; wirksam ab Compile/Build und Testlauf |
| Erwartete Wirkung | `SaveAsync` ist am Vertrag und an `JsonDepartureCacheStore` nicht mehr erreichbar; `LoadAsync`, `UpsertAsync`, `RemoveAsync`, `RemoveOrphansAsync` bleiben unverändert verfügbar; alle Tests grün |
| Umgang mit fehlenden Objekten | Keine Laufzeitobjekte betroffen; bestehende Cache-Dateien bleiben unverändert lesbar (Dateiformat und Persistenz-Limits sind Nicht-Ziel) |

### Scope-Ausschlüsse

Keine.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `SaveAsync` (Interface + Implementierung) | Vollständige Entfernung statt „verintern" | Die Anforderung erlaubt beide Optionen. Ein `InternalsVisibleTo("FlowNRW.Tests")` existiert im Repo nicht (weder Attribut noch csproj-Eintrag); ein `internal`-Member auf der öffentlichen `JsonDepartureCacheStore` wäre produktiv weiterhin toter Code — genau der Befund B3. Die Tests können denselben Zweck über `UpsertAsync` erfüllen, daher entfällt jeder Grund, den Pfad zu erhalten |
| Test-Seeding für Mehr-Eintrags-Bestände | Seeding über wiederholte `UpsertAsync`-Aufrufe | `UpsertAsync` fährt intern dieselbe Sequenz wie `SaveAsync` (`Validate` → `gate` → `ReadAsync` → `WriteAsync` atomar) — die Integritäts-Charakterisierung (korrupte/übergroße Datei wird nicht überschrieben) bleibt ohne Produktivcode-Änderung prüfbar |
| Duplikat-Identitätsprüfung (`Validate`, Z. 151) | Prüfung bleibt im Code; Nachweis über den Lesepfad mit handgeschriebener Datei | Auf dem Schreibpfad ist eine Duplikat-Verletzung über `UpsertAsync` konstruktionsbedingt nicht erreichbar; der einzig verbleibende fachliche Weg für Duplikate ist eine fremd- oder korrupt geschriebene Datei — `ReadAsync` ruft `Validate` und wirft `InvalidDataException`. Die Zeile bleibt also erreichbar und wird weiter getestet |

## Programmabläufe

### Entfernen des Voll-Ersetzungs-Schreibvorgangs

1. Member `SaveAsync` inklusive XML-Dokumentation aus `IDepartureCacheStore` entfernen (`FlowNRW.Core/Favorites/IDepartureCacheStore.cs`, Z. 13–17).
2. Methode `SaveAsync` aus `JsonDepartureCacheStore` entfernen (`FlowNRW.Core/Favorites/JsonDepartureCacheStore.cs`, Z. 28–39). Die privaten Helfer `ReadAsync`, `WriteAsync`, `Validate`, `AssertSingle`, `ValidateKey`, `WithoutCoordinate`, `ValidateEvents` sowie `SemaphoreSlim gate` bleiben — alle werden weiterhin von `LoadAsync`/`UpsertAsync`/`RemoveAsync`/`RemoveOrphansAsync` genutzt.
3. Member `SaveAsync` aus den beiden Test-Implementierern entfernen: `MemoryDepartureCache` in `FavoriteDepartureCacheIntegrationTests.cs` (Z. 221) und `MemoryDepartureCache` in `ConnectionFavoriteTests.cs` (Z. 134). Danach kompiliert die Solution nur noch, wenn sämtliche `SaveAsync`-Aufrufe in `DepartureCacheStoreTests` umgestellt sind (Schritt „Test-Umbau").

Beteiligte Klassen/Komponenten: `IDepartureCacheStore`, `JsonDepartureCacheStore`, `MemoryDepartureCache` (×2)

### Produktiver Schreibpfad (unverändert, Referenz für die Test-Charakterisierung)

1. `FavoriteMonitorViewModel.RefreshCoreAsync` übergibt bei vollständiger Antwort (`IsComplete`) den `persist`-Delegaten an `FavoriteHomeViewModel.PersistAsync`.
2. `PersistAsync` ruft unter dem `persistence`-Semaphore `cache.UpsertAsync(entry)` auf, nur wenn die Karte noch Favorit ist (`Contains(card)`).
3. `UpsertAsync` validiert den Einzeleintrag (`AssertSingle` → `Validate`), liest unter `gate` den Bestand (`ReadAsync`), ersetzt den Eintrag mit gleichem `DepartureCacheKey`, validiert die Gesamtliste erneut (`Validate`) und schreibt atomar (`WriteAsync` via Temp-File + `File.Move(overwrite: true)`).
4. Favoriten-Entfernung (`MutateAsync` → `cache.RemoveAsync`) und Orphan-Bereinigung (`LoadAsync` → `cache.RemoveOrphansAsync`) bleiben unverändert.

Beteiligte Klassen/Komponenten: `FavoriteMonitorViewModel`, `FavoriteHomeViewModel`, `JsonDepartureCacheStore`, `DepartureCacheEntry`, `DepartureCacheKey`

### Test-Charakterisierung über den produktiven Pfad

1. `RoundTrip_SeparatesProviderKeysAndDropsCoordinates`: zwei `UpsertAsync`-Aufrufe ersetzen `SaveAsync([a, b])`; Assertions auf Reihenfolge werden auf unsortiert-gesicherte Prüfung umgestellt (Upsert hängt an — die bisherige `Assert.Collection`-Reihenfolge „first, second" bleibt bei leerem Startbestand und sequentiellen Upserts erhalten, da `Append` ans Ende schreibt).
2. `InvalidExistingFileCannotBeOverwritten` (Theory `{broken`, `[]`) und `OversizedExistingFileCannotBeOverwritten`: der Schreibversuch wechselt von `SaveAsync([Entry])` zu `UpsertAsync(Entry)`. Da `UpsertAsync` vor dem Schreiben `ReadAsync` ausführt, wirft eine korrupte bzw. übergroße Datei weiter `InvalidDataException` und bleibt unverändert — identisches beobachtbares Verhalten.
3. `Save_InvalidEntriesAreRejected` → umbenannt/umgebaut zu `Upsert_InvalidEntriesAreRejected`: `ErrorCode`-Result und leere `Source` werden über `UpsertAsync` abgelehnt (`AssertSingle` läuft vor Dateizugriff, `Assert.False(File.Exists(path))` bleibt gültig); der Duplikat-Fall entfällt hier und wandert in den neuen Lesepfad-Test.
4. `RemoveOrphans_KeepsOnlyCurrentFavoriteKeys` und `Upsert_ReplacesOnlyMatchingBoard`: Startbestand über zwei `UpsertAsync`-Aufrufe statt `SaveAsync`.
5. Neuer Test: handgeschriebene Datei mit zwei Einträgen gleichen `DepartureCacheKey` → `LoadAsync` wirft `InvalidDataException` (Deckt die Duplikat-Prüfung in `Validate` auf dem einzig verbleibenden Weg).

Beteiligte Klassen/Komponenten: `DepartureCacheStoreTests`, `JsonDepartureCacheStore`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `IDepartureCacheStore` (Interface)

- **Entfernte Methoden:** `SaveAsync(IReadOnlyList<DepartureCacheEntry>, CancellationToken)` — produktiv ungenutzter Voll-Ersetzungs-Schreibvorgang; aus dem öffentlichen Vertrag entfernt (Befund B3).

### `JsonDepartureCacheStore` (Klasse)

- **Entfernte Methoden:** `SaveAsync` — Implementierung des entfernten Interface-Members. Keine weiteren Änderungen: `LoadAsync`, `UpsertAsync`, `RemoveAsync`, `RemoveOrphansAsync` und alle privaten Helfer bleiben unverändert.

### `MemoryDepartureCache` in `FavoriteDepartureCacheIntegrationTests.cs` (Test-Fake)

- **Entfernte Methoden:** `SaveAsync` — nach Entfernen des Interface-Members nicht mehr erforderlich; Seeding der Tests erfolgt bereits über die `Entries`-Eigenschaft.

### `MemoryDepartureCache` in `ConnectionFavoriteTests.cs` (Test-Fake)

- **Entfernte Methoden:** `SaveAsync` — No-op-Implementierung, entfällt mit dem Interface-Member.

### `DepartureCacheStoreTests` (Testklasse)

- **Geänderte Methoden:** siehe Programmablauf „Test-Charakterisierung" — `SaveAsync`-Aufrufe werden durch `UpsertAsync` ersetzt; `Save_InvalidEntriesAreRejected` wird in `Upsert_InvalidEntriesAreRejected` umgebaut (Duplikat-Fall wandert in den neuen Lesepfad-Test).
- **Neue Methoden:** `Load_DuplicateIdentitiesAreRejected` — prüft, dass eine handgeschriebene Datei mit doppeltem `DepartureCacheKey` beim Lesen `InvalidDataException` wirft.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine neuen. Die bestehenden `Validate`-/`ValidateKey`-/`ValidateEvents`-Regeln in `JsonDepartureCacheStore` bleiben unverändert und greifen weiterhin auf dem Lese- und dem Upsert-Schreibpfad.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Gleichnamige Member anderer Verträge:** `IFavoriteStore.SaveAsync`, `IConnectionFavoriteStore.SaveAsync` (`JsonConnectionFavoriteStore`, `ConnectionFavoriteTests` Z. 19), `IRefreshSettingsStore.SaveAsync` und die `SaveAsync`-Dekorateure in `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs` sind **nicht** betroffen — beim Entfernen ist auf den korrekten Interface-Kontext zu achten.
- **Kompilierbruch als Kontrollinstrument:** Bis alle `SaveAsync`-Referenzen entfernt sind, schlägt der Build fehl — gewollt; danach ist die Unerreichbarkeit des Voll-Ersetzungswegs durch den erfolgreichen Build belegt.
- **Duplikat-Validierung:** Die `Duplicate departure cache identity`-Prüfung ist auf dem Schreibpfad nicht mehr erreichbar, bleibt aber für den Lesepfad erforderlich und wird dort getestet — kein Verlust der Zeilenabdeckung.
- **Keine produktiven Seiteneffekte:** `FavoriteHomeViewModel` (`LoadAsync` Z. 77/79, `MutateAsync` Z. 230, `PersistAsync` Z. 317) und `StopMonitorViewModel` (`ReadPersistentFavoriteCacheAsync` Z. 366) nutzen ausschließlich die verbleibenden Member.

## Umsetzungsreihenfolge

1. **Test-Umbau `DepartureCacheStoreTests`**
   - Voraussetzungen: Keine.
   - Beschreibung: Alle `SaveAsync`-Seeding-Aufrufe durch sequentielle `UpsertAsync`-Aufrufe ersetzen; `Save_InvalidEntriesAreRejected` zu `Upsert_InvalidEntriesAreRejected` umbauen (ErrorCode-/Leere-Source-Fälle über `UpsertAsync`); neuen Test `Load_DuplicateIdentitiesAreRejected` mit handgeschriebener Duplikat-Datei ergänzen. (Reihenfolge vor Interface-Änderung nicht zwingend — der Umbau kann auch parallel erfolgen; ohne ihn kompiliert Schritt 2 nicht grün.)

2. **`SaveAsync` aus `IDepartureCacheStore` und `JsonDepartureCacheStore` entfernen**
   - Voraussetzungen: Schritt 1 (sonst Kompilierbruch in den Tests).
   - Beschreibung: Interface-Member inkl. XML-Doku sowie die Implementierungsmethode entfernen. Private Helfer unverändert lassen.

3. **`SaveAsync` aus den Test-Fakes entfernen**
   - Voraussetzungen: Schritt 2.
   - Beschreibung: `SaveAsync`-Member aus `MemoryDepartureCache` in `FavoriteDepartureCacheIntegrationTests.cs` und `ConnectionFavoriteTests.cs` löschen.

4. **Verifikation**
   - Voraussetzungen: Schritte 1–3.
   - Beschreibung: `dotnet format --severity error`, Build mit `TreatWarningsAsErrors` und `dotnet test` ausführen; sicherstellen, dass `rg "SaveAsync"` keine `IDepartureCacheStore`-Bezüge mehr zeigt (verbleibende Treffer nur auf anderen Verträgen).

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `Load_DuplicateIdentitiesAreRejected` | `DepartureCacheStoreTests` | Handgeschriebene JSON-Datei mit zwei Einträgen gleichen `DepartureCacheKey` → `LoadAsync` wirft `InvalidDataException`; Datei bleibt unverändert |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `DepartureCacheStoreTests.RoundTrip_SeparatesProviderKeysAndDropsCoordinates` | Seeding via `UpsertAsync` statt `SaveAsync` |
| `DepartureCacheStoreTests.InvalidExistingFileCannotBeOverwritten` (Theory) | Schreibversuch via `UpsertAsync` statt `SaveAsync` |
| `DepartureCacheStoreTests.OversizedExistingFileCannotBeOverwritten` | Schreibversuch via `UpsertAsync` statt `SaveAsync` |
| `DepartureCacheStoreTests.Save_InvalidEntriesAreRejected` → `Upsert_InvalidEntriesAreRejected` | Umbau auf `UpsertAsync`; Duplikat-Fall wandert in `Load_DuplicateIdentitiesAreRejected` |
| `DepartureCacheStoreTests.RemoveOrphans_KeepsOnlyCurrentFavoriteKeys` | Seeding via `UpsertAsync` statt `SaveAsync` |
| `DepartureCacheStoreTests.Upsert_ReplacesOnlyMatchingBoard` | Seeding via `UpsertAsync` statt `SaveAsync` |
| `MemoryDepartureCache` (`FavoriteDepartureCacheIntegrationTests.cs` Z. 217) | `SaveAsync`-Member entfernen |
| `MemoryDepartureCache` (`ConnectionFavoriteTests.cs` Z. 131) | `SaveAsync`-Member entfernen |

### E2E-Tests (primärer Funktionsnachweis)

Keine E2E-Tests erforderlich: Die Änderung entfernt einen ausschließlich von Tests genutzten API-Member und verändert kein über UI oder Nutzeraktion erreichbares Verhalten. Der produktive Benutzerfluss (Favoriten-Board aus Cache wiederherstellen, per Refresh einzeln persistieren, Favorit entfernen) ist bereits durch die Integrationstests `FavoriteDepartureCacheIntegrationTests` und `ConnectionFavoriteTests` gegen `FavoriteHomeViewModel`/`StopMonitorViewModel` abgedeckt; es gibt keinen neuen oder geänderten Benutzerfluss, den ein E2E-Test zusätzlich beweisen müsste.

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Produktiver Cache-Pfad unverändert (Happy Path) | Testlauf `dotnet test` | `SaveAsync` entfernt; `JsonDepartureCacheStore` implementiert `IDepartureCacheStore` vollständig | Favoriten-Refresh persistiert ein vollständiges Board über `PersistAsync` → `UpsertAsync`; `LoadAsync`/`RemoveOrphansAsync` beim nächsten Laden stellen Bestand wieder her und bereinigen Verwaistes | Build erfolgreich; alle Integrationstests der Restore-/Persist-Pfade grün | `FavoriteDepartureCacheIntegrationTests`, `DepartureCacheStoreTests` (alle grün) |
| 2 | Voll-Ersetzungsweg nicht mehr erreichbar | Build + statische Prüfung | `SaveAsync` aus Interface, Implementierung und Fakes entfernt | `dotnet build` mit `TreatWarningsAsErrors`; `rg "SaveAsync"` über `FlowNRW.Core`/`FlowNRW`/`FlowNRW.Tests` | Jeder hypothetische `SaveAsync`-Aufruf auf `IDepartureCacheStore` wäre ein Kompilierfehler; Build besteht → kein Aufrufer existiert | Build grün; Grep zeigt nur `SaveAsync` fremder Verträge (`IFavoriteStore`, `IRefreshSettingsStore`, `IConnectionFavoriteStore`) |
| 3 | Korrupte/übergroße Datei wird nicht überschrieben | `UpsertAsync` auf defekten Bestand | Cache-Datei mit ungültigem JSON bzw. > 512 KiB | `UpsertAsync` führt `ReadAsync` vor dem Schreiben aus → `InvalidDataException`; Datei unverändert | Fehler wird propagiert, Originaldatei bleibt bytegleich | `InvalidExistingFileCannotBeOverwritten`, `OversizedExistingFileCannotBeOverwritten` (umgebaut auf `UpsertAsync`) |
| 4 | Duplikat-Identitäten in fremdgeschriebener Datei | `LoadAsync` auf handgeschriebene Datei | Datei mit zwei Einträgen gleichen `DepartureCacheKey` | `LoadAsync` → `ReadAsync` → `Validate` erkennt Duplikat | `InvalidDataException`, Datei bleibt unverändert | `Load_DuplicateIdentitiesAreRejected` (neu) |

## Offene Punkte

Keine.
