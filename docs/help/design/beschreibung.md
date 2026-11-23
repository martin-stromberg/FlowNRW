← [Zurück zur Übersicht](index.md)

# Design — Beschreibung

## Zweck

Der Design-Refresh hebt die zuvor prozedural wirkende Prototyp-Oberfläche auf ein modernes, minimalistisches Erscheinungsbild mit professioneller visueller Qualität. Er ist bewusst eine **Nachschärfung** des bereits abgenommenen Token-Systems aus [`docs/design/acceptance.md`](../../design/acceptance.md) und [`docs/design/reference/trans_nrw_mobil_system/`](../../design/reference/trans_nrw_mobil_system/), kein Ersatz: bestehende Tokens, Abstände, Zustände und die Schritt-9-Abnahme bleiben verbindlich; geschlossen werden die identifizierten Lücken (echte Icons, explizite lizenzfreie Schrift, fehlende Semantik-Tokens).

## Funktionsweise

### Echte Iconografie

Alle Unicode-Textglyphen (`＋ ⌖ ↻ ⚙ ⌃ ⌄ ▣ ☆ ★ ⌕ ✓ ⇄ ⓘ`) in Aktionsbuttons, Statuszeilen und der Tab-Leiste wurden durch Glyphen der Icon-Font **Material Symbols Rounded** (Apache License 2.0, optische Größe opsz 24) ersetzt. Zwei TTF-Instanzen liegen in `FlowNRW/Resources/Fonts/`: `MaterialSymbolsRounded.ttf` (FILL0, Umrisse) und `MaterialSymbolsRoundedFill.ttf` (FILL1, gefüllte Form — z. B. der gespeicherte Favoritenstern). Alle Codepoints stehen als Konstanten in `FlowNRW/TransitIcons.cs` und wurden gegen das Codepoint-Manifest der eingebetteten Datei verifiziert (`docs/design/licenses/MaterialSymbolsRounded.codepoints`).

Icon-Buttons bleiben `Button` mit Glyph als `Text` und `FontFamily="MaterialSymbols"` — dadurch bleiben ControlType.Button, InvokePattern und alle `AutomationId`s unverändert, und die Iconfarbe folgt über `TextColor` automatisch dem hellen/dunklen Thema. Buttons mit Icon **und** Text (z. B. der Favoriten-Button im Abfahrtsmonitor) erhalten das Symbol als `FontImageSource` in `Button.ImageSource`. Die drei Shell-Tabs tragen `FontImageSource`-Symbole aus `TransitIcons.TabIcon` bei unveränderten Titeln und Routen.

### Lizenzfreie Typografie

**Inter** (SIL Open Font License 1.1) ist die UI-Schrift: `Inter-Regular.ttf`, `Inter-SemiBold.ttf` und `Inter-Bold.ttf` werden als Aliase `Inter`, `InterSemiBold`, `InterBold` registriert und von allen impliziten Styles in `Styles.xaml` sowie den `TransitVisuals`-Fabriken verwendet. `TransitVisuals.Text(..., bold: true)` setzt den dedizierten Schnitt `InterBold` statt `FontAttributes.Bold`, damit kein synthetisches Fett entsteht. **JetBrains Mono** (SIL OFL, Schnitt Bold) dient den prominenten Hauptzeiten der Abfahrtskarten und ersetzt `Consolas`/`Menlo`. OpenSans wurde entfernt. Die Schrift-Aliase stehen zentral in `FlowNRW/Fonts.cs`; die Lizenztexte liegen unter `docs/design/licenses/`.

### Status-Chips und Listeneinträge

Abfahrten und Verbindungen zeigen ihren Status als Chip aus Symbol **plus** Text — Farbe ist nie der alleinige Informationsträger:

| Zustand | Darstellung | Semantikfarbe |
|---|---|---|
| Ausfall gemeldet | `TransitIcons.Cancel`/`Error` + „Fahrt fällt aus" (Karte, Timeline) bzw. „Ausfall gemeldet" (Verbindungsliste) | `Error`/`ErrorDark` |
| Verspätung | `TransitIcons.Schedule` + „+n Min.", Sollzeit durchgestrichen | `Warning`/`WarningDark` |
| Keine Echtzeit | dezenter Hinweis „Keine Echtzeitdaten" | `Muted`/`MutedDark` |

Die Statusentscheidung trifft `DeparturePresentation.Status` in FlowNRW.Core ausschließlich aus gelieferten Echtzeitfeldern (siehe [Fachliche Regeln](business-rules.md)). Die Karten (`DepartureCardView`, `JourneyCardView`, `JourneyTimelineView`, `TransitVisuals.Candidate`, Favoritenkarten in `HomePage.BuildFavoriteCard`) verwenden einheitliches Spacing (Kartenabstand 12, Innenabstand 16) und die Referenz-Typoskala (Titel 22–28, Text 17, Nebeninfo 15, Meta 13).

