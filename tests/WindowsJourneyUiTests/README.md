# Native Windows-UI-Prüfung

Voraussetzungen: Windows mit interaktiver Desktop-Sitzung, .NET 10/MAUI-Windows, Windows PowerShell 5.1 und UIAutomationClient/UIAutomationTypes. Appstart und UIA müssen im selben Windows-Benutzer-/Berechtigungskontext erfolgen. Ein gesperrter Desktop oder getrennte Sandboxkontexte sind kein verlässlicher Testaufbau. Bildschirmaufnahmen benötigen ein sichtbares Vordergrundfenster.

Aus der Repositorywurzel den ausschließlich für Fixtures bestimmten Build erzeugen und in Windows PowerShell 5.1 prüfen:

```powershell
dotnet build FlowNRW/FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true
./tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1 -Exe ./FlowNRW/bin/UiTest/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe -ScreenshotDirectory ./artifacts/ui-screenshots
```

Das Screenshotverzeichnis vorher anlegen. Der Treiber startet und beendet seinen eigenen Appprozess. `-Inspect` unterstützt die Inspektion. Der Fixture-Lauf benötigt keine erreichbaren Fahrplananbieter und darf nicht als Live-Nachweis bezeichnet werden. `UiTest`-Artefakte niemals ausliefern; regulärer Release verwendet reale Services. Reale Bedienproben separat mit Releasebuild und Netzwerkzugang ausführen.

[Prüfumfang, Screenshots und Live-Grenzen](../../docs/help/verbindungssuche/verification/checks-2026-09-16.md).
