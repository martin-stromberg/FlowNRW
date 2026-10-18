# Tasks: Überflüssige Überschriften

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | UI | `MapPage`: In-Page-Überschrift `TransitVisuals.Text(model.Title, 28, true)` entfernen | Erledigt | Kein direkter Test — View-Aufbau, `dotnet build` verifiziert Kompilierung |
| 2 | UI | `SearchPage`: In-Page-Überschrift `TransitVisuals.Text("Verbindungen", 32, true)` entfernen | Erledigt | Kein direkter Test — View-Aufbau, `dotnet build` verifiziert Kompilierung |
| 3 | UI | `StopSearchPage`: In-Page-Überschrift `TransitVisuals.Text("Haltestellen", 32, true)` entfernen | Erledigt | Kein direkter Test — View-Aufbau, `dotnet build` verifiziert Kompilierung |
| 4 | Verifikation | `dotnet test` für `FlowNRW.Tests` sowie Build-/Format-Prüfung ausführen | Erledigt | `dotnet test`: 307/307 bestanden; `dotnet format --verify-no-changes --severity error`: sauber |
