# Code-Review

## Ergebnis
**Status:** Befunde vorhanden

## Befunde

### TransitIcons.cs (TransitIcons)
- **Kopplung** — `TabIcon` (Zeile 127) verwendet die hartcodierten Hexliterale `#0058BC`/`#A5C9FF`, während die Schwester-Methode `IconImage` (Zeile 110) die zentralen Konstanten `TransitVisuals.Action`/`ActionDark` nutzt. Zusätzlich weicht der verwendete Dark-Wert `#A5C9FF` (XAML-Token `PrimaryDark`) von `TransitVisuals.ActionDark` (`#A8C8FF`) ab — es existieren zwei divergierende Dark-Varianten der Interaktionsfarbe.
  Empfehlung: `TransitVisuals.Action` für Light verwenden und einen einheitlichen Dark-Token definieren (entweder `ActionDark` auf `#A5C9FF` angleichen oder einen eigenen benannten Token in `TransitVisuals`/Colors.xaml), dann in beiden Methoden konsistent referenzieren.
- **Toter Code** — `OpenInNew` (Zeile 37) wird nirgends verwendet.
  Empfehlung: Konstante entfernen, bis ein konkreter Anwendungsfall existiert (vermeidet Speculative Generality).

### TransitVisuals.cs (TransitVisuals)
- **Doppelter Code** — `SecondaryAction` (Zeilen 152-167) repliziert die Glyph-Setzung aus `TransitIcons.SetIcon` (Text, `FontFamily = TransitIcons.FontFamily`, `FontSize = 24`, `SemanticProperties.SetDescription`) statt die Methode aufzurufen.
  Empfehlung: Button ohne `Text`/`FontFamily`/`FontSize` erzeugen und intern `TransitIcons.SetIcon(button, icon, description)` aufrufen.
- **Kopplung** — `ActionDark` (`#A8C8FF`, Zeile 13) ist eine zweite Dark-Variante der Interaktionsfarbe neben dem XAML-Token `PrimaryDark` (`#A5C9FF`, Colors.xaml Zeile 10), ohne dass der Unterschied fachlich begründet oder in den Tokens abgebildet ist.
  Empfehlung: Auf einen Wert vereinheitlichen oder den Sonderfall als eigenen Token in Colors.xaml benennen und dokumentieren.

### SearchPage.cs / HomePage.cs (SetIcon-Aufrufer)
- **Fehlerbehandlung/Struktur** — `TransitIcons.SetIcon` setzt kein `Padding`; die globalen Button-Styles definieren `Padding 14,10` (Styles.xaml Zeile 34). Die per `SetIcon` befüllten Icon-Buttons mit `HeightRequest`/`WidthRequest` = 48 in `SearchPage.Endpoint` (`lookup`, `locationButton`, Zeilen 115-118) und `HomePage.RenderCards` (`expand`, `refreshButton`, `openButton`, `removeButton`, Zeilen 260-285) lassen damit nur ~20 DIP für ein 24er-Glyph — Clipping-Risiko. Vergleichbare Aufrufer setzen `Padding = new Thickness(0)` explizit (`TransitVisuals.SecondaryAction`, `StopSearchPage` Zeilen 46/48, `MapPage` Zeile 55) — inkonsistenter Helper-Vertrag.
  Empfehlung: `Padding = new Thickness(0)` in `TransitIcons.SetIcon` zentral setzen (analog `SecondaryAction`), statt es an einzelnen Call-Sites zu vergessen/zu wiederholen.

### DepartureCardView.cs / JourneyCardView.cs / HomePage.cs
- **Doppelter Code** — Die Umwandlung `TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")` + `ConvertTime` + Format `"HH:mm"` ist dreifach implementiert: `DepartureCardView.Clock` (Zeilen 73-78), `JourneyCardView.Clock` (Zeilen 47-48) und inline in `HomePage.RenderSummary` (Zeile 232). Die Fallback-Texte sind inkonsistent (`"unbekannt"` vs. `"Unbekannt"`).
  Empfehlung: Eine gemeinsame Formatierungsmethode (z. B. `JourneyPresentation.Clock` o. ä.) extrahieren, die Zeitzone statisch cachen und den Fallback-Text vereinheitlichen.

