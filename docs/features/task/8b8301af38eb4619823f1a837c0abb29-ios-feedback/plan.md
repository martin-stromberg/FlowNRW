# Umsetzungsplan: iOS-Nutzungsfeedback — Favoriten, Hinweismeldung, Karte und Trefferlisten

## Übersicht

Sieben eigenständige Befundpunkte aus der iOS-Nutzung (siehe `requirement.md`), alle in bestehenden Codepfaden lösbar — **kein Architekturumbau, keine Migration**. Kernarbeiten: (1) `FavoriteMonitorViewModel.Lines` wird auch aus degradierten, angezeigten Antworten angereichert, damit die zugeklappte Linienübersicht nicht leer bleibt; (2) der Hinweistext „Daten möglicherweise unvollständig oder veraltet." wird in der Favoritenkarte (und analog im Abfahrtsmonitor) durch ein kleines `warning`-Symbol mit Toggle-Detailbereich ersetzt (Muster: `MapPage` Info-Button); (3) der `ToggleFavorite{i}`-Schalter wandert aus der Aktionsleiste in die rechte obere Ecke des Kartenkopfs; (4) der Karten-Ladepfad erhält einen defensiven Watchdog (nativ + `map.js`), damit „Basiskarte wird geladen …" unter iOS nicht endlos stehen bleibt; (5)+(6) interne Kennungen (`Source`, `Id`, `Dhid`, Koordinaten) verschwinden aus Nahe-Liste und Verbindungssuche — bei Nahe-Treffern ersetzt durch die Entfernung; (7) ein neuer Präsentationshelfer `JourneyPresentation.DisplayName` bereinigt „X, X Hbf" → „X Hbf". Der UI-Test-/A11y-Vertrag (`AutomationId`, `SemanticProperties.Description`, 48×48-Bedienziele) bleibt stabil; ein bestehender UI-Test (`WindowsJourneyUiTests.ps1` Z.717, Kennungs-Assertion in `OriginSelection`) ist angepasst einzuplanen.

### Lückenanalyse je Punkt

1. **Linienliste zugeklappt leer:** `Lines` wird nur bei `IsComplete(result)` (`FavoriteMonitorViewModel.cs` Z.129–132) oder im Cache-`Restore` (Z.84–86) befüllt; degradierte Antworten (seit Commit `69e6f93` als Board akzeptiert, Z.118–125) lassen `Lines` unangetastet — auf Geräten mit dauerhaft degradierten Antworten (iOS-Timeout der zweiten Quelle) bleibt die Übersicht leer. `RenderFavoriteSummary` (`HomePage` Z.241–256) ist bereit; der Test `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` (`FavoriteDepartureCacheIntegrationTests.cs` Z.110–131) pinnt das Ist-Verhalten und ist anzupassen.
2. **Hinweis zu aufdringlich:** Text entsteht in `FavoriteMonitorViewModel` Z.126–128 und `StopMonitorViewModel` Z.289–291; Sichtbarkeit über Wort-Matching (`HomePage.ShowsFavoriteStatus` Z.360–364, `DeparturePage.UpdateMonitorStatus` Z.137–148). Es fehlt: ein strukturelles Flag (aktuell reiner String), das Symbol und der Detail-Toggle. Vorhanden: `card.Metadata`/`model.Metadata` (Provenienz, ungenutzt gebunden), Icon-Infrastruktur, `warning`-Glyph (U+F083) im Manifest.
3. **Schalter oben rechts:** `ToggleFavorite{index}` sitzt als erste von vier Spalten in `BuildCardActions` (Z.290–325) am Kartenende; `BuildCardHeader` (Z.229–239) hat rechts nur den `ActivityIndicator`. Fehlt: Verschieben des Buttons in den Kopf.
4. **Karte hängt:** `CheckStartupAsync` (Z.124–128) deckt nur `!ready`; zwischen `init` und `loaded`/`failed` existiert **kein** Timeout. Stille Pfade: `pending >= 64`/doppelte `id` (Z.190) verwirft Kachelanfragen ohne Antwort; `MapTile{Data="",Error=false}` (z. B. Seitenabbruch) erzeugt in `map.js` Z.15 `done(Error)` ohne `failed`; ein permanent wartender `waiting`-Eintrag verhindert `layer.on('load')` (Z.29) endgültig. Windows-Nachweis des funktionierenden Pfads vorhanden; iOS-Ursache im Code nicht eingrenzbar → defensiver Timeout + Diagnose-Logging.
5. **Kennungen in Nahe-Liste:** `HomePage.RenderNearby` (Z.327–339) nutzt `Candidate`-Default `showTechnicalIdentity: true` → `Secondary`-Zeile `· Source Id Dhid · Lat, Lon`. `FavoriteHomeViewModel.RefreshNearbyAsync` (Z.120–123) verwirft `DistanceMeters`; `position` (Z.21) und Haversine-`Distance` (Z.274–281) existieren. Referenzmuster: `StopMonitorViewModel.DistanceLabel` (Z.143–149) + `StopSearchPage` Z.72–79.
6. **Kennungen in Verbindungssuche:** `SearchPage.Endpoint` Z.145 nutzt denselben Default; `EndpointViewModel.Selection` (Z.84–90) zeigt die volle Kennung. UI-Test-Konflikt: `WindowsJourneyUiTests.ps1` Z.717 asserted `fixture-0.*de:05113` in `OriginSelection`/`DestinationSelection` — anzupassen. Koordinaten bleiben in `Selection` erhalten (Z.724 asserted `51.4556`).
7. **Namensbereinigung:** Kein Helfer vorhanden; Namen kommen unverändert aus `EfaResponseMapper`/`DbRestResponseMapper`. Anzeigestellen in `naming.md` Z.63–75 vollständig gelistet; natürlicher Testort `JourneyPresentationTests` (`PresentationTests.cs`).

## Designentscheidungen

