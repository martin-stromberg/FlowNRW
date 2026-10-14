# Glyph-Verwendung (Icon-Ersetzung)

Vollständige Liste aller Unicode-Textglyphen, die aktuell als „Icons" in `Button.Text` dienen. Es existiert keine Icon-Font, kein `FontImageSource`, kein `Image`-/`ImageButton`-Icon und keine zentrale Icon-Definition — alle Symbole sind Literal-Strings an den genannten Fundstellen.

Alle Buttons tragen `SemanticProperties.Description` (deutschsprachige Beschreibung) und eine stabile `AutomationId`, die von den UI-Test-Skripten referenziert wird.

## Zentrale Fabrik

`TransitVisuals.SecondaryAction(icon, description, automationId, command)` (`TransitVisuals.cs` Z. 70–83): `Button` mit `Text = icon`, 48×48, `Padding` 0. Sekundär-Optik kommt aus `TransitVisuals.ApplyRoles` (alle Buttons außer der Primary-Whitelist erhalten `#EEEDF3`/`#2C2C2E`-Hintergrund und `#0058BC`/`#A8C8FF`-Text).

## Glyph-Tabelle

| Glyph | Zweck | Fundstelle | AutomationId | SemanticProperties.Description | Konstruktion |
|---|---|---|---|---|---|
| `＋` | Haltestelle hinzufügen (Home-Shortcut) | `HomePage.cs` Z. 50 | `OpenHomeStops` | „Haltestelle hinzufügen" | `TransitVisuals.SecondaryAction` |
| `⌖` | Favoriten auf Karte zeigen | `HomePage.cs` Z. 51 | `HomeMap` | „Favoriten auf Karte zeigen" | `TransitVisuals.SecondaryAction` |
| `↻` | Entfernungen aktualisieren | `HomePage.cs` Z. 52 | `SortFavorites` | „Entfernungen aktualisieren" | `TransitVisuals.SecondaryAction` |
| `⚙` | Aktualisierungs-Einstellungen öffnen | `HomePage.cs` Z. 53 | `OpenRefreshSettings` | „Aktualisierung einstellen" | `TransitVisuals.SecondaryAction` |
| `⌃` / `⌄` | Favoritenkarte auf-/einklappen | `HomePage.cs` Z. 260, 262 | `ToggleFavorite{index}` | „Abfahrten einklappen" / „Abfahrten aufklappen" | `Button` 48×48, Text wechselt dynamisch |
| `↻` | Favoriten-Abfahrten aktualisieren | `HomePage.cs` Z. 265 | `RefreshFavorite{index}` | „Abfahrten aktualisieren" | `Button` 48×48, `card.RefreshCommand` |
| `▣` | Abfahrtsmonitor öffnen | `HomePage.cs` Z. 279 | `OpenFavorite{index}` | „Abfahrtsmonitor öffnen" | `Button` 48×48 |
| `☆` | Favorit entfernen | `HomePage.cs` Z. 283 | `RemoveFavorite{index}` | „Favorit entfernen" | `Button` 48×48 |
| `↻` | Abfahrten aktualisieren (Monitor) | `DeparturePage.cs` Z. 62 | `RefreshDepartures` | „Abfahrten aktualisieren" | `TransitVisuals.SecondaryAction`; in `ApplyRoles`-Whitelist (behält Primary-Optik) |
| `⚙` | Aktualisierungs-Einstellungen (Monitor) | `DeparturePage.cs` Z. 63 | `OpenRefreshSettings` | „Aktualisierung einstellen" | `TransitVisuals.SecondaryAction` |
| `☆` / `★` | Haltestelle als Favorit speichern/entfernen | `DeparturePage.cs` Z. 61, 202 | `ToggleFavorite` | „Als Favorit speichern" / „Favorit entfernen" | `Button` mit Text „☆ Speichern"/„★ Entfernen" (Glyph + Text) |
| `⇄` | Start und Ziel vertauschen | `SearchPage.cs` Z. 48 | `SwapEndpoints` | „Start und Ziel vertauschen" | `TransitVisuals.SecondaryAction` |
| `⌕` | Endpunkt suchen | `SearchPage.cs` Z. 115 | `{prefix}Search` (`OriginSearch`, `DestinationSearch`) | „{title} suchen" | `Button` 48×48 |
| `✓` | Koordinaten übernehmen (Moduswechsel) | `SearchPage.cs` Z. 129 | `{prefix}Search` (wechselt) | „{title} Koordinaten übernehmen" / „{title} suchen" | dynamischer Textwechsel `⌕`↔`✓` |
| `⌖` | Mein Standort für Endpunkt | `SearchPage.cs` Z. 117 | `{prefix}Location` (`OriginLocation`, `DestinationLocation`) | „{title} Mein Standort" | `Button` 48×48 |
| `⌕` | Haltestelle suchen | `StopSearchPage.cs` Z. 46 | `FindStops` | „Haltestelle suchen" | `Button` 48×48; in `ApplyRoles`-Whitelist (Primary-Optik) |
| `⌖` | Haltestellen in meiner Nähe | `StopSearchPage.cs` Z. 48 | `FindNearbyStops` | „Haltestellen in meiner Nähe" | `Button` 48×48 |
| `⌖` | Haltestellen auf Karte zeigen | `StopSearchPage.cs` Z. 50 | `ShowStopMap` | „Haltestellen auf Karte zeigen" | `TransitVisuals.SecondaryAction` |
| `☆` / `★` | Verbindung speichern/entfernen | `ResultsPage.cs` Z. 35, 124 | `ToggleConnectionFavorite` | „Verbindung speichern" / „Gespeicherte Verbindung entfernen" | `TransitVisuals.SecondaryAction`, Text wechselt in `RenderConnectionHeader` |
| `⌖` | Verlauf auf Karte öffnen | `JourneyDetailPage.cs` Z. 35 | `ShowJourneyMap` | „Verlauf auf Karte öffnen" | `TransitVisuals.SecondaryAction` |
| `↻` | Karte neu laden | `MapPage.cs` Z. 47 | `ResetMap` | „Karte neu laden" | `TransitVisuals.SecondaryAction` |
| `ⓘ` | Quellen- und Kartendaten ein-/ausblenden | `MapPage.cs` Z. 55, 60 | `MapInformation` | „Quellen und Kartendaten" / „Kartendaten schließen" | `Button` 48×48 mit `RelayCommand`-Toggle |

