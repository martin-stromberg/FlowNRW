# Design- und Assetquellen

Bestehende, repo-seitig versionierte Quellen für das Erscheinungsbild der Anwendung — Grundlage für „an das Erscheinungsbild der Anwendung angelehnt" sowie für helle/dunkle Darstellung.

## Farb-Tokens

Datei: `FlowNRW/Resources/Styles/Colors.xaml` (49 Zeilen)

| Token | Wert | Rolle |
|---|---|---|
| `Primary` | `#0058BC` | Primärfarbe (hell) |
| `PrimaryDark` | `#A5C9FF` | Primärfarbe (dunkel) |
| `PrimaryDarkText` | `#242424` | Text auf PrimaryDark |
| `Secondary` | `#E8F0FC` | Sekundärfläche |
| `SecondaryDarkText` | `#F5F5F7` | Text auf Secondary (dunkel) |
| `CanvasLight` / `CanvasDark` | `#FAF9FE` / `#000000` | Hintergrund hell/dunkel |
| `CardLight` / `CardDark` | `#FFFFFF` / `#1C1C1E` | Karten hell/dunkel |
| `InkLight` / `InkDark` | `#1A1B1F` / `#F5F5F7` | Primärtext hell/dunkel |
| `MutedLight` / `MutedDark` | `#414755` / `#C5C7D0` | Sekundärtext hell/dunkel |
| `OutlineLight` / `OutlineDark` | `#E3E2E7` / `#414149` | Linien/Ränder hell/dunkel |
| `GroupStrong` / `GroupStrongDark` | `#EEEDF3` / `#2C2C2E` | Starke Gruppierung |
| `Success` / `SuccessDark` | `#006E28` / `#3ECF6B` | Bestätigung |
| `Warning` / `WarningDark` | `#894D00` / `#F0A840` | Warnung |
| `Error` / `ErrorDark` | `#BA1A1A` / `#FF897D` | Fehler/Ausfall |
| `Magenta`, `OffBlack`, `Gray100`–`Gray950` | div. | Weitere Palette |

Der XAML-Kommentar (Zeilen 29–31) dokumentiert geprüfte Kontrastwerte der Dark-Semantik auf `#1C1C1E`/`#2C2C2E`: `SuccessDark` ≈ 8,4:1/6,9:1, `WarningDark` ≈ 8,4:1/6,9:1, `ErrorDark` ≈ 7,4:1/6,1:1 — alle ≥ 4,5:1 (WCAG AA Normaltext).

## Verbindliche Designkriterien

Datei: `docs/design/acceptance.md` (58 Zeilen) — „Verbindliche visuelle Abnahme", Stand 24.09.2026.

- Token-Tabelle (Zeilen 15–26): Hintergrund/Karten `#FAF9FE`/`#FFFFFF`, Gruppen `#F4F3F8`/`#EEEDF3`; Text `#1A1B1F`/`#414755`; Interaktion `#0058BC`, aktiver Container `#0070EB`; Linienkennfarben RE/RB `#BA1B1D`, S `#008D43`, U `#005A9C`, Tram `#E35205`, Bus `#6F2C91`, Fähre `#009AA6`; dunkle Flächen `#000000`/`#1C1C1E`/`#2C2C2E`.
- Typografie-Hierarchie (Zeile 23): Titel 28–34, Abschnitt 22, Haupttext 17, Nebeninformation 15, Metadaten 13 logische Einheiten; Abfahrtszahlen Monospace 18, Verzögerung 14.
- Raster/Form (Zeilen 24–25): 4/8/12/16/24/32-Raster; Kartenradius 16–20, Felder 10, Buttons 12, Linienbadges 6–8, Statuschips als Kapsel.
- Kontrastregeln (Zeile 28): normaler Text ≥ 4,5:1, großer Text/bedienrelevante Konturen ≥ 3:1.
- Prioritätsregel (Zeile 9): drei beschriftete Kernbereiche Abfahrten, Verbindungen, Haltestellen.

## Schriften und Icon-Font

Gebündelte Fontdateien: `FlowNRW/Resources/Fonts/` —
`Inter-Regular.ttf`, `Inter-SemiBold.ttf`, `Inter-Bold.ttf`, `JetBrainsMono-Bold.ttf`,
`MaterialSymbolsRounded.ttf`, `MaterialSymbolsRoundedFill.ttf`.

Registrierung in `FlowNRW/MauiProgram.cs` (Zeilen 30–35) über `fonts.AddFont(...)` mit den Aliasen `Fonts.Inter`, `Fonts.InterSemiBold`, `Fonts.InterBold`, `Fonts.JetBrainsMono`, `TransitIcons.FontFamily`, `TransitIcons.FilledFontFamily`.

Lizenz- und Manifestdateien: `docs/design/licenses/` —
`Inter-LICENSE.txt` (SIL OFL), `JetBrainsMono-OFL.txt` (OFL), `MaterialSymbols-LICENSE.txt` (Apache 2.0), `MaterialSymbolsRounded.codepoints` (Codepoint-Manifest). README-Zeilen 154–156 dokumentieren die Lizenzlage.

## App-Icon und Bildmarken

- `FlowNRW/Resources/AppIcon/`: `appicon.svg`, `appiconfg.svg`, `flownrw.png` — produktives App-Icon.
- `FlowNRW/Resources/Splash/splash.svg` — Splash-Grafik.
- `docs/design/reference/flownrw_icon.png` — Referenzkopie des App-Icons.

## Design-Referenz (nur lesend, nicht auslieferbar)

`docs/design/reference/`: vier HTML-Entwürfe (`abfahrtsmonitor_live/code.html`, `verbindungssuche/code.html`, `fahrtbegleiter_detail/code.html`, `umgebungskarte_stationen/code.html`), `trans_nrw_mobil_system/` (u. a. `DESIGN.md`) und `flownrw_app_flow_dokumentation.md`. Laut `acceptance.md` Zeile 7 sind die HTML-Dateien visuelle Referenzen mit externen Tailwind-/Font-Abhängigkeiten — „keine auszuliefernde Web-App" und keine Produktabhängigkeit.

## Design-Hilfe- und Nachweisbereich

`docs/help/design/` — strukturierte Feature-Doku nach Projektmuster: `index.md`, `beschreibung.md`, `ablauf-anwender.md`, `ablauf-technisch.md`, `api.md`, `business-rules.md`, `troubleshooting.md`, `README.md` (Schritt-9-Integration) und `verification/` (Screenshot-Nachweise, siehe [docs-structure.md](docs-structure.md)).
