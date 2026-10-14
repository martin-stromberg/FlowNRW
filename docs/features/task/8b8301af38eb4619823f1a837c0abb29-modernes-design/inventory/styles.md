# Design-Tokens, Styles und Fonts

Dokumentiert den Ist-Zustand der Style-Ressourcen, Assets und der Font-Registrierung.

## `FlowNRW/Resources/Styles/Colors.xaml`

Farbtokens (Z. 9–41) und SolidColorBrush-Aliase (Z. 43–57). Keine `AppThemeBinding` hier — Hell/Dunkel wird in `Styles.xaml` über paarweise `*Light`/`*Dark`-Tokens aufgelöst.

| Schlüssel | Wert | Rolle |
|---|---|---|
| `Primary` | `#0058BC` | Interaktionsfarbe hell (verbindlich lt. `acceptance.md`) |
| `PrimaryDark` | `#A5C9FF` | Interaktionsfarbe dunkel |
| `PrimaryDarkText` | `#242424` | Text auf `PrimaryDark` |
| `Secondary` | `#E8F0FC` | Sekundärfläche hell |
| `SecondaryDarkText` | `#F5F5F7` | Heller Text dunkel |
| `Tertiary` | `#003E85` | Tertiär-Akzent |
| `CanvasLight` / `CanvasDark` | `#FAF9FE` / `#000000` | Seitenhintergrund |
| `CardLight` / `CardDark` | `#FFFFFF` / `#1C1C1E` | Kartenfläche |
| `GroupLight` / `GroupDark` | `#F4F3F8` / `#2C2C2E` | Innere Gruppierung |
| `InkLight` / `InkDark` | `#1A1B1F` / `#F5F5F7` | Primärtext |
| `MutedLight` / `MutedDark` | `#414755` / `#C5C7D0` | Sekundärtext |
| `OutlineLight` / `OutlineDark` | `#E3E2E7` / `#414149` | Kartenkontur |
| `White`, `Black`, `Magenta`, `MidnightBlue`, `OffBlack` | div. | Hilfsfarben (`Magenta`/`MidnightBlue` noch aus Template in `Headline`/`TabbedPage`-Styles) |
| `Gray100`–`Gray950` | Graustufen | Disabled-/Indikatorzustände |

Brushes: `PrimaryBrush`, `SecondaryBrush`, `TertiaryBrush`, `WhiteBrush`, `BlackBrush`, `Gray100Brush`–`Gray950Brush`.

**Hinweis:** Die Token `#EEEDF3` (stärkere Gruppierung, lt. `acceptance.md`) sowie die Semantikfarben Bestätigung `#006E28`, Warnung `#894D00`, Fehler `#BA1A1A` und die Referenz-Linienfarben `#008D43`/`#E35205`/`#009AA6` sind **nicht** als Token vorhanden — `#EEEDF3` wird hartcodiert in `TransitVisuals.ApplyRoles` (Z. 45) verwendet; Badgefarben kommen aus `DeparturePresentation.BadgeColor` (`FlowNRW.Core/Presentation/DeparturePresentation.cs` Z. 13–36: `#BA1B1D`, `#007A39`, `#005A9C`, `#B33F00`, `#6F2C91`, `#00777F`, Fallback `#414755` — kontrastoptimierte Varianten der Referenzfarben).

## `FlowNRW/Resources/Styles/Styles.xaml`

Implizite Styles (410 Zeilen). Gemeinsame Merkmale: `AppThemeBinding` für Farben, `MinimumHeightRequest`/`MinimumWidthRequest` = 44 für interaktive Controls, `VisualStateGroups` mit `Normal`/`Disabled` (teilweise `PointerOver`), `FontSize` = 17 für Eingabe-/Text-Controls. **Kein Style setzt `FontFamily`.**

