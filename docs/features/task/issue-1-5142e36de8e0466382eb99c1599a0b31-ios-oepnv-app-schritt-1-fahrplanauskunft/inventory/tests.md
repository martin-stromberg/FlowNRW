# Tests und Test-Ausgangszustand

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt der Bestandsaufnahme: 2026-09-08, Europe/Berlin. Der nachfolgend verwendete Baseline-Testlauf ist im bestehenden Projektinventar vom 2026-09-07 dokumentiert; die Rohkonsole wurde in diesem Feature-Arbeitsverzeichnis nicht erneut erzeugt.
- Branch und Commit-ID: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-1-fahrplanauskunft`, `0d5ef943e423a98c425a91f14e8c3fc4151df938`.
- Uncommittete Änderungen im geprüften Stand: `.gitignore` geändert, `docs/projects/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app/steps.md` geändert, `design-draft.zip` unversioniert sowie Feature-Dokumentation unversioniert. Diese Änderungen wurden nicht verändert.
- Testumgebung: Windows/PowerShell, .NET SDK `10.0.400`; Core und Tests zielen auf `net10.0`.
- Testsuite und Quelle des Befehls: `FlowNRW.Tests/FlowNRW.Tests.csproj`; der Befehl ist im bestehenden Projektinventar `docs/projects/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app/inventory.md` als tatsächlich ausgeführter Baseline-Lauf dokumentiert.

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Baseline vor Umsetzung | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --nologo` | Repositorywurzel | 0 | 5 | 0 | 0 | [Projektinventur und Baseline](../../../../projects/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app/inventory.md) |

Der Baseline-Nachweis wurde gemäß Arbeitsauftrag aus der vorhandenen Projektinventur übernommen; für diese reine Dokumentationsphase wurde kein weiterer Testlauf ausgeführt.

### Nachgewiesene bestehende Testfehler

Keine. Der dokumentierte Baseline-Lauf weist keine fehlgeschlagenen Tests aus.

### Testlücken und Ausführungsprobleme

- Es existieren keine Tests für ÖPNV-Modelle, Providerantworten, Routing, regionale Priorität, Fallback, Echtzeitkonsolidierung, Cache oder HTTP-Fehler.
- Es existieren keine UI- oder Integrationstests für die Fahrplanauskunft.
- Ein vollständiger Windows-App-Build und eine iOS-Kompilierung bzw. native UI-Ausführung sind im Baseline-Nachweis nicht enthalten. Dies ist eine Ausführungslücke, kein Testfehler.
- Eine Apple-Werkzeugkette und ein verbundener Mac sind in der Projektinventur nicht nachgewiesen; daraus wird kein zusätzlicher Fehlerstatus abgeleitet.

## Testklassen

### `ClickCounterTests`

Datei: `FlowNRW.Tests/ClickCounterTests.cs`

- `Click_IncrementsCountAndReturnsCaption` — prüft zwei Klicks, den Zählerstand sowie Singular-/Plural-Beschriftung.
- `FormatCaption_UsesSingularOnlyForOne` — parametrisiert mit `0`, `1` und `5`; prüft die Beschriftungsform.
- `FormatCaption_RejectsNegativeCount` — prüft die Zurückweisung eines negativen Zählerstands.

Die Theorie enthält drei Testfälle. Zusammen mit den beiden `Fact`-Methoden ergeben sich die fünf im Baseline-Lauf ausgewiesenen erfolgreichen Tests.

## Hilfsmethoden

Keine anforderungsbezogenen Test-Hilfsklassen oder Hilfsmethoden vorhanden.
