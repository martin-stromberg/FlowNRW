# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

## Fehlgeschlagene Tests

Keine.

## Zusammenfassung

- **Gesamt:** 294
- **Bestanden:** 294
- **Fehlgeschlagen:** 0
- **Übersprungen:** 0
- **Gesamtzeit:** 0,83 s

Befehl: `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"` (xUnit, Projekt `FlowNRW.Tests`)

Hinweis: Die PowerShell-UI-Testskripte unter `tests/WindowsJourneyUiTests/` sind nicht Teil dieses Laufs; sie wurden separat gegen den `UiTest`-Build ausgeführt (Nachbearbeitung: `WindowsJourneyUiTests` inkl. `-Monitors`/`-Favorites`/`-Locations`/`-Maps`, `WindowsRefreshUiTests`, `WindowsLifecycleUiTests`, `WindowsDepartureCacheUiTests` sowie die `WindowsDesignUiTests.ps1`-Screenshot-Matrix — alle bestanden, Ablage `docs/help/design/verification/icon-matrix/`).

Nachtrag 2026-10-15 (Fortsetzungslauf): `dotnet build FlowNRW.sln` (0 Warnungen, 0 Fehler) und `dotnet test FlowNRW.Tests` erneut ausgeführt — **294/294 bestanden** (350 ms). Zusätzlich liefen die `WindowsDesignUiTests.ps1`-Dark-Kombinationen (430×900×100, 430×900×150, 1024×768×100) und das `refresh-store-error`-Szenario (hell/dunkel, 150 %) vollständig grün inkl. aller `AssertTouchTarget`- und Fokus-Assertionen.

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
