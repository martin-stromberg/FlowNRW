# Übersetzte Anforderung: Überflüssige Überschriften

Quelle: GitHub-Issue `martin-stromberg/FlowNRW#12` (Plattform: iOS)

## Originalanforderung

> Die verschiedenen Ansichten der App haben Überschriften, die unnötig sind, weil schon der
> Fenstertitel identisch oder ähnlich ist.
>
> Beispiel: Wird die Favoritenkarte aufgerufen, so steht der Text „Favoritenkarte" im
> Fensterkopf mittig neben dem „Back"-Button und auch direkt darunter als Überschrift der
> Ansicht. Diese Redundanz soll aufgelöst werden.
>
> Gleiches gilt für andere Ansichten der App.

## Fachliche Zusammenfassung

Auf iOS zeigt die Shell-Navigationsleiste den `Title` jeder `ContentPage` zentriert neben
dem Zurück-Button an. Mehrere Seiten rendern denselben oder einen nahezu identischen Text
zusätzlich als große In-Page-Überschrift (28–32 pt, fett). Diese redundanten Seitentitel
sollen entfernt werden; inhaltliche Abschnittsüberschriften und dynamische Inhalte bleiben
erhalten.

## Betroffene Klassen und Komponenten

- `FlowNRW/MapPage.cs` — In-Page-Überschrift `TransitVisuals.Text(model.Title, 28, true)`
  dupliziert `Title = model.Title` (Werte: „Favoritenkarte", „Haltestellenkarte",
  „Verbindungsverlauf").
- `FlowNRW/SearchPage.cs` — In-Page-Überschrift `TransitVisuals.Text("Verbindungen", 32, true)`
  ähnelt `Title = "Verbindung suchen"`.
- `FlowNRW/StopSearchPage.cs` — In-Page-Überschrift `TransitVisuals.Text("Haltestellen", 32, true)`
  ähnelt `Title = "Haltestelle suchen"`.
- Zu prüfen, nicht zwingend betroffen: `DeparturePage` (dynamischer Stationsname),
  `HomePage`, `ResultsPage`, `JourneyDetailPage`, `RefreshSettingsPage`.
- UI-Komponenten: keine neuen; Entfernen von `Label`-Elementen in `VerticalStackLayout`s.
- Tests: `FlowNRW.Tests` (Unit-Tests unberührt, kein UI-Projekt betroffen);
  PowerShell-UI-Tests unter `tests/WindowsJourneyUiTests` referenzieren die entfernten
  Elemente nicht über `AutomationId` (Zu prüfen: `MonitorStop`).

## Implementierungsansatz

- Entfernen der statischen Seitentitel-`Label`s aus den Layouts der betroffenen Seiten;
  der Navigationsleistentitel (`Title`) bleibt unverändert die alleinige Ortsangabe.
- Keine neuen Klassen, Interfaces oder Enums erforderlich; reine View-Anpassung.
- Abschnittsüberschriften innerhalb von Karten/Sektionen („Haltestellenliste",
  „Wohin möchtest du fahren?", „Diagnose", „Nächste Haltestellen") werden geprüft und
  nur entfernt, wenn sie den Fenstertitel wörtlich oder sinngemäß wiederholen.

## Konfiguration

Keine.

## Offene Fragen

- `DeparturePage` zeigt den Stationsnamen (`model.Title`, AutomationId `MonitorStop`,
  28 pt fett) identisch zum gebundenen `Title`. Die Design-Abnahme
  (`docs/design/acceptance.md`) fordert für den Einzelmonitor explizit eine „klare große
  Überschrift"; zudem nutzt die manuelle Windows-UI-Testsuite `MonitorStop` als
  Seitenerkennungs- und Inhaltsmarker. Annahme: Der Stationsname ist Inhalt (identifiziert
  die angezeigten Daten), kein redundanter Ansichtstitel, und bleibt erhalten.
