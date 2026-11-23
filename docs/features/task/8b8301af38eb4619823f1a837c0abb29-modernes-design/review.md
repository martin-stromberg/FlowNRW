# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

- [x] `Font-Assets + Lizenzen` — `Inter-Regular.ttf`/`Inter-SemiBold.ttf`/`Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf`, `MaterialSymbolsRounded.ttf` (FILL0/opsz 24) in `FlowNRW/Resources/Fonts/`; Lizenztexte (`Inter-LICENSE.txt`, `JetBrainsMono-OFL.txt`, `MaterialSymbols-LICENSE.txt`) und `MaterialSymbolsRounded.codepoints` unter `docs/design/licenses/`. Zusätzlich `MaterialSymbolsRoundedFill.ttf` (FILL1) für gefüllte Zustands-Icons (planerweiternd, sinnvoll).
- [x] `MauiProgram.ConfigureFonts` — `FlowNRW/MauiProgram.cs` Z. 30–35: alle fünf geplanten Aliase plus `MaterialSymbolsFilled`; OpenSans-Registrierung erhalten.
- [x] `FlowNRW.csproj unverändert` — `MauiFont`-Glob deckt neue TTFs ab, wie geplant.
- [x] `Colors.xaml-Tokens` — `GroupStrong`/`GroupStrongDark` (`#EEEDF3`/`#2C2C2E`), `Success`/`Warning`/`Error` (`#006E28`/`#894D00`/`#BA1A1A`) und kontrastdokumentierte Dunkelvarianten (`Colors.xaml` Z. 27–37, Kontrastwerte im Kommentar dokumentiert); keine Bestands-Tokens verändert.
- [x] `Styles.xaml-Typografie` — `FontFamily="Inter"` in impliziten Styles (Z. 30, 74, 95, 117, 162, 198, 236, 263, 349); `Headline`/`SubHeadline` auf `InterSemiBold` + Ink-Tokens + Skala 28/22 (Z. 178–191); `ImageButton`-Style mit `CornerRadius` 12 + `GroupStrong`-Fläche ergänzt (Z. 136–141).
- [x] `TransitIcons` — `FlowNRW/TransitIcons.cs`: alle geplanten Glyph-Konstanten (`Add`, `MyLocation`, `Refresh`, `Settings`, `ExpandLess`, `ExpandMore`, `Star`, `StarOutline`, `Search`, `Check`, `SwapVert`, `Info`, `Map`, `Schedule`, `Cancel`, `Error`, `DepartureBoard`, `Route`, `LocationOn`); `SetIcon` (Z. 84), `IconImage` (Z. 101), `TabIcon` (Z. 118). Codepoints laut Klassendokumentation gegen `MaterialSymbolsRounded.codepoints` verifiziert.
- [x] `TransitVisuals.SecondaryAction` — Icon-Font-Glyphen via `TransitIcons.SetIcon` (FontSize 24, 48×48, `Padding` 0); `AutomationId`/`Description`-Signatur unverändert (`TransitVisuals.cs` Z. 163–174).
- [x] `TransitVisuals.StatusChip` + `IconLabel` — Icon + Text in Kapsel, Farbe nie alleiniger Träger (Z. 126–161).
- [x] `TransitVisuals.Text`/`Secondary`/`ApplyRoles`/`Candidate` — Inter-Aliase (`Inter`/`InterBold` via `Fonts`-Konstanten, Z. 99); sämtliche Farbwerte über zentrale Konstanten/Token-Referenzen (Z. 10–49, 87–88).
- [x] `DepartureStatus`-Enum + `DeparturePresentation.Status`/`DelayText` — `FlowNRW.Core/Presentation/DepartureStatus.cs` + `DeparturePresentation.cs` Z. 45–52, 68–75; Ableitung nur aus `Realtime`-Feldern (inkl. `EffectiveDelay`-Teilung mit `Describe`); `BadgeColor`/`Describe` unverändert.
- [x] `DepartureCardView` — `JetBrainsMono`-Hauptzeit (Z. 38), `StatusChip` für `Cancelled` (Z. 46) und `Delayed` (Z. 48), `Describe`-Description unverändert (Z. 31).
- [x] `JourneyCardView`/`JourneyTimelineView` — Ausfall-`StatusChip` (`JourneyCardView.cs` Z. 36, `JourneyTimelineView.cs` Z. 71), Spacing auf Skala (12/16).
- [x] `HomePage` — Shortcuts via `SecondaryAction`+`TransitIcons` (Z. 50–53), `SetIcon`-Wechsel `ExpandLess`/`ExpandMore` mit synchroner Description (Z. 289–290), `Refresh`/`DepartureBoard`/`StarOutline` im Aktionsgrid (Z. 294–312).
- [x] `DeparturePage` — `Refresh`/`Settings` via `SecondaryAction` (Z. 62–63); `RefreshFavorite` setzt `ImageSource` = `IconImage(Star/StarOutline, filled:)` + Text „Entfernen"/„Speichern" + Description-Wechsel (Z. 199–205).
- [x] `SearchPage` — `SwapVert` (Z. 48), `lookup`/`locationButton` via `SetIcon` (Z. 116–118), `Search`↔`Check`-Moduswechsel mit synchroner Description (Z. 129); dokumentierte Matrix-Fix-Verdichtung aus `icon-matrix-run.md` im Code.
- [x] `StopSearchPage`/`ResultsPage`/`JourneyDetailPage`/`MapPage` — alle geplanten Glyph-Ersetzungen via `TransitIcons`/`SetIcon`/`SecondaryAction` (`StopSearchPage` Z. 47–50, `ResultsPage` Z. 35+125, `JourneyDetailPage` Z. 35, `MapPage` Z. 47+56); `ResultsStatus` 15 (Z. 39), `JourneyDetailSection`-Fallback 17 (Z. 111).
- [x] `AppShell`-Tab-Icons — `Tab.Icon` via `TransitIcons.TabIcon` für alle drei Tabs (`DepartureBoard`/`Route`/`LocationOn`), Titel/Routen unverändert (`AppShell.xaml.cs` Z. 18–20); `ToolbarItem` „Haltestellen" bleibt Text. WinUI-Rendering in der Matrix belegt; Tab-Overflow-Fix (`TabBarNavigationViewMenuItem`-Template) in `Platforms/Windows/App.xaml`.
- [x] `Unicode-Glyphen entfernt` — Reposuche über `FlowNRW/*.cs`/`*.xaml` findet keine der Alt-Glyphen (`＋ ⌖ ↻ ⚙ ⌃ ⌄ ▣ ☆ ★ ⌕ ✓ ⇄ ⓘ`) mehr.
- [x] `DepartureStatusTests` — `FlowNRW.Tests/DepartureStatusTests.cs` mit 11 Testmethoden (Status-Matrix, `DelayText`-Regeln, `EffectiveDelay`-Teilung mit `Describe`); dokumentierter Gesamtlauf `dotnet test` 291/291 bestanden (`test-results.md`).
- [x] `UIA-/A11y-Vertrag` — Buttons bleiben `Button`, AutomationIds/Descriptions unverändert; InvokePattern-Klicks auf Icon-Buttons und `AssertTouchTarget` (≥44×44) in den dokumentierten Läufen bestanden.
- [x] `WindowsJourneyUiTests.ps1` (Happy Path) — komplett bestanden inkl. Modi `-Monitors`/`-Favorites`/`-Locations`/`-Maps` (Nachbearbeitungslauf 2026-10-14, dokumentiert in `test-results.md` und `icon-matrix-run.md`).
- [x] `WindowsRefreshUiTests.ps1`/`WindowsLifecycleUiTests.ps1`/`WindowsDepartureCacheUiTests.ps1` — alle bestanden (dokumentiert in `test-results.md`).
- [x] `WindowsDesignUiTests.ps1` Screenshot-Matrix neu aufgenommen — `docs/help/design/verification/icon-matrix/`: 222 PNGs + `matrix.jsonl` (222 Zeilen, Commit `3665332577a25ada4b628f0715552c43f370a3d5` = HEAD); 6 Kombinationen (light/dark × 430×900/1024×768 × 100 %/150 %) abgedeckt, Ausfall-/Verspätungs-/Leer-/Fehlerzustände und Tab-Bar-Szenarien enthalten; Icon-Clipping-Check (150 %) in `icon-matrix-run.md` dokumentiert.

