# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### `FlowNRW/Resources/Raw/map/map.js` (receiveMap)

- **Fehlerhandling/Race (Restlücke)** — Die Iteration-1-Lücke ist im Normalpfad geschlossen (Generations-`loadId` im `init`-Payload, Rückspiegelung bei `loaded`/`failed`, nativer Verwurf veralteter Generationen in `MapPage.cs` Z.222). Eine Restlücke bleibt: Der `load`-Handler (Z.46) und `announceFailed` (Z.9) lesen `session`/`loadId`/`failed` als veränderliche Globals zum Sendezeitpunkt, nicht als an die Generation gebundene Closure-Werte. Feuert das `load`-Event einer per `map.remove()` (Z.33) verworfenen Schicht verspätet — nach dem Re-`init` —, sendet es `loaded` mit der *neuen* `loadId`: nativ wird `loadSettled = true` gesetzt (`MapPage.cs` Z.227), `clearWatchdog()` räumt zudem den frisch installierten JS-Watchdog ab, sodass `WatchLoadAsync` für die laufende Generation wirkungslos aussteigt (`MapPage.cs` Z.176) und nur noch die Per-Tile-`expire`-Timer schützen.
  Empfehlung: Beim `init` Generation und Session in Closure-Konstanten binden (z. B. `const gen=data.loadId, sess=data.session`) und die Layer-Handler (`load`, `announceFailed`, `expire`, `cancel`-Sendungen) gegen diese prüfen bzw. mit ihnen senden — analog zur nativen `generation`-Prüfung.

### `FlowNRW/MapPage.cs` (MapPage)

- **Fehlerhandling/Semantik** — Verworfene Tile-Anforderungen (`pending >= 64` oder doppelte `id`, Z.236–242) werden mit `["error"] = false` und leerem `data` beantwortet. In `map.js` führt das zu `item.done(new Error('tile'))` ohne `announceFailed` (Z.28–29): Die Kachel gilt in Leaflet als fehlgeschlagen, aber `failed` bleibt `false` — beim Drain der Queue meldet `load` dann `loaded` und der Status zeigt „Basiskarte geladen.", obwohl Kacheln dauerhaft fehlen (z. B. wenn `pending` durch hängende, die Cancellation ignorierende `GetAsync`-Aufrufe saturiert bleibt). Die Kommentar-Absicht (Dropped ≠ Gesamtfehler) ist nachvollziehbar, aber die fehlerhafte Kachel wird dem Nutzer als erfolgreicher Ladevorgang verkauft.
  Empfehlung: Entweder `error: true` senden (Drop ist für diese Kachel ein Fehler; ggf. in `map.js` einen separaten „einzelne Kachel verworfen"-Pfad ohne `announceFailed` einführen, falls Prefetch-Drops den Gesamtstatus nicht kippen sollen) oder den bewussten Trade-off im Kommentar um die „geladen trotz fehlender Kacheln"-Konsequenz ergänzen.

### `FlowNRW/StopSearchPage.cs` (StopSearchPage) / `FlowNRW/MapPage.cs` (MapPage)

- **Toter Code/Doppelter Code** — Wie die in Iteration 1 entfernte `SetDescription`-Zeile werden auch `Text`-Initialisierer an `Candidate`-Aufrufern wirkungslos überschrieben (`TransitVisuals.cs` Z.75 setzt `action.Text` unbedingt): `StopSearchPage.cs` Z.74 `Text = JourneyPresentation.Address(candidate) + model.DistanceLabel(candidate)` berechnet zudem die technische Kennung umsonst und ruft `DistanceLabel` doppelt auf (Z.74 und Z.79); `MapPage.cs` Z.79 `Text = station.Label` ist ebenfalls tot. Die Bereinigung aus Befund 11 wurde nur in `HomePage` durchgezogen.
  Empfehlung: Beide `Text`-Initialisierer entfernen; in `StopSearchPage` `model.DistanceLabel(candidate)` einmal in eine lokale Variable heben.

### `FlowNRW/HomePage.cs` (HomePage) / `FlowNRW/DeparturePage.cs` (DeparturePage)

- **Doppelter Code (Residual)** — `NoticeToggle` kapselt Button/Detail-Paar und Sichtbarkeits-Reset sauber, aber der identische Workaround „Warning-Färbung nach `ApplyRoles`" inklusive wortgleichem Kommentar steht weiterhin zweimal (`HomePage.cs` Z.234–236, `DeparturePage.cs` Z.100–102); ebenso die `statusRow`-Assemblierung (`Grid` Star/Auto + `Update(hasNotice && !IsBusy)`-Initialaufruf). Das Wissen über die notwendige Aufrufreihenfolge gegenüber `ApplyRoles` ist damit an beiden Stellen dupliziert.
  Empfehlung: `NoticeToggle` um eine Methode wie `ApplyWarningColors()` (oder `StatusRow(Label)`) erweitern, sodass die Seiten nur noch `notice.ApplyWarningColors()` bzw. `notice.Row(status)` aufrufen und die Reihenfolge-Konvention im Helfer dokumentiert bleibt.

## Geprüfte Dateien

- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- `FlowNRW.Core/Maps/MapViewModel.cs`
- `FlowNRW.Core/Presentation/DataNotice.cs` (neu)
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
- `FlowNRW/StopSearchPage.cs`
- `FlowNRW/TransitIcons.cs`
- `FlowNRW/TransitVisuals.cs`
- `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`
- `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`
