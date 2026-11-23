# UI-Logik und Render-Methoden

Methoden der designrelevanten Klassen, plus abonnierte Events. Sichtbarkeit: `internal` (assembly-intern) bzw. `public`/`private` wie im Code.

## `TransitVisuals` (`FlowNRW/TransitVisuals.cs`, internal static)

Zentrale Helfer, die von fast allen Pages/Views verwendet werden.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `Candidate(Address, Button, string suffix, bool showTechnicalIdentity)` | internal static | Haltestellen-Listeneintrag: `Border` (Padding 14) mit Aktions-Button (transparenter Hintergrund, Ink-Töne via `SetAppThemeColor`) + optionalem `Secondary`-Detailtext; `SemanticProperties.Description` = `JourneyPresentation.Address(candidate)` + suffix (Z. 9–22) |
| `Page(VerticalStackLayout)` | internal static | Seitenrahmen: `MaxWidth` 840, `ScrollView`, responsives Padding (24 ab Breite 640, sonst 16), ruft `ApplyRoles` (Z. 24–32) |
| `ApplyRoles(Element)` | internal static | Rekursive Rollenvergabe: Labels deren `AutomationId` „Metadata" enthält, auf „Status" endet oder `RefreshIntervalStatus` ist → FontSize 13 + Muted-Farben; Buttons (außer `SearchJourneys`, `FindStops`, `RefreshDepartures`, `SaveRefreshSettings`, `OpenJourneySearch`) → Sekundär-Look `#EEEDF3`/`#2C2C2E` BG + `#0058BC`/`#A8C8FF` Text (Z. 34–49) |
| `Text(string, double size = 17, bool bold, string? id)` | internal static | `Label`-Fabrik mit FontSize/FontAttributes/AutomationId/`WordWrap` (Z. 51–61) |
| `Secondary(string, string? id)` | internal static | `Text(...)` mit FontSize 13 + Muted-Farben `#414755`/`#C5C7D0` (Z. 63–68) |
| `SecondaryAction(string icon, string description, string automationId, ICommand)` | internal static | 48×48-`Button` mit **`Text = icon` (Unicode-Glyph)**, `SemanticProperties.Description` (Z. 70–83) |
| `Badge(string line, string? mode)` | internal static | Linienbadge: `Border` (Padding 8,4, `CornerRadius` 7, ohne Stroke), `BackgroundColor` aus `DeparturePresentation.BadgeColor`, weißes `Label` FontSize 13 bold, `TailTruncation` (Z. 85–107) |

## Karten-Views

### `DepartureCardView` (`FlowNRW/DepartureCardView.cs`, public sealed, `: Border`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `DepartureCardView(StopEvent item, string id, bool compact = false)` | public ctor | Karte: Padding 12 (compact) / 16; `Grid`-Kopf mit `Badge` + Ziel (`Text` 17 bold, `id`) + Zeitblock rechts; Hauptzeit 22 bold mit `FontFamily` Consolas/Menlo, `Strikethrough` bei `Cancelled`; Soll-Zeit durchgestrichen bei Abweichung; „Fahrt fällt aus" (15 bold); „Gleis/Steig: …· geplant …" via `Secondary`; compact: Datum bei Tageswechsel; `SemanticProperties.Description` = `DeparturePresentation.Describe(item)` (Z. 14–65) |
| `Clock(DateTimeOffset?)` | private static | `HH:mm` Europe/Berlin oder „unbekannt" (Z. 67–72) |

### `JourneyCardView` (`FlowNRW/JourneyCardView.cs`, public sealed, `: Border`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `JourneyCardView(Journey, string id, ICommand)` | public ctor | Padding 16, Spacing 12; Zeitspanne `Text` 24 bold; „… Min. · n Umstiege" (15); `FlexLayout` mit `Badge` je Leg („Fußweg" ohne Mode); „Ist: …" via `Secondary` (`{id}Realtime`); Datum `Secondary` (`{id}Date`); „Ausfall gemeldet" (15 bold); Button „Verbindung öffnen" (`id`, `SemanticProperties.Description` = `JourneyPresentation.Summary`); `ApplyRoles` (Z. 16–42) |
| `Detach()` | public | `open.Command = null` vor Entfernen (Z. 45) |
| `Clock(DateTimeOffset?)` | internal static | `HH:mm` Europe/Berlin oder „Unbekannt" (Z. 47–48) |

