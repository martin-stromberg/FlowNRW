# Abnahmeprüfung – Entwicklungsschritt 7

## Ergebnis

**Status:** Anforderung vollständig erfüllt

Unabhängige fachliche Abnahme am 25.09.2026 für Produktstand `3c062ed` auf `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-7-monitorintervalle`. Diff-Basis: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Geprüft wurden die ursprüngliche Schritt-7-Anforderung in `project-plan.md`, der Dreipunkt-Diff, die tatsächlichen Quellen und die dauerhaften Testnachweise; Lifecycle-Reviewurteile wurden nicht als Ersatz übernommen.

## Abweichungen

Keine fachlichen Abweichungen im Umfang von Schritt 7 festgestellt.

## Hinweise

Alle nachfolgenden Quellpfade beziehen sich auf die Repositorywurzel.

| Kriterium | Unabhängiger Abgleich und konkrete Fundstellen |
|---|---|
| 1: Wirksame persistente Einstellung, Standard und Grenzen | `FlowNRW.Core/Refresh/RefreshSettingsViewModel.cs`, `LoadAsync`, `SaveAsync`, `IsSupported`: Standard 60; ausschließlich Aus/30/60/120/300; Entwurf wird erst nach erfolgreichem Schreiben wirksam. `JsonRefreshSettingsStore.cs`: begrenztes Lesen, Validierung, temporäre Datei und atomarer Ersatz. `FlowNRW/MauiProgram.cs`: gemeinsamer Settings-Singleton, persistenter AppData-Pfad; Fixtures ausschließlich bedingt kompiliert. `RefreshSettingsPage.cs`: native Auswahl, explizites Speichern und Fehleranzeige. Begründete Grenzen unter `docs/help/monitorintervalle/ablauf-technisch.md`. `RefreshSettingsTests_Persistence.cs` prüft alle Werte, Neustart, beschädigte Daten, Schreibfehler und ungültige Entwürfe. Native Prozessneustarts und sichtbar wirksamer Datenwechsel sind in `verification/intervals-retry1.txt` nachgewiesen. |
| 2: Navigation, Intervallwechsel, Abbruch und Netzlast | `RefreshLoop.cs`, `Start`/`Stop`/`RunAsync`: idempotenter Start, vollständige Wartezeit nach Abschluss, Revision und Cancellation, keine Aufholserie. `FlowNRW/DeparturePage.cs`, `OnAppearing`/`OnDisappearing`/`ReconcileRefreshLoop`, sowie `HomePage.cs`, `ReconcileRefreshLoops`: Seiten- und Fensteraktivität, geladene Einstellungen, eigene Schleife je aktueller Karte und Entfernung obsoleter Schleifen. `App.xaml.cs`, `CreateWindow`: Activated/Deactivated/Destroying angebunden. Bestehender `ProviderOrchestrator.cs`, `Run`, teilt exakt identische Requests; unterschiedliche Anfragezeitpunkte bleiben bewusst unterschiedliche Schlüssel. `RetryPolicy.cs` begrenzt Wiederholungen. `RefreshLoopTests_Scheduling.cs`, `RefreshMonitorTests_Concurrency.cs` und `ProviderOrchestratorTests_Concurrency.cs` sichern Taktung, Wechsel, verspätete Antworten, gemeinsame Abrufe und Abbruch. Native Logs prüfen Aus, Navigation, deaktiviertes Fenster und unabhängige Favoriten mit tatsächlichen 30-Sekunden-Wartezeiten. |
| 3: Manuelle Aktualisierung und erhaltene Fehlerdaten | `StopMonitorViewModel.cs` und `FavoriteMonitorViewModel.cs`, `RefreshCoreAsync`: manuell/automatisch teilen Busy- und Revisionsschutz; Fehler erhalten Result. `JourneyPresentation.cs`, `Metadata`, liefert Quelle, Datenstand und Datenalter, beide Seiten binden diese Metadaten. Native Buttons bleiben erreichbar, auch bei Aus. Cache-/Retry-/Diagnoseimplementierung bleibt im Diff unverändert; `TransitDiagnostics.cs` speichert keine Requests/Antworten und begrenzt technische Felder. `HomePage.RenderCards` koppelt Commands ab, bevor alte native Karten entfernt werden. Fehlererhalt, manuelle Überschneidung und Entfernen während Abruf sind nach endgültigem Fix nativ erfolgreich geprüft. |
| 4: Deterministische und native Regression | Tatsächliche neue Testmethoden in `FlowNRW.Tests/RefreshTests.cs`, `RefreshLoopTests_Scheduling.cs`, `RefreshMonitorTests_Concurrency.cs`, `RefreshSettingsTests_Persistence.cs` gelesen. `tests/WindowsJourneyUiTests/WindowsRefreshUiTests.ps1` bedient echte UIAutomation-Controls, misst reale Zeit, kontrolliert Fensterfokus und prüft echte Prozessneustarts bei isolierten Dateien. Die Endlogs belegen Intervall-, Favoriten-, Routing- und Monitorflüsse; kein reiner ViewModel-PASS wird als native Ausführung gewertet. |

### Prüfbelege und Grenzen

- Dauerhafte Nachweise unter `docs/help/monitorintervalle/verification/`: `release-final.txt` belegt Release-Solution mit 0 Warnungen/0 Fehlern; `core-final.txt` und die vorhandene `core-final.trx` belegen 210 bestandene Tests, keine Fehler/übersprungenen Tests. Cobertura-Wurzel unabhängig gelesen: 1862/2007 Zeilen, 92,77 %, über der Grenze 70 %.
- `intervals-retry1.txt`, `favorite-timers-retry.txt`, `favorites-final-bindings.txt`, `routing-final.txt` und `monitors.txt` einschließlich ihrer Abschlussassertionen gelesen. Der komplette Intervalllauf bestand; nach dem späteren Command-Abkopplungsfix wurden betroffene Favoritentimer und die vollständige Favoritenregression erneut erfolgreich ausgeführt. Frühere Fehler und der externe Fokusabbruch bleiben in der Historie ausdrücklich als fehlgeschlagen erhalten.
- Format, XML-Dokumentation und Diff-Prüfung sind in `verification/index.md` als direkte Root-Prüfung mit erfolgreichen Ergebnissen dokumentiert; hierfür existieren keine gesonderten Logdateien. Diese Abnahme hat gemäß Auftrag keine Builds, Tests oder UI erneut ausgeführt und behauptet dies nicht.
- Windows-Test-/Release-Workflows sind im Produktdiff unverändert. Kein IIS und keine neue Deploymentautomatisierung. Die neue gemeinsame MAUI-Fenster-/Seitenintegration und DI wurden gelesen; plattformspezifische iOS-Ausführung bleibt ausdrücklich beim Nutzer. `docs/help/monitorintervalle/installation.md` enthält eine konkrete manuelle iOS-Prüfanleitung.
- Suspendierung, Hintergrundmechanismen und Wiederaufnahme bleiben Schritt 8. Die vollständige visuelle Integration und Screenshotmatrix gehören gemäß bestätigtem Projektplan zu Schritt 9; diese fachliche Abnahme behauptet keine abgeschlossene Designabnahme.
- Die zum Prüfzeitpunkt zusätzlich vorgemerkte `core-final.trx` ergänzt ausschließlich den bereits dokumentierten Testnachweis. Es gab keine uncommittierte Produktkorrektur. `design-draft.zip` blieb unberührt. Der Bericht ist vor Integration zusammen mit den zugehörigen Nachweisen zu sichern.