## Weitere symbolische Texte (keine Icon-Buttons)

| Stelle | Inhalt | Anmerkung |
|---|---|---|
| `DepartureCardView.cs` Z. 49 | „Fahrt fällt aus" (15 bold) + `Strikethrough` auf Hauptzeit | Status rein textuell, kein Symbol; Farbe aus `Label`-Default (Ink) — kein Semantikrot |
| `JourneyCardView.cs` Z. 36 | „Ausfall gemeldet" (15 bold) | ebenfalls rein textuell |
| `JourneyTimelineView.cs` Z. 76 | „Fahrt fällt aus" | ebenfalls rein textuell |
| `DepartureCardView.cs`/`JourneyTimelineView` | „Gleis/Steig: …", „Bahnsteig: …" | `Secondary`-Texte ohne Symbol |
| `AppShell.xaml.cs` Z. 18–20 | Tab-Titel „Abfahrten"/„Verbindungen"/„Haltestellen" | Tabs ohne `Icon`-Property |
| `AppShell.xaml.cs` Z. 27–33 | `ToolbarItem` „Haltestellen" (`OpenStopSearch`) | Text statt Icon in der Toolbar |
| `RefreshSettingsPage.cs` Z. 46 | „Protokoll senden" (`SendLog`) | Text-Button |

## Referenzierung durch UI-Tests

`tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1` u. a. klicken die AutomationIds `OpenHomeStops`, `SortFavorites`, `OpenRefreshSettings`, `ToggleFavorite{index}`, `OriginSearch`, `OriginLocation`, `SwapEndpoints`, `FindStops`, `ShowStopMap`, `ToggleConnectionFavorite`, `ShowJourneyMap` direkt — die IDs dürfen beim Icon-Umbau nicht umbenannt werden; sichtbarer `Button.Text` wird in Tests nicht geprüft, wohl aber `SemanticProperties.Description` (erscheint als `Name` in UIA, u. a. `Contains`-Assertions).
