# Native Windows-UI-Automation: Vorprüfung

Datum: 15. September 2026. Ausgangsstand: `be7bb34`, noch unveränderte MAUI-Vorlagenoberfläche.

Die vorhandene Release-App unter `FlowNRW/bin/Release/net10.0-windows10.0.19041.0/win-x64/FlowNRW.exe` wurde lokal gestartet. Nach dem asynchronen Fensteraufbau war das Fenster `FlowNRW` in Windows-Sitzung 1 erreichbar. Die anfängliche Fensterkennung 0 unmittelbar nach dem Start ist kein Beleg für fehlende UI-Testmöglichkeiten.

Windows PowerShell 5.1 konnte mit `Add-Type -AssemblyName UIAutomationClient` und `UIAutomationTypes` das Fenster über die Prozess-ID unter `AutomationElement.RootElement` finden. Der native Elementbaum enthielt Titel, Begrüßung und den Button `Click me`. Der Button wurde mit `InvokePattern.Invoke()` tatsächlich ausgelöst; anschließend lieferte der sichtbare Elementbaum `Clicked 1 time`. Das eigens gestartete Fenster wurde danach geschlossen.

Damit sind native Elementinspektion und Buttonbedienung in dieser Sitzung nachgewiesen. Dies ist ausschließlich eine Prüfung der Testinfrastruktur, keine Abnahme der noch nicht implementierten Verbindungssuche. Deren Eingaben, Trefferwahl, Ergebnisdetails, Rücknavigation und Fehlerzustände müssen anschließend mit den tatsächlichen neuen Controls geprüft werden.

Ein vorheriger Versuch, FlaUI 5.0 direkt in PowerShell 7 zu laden, scheiterte an der Typauflösung von `UIA3Automation`. Die funktionierende integrierte Windows-UI-Automation macht diesen Adapter für den lokalen Test nicht erforderlich. Native iOS-Prüfung verbleibt beim Nutzer.
