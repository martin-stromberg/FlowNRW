# Bestandsaufnahme Detail: Views und Überschriften

Alle Views liegen unter `FlowNRW/` und werden in C# (nicht XAML) aufgebaut.
Überschriften entstehen über `TransitVisuals.Text(text, size, bold, id)` bzw. direkte
`Label`-Elemente. Der Fenstertitel wird über `Title` (teilweise gebunden) gesetzt und
auf iOS in der Shell-Navigationsleiste angezeigt.

## `MapPage` (`FlowNRW/MapPage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | `model.Title` → „Favoritenkarte" / „Haltestellenkarte" / „Verbindungsverlauf" (`MapViewModel.cs:42,58,71,85`) | Fenstertitel |
| Überschrift Z. 40 | `TransitVisuals.Text(model.Title, 28, true)` — kein `AutomationId` | **Redundant (identisch)** — Ticket-Beispiel „Favoritenkarte" |
| `TransitVisuals.Text("Haltestellenliste", 20, true)` Z. 73 | Abschnittsüberschrift der alternativen Listenansicht | Nicht redundant, bleibt |

## `SearchPage` (`FlowNRW/SearchPage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | „Verbindung suchen" (Z. 27) | Fenstertitel |
| Überschrift Z. 29 | `TransitVisuals.Text("Verbindungen", 32, true)` — kein `AutomationId` | **Redundant (ähnlich)** |
| `TransitVisuals.Secondary("Plane deine Fahrt …")` Z. 30 | Untertitel/Beschreibung | Kein Titel-Duplikat, bleibt |
| `TransitVisuals.Text("Wohin möchtest du fahren?", 20, true)` Z. 46 | Abschnittsüberschrift der Eingabekarte | Nicht redundant, bleibt |
| `TransitVisuals.Text("Zeitpunkt", 17, true)` Z. 54 | Abschnittsüberschrift | Bleibt |

## `StopSearchPage` (`FlowNRW/StopSearchPage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | „Haltestelle suchen" (Z. 22) | Fenstertitel |
| Überschrift Z. 35 | `TransitVisuals.Text("Haltestellen", 32, true)` — kein `AutomationId` | **Redundant (ähnlich)** |
| `TransitVisuals.Secondary("Suche eine Station …")` Z. 36 | Untertitel/Beschreibung | Bleibt |
| `TransitVisuals.Text("Haltestelle finden", 20, true)` Z. 39 | Abschnittsüberschrift der Suchkarte | Nicht redundant, bleibt |

## `DeparturePage` (`FlowNRW/DeparturePage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | gebunden an `model.Title` (Stationsname, Z. 42) | Fenstertitel |
| `stop`-`Label` Z. 45 | `FontSize = 28`, fett, `AutomationId = "MonitorStop"`, gebunden an `model.Title` | Identischer Text, aber **dynamischer Dateninhalt**: identifiziert die überwachte Haltestelle; von `docs/design/acceptance.md` („Abfahrten/Home und Einzelmonitor: klare große Überschrift") gefordert; `MonitorStop` ist Seitenerkennungs- und Inhaltsmarker der Windows-UI-Testsuite (über 20 Verwendungen). Bleibt erhalten. |

## `HomePage` (`FlowNRW/HomePage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | „Meine Haltestellen" (Z. 42) | Fenstertitel |
| `TransitVisuals.Text("Nächste Haltestellen", 22, true, "NearbyHeading")` Z. 62 | Abschnittsüberschrift über der Umgebungsliste | Anderer Inhalt, bleibt |
| `TransitVisuals.Text(card.Title, 22, true, "FavoriteName"+i)` Z. 243 | Favoritenname je Karte | Dynamischer Inhalt, bleibt |

## `ResultsPage` (`FlowNRW/ResultsPage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | „Verbindungen" (Z. 30) | Fenstertitel |
| `connectionHeader` (`ConnectionHeader`, 20 pt) Z. 33 | „Start → Ziel" der Suche | Dynamischer Inhalt, kein Titel, bleibt |

## `JourneyDetailPage` (`FlowNRW/JourneyDetailPage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | „Verbindungsdetails" (Z. 28) | Fenstertitel |
| — | Keine statische Seitenüberschrift vorhanden | Nicht betroffen |

## `RefreshSettingsPage` (`FlowNRW/RefreshSettingsPage.cs`)

| Element | Wert | Bewertung |
|---------|------|-----------|
| `Title` | „Automatische Aktualisierung" (Z. 24) | Fenstertitel |
| `TransitVisuals.Text("Abfahrten automatisch laden", 22, true)` Z. 71 | Abschnittsüberschrift der Intervall-Karte | Erklärt die Einstellung (mehrere Karten auf einer Seite), bleibt |
| `TransitVisuals.Text("Diagnose", 22, true)` Z. 74 | Abschnittsüberschrift | Bleibt |

## Hilfen

### `TransitVisuals` (`FlowNRW/TransitVisuals.cs`)

- `Text(string text, double size = 17, bool bold = false, string? id = null)` erzeugt
  `Label` mit Inter-Schrift; wird für alle Überschriften verwendet.
- `Page(VerticalStackLayout)` umhüllt das Seitenlayout mit `ScrollView` und ruft
  `ApplyRoles` auf (färbt Status-/Metadaten-Labels, thematisiert Buttons).
- Entfernte Überschriften hinterlassen keine Aufrufer-Referenzen; `Text` bleibt für
  Abschnittsüberschriften in Verwendung.

### `MapViewModel` (`FlowNRW.Core/Maps/MapViewModel.cs`)

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `Title` | `string` | „Haltestellenkarte" (Standard), „Favoritenkarte" (`ShowFavorites`), „Verbindungsverlauf" (`ShowJourney`) |

`Title` wird von `MapPage` sowohl als `Title` als auch aktuell als In-Page-Überschrift
verwendet; die Eigenschaft selbst bleibt unverändert.
