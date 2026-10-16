← [Zurück zur Übersicht](index.md)

# Design — Fehlerhilfe

## Icon erscheint als Kästchen oder leeres Zeichen

**Ursache:** Der Font-Alias ist nicht registriert oder der Glyph ist kein verifizierter Codepoint.

- Aliase in `MauiProgram.ConfigureFonts` prüfen: `"MaterialSymbols"` → `MaterialSymbolsRounded.ttf`, `"MaterialSymbolsFilled"` → `MaterialSymbolsRoundedFill.ttf`, plus die `Fonts`-Aliase für Inter/JetBrains Mono. Die TTFs müssen unter `FlowNRW/Resources/Fonts/` liegen (`MauiFont Include="Resources\Fonts\*"` bindet sie automatisch ein).
- Glyph-Konstante in `TransitIcons` gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` prüfen — die Datei gehört zur exakten eingebetteten TTF-Version. Codepoints nicht aus Webdokumentation kopieren.
- In C# immer die `Fonts`-/`TransitIcons`-Konstanten verwenden; ein Tippfehler im Alias ist sonst ein stiller Fallback auf die Systemschrift.

## Der Favoritenstern wird nicht gefüllt dargestellt

**Ursache:** Die FILL-Variante steckt in der Schriftinstanz, nicht im Codepoint. `Star` und `StarOutline` teilen denselben Codepoint (U+F09A); die gefüllte Form rendert nur mit `TransitIcons.FilledFontFamily`. Bei `SetIcon`/`IconImage` den Parameter `filled: true` setzen — so in `DeparturePage.RefreshFavorite` und `ResultsPage.RenderConnectionHeader` umgesetzt.

## Icon wird bei großer Schriftskalierung abgeschnitten

**Ursache:** Button-Padding oder zu kleine Fläche. `TransitIcons.SetIcon` setzt `Padding = 0` und die Standardgröße 24 in einem 48×48-Ziel; manuell gebaute Icon-Buttons müssen dasselbe tun. Der `UiTest`-Skalierungspfad (`App.xaml.cs`, `FLOWNRW_UI_TEST_TEXT_SCALE=150`) skaliert die Glyph-`FontSize` auf 36 — bei anderen Button-Größen neu prüfen.

## Der dritte Tab rutscht unter Windows in das „Mehr"-Menü

**Ursache:** Icon-Tabs verbreitern die `NavigationViewItem`s. Bei schmalen Fenstern (≤430 px, verifizierte Matrix-Breite) greifen die Verdichtungen in `FlowNRW/Platforms/Windows/App.xaml` (`NavigationViewItemButtonMargin`, `TopNavigationViewItemContentPresenterMargin`, `TabBarNavigationViewMenuItem`-Template mit FontSize 13). Diese Ressourcen nicht entfernen; bei Änderungen die 430-px-Matrix erneut aufnehmen.

## Ein primärer Button liegt am Viewport-Rand oder wird abgeschnitten

**Beobachtet an `SearchJourneys` bei 430×900:** Die Inter-Metriken veränderten die Höhen; behoben durch gerasterte Verdichtung der `SearchPage` (Spacing/Paddings auf die Skala). Bei ähnlichen Symptomen zuerst Spacing auf das 4/8/12/16-Raster prüfen, dann die Fensterbreiten der Screenshot-Matrix testen.

## Text sieht nicht nach Inter aus

- Der implizite `FontFamily="Inter"`-Setter greift nur dort, wo kein lokaler `FontFamily` gesetzt ist; lokale Überschreibungen in C#-Views prüfen.
- `TransitVisuals.Text(..., bold: true)` nutzt den Schnitt `InterBold`; `FontAttributes.Bold` auf `Inter` würde synthetisches Fett erzeugen — nicht verwenden.
- OpenSans ist entfernt; alte Alias-Referenzen (`OpenSansRegular`/`OpenSansSemibold`) würden still auf die Systemschrift fallen.

## `FontImageSource`-Icon hat falsche Farbe im Dunkelmodus

**Ursache:** `FontImageSource.Color` wurde nicht thematisch gesetzt. `TransitIcons.IconImage`/`TabIcon` setzen `SetAppThemeColor` mit `TransitVisuals.Action`/`ActionDark` — bei eigenen `FontImageSource`s dasselbe Muster verwenden, keine statische Farbe.

## Das Warnsymbol zum Datenqualitätshinweis erscheint nicht oder falsch

- **Es gibt schlicht keinen Hinweis:** `HasDataNotice` gilt nur, wenn das angezeigte `Result` `Warnings`, `IsFallback` oder `IsStale` trägt; ein sauberes oder komplett fehlgeschlagenes Ergebnis zeigt bewusst kein Symbol. Während `IsBusy` ist der Schalter verborgen.
- **Glyph als Kästchen:** `TransitIcons.Warning` ist U+F083 — wie oben gegen `MaterialSymbolsRounded.codepoints` verifizieren.
- **Warnfarbe fehlt:** `NoticeToggle.ApplyWarningColors()` muss **nach** `TransitVisuals.ApplyRoles` laufen; die gemeinsame Button-Rolle überschreibt die `Warning`/`WarningDark`-Textfarbe sonst mit der Aktionsfarbe.
- **Detail klappt nicht auf:** Schalter und `Metadata`-Label gehören paarweise (`FavoriteNotice{i}`/`FavoriteMetadata{i}`, `MonitorNotice`/`MonitorMetadata`); `Update(false)` schließt das Detail automatisch — ein verschwundener Hinweis hinterlässt kein geöffnetes Detail.
