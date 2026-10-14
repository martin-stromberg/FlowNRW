# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

## Fehlgeschlagene Tests

Keine.

## Zusammenfassung

- **Gesamt:** 293
- **Bestanden:** 293
- **Fehlgeschlagen:** 0
- **Übersprungen:** 0
- **Gesamtzeit:** 0,78 s

Befehl: `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"` (xUnit, Projekt `FlowNRW.Tests`)

Hinweis: Die PowerShell-UI-Testskripte unter `tests/WindowsJourneyUiTests/` sind nicht Teil dieses Laufs; sie wurden separat gegen den `UiTest`-Build ausgeführt (Nachbearbeitung: `WindowsJourneyUiTests` inkl. `-Monitors`/`-Favorites`/`-Locations`/`-Maps`, `WindowsRefreshUiTests`, `WindowsLifecycleUiTests`, `WindowsDepartureCacheUiTests` sowie die `WindowsDesignUiTests.ps1`-Screenshot-Matrix — alle bestanden, Ablage `docs/help/design/verification/icon-matrix/`).

## Testabdeckung

**Abdeckung:** 88,92 % (Zeilen, Paket `FlowNRW.Core`; Branch-Abdeckung: 75,99 %)

| Datei | Abdeckung |
|-------|-----------|
| `FlowNRW.Core/Presentation/ConnectionFavoritesViewModel.cs` | 0,00 % |
| `FlowNRW.Core/Diagnostics/AppLog.cs` | 20,37 % |
| `FlowNRW.Core/Presentation/RelayCommand.cs` | 25,00 % |

(Generierte Dateien unter `obj/` — z. B. `System.Text.Json.SourceGeneration`-Dateien — wurden ignoriert. Alle übrigen Quelldateien liegen ≥ 80 %.)

## Fehlende Tests

Quelle: Coverage-Daten

- `FlowNRW.Core/Presentation/ConnectionFavoritesViewModel.cs` — 0 % Zeilenabdeckung; keine Tests decken diese ViewModel-Logik ab.
