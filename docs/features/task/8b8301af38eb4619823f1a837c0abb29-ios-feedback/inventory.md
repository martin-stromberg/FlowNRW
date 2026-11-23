# Bestandsaufnahme: iOS-Nutzungsfeedback — Favoriten, Hinweismeldung, Karte und Trefferlisten

Die `requirement.md` beschreibt sieben Befundpunkte aus der iOS-Nutzung von FlowNRW. Diese Bestandsaufnahme dokumentiert den Ist-Stand der betroffenen Codepfade in `FlowNRW/` (Oberfläche), `FlowNRW.Core/` (ViewModels, Präsentation, Transit-Modelle) und `FlowNRW.Tests/` — ausschließlich was existiert, ohne Umsetzungsvorschläge.

## Zusammenfassung

### 1) Favoriten zeigen zugeklappt keine Linienliste mehr

- `FavoriteMonitorViewModel.Lines` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs` Z.37) wird nur an zwei Stellen befüllt: im Cache-`Restore` (Z.84–86) und nach einem Provider-Abruf ausschließlich wenn `IsComplete(result)` gilt — d. h. `ErrorCode is null && Warnings.Count == 0 && !IsFallback && !IsStale` (Z.148, Aufruf Z.129–132).
- Seit Commit `69e6f93` („let degraded fresh answers replace a degraded board") ersetzt ein degradiertes Ergebnis (z. B. `Warnings > 0`) das angezeigte `Result` (Z.118–125), ohne `Lines` neu zu berechnen. Auf einer Karte ohne je vollständige Antwort bleibt `Lines` dauerhaft leer; der `Lines`-Bestand ist als „letztes vollständiges Inventar" konzipiert (`DepartureCacheEntry.Lines`, `IDepartureCacheStore.cs` Z.57–58) und durch den Test `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` (`FavoriteDepartureCacheIntegrationTests.cs` Z.110–131) festgehalten.
- `HomePage.RenderFavoriteSummary` (`FlowNRW/HomePage.cs` Z.241–256) iteriert ausschließlich `card.Lines`; der `summaryContainer` (`FavoriteLines{index}`) ist sichtbar bei `!card.IsExpanded` (Z.222) und wird bei `Items`-/`Lines`-Änderungen neu gerendert (Z.278).

### 2) Hinweis „Daten möglicherweise unvollständig oder veraltet." zu aufdringlich

- Der Text entsteht an zwei Stellen: `FavoriteMonitorViewModel.RefreshCoreAsync` Z.126–128 und `StopMonitorViewModel.RefreshCoreAsync` (`FlowNRW.Core/Presentation/StopMonitorViewModel.cs` Z.289–291), jeweils bei `Warnings.Count > 0 || IsFallback || IsStale`.
- Anzeige: `FavoriteStatus{index}`-Label in `HomePage` Z.210–213 mit Sichtbarkeit über `ShowsFavoriteStatus` (Z.360–364, matcht u. a. „unvollständig"); `MonitorStatus`-Label in `DeparturePage` Z.87–91 mit `UpdateMonitorStatus` (Z.137–148, gleiche Wort-Matches).
- Detailinformation existiert bereits: `JourneyPresentation.Metadata<T>` (Z.48) liefert Quelle/Datenstand/Datenalter/Fallback/Cache/Anbieterwarnung; `card.Metadata` (`FavoriteMonitorViewModel` Z.39–42) ist vorhanden, wird aber in `HomePage` nicht gebunden. `CompactWarning<T>` (Z.53–59) wird an anderer Stelle (StopSearchPage, JourneyDetailPage) bereits als kompakter Text genutzt.
- Icon-/Chip-Infrastruktur vorhanden (`TransitIcons`, `TransitVisuals.StatusChip`/`IconLabel`, Semantikfarben `Warning`/`WarningDark`, `Error`/`ErrorDark`); ein dezentes Warnsymbol ist noch nicht als Konstante definiert (`warning` U+F083 existiert im Codepoints-Manifest, Zeile 4163).

### 3) Auf-/Zuklapp-Schalter in rechter oberer Ecke

- Aktuell sitzt `ToggleFavorite{index}` (Button, `expand_more`/`expand_less`, 48×48) als erste Spalte der Vier-Spalten-Aktionsleiste `BuildCardActions` (`HomePage.cs` Z.290–325) am Kartenende.
- `BuildCardHeader` (Z.229–239) hat rechts bereits eine `Auto`-Spalte mit `FavoriteBusy{index}`-`ActivityIndicator`.
- Zustandslogik: `card.ToggleExpanded()` / `IsExpanded` (`FavoriteMonitorViewModel` Z.97–102, Z.46), Sichtbarkeit über `SubscribeCardChanges` (`HomePage` Z.274–288).

### 4) OSM-Basiskarte lädt unter iOS nicht („Basiskarte wird geladen …")

- Ladepfad: `HybridWebView` `MapCanvas` (`MapPage.cs` Z.17) lädt `Resources/Raw/map/index.html`; `map.js` sendet bei `window load` `{type:'ready'}`; `MessageReceived` (`MapPage.cs` Z.168–204) ruft bei `ready` `InitializeAsync` (Z.136–157) → `EvaluateJavaScriptAsync("window.receiveMap({init…})")` (Z.166); Leaflet-`NativeTiles`-`GridLayer` sendet pro Kachel `{type:'tile'}`, maximal 4 parallel (`map.js` Z.6, 23–28); Antworten kommen als `tile` zurück; `layer.on('load')` meldet `loaded`/`failed` (`map.js` Z.29).
- `CheckStartupAsync` (Z.124–128) deckt nur den Fall `!ready` nach 15 s ab. Bleiben `tile`-Antworten aus oder feuert `layer.on('load')` nicht, steht der Status endlos auf „Basiskarte wird geladen …" — ein `failed`-Pfad wird nur bei `data.error` oder `image.onerror` ausgelöst (`map.js` Z.13–15).
- Kacheln laufen ausschließlich über das native Gateway `MapTileService.GetAsync` (`FlowNRW.Core/Maps/MapTileService.cs` Z.45–103): serialisiert über `SemaphoreSlim(1,1)`, HTTPS mit `AllowAutoRedirect=false`, PNG-Signaturprüfung, Disk-Cache unter `FileSystem.CacheDirectory/MapTiles` (`MauiProgram.cs` Z.170–172), Timeout `Map:Timeout` (Default 10 s).
- Alle Webressourcen sind lokal (`index.html` CSP: `connect-src 'self'`, `img-src 'self' data:`); `Info.plist` enthält keine ATS-Ausnahmen — der WebView lädt keine Netzressourcen. Auf iOS rendert `HybridWebView` über WKWebView; es existiert kein plattformspezifischer WebView-Code (`Platforms/iOS/`: nur `AppDelegate.cs`, `IosBackgroundRefresh.cs`, `Program.cs`, `Info.plist`). Die konkrete Fehlerstelle (fehlendes `ready`, scheiterndes `EvaluateJavaScriptAsync`, hängende `tile`-Roundtrips oder ausbleibendes `load`-Event) ist im Code nicht unterscheidbar; die Windows-UI-Tests belegen ein funktionierendes Ende („Basiskarte geladen", `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1` u. a. Z.480, 576).

### 5) Interne Kennungen bei nahegelegenen Haltestellen / Entfernung fehlt

- `HomePage.RenderNearby` (Z.327–339) ruft `TransitVisuals.Candidate(candidate, button)` ohne `showTechnicalIdentity` auf → Default `true` (`TransitVisuals.cs` Z.65–78): `Secondary`-Detailzeile enthält den `JourneyPresentation.Address`-Rest `Source Id Dhid · Lat, Lon`.
- `FavoriteHomeViewModel.RefreshNearbyAsync` (Z.117–123) bildet `NearbyStopResult` auf `Address` ab und verwirft dabei `DistanceMeters`; die Position liegt intern in `position` (Z.21, gesetzt Z.114), die Haversine-Hilfsfunktion `Distance(origin, destination)` existiert bereits (Z.274–281).
- Referenzmuster mit Entfernung existiert: `StopSearchPage.RenderMatches` (Z.72–79) nutzt `showTechnicalIdentity: false` + `StopMonitorViewModel.DistanceLabel` (Z.143–149, „ · Entfernung: X m" aus `NearbyResult.Items[].DistanceMeters`).

### 6) Interne Kennungen in der Verbindungssuche

- `SearchPage.Endpoint` (Z.142–147) ruft `TransitVisuals.Candidate(candidate, button)` mit Default `showTechnicalIdentity: true` auf → identische Kennungs-Detailzeile und dieselbe Kennung in `SemanticProperties.Description` (`TransitVisuals` Z.67, 72).
- `EndpointViewModel.Selection` (Z.84–90) zeigt „Ausgewählt: " + `JourneyPresentation.Address(...)` — also ebenfalls `Source`/`Id`/`Dhid`/Koordinaten, gebunden an `{prefix}Selection` (`SearchPage` Z.149, sichtbar ab Z.178).
- Bereits bereinigt: `MapPage` (Z.75–81) und `StopSearchPage` (Z.72–79) übergeben `showTechnicalIdentity: false`.

### 7) Haltestellenname „X, X Hbf" → „X Hbf"

- Namen stammen unverändert aus den Providern: `EfaResponseMapper` (`Address.Name`/`Stop.Name` aus `name`, Z.18/46 — EFA liefert typischerweise „Ort, Haltestelle"), `DbRestResponseMapper` (`name`/`address`, Z.18/42).
- Es existiert kein Namensbereinigungs-Helfer. Namensanzeigestellen: `TransitVisuals.Candidate` (`action.Text = candidate.Name`, Z.68), `EndpointViewModel.Selection` (Z.88), `JourneyPresentation.Event`/private (Z.91: `item.Identity.Stop.Name`), `JourneyTimelineView` (Z.32–33, 53), `MapStation.Label` (`MapViewModel.cs` Z.25–28), `FavoriteMonitorViewModel.Title` (Z.29), `StopMonitorViewModel.Title` (Z.80).

## Details

- [Favoritenkarten: Lines-Logik und Kartenstruktur](inventory/favorites.md) — `FavoriteMonitorViewModel`-Zustandsmaschine, `IsComplete`, Cache-`Lines`, `HomePage`-Kartenaufbau, Expand-Mechanik
- [Hinweis „unvollständig oder veraltet": alle Erzeugungs- und Anzeigestellen](inventory/notice.md) — Status-Texte, Sichtbarkeitsregeln, `Metadata`/`CompactWarning`, beteiligte AutomationIds
- [Karten-Ladepfad](inventory/map.md) — HybridWebView, map.js-Protokoll, Tile-Gateway, CSP, iOS-Unterschiede, Timeout-Abdeckung
- [Namen und interne Kennungen](inventory/naming.md) — `JourneyPresentation.Address`, `TransitVisuals.Candidate`, alle Call-Sites, Namensquellen und Anzeigestellen
- [Entfernungsmechanik](inventory/distance.md) — `DistanceLabel`, `NearbyStopResult.DistanceMeters`, `position`/`Distance`-Hilfsfunktion in `FavoriteHomeViewModel`
- [Vorhandene Tests und UI-Test-Fixtures](inventory/tests.md) — Core-Tests, PowerShell-UI-Test-Erwartungen, Fixture-Szenarien
- [Icon- und Farbinventar](inventory/icons.md) — `TransitIcons`-Glyphen, `TransitVisuals`-APIs, Semantikfarben, freie Manifest-Codepoints
