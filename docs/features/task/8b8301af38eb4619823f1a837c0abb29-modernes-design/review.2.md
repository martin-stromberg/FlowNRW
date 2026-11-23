# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

- [x] `Font-Assets + Lizenzen` — `Inter-Regular.ttf`/`Inter-SemiBold.ttf`/`Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf`, `MaterialSymbolsRounded.ttf` (FILL0/opsz 24) in `FlowNRW/Resources/Fonts/`; Lizenztexte (`Inter-LICENSE.txt`, `JetBrainsMono-OFL.txt`, `MaterialSymbols-LICENSE.txt`) und `MaterialSymbolsRounded.codepoints` unter `docs/design/licenses/`. Zusätzlich `MaterialSymbolsRoundedFill.ttf` (FILL1) für gefüllte Zustands-Icons (planerweiternd, sinnvoll).
- [x] `MauiProgram.ConfigureFonts` — `FlowNRW/MauiProgram.cs` Z. 30–35: alle fünf geplanten Aliase plus `MaterialSymbolsFilled`; OpenSans-Registrierung erhalten.
- [x] `FlowNRW.csproj unverändert` — `MauiFont`-Glob deckt neue TTFs ab, wie geplant.
- [x] `Colors.xaml-Tokens` — `GroupStrong`/`GroupStrongDark` (`#EEEDF3`/`#2C2C2E`), `Success`/`Warning`/`Error` (`#006E28`/`#894D00`/`#BA1A1A`) und kontrastdokumentierte Dunkelvarianten (`Colors.xaml` Z. 27–37, Kontrastwerte im Kommentar dokumentiert); keine Bestands-Tokens verändert.
- [x] `Styles.xaml-Typografie` — `FontFamily="Inter"` in impliziten Styles (Z. 30, 74, 95, 117, 162, 198, 236, 263, 349); `Headline`/`SubHeadline` auf `InterSemiBold` + Skala (Z. 178–188); `ImageButton`-Style ergänzt.
- [x] `TransitIcons` — `FlowNRW/TransitIcons.cs`: alle geplanten Glyph-Konstanten (`Add`, `MyLocation`, `Refresh`, `Settings`, `ExpandLess`, `ExpandMore`, `Star`, `StarOutline`, `Search`, `Check`, `SwapVert`, `Info`, `Map`, `Schedule`, `Cancel`, `Error`, `DepartureBoard`, `Route`, `LocationOn`); `SetIcon(Button, glyph, description, filled, size)` (Z. 84), `IconImage` (Z. 101), `TabIcon` (Z. 118). Codepoints gegen `MaterialSymbolsRounded.codepoints` verifiziert.
- [x] `TransitVisuals.SecondaryAction` — Icon-Font-Glyphen (`FontFamily="MaterialSymbols"`, FontSize 24, 48×48); `AutomationId`/`Description`-Signatur unverändert (`TransitVisuals.cs` Z. 152).
- [x] `TransitVisuals.StatusChip` + `IconLabel` — Icon + Text in Kapsel, Farbe nie alleiniger Träger (Z. 115–150).
- [x] `TransitVisuals.Text`/`Secondary`/`ApplyRoles`/`Candidate` — Inter-Aliase; sämtliche hartcodierten Hexliterale durch zentrale Konstanten ersetzt (Z. 10–49).
- [x] `DepartureStatus`-Enum + `DeparturePresentation.Status`/`DelayText` — `FlowNRW.Core/Presentation/DepartureStatus.cs` + `DeparturePresentation.cs` Z. 45–58; Ableitung nur aus `Realtime`-Feldern, `BadgeColor`/`Describe` unverändert.
- [x] `DepartureCardView` — `JetBrainsMono`-Hauptzeit (Z. 38), `StatusChip` für `Cancelled` (Z. 51) und `Delayed` (Z. 53), `Scheduled`-Hinweis; `Describe`-Description unverändert (Z. 31).
- [x] `JourneyCardView`/`JourneyTimelineView` — Ausfall-`StatusChip` (`JourneyCardView.cs` Z. 36, `JourneyTimelineView.cs` Z. 76), Spacing auf Skala.
- [x] `HomePage` — Shortcuts via `SecondaryAction`+`TransitIcons` (Z. 50–53), `SetIcon`-Wechsel `ExpandLess`/`ExpandMore` mit Description (Z. 286–287), `Refresh`/`DepartureBoard`/`StarOutline` im Aktionsgrid (Z. 291–309), verdichtete `RenderCards`.
- [x] `DeparturePage` — `Refresh`/`Settings` via `SecondaryAction` (Z. 62–63); `RefreshFavorite` setzt `ImageSource` = `IconImage(Star/StarOutline, filled:)` + Text „Entfernen"/„Speichern" (Z. 202).
- [x] `SearchPage` — `SwapVert` (Z. 48), `lookup`/`locationButton` via `SetIcon` (Z. 116–118), `Search`↔`Check`-Moduswechsel mit synchroner Description (Z. 129); Matrix-Fix: gerasterte Verdichtung (Spacing 12, `searchForm`-Padding 12, `timeLayout`-Padding 4, Endpoint-Karten 8) dokumentiert in `icon-matrix-run.md`.
- [x] `StopSearchPage`/`ResultsPage`/`JourneyDetailPage`/`MapPage` — alle geplanten Glyph-Ersetzungen via `TransitIcons`/`SetIcon` (`StopSearchPage` Z. 47–50, `ResultsPage` Z. 35+125, `JourneyDetailPage` Z. 35, `MapPage` Z. 47+56); `ResultsStatus` 24→15, Fallback 18→17.
- [x] `AppShell`-Tab-Icons — `Tab.Icon` via `TransitIcons.TabIcon` für alle drei Tabs (`DepartureBoard`/`Route`/`LocationOn`), Titel/Routen unverändert (`AppShell.xaml.cs` Z. 18–20); `ToolbarItem` „Haltestellen" bleibt Text. WinUI-Rendering in der Matrix belegt; Tab-Overflow-Fix (`TabBarNavigationViewMenuItem`-Template, FontSize 13) in `Platforms/Windows/App.xaml` Z. 18–27.
- [x] `Unicode-Glyphen entfernt` — Reposuche über `FlowNRW/*.cs`/`*.xaml` findet keine der Alt-Glyphen (`＋ ⌖ ↻ ⚙ ⌃ ⌄ ▣ ☆ ★ ⌕ ✓ ⇄ ⓘ`) mehr außerhalb von `TransitIcons.cs`.
- [x] `DepartureStatusTests` — `FlowNRW.Tests/DepartureStatusTests.cs` mit 9 Tests (Status-Matrix + `DelayText`-Regeln); Gesamtlauf `dotnet test` 291/291 bestanden (`test-results.md`).
- [x] `UIA-/A11y-Vertrag` — Buttons bleiben `Button`, AutomationIds/Descriptions unverändert; InvokePattern-Klicks auf Icon-Buttons im `WindowsJourneyUiTests.ps1`-Lauf und `AssertTouchTarget` (≥44×44) in der Design-Matrix bestanden.
- [x] `WindowsJourneyUiTests.ps1` (Happy Path) — komplett bestanden inkl. Modi `-Monitors`/`-Favorites`/`-Locations`/`-Maps` (Nachbearbeitungslauf 2026-10-14, dokumentiert in `test-results.md` Z. 21 und `icon-matrix-run.md`).
- [x] `WindowsRefreshUiTests.ps1`/`WindowsLifecycleUiTests.ps1`/`WindowsDepartureCacheUiTests.ps1` — alle bestanden (dokumentiert in `test-results.md` Z. 21).
- [x] `WindowsDesignUiTests.ps1` Screenshot-Matrix neu aufgenommen — `docs/help/design/verification/icon-matrix/`: 222 PNGs + `matrix.jsonl`; verifiziert: 6 Kombinationen (light/dark × 430×900/1024×768 × 100 %/150 %) × je 37 Szenarien vollständig, alle in `matrix.jsonl` referenzierten Dateien vorhanden, Timestamps 2026-10-14 21:47–22:01, Commit `36653325…` (= HEAD), einer der zwei `buildSha256`-Werte (`DEFC717D…`) identisch mit dem Hash der vorliegenden `FlowNRW.dll` im `UiTest`-Build (zweiter Hash = Zwischenbuild vor den dokumentierten Nachbearbeitungs-Fixes). Ausfall-/Verspätungs-/Leer-/Fehlerzustände und Tab-Bar-Szenarien in den 37 Szenarien enthalten; Icon-Clipping-Check (150 %) in `icon-matrix-run.md` dokumentiert.