### Seitentitel ohne Dublette

Seiten wiederholen ihren `Title` — den Eintrag in der Navigationsleiste — nicht als große In-Page-Überschrift. Die ehemaligen Seitentitel „Verbindungen" (`SearchPage`), „Haltestellen" (`StopSearchPage`) und der Kartenname (`MapPage`: „Favoritenkarte"/„Haltestellenkarte"/„Verbindungsverlauf") sind entfernt; der Fenstertitel benennt die Ansicht allein. Unverändert bleiben Abschnittsüberschriften innerhalb von Karten und Bereichen („Haltestelle finden", „Wohin möchtest du fahren?", „Nächste Haltestellen", „Diagnose") sowie dynamische Inhaltstitel wie der Stationsname im Abfahrtsmonitor (`MonitorStop`), der die angezeigten Daten identifiziert.

### Token-Nachschärfung

`Colors.xaml` wurde um `GroupStrong` (`#EEEDF3` hell / `#2C2C2E` dunkel) sowie die Semantikfarben `Success`/`Warning`/`Error` (`#006E28`/`#894D00`/`#BA1A1A`) samt kontrastgeprüften Dunkelvarianten (`#3ECF6B`/`#F0A840`/`#FF897D`, jeweils ≥4,5:1 auf den dunklen Karten-/Gruppenflächen) erweitert. Die C#-Fabriken in `TransitVisuals` lesen dieselben Werte über zentrale Konstanten. `Styles.xaml` setzt `FontFamily="Inter"` in allen impliziten Text- und Eingabe-Styles, stuft `Headline` auf 28 und `SubHeadline` auf 22 mit den Ink-Tokens um und gibt `ImageButton` einen 12er-Eckenradius auf `GroupStrong`-Fläche.

## Beispiele

- **Abfahrtskarte mit Verspätung:** Linienbadge und Ziel links, Hauptzeit in JetBrains Mono rechts; weicht die Istzeit ab, steht die Sollzeit durchgestrichen darunter, gefolgt vom `schedule`-Chip „+3 Min." in Warning-Farbe.
- **Favoritenstern im Abfahrtsmonitor:** `DeparturePage.RefreshFavorite` zeigt den Umriss-Stern + „Speichern" bzw. den gefüllten Stern (FILL1-Instanz) + „Entfernen".
- **Auf-/Einklappen auf der Startseite:** `HomePage.BuildCardHeader` wechselt das Symbol des Schalters rechts oben im Kartenkopf zwischen `expand_more` und `expand_less`, jeweils mit passender Sprachbeschreibung.
- **Datenhinweis:** Trägt ein angezeigtes Ergebnis Warnungen, eine Ersatzquelle oder veraltete Daten, zeigt `TransitVisuals.NoticeToggle` ein `warning`-Symbol (`TransitIcons.Warning`, Semantikfarbe `Warning`/`WarningDark`) statt einer dauerhaften Textzeile; erst ein Tipp blendet die Provenienz ein.
- **Tab-Leiste:** „Abfahrten" (`departure_board`), „Verbindungen" (`route`), „Haltestellen" (`location_on`) als Symbole mit Text.

## Einschränkungen

- **Linienbadges bleiben Textbadges** (VDV-Konvention, keine Symbolfunktion); Farbe nur bei gelieferter Kategorie über `DeparturePresentation.BadgeColor`.
- **Kartenmarker bleiben Leaflet** (`HybridWebView`) und sind vom Icon-Umbau unberührt; `ToolbarItem` „Haltestellen" bleibt Text (Plattformkonvention).
- Es gibt **keine erfundenen Echtzeitwerte**, Geometrien oder Zusatzdaten; nicht gelieferte Werte bleiben ausdrücklich unbekannt.
- Es entstehen **keine neuen Seiten**, kein XAML-Umbau der prozeduralen Views, keine Provider-/Datenänderungen; MVVM und DI bleiben unverändert.
- Die **subjektive AAA-Bewertung** ist nicht automatisierbar und bleibt ein separater manueller Abnahmeschritt gegen `acceptance.md` und die Referenz-HTMLs.
- Plattformziel des Nachweises ist Windows; iOS bleibt über die bestehende Gerätecheckliste abgedeckt.

## Nachweise

- Icon-/Font-Screenshot-Matrix (222 Bilder, hell/dunkel, 430×900 und 1024×768, 100 % und 150 % Textskalierung): [`verification/icon-matrix-run.md`](verification/icon-matrix-run.md) und [`verification/icon-matrix/`](verification/icon-matrix/)
- Font-Lizenzen und Codepoint-Manifest: [`docs/design/licenses/`](../../design/licenses/)
- 293 Core-Tests bestehen, darunter die neuen `FlowNRW.Tests/DepartureStatusTests.cs`; native Windows-UI-Läufe sind in der Matrix-Dokumentation verlinkt.
