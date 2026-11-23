# Karten-Ladepfad: „Basiskarte wird geladen …" hängt unter iOS

Deckt Anforderungspunkt 4 ab. Dokumentiert den kompletten Codepfad von der `HybridWebView` bis zum Tile-Gateway und die vorhandene Fehler-/Timeout-Abdeckung. Der konkrete iOS-Auslöser ist im Code nicht eingrenzbar — unten sind alle möglichen Bruchstellen aufgeführt.

## Nativer Teil: `FlowNRW/MapPage.cs`

| Element | Zeile | Beschreibung |
|---|---|---|
| `map` | `HybridWebView`, Z.17 | `HybridRoot = "map"`, `DefaultFile = "index.html"`, `HeightRequest = 430`, `AutomationId = "MapCanvas"` |
| `status` | `Label`, Z.18 | `AutomationId = "MapStatus"`, Initialtext „Karte wird geladen …" |
| `ready`, `pending`, `tileRequests` | Z.20–22 | `ready`-Flag; `pending`-Zähler für Kachelrunden (Obergrenze 64, Z.190); offene Kachel-`CancellationTokenSource`s je `id` |
| `lifetime` | `CancellationTokenSource`, Z.19 | Seiten-Lebenszeit; `InitializeAsync` setzt sie neu (Z.139) |
| Konstruktor | Z.32–89 | `ResetMap`-Aktion (`InitializeAsync`), `MapInformation`-Toggle, `ShowMapList`/`ShowMapCanvas`-Umschalter (Z.64–71), Stationsliste via `TransitVisuals.Candidate(..., showTechnicalIdentity: false)` (Z.75–81), `MapAttribution` (Z.84), `map.RawMessageReceived += MessageReceived` (Z.87) |
| `OnAppearing` | Z.92–99 | `foreground` abonnieren, `lifetime` neu; `ready ? InitializeSafelyAsync : CheckStartupAsync(token)` |
| `ForegroundChanged` | Z.109–122 | Inaktiv: `lifetime.Cancel()` + „Kartenabrufe pausiert…"; aktiv: `ready ? InitializeSafelyAsync : CheckStartupAsync` |
| `CheckStartupAsync` | Z.124–128 | Nach 15 s ohne `ready`: „Karte nicht verfügbar. Haltestellenliste und Zurück bleiben nutzbar." — **einziger** Timeout |
| `InitializeSafelyAsync` | Z.130–134 | Fängt `InitializeAsync`-Fehler → „Karte nicht verfügbar. Bitte die Haltestellenliste verwenden." |
| `InitializeAsync` | Z.136–157 | Setzt „Basiskarte wird geladen …" (Z.140), baut `markers`/`segments`/`endpoints`, sendet `{type:"init", session, …, minZoom, maxZoom}` via `SendAsync` |
| `SendAsync` | Z.166 | `map.EvaluateJavaScriptAsync("window.receiveMap(" + json + ")")` |
| `MessageReceived` | Z.168–204 | `RawMessageReceived`-Handler, `args.Message` ≤1024 Zeichen JSON. `ready` → `ready=true`, `InitializeAsync` (Z.177). Frühe Abbrüche bei `token`-Abbruch/`!foreground.IsActive` (Z.178) oder falscher `session` (Z.179). `select` → `model.SelectAsync` (Z.183). `loaded` → „Basiskarte geladen." (Z.184). `failed` → „Basiskarte fehlt oder ist veraltet. Haltestellen und gelieferte Verläufe bleiben nutzbar." (Z.185). `cancel` → `tileRequests[id].Cancel()` (Z.186). `tile` → `tiles.GetAsync(z,x,y)` → Antwort `{type:"tile", data|error}` (Z.188–200). Generischer `catch` → „Kartenabruf fehlgeschlagen. Bitte erneut laden oder die Liste verwenden." (Z.203) |
| `MapViewport` (nur `UI_TEST_FIXTURES`) | Z.23–25, 43–46, 181 | Testkanal: `view`-Nachrichten mit lat/lon/zoom |

## WebView-Ressourcen (`FlowNRW/Resources/Raw/map/`)

| Datei | Inhalt |
|---|---|
| `index.html` (6 Zeilen) | CSP: `default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'; frame-src 'none'; object-src 'none'; base-uri 'none'`. Lädt `_framework/hybridwebview.js`, `leaflet.js`, `map.js` (defer), `leaflet.css`, `map.css` — alle lokal |
| `map.js` (45 Zeilen) | `send` = `window.HybridWebView.SendRawMessage(JSON.stringify(data))` (Z.5). `window.receiveMap` (Z.8–40): `tile`-Antworten → `waiting`-Map, `image.onload`/`onerror` → `done`, bei `data.error` oder `onerror` → `send({type:'failed'})` (Z.9–17); `init` → `L.map` mit `NativeTiles = L.GridLayer.extend({createTile})` (Z.21–27): `createTile` queued `{type:'tile',session,id,z,x,y}`, `pump()` hält max. 4 aktive Kacheln (Z.6); `tileunload` → `cancel` (Z.28); `layer.on('load')` → `send({type: failed?'failed':'loaded'})` (Z.29); Marker als `L.divIcon` mit `select`-Klick, Polylinien-Segmente, `fitBounds` (Z.30–39). `window.addEventListener('load', …)` sendet `{type:'ready'}` (Z.41) |
| `leaflet.js`/`leaflet.css` | Lokal gebündelte Leaflet 1.9.4 (SHA256 in `ASSETS.md` dokumentiert) |
| `map.css` | `#map` 100 %-Höhe, `.station`-Div-Marker, Zoom-Control ≥44 px, Tooltip-Styles |
| `ASSETS.md` / `LEAFLET-LICENSE.txt` | Herkunft/Lizenz |

