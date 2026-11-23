# Code-Review

## Ergebnis
**Status:** Befunde vorhanden

## Befunde

### MauiProgram.cs (CreateMauiApp)
- **Kopplung** — Die Icon-Font-Aliase werden in den Zeilen 34-35 als Stringliterale `"MaterialSymbols"`/`"MaterialSymbolsFilled"` registriert, obwohl `TransitIcons.FontFamily`/`TransitIcons.FilledFontFamily` (`TransitIcons.cs` Zeilen 13/16) dieselben Werte bereits als Konstanten zentralisieren — im direkten Kontrast zu den Zeilen 30-33, die konsequent `Fonts.*` verwenden. Ein Tippfehler in einem der Literale fiele erst als stiller Font-Fallback zur Laufzeit auf.
  Empfehlung: `fonts.AddFont("MaterialSymbolsRounded.ttf", TransitIcons.FontFamily)` und `fonts.AddFont("MaterialSymbolsRoundedFill.ttf", TransitIcons.FilledFontFamily)` verwenden.

### JourneyCardView.cs / JourneyTimelineView.cs / JourneyPresentation.cs
- **Doppelter Code** — Die Linien-Fallback-Kette `leg.Line?.Name ?? leg.Departure.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt"` steht wortgleich an drei Stellen (`JourneyCardView` Zeile 29, `JourneyTimelineView` Zeile 31, `JourneyPresentation.LegName` Zeile 73); die zugehörige Mode-Auflösung `(leg.Line ?? leg.Departure.Line)?.Mode` an zwei Stellen (`JourneyCardView` Zeile 29, `JourneyTimelineView` Zeile 35). Eine vierte Fallback-Quelle müsste an allen Stellen gepflegt werden.
  Empfehlung: Auflösung in `JourneyPresentation` zentralisieren (z. B. `LegLineName(JourneyLeg)`/`LegLineMode(JourneyLeg)` oder eine Methode, die das effektive `Line` liefert) und an allen Call-Sites verwenden.

### Colors.xaml
- **Toter Code** — `Tertiary` (Zeile 14), `GroupLight` (Zeile 19) und `GroupDark` (Zeile 20) werden weder per `StaticResource`/`AppThemeBinding` noch von einer C#-Stelle referenziert. `Tertiary` hat mit dem entfernten `TertiaryBrush` seinen letzten Verweis verloren; `GroupLight`/`GroupDark` sind Template-Reste — der `GroupDark`-Wert `#2C2C2E` lebt bereits als eigenständiger Token `GroupStrongDark` weiter. Analog zum `MidnightBlue`-Befund der vorherigen Iteration.
  Empfehlung: Die drei unreferenzierten Tokens entfernen (Semantik-Tokens wie `Success`/`Warning`/`Error` bleiben — sie werden über die `TransitVisuals`-Konstanten konsumiert).

### TransitIcons.cs (TransitIcons)
- **Doppelter Code** — `TabIcon` (Zeilen 118-128) repliziert `IconImage` (Zeilen 101-111): identische `FontImageSource`-Konstruktion inklusive `SetAppThemeColor(..., TransitVisuals.Action, TransitVisuals.ActionDark)`; die einzigen Unterschiede sind `Size = 24` und der fehlende `filled`-Parameter.
  Empfehlung: `TabIcon` auf `=> IconImage(glyph, size: 24)` reduzieren.

### HomePage.cs (BuildCardActions)
- **Doppelter Code** — Der `TransitIcons.SetIcon`-Aufruf für den Expand-Button steht mit identischen Argumenten zweimal: initial in Zeile 289 und nochmals im `AsyncRelayCommand`-Lambda in Zeile 290 (`card.IsExpanded ? TransitIcons.ExpandLess : TransitIcons.ExpandMore` und `card.IsExpanded ? "Abfahrten einklappen" : "Abfahrten aufklappen"`).
  Empfehlung: In eine lokale Funktion auslagern (z. B. `void SetExpandIcon() => TransitIcons.SetIcon(expand, ...)`), die initial und im Command aufgerufen wird.

