# Umsetzungsplan: Einheitlicher Einstieg zum Wiederherstellen persistierter Favoriten-Boards

## Übersicht

Es wird ein dedizierter, plattformunabhängiger Lesedienst `DepartureBoardCache` in `FlowNRW.Core/Favorites` als einzige Cache-Lesefassade für den Geschäftsvorgang „persistiertes Board einer gespeicherten Favoriten-Haltestelle wiederherstellen" eingeführt. Beide bisherigen Eigenkompositionen — `FavoriteHomeViewModel.LoadAsync`→`FavoriteMonitorViewModel.Restore` und `StopMonitorViewModel.ReadPersistentFavoriteCacheAsync` — konsumieren dieselbe Fassade mit derselben Semantik: Stale = anzeigen und markieren, zeitlose Fahrten verwerfen, Linieninventar erhalten, gebündeltes Lesen mit CancellationToken unter einer Sperre. Der Schreibpfad (`UpsertAsync`/`RemoveAsync`/`RemoveOrphansAsync`), der Session-LRU und `OpenFavoriteAsync` bleiben unverändert.

## Scope-Klärung

Verbindliche fachliche Abgrenzung, konkretisiert aus dem Abschnitt „Fachliche Anforderungen" der `requirement.md`:

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Ein `DepartureCacheEntry` (persistiertes Board einer gespeicherten Favoriten-Haltestelle, Schlüssel `DepartureCacheKey(Source, Id)`) |
| Auslösende Ereignisse | (a) Initiales Laden der Favoriten-Übersicht (`FavoriteHomeViewModel.LoadAsync`, einmalig unter `loaded`-Flag); (b) Öffnen eines Haltestellen-Monitors ohne Session-/retained-Board (`StopMonitorViewModel.OpenStopAsync` via `OpenAsync` bzw. `OpenNearbyFromHomeAsync`) |
| Erwartete Wirkung | Einheitlich in beiden Pfaden: persistiertes Board wird sofort angezeigt; nur künftige Fahrten (wirksame Zeit ≥ jetzt); zeitlose Fahrten verworfen; veraltete Boards angezeigt und `IsStale`-markiert; Board ohne künftige Fahrten liefert nur sein Linieninventar; anschließend ersetzt der reguläre Live-Refresh die Anzeige |
| Umgang mit fehlenden Objekten | Monitor-Pfad: `boards` nicht injiziert → kein Restore, Monitor öffnet leer und lädt live (wie heute). Home-Pfad: `cache` nicht gesetzt → Auto-Default liefert `boards == null` → kein Restore; ein explizit injiziertes `boards` bei `cache == null` wird bewusst ignoriert, weil der gesamte persistente Block (inkl. `RemoveOrphansAsync`) am `cache is not null`-Guard hängt. Kein Cache-Eintrag zum Stop → kein Board, kein Fehler. Stop ist kein gespeicherter Favorit → kein Board (Membership-Prüfung). Eintrag ohne künftige Fahrten → `RestoredDepartureBoard` mit `Result == null`, `Lines` befüllt. Verwaiste Cache-Einträge → weiterhin von `FavoriteHomeViewModel.LoadAsync` via `RemoveOrphansAsync` bereinigt (unverändert) |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Kein dateiübergreifender In-Memory-Snapshot in der Fassade: Der Monitor-Pfad liest Favoriten- und Cache-Bestand weiterhin je Öffnen — jetzt gebündelt unter einer Sperre mit CancellationToken. | Die Favoritenzugehörigkeit kann sich zwischen zwei Öffnungen ändern (`FavoriteHomeViewModel.ToggleAsync` vom Monitor aus). Ein Snapshot bräuchte Invalidierung an allen Schreibstellen — die Anforderung verbietet den Umbau des Schreibpfads. Öffnen ist nutzergetaktet; Dateien sind ≤ 512 KiB. Der Befund (divergente Komposition, fehlende Abbruchmöglichkeit, fehlende gemeinsame Sperre, Stale-Verwerfen) ist vollständig gelöst. | bestätigt (vorbehaltlich; in Issue #21 zur Kenntnis/Widerspruch vorgelegt, 2026-11-22) |
| 2 | `HomePage.RenderFavoriteSummary` (FlowNRW/HomePage.cs Z. 269) wird nicht auf die zentrale `EffectiveTime`-Hilfe umgestellt. | Die Zeile nutzt bewusst eine abweichende Formel (`ActualTime ?? PlannedTime`, ohne `Delay`-Zuschlag) für die Live-Summenansicht, liegt im UI-Projekt (keine interne Sichtbarkeit) und ist kein Restore-Pfad; „keine UI-Änderungen" ist Nicht-Ziel-Bedingung. | bestätigt (vorbehaltlich; in Issue #21 zur Kenntnis/Widerspruch vorgelegt, 2026-11-22) |
| 3 | `StopMonitorViewModel.RefreshCoreAsync` behält seine umgekehrte Filtersemantik (zeitlose Items bleiben); nur die `EffectiveTime`-Formel selbst wird gemeinsam genutzt. | Laut Anforderung ist die Live-Refresh-Filterung explizit nicht Teil der Harmonisierung; lediglich die mehrfach duplizierte Zeit-Ermittlung wird zentralisiert. | bestätigt (vorbehaltlich; in Issue #21 zur Kenntnis/Widerspruch vorgelegt, 2026-11-22) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Fassaden-Ort | Dedizierte `sealed class DepartureBoardCache` in `FlowNRW.Core/Favorites` (konkret, kein Interface — Muster wie `RefreshFreshness`) | Bestätigt die Empfehlung der Bestandsaufnahme: `StopMonitorViewModel` kennt `FavoriteHomeViewModel` bewusst nicht; eine Fassade am Home-ViewModel würde den Monitor an die gesamte Home-Session koppeln. Als konkrete Klasse ist der Dienst mit den vorhandenen handgeschriebenen Store-Fakes voll testbar; es gibt keine zweite Implementierung, die ein Interface rechtfertigte. |
| Fassaden-Signatur | Zwei Einstiegsmethoden an einer Fassade: `RestoreBoardAsync(Stop, CancellationToken)` (Monitor) und `RestoreBoardsAsync(IReadOnlyCollection<DepartureCacheKey>, CancellationToken)` (Home); beide teilen die private Projektion `Project` | Das Membership-Wissen der Pfade ist strukturell verschieden: Das Home-ViewModel besitzt mit `savedOrder` bereits die autoritative, einmal geladene Favoritenliste — ein zweites Lesen der Favoriten-Datei durch die Fassade wäre redundant und könnte von `savedOrder` abweichen. Der Monitor hat kein Favoritenwissen und delegiert die Prüfung an die Fassade. Ein einziger Einstiegspunkt (eine Fassade, ein `Project`) statt zweier divergenter Kompositionen. |
| Fassaden-Konstruktor | `DepartureBoardCache(IDepartureCacheStore store, IFavoriteStore favorites, RefreshFreshness? freshness = null)` mit `this.freshness = freshness ?? new RefreshFreshness(new TransitCacheOptions())` | Folgt exakt der Repo-Konvention der beiden ViewModels (`StopMonitorViewModel` Z. 46, `FavoriteHomeViewModel` Z. 36). Dadurch sind beide Nutzungen abgedeckt: die 2-Argument-Testkonstruktion `new DepartureBoardCache(cache, favorites)` (Default-Freshness mit System-Clock, ausreichend für frische Boards) und die Clock-gesteuerte Injektion `new DepartureBoardCache(cache, favorites, freshness)` für deterministische Stale-Tests (`TransitTestClock`). Produktiv injiziert DI explizit den geteilten `RefreshFreshness`-Singleton; der Home-Auto-Default übergibt `this.freshness` des ViewModels (geteilte Clock/TTL). |
| Rückgabetyp | Neuer `sealed record RestoredDepartureBoard { ProviderResult<StopEvent>? Result; IReadOnlyList<string> Lines; }` | `Result` trägt bereits gefilterte Items und die `IsStale`-Markierung; `null` signalisiert „kein anzeigbares Board, aber Linieninventar erhalten" — deckt die Kartenanzeige-Pflicht ab, ohne dem Monitor (der `Lines` nicht nutzt) Ballast aufzuerlegen. |
| Membership- und Sperrmodell | Fassade hält eigenes `SemaphoreSlim gate`; der Monitor-Pfad liest `favorites.LoadAsync(ct)` + `cache.LoadAsync(ct)` unter einer Gate-Haltung | Serialisiert die gebündelte Zwei-Dateien-Lesung als eine Operation und versieht sie mit CT. Gegenseitiger Ausschluss gegenüber `UpsertAsync`-Schreibern ist bereits durch die store-internen `gate`-Semaphores (`JsonDepartureCacheStore` Z. 14, `JsonFavoriteStore` Z. 12) gegeben; die Fassaden-Sperre ordnet die Paar-Lesung konsistent ein. |
| Anbindung `FavoriteMonitorViewModel.Restore` | Signaturwechsel zu `internal bool Restore(RestoredDepartureBoard board)`: Mapping (Filter, Stale-Mark, Lines) liegt komplett in der Fassade; `Restore` übernimmt nur noch Karten-Zustand (`Result`, `Status`, `LastAttempt`, `Lines`, Bindings). Der Key-Abgleich entfällt — die Distribution per `DepartureCacheKey`-Dictionary garantiert die Identität. | `Restore` ist `internal`, wird nur von `FavoriteHomeViewModel.LoadAsync` aufgerufen; kein `InternalsVisibleTo`, keine externen Aufrufer. So wandert die komplette Fachlogik in die Fassade, die Karte bleibt Darstellungszustand. |
| Guard im `LoadAsync`-Restore-Block | Der gesamte persistente Block (Fassaden-Lesung, `RemoveOrphansAsync`, Karten-Verteilung) bleibt unter dem einen bestehenden Guard `cache is not null`; der Fassaden-Aufruf hängt damit am `cache`-Guard, nicht an `boards` | `boards` ist innerhalb des Blocks konstruktionsbedingt garantiert nicht-null (Auto-Default `cache is null ? null : new DepartureBoardCache(...)`). Ein explizit injiziertes `boards` bei `cache == null` wäre eine Fassade über einen dem ViewModel unbekannten Store und wird ignoriert — so bleibt genau ein Schalter für den gesamten persistenten Pfad und `RemoveOrphansAsync` ist stets konsistent zum gelesenen Bestand. |
| Konstruktor-Verträge der ViewModels | `StopMonitorViewModel`: die drei optionalen Parameter `favoriteStore`, `departureCache`, `freshness` werden durch `DepartureBoardCache? boards` ersetzt. `FavoriteHomeViewModel`: erhält zusätzlich `DepartureBoardCache? boards` mit Auto-Default `boards ?? (cache is null ? null : new DepartureBoardCache(cache, store, this.freshness))` | Monitor: drei Roh-Abhängigkeiten kollabieren zu einer Fassade — nur für den persistenten Lesepfad genutzt. Home: `cache` bleibt (Schreibpfad `PersistAsync`, `RemoveAsync`, `RemoveOrphansAsync`), `freshness` bleibt (`RefreshMissingAsync`); der Auto-Default folgt der Repo-Konvention (`freshness ?? new RefreshFreshness(...)`) und hält alle bestehenden Testkonstruktionen ohne Änderung kompilier- und laufzeitfähig. DI injiziert produktiv den geteilten Singleton. |
| `EffectiveTime`-Zentralisierung | `internal static` Hilfen `EffectiveTime(StopEvent)`, `LineName(StopEvent)` und `SortLines(IEnumerable<string>)` an `DepartureBoardCache`; private Duplikate in `FavoriteMonitorViewModel` (Z. 157, 165–169, 184–185) und `StopMonitorViewModel` (Z. 333–334) entfallen | Alle drei Helfer sind wörtlich dupliziert; die Restore-relevanten und die Live-Aufrufstellen nutzen danach dieselbe Definition. `StopMonitorViewModel.RefreshCoreAsync` ändert nur die Auflösung der Hilfe, nicht das Filterprädikat (Scope-Ausschluss 3). |
| Cancellation im Monitor-Pfad | `OpenStopAsync` setzt für den Fassaden-Aufruf `request = source` (`using var source`), reicht `source.Token` durch und räumt das Feld in einem `finally` per `if (ReferenceEquals(request, source)) request = null;` — Scope der `request`-Belegung ist nur der Fassaden-Aufruf; `CancelPending` bricht den Lesevorgang mit ab; der `version`-Revision-Guard bleibt | `ReferenceEquals`-Guard statt Versionsvergleich, weil ein späterer `RefreshCoreAsync` `request` bereits mit einer eigenen CTS überschrieben haben kann — genullt wird nur die eigene, zu disponierende Instanz. Das Muster ist etabliert (`DepartureService`/`RoutingService`/`StopSearchService`, jeweils `finally { if (ReferenceEquals(current, request)) current = null; }`). Ohne das Räumen bliebe nach Methodenende (Early-Return, `ShowMonitorAsync`-Ausnahme) eine disposed CTS in `request`, und `CancelPending` → `request?.Cancel()` würde `ObjectDisposedException` werfen. Beseitigt zugleich den Befund „ohne Abbruchmöglichkeit", ohne die öffentlichen Open-Signaturen zu verändern. |

## Programmabläufe

### Home-Restore beim initialen Laden

1. `FavoriteHomeViewModel.LoadAsync` betritt den `persistence`-Semaphore, lädt `store.LoadAsync`, dedupliziert, validiert (≤ 100, nicht-leere Identitäten) und erzeugt Karten via `CreateCard` (unverändert).
2. Innerhalb des bestehenden `cache is not null`-Blocks (einzige Guard-Bedingung für den gesamten persistenten Block; `boards` ist dort per Auto-Default garantiert gesetzt): `boards.RestoreBoardsAsync(keys, cancellationToken)` mit den `DepartureCacheKey`s der `savedOrder`-Karten — `keys` sind die autoritative Membership-Liste.
3. Die Fassade hält ihr `gate`, liest `cache.LoadAsync(cancellationToken)` einmal, projiziert jeden Eintrag, dessen `Key` in `keys` liegt, über `Project` und liefert `IReadOnlyDictionary<DepartureCacheKey, RestoredDepartureBoard>` — Einträge, deren `Key` nicht in `keys` liegt, werden nicht projiziert und sind im Ergebnis nicht enthalten.
4. `LoadAsync` ruft anschließend `cache.RemoveOrphansAsync(keys, cancellationToken)` im selben `cache is not null`-Block auf — Orphan-Bereinigung bleibt hier verankert (unveränderte Verantwortung, gleicher Bestand wie die gelesene Liste).
5. Pro Karte: `restored.TryGetValue(key, out board)` → `card.Restore(board)`.
6. `card.Restore` setzt `Lines = board.Lines`; bei `board.Result is null` → `RefreshBindings`, `return false`; sonst `Result = board.Result`, `LastAttempt = null`, `Status = "Letzter Stand wird aktualisiert …"`, `return true` — Stale ist bereits am Result markiert.
7. Fehler der Fassade (außer `OperationCanceledException`) werden vom bestehenden inneren `catch (Exception)` in `LoadAsync` geschluckt — Ladeverhalten unverändert.

Beteiligte Klassen/Komponenten: `FavoriteHomeViewModel`, `DepartureBoardCache`, `FavoriteMonitorViewModel`, `IDepartureCacheStore`, `RestoredDepartureBoard`.

### Monitor-Restore beim Öffnen

1. `OpenStopAsync(stop)` (via `OpenAsync`/`OpenNearbyFromHomeAsync`) setzt `opening`, `SelectedStop`, versucht `retained ?? ReadSession(key)` — unverändert (Session-Pfad bleibt).
2. Ist `cached is null` und `boards` gesetzt: `using var source = new CancellationTokenSource(); request = source;` dann `await boards.RestoreBoardAsync(stop, source.Token)`; `cached = board?.Result`. Der Fassaden-Aufruf steht in `try/catch/finally`: `catch (OperationCanceledException)` → Öffnen abbrechen (`opening = false`, `return`); `catch (Exception)` → `cached` bleibt `null`, Öffnen läuft weiter (bisherige Resilienz); `finally { if (ReferenceEquals(request, source)) request = null; }` — `request` wird auf jedem Ausstiegspfad geräumt, bevor `using` die CTS disposiert, sodass `CancelPending` nie auf eine disposed CTS trifft.
3. Die Fassade hält ihr `gate`, liest `favorites.LoadAsync(token)`, prüft `favorites.Any(item => item.Source == stop.Source && item.Id == stop.Id)` — nicht Mitglied → `null`; sonst liest sie `cache.LoadAsync(token)`, sucht den Eintrag per `entry.Source == stop.Source && entry.StopId == stop.Id`, projiziert ihn via `Project` und liefert `RestoredDepartureBoard` (oder `null` ohne Eintrag).
4. `version != revision || SelectedStop != stop` → Abbruch wie bisher.
5. `Result = cached with { Items = … }` — der bestehende einheitliche Filter bleibt für `retained`- und Session-Boards bestehen und nutzt nun `DepartureBoardCache.EffectiveTime`; auf das bereits gefilterte Fassaden-Ergebnis wirkt er idempotent.
6. Status „Abfahrten werden geladen …", Navigation, anschließend `RefreshAsync` — unverändert. Ein stale-markiertes Board wird bis zum Live-Ergebnis angezeigt und trägt `IsStale` (Notice-Indikator analog zur Kartenanzeige).

Beteiligte Klassen/Komponenten: `StopMonitorViewModel`, `DepartureBoardCache`, `IFavoriteStore`, `IDepartureCacheStore`, `RefreshFreshness`, `RestoredDepartureBoard`.

### Projektion `Project` (Fassade-intern, einzige Semantik-Quelle)

1. `now` einmalig pro Aufruf bestimmen.
2. `Lines`: `entry.Lines` wenn nicht leer, sonst aus `entry.Result.Items.Select(LineName)`; `SortLines` (Whitespace raus, Ordinal-distinct, sortiert).
3. `retained`: `entry.Result.Items` mit `EffectiveTime(item) is { } time && time >= now`, sortiert nach `EffectiveTime` — zeitlose Fahrten verworfen.
4. `retained.Length == 0` → `RestoredDepartureBoard { Result = null, Lines = lines }`.
5. Sonst `Result = entry.Result with { Items = retained, IsStale = entry.Result.IsStale || freshness.IsStale(entry.Result) }` — markieren-und-anzeigen.

Beteiligte Klassen/Komponenten: `DepartureBoardCache`, `DepartureCacheEntry`, `ProviderResult<StopEvent>`, `RefreshFreshness`.

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `DepartureBoardCache` | `public sealed class` in `FlowNRW.Core/Favorites/DepartureBoardCache.cs` | Einzige Cache-Lesefassade: Ctor `(IDepartureCacheStore, IFavoriteStore, RefreshFreshness? freshness = null)` mit Default `new RefreshFreshness(new TransitCacheOptions())`; Membership-Prüfung, gebündeltes Lesen unter `gate` mit CT, Freshness-Bewertung, einheitliche Item-Filterung und Stale-Markierung; interne `static` Hilfen `EffectiveTime`, `LineName`, `SortLines`; Methoden `RestoreBoardAsync` und `RestoreBoardsAsync`. XML-Doku auf allen öffentlichen Membern (CS1591). |
| `RestoredDepartureBoard` | `public sealed record` (in `DepartureBoardCache.cs`) | Projiziertes Restore-Ergebnis: `Result` (gefiltert, `IsStale`-markiert, `null` bei keinem anzeigbaren Board) und `Lines` (Linieninventar). XML-Doku auf allen öffentlichen Membern. |

## Änderungen an bestehenden Klassen

### `FavoriteHomeViewModel` (`FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`)

- **Neue Felder:** `private readonly DepartureBoardCache? boards` — geteilte Cache-Lesefassade.
- **Geänderte Methoden:**
  - Ctor: neuer optionaler Parameter `DepartureBoardCache? boards = null`; Feldzuweisung `this.boards = boards ?? (cache is null ? null : new DepartureBoardCache(cache, store, this.freshness))` — `cache`, `freshness` bleiben (Schreibpfad, `RefreshMissingAsync`); der Auto-Default teilt dem instanzeigenen Fassaden-Ersatz die VM-`freshness` (Clock/TTL).
  - `LoadAsync`: der Block Z. 76–86 (`cache.LoadAsync` + `byKey`-Dict + `card.Restore(entry, now, freshness)`) wird durch `boards.RestoreBoardsAsync(keys, cancellationToken)` und `card.Restore(board)` ersetzt — der Aufruf bleibt innerhalb des bestehenden `cache is not null`-Blocks; dort ist `boards` per Auto-Default garantiert nicht-null. `cache.RemoveOrphansAsync` bleibt im selben `cache is not null`-Block erhalten.

### `FavoriteMonitorViewModel` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`)

- **Geänderte Methoden:** `Restore` — neue Signatur `internal bool Restore(RestoredDepartureBoard board)`; Mapping-Logik (Key-Abgleich, Filter, `IsStale`-Berechnung, Lines-Ableitung) entfällt zugunsten der Fassade; Karten-Zustand (`Result`, `LastAttempt`, `Status`, `Lines`, `RefreshBindings`) unverändert.
- **Entfernte Member:** private `EffectiveTime`, `LineName`, `SortedLines` — Aufrufstellen in `Restore`-Umfeld und `RefreshCoreAsync`/`EnrichLines` zeigen auf die internen `static` Hilfen der Fassade (identische Formeln, kein Verhaltensdelta).

### `StopMonitorViewModel` (`FlowNRW.Core/Presentation/StopMonitorViewModel.cs`)

- **Entfernte Felder:** `favoriteStore`, `departureCache`, `freshness` — nur vom persistenten Lesepfad genutzt.
- **Neue Felder:** `private readonly DepartureBoardCache? boards`.
- **Geänderte Methoden:**
  - Ctor: `IFavoriteStore? favoriteStore, IDepartureCacheStore? departureCache, RefreshFreshness? freshness` → `DepartureBoardCache? boards = null` (XML-Doku-Parameter anpassen).
  - `OpenStopAsync`: `ReadPersistentFavoriteCacheAsync`-Aufruf → `boards.RestoreBoardAsync(stop, source.Token)` mit `request`-gebundener CTS und `try/catch/finally` (OCE → Öffnen abbrechen; andere Fehler → `cached` bleibt `null`; `finally { if (ReferenceEquals(request, source)) request = null; }` — verhindert disposed CTS in `request` auf allen Ausstiegspfaden, Muster aus `DepartureService`/`RoutingService`/`StopSearchService`); Result-Zuweisung und `version`-Guard unverändert.
  - `RefreshCoreAsync`: Aufrufstellen der gelöschten privaten `EffectiveTime` zeigen auf `DepartureBoardCache.EffectiveTime` — Filterprädikat unverändert (Scope-Ausschluss 3); das dortige `using var source`-/`request`-Muster bleibt unverändert (seine `finally`-Räumung unter `version == revision` deckt die Dispose-Konstellation bereits ab, weil bei `version != revision` `request` nicht mehr auf die eigene CTS zeigt).
- **Entfernte Member:** `ReadPersistentFavoriteCacheAsync` (Z. 359–371), private `EffectiveTime` (Z. 333–334).

### `MauiProgram` (`FlowNRW/MauiProgram.cs`)

- **Neue Registrierung:** `builder.Services.AddSingleton(services => new DepartureBoardCache(services.GetRequiredService<IDepartureCacheStore>(), services.GetRequiredService<IFavoriteStore>(), services.GetRequiredService<RefreshFreshness>()))` — explizite 3-Argument-Konstruktion, damit der geteilte `RefreshFreshness`-Singleton (Clock/TTL) gilt.
- **Geänderte Fabriken:** `StopMonitorViewModel`-Singleton (Z. 101–105): die letzten drei Argumente `IFavoriteStore`, `IDepartureCacheStore`, `RefreshFreshness` → `services.GetRequiredService<DepartureBoardCache>()`; `FavoriteHomeViewModel`-Singleton (Z. 155–157): `DepartureBoardCache` als zusätzliches Argument.

## Datenbankmigrationen

Keine. (Persistenz erfolgt über die unveränderten JSON-Datei-Stores; das `DepartureCacheEntry`-Schema ändert sich nicht.)

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `Stop` beim Monitor-Restore | Membership greift nur bei exakter `(Source, Id)`-Übereinstimmung; leere/unvollständige Identität schlägt fehl | Kein Board — Verhalten wie bisher, keine neue Ausnahme |
| Fassaden-Ergebnis | Keine neue Wertebereichsvalidierung — Identitäts- und Inhaltsvalidierung bleibt bei `JsonFavoriteStore.Validate` bzw. `JsonDepartureCacheStore.Validate`/`ValidateKey` auf dem Lese-/Schreibpfad | — |

## Konfigurationsänderungen

Keine. Die neue DI-Singleton-Registrierung nutzt die vorhandenen Singletons `IDepartureCacheStore`, `IFavoriteStore`, `RefreshFreshness` — keine `appsettings`-/Options-Einträge.

## Seiteneffekte und Risiken

- **Monitor-Anzeige bei veraltetem Board (gewollt):** `StopMonitorViewModel` zeigt ein persistiertes veraltetes Board nun markiert (`IsStale` → `HasDataNotice`/`MonitorNotice` auf `DeparturePage`) statt es zu verwerfen — sichtbare Verhaltensänderung, fachlich gefordert; der unmittelbar folgende `RefreshAsync` ersetzt das Board.
- **Disposed CTS in `request` (mitigiert):** `request = source` mit `using var source` in `OpenStopAsync` würde ohne explizites Räumen auf Ausstiegspfaden ohne nachfolgendes `RefreshAsync` (Early-Return, `ShowMonitorAsync`-Ausnahme) eine disposed CTS hinterlassen — `CancelPending` → `request?.Cancel()` wäre `ObjectDisposedException`-gefährdet. Der geplante `finally`-Guard `if (ReferenceEquals(request, source)) request = null;` räumt das Feld, bevor `using` disposiert (Muster `DepartureService`/`RoutingService`/`StopSearchService`).
- **`FavoriteMonitorViewModel.Restore`-Signatur:** `internal`, nur `FavoriteHomeViewModel.LoadAsync` ruft sie; kein `InternalsVisibleTo`, keine externen Aufrufer — sicher änderbar.
- **`StopMonitorViewModel`-Ctor:** öffentliche Signatur ändert sich; betroffen sind nur `MauiProgram` (Fabrik-Lambda) und zwei Testkonstruktionen in `ConnectionFavoriteTests` — alle übrigen Testkonstruktionen nutzen nur Positionsparameter vor den optionalen Parametern.
- **Auto-Default in `FavoriteHomeViewModel`:** Tests mit `cache:`-Argument erhalten automatisch eine instanzinterne Fassade über VM-`store`/`cache`/`freshness` — produktiv wird der geteilte Singleton injiziert; Verhalten identisch.
- **`RefreshCoreAsync`-Aufrufstellen:** nur Hilfs-Auflösung wechselt; versehentliche Prädikat-Änderung ist das Hauptrisiko und wird durch unveränderte Tests (`StopMonitorTests`, `RefreshMonitorTests_Concurrency`) abgesichert.
- **Doppelfilterung im Monitor:** Fassaden-Ergebnis passiert den Open-Filter erneut — idempotent, gewollt, weil `retained`/Session-Quellen denselben Filter brauchen.

## Umsetzungsreihenfolge

1. **`RestoredDepartureBoard` + `DepartureBoardCache` anlegen** (`FlowNRW.Core/Favorites/DepartureBoardCache.cs`)
   - Voraussetzungen: Keine (baut auf vorhandenen `IDepartureCacheStore`, `IFavoriteStore`, `RefreshFreshness`, `TransitCacheOptions`, `DepartureCacheEntry`, `DepartureCacheKey` auf).
   - Beschreibung: Klasse mit Ctor `(IDepartureCacheStore store, IFavoriteStore favorites, RefreshFreshness? freshness = null)` und `this.freshness = freshness ?? new RefreshFreshness(new TransitCacheOptions())`, `SemaphoreSlim gate`, `RestoreBoardAsync` (Membership → gebündelte Lesung → `Project`), `RestoreBoardsAsync` (gebündelte Lesung → Keys-Filter → `Project`), private `Project`, interne `static` Hilfen `EffectiveTime`, `LineName`, `SortLines`; XML-Doku überall.

2. **`FavoriteMonitorViewModel.Restore` umstellen**
   - Voraussetzungen: Schritt 1 (Record + interne Hilfen).
   - Beschreibung: Signatur auf `Restore(RestoredDepartureBoard)` ändern; private `EffectiveTime`/`LineName`/`SortedLines` entfernen, Aufrufstellen auf Fassaden-Hilfen umleiten.

3. **`FavoriteHomeViewModel` anbinden**
   - Voraussetzungen: Schritte 1–2.
   - Beschreibung: Ctor-Parameter `boards` + Auto-Default (`cache is null ? null : new DepartureBoardCache(cache, store, this.freshness)`); `LoadAsync`-Restore-Block innerhalb des `cache is not null`-Guards auf `RestoreBoardsAsync` + `card.Restore(board)` umstellen; `RemoveOrphansAsync` im selben Block unverändert belassen.

4. **`StopMonitorViewModel` umbauen**
   - Voraussetzungen: Schritt 1.
   - Beschreibung: Ctor/Felder auf `boards`; `ReadPersistentFavoriteCacheAsync` entfernen; `OpenStopAsync` mit `request`-CTS, Fassaden-Aufruf, Fehlerbehandlung und `ReferenceEquals`-`finally`-Räumung; private `EffectiveTime` entfernen und Aufrufstellen umleiten.

5. **DI-Verdrahtung in `MauiProgram`**
   - Voraussetzungen: Schritte 1, 3, 4.
   - Beschreibung: `DepartureBoardCache`-Singleton registrieren (explizite 3-Argument-Konstruktion mit geteilter `RefreshFreshness`); beide ViewModel-Fabriken aktualisieren.

6. **Tests anpassen und neue Fassaden-Tests schreiben**
   - Voraussetzungen: Schritte 1–5.
   - Beschreibung: `ConnectionFavoriteTests` (Ctor-Argumente + Stale-Test-Semantik), neue `DepartureBoardCacheTests.cs`; bestehende `FavoriteDepartureCacheIntegrationTests` müssen unverändert grün bleiben.

7. **UI-Test-Skript erweitern**
   - Voraussetzungen: Schritte 1–6.
   - Beschreibung: `tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1` um einen Monitor-Pfad-Stale-Nachweis ergänzen (siehe E2E-Abschnitt).

8. **Verifikation**
   - Voraussetzungen: Schritte 1–7.
   - Beschreibung: `dotnet format --severity error`, Build (`TreatWarningsAsErrors`), `dotnet test` (≥ 70 % Line-Coverage), UI-Testlauf auf Windows.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `RestoreBoardAsync_NonFavoriteReturnsNullWithoutBoard` | `DepartureBoardCacheTests` (neu, `FlowNRW.Tests`) | Stop steht nicht in `IFavoriteStore` → `null`, kein Board trotz vorhandenem Cache-Eintrag (Membership-Prüfung) |
| `RestoreBoardAsync_FavoriteWithoutEntryReturnsNull` | `DepartureBoardCacheTests` | Mitglied, aber kein Cache-Eintrag → `null` |
| `RestoreBoardAsync_DropsTimelessAndExpiredItems` | `DepartureBoardCacheTests` | Zeitlose + vergangene Items verworfen, künftige sortiert zurückgegeben |
| `RestoreBoardAsync_StaleBoardIsReturnedMarked` | `DepartureBoardCacheTests` | Fassade mit Clock-gesteuerter Freshness: `new DepartureBoardCache(cache, favorites, new RefreshFreshness(new TransitCacheOptions { RealtimeTimeToLive = … }, new TransitTestClock { … }))`; `freshness.IsStale` bzw. `entry.Result.IsStale` → Board geliefert, `Result.IsStale == true` (einheitliche Stale-Semantik, deterministisch) |
| `RestoreBoardAsync_OnlyExpiredKeepsLineInventory` | `DepartureBoardCacheTests` | `Result == null`, `Lines` aus `entry.Lines` bzw. aus Items abgeleitet |
| `RestoreBoardsAsync_ProjectsOnlyGivenKeys` | `DepartureBoardCacheTests` | Erwartung eindeutig: das Ergebnis-Dictionary enthält ausschließlich Projektionen der übergebenen Membership-Keys; Cache-Einträge, deren `Key` nicht in `keys` liegt (verwaiste Einträge), werden nicht projiziert und sind im Ergebnis nicht enthalten |
| `RestoreBoardAsync_HonorsCancellation` | `DepartureBoardCacheTests` | Vorkündigtes `CancellationToken` → `OperationCanceledException` propagiert |
| `RecordingFavorites`/`RecordingDepartureCache` | `DepartureBoardCacheTests` | Handgeschriebene Fakes mit Aufrufzählern (Konvention), um gebündelte Lesung/Lesereihenfolge nachzuweisen |
| `StopSearch_PersistentBoardForNonFavoriteIsIgnored` | `ConnectionFavoriteTests` | Gleiche Konstellation mit Cache-Eintrag, aber Stop nicht in `MemoryFavorites` → `Result == null` vor Live-Refresh |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `ConnectionFavoriteTests.StopSearch_UsesPersistentFavoriteBoardBeforeRefresh` (Z. 66–84) | Ctor-Parameter `favoriteStore`/`departureCache` entfallen → `boards: new DepartureBoardCache(cache, favorites)` (2-Argument-Form, Default-Freshness reicht — das Fixture-Board ist frisch); Assertions fachlich unverändert |
| `ConnectionFavoriteTests.StopSearch_DoesNotDisplayStalePersistentBoard` (Z. 86–106) | Alte Monitor-Semantik (verwerfen) wird zur einheitlichen Semantik: umbenennen (z. B. `StopSearch_DisplaysStalePersistentBoardMarked`); Konstruktion `boards: new DepartureBoardCache(cache, favorites, freshness)` mit der bestehenden deterministischen Freshness (`TransitTestClock`, TTL 1 s — das dritte Argument ist hier fachlich erforderlich); Assert: `Result?.Source == "stale"`, `Result.IsStale`, danach Live-Ersetzung — gespiegelt zu `LoadAsync_RestoresStaleBoardWithFutureDepartureAsMarkedStale` |
| `MemoryDepartureCache`/`MemoryFavorites`-Fakes in `ConnectionFavoriteTests` | Werden in `DepartureBoardCache` verpackt statt direkt injiziert; Schnittstellen unverändert |

Die sechs `FavoriteDepartureCacheIntegrationTests` bleiben unverändert und müssen grün laufen — sie sind der Regressionsschirm der Home-Semantik über den Auto-Default der Fassade.

### E2E-Tests (primärer Funktionsnachweis)

Der Monitor-Pfad ist eine sichtbare Benutzeraktion (Haltestelle öffnen); die veränderte Stale-Anzeige ist anwendersichtbar, deshalb E2E-Pflicht. Infrastruktur vorhanden: `tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1` mit `FLOWNRW_UI_TEST_*`-Fixture-Pfaden; die Test-App erzeugt die `departure-cache.json` selbst (Seed-Lauf via `favorite-cache-seed`), das Skript kann `RetrievedAt` auf einen alten Zeitstempel patchen (`(Get-Content … -Raw) -replace '"RetrievedAt":"[^"]+"'`), bevor die App neu gestartet wird.

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Pflicht | Persistiertes veraltetes Favoriten-Board wird beim Öffnen über die Stopsuche angezeigt (nicht verworfen) und danach durch Live-Daten ersetzt | `WindowsDepartureCacheUiTests.ps1` (neuer Schalter, z. B. `-MonitorStale`) | Einheitliche Stale-Semantik „anzeigen und markieren" auf dem Monitor-Pfad: `Departure0` zeigt gecachte `RE 1`, `MonitorNotice`-Indikator sichtbar, nach Refresh `Live Stand` | Der geänderte Nutzerfluss (Öffnen → Cache-Anzeige → Live-Ersatz) kann nur über die UI-Kette StopSearchPage→`OpenAsync`→`OpenStopAsync`→Fassade→DeparturePage nachgewiesen werden |
| Empfohlen | Nicht-Favorit mit zufällig gleich gelagertem Cache-Eintrag zeigt kein persistiertes Board | gleiche Datei (im selben Schalter) | Membership-Prüfung der Fassade auf realem Pfad | Membership ist eine sicherheitsrelevante Nebenbedingung des Geschäftsvorgangs, über die UI auslösbar |

Bestehende E2E-Tests (`-InventoryOnly`, `-ExpiredOnly`, `-DetailRetention`, Standardlauf): keine Anpassung nötig — sie prüfen Home-Restore- und Session-Handoff-Pfade, deren sichtbares Verhalten unverändert bleibt; sie müssen nach dem Umbau weiterhin grün laufen (Regression).

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Home-Restore (Happy Path) | App-Start / erstmaliges Öffnen der Favoriten-Übersicht | Favorit gespeichert; `departure-cache.json` enthält Board mit künftiger Fahrt, `RetrievedAt` abgelaufen (stale) | `HomePage.OnAppearing` → `model.LoadAsync` → `RestoreBoardsAsync` → `card.Restore` | Karte zeigt sofort die gecachte Fahrt mit „Letzter Stand wird aktualisiert …", `IsStale` gesetzt; Startup-Refresh ersetzt sie durch Live-Daten | `FavoriteDepartureCacheIntegrationTests.LoadAsync_RestoresStaleBoardWithFutureDepartureAsMarkedStale` + UI-Test Standardlauf |
| 2 | Monitor-Restore (Happy Path) | Nutzer öffnet eine gespeicherte Favoriten-Haltestelle über die Stopsuche | Favorit gespeichert; Cache-Board vorhanden; keine Session-Boards (`retained == null`, LRU leer) | `OpenAsync` → `OpenStopAsync` → `RestoreBoardAsync` → Membership ok → Board → `Result` → `RefreshAsync` | Gecachte (ggf. stale-markierte) Abfahrten sofort sichtbar, danach Live-Board | `ConnectionFavoriteTests` (angepasst) + E2E `-MonitorStale` |
| 3 | Nicht-Favorit | Nutzer öffnet eine beliebige gefundene Haltestelle | Cache enthält Eintrag zum Stop, aber Stop ist kein gespeicherter Favorit | wie 2, Membership-Prüfung schlägt fehl | Kein persistiertes Board; nur Live-Anzeige | `DepartureBoardCacheTests` + neuer Monitor-Test |
| 4 | Board ohne künftige Fahrten | App-Start | Cache-Eintrag nur mit vergangenen/zeitlosen Items, `Lines = ["S1"]` | `RestoreBoardsAsync` → `Project` → `Result == null`, `Lines` gefüllt | Karte ohne Abfahrtszeilen, aber mit Linien-Badges | `LoadAsync_KeepsCacheBoardWithOnlyExpiredDeparturesForItsLineInventory` |
| 5 | Abbruch während des Lesens | Nutzer verlässt die Seite, während die Fassade die Dateien liest | Öffnen läuft, `request`-CTS aktiv | `CancelPending` → Token abgebrochen → `OperationCanceledException` → Öffnen abgebrochen; `finally` räumt `request` per `ReferenceEquals` | Kein Board, keine verwaiste Anzeige; Revision-Guard verhindert späte Übernahme; kein disposed-CTS-Fehler beim nächsten `CancelPending` | Unit-Ebene über `RestoreBoardAsync_HonorsCancellation`; `ResumeMonitorTests_Cancellation` bleibt grün |
| 6 | Fassade nicht injiziert | `boards == null` (Monitor ohne Fassade; Home ohne `cache`-Store) | — | `OpenStopAsync`/`LoadAsync` ohne Fassaden-Aufruf | Monitor öffnet leer + Live; Home-Load ohne Restore — wie heute ohne Cache | `StopMonitorTests`, `FavoriteTests` unverändert grün |

## Offene Punkte

Keine. (Die drei Scope-Ausschlüsse stehen auf `bestätigt (vorbehaltlich)` — sie wurden am 2026-11-22 in Issue #21 zur Kenntnis/Widerspruch vorgelegt; bei Widerspruch würden sie wieder geöffnet.)