Einbindung: `FlowNRW.csproj` Z.69–70 `<MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)"/>` — `map/` wird als `HybridRoot` verwendet.

## Tile-Gateway (`FlowNRW.Core/Maps/MapTileService.cs`)

| Element | Zeile | Beschreibung |
|---|---|---|
| `IMapTileService.GetAsync(z,x,y,ct)` → `MapTile` | Z.13–25 | `MapTile(Data, Stale, Error)`; `Data` = `data:image/...;base64,…`-URI oder `""` |
| `gate = SemaphoreSlim(1,1)` | Z.33, 48, 102 | **Serialisiert alle** Kachel-Abrufe inkl. Cache-Hit-Pfad; JS-Seite erlaubt 4 parallel (`map.js` Z.6), nativ wird aber strikt nacheinander bedient |
| Cache-Lookup | Z.51–54 | Schlüssel = SHA256(URI), Datei `{hex}.json` unter `directory`; frischer Hit → sofort `new(cached.Data)` |
| HTTP | Z.55–100 | `timeout.CancelAfter(options.Timeout)`; `AllowAutoRedirect=false` (Client in `MauiProgram` Z.171); `User-Agent: FlowNRW/0.0.1`, `Accept: image/png`, `If-None-Match`/`If-Modified-Since`; `NotModified` → Cache verlängern (Z.66–76); `!IsSuccessStatusCode` oder `ContentLength > 1 MB` → `Failure(cached)` (Z.77–78); PNG-Signatur `137 80 78 71 13 10 26 10` (Z.89–90); `OperationCanceledException`-Timeout/`HttpRequestException`/`IOException` → `Failure(cached)` (Z.98–100) |
| `Failure(entry)` | Z.119 | `AllowStale` → `new(entry.Data, Stale:true, Error:true)`, sonst `new("", Error:true)` |
| Disk-Cache | Z.134–154 | `Directory.CreateDirectory`, LRU-Eviction bis `CacheMaxBytes`, `.tmp`-Datei + `File.Move` |
| `Expiry` | Z.105–117 | `Cache-Control` (`no-cache`, `max-age`, `Age`, `Date`), sonst `Expires`-Header oder +7 Tage |

## Konfiguration und Registrierung (`FlowNRW/MauiProgram.cs` Z.159–174)

| Element | Zeile | Beschreibung |
|---|---|---|
| `MapOptions` | Z.159–168 | `Map:TileUrl` Default `https://tile.openstreetmap.org/{z}/{x}/{y}.png`, `Map:Attribution`, `Map:MinZoom` 1 / `MaxZoom` 18, `Map:CacheMaxBytes` 64 MiB, `Map:Timeout` 10 s; `Validate()` (`MapOptions.cs` Z.21–30) erzwingt HTTPS, keine Credentials/Fragmente, Timeout ≤30 s |
| `IMapTileService` | Z.170–172 | `MapTileService` mit eigenem `HttpClient` (`Timeout = InfiniteTimeSpan`), Cache unter `FileSystem.CacheDirectory/MapTiles` |
| UiTest-Override | Z.175–177 | `UiTestMapTiles` (`tests/WindowsJourneyUiTests/UiTestFixtureServices.cs` Z.158–171) liefert SVG-data-URIs; „map-offline" → 4× `Error`, „map-slow" → 6 s Verzögerung |

## Karten-Anzeigequellen (`FlowNRW.Core/Maps/MapViewModel.cs`)

| Element | Zeile | Beschreibung |
|---|---|---|
| `ShowStops()` | Z.55–63 | Titel „Haltestellenkarte", `Stations` aus `monitor.Stops` |
| `ShowFavorites(home)` | Z.67–77 | **Titel „Favoritenkarte"** — die vom Kunden gemeldete Ansicht; `Stations` aus gespeicherten Karten, `favoriteSelection` ruft `monitor.OpenFavoriteAsync` (Z.76) |
| `ShowJourney` | Z.82–109 | Segmente/Endpoints aus Geometrie |
| `MapStation.Position`/`Label` | Z.17–29 | `IsMappable` (Z.121, |lat| ≤ 85.05112878); Label ergänzt „· Keine Kartenposition vorhanden" |
| `Session` | Z.40, 123–128 | `Guid.NewGuid().ToString("N")` je `Reset`; Nachrichten mit falscher `session` werden verworfen (`MapPage` Z.179) |

