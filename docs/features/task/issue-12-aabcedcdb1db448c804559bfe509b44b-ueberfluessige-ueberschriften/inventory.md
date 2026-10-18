# Bestandsaufnahme: Überflüssige Überschriften

Analysiert wurden alle `ContentPage`-Views von `FlowNRW` sowie die gemeinsamen
Layout-Hilfen in `TransitVisuals`, bezogen auf die Anforderung, In-Page-Überschriften zu
entfernen, die den Fenstertitel (Shell-Navigationsleiste, `Title`) identisch oder ähnlich
wiederholen.

## Zusammenfassung

- Drei Seiten rendern einen statischen Seitentitel als große Überschrift, der den
  `Title` wörtlich oder nahezu wiederholt: `MapPage` (identisch), `SearchPage` und
  `StopSearchPage` (ähnlich).
- `DeparturePage` zeigt den Stationsnamen groß (28 pt, `MonitorStop`); der Text ist
  dynamischer Dateninhalt, wird von `docs/design/acceptance.md` als „klare große
  Überschrift" des Einzelmonitors gefordert und von der Windows-UI-Testsuite als
  Marker verwendet.
- `HomePage`, `ResultsPage`, `JourneyDetailPage` und `RefreshSettingsPage` besitzen
  keine Überschrift, die den Fenstertitel wiederholt; vorhandene große Texte sind
  Abschnittsüberschriften oder dynamische Inhalte.
- Alle redundanten Überschriften werden über `TransitVisuals.Text(text, size, bold)`
  erzeugt und tragen keine `AutomationId` — die UI-Testsuite greift nicht darauf zu.

## Details

- [Views und Überschriften](inventory/pages.md)
- [Tests](inventory/tests.md)
