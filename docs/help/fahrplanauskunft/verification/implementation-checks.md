# Implementierungsnachweis – Fahrplanauskunft

Stand: Implementierung vom 8. September 2026, Nachweise am 9. September gelesen und gesichert. Kein neuer Testlauf am 9. September; Quellcode seit den folgenden finalen Prüfungen unverändert.

## Ausgeführte Qualitätsprüfungen

Arbeitsverzeichnis: Repositorywurzel. .NET SDK 10.0.400, Windows. Befehle wurden mit der vorhandenen Installation `C:/Program Files/dotnet/dotnet.exe` ausgeführt.

| Prüfung | Befehl | Exitcode / Ergebnis |
|---|---|---|
| Finale Core-Tests einschließlich Coverage | `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true --collect:"XPlat Code Coverage" --results-directory <TEMP>/flownrw-step1-final-tests` | 0; 89 erfolgreich, 0 fehlgeschlagen, 0 übersprungen |
| Core-Zeilenabdeckung | Coverlet-Cobertura aus vorigem Lauf | 497/528 Zeilen = 94,12%; einziges Paket FlowNRW.Core; keine Include-/Exclude-Filter |
| Windows-Solution | `dotnet build FlowNRW.sln -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true` | 0; 0 Warnungen, 0 Fehler; Core, Tests und Windows-MAUI gebaut |
| Core-Format | `dotnet format FlowNRW.Core/FlowNRW.Core.csproj --verify-no-changes --no-restore` | 0 |
| Test-Format | `dotnet format FlowNRW.Tests/FlowNRW.Tests.csproj --verify-no-changes --no-restore` | 0 |
| Verpflichtender XML-Hook | `python .githooks/csproj-xmldoc-check.py --all` | 0; 73 C#-/Projektdateien geprüft; Hook unverändert |

Rohreport: [coverage.cobertura.xml](coverage.cobertura.xml). Ursprünglicher Collectorpfad: `<TEMP>/flownrw-step1-final-tests/1519ec9c-26ef-4c41-afbf-796d591c4f2d/coverage.cobertura.xml`.

Ein zunächst roter Regressionstest wies nach, dass EFA auch vor dem angefragten Abfahrtszeitpunkt liegende Verbindungen liefert (88 erfolgreich, 1 fehlgeschlagen). Der Routingservice filtert jetzt mit Echtzeit, soweit vorhanden, sonst Sollzeit gegen den Anfragezeitpunkt. Anschließend bestanden alle 89 Tests. Bestehende fünf ClickCounter-Testfälle blieben unverändert. Vor dem finalen Format-/XML-Schritt ergab der erste vollständige Coverage-Lauf 494/525 Zeilen; maßgeblich ist der obige finale Rohreport.

Der Windows-Build erfolgte vor der anschließenden ausschließlich syntaktischen XML-/Formatanpassung (Propertydokumentation, Collectionausdrücke, Blockkörper der Mapper); der finale Core-/Testlauf prüfte deren Kompilierung. Die MAUI-Komposition wurde nach erfolgreichem Windows-Build nicht fachlich verändert.

## Tatsächlich ausgeführte Serviceproben

[Normalisierte Serviceausgaben](../service-live-2026-09-08.jsonl) entstanden am 8. September 2026 zwischen 18:10:22 und 18:11:47 UTC durch die implementierten Fachservices, ProviderOrchestrator, HTTP-Gateway und Adapter. [Ausführbarer Konsolenquelltext](../provider-probe.cs.txt) dokumentiert die Komposition. Für Wiederholung als Program.cs in einem temporären net10.0-Konsolenprojekt mit ProjectReference auf FlowNRW.Core verwenden und `dotnet run --project <TEMP>/flownrw-service-probe/Probe.csproj -c Release` ausführen. Der Probezeitpunkt ist jeweils DateTimeOffset.UtcNow.

| Serviceprobe | Normalisiertes Ergebnis |
|---|---|
| Ortssuche Berlin Hbf | 5 echte Treffer über EFA-Fallback, eindeutige Stations-DHID in Treffern; Suchmehrdeutigkeit bleibt als Warnung sichtbar |
| Nähe Gelsenkirchen | 5 Haltestellen, erster Treffer Wiehagen in 478 Metern |
| NRW-Abfahrten Gelsenkirchen Hbf | 5 Ereignisse; erstes Ereignis Linie107 mit Soll17:33UTC, Ist18:52:18UTC, gemeldeter Verspätung01:19:18, Plattform3 und Sollplattform U-Bahn Gleis1 |
| Bundesweite Verbindung Berlin–Hamburg | 2 kommende Verbindungen; erste RJ172 ab18:34UTC/an20:24UTC,1594Geometriepunkte, anschließender Fußweg mit16Geometriepunkten |

Die zuvor separat abgerufenen Rohantworten und Providerdokumentationsquellen sind im [Providerprobebericht](../provider-probes.md) und dessen Fixtures erhalten. Fixtures sind deterministische Testeingaben und keine neuen Liveabrufe. db-rest antwortete bei der früheren Rohprobe503 und lief in den späteren Serviceproben in das konfigurierte Timeout. EFA ist ein Entwicklungszugang; zeitweise unplausible Werte (z.B. die oben ausdrücklich unverändert dokumentierte große Verspätung) und Anbieterwarnungen werden nicht versteckt. Einzelne erfolgreiche Fahrten belegen keine flächendeckende Verfügbarkeit oder Produktivfreigabe.

## Übergabegrenzen

Diese Datei ist ein Implementierungsnachweis, keine unabhängige fachliche Abnahme. Separate Lifecycle-Reviews, unabhängige Tests und dauerhafte Betriebs-/Erweiterungsdokumentation folgen im übergeordneten Workflow. Keine Commits, Branchwechsel oder Merges durch den Implementierungsagenten. Die vorhandenen Windows-CI-Dateien sind unverändert; keine iOS-CI oder Deployment-Automation hinzugefügt.
