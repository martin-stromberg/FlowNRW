# Bestandsaufnahme Detail: Tests

## Testklassen (`FlowNRW.Tests/`)

Das Unit-Testprojekt deckt `FlowNRW.Core` ab (ViewModels, Presentation, Refresh,
Transit). Die View-Klassen in `FlowNRW/` werden dort nicht instanziiert — keine
bestehende Testmethode prüft Layout- oder Überschriftselemente der Seiten.

Relevant, weil `MapViewModel.Title` betroffen sein könnte:

- `FlowNRW.Tests/MapTests.cs` — Tests der Titel-Strings „Haltestellenkarte",
  „Favoritenkarte", „Verbindungsverlauf" bleiben unverändert (die Eigenschaft wird
  nicht geändert).

## Hilfsmethoden

Keine betroffenen.

## Windows-UI-Tests (`tests/WindowsJourneyUiTests/`)

PowerShell-Skripte mit UI-Automation (`AutomationId`-basiertes `Find`/`Wait`/`Name`).
Laufen manuell, nicht in CI (kein Verweis in `.github/workflows/`).

- `MonitorStop` wird in `WindowsJourneyUiTests.ps1`, `WindowsRefreshUiTests.ps1` und
  `WindowsLifecycleUiTests.ps1` über 20-mal als Seitenerkennungs- und Inhaltsmarker
  verwendet → `MonitorStop` darf nicht entfernt werden.
- Die zu entfernenden Überschriften (`MapPage` Z. 40, `SearchPage` Z. 29,
  `StopSearchPage` Z. 35) besitzen keinen `AutomationId`-Wert und werden von keinem
  Skript referenziert.
- Verifikationsprotokolle unter `docs/help/*/verification/*.txt` enthalten ebenfalls
  `MonitorStop`-PASS-Zeilen, aber keine Einträge zu den entfernten Überschriften.