### `JourneyTimelineView` (`FlowNRW/JourneyTimelineView.cs`, public sealed, `: VerticalStackLayout`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `JourneyTimelineView(Journey)` | public ctor | Spacing 16; Kopfkarte („Deine Verbindung" 22 bold `JourneyDetailSection0`, Zeit 28 bold, Dauer via `Secondary`); je Leg eine `Border`-Karte (`JourneyTimeline{index}`): `Grid`-Kopf `Badge` + Start→Ziel (`JourneyDetailSection{index}`, `JourneyLine{index}` am Badge-Label), `Event`-Zeilen Abfahrt/Ankunft, Fußweg (`JourneyWalk{index}`) bzw. Betreiber (`JourneyOperator{index}`); Umstiegskarten dazwischen (Z. 11–57) |
| `Event(string title, StopEvent, DateTimeOffset? reference)` | private static | `Grid` Titel 15 bold + rechtsbündige Ist-Zeit via `JourneyPresentation.EventTime`; Soll-Zeit 12pt durchgestrichen bei Abweichung; „Fahrt fällt aus"; „Bahnsteig: …· geplant …" (Z. 59–84) |

## Pages — Render- und Statusmethoden

### `HomePage` (`FlowNRW/HomePage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `RenderCards()` | private | Baut Favoritenkarten neu (Z. 197–291): `Border` (Padding 16) je Karte mit Titel 22 bold (`FavoriteName{i}`), `FavoriteBusy{i}`-Indikator, `FavoriteDistance{i}`, `FavoriteStatus{i}` (konditional sichtbar via `ShowsFavoriteStatus`), horizontalem Linien-`Badge`-Scroll (`FavoriteLines{i}`), bis zu 5 `DepartureCardView` (compact), Aktionsgrid (4× `Star`) mit `⌃`/`⌄`-Toggle (`ToggleFavorite{i}`), `↻` (`RefreshFavorite{i}`), `▣` (`OpenFavorite{i}`), `☆` (`RemoveFavorite{i}`) — alle 48×48 mit `SemanticProperties.Description`; danach `ApplyRoles` |
| `RenderNearby()` | private | `TransitVisuals.Candidate`-Einträge `NearbyStop{i}` (Z. 293–305) |
| `RenderSummary()` / `RenderDepartures()` | lokale Funktionen in `RenderCards` | Badge-Summary bzw. Abfahrtsliste einer Karte |
| `AddLabel(...)` | private static | Gebundenes Label mit AutomationId, Default-FontSize 14 (Z. 307–311) |
| `ShowsHomeStatus` / `ShowsLocationStatus` / `ShowsNearbyStatus` / `ShowsFavoriteStatus` | private | Substring-Regeln für Statussichtbarkeit (Z. 313–330) |
| `OnAppearing` / `OnDisappearing` / `ForegroundChanged` / `ReconcileRefreshLoops` / `RefreshMissingAsync` / `RefreshAtStartupAsync` / `SettingsChanged` / `Navigate` | protected/private | Lifecycle-, RefreshLoop- und Resume-Logik (Z. 100–195) — kein Visuals-Fokus, bleibt erhalten |

### `DeparturePage` (`FlowNRW/DeparturePage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `RenderItems()` | private | `DepartureCardView`-Liste `Departure{index}` (Z. 207–214) |
| `RefreshFavorite()` | private | `favoriteButton.Text` „★ Entfernen"/„☆ Speichern" + Semantic-Beschreibung (Z. 199–205) |
| `UpdateMonitorStatus()` / `UpdateFavoriteStatus()` | private | Substring-Regeln `MonitorStatus`/`FavoriteToggleStatus` (Z. 135–157) |
| Aktionsgrid im Konstruktor | — | `SecondaryAction("↻", "RefreshDepartures")`, `favoriteButton` (`ToggleFavorite`), `SecondaryAction("⚙", "OpenRefreshSettings")` (Z. 61–68) |

