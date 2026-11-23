# Test-Ergebnisse

## Ergebnis

**Status:** Unit-Tests fehlerfrei; interaktive UI-Läufe bis auf ein Umgebungslimit bestanden.

## Fehlgeschlagene Tests

- `WindowsLifecycleUiTests.ps1` (interaktiv): scheitert an der Fenster-Aktivierung dieser Remote-/VNC-Session — nach Minimieren/Wiederherstellen erhält die App kein `Activated`/`Resumed`, dadurch feuert kein Resume-Refresh (`RefreshForeground: False`, `fixture-0=1` statt 2). Mehrere Ansätze dokumentiert (Konsole-Vordergrund, `AttachThreadInput`, `SC_RESTORE`, `WM_ACTIVATE`); kein Feature-Bezug.

## Zusammenfassung

- **Gesamt:** 307
- **Bestanden:** 307
- **Fehlgeschlagen:** 0
- **Übersprungen:** 0

Befehl: `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj` (xUnit).

## Interaktive UI-Läufe (UiTest-Build, `schtasks /it`, Logs `artifacts/tests/continue-20261015-213718/`)

| Lauf | Ergebnis |
|---|---|
| `WindowsJourneyUiTests.ps1` (Default) | bestanden (48 s) |
| `WindowsJourneyUiTests.ps1 -Monitors` | bestanden (38 s), inkl. `MonitorNotice`/`MonitorMetadata` |
| `WindowsJourneyUiTests.ps1 -Favorites` | bestanden nach Skript-Fix (`HomeScenario` nach `RestartFavorites` erneut auf `success`; sonst vollständige Antwort ohne Notice), inkl. `FavoriteNotice`-Toggle + `ToggleFavorite`-Header |
| `WindowsJourneyUiTests.ps1 -Locations` | bestanden (99 s) |
| `WindowsJourneyUiTests.ps1 -Maps` | bestanden (63 s), inkl. `map-hang`-Watchdog |
| `WindowsDepartureCacheUiTests.ps1` | bestanden (19 s) |
| `WindowsRefreshUiTests.ps1` | bestanden (448 s) |
| `WindowsLifecycleUiTests.ps1` | fehlgeschlagen — Umgebungslimit (siehe oben); Skript um ikonisches-Vordergrund-Toleranz + Konsolen-Handoff robuster gemacht |
| `WindowsDesignUiTests.ps1` ×6 (light/dark × 430×900/1024×768 × 100 %/150 %) | bestanden (~90 s je Lauf), Matrix in `docs/help/design/verification/icon-matrix/` erneuert |

iOS-Gerätenachweis (Task 30) weiterhin offen — kein Gerät verfügbar.
