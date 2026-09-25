# Verifikation Monitorintervalle

Abschluss der Prüfungen: 25.09.2026, Windows, .NET 10, native MAUI-App. Projektbasis `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`, geprüfter Arbeitsbranch mit Suffix `-schritt-7-monitorintervalle`. Die Nachweise enthalten ausdrücklich synthetische UiTest-Daten, keine tatsächlichen Gerätepositionen oder privaten Favoriten. Alle Tests blieben lokal; kein IIS oder Deployment.

## Endergebnisse

| Prüfung | Ergebnis | Dauerhafter Nachweis |
|---|---|---|
| Gesamte Release-Solution | 0 Warnungen, 0 Fehler | [Build](release-final.txt) |
| Coretests nach vollständigem Release-Build | 210 bestanden, 0 fehlgeschlagen, 0 übersprungen | [Ausgabe](core-final.txt), [TRX](core-final.trx) |
| Core-Zeilenabdeckung | 1862/2007 = 92,77 %, Mindestgrenze 70 % erfüllt | [Cobertura](core-cobertura.xml) |
| Vollständige native Intervallprüfung | Bestanden, tatsächliche unbeschleunigte 30-Sekunden-Intervalle | [Intervalllauf](intervals-retry1.txt) |
| Betroffene Favoritentimer nach UI-Command-Fix | Bestanden: unabhängige Taktung, Überschneidungsschutz, Fehlererhalt, Aus und mehrfache Navigation | [Wiederholung](favorite-timers-retry.txt) |
| Vollständige native Favoritenregression nach endgültigem Fix | Bestanden einschließlich vier Prozessstarts und Entfernen während laufendem Abruf | [Favoriten](favorites-final-bindings.txt) |
| Vollständige native Routingregression | Bestanden, einschließlich Koordinaten, Fehler, Rücknavigation und veralteter Antworten | [Routing](routing-final.txt) |
| Native Einzelmonitorregression | Bestanden | [Monitore](monitors.txt) |
| Format, XML-Dokumentation und Diff | Root-Prüfung am 25.09.2026: Format Exit 0, XML 123 Dateien PASS, Diff Exit 0 | Siehe Prüfkommandos und Herkunft unten |

Der vollständige Intervalllauf prüft Standard 60 Sekunden, Speichern von 30 Sekunden, Prozessneustart, ersten Timerabruf, sichtbaren Datenwechsel, manuelle/automatische Überschneidung, Fehlererhalt, Settings-Navigation, Aus über 35 Sekunden, Wechsel 30→60, Speicherfehler und Wiederholung, Fensterdeaktivierung über 35 Sekunden, Reaktivierung ohne Aufholabrufe und beide Favoritenmonitore. Nach dem UI-Command-Fix wurden die betroffenen Favoritentimer erneut mit echter Wartezeit geprüft; unveränderte Einzelmonitor- und Settingslogik wurde nicht unnötig erneut durchlaufen.

## Ausgeführte Kommandos

Arbeitsverzeichnis: Repositorywurzel. Nativer Runner: Windows PowerShell mit UIAutomationClient/UIAutomationTypes. Der UiTest-Build vor den endgültigen nativen Läufen war erfolgreich mit 0 Warnungen/0 Fehlern; die Konfiguration enthält isolierte Fixture-Services.

```powershell
dotnet build FlowNRW/FlowNRW.csproj -c UiTest --no-restore -v:q
./tests/WindowsJourneyUiTests/WindowsRefreshUiTests.ps1 -Exe <UiTest-exe>
./tests/WindowsJourneyUiTests/WindowsRefreshUiTests.ps1 -Exe <UiTest-exe> -FavoriteTimersOnly
./tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1 -Exe <UiTest-exe> -Favorites
./tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1 -Exe <UiTest-exe>
dotnet build FlowNRW.sln -c Release --no-restore -p:TreatWarningsAsErrors=true -v:minimal
dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --no-build --no-restore --collect:'XPlat Code Coverage' --results-directory artifacts/step7-final-coverage --logger 'trx;LogFileName=core-final.trx'
dotnet format FlowNRW.sln --verify-no-changes --severity error --no-restore
git diff --check
```

`<UiTest-exe>` bezeichnet `FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe`. Für Format, den repositoryeigenen XML-Checker mit `--all` und Diff existiert kein separater Logfile; diese drei Ergebnisse stammen aus der direkten abschließenden Root-Toolprüfung dieser Sitzung. Nach den nativen Tests wurden ausschließlich Einrückung und Zeilenenden von drei UI-Dateien formatiert; keine Verhaltensänderung. Die Formatprüfung danach lief erfolgreich. Es wurde kein neuer UI-PASS aus dieser reinen Formatänderung abgeleitet.