### `SearchPage` (`FlowNRW/SearchPage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `Endpoint(EndpointViewModel, string prefix, string title)` | private | Eingabekarte (`Border` Padding 12): Titel 17 bold, `Entry`, `⌕`-Lookup-Button (`{prefix}Search`), `⌖`-Standort (`{prefix}Location`), Koordinaten-`Switch` + Felder, Trefferliste via `TransitVisuals.Candidate` (`{prefix}Match{i}`), Favoritenliste (`{prefix}Favorites`), Status-/Metadatenlabels; Moduswechsel setzt `lookup.Text` `⌕`↔`✓` (Z. 108–179) |
| `RenderConnectionFavorites()` | lokale Funktion | `Button`-Liste gespeicherter Verbindungen (`ConnectionFavorite…`, Höhe 44) (Z. 74–87) |
| Konstruktor-Anteile | — | `SecondaryAction("⇄", "SwapEndpoints")`, Zeitpunkt-Block (`RoutingTimeMode`/`RoutingDate`/`RoutingTime`/`RoutingArriveBy`), `SearchJourneys`-Primary-Button (Z. 45–95) |

### `StopSearchPage` (`FlowNRW/StopSearchPage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `RenderMatches()` | lokale Funktion | `TransitVisuals.Candidate` mit `JourneyPresentation.Address` + `DistanceLabel`-Text, `StopMatch{index}` (Z. 66–81) |
| `RefreshInformation()` / `ShowsNearbyStatus` / `ShowsSearchStatus` / `SearchWarning` | lokal/private | Status-/Warnsichtbarkeit, `JourneyPresentation.CompactWarning` (Z. 82–122) |
| Konstruktor-Anteile | — | Suchkarte: `Entry StopQuery`, `⌕` `FindStops`, `⌖` `FindNearbyStops`, `SecondaryAction("⌖", "ShowStopMap")` (Z. 37–52) |

### `ResultsPage` (`FlowNRW/ResultsPage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `RenderJourneys()` | private | `JourneyCardView`-Liste `Journey{i}` mit `Detach()` beim Neuaufbau (Z. 98–108) |
| `RenderConnectionHeader()` | private | `ConnectionHeader` „Origin → Destination", `favoriteConnection.Text` `★`/`☆` + Semantic-Beschreibung (Z. 118–127) |
| `ToggleConnectionAsync` / `CanToggleConnection` | private | Verbindungsfavorit (`ToggleConnectionFavorite`, `SecondaryAction("☆", …)`) |

### `JourneyDetailPage` (`FlowNRW/JourneyDetailPage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `RenderDetails()` | private | `JourneyTimelineView` bei `SelectedJourney`, sonst Text-`Border`-Abschnitte `JourneyDetailSection{index}` FontSize 18 (Z. 100–113) |
| Konstruktor-Anteile | — | `SecondaryAction("⌖", "ShowJourneyMap")`, `DetailStatus`/`DetailMetadata` (Z. 35–43) |

### `MapPage` (`FlowNRW/MapPage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `InitializeAsync` / `SendAsync` / `Points` / `MessageReceived` | private | JSON-Bridge zur `HybridWebView` (`MapCanvas`, Höhe max(320, 55 % Seitenhöhe)), Marker/Segmente/Tile-Pipeline (Z. 136–204) |
| Konstruktor-Anteile | — | `SecondaryAction("↻", "ResetMap")`, `ⓘ`-Info-Button (`MapInformation`), `Liste`/`Karte`-Toggle (`ShowMapList`/`ShowMapCanvas`), Stationsliste via `TransitVisuals.Candidate` (`MapStation{i}`), `MapAttribution` (Z. 47–88) |

### `RefreshSettingsPage` (`FlowNRW/RefreshSettingsPage.cs`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ModelChanged` / `ShowsStatus` / `OnAppearing` / `OnDisappearing` | private/protected | `RefreshInterval`-Picker ↔ `SelectedSeconds`, `SaveRefreshSettings`, Diagnosekarte (`LoggingEnabled`, `SendLog`, `LogStatus`), `RefreshIntervalStatus` (Z. 25–121) |

## Core-Präsentationshelfer (farb-/textgebend, vom Design genutzt)

