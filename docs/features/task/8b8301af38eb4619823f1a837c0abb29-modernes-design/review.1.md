# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

- [x] `Font-Assets + Lizenzen` — `Inter-Regular.ttf`/`Inter-SemiBold.ttf`/`Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf`, `MaterialSymbolsRounded.ttf` (FILL0/opsz 24) liegen in `FlowNRW/Resources/Fonts/`; Lizenztexte (`Inter-LICENSE.txt`, `JetBrainsMono-OFL.txt`, `MaterialSymbols-LICENSE.txt`) und `MaterialSymbolsRounded.codepoints` unter `docs/design/licenses/`. Zusätzlich `MaterialSymbolsRoundedFill.ttf` (FILL1) eingebettet — planerweiternd, für gefüllte Favoriten-Sterne.
- [x] `MauiProgram.ConfigureFonts` — `FlowNRW/MauiProgram.cs` Z. 32–37: alle fünf geplanten Aliase plus `MaterialSymbolsFilled`; OpenSans-Registrierung unverändert erhalten.
- [x] `FlowNRW.csproj unverändert` — `MauiFont Include="Resources\Fonts\*"` (Z. 67) deckt die neuen TTFs ohne Projektänderung ab, wie geplant.
- [x] `Colors.xaml-Tokens` — `GroupStrong`/`GroupStrongDark` (`#EEEDF3`/`#2C2C2E`), `Success`/`Warning`/`Error` (`#006E28`/`#894D00`/`#BA1A1A`) und kontrastdokumentierte Dunkelvarianten `SuccessDark`/`WarningDark`/`ErrorDark` in `Colors.xaml` Z. 27–37; keine vorhandenen Tokens verändert.
- [x] `Styles.xaml-Typografie` — `FontFamily="Inter"` in den impliziten `Button`/`Label`/`Entry`/`Editor`/`Picker`/`SearchBar`/`DatePicker`/`TimePicker`/`CheckBox`/`RadioButton`-Styles; `Headline` 28 + `SubHeadline` 22 auf `InterSemiBold`/Ink-Tokens; `ImageButton`-Style um `CornerRadius` 12 + `GroupStrong`-Fläche ergänzt.
- [x] `TransitIcons` — `FlowNRW/TransitIcons.cs`: alle geplanten Glyphen-Konstanten (`Add`, `MyLocation`, `Refresh`, `Settings`, `ExpandLess`, `ExpandMore`, `OpenInNew`, `Star`, `StarOutline`, `Search`, `Check`, `SwapVert`, `Info`, `Map`, `Schedule`, `Cancel`, `Error`, `DepartureBoard`, `Route`, `LocationOn`); `SetIcon(Button, glyph, description)`, `TabIcon(glyph)` und `IconImage(glyph)`-Fabrik. Alle 17 Codepoints gegen `docs/design/licenses/MaterialSymbolsRounded.codepoints` verifiziert (stimmen exakt überein). Abweichend, aber sinnvoll: gefüllter Stern über `FilledFontFamily` (FILL1) statt eigenem `StarBorder`-Codepoint — im Docstring dokumentiert.
- [x] `TransitVisuals.SecondaryAction` — `FontFamily="MaterialSymbols"`, `FontSize` 24, 48×48, `Padding` 0; `AutomationId`/`SemanticProperties.Description`-Signatur unverändert (`TransitVisuals.cs` Z. 152–167).
- [x] `TransitVisuals.StatusChip` + `IconLabel` — neu implementiert (Z. 115–150): Icon-`Label` + Text-`Label` in Kapsel, Farbe nie alleiniger Träger.
- [x] `TransitVisuals.Text`/`Secondary`/`ApplyRoles` — `Inter`/`InterBold` je `bold`-Flag; sämtliche hartcodierten Hexliterale durch zentrale Konstanten (`Action`, `Ink`, `Muted`, `GroupStrong`, `Success`/`Warning`/`Error` + `Dark`) ersetzt (Z. 10–49, 76–110).
- [x] `DepartureStatus`-Enum — `FlowNRW.Core/Presentation/DepartureStatus.cs` mit `Scheduled`/`OnTime`/`Delayed`/`Cancelled`.
- [x] `DeparturePresentation.Status`/`DelayText` — `DeparturePresentation.cs` Z. 45–67; Ableitung ausschließlich aus `Realtime`-Feldern, keine erfundenen Werte. `BadgeColor`/`Describe` unverändert.
- [x] `DepartureCardView` — Hauptzeit `JetBrainsMono` 18 bold, `StatusChip` für `Cancelled` („Fahrt fällt aus", Error) und `Delayed` (`DelayText`, Warning) neben durchgestrichener Soll-Zeit, `Scheduled`-Hinweis „Keine Echtzeitdaten"; `Describe`-Description unverändert (Z. 35–55).
- [x] `JourneyCardView` — „Ausfall gemeldet" als `StatusChip` (`TransitIcons.Error`, Z. 36); `Spacing` 12 / `Padding` 16; `Detach()` unverändert.
- [x] `JourneyTimelineView.Event` — „Fahrt fällt aus" als `StatusChip` (Z. 76); Konstruktor-Spacing 16, Karten-Padding 16.
- [x] `HomePage` — Shortcut-Reihe via `SecondaryAction` mit `Add`/`Map`/`Refresh`/`Settings` (Z. 50–53); Aktionsgrid mit `SetIcon` inkl. `ExpandLess`/`ExpandMore`-Wechsel samt Description (Z. 260–285); `RenderCards` Spacing 12 / Padding 16, Titel 22 bold; `AddLabel`-Default 13.
- [x] `DeparturePage` — `↻`/`⚙` → `TransitIcons.Refresh`/`Settings` via `SecondaryAction`; `RefreshFavorite()` setzt `ImageSource` = `TransitIcons.IconImage(Star/StarOutline, filled)` + `Text` „Entfernen"/„Speichern", Description-Logik erhalten (Z. 199–206).
- [x] `SearchPage` — `SwapVert` via `SecondaryAction`; `lookup`/`locationButton` via `SetIcon`; `⌕`↔`✓`-Wechsel als `Search`↔`Check` mit synchronem Description-Wechsel (Z. 115–129).
- [x] `StopSearchPage` — `find` (`Search`), `nearby` (`MyLocation`), `showMapButton` (`Map`) als 48×48-`Button`s (Z. 46–52).
- [x] `ResultsPage` — `favoriteConnection` ☆/★ → `SetIcon(Star/StarOutline, filled:)` in `RenderConnectionHeader` (Z. 35, 125–126); `ResultsStatus`-FontSize 24→15 (Z. 39).
- [x] `JourneyDetailPage` — `ShowJourneyMap` via `TransitIcons.Route` (Z. 35); `RenderDetails`-Fallback-`Label` FontSize 18→17 (Z. 111).
- [x] `MapPage` — `reset` (`Refresh`) und `infoButton` (`Info`) via `TransitIcons`/`SetIcon`; Description-Wechsel „Quellen und Kartendaten"/„Kartendaten schließen" erhalten; Leaflet-Marker unverändert.
- [x] `AppShell` — `Tab.Icon` via `TransitIcons.TabIcon` für alle drei Tabs (`DepartureBoard`/`Route`/`LocationOn`), Titel/Routen unverändert; `ToolbarItem` „Haltestellen" bleibt Text (Z. 18–33).
- [x] `Unicode-Glyphen entfernt` — Reposuche über `FlowNRW/*.cs`/`*.xaml` findet keine der Alt-Glyphen (`＋ ⌖ ↻ ⚙ ⌃ ⌄ ▣ ☆ ★ ⌕ ✓ ⇄ ⓘ`) mehr außerhalb der `TransitIcons`-Codepoints.
- [x] `DepartureStatusTests` — `FlowNRW.Tests/DepartureStatusTests.cs`: Status-Matrix (Cancelled/Delayed/OnTime/Scheduled) und `DelayText`-Regeln; `dotnet test` — 9/9 bestanden.
- [x] `UIA-/A11y-Vertrag` (code-seitig) — Buttons bleiben `Button`, `AutomationId`s/`SemanticProperties.Description`s unverändert; dynamische Wechsel laufen synchron über `SetIcon` + Description.

## Offene Aufgaben

- [ ] `UiTest-Build-Verifikation (UIA)` — teilweise: Ein `UiTest`-Build existiert (`FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.dll`, enthält `TransitIcons`/`DepartureStatus`/`StatusChip`/`JetBrainsMono` — neuer Code ist eingebaut). Fehlt: der UIA-Lauf gegen die laufende App (alle AutomationIds auffindbar, InvokePattern-Klicks auf Icon-Buttons, unveränderte Descriptions) — interaktiver Schritt, kein Ausführungsnachweis im Repo.
- [ ] `WindowsJourneyUiTests.ps1` (Happy Path) — komplett interaktiver E2E-Schritt (laufende App + UIA), im Plan als Pflicht-E2E markiert; kein Ausführungsnachweis nach dem Umbau.
- [ ] `WindowsRefreshUiTests.ps1`/`WindowsLifecycleUiTests.ps1`/`WindowsDepartureCacheUiTests.ps1` — interaktive E2E-Regression; kein Ausführungsnachweis nach dem Umbau.
- [ ] `WindowsDesignUiTests.ps1` Screenshot-Matrix neu aufnehmen — interaktiv; vorhandene Matrizen unter `docs/help/design/verification/` (`matrix/`, `final-matrix/`, `revised-matrix/`) datieren auf 2026-10-02/03 und beziehen sich laut `matrix.jsonl` auf den Schritt-9-Build (`uncommitted step 9`), nicht auf den neuen Icon-/Font-Stand. Die geforderte Neuaufnahme (hell/dunkel, 430×900/1024×768, 150 % Skalierung inkl. Icon-Clipping-Check, Status-Chips, Tab-Icons) fehlt.
- [ ] `Separate visuelle Abnahme (AAA-Nachweis)` — interaktiv; die vorhandenen Review-Berichte (`visual-review-oct3.md`, `visual-recheck-root-oct3.md`, `visual-review-increment.md`) beziehen sich auf die Schritt-9-Abnahme, nicht auf den neuen Designstand.
- [ ] `iOS-Checkliste` — Abhaken der bestehenden `acceptance.md`-Checkliste (Schrift, Kontraste, Touch-Ziele, Textskalierung); plattform-/geräteabhängiger Schritt, kein Nachweis im Repo.

## Hinweise

- Alle offenen Aufgaben sind die im Plan explizit als E2E/interaktiv deklarierten Nachweisschritte (Tasks 21–26); sämtliche Code-, Token-, Font- und Unit-Test-Planelemente sind umgesetzt.
- Planerweiternd umgesetzt: `MaterialSymbolsRoundedFill.ttf` + Alias `MaterialSymbolsFilled` + `filled`-Parameter in `SetIcon`/`IconImage` für gefüllte Zustands-Icons (Stern). Sinnvolle Ergänzung, nicht plangegenständlich.
- `App.xaml.cs` Z. 38–44: Icon-Buttons werden bei `FLOWNRW_UI_TEST_TEXT_SCALE=150` weiterhin ×1,5 skaliert (24→36 in 48×48-Buttons). Plan lässt die Ausnahme nur „falls Clipping" zu — der Clipping-Check ist Teil der noch offenen Design-Matrix (Task 24).
- `JourneyTimelineView` nutzt intern teils Spacing 4/6/10 (neben den vereinheitlichten 16); innerhalb der geforderten 4/8/12/16/24-Raster-Toleranz bzw. Bestand — keine Abweichung mit Funktionsfolge.
- Keine neuen Planelemente ohne Task-Zeile gefunden; alle 26 Tasks abgedeckt.
