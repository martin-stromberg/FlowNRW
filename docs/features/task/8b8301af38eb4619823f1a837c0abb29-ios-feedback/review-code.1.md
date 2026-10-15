# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs` (FavoriteMonitorViewModel)

- **Fehlerbehandlung/Konsistenz** — `RefreshCoreAsync`, Z.126: `EnrichLines(result)` läuft vor der Prüfung, ob das degradierte Ergebnis überhaupt angezeigt wird. Wird ein degradiertes Ergebnis verworfen, weil `Result` bereits ein komplettes Inventar enthält (`SetFailure`-Pfad, Z.127), werden dessen Linien trotzdem in `Lines` gemergt — die Zusammenfassung zeigt dann Linien einer nie angezeigten Antwort. Plan Z.38 schreibt die Anreicherung nur für *angezeigte* degradierte Ergebnisse vor. Zusätzlich ist die XML-Dokumentation von `Lines` („Last complete line inventory", Z.39–40) durch die Merge-Semantik veraltet.
  Empfehlung: `EnrichLines(result)` in den `else`-Zweig hinter `Result = result` verlagern (nur bei tatsächlich übernommenem Ergebnis) und die `Lines`-Dokumentation auf „komplettes Inventar ∪ Linien angezeigter degradierte Ergebnisse" anpassen.
- **Doppelter Code** — Die Dedup-und-Sortier-Pipeline `Select(LineName) … Where(!IsNullOrWhiteSpace).Distinct(StringComparer.Ordinal).OrderBy(…, StringComparer.Ordinal).ToArray()` steht dreimal identisch im Code (`Restore` Z.93–94, `IsComplete`-Zweig Z.140, `EnrichLines` Z.162–164).
  Empfehlung: privaten statischen Helfer `SortedLines(IEnumerable<string>)` extrahieren und an allen drei Stellen nutzen.

### `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs` (FavoriteHomeViewModel)

- **Kopplung/fragiles Matching** — `NearbyDistanceLabel`, Z.140/143: Der Treffer erfolgt ausschließlich über `ReferenceEquals(item.Stop, candidate.Stop)`, und der Haversine-Fallback ist an `item is not null` gekoppelt. Sobald `NearbyStops` je mit identitätsgleichen, aber nicht referenzidentischen `Stop`-Objekten neu projiziert wird (oder ein Aufrufer eine gleichwertige `Address` übergibt), wird still „Entfernung unbekannt" geliefert — obwohl Koordinaten für die Luftlinie vorhanden wären. `SameIdentity` (Z.300) existiert bereits und deckt diesen Fall robust ab.
  Empfehlung: `FirstOrDefault(item => candidate.Stop is { } s && (ReferenceEquals(item.Stop, s) || SameIdentity(item.Stop!, s)))` verwenden bzw. den Luftlinien-Fallback nicht an den Referenz-Treffer binden.

### `FlowNRW.Core/Presentation/StopMonitorViewModel.cs` (StopMonitorViewModel)

- **Doppelter Code** — `HasDataNotice` (Z.99–102) ist wortgleich mit `FavoriteMonitorViewModel.HasDataNotice` (Z.47–50) dupliziert.
  Empfehlung: gemeinsame Ableitung teilen, z. B. als interne Extension `HasDataQualityNotice()` auf `ProviderResult<T>` im `FlowNRW.Core.Presentation`-Namespace.

### `FlowNRW.Core/Presentation/JourneyPresentation.cs` (JourneyPresentation)

- **Doppelter Code** — Der Koordinaten-Suffix `FormattableString.Invariant($" · {c.Latitude:0.#####}, {c.Longitude:0.#####}")` steht identisch in `Address` (Z.43) und `DisplayAddress` (Z.64).
  Empfehlung: privaten Helfer `CoordinateSuffix(GeoCoordinate?)` extrahieren.

### `FlowNRW/MapPage.cs` (MapPage)

- **Fehlerbehandlung/Race** — Der `loadSettled`-/Session-Guard ist lückenhaft: `model.Session` ändert sich nur in `MapViewModel.Reset()` (ShowStops/ShowFavorites/ShowJourney), nicht je `init`. Eine in-flight `loaded`/`failed`-Nachricht eines vorherigen `init` (z. B. beim „Karte neu laden" genau um den JS-Watchdog-Zeitpunkt) passiert den Session-Filter (Z.204), setzt nach `loadSettled = false` (Z.143) erneut `loadSettled = true` und überschreibt den Status des neuen Ladevorgangs; der frisch gestartete `WatchLoadAsync` (Z.172) wird dadurch wirkungslos — genau das Endlos-Ladeszenario, das der Watchdog absichern soll.
  Empfehlung: pro `init` eine Generations-ID (inkrementierender Zähler, z. B. `loadId` im `init`-Payload) mitsenden, die `map.js` bei `loaded`/`failed` zurückspiegelt; native Seite verwirft Nachrichten mit veralteter Generation und `WatchLoadAsync` prüft dieselbe ID statt nur `loadSettled`/`session`.
- **Fehlerbehandlung** — `_ = WatchLoadAsync(session, token)` (Z.162): Es wird nur `OperationCanceledException` gefangen. `request.Cancel()` (Z.177) kann `AggregateException` werfen, wenn ein Cancellation-Callback fehlschlägt; jeder sonstige Fehler bleibt auf der verworfenen Task unobserved und der Watchdog stirbt still, ohne dass der Status je aktualisiert würde.
  Empfehlung: zusätzlich `catch (Exception)` mit `Core.Diagnostics.AppLog.Write("map", …)` ergänzen — konsistent zum übrigen Fehlerstil der Datei.
- **Cancellation-Hygiene** — `InitializeAsync` bricht offene `tileRequests` des vorherigen `init` nicht ab: Bis zu 64 laufende `tiles.GetAsync`-Aufrufe belegen weiter `pending` und erzeugen Antworten, die `map.js` nach `waiting.clear()` verwirft. Der Abbruch-Loop existiert nur im Watchdog-Timeout (Z.175–179).
  Empfehlung: Beim Re-`init` dieselbe Cancel-Schleife über `tileRequests` ausführen (gemeinsamen privaten Helfer nutzen), bevor `loadSettled` zurückgesetzt wird.

### `FlowNRW/Resources/Raw/map/map.js`

- **Ressourcen-Leak** — Der `watchdog`-`setInterval` (Z.35) wird nach `failed`/`loadDone` nie gecleart; er tickt alle 15 s nutzlos weiter bis zum nächsten `init` oder Seitenende.
  Empfehlung: `clearInterval(watchdog); watchdog=null;` ausführen, sobald `failed`/`loadDone` gesetzt wird — direkt im Intervall-Callback sowie im `load`-Handler (Z.44) und nach `expire` (Z.16).

### `FlowNRW/HomePage.cs` (HomePage)

- **Doppelter Code** — Der Notice-Toggle-Block ist nahezu identisch mit `DeparturePage` dupliziert: Status-`Grid` mit `[Star, Auto]` (Z.210–226), `warning`-Button mit Toggle-`RelayCommand` (Z.217–223), `FavoriteMetadata`-Label (Z.215–216), Reset-beim-Ausblenden (Z.312–317) und `SetAppThemeColor` nach `ApplyRoles` inklusive identischem Kommentar (Z.240–242; Gegenstück `DeparturePage` Z.89–109, Z.163–168).
  Empfehlung: gemeinsamen Helfer extrahieren, z. B. `TransitVisuals.NoticeToggle(string automationIdPrefix, Func<bool> hasNotice, …)`, der Status-Row, Detail-Label und Sichtbarkeits-Reset liefert; beide Seiten nutzen denselben Code.
- **Toter Code** — `SemanticProperties.SetDescription(button, "Nahe Haltestelle " + candidate.Name)` (Z.359) wird unmittelbar danach von `TransitVisuals.Candidate` (`SetDescription(action, description)`, `TransitVisuals.cs` Z.74) überschrieben; die Zeile hat keine Wirkung.
  Empfehlung: Zeile entfernen (oder die „Nahe Haltestelle"-Kontextinfo in den `suffix`/die Beschreibung von `Candidate` überführen, falls sie gewünscht ist).

### `FlowNRW/TransitVisuals.cs` (TransitVisuals)

- **Speculative Generality/toter Code** — Der `showTechnicalIdentity: true`-Pfad (`JourneyPresentation.Address` als `identity`/`shown`, Z.68/75–76) hat nach dieser Änderung keinen Aufrufer mehr: Alle vier Call-Sites (`MapPage`, `StopSearchPage`, `SearchPage`, `HomePage`) übergeben `false`. Der Default `true` bleibt als ungenutztes Opt-in bestehen.
  Empfehlung: Falls der Plan-Erhalt bewusst bleibt (plan.md Z.27), ist dies zu akzeptieren — andernfalls Parameter entfernen oder Default auf `false` ändern, damit der tote Zweig nicht unbemerkt veraltet.

## Geprüfte Dateien

- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- `FlowNRW.Core/Maps/MapViewModel.cs`
- `FlowNRW.Core/Presentation/EndpointViewModel.cs`
- `FlowNRW.Core/Presentation/JourneyPresentation.cs`
- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs`
- `FlowNRW.Tests/FavoriteTests.cs`
- `FlowNRW.Tests/PresentationTests.cs`
- `FlowNRW.Tests/StopMonitorTests.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/JourneyTimelineView.cs`
- `FlowNRW/MapPage.cs`
- `FlowNRW/Resources/Raw/map/map.js`
- `FlowNRW/SearchPage.cs`
- `FlowNRW/TransitIcons.cs`
- `FlowNRW/TransitVisuals.cs`
- `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`
- `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`
