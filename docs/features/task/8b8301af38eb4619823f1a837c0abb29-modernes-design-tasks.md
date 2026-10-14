# Tasks: Modernes Design auf AAA-Niveau

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---|---|---|---|
| 1 | Typografie | Lizenzfreie Font-Dateien `Inter-Regular.ttf`/`Inter-SemiBold.ttf`/`Inter-Bold.ttf` (SIL OFL), `JetBrainsMono-Bold.ttf` (SIL OFL) und `MaterialSymbolsRounded.ttf` (Apache-2.0, Variante **FILL0/opsz 24**) samt Lizenztexten in `FlowNRW/Resources/Fonts` ablegen; die zugehörige `.codepoints`-Datei der Icon-TTF zusätzlich unter `docs/design/licenses/` (o. ä.) sichern | Offen | — |
| 2 | Typografie | `MauiProgram.ConfigureFonts` um `Inter`/`InterSemiBold`/`InterBold`/`JetBrainsMono`/`MaterialSymbols` erweitern | Offen | — |
| 3 | Design-Tokens | `Colors.xaml` um `GroupStrong` (`#EEEDF3`/`#2C2C2E`) und `Success`/`Warning`/`Error` inkl. kontrastgerechter Dunkelvarianten erweitern | Offen | — |
| 4 | Design-Tokens | `Styles.xaml`: `FontFamily="Inter"` in impliziten Text-/Eingabe-Styles setzen, `Headline`/`SubHeadline` auf Ink-Tokens und Skala 28/22 bringen | Offen | — |
| 5 | Icons | `TransitIcons` anlegen: Glyph-Konstanten (Material-Symbols-Codepoints gegen `.codepoints`-Datei verifizieren), `SetIcon(Button, glyph)`, `TabIcon(glyph)`/`FontImageSource`-Fabrik | Offen | — |
| 6 | Icons | `TransitVisuals.SecondaryAction` auf Icon-Font-Glyphen umstellen (`FontFamily="MaterialSymbols"`, FontSize ~24, 48×48; AutomationId/Description unverändert) | Offen | — |
| 7 | Icons | `TransitVisuals.ApplyRoles`/`Text`/`Secondary` auf neue Tokens und Inter-Aliase vereinheitlichen (hartcodierte Hexwerte beseitigen) | Offen | — |
| 8 | Listeneinträge | `DeparturePresentation`: `DepartureStatus`-Enum, `Status(StopEvent)` und `DelayText(StopEvent)` ergänzen | Offen | — |
| 9 | Listeneinträge | `DepartureCardView`: Hauptzeit auf `JetBrainsMono` (18 bold), Status-Chips für Ausfall/Verspätung/keine Echtzeit via `TransitVisuals.StatusChip` | Offen | — |
| 10 | Listeneinträge | `JourneyCardView`: „Ausfall gemeldet" als `StatusChip`, Spacing auf 12/16-Skala | Offen | — |
| 11 | Listeneinträge | `JourneyTimelineView.Event`: „Fahrt fällt aus" als `StatusChip`, Spacing vereinheitlichen | Offen | — |
| 12 | Listeneinträge | `TransitVisuals.Candidate` und `HomePage.RenderCards`/`AddLabel` auf einheitliches Spacing und Typo-Rollen verdichten | Offen | — |
| 13 | Icons | `HomePage`: Shortcut-Reihe und Favoriten-Aktionsgrid auf `TransitIcons` umstellen (inkl. `⌃`/`⌄`-Wechsel via `SetIcon`) | Offen | — |
| 14 | Icons | `DeparturePage`: `↻`/`⚙` auf `TransitIcons`; `ToggleFavorite` auf `FontImageSource`-Icon (`Star`/`StarOutline`) + Text „Speichern"/„Entfernen" | Offen | — |
| 15 | Icons | `SearchPage`: `SwapEndpoints`-, `{prefix}Search`-, `{prefix}Location`-Buttons auf `TransitIcons`; `⌕`↔`✓`-Moduswechsel via `SetIcon` (`Search`↔`Check`) | Offen | — |
| 16 | Icons | `StopSearchPage`: `FindStops`/`FindNearbyStops`/`ShowStopMap` auf `TransitIcons` | Offen | — |
| 17 | Icons | `ResultsPage`: `ToggleConnectionFavorite` ☆/★ via `SetIcon`; `JourneyDetailPage`: `ShowJourneyMap`; `MapPage`: `ResetMap`/`MapInformation` auf `TransitIcons` | Offen | — |
| 18 | Icons | `AppShell`: `Tab.Icon` für die drei Tabs via `FontImageSource` setzen (`DepartureBoard`/`Route`/`LocationOn`; Titel/Routen unverändert); WinUI-Rendering früh sichtbar prüfen. Bewusst unverändert: `ToolbarItem` „Haltestellen" (Text), Linienbadges (Text-Badges), Leaflet-Kartenmarker | Offen | — |
| 19 | Design-Tokens | Verstreute FontSize-Ausreißer normalisieren (`ResultsStatus` 24→15, `JourneyDetailSection`-Fallback 18→17) | Offen | — |
| 20 | Tests | `DepartureStatusTests` anlegen: Status-Matrix (Cancelled/Delayed/OnTime/Scheduled) und `DelayText`-Regeln gegen `TransitTestSupport`-Fixtures | Offen | — |
| 21 | Tests | `UiTest`-Build verifizieren: alle AutomationIds auffindbar, InvokePattern-Klicks auf Icon-Buttons funktionieren, `SemanticProperties.Description`s unverändert | Offen | — |
| 22 | E2E-Tests | `WindowsJourneyUiTests.ps1` vollständig ausführen (Happy Path: Tabs, Haltestelle→Monitor→Favorit, Suche→Ergebnis→Detail→Karte, ☆/★-Toggle) | Offen | — |
| 23 | E2E-Tests | `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsDepartureCacheUiTests.ps1` erneut ausführen | Offen | — |
| 24 | E2E-Tests | `WindowsDesignUiTests.ps1`: Screenshot-Matrix neu aufnehmen (430×900/1024×768, hell/dunkel, 150 % Textskalierung inkl. Icon-Clipping-Check, Ausfall-/Verspätungs-/Leerzustände, Tab-Bar mit Icons) | Offen | — |
| 25 | E2E-Tests | Separate visuelle Abnahme gegen `docs/design/acceptance.md` + Referenz-HTMLs (Nachweis des „AAA"-Qualitätssprungs); Bericht und Screenshots unter `docs/help/design/verification` ablegen | Offen | — |
| 26 | E2E-Tests | iOS-Abdeckung über die bestehende `acceptance.md`-Checkliste abhaken (Schrift, Kontraste, Touch-Ziele, Textskalierung) — kein eigener Gerätematrix-Lauf | Offen | — |
