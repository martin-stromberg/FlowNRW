# Bestandsaufnahme: konfigurierbare bundesweite und regionale Fahrplanauskunft

Analysiert wurde der vorhandene C#/.NET-MAUI-Code für die in `requirement.md` beschriebene Datenversorgung von Adress-, Haltestellen-, Routing-, Abfahrts- und Echtzeitdaten. Die Bestandsaufnahme berücksichtigt den tatsächlichen Repository-Aufbau unter `FlowNRW.Core/`, `FlowNRW.Tests/` und `FlowNRW/` sowie die vorhandenen CI-Konfigurationen.

## Zusammenfassung

- Der plattformunabhängige Fachkern enthält ausschließlich `ClickCounter` in `FlowNRW.Core/ClickCounter.cs`; fachliche ÖPNV-Modelle, Provider, Services, Adapter, Normalisierung, Konsolidierung und Cache sind nicht vorhanden.
- Die MAUI-App ist ausschließlich auf `net10.0-windows10.0.19041.0` ausgerichtet. iOS-Plattformdateien, Providerkonfiguration und fachliche UI/Services für die Fahrplanauskunft existieren nicht.
- `MauiProgram` verwendet den MAUI-DI-Container bislang nur für die App-Erzeugung und Debug-Logging; fachliche Services sind nicht registriert.
- Es gibt keine fachlichen Interfaces oder Enums für Routing, Haltestellen, Abfahrten oder Echtzeit.
- Das Testprojekt enthält nur fünf bestehende Counter-Testfälle; Service-, Modell-, Provider-, Cache- und Integrationstests für Schritt 1 fehlen.
- Der dokumentierte Baseline-Testlauf weist 5 erfolgreiche, 0 fehlgeschlagene und 0 übersprungene Tests aus. Der bestehende Projekt-Nachweis ist unter [inventory/tests.md](inventory/tests.md) verlinkt.
- Die bestehende Windows-Test-/Releasekonfiguration in GitHub Actions ist vorhanden; ein iOS-CI- oder iOS-Deployment-Job ist nicht vorhanden.

## Details

- [Datenmodelle](inventory/models.md)
- [Logik](inventory/logic.md)
- [Tests](inventory/tests.md)

Interfaces und Enums wurden nicht angelegt, da im geprüften Bestand keine für die Anforderung relevanten Interfaces oder Enums vorhanden sind.
