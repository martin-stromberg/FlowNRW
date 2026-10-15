# Offene Aufgaben

Erstellt am: 2026-10-14
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

- [x] Task 25 — Separate visuelle Abnahme (AAA-Nachweis): erledigt 2026-10-15. Matrix vervollständigt (Dark-Kombinationen neu aufgenommen, `refresh-store-error` ergänzt → 226 PNGs + `matrix.jsonl`), Stichprobe von 24 Bildern + Pixelchecks gegen `docs/design/acceptance.md` bewertet. Ergebnis: **Windows-Abnahme bestanden**, dokumentiert in `docs/help/design/verification/visual-acceptance.md` (drei Anmerkungen ohne Blockercharakter). Hinweis: ohne verschachtelte Subagenten wurde die Abnahme vom ausführenden Agenten dokumentiert selbst durchgeführt.
- [ ] Task 26 — iOS-Checkliste: Abhaken der `acceptance.md`-Checkliste erfordert iOS-Gerät/Mac; auf Windows nicht ausführbar. Bleibt offen — kein iOS-Gerät in dieser Umgebung verfügbar; Gerätebilder nicht zu behaupten.

## Code-Review-Befunde

- [x] `MauiProgram.cs` (`CreateMauiApp`) — Kopplung: `"MaterialSymbols"`/`"MaterialSymbolsFilled"` als Literale registriert statt `TransitIcons.FontFamily`/`FilledFontFamily` (Z. 34–35) — inkonsistent zu den `Fonts.*`-Registrierungen. Empfehlung: `TransitIcons.FontFamily`/`FilledFontFamily` verwenden.
- [x] `JourneyCardView.cs` / `JourneyTimelineView.cs` / `JourneyPresentation.cs` — Doppelter Code: Fallback-Kette `leg.Line?.Name ?? leg.Departure.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt"` an 3 Stellen; Mode-Auflösung `(leg.Line ?? leg.Departure.Line)?.Mode` an 2 Stellen. Empfehlung: In `JourneyPresentation` zentralisieren. *(Behoben: `JourneyPresentation.LegLineName`/`LegLineMode`; Test `ResolvesLegLineThroughSharedFallback` in `FlowNRW.Tests/PresentationTests.cs`.)*
- [x] `Colors.xaml` — Toter Code: `Tertiary`, `GroupLight`, `GroupDark` unreferenziert. Empfehlung: Entfernen.
- [x] `TransitIcons.cs` — Doppelter Code: `TabIcon` repliziert `IconImage` bis auf `Size=24`. Empfehlung: `TabIcon` auf `IconImage(glyph, size: 24)` delegieren.
- [x] `HomePage.cs` (`BuildCardActions`) — Doppelter Code: identischer `SetIcon`-Aufruf für Expand-Button in Z. 289 und im Command-Lambda Z. 290. Empfehlung: lokale `SetExpandIcon()`-Funktion.

## Fehlgeschlagene Tests

Keine.
