# Korrekturrunde 2 – drei unabhängige Reviewbefunde

Korrekturen nach review.md und review-code.md des Lifecycle, abgeschlossen am 9. September 2026. Keine weiteren funktionalen Änderungen, keine neuen Liveproben.

## Rot → Grün

Alle Testbefehle wurden in der Repositorywurzel mit `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-restore --nologo --filter FullyQualifiedName~<Testklasse>` ausgeführt.

| Befund / Testklasse | Vor Fix | Korrektur | Nach Fix |
|---|---|---|---|
| Body-Timeout und IOException nach HTTP 200 / TransitHttpGatewayTests_BodyFailures | Exit 1, zwei Fehler: http-200 statt timeout/transport | Transport-Catch setzt den zuvor empfangenen HTTP-Status zurück; Retryentscheidung und Endfehler verwenden den Transportfehler | Exit 0, zwei Tests grün; je zwei Versuche bei MaxRetries=1 |
| Ungültige Folgeeingaben / TransitServiceTests_InvalidReplacement | Exit 1, drei Fehler: alte Search-/Route-/Departure-Anfrage liefert weiterhin erfolgreich | Auch Validierungsfehler werden durch Latest verarbeitet und lösen die ältere Anfrage ab | Exit 0, drei Tests grün |
| NRW-Namen-/ID-Auflösung vor Auswahl / ProviderOrchestratorTests_RegionResolution | Exit 1, vier Fehler: national statt regional bzw. fehlende Unknown-Warnung | Namen bzw. ausgewählte Stopidentitäten eindeutig auflösen, dann Polygonklassifikation und Providerwahl; unaufgelöste Region trägt region-unknown. Ausgewählte Stopidentitäten müssen per Providernamespace/ID oder DHID übereinstimmen | Exit 0, vier Tests grün |

## Gesamtnachweis nach allen drei Korrekturen

- `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true --collect:"XPlat Code Coverage" --results-directory <TEMP>/flownrw-step1-iteration2-tests`: Exit 0, **98 erfolgreich, 0 fehlgeschlagen, 0 übersprungen**.
- Core-Coverage: **541/552 Zeilen, 98,00 %**, keine Include-/Exclude-Filter. [Aktueller Rohreport](coverage-iteration2.cobertura.xml), Collector-Unterverzeichnis e615d0a1-8f39-4481-a5f9-3ecdbd958344.
- `dotnet build FlowNRW.sln -c Release --no-restore --nologo -p:TreatWarningsAsErrors=true`: Exit 0, Windows-MAUI, Core und Tests gebaut, **0 Warnungen, 0 Fehler**.
- `dotnet format FlowNRW.Core/FlowNRW.Core.csproj --verify-no-changes --no-restore` und entsprechender Befehl für FlowNRW.Tests: jeweils Exit 0.
- `python .githooks/csproj-xmldoc-check.py --all`: Exit 0, **76 Dateien geprüft**. Hook unverändert. Die zwischenzeitlich entfernte portable Pythoninstallation wurde aus ihrem bereits vorhandenen temporären Archiv wiederhergestellt; danach lief der Hook erfolgreich.

Die vorhandenen Provider-/Service-Liveproben werden weiterverwendet und nicht als neue Abrufe ausgegeben. Betriebs-/Erweiterungsdokumentation und erneute unabhängige Reviews folgen beim übergeordneten Lifecycle. Keine Commits, Branchwechsel oder Änderungen an Review-/Todo-/Projektdateien durch den Korrekturagenten.