## Iteration-2-Nachprüfung
Alle 6 Befunde sind korrekt behoben:
- `EffectiveDelay(StopEvent)` extrahiert (`DeparturePresentation.cs` Zeilen 58-63) und in `DelayText` (Zeile 72) sowie `Describe` (Zeile 84) verwendet.
- `StrikethroughTime` in `TransitVisuals` (Zeilen 116-121) über `Secondary` mit einheitlicher Schriftgröße 13; Call-Sites `DepartureCardView` Zeile 42 und `JourneyTimelineView` Zeile 68 umgestellt.
- Die redundanten `Padding = new Thickness(0)` an den `SetIcon`-Call-Sites in `StopSearchPage` (Zeilen 46-49) und `MapPage` (Zeilen 55-56) sind entfernt; `SetIcon` setzt das Padding zentral (`TransitIcons.cs` Zeile 89).
- `FavoriteCardViews`-Record eingeführt (`HomePage.cs` Zeilen 266-267); `SubscribeCardChanges` nimmt nur noch `card`, `index` und das View-Bündel entgegen.
- Neue `Fonts`-Klasse (`FlowNRW/Fonts.cs`) mit den vier Text-Alias-Konstanten; alle C#-Verwendungen umgestellt (`TransitVisuals` Zeilen 99/192, `DepartureCardView` Zeile 38, `MauiProgram` Zeilen 30-33).
- `MidnightBlue` und der gesamte `SolidColorBrush`-Block sind aus `Colors.xaml` entfernt; keine verwaisten Verweise zurückgeblieben.

## Geprüfte Dateien
- `FlowNRW.Core/Presentation/DeparturePresentation.cs`
- `FlowNRW.Core/Presentation/DepartureStatus.cs`
- `FlowNRW.Core/Presentation/JourneyPresentation.cs`
- `FlowNRW.Tests/DepartureStatusTests.cs`
- `FlowNRW.Tests/PresentationTests.cs`
- `FlowNRW/AppShell.xaml.cs`
- `FlowNRW/DepartureCardView.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/Fonts.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/JourneyCardView.cs`
- `FlowNRW/JourneyDetailPage.cs`
- `FlowNRW/JourneyTimelineView.cs`
- `FlowNRW/MapPage.cs`
- `FlowNRW/MauiProgram.cs`
- `FlowNRW/Platforms/Windows/App.xaml`
- `FlowNRW/Resources/Styles/Colors.xaml`
- `FlowNRW/Resources/Styles/Styles.xaml`
- `FlowNRW/ResultsPage.cs`
- `FlowNRW/SearchPage.cs`
- `FlowNRW/StopSearchPage.cs`
- `FlowNRW/TransitIcons.cs`
- `FlowNRW/TransitVisuals.cs`

## Verifikation (Spezialfokus)
- Font-Aliase: XAML-Literale `Inter`/`InterSemiBold` in `Styles.xaml` sind deckungsgleich mit den `Fonts`-Konstanten (dokumentierte Ausnahme für XAML); `MauiProgram` registriert alle sechs Aliase konsistent mit den Konstantenwerten. Abweichung nur bei den Icon-Aliasen (s. Befund MauiProgram).
- Codepoints: alle 19 Glyph-Konstanten in `TransitIcons` gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` geprüft — vollständig konsistent (`add` U+E145 … `location_on` U+F1DB; `star`/`star_outline` bewusst U+F09A via FILL0/FILL1-Fonts).
- Token-Nutzung: alle `StaticResource`-/`AppThemeBinding`-Referenzen in `Styles.xaml` und `Platforms/Windows/App.xaml` zeigen auf definierte `Colors.xaml`-Tokens; keine Verweise auf die entfernten `MidnightBlue`/Brush-Keys zurückgeblieben. Unreferenzierte Rest-Tokens separat als Befund geführt.
- Tests: neue `EffectiveDelay_*`-Tests (`DepartureStatusTests`) verifizieren die geteilte Ableitung für `DelayText` und `Describe`; neue `JourneyPresentation`-Tests decken Berlin-Zeitzone (Sommer/Winter) und einheitlichen `unbekannt`-Fallback ab. AAA-Struktur und Namenskonvention der Datei eingehalten.
