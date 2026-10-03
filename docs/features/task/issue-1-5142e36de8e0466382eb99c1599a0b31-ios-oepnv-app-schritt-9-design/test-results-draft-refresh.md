# Test-Ergebnisse – aktualisierter Stitch-Entwurf

**Stand:** 03.10.2026
**Status:** Kompilierung und Fachtests bestanden; nativer visueller Nachweis noch offen

## Belegte Prüfungen

| Prüfung | Befehl / Nachweis | Ergebnis |
|---|---|---|
| Core-Test-Suite | `dotnet test .\FlowNRW.Tests\FlowNRW.Tests.csproj --no-restore` | 256 bestanden, 0 fehlgeschlagen, 0 übersprungen |
| Nearby-Identitätsgrenze | `dotnet test .\FlowNRW.Tests\FlowNRW.Tests.csproj --no-restore --filter FullyQualifiedName~StopMonitorTests` | 7 bestanden, einschließlich Home-Nearby-Öffnung und unvollständiger Identität |
| Windows-/iOS-UiTest-Kompilierung | `dotnet build .\FlowNRW\FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore` | 0 Warnungen, 0 Fehler |
| PowerShell-Syntax | `WindowsJourneyUiTests.ps1` mit `System.Management.Automation.Language.Parser.ParseFile` | Bestanden |
| Whitespace | `git diff --check` | Keine Whitespace-Fehler; Git meldet nur Zeilenendungs-Hinweise für bestehend berührte Dateien |

## Automatisierte UI-Abdeckung

Der bestehende `-Favorites`-Fixture-Ablauf in `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1` enthält jetzt zusätzlich:

`NearbyStop0` → `MonitorStop` mit „Umgebung Süd“ → abgeschlossenes `MonitorStatus` → `MonitorMetadata` mit `fixture-nearby-0`.

Damit ist der zuvor stille Klickpfad von einer nicht gespeicherten nahen Haltestelle zum echten Abfahrtsmonitor automatisiert abgedeckt. Die vorhandenen Such-, Favoriten-, Standort-, Karten- und Detail-IDs bleiben Teil der bisherigen Regression.

## Noch offene Abnahme

- Den nativen Windows-Designmatrixlauf für die aktualisierten Oberflächen in Hell und Dunkel ausführen und die neuen Screenshots prüfen. Er muss mindestens Startseite/Nearby-Drilldown, Abfahrtsmonitor, Suche, Verbindungsdetail sowie Haltestellensuche/Karte enthalten.
- Die iOS-Geräteabnahme durch den Nutzer durchführen: Safe Areas, Systemthema, Dynamic Type, VoiceOver sowie Installation und Start auf dem Gerät.

Diese offenen Punkte sind keine als bestanden behaupteten UIAutomation-Ergebnisse.

## Nicht übernommene Artefakte

`design-draft.zip` und `stitch_nrw_transit_ios_app.zip` bleiben lokal bereitgestellte Referenzarchive. Sie und daraus abgeleiteten PNG-/HTML-Dateien werden in diesem Durchlauf weder als Produktassets noch als Teil dieser Testartefakte übernommen. Herkunft, Inhalt und Hash des neuen Stitch-Archivs sind in `inventory-draft-refresh.md` dokumentiert.
