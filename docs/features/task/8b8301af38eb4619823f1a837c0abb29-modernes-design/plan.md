# Umsetzungsplan: Modernes Design auf AAA-Niveau

## Übersicht

FlowNRW erhält einen visuellen Qualitätssprung auf Basis des vorhandenen, gegen `docs/design/acceptance.md` abgenommenen Token-Systems — **Nachschärfung, kein Ersatz** (Anwenderentscheidung): vorhandene Tokens und die Schritt-9-Abnahme bleiben verbindlich, nur identifizierte Lücken werden geschlossen. Vier Arbeitsschwerpunkte: (1) **echte Iconografie** — alle Unicode-Textglyphen (`＋ ⌖ ↻ ⚙ ⌃ ⌄ ▣ ☆ ★ ⌕ ✓ ⇄ ⓘ`) in Aktionsbuttons werden durch die Icon-Font **Material Symbols Rounded (FILL0, opsz 24)** ersetzt, zentral über eine neue Klasse `TransitIcons` gesteuert, plus `FontImageSource`-Tab-Icons in `AppShell`; (2) **klarere Listeneinträge** — `DepartureCardView`, `JourneyCardView`, `JourneyTimelineView`, `TransitVisuals.Candidate` und die Favoritenkarten in `HomePage.RenderCards()` erhalten Status-Chips (Icon + Text) und einheitliches Spacing; (3) **lizenzfreie Typografie** — Inter (SIL OFL) als UI-Schrift und JetBrains Mono (OFL) für tabellarische Live-Zeiten ersetzen Systemschrift bzw. `Consolas`/`Menlo`; (4) **Token-Nachschärfung** — fehlende Semantikfarben (`#006E28`/`#894D00`/`#BA1A1A`) und `GroupStrong` (`#EEEDF3`) werden als echte Tokens nachgezogen, hartcodierte Hexwerte in `TransitVisuals` vereinheitlicht. Der UI-Test-/A11y-Vertrag (`AutomationId`, `SemanticProperties.Description`, InvokePattern) bleibt unverändert; ViewModels und Datenlogik bleiben unberührt.

