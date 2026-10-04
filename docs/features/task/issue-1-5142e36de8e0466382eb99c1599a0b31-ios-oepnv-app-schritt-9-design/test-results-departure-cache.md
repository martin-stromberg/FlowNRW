# Test-Ergebnisse – Zwischenspeicher für Favoritenabfahrten

**Stand:** 03.10.2026

## Automatisierter Nachweis

`tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1` verwendet pro Lauf getrennte Favoriten-, Cache- und Einstellungspfade.

1. Es speichert einen Favoriten mit einer erfolgreichen zukünftigen Fixture-Abfahrt und prüft die erstellte Cache-Datei.
2. Es startet die App mit derselben lokalen Speicherung und einer künstlich sechs Sekunden verzögerten Anbieterantwort erneut.
3. Vor Abschluss der Antwort prüft es die sichtbare zwischengespeicherte Abfahrt und genau eine angeforderte Startaktualisierung.
4. Nach Abschluss prüft es den automatischen Status und die im Fixture eindeutig markierte Live-Abfahrt.

Der Ablauf ist für eine interaktive Windows-Desktop-Sitzung bestimmt:

```powershell
.\tests\WindowsJourneyUiTests\WindowsDepartureCacheUiTests.ps1 -Exe 'FlowNRW\bin\UiTest\net10.0-windows10.0.19041.0\win-x64\FlowNRW.exe'
```

Er ergänzt die bestehende breite Journey-Regression und wird nicht als ausgeführt behauptet, solange kein nativer Prozesslauf protokolliert ist.

## Lokale Prüfungen

| Prüfung | Ergebnis |
|---|---|
| Core-Suite mit Warnungen als Fehler | 267 bestanden, 0 fehlgeschlagen |
| Windows-/iOS-UiTest-Build mit Warnungen als Fehler | 0 Warnungen, 0 Fehler |
| PowerShell-Syntax beider geänderter Fixture-Skripte | Bestanden |

## Verbleibende manuelle Abnahme

Die iOS-Geräteabnahme bleibt getrennt: lokaler Cache, sofortige Anzeige, laufende Aktualisierung, Abbruch und VoiceOver müssen auf dem bereitgestellten Gerät geprüft werden.

Der native Fixturelauf muss in einer aktiven Desktop-Sitzung ausgeführt werden. Ein Lauf mit versteckt gestarteter App lieferte keinen aktiven Fenster-Lifecycle und damit erwartungsgemäß keine Startaktualisierung; der Fixture-Start aktiviert deshalb vor den Assertions jetzt sein eigenes Fenster. Ein erfolgreicher Prozessnachweis ist noch nachzutragen.
