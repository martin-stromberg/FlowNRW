# Testergebnisse – Schritt 2: Einheitlicher Restore-Einstieg

Datum: 2026-11-22
Ausgeführt durch: Orchestrierung (direkte Läufe)

## Ergebnis

**Status:** Keine Fehler

## Ausgeführte Checks

| Check | Befehl | Ergebnis |
|---|---|---|
| Unit-/Integrationstests | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release` | 312/312 bestanden, 0 Fehler |
| Build mit Warnungsfehlern | `dotnet build FlowNRW.Core -c Release -p:TreatWarningsAsErrors=true` | 0 Warnungen, 0 Fehler |
| Build Tests | `dotnet build FlowNRW.Tests -c Release -p:TreatWarningsAsErrors=true` | 0 Warnungen, 0 Fehler |
| Build UiTest-Fixtures | `dotnet build FlowNRW/FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true` | 0 Warnungen, 0 Fehler (nach Enabling-Fix `CsWinRT1032` in `UiTestFixtureServices.cs` — explizite Array-Casts, vorbestehende Toolchain-Bruchstelle) |
| Format | `dotnet format FlowNRW.sln --verify-no-changes --severity error` | clean |

## Reale End-to-End-Abnahme

**Bestanden** — realer Lauf auf Windows-Desktop (interaktive Konsole) gegen den
UiTest-Fixture-Build, UI-Automation:

`powershell.exe -File tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1 -Exe FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe -MonitorStale`

Ergebnis (alle PASS):

- `Stale persisted board is displayed on monitor open` — veraltetes persistiertes
  Board wird beim Öffnen über die Stopsuche angezeigt und `MonitorNotice` markiert
  (statt verworfen) — die neue einheitliche Semantik auf dem realen UI-Pfad
  StopSearchPage → `OpenAsync` → `OpenStopAsync` → `DepartureBoardCache` →
  DeparturePage.
- `Failed monitor refresh retains the displayed stale board and its notice` —
  Providerfehler erhält das markierte Board.
- `Cached board is shown before the delayed live refresh` — gecachtes Board zuerst,
  danach `Live Stand`-Ersatz.
- `native monitor stale restore regression` — Gesamtnachweis.
- `Non-favorite does not display the matching persisted board` — bei geleertem
  Favoriten-Bestand und vorhandenem Cache-Eintrag zeigt der Monitor kein
  persistiertes Board (Membership-Prüfung der Fassade, empfohlenes E2E-Szenario).
- `native monitor membership regression` — danach reguläre Live-Anzeige (`RE 1`).

Vorangegangene Seed-/Setup-Asserts (Favorit anlegen, isolierter Cache entsteht)
ebenfalls PASS.

## Sichtprüfung der Abnahme-Screenshots

Keine Screenshots erzeugt — der `-MonitorStale`-Lauf produziert keine Bilder
(`-ScreenshotDirectory` nicht übergeben; das Szenario ist Assertions-basiert). Es
gibt keine geänderte Oberfläche; das Stale-Verhalten des Monitors gleicht sich dem
bestehenden Home-Verhalten an. Damit entfällt die Sichtprüfung (kein Material).

## Fehlgeschlagene Tests

Keine.
