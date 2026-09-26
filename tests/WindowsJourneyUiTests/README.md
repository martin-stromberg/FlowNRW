# Native Windows-UI-Prüfung

Voraussetzungen: Windows mit interaktiver Desktop-Sitzung, .NET 10/MAUI-Windows, Windows PowerShell 5.1 und UIAutomationClient/UIAutomationTypes. Appstart und UIA müssen im selben Windows-Benutzer-/Berechtigungskontext erfolgen. Ein gesperrter Desktop oder getrennte Sandboxkontexte sind kein verlässlicher Testaufbau. Bildschirmaufnahmen benötigen ein sichtbares Vordergrundfenster.

Aus der Repositorywurzel den ausschließlich für Fixtures bestimmten Build erzeugen und in Windows PowerShell 5.1 prüfen:

```powershell
dotnet build FlowNRW/FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true
./tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1 -Exe ./FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe -ScreenshotDirectory ./artifacts/ui-screenshots
```

Das Screenshotverzeichnis vorher anlegen. Der Treiber startet und beendet seinen eigenen Appprozess. `-Inspect` unterstützt die Inspektion. Der Fixture-Lauf benötigt keine erreichbaren Fahrplananbieter und darf nicht als Live-Nachweis bezeichnet werden. `UiTest`-Artefakte niemals ausliefern; regulärer Release verwendet reale Services. Reale Bedienproben separat mit Releasebuild und Netzwerkzugang ausführen.

[Prüfumfang, Screenshots und Live-Grenzen](../../docs/help/verbindungssuche/verification/checks-2026-09-16.md).

Mit `-Monitors` werden die deterministischen Monitorflüsse geprüft. `-LiveMonitors` verwendet mit dem regulären Releasebuild echte Gelsenkirchener Haltestellen/Abfahrten und prüft Aktualisieren und Rücknavigation; Netzwerk und erreichbare Anbieter sind erforderlich. [Monitor-Nachweise](../../docs/help/abfahrten/verification/checks-2026-09-16.md).

`-Maps` prüft Marker per echter Maus, native Listen-/Tastaturwahl, Zoom/Pan, Kartenfehler, fehlende Positionen, Verbindungsgeometrie und Rücknavigation mit isolierten Kachelfixtures. `-LiveMaps` ist eine einzelne Release-Probe mit realer Gelsenkirchener Suche und Basiskarte; keine öffentlichen Kacheln für automatische Pan-/Zoom-Tests verwenden. [Kartenprüfung](../../docs/help/karte/verification/checks-2026-09-17.md).

`-Locations` prüft Standortfreigabe, Fehler/Entzug, späte Antworten und Umgebung→Karte→Monitor mit synthetischen Positionen. `-Favorites` prüft Speichern/Entfernen, Prozessneustarts, Sortierung und unabhängige Karten. Alle fünf Modi (ohne Modusschalter, Monitors, Maps, Locations, Favorites) gehören zur integrierten Regression.

Die eigenen Intervall- und Wiederaufnahmeläufe werden nacheinander ausgeführt:

```powershell
./tests/WindowsJourneyUiTests/WindowsRefreshUiTests.ps1 -Exe ./FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe -ScreenshotDirectory ./artifacts/ui-screenshots
./tests/WindowsJourneyUiTests/WindowsLifecycleUiTests.ps1 -Exe ./FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe -ScreenshotDirectory ./artifacts/ui-screenshots
```

Diese Läufe verwenden echte Wartezeiten und minimieren/aktivieren ausschließlich das eigene Testfenster. Währenddessen nicht mit einer zweiten UI-Automation oder manuell um den Vordergrund konkurrieren. Der Intervalllauf dauert mehrere Minuten. Der Lifecyclelauf prüft frische/veraltete Daten, schnellen Aktivitätswechsel, Abbruch, Fehlererhalt, unabhängige Favoriten, Aus/manuell sowie Ergebnis-/Detailauswahl ohne Navigation. Hintergrund-GPS und echte iOS-Tasks werden damit nicht simuliert oder als bestanden behauptet.
