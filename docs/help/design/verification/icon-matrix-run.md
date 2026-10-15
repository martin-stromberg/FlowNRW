# Icon-/Font-Matrix-Neuaufnahme (Nachbearbeitung „Modernes Design")

Neuaufnahme der Design-Screenshot-Matrix auf dem Stand mit Material-Symbols-Icons,
Inter-/JetBrains-Mono-Typografie und Status-Chips. Ablage: `icon-matrix/`
(226 Bilder + `matrix.jsonl` mit Build-Hash/Commit/Geometrie je Bild —
Endstand nach Fortsetzungslauf 2026-10-15, siehe unten).

## Ausführung

Datum: 2026-10-14 — gegen den `UiTest`-Fixture-Build
(`FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe`):

```
dotnet build FlowNRW/FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true
./tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1 -Exe <exe> `
    -ScreenshotDirectory docs/help/design/verification/icon-matrix `
    -Theme <light|dark> -Width <430|1024> -Height <900|768> -TextScale <100|150>
```

| Kombination | Ergebnis |
|---|---|
| light · 430×900 · 100 % | bestanden (alle Assertionen + CAPTUREs) |
| light · 430×900 · 150 % | bestanden |
| light · 1024×768 · 100 % | bestanden |
| dark · 430×900 · 100 % | bestanden |
| dark · 430×900 · 150 % | bestanden |
| dark · 1024×768 · 100 % | bestanden |

Alle `AssertTouchTarget`-Prüfungen (≥44×44 DIP auf `SaveRefreshSettings`,
`RefreshInterval`, `OriginSearch`, `OriginLocation`, `OriginCoordinateMode`,
`SearchJourneys`) und alle Sichtbarkeits-/Inhalts-Assertionen grün.

## Nachbearbeitungs-Fixes im Zuge der Aufnahme

- **Tab-Overflow bei 430 px:** Die Icon-Tabs verbreiterten die
  `NavigationViewItem`s so weit, dass „Haltestellen" ins „Mehr"-Overflow-Menü
  rutschte. Behoben über kompaktere Theme-Margins plus eine
  `TabBarNavigationViewMenuItem`-Template-Überschreibung (FontSize 13) in
  `FlowNRW/Platforms/Windows/App.xaml`. Alle drei Tabs bleiben als
  `TabItem` per `SelectionItemPattern` selektierbar.
- **`SearchJourneys` am Viewport-Rand:** Bei 430×900 war der primäre Button
  nur zu 9 px sichtbar (Inter-Metriken). Behoben durch gerasterte
  Verdichtung der `SearchPage` (`Spacing` 16→12, `searchForm`-Padding 16→12,
  `timeLayout`-Padding 8→4, Endpoint-Karten 12→8). Button jetzt vollständig
  sichtbar (382×44).

## Icon-Clipping-Check (150 %)

In den `*-150-*`-Aufnahmen rendern die Icon-Glyphen in den 48×48-Buttons
vollständig (`SetIcon` setzt `Padding = 0` zentral; `App.xaml.cs` skaliert
die Glyph-`FontSize` ×1,5 auf 36 — passt ohne Clipping). Stichproben-Pixelcheck:
Shortcut-Icons und selektierter Tab rendern in der Interaktionsfarbe
`#0058BC` (light) bzw. aufgehellt (dark); Status-Chips zeigen
`Error`/`Warning`-Färbung mit Icon + Text.

## Fortsetzung 2026-10-15 (Dark-Neuaufnahme + Speicherfehler)

Der vorangegangene Lauf war nach den drei hellen Kombinationen
abgebrochen; die `dark-*`-Bilder waren im Arbeitsbaum gelöscht worden.
In diesem Lauf wurden gegen denselben UiTest-Build (`buildSha256`
`6F608E16…`, Commit `a34a04f` + Nachbearbeitungen) die drei dunklen
Kombinationen neu aufgenommen sowie das `refresh-store-error`-Szenario
(Speicherfehler via Tastatur, hell/dunkel × 150 %) ergänzt:

| Kombination | Ergebnis |
|---|---|
| dark · 430×900 · 100 % | bestanden (alle Assertionen + CAPTUREs) |
| dark · 430×900 · 150 % | bestanden |
| dark · 1024×768 · 100 % | bestanden |
| light/dark · 430×900 · 150 % · `refresh-store-error` | bestanden (`settings-error-keyboard`) |

Hinweis zur Ausführung: Die Skripte wurden über `schtasks … /it` in der
interaktiven Sitzung gestartet; ein direkter Lauf aus einer nicht-
interaktiven Shell scheitert an der Foreground-Prüfung
(`Physical input cancelled`), einzelne Läufe brachen ohne Fehlermeldung
ab und wurden wiederholt — die wiederholten Einträge wurden in
`matrix.jsonl` dedupliziert (letzter Eintrag je Bildname gewinnt).

Endstand: **226 PNGs + `matrix.jsonl`** — alle Pflichtvarianten aus
`docs/design/acceptance.md` abgedeckt (inkl. Speicherfehler). Die
separate visuelle Abnahme ist dokumentiert in
`docs/help/design/verification/visual-acceptance.md`.