## Erhaltene Fehlerhistorie und Korrektur

- Der [erste Intervalllauf](history/intervals.txt) scheiterte beim erwarteten ersten automatischen Request für `fixture-0` (erwartet 2). Das war ein fehlgeschlagener Test, keine bloße Nichtausführung. Die vollständige Wiederholung nach erneutem UiTest-Build und ergänzter reiner Testdiagnose bestand. Eine konkrete Ursache dieses ersten Fehlers wurde nicht nachgewiesen; alle später geprüften realen Takte liefen korrekt.
- Der [ursprüngliche Favoritenlauf](history/favorites.txt) und die [Wiederholung](history/favorites-retry1.txt) scheiterten nach Neustart beim Entfernen einer gerade ladenden Karte: Anzeige blieb bei 2 statt 1. Die [gezielte Diagnose](history/favorites-diagnostic.txt) zeigte einen vermeintlichen Speicherfehler, obwohl die Datei bereits aktualisiert war. Der [native Stacktrace](history/remove-probe.txt) lokalisierte eine COMException aus einem abgehängten WinUI-Button bei `CanExecuteChanged`.
- Ursache: `HomePage.RenderCards` entfernte alte Karten, ohne alle Button-Commands abzukoppeln. Der langlebige Refresh-Command benachrichtigte deshalb noch einen abgebauten nativen Button; die Exception unterbrach die Aktualisierung des Favoritenbestands nach erfolgreichem Speichern. Jetzt werden Refresh-, Öffnen- und Entfernen-Commands vor dem Entfernen der Ansichten abgekoppelt. [Gezielte Gegenprobe](history/remove-probe-fixed.txt), vollständige Favoritenregression und betroffene automatische Favoritenzyklen bestanden danach. Die vorübergehende Exceptiondiagnose wurde aus dem Produktcode entfernt. Dieser WinUI-Handlerfehler wird durch den tatsächlichen nativen Neustart-/Entfernen-Fluss abgesichert; ein Core-Unit-Test könnte den geschlossenen WinUI-Handler nicht realistisch reproduzieren.
- Ein zusätzlicher [Reproduktionslauf](history/favorites-repro.txt) hatte zuvor einen UIA-Invoke-Ausführungsfehler beim nächsten Favoritenbutton. Der Harness wartet nun auf aktivierte Controls und macht außerhalb des Fensters liegende Controls über UIA-Scroll/Fokus erreichbar; er wiederholt keine möglicherweise bereits ausgeführte Aktion blind.
- Die erste [Favoritentimer-Wiederholung nach dem Fix](history/favorite-timers-final.txt) wurde wegen eines externen Fensterfokuswechsels sicher abgebrochen. Sie gilt nicht als bestanden. Die vollständige Wiederholung mit durchgehend kontrolliertem Fokus ist unter den Endergebnissen aufgeführt.

Die ursprünglichen Konsolenlogs enthalten teils nur die Ausgabe bis zum terminierenden Fehler; dessen tatsächlich beobachtete Ursache und betroffene Assertion sind deshalb hier zusätzlich festgehalten. Kein alter Fehler wird durch Überschreiben des Logs verdeckt.

## UI und Plattformgrenzen

[Schmale Einstellungsseite](native-refresh-settings-narrow.png) und [schmale Startseite](native-favorites-narrow.png) wurden visuell kontrolliert: Auswahl, Speichern und Hinweise sind lesbar, Buttons nutzbar; längere Karten laufen im Scrollbereich weiter. Die sichtbaren Szenariofelder und Requestzähler sind ausdrücklich UiTest-Instrumentierung und fehlen in Release.

Die Runner verwenden pro Lauf eigene GUID-Verzeichnisse unter `artifacts/tests`, halten deren Pfade über echte Prozessneustarts konstant und stellen die ursprünglichen Umgebungsvariablen anschließend wieder her. Alte Regressionen erhalten nur im isolierten Testsettingspfad `Aus`; Release behält seinen Standard 60 Sekunden. Fokusprüfungen und Screenshots beziehen sich ausschließlich auf den eigenen App-Prozess.

Native iOS-Geräteprüfung bleibt wie vereinbart beim Nutzer; sie wird nicht durch Windows- oder Fixture-Ergebnisse ersetzt. Suspendierung und iOS-Hintergrundaktualisierung gehören zu Schritt 8.
