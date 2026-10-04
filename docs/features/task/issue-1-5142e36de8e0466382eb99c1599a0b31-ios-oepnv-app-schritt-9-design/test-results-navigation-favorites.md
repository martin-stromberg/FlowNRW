# Testergebnisse – Navigation und Verbindungsfavoriten

**Stand:** 04.10.2026

## Automatisierte Kernprüfung

- `dotnet test .\FlowNRW.Tests\FlowNRW.Tests.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore`
  - erfolgreich: **278/278**
- `dotnet build .\FlowNRW\FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore`
  - iOS-Simulator und Windows: **0 Warnungen, 0 Fehler**

Die zusätzlichen Kernprüfungen decken den Roundtrip gerichteter Verbindungsfavoriten, die Auswahl und Vertauschung ohne Providerabruf sowie den Haltestellen-Sitzungscache mit erhaltener Trefferliste ab.

## Nativer Windows-Lauf

- `WindowsJourneyUiTests.ps1` wurde mit der gebauten UiTest-EXE gestartet.
- Bestätigte Schritte: deaktivierte Routensuche ohne Endpunkte, Texteingabe ohne Auswahl, mehrdeutige Endpunktliste, Auswahl eines zweiten Starttreffers und Auswahl des Ziels.
- Der Prozess blieb anschließend in der nativen Automation ohne weitere Ausgabe stehen und wurde beendet. Der Lauf ist deshalb **nicht als erfolgreicher E2E-Nachweis** gewertet.

Die Ursache liegt im nativen UI-Automationslauf nach der Endpunktauswahl und muss vor der Abnahme reproduziert werden. Smartphone-/iOS-Prüfung bleibt separat.
