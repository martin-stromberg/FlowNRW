# Korrekturprüfung – Vereinigung partieller Fahrplanantworten

Stand: 2026-09-15, unveröffentlichter Korrekturstand auf dem Schritt-1-Branch, vor Abschlusscommit. Fachliche unabhängige Projekt-Abnahmerunde 2: Anforderung vollständig erfüllt (Bericht im Projekttracking).

## Ausführung

- `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true --collect:"XPlat Code Coverage" --results-directory D:/temp/flownrw-union-final`: Exit 0, **106 erfolgreich, 0 fehlgeschlagen, 0 übersprungen**, 298 ms Testdauer. SDK 10.0.401.
- Gesamte Core-Assembly: **570/578 Zeilen, 98,61 %**, keine Ausschlussfilter. [Rohreport](coverage-union.cobertura.xml).
- Windows: `dotnet build FlowNRW.sln -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true`: Exit 0, **0 Warnungen, 0 Fehler**, 6,87 s. SDK-Zugriff erforderte Sandbox-Eskalation; unveränderte Windows-Konfiguration.
- `dotnet format` Core und Tests erfolgreich, nur Formatbereinigung nach Testlauf.
- Unveränderter XML-Hook: `python .githooks/csproj-xmldoc-check.py --all`: Exit 0, **77 Dateien**.
- Vorhandene 26 bestandene Release-Skripttests bleiben gültig: Release-Skripte und GitHub-Workflows unverändert. Keine erneuten Liveabfragen, vorhandene Serviceproben weiter gültig.

## Regressionen und Befundkorrektur

Die beiden ursprünglichen Tests für regionale A plus nationale A/B schlugen vor der Union fehl; [roter Nachweis](union-correction-red.txt), [erste grüne Probe](union-correction-green.txt). Weitere Tests prüfen mehrdeutige/fremde Identitäten, mehrere Fahrtabschnitte einschließlich Fußweg, Sortierung und finales Limit. Fünf alte Erwartungen auf genau einen Treffer wurden an die jetzt ausdrücklich verlangte Ergebnisergänzung angepasst; regionale Priorität wird weiterhin geprüft.

Bei Übernahme des unterbrochenen Standes fand der Hauptagent zusätzlich verlorene Echtzeitanreicherung in MergeJourneys. Ein neuer Test MergeJourneys_UniqueMatch_EnrichesMissingFieldsWithoutReplacingRegionalRealtime schlug mit erwartetem Cancelled=true, tatsächlich null fehl (Exit 1). Nach beidseitig eindeutiger vollständiger Fahrtzuordnung werden fehlende Felder ergänzt; vorhandene regionale Werte, Geometrie und Umstiege bleiben erhalten. Gesamtlauf danach 106/106 grün.

## Workflow bei Unterbrechung

Die Implementierungsagenten waren wegen Nutzungslimit ausgefallen. Der Hauptagent führte die verbleibende Umsetzung, Tests und Dokumentation deshalb gemäß Skill-Fallback lokal in getrennten Phasen aus. Der anschließend wieder verfügbare separate Prüfer bestätigte die fachliche Korrektur unabhängig; keine unabhängige Prüfung wird durch reine Selbstauskunft ersetzt.
