# Code-Review

## Ergebnis
**Status:** Befunde vorhanden

## Befunde

### DeparturePresentation.cs (DeparturePresentation)
- **Doppelter Code** — Die Ableitung der effektiven Abweichung `realtime.ActualTime is { } actual && item.PlannedTime is { } planned ? actual - planned : realtime.Delay` ist in `DelayText` (Zeilen 62-63) und `Describe` (Zeilen 75-76) identisch dupliziert.
  Empfehlung: In eine private Methode `EffectiveDelay(StopEvent)` extrahieren und in beiden Methoden verwenden (auch `Status` Zeilen 49-51 prüft dieselbe Kombination und kann davon profitieren).

### DepartureCardView.cs / JourneyTimelineView.cs
- **Doppelter Code / Konsistenz** — Der Block „Soll-Zeit durchgestrichen, wenn Ist abweicht" (`ActualTime is { } && PlannedTime is { } && actual != planned` → `Secondary` + `TextDecorations.Strikethrough`) ist in `DepartureCardView` (Zeilen 41-47, `FontSize = 13`) und `JourneyTimelineView.Event` (Zeilen 67-73, `FontSize = 12`) doppelt implementiert — inklusive divergierender Schriftgröße ohne erkennbare Begründung.
  Empfehlung: Gemeinsamen Helper in `TransitVisuals` (z. B. `StrikethroughTime(DateTimeOffset? planned)`) extrahieren und die Schriftgröße vereinheitlichen.

### StopSearchPage.cs / MapPage.cs
- **Doppelter Code** — `Padding = new Thickness(0)` wird an drei `SetIcon`-Call-Sites redundant im Objektinitialisierer gesetzt (`StopSearchPage` Zeilen 46 und 48, `MapPage` Zeile 55), obwohl `TransitIcons.SetIcon` (`TransitIcons.cs` Zeile 89) das Padding seit der Iteration-1-Korrektur zentral zurücksetzt. Die expliziten Werte werden sofort überschrieben.
  Empfehlung: Die drei redundanten `Padding`-Setter entfernen, damit der zentrale Vertrag alleinige Quelle ist.

### HomePage.cs (HomePage)
- **Code Smell: Long Parameter List** — `SubscribeCardChanges` (Zeilen 265-280) nimmt sechs Parameter entgegen (`card`, `index`, `status`, `summary`, `summaryContainer`, `departures`); vier davon sind die zusammengehörigen Views derselben Karte.
  Empfehlung: Die Karten-Views in einem privaten `record`/ kleinen Typ (z. B. `FavoriteCardViews`) bündeln und nur `card`, `index` und dieses Bündel übergeben — oder die Subskription in die Builder-Methode der Karte verschieben.

### TransitVisuals.cs / DepartureCardView.cs (Font-Aliase)
- **Kopplung** — Die Text-Font-Aliase `"Inter"`, `"InterBold"`, `"InterSemiBold"` und `"JetBrainsMono"` sind als Stringliterale verteilt (`TransitVisuals` Zeilen 99 und 181, `DepartureCardView` Zeile 38, zusätzlich `Styles.xaml`), während die Icon-Aliase sauber über `TransitIcons.FontFamily`/`FilledFontFamily` zentralisiert sind. Ein Tippfehler an einer Stelle fiele nicht beim Kompilieren, sondern erst als stiller Font-Fallback zur Laufzeit auf.
  Empfehlung: Konstanten für die Text-Aliase definieren (z. B. in `TransitVisuals` oder einer `FontAliases`-Klasse) und die C#-Verwendungen darauf umstellen; die XAML-Styles verbleiben zwangsläufig als Literale.

### Colors.xaml
- **Toter Code** — `MidnightBlue` (Zeile 42) hat mit der Umstellung von `Headline`/`SubHeadline` auf die Ink-Tokens (Styles.xaml Zeilen 178-192) seinen letzten Verweis verloren. Ebenso ist der gesamte `SolidColorBrush`-Block (Zeilen 54-67: `PrimaryBrush` … `Gray950Brush`) in keiner XAML- oder C#-Stelle referenziert (Template-Rest).
  Empfehlung: `MidnightBlue` entfernen; den ungenutzten Brush-Block ebenfalls entfernen oder zumindest `MidnightBlue` nicht verwaist stehen lassen.

## Iteration-1-Nachprüfung
Alle 8 Befunde sind korrekt behoben:
- `TransitIcons.TabIcon` nutzt jetzt `TransitVisuals.Action`/`ActionDark` (Zeile 126); `ActionDark` wurde auf `#A5C9FF` angeglichen und entspricht dem XAML-Token `PrimaryDark` — eine einheitliche Dark-Variante.
- `TransitIcons.OpenInNew` wurde entfernt.
- `TransitVisuals.SecondaryAction` ruft jetzt `TransitIcons.SetIcon` (Zeile 161).
- `TransitIcons.SetIcon` setzt zentral `Padding = new Thickness(0)` (Zeile 89).
- Die dreifache Zeitzonen-/`HH:mm`-Logik ist in `JourneyPresentation.LocalTime`/`Clock` zentralisiert, die Zeitzone statisch gecacht, Fallback einheitlich `„unbekannt"`.
- `HomePage.RenderCards` ist in `BuildFavoriteCard`/`BuildCardHeader`/`RenderFavoriteSummary`/`RenderFavoriteDepartures`/`SubscribeCardChanges`/`BuildCardActions` zerlegt.
- OpenSans-Registrierungen und Font-Assets wurden entfernt.
- Die `<summary>` von `DelayText_RealDeviation_FormatsPositiveMinutes` beschreibt jetzt den tatsächlich getesteten Fall.

## Geprüfte Dateien
- `FlowNRW.Core/Presentation/DeparturePresentation.cs`
- `FlowNRW.Core/Presentation/DepartureStatus.cs`
- `FlowNRW.Core/Presentation/JourneyPresentation.cs`
- `FlowNRW.Tests/DepartureStatusTests.cs`
- `FlowNRW.Tests/PresentationTests.cs`
- `FlowNRW/AppShell.xaml.cs`
- `FlowNRW/DepartureCardView.cs`
- `FlowNRW/DeparturePage.cs`
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
- Font-Aliase: sechs Registrierungen in `MauiProgram` (Zeilen 30-35: `Inter`, `InterSemiBold`, `InterBold`, `JetBrainsMono`, `MaterialSymbols`, `MaterialSymbolsFilled`); `<MauiFont Include="Resources\Fonts\*" />` in `FlowNRW/FlowNRW.csproj` (Zeile 67) erfasst alle `.ttf`. Verwendungen konsistent; `JetBrainsMono` nur für die prominente Zeit.
- Codepoints: alle 19 Glyph-Konstanten in `TransitIcons` gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` geprüft — vollständig konsistent (u. a. `grade`/`star`/`star_outline` U+F09A, `schedule`/`access_time` U+EFD6, `error` U+F8B6, `location_on` U+F1DB). FILL1-Stern über eigenen Font-Alias wie geplant.
- `Platforms/Windows/App.xaml`: `TabBarNavigationViewMenuItem`-Override entspricht exakt den Bindings des MAUI-Default-Templates (`TabbedPageStyle.xaml`: `Content`, `Background`, `IsEnabled`, `IsSelected` TwoWay, `MenuItemsSource`, `Icon`) plus `FontSize="13"` — korrekt gebunden.
- `Styles.xaml`: alle `StaticResource`-/`AppThemeBinding`-Referenzen zeigen auf definierte `Colors.xaml`-Tokens; kein neues Hexliteral außerhalb der Token-Definitionen.