„AAA-Niveau" wird als Qualitätsanspruch gegen den verbindlichen Referenzrahmen `docs/design/acceptance.md` + `docs/design/reference/trans_nrw_mobil_system/DESIGN.md` umgesetzt und über die vollständig neu aufgenommene Screenshot-Matrix (`WindowsDesignUiTests.ps1`) plus eine separate visuelle Abnahme nachgewiesen. Plattformziel ist primär Windows (Matrix neu belegen); iOS wird über die bestehende Checkliste abgedeckt.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|---|---|---|
| Verhältnis zur Schritt-9-Abnahme | **Nachschärfung** desselben Designsystems — alle bestehenden Token- und Abnahmeentscheidungen bleiben verbindlich; geschlossen werden nur die Lücken: Semantikfarben als Tokens, `GroupStrong`, explizite lizenzfreie Schrift | Anwenderentscheidung. `acceptance.md` erlaubt externe Schrift („keine externe Schrift erforderlich" = zulässig, nicht verboten); die neue Anforderung fordert explizit „moderne lizenzfreie Typografie". Kein Re-Theme, kein Verwerfen der Abnahme. |
| „AAA-Niveau" / Nachweis | `docs/design/acceptance.md` + `DESIGN.md` bleiben der verbindliche Referenzrahmen; der Qualitätssprung wird über die **erneute Screenshot-Matrix** (alle `acceptance.md`-Zeilen) **plus separate visuelle Abnahme** nachgewiesen | Anwenderentscheidung; es gibt keine neue Designvorlage/Mockups. Deckt sich mit der Freigaberegel in `acceptance.md` (unabhängige visuelle Prüfung, Ablage `docs/help/design/verification`). |
| Icon-Quelle | **Icon-Font** „Material Symbols **Rounded**", Variante **FILL0, opsz 24** (Apache License 2.0) als TTF in `FlowNRW/Resources/Fonts`, Alias `MaterialSymbols`, Glyphen als Konstanten in `TransitIcons`. Icon-only-Buttons bleiben `Button` mit `FontFamily="MaterialSymbols"` + Glyph-`Text`. Codepoints werden beim Import gegen die `.codepoints`-Datei der exakten heruntergeladenen TTF-Version verifiziert. | Anwenderentscheidung zur Font-Variante: Rounded passt zur weichen/minimalistischen Formsprache, FILL0/opsz 24 ist die Referenz-Konfiguration. Ein einziges Font-Asset statt dutzender SVG/PNG-`MauiImage`-Dateien; Iconfarbe folgt `TextColor` und wird damit automatisch über `SetAppThemeColor`/`AppThemeBinding` hell/dunkel-gefärbt (SVG-MauiImages sind statisch eingefärbt und bräuchten Theme-Varianten); `Button` bleibt `Button` → ControlType.Button und InvokePattern in den UIA-Testskripten bleiben garantiert erhalten (`ImageButton`-Umbau entfällt). Alternativen: Fluent UI System Icons (MIT, ebenfalls TTF-fähig, aber weniger dokumentierter Codepoint-Katalog), eingebettete SVGs (keine Theme-Färbung ohne Mehrfach-Assets, ControlType-Wechsel), eigene Images (Aufwand + kein Vorteil). |
| Icon + Text-Buttons | `Button.ImageSource` = `FontImageSource` (`FontFamily="MaterialSymbols"`, `Size` ~20, `Color` per `SetAppThemeColor`) neben normalem `Text` | Stellen wie `ToggleFavorite` („★ Entfernen"/„☆ Speichern", `DeparturePage.cs` Z. 202) mischen Symbol und Wort in einem `Text`; `FontFamily` gilt für den gesamten String, daher Icon als `ImageSource` + Text in Inter. `FontImageSource.Color` ist thembar (`BindableObject`-Extension). |
| Schriftwahl | **Inter** (SIL OFL 1.1): `Inter-Regular.ttf`/`Inter-SemiBold.ttf`/`Inter-Bold.ttf` als Aliase `Inter`/`InterSemiBold`/`InterBold`; **JetBrains Mono** (SIL OFL): `JetBrainsMono-Bold.ttf` als `JetBrainsMono` für Live-Zeiten | Deckt sich exakt mit der verbindlichen Referenz (`DESIGN.md` Typografie: Inter-Skala, JetBrains Mono für tabellarische Zeiten) und erfüllt „moderne lizenzfreie Typografie" explizit. Alternativen: Systemschrift (von `acceptance.md` zugelassen, aber kein sichtbarer Typografie-Schritt), OpenSans-Ausbau (lizenzfrei, aber generischer MAUI-Template-Look, nicht Referenz-konform), kommerzielle Schriften (nicht lizenzfrei). |
| Bold-Rendering mit Inter | `TransitVisuals.Text(..., bold: true)` setzt `FontFamily="InterBold"` statt `FontAttributes.Bold`; XAML-Styles setzen `FontFamily="Inter"` | MAUI registriert pro `AddFont` eine einzelne Familie; `FontAttributes.Bold` auf `Inter` würde synthetisches Fett erzeugen. Dedizierte Schnitt-Aliase sind plattformsicher. |
| Token-Struktur | Bestehende `Colors.xaml` erweitern um `GroupStrong` (`#EEEDF3` hell / `#2C2C2E` dunkel) und `Success`/`Warning`/`Error` (`#006E28`/`#894D00`/`#BA1A1A` + kontrastgerechte Dunkelvarianten); C#-Fabriken in `TransitVisuals` lesen dieselben Werte über zentrale Konstanten | Eine Tokenquelle, Werte verbindlich aus `acceptance.md`. Alternative externes UI-Toolkit (z. B. Material/Uranium) verworfen: schwere Abhängigkeit, bricht die bestehende visuelle Abnahme und die C#-basierte UI-Erzeugung. |
| UI-Erzeugung | Prozedurale C#-Views und `TransitVisuals`-Fabriken bleiben; keine XAML-Portierung | Anforderung betrifft Visuals, nicht Architektur; XAML-Umbau wäre ein nicht geforderter Großumbau. |
| Statusdarstellung | Status-Chip = Icon-Glyph + Text: Ausfall → `cancel`/`error` + „Fahrt fällt aus" in `Error`; Verspätung → `schedule` + „+n Min." in `Warning`; Semantik aus `DeparturePresentation.Status()`/`DelayText()` (neu in `FlowNRW.Core`) | `acceptance.md`: Farbe nie alleiniger Informationsträger, Semantikfarben verbindlich. Nur gelieferte Daten (`Realtime.Cancelled`, `Realtime.Delay`, `Realtime.ActualTime` vs `PlannedTime`) — keine erfundenen Echtzeitwerte. |
| Umfang der Symbolik | **Tab-Icons ja**: `Tab.Icon` via `FontImageSource` (`departure_board`, `route`, `location_on`) bei unveränderten Titeln. **Linienbadges bleiben Text-Badges** (VDV-Konvention, keine Symbolfunktion). **Kartenmarker bleiben Leaflet** (`HybridWebView`, unberührt). **`ToolbarItem` „Haltestellen" bleibt Text** (Plattformkonvention). Aktions-/Statussymbole werden überall sonst durch echte Icons ersetzt | Anwenderentscheidung. Referenzdesign sieht beschriftete Icons in der Tab-Bar vor; Titel und Routen bleiben → Tab-/Navigations-Tests unberührt. Begrenzung vermeidet Scope-Creep in Karten-/Provider-Bereiche. |
| Plattformziel | **Primär Windows**: komplette Screenshot-/Bedienmatrix (430×900 + 1024×768, hell/dunkel, 150 % Textskalierung) wird neu belegt. **iOS** wird über die **bestehende Checkliste** aus `acceptance.md` abgedeckt (kein neuer lokaler Gerätebeleg) | Anwenderentscheidung; keine neuen Plattformanforderungen. Der gesamte Umbau ist plattformneutral (Fonts/Tokens/Glyphen), nur der Nachweis läuft Windows-seitig. |
| Spacing-/Typo-Skala | Vorhandenes 4/8/12/16/24-Raster und die Referenz-Skala (Titel 28–34, Abschnitt 20–22, Text 17, Nebeninfo 15, Meta 13) vereinheitlichen; Ausreißer normalisieren (`ResultsStatus` 24→15, `JourneyDetailSection`-Fallback 18→17) | Kleinere Abweichungen zur Skala, kein Layout-Umbruch; erhöht visuelle Konsistenz ohne neue Komponenten. |

## Programmabläufe

### Font- und Icon-Auflösung beim App-Start

1. **Asset-Import (einmalig, vor dem Build):** TTFs werden in der exakten Varianten-Version bezogen — Material Symbols Rounded mit `FILL0, opsz 24`; die mitgelieferte `.codepoints`-Datei (bzw. das Release-Manifest) dieser TTF-Version wird gesichert. Die geplanten `TransitIcons`-Konstanten werden **gegen diese `.codepoints`-Datei verifiziert** (Glyphname → Codepoint), nicht aus Webdokumentation kopiert.
2. `MauiProgram.CreateMauiApp` → `ConfigureFonts` registriert zusätzlich zu den bestehenden OpenSans-Aliassen: `Inter-Regular.ttf` (`Inter`), `Inter-SemiBold.ttf` (`InterSemiBold`), `Inter-Bold.ttf` (`InterBold`), `JetBrainsMono-Bold.ttf` (`JetBrainsMono`), `MaterialSymbolsRounded.ttf` (`MaterialSymbols`).
3. `FlowNRW.csproj` bleibt unverändert — `MauiFont Include="Resources\Fonts\*"` (Z. 67) bindet neue TTFs automatisch ein.
4. `App.xaml` lädt unverändert `Colors.xaml` + `Styles.xaml`; die neuen Semantik-Tokens stehen allen Styles zur Verfügung.
5. Implizite Styles (`Label`, `Button`, `Entry`, `Picker`, `Editor`, `SearchBar` u. a.) setzen `FontFamily="Inter"`; `TransitVisuals.Text`/`Secondary` wählen `Inter`/`InterBold`; `DepartureCardView` setzt `JetBrainsMono` für die Hauptzeit.

### Aktionsbutton mit echtem Icon

1. Aufrufer übergeben eine `TransitIcons`-Konstante (z. B. `TransitIcons.Refresh`) statt eines Unicode-Literals — die `SecondaryAction`-Signatur `(string icon, string description, string automationId, ICommand)` bleibt kompatibel.
2. `TransitVisuals.SecondaryAction` erzeugt `Button` mit `FontFamily="MaterialSymbols"`, `Text`=Glyph-Codepoint, `FontSize` 24 (Referenz-opsz), `HeightRequest`/`WidthRequest` 48, `Padding` 0; `AutomationId` und `SemanticProperties.Description` unverändert.
3. `TransitVisuals.ApplyRoles` färbt Sekundär-Buttons weiterhin (`GroupStrong`-Hintergrund, `Primary`-Textfarbe) — die Iconfarbe ist die `TextColor` und folgt damit automatisch dem Thema.
4. Dynamische Zustandswechsel gehen über `TransitIcons.SetIcon(button, glyph)` + gleichzeitigem `SemanticProperties.SetDescription`: `⌕`↔`✓` (`SearchPage` `lookup`), `⌃`/`⌄` (`HomePage` `ToggleFavorite{i}`), `☆`/`★` (`DeparturePage.RefreshFavorite`, `ResultsPage.RenderConnectionHeader`).

### Listeneintrag-Rendering (Abfahrten und Favoriten)

1. `HomePage.RenderDepartures`/`DeparturePage.RenderItems` erzeugen `DepartureCardView` wie bisher (max. 5 Einträge je Favoritenkarte bleiben).
2. `DepartureCardView` behält die Kopfstruktur (Badge + Ziel + Zeitblock), ersetzt `Consolas`/`Menlo` durch `JetBrainsMono` (18 bold, Referenzwert) und ergänzt eine Statuszeile via `DeparturePresentation.Status(item)`: `Cancelled` → `TransitVisuals.StatusChip(error-Glyph, "Fahrt fällt aus", Error)`; `Delayed` → `StatusChip(schedule-Glyph, DelayText, Warning)` neben durchgestrichener Soll-Zeit; `Scheduled` (keine Echtzeit) → dezenter `Secondary`-Hinweis.
3. Gleis/Steig-Text und `SemanticProperties.Description` = `DeparturePresentation.Describe(item)` bleiben unverändert.
4. `JourneyCardView` ersetzt das reine „Ausfall gemeldet"-Label durch denselben `StatusChip`; `JourneyTimelineView.Event` analog für „Fahrt fällt aus".
5. `TransitVisuals.Candidate` und die Favoritenkarten in `HomePage.RenderCards` erhalten vereinheitlichtes Spacing (Kartenabstand 12, Innenabstand 16) und die Skala-Rollen (Titel 22 `InterSemiBold`/`InterBold`, Meta 13 Muted).

### Shell-Navigation mit Tab-Icons

1. `AppShell`-Konstruktor setzt `Icon` auf jedem `Tab` über `TransitIcons.TabIcon(glyph)` (`FontImageSource`, `Color` per `SetAppThemeColor` auf `Primary`/`PrimaryDark` bzw. Muted-Töne).
2. `Title`/`Route`/`AutomationId`s unverändert → Navigation und Tests unberührt. `ToolbarItem` „Haltestellen" (`OpenStopSearch`) bleibt bewusst ein Text-Eintrag.

### Theme-Wechsel

1. `UserAppTheme`-Wechsel (systemgeführt bzw. `FLOWNRW_UI_TEST_THEME`) löst die vorhandenen `AppThemeBinding`s/`SetAppThemeColor`s neu auf; Icon-Glyphen in `Button.Text` und `FontImageSource.Color` färben mit.
2. Kein neuer Theme-Mechanismus; kein Codepfad liest `AppInfo.RequestedTheme` neu.

## Neue Klassen

| Klasse | Typ | Zweck |
|---|---|---|
| `TransitIcons` | `internal static class` (FlowNRW, z. B. `FlowNRW/TransitIcons.cs`) | Zentrale Icon-Definition: `const string`-Glyph-Codepoints (Material Symbols Rounded FILL0: `Add`, `MyLocation`, `Refresh`, `Settings`, `ExpandLess`, `ExpandMore`, `OpenInNew`, `Star`, `StarOutline`/`StarBorder`, `Search`, `Check`, `SwapVert`, `Info`, `Map`, `Schedule`, `Cancel`/`Error`, `DepartureBoard`, `Route`, `LocationOn`), `FontFamily`-Alias-Konstante, `SetIcon(Button, glyph)`-Helfer (Glyph + Beschreibung konsistent setzen), `TabIcon(glyph)`-/`FontImageSource`-Fabrik. Alle Codepoints werden beim Import gegen die `.codepoints`-Datei der eingebetteten TTF verifiziert |
| `DepartureStatus` | `enum` (`FlowNRW.Core/Presentation`) | Statussemantik einer Abfahrt: `Scheduled` (keine Echtzeit), `OnTime`, `Delayed`, `Cancelled` — Basis für Symbol + Text + Semantikfarbe |

## Änderungen an bestehenden Klassen

### `TransitVisuals` (internal static class)

- Neue Methoden: `StatusChip(string glyph, string text, Color light, Color dark)` (Icon-`Label` + Text-`Label` in horizontaler Reihe/Kapsel; Farbe nie alleiniger Träger), `IconLabel(string glyph)` (kleiner Helfer für inline Icons).
- Geänderte Methoden: `SecondaryAction` (Glyph statt Unicode-Text: `FontFamily="MaterialSymbols"`, `FontSize` 24, sonst unverändert), `Text`/`Secondary` (`FontFamily` `Inter`/`InterBold` je `bold`-Flag), `ApplyRoles` (hartcodierte `#EEEDF3`/`#2C2C2E`/`#0058BC`/`#A8C8FF`-Literale über zentrale Konstanten bzw. neue Tokens, Verhalten identisch).
- Neue Events/Event-Handler: keine.

### `DeparturePresentation` (public static class, FlowNRW.Core)

- Neue Methoden: `Status(StopEvent)` → `DepartureStatus` (Ableitung aus `Realtime.Cancelled`, `Realtime.Delay`/`ActualTime` vs `PlannedTime`), `DelayText(StopEvent)` → `string?` („+n Min." nur bei echtem Delay, sonst `null`).
- Geänderte Methoden: keine (`BadgeColor`, `Describe` unverändert).
- Neue Eigenschaften/Events: keine.

### `DepartureCardView` (public sealed class : Border)

- Geänderte Methoden: Konstruktor — Hauptzeit auf `FontFamily="JetBrainsMono"` (18 bold statt 22), Soll-Zeit wie gehabt; „Fahrt fällt aus" wird zu `StatusChip` in `Error`; zusätzlicher `Delayed`-`StatusChip`; `Scheduled`-Hinweis via `Secondary`.
- Neue Eigenschaften/Methoden/Events: keine.

### `JourneyCardView` (public sealed class : Border)

- Geänderte Methoden: Konstruktor — „Ausfall gemeldet" als `StatusChip` (Icon + Text, `Error`); Zeitspanne/Badges/„Verbindung öffnen"-Button unverändert; Spacing auf Skala (12/16) prüfen.
- `Detach()` und `Clock` unverändert; keine neuen Events.

### `JourneyTimelineView` (public sealed class : VerticalStackLayout)

- Geänderte Methoden: `Event` — „Fahrt fällt aus" als `StatusChip`; Konstruktor-Spacing auf 12/16-Skala vereinheitlichen.
- Neue Eigenschaften/Events: keine.

### `HomePage` (ContentPage)

- Geänderte Methoden: Konstruktor-Shortcuts (Z. 50–53) und `RenderCards()`-Aktionsgrid (Z. 260–285) — Glyph-Literale durch `TransitIcons`-Konstanten; `⌃`/`⌄`-Wechsel über `TransitIcons.SetIcon` + bestehender Description-Wechsel (Z. 262). `AddLabel`-Default-FontSize auf Skala (13/15) prüfen.
- Neue Methoden/Event-Handler: keine (bestehende `PropertyChanged`-Handler bleiben).

### `DeparturePage` (ContentPage)

- Geänderte Methoden: Konstruktor-Aktionsgrid (`↻`, `⚙` → `TransitIcons`); `RefreshFavorite()` (Z. 199–205) setzt statt `Text` „★ Entfernen"/„☆ Speichern" `ImageSource` (`FontImageSource` `Star`/`StarOutline`) + `Text` „Entfernen"/„Speichern"; Description-Logik unverändert.
- `favoriteButton` (Z. 61) bleibt `Button` mit `LineBreakMode.TailTruncation`; keine neuen Events.

### `SearchPage` (ContentPage)

- Geänderte Methoden: Konstruktor (`⇄` → `TransitIcons.SwapVert`); `Endpoint(...)` — `lookup`/`locationButton` (Z. 115–118) auf Icon-Font; dynamischer Wechsel `⌕`↔`✓` (Z. 129) via `TransitIcons.SetIcon` (`Search`↔`Check`).
- Neue Events/Event-Handler: keine; `endpoint.PropertyChanged`-Handler bleibt, nur der gesetzte Wert ändert sich.

### `StopSearchPage` (ContentPage)

- Geänderte Methoden: Konstruktor — `find` (`⌕`→`Search`), `nearby` (`⌖`→`MyLocation`), `showMapButton` (`⌖`→`Map`/`LocationOn`); Buttons bleiben `Button` 48×48.
- Keine neuen Methoden/Events.

### `ResultsPage` (ContentPage)

- Geänderte Methoden: `RenderConnectionHeader()` (Z. 124) — `favoriteConnection.Text` ☆/★ → `TransitIcons.SetIcon` (`Star`/`StarOutline`); Konstruktor-Aufruf entsprechend; `ResultsStatus`-FontSize 24→15 (Skala).
- `ToggleConnectionAsync`/`CanToggleConnection` unverändert.

### `JourneyDetailPage` (ContentPage)

- Geänderte Methoden: Konstruktor (`⌖`→`TransitIcons.Route`/`Map`); `RenderDetails()` Fallback-`Label` FontSize 18→17.
- Keine neuen Methoden/Events.

### `MapPage` (ContentPage)

- Geänderte Methoden: Konstruktor — `reset` (`↻`→`Refresh`), `infoButton` (`ⓘ`→`Info`) via `TransitIcons`; Description-Wechsel „Quellen und Kartendaten"/„Kartendaten schließen" bleibt. Kartenmarker/Leaflet-Komponente unverändert (Symbolik-Umfangsbegrenzung).
- Keine neuen Methoden/Events.

### `AppShell` (Shell)

- Geänderte Methoden: Konstruktor — `Icon = TransitIcons.TabIcon(...)` auf den drei `Tab`s („Abfahrten" → `DepartureBoard`, „Verbindungen" → `Route`, „Haltestellen" → `LocationOn`); Titel/Routen unverändert.
- `ToolbarItem` „Haltestellen" (`OpenStopSearch`) bleibt bewusst Text (Plattformkonvention, Anwenderentscheidung).
- Neue Events/Event-Handler: keine.

### `MauiProgram` (static class)

- Geänderte Methoden: `CreateMauiApp` → `ConfigureFonts`-Block (Z. 28–32) um die fünf neuen `AddFont`-Aufrufe erweitern; OpenSans-Registrierung bleibt (Bestand, kein Umbau nötig).
- Neue Methoden/Events: keine.

### `Colors.xaml` (ResourceDictionary)

- Neue Ressourcen: `GroupStrong` (`#EEEDF3`), `GroupStrongDark` (`#2C2C2E` — Alias auf vorhandenen `GroupDark`-Wert, als eigener Token damit die Semantik „stärkere Gruppierung" klar bleibt), `Success`/`Warning`/`Error` (`#006E28`/`#894D00`/`#BA1A1A`) + Dunkelvarianten (`SuccessDark`/`WarningDark`/`ErrorDark`, kontrastgerechte Aufhellung auf `#1C1C1E`).
- Geänderte Ressourcen: keine vorhandenen Tokens (Nachschärfung, kein Ersatz).

### `Styles.xaml` (ResourceDictionary)

- Geänderte Styles: implizite `Label`/`Button`/`Entry`/`Editor`/`Picker`/`SearchBar`/`DatePicker`/`TimePicker`/`CheckBox`/`RadioButton`-Styles erhalten `FontFamily="Inter"`; `Headline` (32→28, `MidnightBlue`/`White` → Ink-Tokens) und `SubHeadline` (24→22, ebenso Ink-Tokens) auf Referenzskala; `ImageButton`-Style um `CornerRadius` 12 + `GroupStrong`-Fläche für Icon-Container-Fälle ergänzen (falls `ImageButton` benötigt wird).
- Keine neuen TargetTypes nötig.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine neuen fachlichen Validierungen — Eingabe-, Such- und Speichervalidierung bleiben unverändert. Verbindliche visuelle Regeln (aus `docs/design/acceptance.md`, gelten als Umsetzungs-DoD):

- Farbe ist nie alleiniger Informationsträger: jeder Status hat Icon + Text.
- Kontrast ≥4,5:1 Text / ≥3:1 bedienrelevante Konturen; neue Semantik-Dunkelvarianten rechnerisch gegen `CardDark`/`GroupDark` prüfen und dokumentieren.
- Interaktive Ziele ≥44×44 logische Einheiten (Icon-Buttons bleiben 48×48), beschriftet via `SemanticProperties.Description`.
- Keine starren Kartenhöhen; Icon-Glyphen dürfen bei 150 % Textskalierung nicht clippen (Prüfschritt).
- Linienbadges nur bei gelieferter Kategorie farbig (`DeparturePresentation.BadgeColor` unverändert); keine erfundenen Echtzeitwerte, Geometrien oder Zusatzdaten.
- `TransitIcons`-Codepoints müssen zur eingebetteten `MaterialSymbolsRounded.ttf` passen — Verifikation gegen deren `.codepoints`-Datei ist Pflichtschritt, kein Optional.

## Konfigurationsänderungen

- `FlowNRW/Resources/Fonts/`: neue TTF-Dateien `Inter-Regular.ttf`, `Inter-SemiBold.ttf`, `Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf`, `MaterialSymbolsRounded.ttf` (Variante FILL0/opsz 24) + jeweilige Lizenztexte (`OFL.txt`/`LICENSE` der Quelle) in `docs/design/licenses/` (oder neben den Fonts) ablegen; die zugehörige `.codepoints`-Datei der Icon-TTF ebenfalls ablegen (Verifikationsgrundlage, nicht Teil des App-Pakets — z. B. unter `docs/design/licenses/` oder `docs/design/reference/`).
- `FlowNRW/MauiProgram.cs`: fünf `fonts.AddFont(...)`-Zeilen.
- `FlowNRW/FlowNRW.csproj`: **keine Änderung** — `MauiFont`-Glob (Z. 67) deckt neue Fonts ab; kein NuGet-Paket nötig (Font-Dateien werden direkt eingebettet).
- Keine Laufzeit-/Anwendungskonfiguration; `UI_TEST_FIXTURES`-Mechanik unverändert.

## Seiteneffekte und Risiken

- **UIA-/A11y-Vertrag:** UI-Tests finden Elemente per `AutomationId` und `Name` (= `SemanticProperties.Description`) und klicken per InvokePattern; ControlType wird nur in einem Diagnose-Dump gelesen (`WindowsJourneyUiTests.ps1` Z. 677). Icon-Buttons bleiben `Button` → Vertrag sicher. Dennoch: alle AutomationIds (`OpenHomeStops`, `ToggleFavorite{i}`, `OriginSearch`, `SwapEndpoints`, `FindStops`, `ToggleConnectionFavorite`, `ShowJourneyMap`, …) und Descriptions dürfen nicht umbenannt werden; bei `DeparturePage.ToggleFavorite` bleibt `Text` „Speichern"/„Entfernen" zusätzlich erhalten.
- **Dynamische Glyph-Wechsel:** Vier Stellen wechseln Symbol **und** Beschreibung zur Laufzeit (`HomePage` Z. 262, `SearchPage` Z. 129, `DeparturePage.RefreshFavorite`, `ResultsPage.RenderConnectionHeader`) — beide müssen über `TransitIcons.SetIcon` synchron bleiben, sonst stimmen UIA-`Name` und Icon nicht überein.
- **150-%-Textskalierung:** `App.xaml.cs` Z. 38–44 skaliert `Button.FontSize` ×1,5 — Icon-Glyphen in `Button.Text` wachsen mit und können in den 48×48-Buttons clippen. Gegenmaßnahme: Icon-`FontSize` bewusst moderat wählen (~20–24) und im Design-Test prüfen; falls Clipping: Icon-Buttons von der Skalierung ausnehmen (analog `TextScaleApplied`-Mechanik) oder Glyph in `ImageSource` verlagern.
- **Hartcodierte Farben:** `TransitVisuals` (Z. 15, 41, 45–46) nutzt Literale; neue Tokens erst anlegen, dann Literale vereinheitlichen — Reihenfolge beachten, sonst doppelte Wahrheit.
- **Font-Registrierung:** Alias-Namen müssen exakt zu `FontFamily`-Strings passen; bei falscher Familie greift lautlos die Systemschrift → Screenshot-Matrix zeigt Abweichung erst visuell. Früh einen Render-Check (Screenshot oder manuell) einplanen.
- **Icon-Glyph-Codepoints:** Material-Symbols-Codepoints liegen in der Private Use Area und hängen von der Font-Datei-Version ab; beim Import gegen die `.codepoints`-Datei der exakten TTF-Version verifizieren (Anwenderentscheidung, kein Copy-paste aus Webdocs ohne Check). Rounded/Outlined-Varianten können unterschiedliche Codepoint-Zuordnungen haben.
- **Icon+Text-Rendering:** `Button.ImageSource` + `Text` auf WinUI früh sichtbar prüfen (Ausrichtung/Abstand); Fallback: zwei getrennte Elemente (Icon-`Label` + Text-`Button`) oder Icon-Button ohne Wort wie die übrigen Sekundäraktionen.
- **Screenshot-Referenzen:** Alle bisherigen Abnahmebilder ändern sich (Icons, Schrift, Chips, Tab-Icons) → `WindowsDesignUiTests.ps1`-Matrix muss vollständig neu aufgenommen und separat visuell abgenommen werden (Anwenderentscheidung: Nachweis des Qualitätssprungs).
- **Tab-Icons auf Windows:** Shell-Tab-Bar zeigt Icons; früh prüfen, dass `FontImageSource` in `Tab.Icon` auf WinUI korrekt rendert und eingefärbt wird; Fallback: `MauiImage`-Icon oder Titel-only (kein Funktionsverlust).
- **Scope-Schutz:** Keine neuen Seiten, keine Provider-/ViewModel-Änderungen außer `DeparturePresentation.Status`/`DelayText`; keine erfundenen Echtzeitwerte (Delay kommt aus `RealtimeStatus`); Linienbadges, Kartenmarker und `ToolbarItem` bleiben explizit unverändert.

## Umsetzungsreihenfolge

1. **Font-Assets und Registrierung** — Voraussetzungen: keine (externer Download der lizenzfreien TTFs). Inter (Regular/SemiBold/Bold), JetBrains Mono (Bold) und Material Symbols **Rounded FILL0 opsz 24** in `FlowNRW/Resources/Fonts` ablegen, Lizenztexte + `.codepoints`-Datei sichern, `TransitIcons`-Codepoints gegen die `.codepoints`-Datei verifizieren, `MauiProgram.ConfigureFonts` erweitern. Nachweis: Build startet, Font-Test-Label mit `FontFamily="Inter"`/`MaterialSymbols` rendert sichtbar anders.
2. **`Colors.xaml`-Tokens ergänzen** — Voraussetzungen: keine. `GroupStrong`, `Success`/`Warning`/`Error` + Dunkelvarianten anlegen; Kontrast der Dunkelvarianten gegen `#1C1C1E` rechnerisch dokumentieren.
3. **`Styles.xaml`-Typografie** — Voraussetzungen: Schritt 1. `FontFamily="Inter"` in implizite Styles, `Headline`/`SubHeadline` auf Ink-Tokens + Skala, Ausreißer-FontSizes normalisieren.
4. **`TransitIcons` + `TransitVisuals`-Umbau** — Voraussetzungen: 1–3. Glyph-Konstanten (verifiziert), `SetIcon`, `TabIcon`, `StatusChip`; `SecondaryAction` auf Icon-Font; `Text`/`Secondary` auf Inter-Aliase; `ApplyRoles` auf Token-Konstanten.
5. **`DeparturePresentation.Status`/`DelayText` + Unit-Tests** — Voraussetzungen: keine (Core, parallel zu 1–4 möglich). Enum `DepartureStatus`, Ableitung aus `Realtime`-Feldern, Tests gegen `TransitTestSupport`-Fixtures.
6. **`DepartureCardView` restrukturieren** — Voraussetzungen: 4 + 5. JetBrainsMono-Zeit, Status-Chips (Ausfall/Verspätung/keine Echtzeit), Spacing.
7. **`JourneyCardView` + `JourneyTimelineView`** — Voraussetzungen: 4 (+5 für einheitliche Chip-Semantik). Ausfall-Chip, Skala-Spacing.
8. **Glyph-Ersetzung in allen Pages** — Voraussetzungen: 4. `HomePage` (Shortcuts + Aktionsgrid inkl. `⌃`/`⌄`-Wechsel), `DeparturePage` (inkl. `ToggleFavorite` Icon+Text), `SearchPage` (inkl. `⌕`↔`✓`), `StopSearchPage`, `ResultsPage` (☆/★), `JourneyDetailPage`, `MapPage` (↻/ⓘ). AutomationIds/Descriptions unverändert lassen — Diff auf die Strings prüfen. `ToolbarItem`, Linienbadges und Leaflet-Marker bleiben bewusst unverändert.
9. **`AppShell`-Tab-Icons** — Voraussetzungen: 4. `Tab.Icon` via `FontImageSource` (`DepartureBoard`/`Route`/`LocationOn`), Titel unverändert; WinUI-Rendering des Tab-Icons früh sichtbar prüfen.
10. **Listenverdichtung `Candidate`/`RenderCards`** — Voraussetzungen: 2–4. Einheitliches Karten-Spacing, Typo-Rollen, Hierarchie in Favoritenkarten und Trefferlisten.
11. **UI-Test-Vertrag verifizieren** — Voraussetzungen: 6–10. `UiTest`-Build: alle AutomationIds auffindbar, InvokePattern-Klicks funktionieren, Descriptions unverändert; `WindowsDesignUiTests.ps1`-Matrix (hell/dunkel, 430×900/1024×768, 150 % Textskalierung) **vollständig neu aufnehmen**.
12. **Visuelle Abnahme und Dokumentation** — Voraussetzungen: 11. Separater visueller Vergleich gegen `docs/design/acceptance.md` + Referenz-HTMLs (Nachweis des „AAA"-Qualitätssprungs); Bericht/Screenshots unter `docs/help/design/verification`; iOS-Punkte über die bestehende `acceptance.md`-Checkliste abhaken.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft |
|---|---|---|
| `Status_CancelledEvent_ReturnsCancelled` | `DepartureStatusTests` (neu, `FlowNRW.Tests`) | `Realtime.Cancelled == true` → `DepartureStatus.Cancelled` |
| `Status_DelayedEvent_ReturnsDelayed` | `DepartureStatusTests` | `ActualTime != PlannedTime` bzw. `Delay > 0` → `Delayed` |
| `Status_OnTimeEvent_ReturnsOnTime` | `DepartureStatusTests` | Echtzeit ohne Abweichung → `OnTime` |
| `Status_NoRealtime_ReturnsScheduled` | `DepartureStatusTests` | `ActualTime == null` → `Scheduled` (kein erfundener Wert) |
| `DelayText_OnlyForRealDelay` | `DepartureStatusTests` | „+n Min." nur bei positivem Delay; `null` bei Pünktlichkeit/Ausfall |
| `BadgeColor_*` (bestehend, bleibt) | `DeparturePresentationTests_Badges` | Regression: Farbmatrix + WCAG-Kontrast unverändert |

### Betroffene bestehende Tests

- `DeparturePresentationTests_Badges` (`FlowNRW.Tests/DeparturePresentationTests_Badges.cs`): `BadgeColor` unverändert → muss grün bleiben.
- `JourneyPresentationTests`, `StopMonitorTests`, `FavoriteTests` u. a.: Status-/Textquellen unverändert → müssen grün bleiben.
- `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`: Screenshot-Matrix ändert sich vollständig (Icons, Schrift, Chips, Tab-Icons) — Referenzbilder neu aufnehmen; `AssertTouchTarget` (≥44×44) muss für alle Icon-Buttons weiterhin bestehen (48×48 beibehalten).
- `WindowsJourneyUiTests.ps1`, `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsDepartureCacheUiTests.ps1`: keine Assertion auf `Button.Text`-Glyphen vorhanden — müssen ohne Skriptänderung grün bleiben; nur falls ein AutomationId/Name bricht, ist das ein Umsetzungsfehler, kein Grund zur Skriptanpassung.

### E2E-Tests (Pflicht)

- **Happy Path (bestehendes Skript):** `WindowsJourneyUiTests.ps1` komplett — Tab-Navigation (mit neuen Tab-Icons), Haltestellensuche → Monitor → Favorit speichern, Verbindungssuche → Ergebniskarte → Detail-Timeline → Karte, Verbindungsfavorit ☆/★-Toggle; Deckt alle geänderten Icon-Interaktionen ab.
- **Refresh/Lifecycle/Cache-Regression:** `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsDepartureCacheUiTests.ps1` erneut ausführen (Icon-Buttons `RefreshDepartures`, `RefreshFavorite{i}` müssen InvokePattern-fähig bleiben).
- **Design-Matrix (primär Windows):** `WindowsDesignUiTests.ps1` — alle `acceptance.md`-Zeilen: schmal 430×900 + breit 1024×768, hell/dunkel (Icon-Färbung prüfen!), 150 % Textskalierung (Icon-Clipping prüfen), Ausfall-/Verspätungs-/Fehler-/Leerzustände mit neuen Status-Chips, Tab-Bar mit Icons.
- **Separate visuelle Abnahme (AAA-Nachweis):** wie `acceptance.md`-Freigaberegel — unabhängige visuelle Prüfung, Abweichungen dokumentieren, Ablage `docs/help/design/verification`.
- **iOS:** über die bestehende `acceptance.md`-Checkliste (Schrift, Kontraste, Touch-Ziele, Textskalierung) — kein eigener Gerätematrix-Lauf.

## Offene Punkte

Keine.
