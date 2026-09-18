# Test-Ergebnisse

## Ergebnis

**Status:** Fehler vorhanden

## Fehlgeschlagene bzw. nicht ausgeführte Pflichtprüfung

- **Echter Windows-Standortversuch** — Vor Prozessstart durch automatische Freigabeprüfung wegen sensibler Positionsübermittlung abgelehnt; Nutzerfreigabe ausstehend. Kein Produktfehler nachgewiesen.

## E2E-Abdeckung

| Szenario | Nachweis | Ergebnis |
|---|---|---|
| Manueller Einstieg ohne Standortzugriff | LocationCalls bleibt 0 nach Routing | Bestanden |
| GPS Start/Ziel, Ergebnisse, Details, zurück | Native Location-Fixtures | Bestanden |
| Ablehnung/Entzug, Dienst aus, Timeout, keine Position, Fehler | Native Location-Fixtures und Coretests | Bestanden |
| Umgebungsliste und Monitor | Native Location-Fixtures | Bestanden |
| Umgebungskarte, tatsächlicher Mausklick auf Marker, native Tastaturliste | Native Location-Fixtures | Bestanden |
| Unbekannte Entfernung/Position, leer/Fehler/alte Daten | Native Location-Fixtures und Coretests | Bestanden |
| Späte Standort-/Providerantworten, neue Eingabe/Umgebung, Seitenwechsel | Native Location-Fixtures und Coretests | Bestanden |
| Schmale Darstellung, Tastatur, zurück | Native Location-Fixtures, visuelle Screenshots | Bestanden |
| Routing-/Monitor-/Kartenregression | Drei native Harnessläufe | Bestanden |
| Echte Windows-OS-Abfrage | Automatische Freigabeprüfung abgelehnt | Nicht ausgeführt |
| Native iOS-Ausführung | Manuelle Nutzerprüfliste | Vereinbarungsgemäß beim Nutzer |

## Zusammenfassung

165 Coretests bestanden, 0 fehlgeschlagen, 0 übersprungen. Release-Solution und UiTest-App: 0 Warnungen/0 Fehler. Format und XML-Dokumentation (107 Dateien) bestanden.

## Testabdeckung

**Abdeckung:** 94,42 % Core-Zeilen (1271/1346). LocationResult 100 %, neue Endpunkt-/Monitorpfade einschließlich asynchroner Abläufe überwiegend über 89 %. Bestehende Teilklassen EfaProvider, AsyncRelayCommand, RelayCommand und MapTileService enthalten unter 80 % abgedeckte Anteile; generierte JSON-Dateien bleiben aus der fachlichen Dateibewertung ausgeschlossen. Keine neue Produktdatei im Core ohne Testnachweis. Der MAUI-Adapter liegt außerhalb der Core-Coverage; native OS-Prüfung steht aus.

Vollständige Belege: docs/help/standort/verification/checks-2026-09-18.md und core-cobertura.xml. Getrennte lokale Testausführung nach Agentenlimit.