### HomePage.cs (RenderCards)
- **Struktur** — `RenderCards` (Zeilen 197-291, ~95 Zeilen) übernimmt mehrere Verantwortlichkeiten: Kartenkopf, Linien-Summary inkl. eigener `RenderSummary`-Closure, Abfahrtsliste inkl. `RenderDepartures`-Closure, vier Aktions-Buttons sowie Unsubscribe-Verwaltung.
  Empfehlung: In Teil-Methoden zerlegen (z. B. `BuildCardHeader`, `BuildSummary`, `BuildDepartures`, `BuildCardActions`), ggf. in eine `FavoriteCardView`-Klasse auslagern analog `DepartureCardView`/`JourneyCardView`.

### MauiProgram.cs (CreateMauiApp)
- **Toter Code** — Die Registrierungen `OpenSans-Regular.ttf`/`OpenSans-Semibold.ttf` (Zeilen 30-31) und die zugehörigen Font-Assets werden nach der Umstellung auf Inter nirgends mehr referenziert (kein `FontFamily`-Verweis in Code oder XAML); die Fonts werden dennoch ins App-Bundle gepackt.
  Empfehlung: Beide `AddFont`-Aufrufe und die Dateien `FlowNRW/Resources/Fonts/OpenSans-*.ttf` entfernen.

### DepartureStatusTests.cs (DepartureStatusTests)
- **Namenskonventionen/Doku** — Die `<summary>` von `DelayText_RealDeviation_FormatsPositiveMinutes` (Zeile 57) behauptet „a reported actual time equal to the plan“, der Test stellt jedoch `ActualTime = planned + 7 Min` ein — die Doku beschreibt den `DelayText_OnTimeEvent`-Fall, nicht den getesteten.
  Empfehlung: Summary korrigieren, z. B. „A reported actual time deviating from the plan is formatted as a positive delay text.“

## Geprüfte Dateien
- `FlowNRW.Core/Presentation/DeparturePresentation.cs`
- `FlowNRW.Core/Presentation/DepartureStatus.cs`
- `FlowNRW.Tests/DepartureStatusTests.cs`
- `FlowNRW/AppShell.xaml.cs`
- `FlowNRW/DepartureCardView.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/JourneyCardView.cs`
- `FlowNRW/JourneyDetailPage.cs`
- `FlowNRW/JourneyTimelineView.cs`
- `FlowNRW/MapPage.cs`
- `FlowNRW/MauiProgram.cs`
- `FlowNRW/Resources/Styles/Colors.xaml`
- `FlowNRW/Resources/Styles/Styles.xaml`
- `FlowNRW/ResultsPage.cs`
- `FlowNRW/SearchPage.cs`
- `FlowNRW/StopSearchPage.cs`
- `FlowNRW/TransitIcons.cs`
- `FlowNRW/TransitVisuals.cs`

## Verifikation (Spezialfokus)
- MauiFont-Einbindung korrekt: `<MauiFont Include="Resources\Fonts\*" />` in `FlowNRW/FlowNRW.csproj` (Zeile 67) erfasst alle neuen `.ttf`-Dateien; alle sechs Aliase aus `MauiProgram` (`Inter`, `InterSemiBold`, `InterBold`, `JetBrainsMono`, `MaterialSymbols`, `MaterialSymbolsFilled`) werden konsistent verwendet.
- `TransitIcons`-Codepoints: alle 19 Glyph-Konstanten gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` geprüft — vollständig konsistent; `Star`/`StarOutline` teilen bewusst denselben Codepoint (FILL0/FILL1 via separatem Font-Alias).
- Alle `StaticResource`-Referenzen in `Styles.xaml` zeigen auf definierte `Colors.xaml`-Tokens; `AppThemeBinding` korrekt verwendet.