## Offene Aufgaben

- [ ] `Separate visuelle Abnahme (AAA-Nachweis)` — Task 25. Subjektive Bewertung des Qualitätssprungs gegen `docs/design/acceptance.md` + Referenz-HTMLs; per `acceptance.md`-Freigaberegel ein manueller, unabhängiger Sichtprüfungs-Schritt, der nicht automatisierbar ist. Die vollständige neue Matrix liegt zur Abnahme unter `docs/help/design/verification/icon-matrix/` bereit; `icon-matrix-run.md` enthält den technischen Befund (inkl. Stichproben-Pixelcheck der Icon-Färbung und Status-Chips).
- [ ] `iOS-Checkliste` — Task 26. Abhaken der `acceptance.md`-Checkliste (Schrift, Kontraste, Touch-Ziele, Textskalierung) erfordert ein iOS-Gerät/Mac; auf diesem Windows-Rechner nicht ausführbar — manueller Schritt.

## Hinweise

- **Keine automatisierbaren Umsetzungslücken mehr vorhanden.** Alle code- und artefaktseitig prüfbaren Planelemente sind umgesetzt und belegt. Die beiden einzigen offenen Aufgaben (25, 26) sind per Definition manuelle, nicht-automatisierbare Abnahmeschritte: eine subjektive visuelle Bewertung durch einen Menschen bzw. ein physisch nicht verfügbares iOS-Gerät. Der Code- und Nachweisstand ist damit abschlussreif; die Restpunkte sind Termine für die menschliche Abnahme.
- Iteration 2 → 3: Keine Änderung am offenen Punktestand; erneute Verifikation bestätigt alle zuvor als erledigt markierten Elemente unverändert. `DepartureStatusTests.cs` enthält inzwischen 11 Testmethoden (zwei `EffectiveDelay`-Tests und `Status_PositiveDelayField_ReturnsDelayed` gegenüber dem in Iteration 2 dokumentierten Stand von 9 hinzugekommen); der dokumentierte Gesamtlauf bleibt 291/291 (`test-results.md`).
- Planerweiternd umgesetzt (aus früheren Iterationen bestätigt): `MaterialSymbolsRoundedFill.ttf` + `MaterialSymbolsFilled`-Alias + `filled`-Parameter für gefüllte Sterne. Zwei dokumentierte Nachbearbeitungs-Fixes aus der Matrix-Aufnahme im Code verifiziert (Tab-Overflow-Template-Override, `SearchPage`-Verdichtung).
- `matrix.jsonl` referenziert Commit `36653325…` (= HEAD); alle 222 Bilddateien vorhanden und konsistent.
