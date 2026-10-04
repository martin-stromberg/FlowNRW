# Testergebnisse – Navigation und Verbindungsfavoriten

**Stand:** 04.10.2026

## Automatisierte Kernprüfung

- `dotnet test .\FlowNRW.Tests\FlowNRW.Tests.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore`
  - erfolgreich: **278/278**
- `dotnet build .\FlowNRW\FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore`
  - iOS-Simulator und Windows: **0 Warnungen, 0 Fehler**

Die zusätzlichen Kernprüfungen decken den Roundtrip gerichteter Verbindungsfavoriten, die Auswahl und Vertauschung ohne Providerabruf sowie den Haltestellen-Sitzungscache mit erhaltener Trefferliste ab.

## Nativer Windows-Lauf

- `WindowsJourneyUiTests.ps1` wurde nach der Korrektur der zwei überholten Trefferlistenannahmen erneut mit der gebauten UiTest-EXE ausgeführt.
- Ergebnis: **`PASS all fixture native UI scenarios`**.

Der native Windows-E2E-Nachweis ist damit erbracht. Smartphone-/iOS-Prüfung bleibt separat.
