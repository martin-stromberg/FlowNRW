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

`NearbyStop0` → `MonitorStop` mit „Umgebung Süd“ → abgeschlossener `MonitorBusy`-Zyklus → `Departure0` mit den geladenen Abfahrten.

Damit ist der zuvor stille Klickpfad von einer nicht gespeicherten nahen Haltestelle zum echten Abfahrtsmonitor automatisiert abgedeckt. Die vorhandenen Such-, Favoriten-, Standort-, Karten- und Detail-IDs bleiben Teil der bisherigen Regression.

Hinweis zum Sichtbarkeitsvertrag: Erfolgreiche Aktualisierungen werden seit der Textverdichtung ohne `MonitorStatus`/`FavoriteStatus`-Text und ohne `MonitorMetadata` dargestellt. Refresh- und Lifecycle-Regressionen prüfen Abschlusszustände deshalb über die Busy-Indikatoren (`MonitorBusy`, `FavoriteBusy*`) und die sichtbar bleibenden Fehler- und Datenstandstexte („Letzte bekannte“, „Datenstand“, „nicht gespeichert“). Vollständige Antworten liefert die Fixture über die Szenarien `complete`/`complete-slow`/`resume-fresh`; das Monitor-ViewModel rendert `Departure0` nur für vollständige, nicht veraltete Ergebnisse.

## Abgeschlossener Nachweis, 05.10.2026

Der vollständige native Matrix- und Regressionslauf liegt auf Commit `97b940f` (Build-SHA256 `175DBAB0…095D`, sauberer Arbeitsbaum) vor: alle vier Designmatrizen (430×900 hell/dunkel-150 %, 1024×768 hell/dunkel) mit je 37 PNGs plus das zweiteilige Cache-Refresh-Paar wurden mit `PASS native design matrix sequence` abgeschlossen; die nativen Regressionen `journey-*`, `departure-cache`, `refresh` und `lifecycle` sind über den Retry-Runner `artifacts/night-runner.ps1` alle bestanden. Details stehen in `test-results-visual-final.md` und `review-visual-current.md`.

Zusätzlich wurde ein Produktfehler behoben: Nach dem Entfernen eines Favoriten blieben die verbleibenden Entfernen-Schaltflächen wegen des während `IsSaving` neu gerenderten `CanExecute`-Ergebnisses dauerhaft deaktiviert; `HomePage.cs` invalidiert das Kommando jetzt bei `IsSaving`-Wechseln (`97b940f`).

## Noch offene Abnahme

- Die iOS-Geräteabnahme durch den Nutzer durchführen: Safe Areas, Systemthema, Dynamic Type, VoiceOver sowie Installation und Start auf dem Gerät.

Dieser offene Punkt ist keine als bestanden behauptete UIAutomation-Messung.

## Nicht übernommene Artefakte

`design-draft.zip` und `stitch_nrw_transit_ios_app.zip` bleiben lokal bereitgestellte Referenzarchive. Sie und daraus abgeleiteten PNG-/HTML-Dateien werden in diesem Durchlauf weder als Produktassets noch als Teil dieser Testartefakte übernommen. Herkunft, Inhalt und Hash des neuen Stitch-Archivs sind in `inventory-draft-refresh.md` dokumentiert.
