# Icon- und Farbinventar (`FlowNRW/TransitIcons.cs`, `FlowNRW/TransitVisuals.cs`)

Relevant für Punkt 2 (kleines Hinweis-Symbol) und Punkt 3 (Schalter im Kartenkopf). Alle Glyphen stammen aus der eingebetteten Schrift **Material Symbols Rounded** (FILL0, opsz 24, wght 400); Codepoints sind gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` verifiziert (Kommentar `TransitIcons.cs` Z.3–9). Schriftregistrierung: `MauiProgram.cs` Z.34–35 (`MaterialSymbols` = `MaterialSymbolsRounded.ttf`, `MaterialSymbolsFilled` = `MaterialSymbolsRoundedFill.ttf`).

## Definierte Glyphen-Konstanten (`TransitIcons`)

| Konstante | Codepoint | Glyph-Name | Verwendung |
|---|---|---|---|
| `Add` | U+E145 | `add` | „Haltestelle hinzufügen" (`HomePage` Z.50) |
| `MyLocation` | U+E55C | `my_location` | Standort/Nahe (`SearchPage` Z.121, `StopSearchPage` Z.49) |
| `Refresh` | U+E5D5 | `refresh` | Aktualisieren, `ResetMap` |
| `Settings` | U+E8B8 | `settings` | Aktualisierungs-Einstellungen |
| `ExpandLess` | U+E5CE | `expand_less` | `ToggleFavorite` eingeklappt-Zustand (`HomePage` Z.296) |
| `ExpandMore` | U+E5CF | `expand_more` | `ToggleFavorite` ausgeklappt-Zustand |
| `Star` | U+F09A | `star` (FILL1 via `FilledFontFamily`) | gefüllter Favoriten-Stern |
| `StarOutline` | U+F09A | `star` (FILL0) | Umriss-Stern, „Favorit entfernen"/nicht gespeichert |
| `Search` | U+EF7A | `search` | Such-Buttons |
| `Check` | U+E668 | `check` | Koordinaten-Übernahme (`SearchPage` Z.132) |
| `SwapVert` | U+E8D5 | `swap_vert` | Start/Ziel tauschen |
| `Info` | U+E88E | `info` | `MapInformation`-Toggle (`MapPage` Z.56) |
| `Map` | U+E55B | `map` | Kartenaktionen (`HomePage` Z.51, `StopSearchPage` Z.50) |
| `Schedule` | U+EFD6 | `schedule` | Verspätungs-Chip (`DepartureCardView` Z.48) |
| `Cancel` | U+E888 | `cancel` | Ausfall-Chip (`DepartureCardView` Z.46, `JourneyTimelineView` Z.71) |
| `Error` | U+F8B6 | `error` | Ausfall-Chip in Verbindungsliste (`JourneyCardView` Z.36) |
| `DepartureBoard` | U+E576 | `departure_board` | Tab-Icon + `OpenFavorite` (`HomePage` Z.317) |
| `Route` | U+EACD | `route` | `ShowJourneyMap` (`JourneyDetailPage` Z.35), Tab-Icon |
| `LocationOn` | U+F1DB | `location_on` | Tab-Icon „Haltestellen" |

**Für Punkt 2 relevant:** `Info` (U+E88E) und `Error` (U+F8B6) sind definiert; `warning` (U+F083, Manifest Z.4163) ist im Font-Manifest vorhanden, aber **keine** `TransitIcons`-Konstante. Weitere in Manifest vorhandene, unbenutzte Kandidaten: `error_outline`/`error_circle_rounded` (U+F8B6, Z.1344/1343), `info_i` (U+F59B, Z.1988), `chat_info` (U+F52B, Z.778), `data_info_alert` (U+F7F5, Z.1031).

## API-Oberflächen

| API | Datei/Zeile | Beschreibung |
|---|---|---|
| `SetIcon(Button, glyph, description?, filled, size=24)` | `TransitIcons.cs` Z.84–91 | Setzt `Text`/`FontFamily`/`FontSize`, `Padding=0`, optional `SemanticProperties.Description` |
| `IconImage(glyph, filled, size=20)` | Z.101–111 | `FontImageSource` in `Action`/`ActionDark`-Farbe; für Buttons mit Text (`DeparturePage` Z.204) |
| `TabIcon(glyph)` | Z.118–121 | `IconImage` mit `size:24` |
| `TransitVisuals.SecondaryAction(icon, description, automationId, command)` | `TransitVisuals.cs` Z.177–188 | 48×48-Icon-Button inkl. `SetIcon`+Beschreibung |
| `TransitVisuals.IconLabel(glyph)` | Z.140–149 | 16 pt Label in `TransitIcons.FontFamily`; Farbe durch Aufrufer |
| `TransitVisuals.StatusChip(glyph, text, light, dark)` | Z.157–175 | `Border`-Kapsel (Icon 16 + Text 13 bold), Hintergrund = Farbe mit 0,14/0,22 Alpha |
| `TransitVisuals.Badge(line, mode?)` | Z.190–212 | Linienbadge, Farbe via `DeparturePresentation.BadgeColor` |

## Semantikfarben (`TransitVisuals`)

| Token | Hell | Dunkel | Zweck |
|---|---|---|---|
| `Action`/`ActionDark` | `#0058BC` | `#A5C9FF` | Interaktion (Z.11–15) |
| `Ink`/`InkDark` | `#1A1B1F` | `#F5F5F7` | Primärtext (Z.19–23) |
| `Muted`/`MutedDark` | `#414755` | `#C5C7D0` | Sekundärtext, auch `*Status`-Labels via `ApplyRoles` (Z.27–31, 92–98) |
| `GroupStrong`/`GroupStrongDark` | `#EEEDF3` | `#2C2C2E` | Button-Flächen (Z.35–39) |
| `Success`/`SuccessDark` | `#006E28` | `#3ECF6B` | Bestätigung (Z.43–47) |
| `Warning`/`WarningDark` | `#894D00` | `#F0A840` | Warnung — aktuell nur Verspätungs-Chip (Z.51–55) |
| `Error`/`ErrorDark` | `#BA1A1A` | `#FF897D` | Fehler — aktuell nur Ausfall-Chip (Z.59–63) |

## Vorhandenes Muster für „kleines Symbol + Klick → Details"

`MapPage.cs` Z.50–61: `information`-Container (`IsVisible=false`) + `MapInformation`-Button (`TransitIcons.Info`, 48×48, `SemanticProperties.Description` wechselt „Quellen und Kartendaten" ↔ „Kartendaten schließen"). Exakt das Interaktionsmuster, das Punkt 2 beschreibt — bereits im Code vorhanden.

## Design-Regeln (Ist)

`docs/help/design/business-rules.md`: Bedienziele ≥44×44, Symbolschalter 48×48 mit gesprochener Beschreibung; Zustandswechsel aktualisiert die Beschreibung. „Farbe ist nie alleiniger Informationsträger" — Status tragen Symbol **plus** Text (`StatusChip`). Icon-Glyphen dürfen bei 150 % Textskalierung nicht clippen (Nachweis `docs/help/design/verification/icon-matrix-run.md`).