## Offene Aufgaben

- [ ] `Separate visuelle Abnahme (AAA-Nachweis)` — Task 25. Subjektive Bewertung des Qualitätssprungs gegen `docs/design/acceptance.md` + Referenz-HTMLs; per `acceptance.md`-Freigaberegel ein manueller, unabhängiger Sichtprüfungs-Schritt, der nicht automatisierbar ist. Die vollständige neue Matrix liegt zur Abnahme unter `docs/help/design/verification/icon-matrix/` bereit; `icon-matrix-run.md` enthält den technischen Befund (inkl. Stichproben-Pixelcheck der Icon-Färbung und Status-Chips).
- [ ] `iOS-Checkliste` — Task 26. Abhaken der `acceptance.md`-Checkliste (Schrift, Kontraste, Touch-Ziele, Textskalierung) erfordert ein iOS-Gerät/Mac; auf diesem Windows-Rechner nicht ausführbar — manueller Schritt.

## Hinweise

- **Alle automatisierbaren Planelemente sind umgesetzt und belegt.** Die beiden einzigen offenen Aufgaben (25, 26) sind per Definition manuelle, nicht-automatisierbare Abnahmeschritte: eine subjektive visuelle Bewertung durch einen Menschen bzw. ein physisch nicht verfügbares iOS-Gerät. Der Code- und Nachweisstand ist damit abschlussreif; die Restpunkte sind Termine für die menschliche Abnahme, keine Umsetzungslücken.
- Iteration 1 → 2: Die vier damals offenen Windows-E2E-Nachweise (Tasks 21–24) sind inzwischen alle erbracht — Unit-Test-Gesamtlauf (291/291), UIA-Läufe der vier Skripte und die vollständig neu aufgenommene Design-Matrix (222 Bilder, `matrix.jsonl`-Integrität inkl. Build-Hash-Abgleich gegen den vorliegenden UiTest-Build verifiziert).
- Planerweiternd umgesetzt (Dokumentation aus Iteration 1 bestätigt): `MaterialSymbolsRoundedFill.ttf` + `MaterialSymbolsFilled`-Alias + `filled`-Parameter für gefüllte Sterne. Zwei dokumentierte Nachbearbeitungs-Fixes aus der Matrix-Aufnahme im Code verifiziert (Tab-Overflow-Template-Override, `SearchPage`-Verdichtung).
- `matrix.jsonl` enthält zwei `buildSha256`-Werte — erklärbar durch den Rebuild nach den im Lauf dokumentierten Fixes; beide Artefakt-Serien sind konsistent.