| TargetType | Zeilen | Wesentliche Setter |
|---|---|---|
| `ActivityIndicator` | 7–9 | `Color` Primary/White |
| `IndicatorView` | 11–14 | Indikatorfarben Gray200/500, Gray950/100 |
| `Border` (`ApplyToDerivedTypes`) | 16–21 | `Stroke` OutlineLight/Dark, `BackgroundColor` CardLight/Dark, `StrokeShape` RoundRectangle 18, `StrokeThickness` 1 |
| `BoxView` | 23–25 | Hintergrund Gray950/Gray200 |
| `Button` | 27–50 | Text White/PrimaryDarkText, BG Primary/PrimaryDark, `FontSize` 17, `BorderWidth` 0, `CornerRadius` 12, `Padding` 14,10, 44er-Mindestmaß, Disabled-VSM |
| `CheckBox` | 52–68 | `Color` Primary/White, 44er, Disabled-VSM |
| `DatePicker` | 70–88 | Text Gray900/White, transparent, 17, 44er, Disabled-VSM |
| `Editor` | 90–109 | Text InkLight/Dark, Placeholder Muted, 17, 44er, Disabled-VSM |
| `Entry` | 111–130 | wie `Editor` |
| `ImageButton` | 132–152 | `Opacity` 1, transparent, `CornerRadius` 0, 44er, Disabled→Opacity 0.5 |
| `Label` | 154–170 | Text InkLight/Dark, transparent, 17, Disabled-VSM |
| `Label` `x:Key="Headline"` | 172–177 | MidnightBlue/White, 32, zentriert |
| `Label` `x:Key="SubHeadline"` | 179–184 | MidnightBlue/White, 24, zentriert |
| `Picker` | 186–206 | Text/Title Gray900, transparent, 17, 44er, Disabled-VSM |
| `ProgressBar` | 208–222 | `ProgressColor` Primary/White, Disabled-VSM |
| `RadioButton` | 224–242 | transparent, InkLight/Dark, 17, 44er, Disabled-VSM |
| `RefreshView` | 244–246 | `RefreshColor` Gray900/Gray200 |
| `SearchBar` | 248–269 | Text Gray900/White, Placeholder Gray500, transparent, 17, 44er, Disabled-VSM |
| `Shadow` | 271–276 | Radius 15, Opacity 0.5, White, Offset 10,10 |
| `Slider` | 278–296 | Track/Thumb Primary/White, Disabled-VSM |
| `SwipeItem` | 298–300 | BG White/Black |
| `Switch` | 302–333 | `Scale` 1.2, `HeightRequest` 48, On/Off-VSM, OnColor Primary/White |
| `TimePicker` | 335–353 | wie `DatePicker` |
| `TitleBar` | 355–377 | **auskommentiert** |
| `Page` (`ApplyToDerivedTypes`) | 379–382 | `Padding` 0, BG CanvasLight/Dark |
| `Shell` (`ApplyToDerivedTypes`) | 384–395 | Shell-Farben (Background Canvas, TabBar White/Black, TabBarForeground Primary/PrimaryDark, `NavBarHasShadow` False) |
| `NavigationPage` | 397–401 | Bar White/OffBlack, Text/Icon Gray200/White |
| `TabbedPage` | 403–408 | Bar White/Gray950, `BarTextColor` Magenta/White, Tabfarben Gray |

## `FlowNRW/App.xaml`

Merged Dictionaries (Z. 8–11): `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`. Keine weiteren Ressourcen.

## `FlowNRW/App.xaml.cs`

| Element | Zeilen | Beschreibung |
|---|---|---|
| `TextScaleApplied` (attached, `#if UI_TEST_FIXTURES`) | 15–22 | Attached Property, markiert bereits skalierte Elemente |
| `UserAppTheme` aus `FLOWNRW_UI_TEST_THEME` | 37 | Erzwingt light/dark im UiTest-Build |
| `DesignTextScale`-Handler-Mappings | 38–44 | ×1.5-`FontSize` für `Label`/`Button`/`Entry`/`Picker` bei `FLOWNRW_UI_TEST_TEXT_SCALE=150` |
| `CreateWindow` | 49–63 | `Window(shell)`, Lifecycle-Hooks an `RefreshLifecycle.SetActive`, `FLOWNRW_UI_TEST_PIN_ACTIVE` unterdrückt Deaktivierung |

## Fonts und Bild-Assets

| Asset | Ort | Registrierung |
|---|---|---|
| `OpenSans-Regular.ttf` | `FlowNRW/Resources/Fonts/` | `fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular")` — `MauiProgram.cs` Z. 30 |
| `OpenSans-Semibold.ttf` | `FlowNRW/Resources/Fonts/` | `fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold")` — `MauiProgram.cs` Z. 31 |
| `dotnet_bot.png` | `FlowNRW/Resources/Images/` | `MauiImage Include="Resources\Images\*"` + `MauiImage Update ... BaseSize="300,185"` — `FlowNRW.csproj` Z. 63–64; **wird in keiner View referenziert** |
| `flownrw.png`, `appicon.svg`, `appiconfg.svg` | `FlowNRW/Resources/AppIcon/` | `MauiIcon`/`MauiSplashScreen` — `FlowNRW.csproj` Z. 57, 60 |
| `map/` (Leaflet: `index.html`, `leaflet.js/css`, `map.js/css`) | `FlowNRW/Resources/Raw/` | `MauiAsset Include="Resources\Raw\**"` — `FlowNRW.csproj` Z. 70; HybridWebView-Root der `MapPage` |

**Explizite `FontFamily`-Setzungen im gesamten App-Code:** genau eine — `DepartureCardView.cs` Z. 37 (`Consolas` auf WinUI, sonst `Menlo` für die prominent dargestellte Abfahrtszeit). Die registrierten `OpenSans*`-Aliase werden nirgends referenziert; ohne `FontFamily` greift die jeweilige Systemschrift.

## Referenz-Designsystem (kein Produktcode)

`docs/design/reference/trans_nrw_mobil_system/DESIGN.md` enthält das verbindliche Referenzsystem: Inter-Typografieskala (34/28/22/20/17/16/15/13/12/11), `JetBrains Mono` für tabellarische Live-Zeiten (18 bold bzw. 14 für Verspätung), Linienfarben (RE/RB `#BA1B1D`, S `#008D43`, U `#005A9C`, Tram `#E35205`, Bus `#6F2C91`, Fähre `#009AA6`), Radien (sm–xl, full), Spacing-Raster (2–40 px, Gutter 16/24, Kartenabstand 12), Semantikfarben (`#006E28`/`#894D00`/`#BA1A1A`) und Flächentokens (surface/on-surface/outline-Varianten). Vier `code.html`-Referenzlayouts unter `docs/design/reference/{abfahrtsmonitor_live,verbindungssuche,fahrtbegleiter_detail,umgebungskarte_stationen}/` (nutzen externe Tailwind-/Fontquellen — laut `acceptance.md` keine Produktabhängigkeit).