## Plattform-Hinweise

| Bereich | Befund |
|---|---|
| `FlowNRW/Platforms/iOS/` | Nur `AppDelegate.cs` (MauiUIApplicationDelegate + `IosBackgroundRefresh`), `IosBackgroundRefresh.cs` (BGTaskScheduler), `Program.cs`, `Info.plist` — **kein** WebView-/WKWebView-spezifischer Code |
| `Info.plist` | Keine ATS-Keys (`NSAppTransportSecurity` fehlt vollständig); `UIBackgroundModes fetch`, `NSLocationWhenInUseUsageDescription`, `UIDeviceFamily 1`. Keine ATS-Ausnahme erforderlich, da der WebView nur lokale Assets lädt und Kacheln via nativen HTTPS-Client laufen |
| `MauiProgram.cs` Z.23–25 | Windows setzt `WEBVIEW2_USER_DATA_FOLDER` unter `FileSystem.AppDataDirectory/MapWebView`; iOS hat kein Äquivalent (nicht erforderlich) |
| `FlowNRW.csproj` | iOS: `net10.0-ios`, `SupportedOSPlatformVersion 15.0` (Z.11, 43–45) |

## Status-Texte und Zeitlimit-Matrix

| Meldung | Auslöser | Ort |
|---|---|---|
| „Karte wird geladen …" | Initial | `MapPage` Z.18 |
| „Basiskarte wird geladen …" | `InitializeAsync` | Z.140 |
| „Basiskarte geladen." | `loaded` | Z.184 |
| „Basiskarte fehlt oder ist veraltet. Haltestellen und gelieferte Verläufe bleiben nutzbar." | `failed` | Z.185 |
| „Karte nicht verfügbar. Haltestellenliste und Zurück bleiben nutzbar." | `!ready` nach 15 s | Z.126 |
| „Karte nicht verfügbar. Bitte die Haltestellenliste verwenden." | `InitializeAsync`-Exception / `ResetMap`-Fehler | Z.48, 133 |
| „Kartenabrufe pausiert. Beim Zurückkehren wird die Karte neu geladen." | `foreground` inaktiv | Z.114 |
| „Kartenabruf fehlgeschlagen. Bitte erneut laden oder die Liste verwenden." | generischer `catch` in `MessageReceived` | Z.203 |
| „Karte nicht verfügbar." | `ShowMapCanvas`-Command-Fehler | Z.68 |

## Mögliche Hängepunkte (Ist-Stand, kein iOS-Gerätebeweis vorhanden)

1. `ready` kommt nie an: `window load` → `HybridWebView.SendRawMessage` (`_framework/hybridwebview.js`) — Bridge- oder Asset-Problem unter WKWebView. Abgedeckt durch `CheckStartupAsync` (15 s → „nicht verfügbar"). Meldung wäre dann „Karte nicht verfügbar", **nicht** „Basiskarte wird geladen …" — die Meldung des Kunden impliziert `ready` **kam** an.
2. `init`/`tile`-Roundtrips scheitern: `EvaluateJavaScriptAsync` oder `RawMessageReceived` funktioniert, aber `tiles.GetAsync` antwortet nicht/liefert `""` ohne `Error`, oder `SendAsync`-Antworten kommen nicht im WebView an. `MapTile` mit `Data=""` und `Error=false` (z. B. `OperationCanceledException` bei Seiten-`token`) löst in `map.js` `item.done(new Error('tile'))` ohne `failed`-Meldung aus (Z.15).
3. `layer.on('load')` feuert nie: Leaflet meldet `load` erst, wenn keine Kacheln mehr ausstehen; ein permanent wartender `waiting`-Eintrag (nie beantwortete `tile`-Anfrage) verhindert `loaded`/`failed` — es existiert **kein** Timeout für den Zeitraum zwischen `init` und `loaded`/`failed` (`CheckStartupAsync` beendet sich nach `ready=true`, Z.126–128).
4. `pending >= 64`/doppelte `id` → Anfrage wird still verworfen (`MapPage` Z.190), Kachel bleibt in `waiting` → kein `load`-Event.
5. `Directory`/`File`-Zugriff unter `FileSystem.CacheDirectory/MapTiles` auf iOS schlägt fehl → `ReadAsync`/`SaveAsync` fangen `IOException`/`UnauthorizedAccessException` (Z.129–130, 151–153); HTTP-Pfad selbst läuft weiter, `Failure(cached)` liefert dann `Error:true` → JS würde `failed` senden — hängt also nur, wenn `GetAsync` selbst nie zurückkehrt (Timeout deckt das: 10 s/Kachel, aber sequentiell hinter `gate`).

Windows-Referenz: `WindowsJourneyUiTests.ps1` Z.480, 525, 576, 582, 589–591, 602, 608, 614 und `WindowsDesignUiTests.ps1` Z.305–308 erwarten „Basiskarte geladen" bzw. „fehlt oder ist veraltet" (map-offline) — der Pfad ist auf Windows nachgewiesen funktionsfähig.