### `DeparturePresentation` (`FlowNRW.Core/Presentation/DeparturePresentation.cs`, public static)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `BadgeColor(string line, string? mode)` | public static | Hex-Farbe je Provider-`mode` (rail→`#BA1B1D`, suburban→`#007A39`, subway→`#005A9C`, tram→`#B33F00`, bus→`#6F2C91`, ferry→`#00777F`), Fallback Namenspräfix RE/RB→`#BA1B1D`, S→`#007A39`, U→`#005A9C`, sonst `#414755` (Z. 13–36) |
| `Describe(StopEvent)` | public static | Mehrzeiliger Accessibility-Text: Linie→Ziel, Ausfallstatus, Soll/Ist, Verspätung, Gleis/Steig Soll/Aktuell + Wechselmarker, Betreiber (Z. 41–62) |

### `JourneyPresentation` (`FlowNRW.Core/Presentation/JourneyPresentation.cs`, public static)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `Time(DateTimeOffset?)` | public static | `dd.MM.yyyy HH:mm 'UTC'zzz` Europe/Berlin oder „unbekannt" |
| `EventTime(DateTimeOffset?, DateTimeOffset?)` | public static | `HH:mm` mit Vortag-/Folgetag-/Datumsmarker relativ zur Referenz |
| `Address(Address)` | public static | Disambiguierender Kandidatentext (Name · Source Id Dhid · Koordinaten) |
| `Metadata<T>(ProviderResult<T>)` | public static | Quelle/Datenstand/Datenalter + Fallback-/Stale-/Warnhinweise |
| `CompactWarning<T>(ProviderResult<T>?)` | public static | „Daten möglicherweise unvollständig" / „Zwischengespeicherte Daten" / „Ersatzquelle" |
| `Summary(Journey)` / `Detail(Journey)` | public static | Verbindungs-Zusammenfassung / Volltext (Soll/Ist, Umstiege, Legs, Betreiber, Bahnsteig, Echtzeitquelle) |

## Abonnierte Events (design-relevant)

| Abonnent | Event | Reaktion |
|---|---|---|
| `HomePage` | `model.PropertyChanged` (`FavoriteHomeViewModel`) | `RenderCards`, Status-Sichtbarkeiten, `RenderNearby`, Command-Refresh (Z. 65–79) |
| `HomePage` | `card.PropertyChanged` (`FavoriteMonitorViewModel`) | `RenderDepartures`/`RenderSummary`, Expand-Sichtbarkeit (Z. 246–256) |
| `HomePage` | `model.PropertyChanged` (`IsSaving`) | `remove.InvalidateExecution()` (Z. 275–278) |
| `HomePage`/`DeparturePage`/`ResultsPage`/`JourneyDetailPage`/`MapPage`/`SearchPage`/`StopSearchPage` | `foreground.PropertyChanged` (`ForegroundState`) | Refresh-Pause/Resume, `CancelPending` |
| `DeparturePage` | `model.PropertyChanged`, `favorites.PropertyChanged`, `settings.Changed` | `RenderItems`, `RefreshFavorite`, Statusupdates |
| `StopSearchPage` | `model.PropertyChanged` | `RenderMatches`, `RefreshInformation` |
| `SearchPage` | `endpoint.PropertyChanged`, `connectionFavorites.PropertyChanged`, `text.Focused/Unfocused` | Matches-/Favoritenlisten, `lookup.Text`-Glyphwechsel |
| `ResultsPage`/`JourneyDetailPage` | `model.Session.PropertyChanged` | `RenderJourneys`/`RenderDetails`, `RenderConnectionHeader` |
| `RefreshSettingsPage` | `model.PropertyChanged` | Picker-Enable, Statussichtbarkeit |
| `MapPage` | `map.RawMessageReceived` (`HybridWebView`) | `ready`/`view`/`select`/`loaded`/`failed`/`cancel`/`tile`-Nachrichten |
| `TransitVisuals.Page` | `scroll.SizeChanged` | Responsives Padding 24/16 (Z. 29) |

Es werden keine Events publiziert; Benachrichtigungen laufen über `ObservableObject`/`INotifyPropertyChanged` und `AsyncRelayCommand.Refresh()`/`InvalidateExecution()`.