| Komponente | Gewählter Ansatz | Begründung |
|---|---|---|
| Linieninventar bei degradierten Antworten (Punkt 1) | **Merge-Semantik:** bei `IsComplete(result)` bleibt `Lines = Linien der Antwort` (ersetzend, autoritativ) und wird persistiert; bei degradiertem, aber angezeigtem Ergebnis (`ErrorCode is null`, `!IsComplete`) wird `Lines = Lines ∪ Linien der angezeigten Items` angereichert (dedupliziert, sortiert). Cache-`DepartureCacheEntry.Lines` bleibt ausschließlich komplette Inventare. | Jede angezeigte Linie stammt aus einer echten gelieferten Antwort — keine erfundenen Linien (Anforderungsvorgabe). Deckt den iOS-Fall „nie ein komplettes Ergebnis" ab (Lines füllen sich aus dem degradierten Board) **und** erhält den volleren Bestand, wenn eine degradierte Antwort weniger Linien liefert als das letzte komplette Inventar. Alternativen: (a) `Lines` nur befüllen, wenn leer — minimalinvasiv, lässt aber angereicherte Neulinien aus degradierten Antworten liegen; (b) `Lines` immer aus den angezeigten `Items` neu ableiten — zerstört die dokumentierte „last complete inventory"-Semantik (Linien ohne aktuelle Abfahrt verschwänden) und den Cache-Vertrag. |
| Hinweis-„unvollständig" (Punkt 2) | **Strukturelles Flag + Icon-Toggle:** neue Eigenschaft `HasDataNotice` (public, Core) auf `FavoriteMonitorViewModel` und `StopMonitorViewModel` (`Result is { } r && (r.Warnings.Count > 0 \|\| r.IsFallback \|\| r.IsStale)`); dazu ein Icon-Button `FavoriteNotice{index}`/`MonitorNotice` (48×48, `warning`-Glyph, `Warning`/`WarningDark`-Färbung) nach dem `MapPage`-Muster (Z.50–61) — Klick blendet einen Detailbereich `FavoriteMetadata{index}`/`MonitorMetadata` mit dem vorhandenen `Metadata`-Text ein/aus. `ShowsFavoriteStatus`/`UpdateMonitorStatus` verlieren ihren „unvollständig"-Match; der `Status`-String im ViewModel bleibt unverändert (Semantik/Logging). | Der `Status`-Kanal mischt echte Fehler („fehlgeschlagen", „nicht geladen") mit dem beanstandeten Hinweis — Wort-Matching auf UI-Ebene kann sie nicht sauber trennen, ein strukturelles Flag schon (zusätzlich unit-testbar in `FlowNRW.Tests`, im Gegensatz zu reinem UI-Code). `Metadata` ist der vorhandene, aussagekräftige Detailinhalt (Quelle, Datenstand, Fallback, Cache, Anbieterwarnung) — kein neuer Textbestand nötig. Fallback- und Stale-Fälle wandern gleichermaßen hinter das Symbol: sie sind dieselbe Informationsklasse („Datenlage nicht aktuell/vollständig"), der Kunde beanstandet die Aufdringlichkeit des Kanals, nicht den Einzelfall. Alternativen: Textzeile behalten und nur entschärfen (kleiner/dezenter) — erfüllt „darf nur durch Klick gezeigt werden" nicht; `StatusChip` statt Icon-Button — bleibt permanent sichtbarer Text, gleiches Problem. |
| Hinweis-Glyph | Neue Konstante `TransitIcons.Warning = "\uF083"` (`warning`, Material Symbols Rounded; Manifest-Z.4163 verifiziert), Färbung `Warning`/`WarningDark`. | `warning` (Dreieck) ist die Semantik „Achtung, Datenlage" — `info` würde neutral wirken (und ist bereits `MapInformation` belegt), `error` signalisiert einen harten Fehler, den der Fall ausdrücklich nicht ist (Daten sind da, ggf. nur unvollständig). Farbe nie alleiniger Träger: Symbolform + Beschreibung + Detail-Text. |
| Umfang des Symbol-Musters | Favoritenkarten **und** Abfahrtsmonitor (`DeparturePage.MonitorStatus`). | Beide Seiten erzeugen denselben Text aus derselben Bedingung; nur an einer Stelle zu ändern ließe den beanstandeten Hinweis im Monitor aufdringlich. Kleiner Zusatzaufwand, konsistente Bedienung. |
| Auf-/Zuklapp-Schalter (Punkt 3) | **Bestehenden Button verschieben** — `ToggleFavorite{index}` (inkl. `SetExpandIcon`-Beschreibungswechsel „Abfahrten aufklappen"/„einklappen") wird aus `BuildCardActions` in `BuildCardHeader` verlagert; Kopf-Grid wird `[Star, Auto, Auto]` (Titel / `FavoriteBusy` / Toggle). Die Aktionsleiste reduziert sich auf drei Spalten (Refresh/Open/Remove). | Genau **ein** Umschalter je Karte — keine Duplikation, kein divergierender Zustand. `AutomationId`, Glyph-Wechsel, 48×48 und Beschreibungs-Update bleiben identisch → alle `Click 'ToggleFavorite0'`-UI-Tests und `WindowsDesignUiTests`-Expand-Assertions laufen unverändert. Alternativen: zweiter Toggle im Kopf zusätzlich zum unteren (doppelte Pflege, zwei gleiche Ziele in der UIA-Hierarchie); `Switch`-Steuerelement (im Korrekturteil der Anforderung bereits als Symbol-Umschalter festgelegt). |
| Karten-Timeout (Punkt 4) | **Zweistufiger Watchdog:** (a) `map.js` — Fortschritts-Watchdog, der nach `init` bei >15 s ohne Kachel-Fortschritt (kein `done`/`tileunload`/neue `createTile`-Aktivität, solange `waiting.size > 0` oder `layer` noch nicht `load` gemeldet hat) einmalig `failed` sendet; zusätzlich Per-Tile-Timeout (~15 s je `waiting`-Eintrag → `done(Error)` + `cancel`-Nachricht ans Native). (b) `MapPage` — nativer Post-`init`-Watchdog: wenn nach `InitializeAsync` >30 s weder `loaded` noch `failed` ankommt, Status „Basiskarte konnte nicht geladen werden. Haltestellenliste und Zurück bleiben nutzbar." (gekoppelt an `lifetime`-Token). (c) Stille Pfade schließen: verworfene Kachelanfragen (`pending >= 64`, doppelte `id`) erhalten eine explizite `{type:'tile', data:'', error:false}`-Antwort, damit die JS-Seite die Kachel als Fehler abschließt statt ewig zu warten. (d) `Diagnostics.AppLog.Write` in `MessageReceived`/`InitializeAsync`/Tile-Pfad (Session, Kachelzähler, Fehler) für die iOS-Felddiagnose. | Der Ist-Stand hat zwischen `ready` und `loaded`/`failed` **kein** Limit — genau das gemeldete Symptom. Der Aktivitäts- (statt Gesamtzeit-)Ansatz straft langsame, aber fortschreitende Ladevorgänge nicht (relevant, weil `MapTileService` alle Kacheln über `SemaphoreSlim(1,1)` serialisiert). Da die iOS-Ursache unter Windows nicht reproduzierbar ist, ist der Watchdog die robuste Absicherung; `AppLog` macht den realen Fehler auf dem Gerät diagnostizierbar. Alternativen: nur nativer Timeout (lässt `map.js`-`waiting`-Einträge hängen — Status stimmt dann, aber die Karte selbst bleibt im WebView halbgeladen); nur JS-Timeout (deckt Bridge-/`SendAsync`-Probleme nicht ab — dann kommt nie eine Nachricht zurück). |
| Interne Kennungen entfernen (Punkte 5/6) | **Call-Sites explizit umstellen:** `HomePage.RenderNearby` und `SearchPage.Endpoint` erhalten `showTechnicalIdentity: false` (wie `MapPage`/`StopSearchPage` bereits). `TransitVisuals.Candidate`-Signatur und `JourneyPresentation.Address` bleiben unverändert. | Explizite Flags an den zwei betroffenen Stellen statt Default-Flip: der Parameter `showTechnicalIdentity: true` dokumentiert bewusst gewähltes Verhalten; ein Default-Wechsel würde den „technischen" Pfad für künftige Call-Sites unsichtbar umdefinieren. `JourneyPresentation.Address` bleibt als technische Formatierung erhalten (Diagnose/potenziell künftige Nutzung). Alternative im ViewModel formatieren (bereinigte `Address`-Projektion) verworfen: die Kennung gehört zum Datenmodell (`Address.Stop` bleibt nötig für Öffnen/Identität) — das Problem ist reine Anzeige. |
| `EndpointViewModel.Selection` (Punkt 6) | Neuer Helfer `JourneyPresentation.DisplayAddress(Address)` = `DisplayName(Name)` + optional ` · {Lat}, {Lon}` (Koordinaten **ohne** `Source`/`Id`/`Dhid`); `Selection` = „Ausgewählt: " + `DisplayAddress(...)`. | Die technische Kennung ist laut Kundenfeedback irrelevant und verschwindet; die Koordinaten bleiben, weil sie im Koordinatenmodus die **Nutzereingabe selbst** sind — und der UI-Test Z.724 (`'51.4556'` in `OriginSelection`) sie weiter erwartet. Nur Z.717 (`fixture-0.*de:05113`) ist anzupassen. Alternative „nur `Name`" würde Koordinaten-Endpunkte unkenntlich machen und Z.724 brechen. |
| Entfernung in Nahe-Liste (Punkt 5) | Neuer Helfer `FavoriteHomeViewModel.NearbyDistanceLabel(Address candidate)`: primär `NearbyStopResult.DistanceMeters` des Providers (`" · Entfernung: {de-DE,0} m"` bei endlichem Wert ≥0), Fallback Haversine über die bereits gesetzte `position` (`" · Luftlinie: {0} m"`), sonst `" · Entfernung unbekannt"`. `RefreshNearbyAsync` behält die gefilterten `NearbyStopResult`-Einträge in einem privaten Feld (Mapping per `ReferenceEquals(item.Stop, candidate.Stop)` wie `StopMonitorViewModel.DistanceLabel`). `NearbyStops` bleibt `IReadOnlyList<Address>` (Typ stabil). | Provider-Distanz ist der vorhandene Referenzweg (`StopMonitorViewModel.DistanceLabel`); der Haversine-Fallback schließt die Lücke für Provider ohne Distanzangabe (Fixture liefert genau diesen Fall: zweiter Treffer ohne `DistanceMeters`) ohne neue Standortabfrage — `position` ist zum Zeitpunkt der Projektion garantiert gesetzt. „Luftlinie"-Kennzeichnung beim Fallback hält die Metrik ehrlich unterscheidbar (gleiches Wording wie `FavoriteMonitorViewModel.DistanceLabel`). `Address` bleibt unverändert — kein Datenmodell-Eingriff. |
| Namensbereinigung „X, X Hbf" (Punkt 7) | Neuer Helfer `JourneyPresentation.DisplayName(string name)` — **exakte Regel:** Hat `name` die Form `"{prefix}, {rest}"` (erstes Komma, `prefix`/`rest` getrimmt, beide nicht leer) und beginnt `rest` mit `prefix` (Ordinal, case-sensitive) **gefolgt von einer Wortgrenze** (`rest.Length == prefix.Length` oder `rest[prefix.Length]` ∈ {`' '`, `'('`, `'-'`, `'/'`}), ist nur `rest` der Anzeigename; sonst `name` unverändert. Angewendet auf: `TransitVisuals.Candidate` (Anzeigetext + `SemanticProperties`-Basis, alle Trefferlisten), `JourneyPresentation.DisplayAddress` (→ `EndpointViewModel.Selection`), `MapStation.Label` (Kartenliste **und** Leaflet-Marker-Tooltips, `MapViewModel` Z.25–28), `JourneyPresentation.Event` (Haltestellen in Verbindungsdetails, Z.91), `JourneyTimelineView` (Z.32–33, 53) sowie `FavoriteMonitorViewModel.Title`/`StopMonitorViewModel.Title`. | „Danach nochmal genannt" wird präzise als Prefix-Match am Wortanfang des Rests definiert: „Münster (Westf), Münster (Westf) Hbf" → „Münster (Westf) Hbf"; „Bochum, Hbf" und „Essen, Borbeck Süd" bleiben unverändert; die Wortgrenze verhindert Fehlschnitte wie „Bergheim, Bergheimer Str." → „Bergheimer Str." (verboten: `'e'` ist keine Grenze). Zentral in `FlowNRW.Core/Presentation` platziert → direkt unit-testbar; Identität bleibt unberührt (`Stop.Name`, gespeicherte Favoriten, `Source`/`Id` unverändert, keine Store-Migration). Alternativen: Bereinigung schon in den Response-Mappern (verwirft Provider-Information und würde gespeicherte Namen mitschreiben — abgelehnt); nur Trefferlisten bereinigen (inkonsistent: derselbe Haltestellenname würde in Liste vs. Detail/Titel unterschiedlich lesen). |
| `NearbyDistance`-Sichtbarkeit für UI-Tests | `TransitVisuals.Candidate` erhält einen optionalen Parameter `detailsId` (AutomationId der `Secondary`-Detailzeile); `RenderNearby` übergibt `"NearbyDistance" + index`. | Ohne eigene AutomationId ist die `Secondary`-Zeile in der UIA-Hierarchie nicht einzeln addressierbar — die Entfernungsanzeige wäre nicht E2E-prüfbar. Optionaler Parameter: bestehende Call-Sites unberührt. |

## Programmabläufe

### Favoritenkarte — Refresh und zugeklappte Linien (Punkte 1–3)

1. `RefreshCoreAsync` (`FavoriteMonitorViewModel`): wie bisher — `Result` wird bei `ErrorCode is null` übernommen (komplett oder degradiert-aber-anzeigbar); `IsComplete(result)` → `Lines` **ersetzend** aus den Items berechnen und `DepartureCacheEntry` mit `Lines` persistieren (unverändert).
2. Neu: degradiertes, angezeigtes Ergebnis (`!IsComplete`, `ErrorCode is null`) → `Lines = Lines ∪ Linien(result.Items)` (dedupliziert, `StringComparer.Ordinal`, sortiert), `Notify(nameof(Lines))` — keine Persistenz-Schreibung außerhalb des `IsComplete`-Zweigs.
3. `HomePage.RenderFavoriteSummary` bleibt unverändert; der `summaryContainer` rendert die angereicherte `Lines`-Menge bei `!card.IsExpanded` automatisch (`Items`/`Lines`-Notify in `SubscribeCardChanges` Z.278).
4. Kartenkopf: `[Star FavoriteName{index} · Auto FavoriteBusy{index} · Auto ToggleFavorite{index}]` — Toggle-Command ruft `card.ToggleExpanded()` und `SetExpandIcon()` (Glyph + `SemanticProperties.Description` synchron wechselnd). `departures`/`summaryContainer`-Sichtbarkeit unverändert über `IsExpanded`-Notify.
5. Hinweis: `HasDataNotice && !IsBusy` → `FavoriteNotice{index}`-Button sichtbar (Glyph `Warning`, `Warning`/`WarningDark`-Textfarbe). Klick → `FavoriteMetadata{index}` (`card.Metadata`) ein-/ausblenden + Beschreibungswechsel („Hinweis zu den Daten anzeigen"/„Hinweis schließen"). `FavoriteStatus{index}` zeigt weiterhin „Keine nächsten"/„Letzter Stand"/„fehlgeschlagen"/„nicht geladen" — nicht mehr den Unvollständigkeits-Text.

### Abfahrtsmonitor — gleiches Muster (Punkt 2)

1. `StopMonitorViewModel.HasDataNotice` identische Ableitung aus `Result`.
2. `DeparturePage`: `MonitorNotice`-Button neben `MonitorStatus` im `monitorState`-Block; Toggle-Detail `MonitorMetadata` (`model.Metadata`). `UpdateMonitorStatus` verliert den „unvollständig"-Match — „Keine nächsten", „fehlgeschlagen", „nicht geladen", „abgebrochen", „Letzte bekannte"/„Datenstand" bleiben sichtbar.

### Karte — Ladepfad mit Watchdog (Punkt 4)

1. `MapPage.InitializeAsync` → „Basiskarte wird geladen …" wie bisher; zusätzlich nativer Watchdog-Task am `lifetime`-Token: nach 30 s ohne `loaded`/`failed` → „Basiskarte konnte nicht geladen werden. Haltestellenliste und Zurück bleiben nutzbar." + offene `tileRequests` abbrechen. `loaded`/`failed`/erneutes `init`/`OnDisappearing`/`ForegroundChanged` beenden den Watchdog (Session-/`lifetime`-Guard verhindert Fehlalarme).
2. `MessageReceived` `tile`-Pfad: `pending >= 64` oder doppelte `id` → statt stillem `return` eine `{type:'tile', id, data:'', error:false}`-Antwort senden (Kachel schließt JS-seitig mit `done(Error)` ab, `load` kann feuern). `AppLog.Write("map", …)` bei `init`/`ready`/`loaded`/`failed`/Tile-Fehlern (Session-Kürzel, `pending`, `id`).
3. `map.js`: `waiting`-Einträge erhalten Per-Tile-`setTimeout` (~15 s): Timeout → `waiting.delete`, `active`-Zähler korrigieren, `send({type:'cancel', id})` ans Native, `done(new Error('timeout'))`. Fortschritts-Watchdog nach `init`: alle 15 s prüfen — `waiting.size > 0` ohne Fortschritt **oder** `layer` hat nie `load` gefeuert → `failed=true`, `send({type:'failed'})` (einmalig, idempotent über vorhandenes `failed`-Flag; `session`-Filter in `receiveMap` bleibt).
4. Unverändert: `CheckStartupAsync` (`!ready` nach 15 s), alle Status-Texte außer dem neuen Timeout-Text, CSP/Tile-Gateway, `Map:*`-Konfiguration.

### Trefferlisten und Namen (Punkte 5–7)

1. `FavoriteHomeViewModel.RefreshNearbyAsync`: gefilterte `NearbyStopResult`s im privaten Feld `nearbyItems` halten; `NearbyStops`-Projektion unverändert. `NearbyDistanceLabel(candidate)`: `ReferenceEquals`-Match → `DistanceMeters` endlich ≥0 → ` · Entfernung: X m`; sonst `position` + `candidate.Stop?.Coordinate ?? candidate.Coordinate` vorhanden → ` · Luftlinie: X m` (Haversine `Distance`); sonst ` · Entfernung unbekannt`. Der Suffix wird nur in `RenderNearby` genutzt — die dort gerenderten Kandidaten stammen immer aus der gefilterten Liste; kein `ReferenceEquals`-Treffer oder fehlende Koordinate ergibt `" · Entfernung unbekannt"`.
2. `HomePage.RenderNearby`: `TransitVisuals.Candidate(candidate, button, model.NearbyDistanceLabel(candidate), showTechnicalIdentity: false, detailsId: "NearbyDistance" + index)` → sichtbare Detailzeile enthält nur noch die Entfernung.
3. `SearchPage.Endpoint`: `Candidate(candidate, …, showTechnicalIdentity: false)` — `{prefix}Match{i}`-Buttons zeigen nur noch `DisplayName(Name)`; `SemanticProperties.Description` = `DisplayName + suffix`.
4. `TransitVisuals.Candidate`: intern `displayName = JourneyPresentation.DisplayName(candidate.Name)`; `action.Text = displayName + suffix`; Beschreibung `false`-Pfad = `displayName + suffix`; `true`-Pfad = `JourneyPresentation.Address(candidate) + suffix` (technisch, unverändert); `details`-Extraktion prefixt auf den jeweils verwendeten Namen.
5. `EndpointViewModel.Selection` → `JourneyPresentation.DisplayAddress`: „Ausgewählt: {DisplayName} · {lat}, {lon}"; bei reiner Namensauswahl ohne Koordinate nur der bereinigte Name.
6. `MapStation.Label`, `JourneyPresentation.Event`, `JourneyTimelineView` (Z.32–33 origin/destination, Z.53 Umstieg), `FavoriteMonitorViewModel.Title`, `StopMonitorViewModel.Title` rendern `DisplayName(...)`. Speicher-/Identitätswege (`Stop.Name`, `Address.Name`, `SameIdentity`, Cache-Keys, Fixture-`Stop`-Objekte) bleiben unverändert.

## Neue Klassen

Keine. Alle Erweiterungen sind neue Member auf bestehenden Klassen (`JourneyPresentation`, `FavoriteMonitorViewModel`, `StopMonitorViewModel`, `FavoriteHomeViewModel`, `TransitIcons`, `TransitVisuals`) bzw. UI-Code in den Pages.

## Änderungen an bestehenden Klassen

### `JourneyPresentation` (`FlowNRW.Core/Presentation/JourneyPresentation.cs`, public static)

- **Neue Methoden:** `DisplayName(string? name)` → `string` (Bereinigungsregel aus Designentscheidungen: führendes `"{prefix}, "` entfernen, wenn `rest` mit `prefix` + Wortgrenze beginnt; `null`/leer/kommafrei → unverändert bzw. `string.Empty` bei `null`); `DisplayAddress(Address address)` → `string` (`DisplayName(address.Name)` + optional ` · {Lat:0.#####}, {Lon:0.#####}` — Koordinaten, aber keine `Source`/`Id`/`Dhid`).
- **Geänderte Methoden:** `Event` (Z.91) rendert `DisplayName(item.Identity.Stop.Name)`.
- `Address`, `Metadata`, `CompactWarning`, `Clock`, `EventTime` unverändert.

### `FavoriteMonitorViewModel` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`, public sealed)

- **Neue Eigenschaft:** `HasDataNotice` → `bool` (`Result is { } result && (result.Warnings.Count > 0 || result.IsFallback || result.IsStale)`); mitnotifiziert in `RefreshBindings` (Z.174–178).
- **Geänderte Methoden:** `RefreshCoreAsync` (Z.104–146) — im Erfolgs-Else-Zweig: `IsComplete(result)`-Pfad unverändert (ersetzendes `Lines` + Persistenz); zusätzlich `else`-Pfad für degradierte angezeigte Ergebnisse: `Lines` = Vereinigung vorhandener `Lines` mit Liniennamen aus `result.Items` (dedupliziert, sortiert), `Notify(nameof(Lines))`, **kein** `persist`-Aufruf.
- `Title` (Z.29) → `JourneyPresentation.DisplayName(Stop.Name)`.

### `StopMonitorViewModel` (`FlowNRW.Core/Presentation/StopMonitorViewModel.cs`, public sealed)

- **Neue Eigenschaft:** `HasDataNotice` → `bool` (identische Ableitung aus `Result`); Notify in `RefreshBindings` (Z.376–386).
- `Title` (Z.80) → `JourneyPresentation.DisplayName(SelectedStop.Name)`.
- `Status`-Texte unverändert (Z.289–291 bleibt „Daten möglicherweise unvollständig oder veraltet." — die Sichtbarkeit ändert sich nur in `DeparturePage`).

### `FavoriteHomeViewModel` (`FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`, public sealed)

- **Neues Feld:** `private NearbyStopResult[] nearbyItems` — gefilterte Provider-Ergebnisse aus `RefreshNearbyAsync` (Z.120–123).
- **Neue Methode:** `NearbyDistanceLabel(Address candidate)` → `string` (Suffix ` · Entfernung: X m` / ` · Luftlinie: X m` / ` · Entfernung unbekannt`; Regeln in Designentscheidungen; de-DE-Formatierung analog `StopMonitorViewModel.DistanceLabel`).

### `EndpointViewModel` (`FlowNRW.Core/Presentation/EndpointViewModel.cs`, public sealed)

- **Geänderte Eigenschaft:** `Selection` (Z.84–90) → `„Ausgewählt: " + JourneyPresentation.DisplayAddress(SelectedAddress)`.

### `MapViewModel` / `MapStation` (`FlowNRW.Core/Maps/MapViewModel.cs`)

- `MapStation.Label` (Z.25–28) → `JourneyPresentation.DisplayName(Candidate.Name)` + unverändertes „ · Keine Kartenposition vorhanden".

### `TransitIcons` (`FlowNRW/TransitIcons.cs`, internal static)

- **Neue Konstante:** `Warning = "\uF083"` — mit XML-Doc-Kommentar; Codepoint vor Umsetzung gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` (Zeile 4163) verifizieren.

### `TransitVisuals` (`FlowNRW/TransitVisuals.cs`, internal static)

- **Geänderte Methode:** `Candidate` — Signatur erhält optionalen `string? detailsId = null`; intern `displayName = JourneyPresentation.DisplayName(candidate.Name)`; `action.Text = displayName + suffix`; `false`-Beschreibung = `displayName + suffix`; `Secondary(details, detailsId)` nutzt die optionale Id.
- `SecondaryAction`, `StatusChip`, `ApplyRoles` unverändert.

### `HomePage` (`FlowNRW/HomePage.cs`, ContentPage)

- `BuildCardHeader` (Z.229–239): drei Spalten `[Star, Auto, Auto]`; `ToggleFavorite{index}`-Button samt `SetExpandIcon`-Logik hierher verlagert.
- `BuildCardActions` (Z.290–325): Toggle-Spalte entfernt → drei `Star`-Spalten (Refresh/Open/Remove); `unsubscribe`-Aufräumen für den Toggle-Command wandert mit.
- `BuildFavoriteCard` (Z.205–227): neuer Hinweisbereich — `FavoriteNotice{index}`-Button (48×48, `TransitIcons.Warning`, `SemanticProperties.Description` „Hinweis zu den Daten anzeigen", TextColor `Warning`/`WarningDark`, `IsVisible` initial `card.HasDataNotice && !card.IsBusy`) + `FavoriteMetadata{index}`-`Label` (Bindung an `card.Metadata`, initial unsichtbar); Button-Command togglet Detail + Beschreibung („Hinweis schließen").
- `SubscribeCardChanges` (Z.274–288): Notice-Sichtbarkeit bei `Status`/`Result`/`IsBusy`-Notify neu bewerten (`HasDataNotice && !IsBusy`); Detail-Bereich beim Card-Rebuild zurückgesetzt (Ansichtszustand ist seitenlokal, wie in `MapPage`).
- `ShowsFavoriteStatus` (Z.360–364): „unvollständig"-Match entfernt.
- `RenderNearby` (Z.327–339): `Candidate(candidate, button, model.NearbyDistanceLabel(candidate), showTechnicalIdentity: false, detailsId: "NearbyDistance" + index)`.
- `FavoriteCardViews`-Record (Z.271–272) um `Notice`-Button/Detail-Label erweitert.

### `DeparturePage` (`FlowNRW/DeparturePage.cs`, ContentPage)

- `monitorState`-Block (Z.89–91): `MonitorNotice`-Button (48×48, `TransitIcons.Warning`, Beschreibung „Hinweis zu den Daten anzeigen") + `MonitorMetadata`-`Label` (Bindung `model.Metadata`, initial unsichtbar, Toggle analog `MapPage`).
- `UpdateMonitorStatus` (Z.137–148): „unvollständig"-Match entfernt; `MonitorNotice.IsVisible = model.HasDataNotice && !model.IsBusy` (in `ModelChanged` auf `Status`/`IsBusy`/`Result`-Notify mitbewerten).
- `model.HasDataNotice`-Notify: `StopMonitorViewModel.RefreshBindings` erweitert (s. o.).

### `SearchPage` (`FlowNRW/SearchPage.cs`, ContentPage)

- `Endpoint(...)` Z.145: `Candidate(..., showTechnicalIdentity: false)`.
- `RenderFavorites` (Z.157–170): `Text`/`SemanticProperties.Description` der `{prefix}Favorite{id}`-Buttons über `JourneyPresentation.DisplayName(card.Stop.Name)` — nur Anzeige; `endpoint.SelectFavorite(card.Stop)` bekommt weiterhin den originalen `Stop`.

### `JourneyTimelineView` (`FlowNRW/JourneyTimelineView.cs`)

- Z.32–33 (`origin`/`destination`) und Z.53 (Umstieg-`transfer.Stop.Name`) über `JourneyPresentation.DisplayName(...)`.

### `MapPage` (`FlowNRW/MapPage.cs`, ContentPage)

- `InitializeAsync` (Z.136–157): nach dem `SendAsync(init)` den nativen Post-`init`-Watchdog starten (30 s, `lifetime`-Token, Session-Guard); `loaded`/`failed` in `MessageReceived` (Z.184–185) beenden ihn.
- `MessageReceived` `tile`-Zweig (Z.187–200): bei `pending >= 64 || tileRequests.ContainsKey(id)` explizite leere `tile`-Antwort senden statt stillem `return`; `AppLog.Write`-Diagnose bei `init`/`loaded`/`failed`/Tile-Fehlern.
- Neuer privater Watchdog-Task (z. B. `WatchLoadAsync`), Fehlertext: „Basiskarte konnte nicht geladen werden. Haltestellenliste und Zurück bleiben nutzbar." — AutomationId `MapStatus` unverändert.

### `map.js` (`FlowNRW/Resources/Raw/map/map.js`)

- Per-Tile-Timeout: `waiting`-Einträge bekommen `setTimeout` (~15 s) → `waiting.delete`, `active` korrigieren, `{type:'cancel', session, id}` senden, `done(new Error('timeout'))`.
- Fortschritts-Watchdog: nach `init` periodisch prüfen (Intervall/Timeout ~15 s, zurückgesetzt bei Kachel-Fortschritt); ohne Fortschritt mit `waiting.size > 0` oder ausbleibendem `load` → `failed = true; send({type:'failed', session})`; bei erneutem `init`/`resetting` zurückgesetzt.
- `leaflet.js`/`index.html`/CSP unverändert; JS-Dateien haben **kein** XML-Doc-Erfordernis.

## Datenbankmigrationen

Keine. `DepartureCacheEntry`-Schema, `IFavoriteStore`-Persistenz und alle gespeicherten Namen/Identitäten bleiben unverändert; `DisplayName` ist reine Anzeigeformatierung.

## Validierungsregeln

Keine neuen fachlichen Validierungen. Verbindliche Regeln aus `docs/help/design/business-rules.md`, die als Umsetzungs-DoD gelten:

- Symbolschalter 48×48 mit gesprochener `SemanticProperties.Description`; Zustandswechsel (Expand-Glyph, Notice-Detail) aktualisiert die Beschreibung im selben Aufruf.
- Farbe nie alleiniger Informationsträger: das `warning`-Symbol trägt Bedeutung über Glyph-Form + Beschreibung + aufklappbaren Text (nicht nur über die `Warning`-Färbung).
- Icon-Glyphen dürfen bei 150 % Textskalierung in den 48×48-Buttons nicht clippen (Sichtprüfung in der Design-Matrix).
- Keine erfundenen Daten: `Lines` nur aus gelieferten/persistierten Beständen; Entfernung nur aus gelieferter `DistanceMeters` oder berechneter Luftlinie (gekennzeichnet); `DisplayName` entfernt ausschließlich den wörtlich doppelten Präfix.

## Konfigurationsänderungen

Keine. `Map:*`-Einstellungen, `Info.plist` (keine ATS-Ausnahme nötig), `MauiProgram`-Registrierungen und `FlowNRW.csproj` bleiben unverändert; das `warning`-Glyph nutzt die bereits eingebettete `MaterialSymbolsRounded.ttf`.

## Seiteneffekte und Risiken

- **Gepinntes Verhalten ändert sich bewusst (Punkt 1):** `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` (`FavoriteDepartureCacheIntegrationTests.cs` Z.110–131) asserted aktuell `card.Lines == ["S1"]` nach degradiertem Ergebnis mit „S2" — mit Merge-Semantik wird `card.Lines == ["S1","S2"]`, während `cache.Entries[0].Lines` weiterhin `["S1"]` bleibt. Test anpassen (Assertion aufteilen: Anzeige gemerged, Persistenz komplett-only). `DepartureCacheStore`-Validierung und `Restore`-Pfad unberührt.
- **`Status`-String bleibt:** „Daten möglicherweise unvollständig oder veraltet." bleibt der ViewModel-Status (Semantik/Logging); nur `ShowsFavoriteStatus`/`UpdateMonitorStatus` blenden ihn nicht mehr als Zeile ein. Kein bekannter UI-Test asserted den Text auf `FavoriteStatus`/`MonitorStatus` (Assertion Z.472 trifft `StopSearchMetadata`/`CompactWarning` — unberührt); trotzdem in den E2E-Läufen auf `-notmatch`-Regressionen prüfen.
- **UI-Test-Konflikt (Punkt 6):** `WindowsJourneyUiTests.ps1` Z.717 asserted `'fixture-0.*de:05113'` in `OriginSelection`/`DestinationSelection` — muss auf die Kennungs-freie Darstellung angepasst werden (z. B. Name-Match + `-notmatch 'de:'`). Koordinaten-Assertionen Z.724 (`51.4556`/`13.405`) bleiben durch `DisplayAddress` erhalten.
- **Karten-Watchdog ist defensiv, nicht ursächlich (Punkt 4):** der iOS-Fehler ist unter Windows nicht reproduzierbar; der Watchdog verhindert den endlosen „wird geladen"-Zustand und `AppLog` macht die Ursache am Gerät diagnostizierbar — ein iOS-Geräte-/Simulatorlauf (Safari-Web-Inspector an der `HybridWebView`/WKWebView) bleibt für die Verifikation erforderlich und ist ausdrücklich **ausstehend** — er erfolgt manuell zu einem späteren Zeitpunkt und ist kein Umsetzungsblocker. Nebeneffekt: `map-slow`-Szenario (6 s/Kachel, seriell) kann den Fortschritts-Watchdog nur dann nicht auslösen, wenn er bei Kachel-Fortschritt zurückgesetzt wird — Rücksetz-Logik ist Pflichtbestandteil.
- **Festplatte der stillen Pfade:** die explizite leere `tile`-Antwort bei `pending >= 64`/Duplikat erzeugt JS-seitig `done(Error)` (Kachel-Fehler statt Dauer-Hänger) — bewusst `error:false`, damit einzelne Löcher nicht die ganze Basiskarte als `failed` markieren.
- **XML-Doc-Pflicht + Parser-Falle:** der `pre-commit`-Hook (`.githooks/csproj-xmldoc-check.py`) parst dokumentierte Member zeilenbasiert und zählt Klammern — **expression-bodied Member mit Klammern im `=>`-Body werden falsch geparst** (Deklaration läuft in die Folgezeile). Neue/geänderte Member daher als **Block-Bodies** schreiben (wie der vorhandene `get { … }`-Stil in `FavoriteMonitorViewModel.Metadata`) und `<summary>`/`<param>`/`<returns>` vollständig halten; zusätzlich `dotnet format --verify-no-changes` (Teil des Hooks) beachten.
- **Zeilenenden:** `.gitattributes`/`/.editorconfig` erzwingen LF (`*.cs`, `*.md`, `*.js`); `*.ps1` bleibt CRLF. Neue Dateien und Edits entsprechend schreiben.
- **A11y-/UIA-Vertrag:** bestehende `AutomationId`s (`ToggleFavorite{i}`, `FavoriteStatus{i}`, `FavoriteLines{i}`, `NearbyStop{i}`, `{prefix}Match{i}`, `{prefix}Selection`, `MapStatus`, `MonitorStatus`) und ihre `SemanticProperties.Description`-Semantik nicht umbenennen; neue Ids nur ergänzen (`FavoriteNotice{i}`, `FavoriteMetadata{i}`, `MonitorNotice`, `MonitorMetadata`, `NearbyDistance{i}`). `ToggleFavorite{i}` behält ControlType `Button`/InvokePattern — Positionswechsel in den Header ist für die UIA-Tests transparent.
- **Entfernungs-Metrik:** Provider-`DistanceMeters` (z. B. EFA-`properties.distance`) und Haversine-Luftlinie können abweichen — durch unterschiedliches Label („Entfernung" vs. „Luftlinie") ehrlich getrennt; kein Vermischen innerhalb eines Suffixes.
- **Namensbereinigungs-Reichweite:** `DisplayName` greift nur beim exakten Prefix-Match mit Wortgrenze; dennoch Anzeigestellen weit gefasst (Liste, Auswahl, Karte, Titel, Timeline). Risiko „zu viel bereinigt" besteht bei exotischen Provider-Namen wie „X, X-Y" (wird zu „X-Y") — bewusst akzeptiert, da der Rest den Ortsnamen weiter trägt.
- **150-%-Textskalierung:** zwei neue Icon-Buttons pro Favoritenkarte/Monitor — Glyph-Clipping in `WindowsDesignUiTests.ps1`-Matrix (150 %) prüfen.
- **UI-Tests brauchen interaktive Session:** die PowerShell-UIA-Skripte erfordern eine interaktive Windows-Session (`schtasks /it`); sie können nicht headless laufen — bei der Ausführung einplanen.
- **Scope-Schutz:** keine Provider-/Mapper-Änderungen, keine neuen Seiten, keine Store-/Cache-Schema-Änderungen; `JourneyPresentation.Address` (technisch) bleibt erhalten.

## Umsetzungsreihenfolge

1. **Core-Präsentation `JourneyPresentation`** — Voraussetzungen: keine. `DisplayName` + `DisplayAddress` mit Unit-Tests (`JourneyPresentationTests`).
2. **`FavoriteMonitorViewModel`** — `HasDataNotice` + `Lines`-Merge in `RefreshCoreAsync`; `Refresh_PersistsLineInventoryOnlyFromCompleteResponses` anpassen + neuer Merge-/Erstbefüllungs-Test.
3. **`StopMonitorViewModel`** — `HasDataNotice` (+ Test).
4. **`FavoriteHomeViewModel.NearbyDistanceLabel`** — `nearbyItems` halten + Label-Regeln (+ Tests).
5. **`EndpointViewModel.Selection`** auf `DisplayAddress` (+ `EndpointViewModelTests`-Ergänzung).
6. **`TransitIcons.Warning` + `TransitVisuals.Candidate`** — Konstante (Codepoint-Verifikation), `displayName`-Rendering, optionaler `detailsId`.
7. **`HomePage`** — Header-Toggle-Umzug, Notice-Button + `FavoriteMetadata`-Toggle, `ShowsFavoriteStatus` ohne „unvollständig", `RenderNearby` mit Suffix/`detailsId`.
8. **`DeparturePage`** — `MonitorNotice` + `MonitorMetadata`, `UpdateMonitorStatus` ohne „unvollständig".
9. **`SearchPage` + `MapStation.Label` + `JourneyPresentation.Event` + `JourneyTimelineView` + Titel-Anwendungen** — Kennung/`DisplayName` an allen Anzeigestellen.
10. **Karte `map.js` + `MapPage`** — Watchdogs, leere Tile-Antworten, `AppLog`-Diagnose.
11. **UI-Test-Fixture + Skripte** — `UiTestMapTiles` um Szenario `map-hang` (nie antwortende Kacheln) erweitern; `WindowsJourneyUiTests.ps1` Z.717 anpassen; neue Assertionen (`FavoriteNotice0`-Toggle → `FavoriteMetadata0` sichtbar, `NearbyDistance0` enthält „Entfernung", `map-hang` → Timeout-Status, `ToggleFavorite0` im Header weiterhin klickbar/Expand funktioniert).
12. **Vollständige E2E- und Test-Läufe** — `dotnet test FlowNRW.Tests`, alle vier UI-Test-Skripte (interaktive Session), `WindowsDesignUiTests.ps1`-Matrix prüfen (Notice-Icon, Header-Toggle, 150 % Skalierung).

## Tests

### Neue Tests

| Test | Testklasse / Datei | Was wird geprüft |
|---|---|---|
| `DisplayName_RemovesRepeatedPrefix` | `JourneyPresentationTests` (`FlowNRW.Tests/PresentationTests.cs`) | „Münster (Westf), Münster (Westf) Hbf" → „Münster (Westf) Hbf"; „Köln, Köln Hbf" → „Köln Hbf" |
| `DisplayName_KeepsNonRepeatedAndMissingComma` | `JourneyPresentationTests` | „Bochum, Hbf", „Essen Hbf", „" unverändert; `null` → `string.Empty` |
| `DisplayName_RequiresWordBoundary` | `JourneyPresentationTests` | „Bergheim, Bergheimer Str." bleibt unverändert (kein Grenzzeichen); „Essen, Essen-Steele" → „Essen-Steele" (`-` als Grenze) |
| `DisplayAddress_OmitsIdentityKeepsCoordinates` | `JourneyPresentationTests` | `Source`/`Id`/`Dhid` nicht enthalten, `Lat,Lon` enthalten; ohne Koordinate nur Name |
| `HasDataNotice_ReflectsDisplayedResult` | `FavoriteTests` / `StopMonitorTests` | `HasDataNotice` true bei `Warnings`/`IsFallback`/`IsStale` auf `Result`, false bei komplettem/`null`-`Result`; Fehlschlag (`SetFailure`, `Result` komplett vorhanden) → abhängig vom angezeigten Board |
| `Refresh_DegradedResultEnrichesDisplayedLines` | `FavoriteDepartureCacheIntegrationTests` | Karte ohne je komplette Antwort: degradiertes Ergebnis befüllt `Lines` aus seinen Items; bestehendes Inventar wird gemerged, nicht verworfen; Cache bleibt ohne `IsComplete` unbeschrieben |
| `NearbyDistanceLabel_PrefersProviderFallsBackToAirline` | `FavoriteTests` (o. ä., `ControlledLocation`/`ControlledNearby`-Doubles) | Provider-`DistanceMeters` → „Entfernung: X m"; fehlende Provider-Distanz mit Position/Koordinate → „Luftlinie: X m"; ohne beides → „Entfernung unbekannt" |
| `Selection_OmitsTechnicalIdentity` | `EndpointViewModelTests` | `Selection` enthält Namen, nicht `Source`/`Id`/`Dhid`; Koordinaten bei Koordinaten-Endpunkt weiterhin enthalten |

### Betroffene bestehende Tests

- `FavoriteDepartureCacheIntegrationTests.Refresh_PersistsLineInventoryOnlyFromCompleteResponses` (`FlowNRW.Tests` Z.110–131): `card.Lines`-Assertion auf Merge-Erwartung (`["S1","S2"]`) anpassen; Cache-Assertion (`["S1"]`) bleibt — die Testaussage zur **Persistenz** ändert sich nicht.
- `LocationTests` (`NearbyUsesActualPositionAndOpensSameMapStop`, `NearbyFailureRetainsProvenanceAndEmptySuccessClears`): betreffen `StopMonitorViewModel.DistanceLabel` — unverändert gültig; die neue `FavoriteHomeViewModel`-Variante bekommt eigene Tests.
- `EndpointViewModelTests`: `Selection`-String ändert sich (kein `Address(...)` mehr) — vorhandene Tests asserted die Kennung nicht direkt, müssen aber auf grün geprüft werden.
- `WindowsJourneyUiTests.ps1` Z.717: Kennungs-Assertion `'fixture-0.*de:05113'` in `OriginSelection`/`DestinationSelection` → auf Namens-Match + Negativ-Assertion umstellen. Übrige `FavoriteLines{far}`/`FavoriteStatus{far}`/`ToggleFavorite{i}`-Assertionen (Z.150–156, 231–278) müssen **unverändert** grün bleiben (gleiche AutomationIds, Merge füllt nur leere Bestände).
- `WindowsDepartureCacheUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsRefreshUiTests.ps1`: `FavoriteLines`/`FavoriteStatus`/`ToggleFavorite0`-Assertionen (u. a. Z.104–261) — unverändert grün erwartet; der Toggle sitzt nur an anderer visueller Position.
- `WindowsDesignUiTests.ps1` Z.294–295: `ToggleFavorite0`-Klick + Captures bleiben gültig; Screenshot-Matrix ändert sich visuell (Header-Toggle, Notice-Icon, bereinigte Namen, Entfernungszeile) — Referenzbilder neu bewerten.
- `MapTests` / `MapTileService`-Tests: nativer Teil unberührt — grün erwartet (Watchdog ist `MapPage`/`map.js`, nicht `MapTileService`).

### E2E-Tests

- **`WindowsJourneyUiTests.ps1` (angepasst + erweitert):** Z.717-Konflikt auflösen; neue Checks — `FavoriteNotice{far}` sichtbar bei degradiertem Standard-Szenario, Klick zeigt `FavoriteMetadata{far}` mit „Quelle:"-Provenienz und verbirgt sie wieder; `FavoriteStatus{far}` zeigt bei degradiertem Ergebnis **kein** „unvollständig" mehr (`-notmatch`); `ToggleFavorite{far}` im Kartenkopf auf/zu wie bisher; `NearbyDistance0` enthält „Entfernung" bzw. „Luftlinie"; `{prefix}Match{i}`/Nahe-Buttons zeigen keine `de:`-Kennung; `MapStatus` nach `map-hang` endet im Timeout-/Fehlertext statt „wird geladen".
- **`WindowsDepartureCacheUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsRefreshUiTests.ps1`:** vollständige Regression (Lines-Inventar, Expand-Toggle, Status-Texte).
- **`WindowsDesignUiTests.ps1`:** Favoritenkarte mit Header-Toggle + Notice-Icon in allen Matrix-Kombinationen (430×900/1024×768, hell/dunkel — Warning-Färbung prüfen, 150 % Skalierung — Glyph-Clipping prüfen); Referenzbilder neu aufnehmen.
- **Ausführungsbedingung:** UI-Tests benötigen eine interaktive Windows-Session (`schtasks /it`-Mechanik wie bisher); der `UiTest`-Build (`UI_TEST_FIXTURES`) muss das neue `map-hang`-Szenario enthalten.
- **iOS-Verifikation Punkt 4:** nicht auf Windows abdeckbar — Geräte-/Simulatorlauf mit `AppLog`-Auswertung ist separater Nachweis und erfolgt manuell zu einem späteren Zeitpunkt (Entscheidung: defensiver Fix wird umgesetzt, Verifikation ausstehend — siehe Seiteneffekte und Risiken).

## Offene Punkte

Keine.
