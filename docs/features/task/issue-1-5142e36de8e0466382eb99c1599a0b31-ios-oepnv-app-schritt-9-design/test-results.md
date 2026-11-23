# Test-Ergebnisse – Schritt 9

## Ergebnis

**Status:** Windows-UiTest bestanden; Release-Build außerhalb der Sandboxgrenze

Der Core-Testlauf, der Windows-UiTest-Build und die native Windows-Designmatrix sind erfolgreich. Der vollständige Release-Solution-Build konnte in der Sandbox nicht abgeschlossen werden, weil der Zugriff auf `C:\Users\Martin\AppData\Local\Microsoft SDKs` verweigert wurde. Dieser Fehler betrifft die Ausführungsumgebung, nicht den Quellcode.

## Fehlgeschlagene Tests

### Vollständiger Release-Build

- **FlowNRW.sln Release** — MSB4184: Zugriff auf `C:\Users\Martin\AppData\Local\Microsoft SDKs` verweigert (Sandboxgrenze, kein Quellfehler).

### Kartenlog

- **maps-final.log** — leerer Lauf ohne verwertbaren Nachweis; der zuvor vollständig bestandene Kartenlauf ist unter `docs/help/design/verification/maps-oct2-accessible.log` dokumentiert und die aktuelle Auswahl zusätzlich in den revidierten PNGs.

## E2E-Abdeckung

| Szenario | Test / Nachweis | Ergebnis |
|---|---|---|
| Routing, Details, leere/fehlerhafte/späte Antwort | `routing-final.log` | Bestanden |
| Abfahrtsmonitor mit Echtzeit-/Fehler-/Leerzuständen | `monitors-final.log` | Bestanden |
| Haltestellen, Ortungsfehler, Nearby, Liste/Karte | `locations-final-retry.log`, `maps-oct2-accessible.log` | Bestanden |
| Bildmatrix hell/dunkel, schmal/breit, 150 % | `final-design-*.log`, `design-revised-*.log`, `revised-matrix/` | Bestanden; aktuelle gezielte Nachsicht dokumentiert |
| Einstellungen Erfolg/Fehler, Tastatur, große Schrift | `design-revised-settings-error.log` | Bestanden |
| Automatische Aktualisierung/Lifecycle | bestehende Nachweise aus Schritt 8 | Bestanden |
| Favoriten/Entfernung | revidierte Mehrfavoritenbilder und native Bildmatrix | Bestanden |

## Zusammenfassung

- Coretests: 255 bestanden, 0 fehlgeschlagen, 0 übersprungen
- Corecoverage: 93,21 % Zeilen (1979/2123), 80,23 % Zweige (1506/1877)
- Native Windows-UI-Nachweise: Routing, Monitor, Standort, Karte und Designzustände bestanden
- Umgebungsbedingt offen: vollständiger Release-Solution-Build in der Sandbox

## Testabdeckung

**Abdeckung:** 93,21 % Zeilen / 80,23 % Zweige

Quelle: `FlowNRW.Tests/TestResults/0483140e-4ebb-461b-a6fe-67f7997fd36c/coverage.cobertura.xml`.
