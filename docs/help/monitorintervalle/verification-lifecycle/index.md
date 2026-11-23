# Prüfung von Wiederaufnahme und Hintergrundkoordination

Stand: 26.09.2026, Schritt-8-Implementierung auf dem Branch task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-8-hintergrund. Die Belege werden gemeinsam mit den geprüften Produktdateien committed. Alle nativen Windows-Läufe verwenden isolierte UiTest-Fixtures; keine private Position ist enthalten.

| Prüfung | Ergebnis / Beleg |
|---|---|
| Vollständige Windows-Release-Solution | 0 Warnungen/0 Fehler: [Build](release-final.txt) |
| Windows UiTest-Build | 0 Warnungen/0 Fehler; der tatsächlich gestartete Stand enthält die finalen Home-/Map-Guards |
| Core | 236 bestanden, 0 übersprungen/fehlgeschlagen: [Ausgabe](core-final.txt), [TRX](core-final.trx) |
| Coverage | 1959/2103 Zeilen = 93,15 %, Branches 80,05 %: [Cobertura](core-cobertura.xml); keine vollständig ungetestete Core-Quelldatei. Generierte Unterklassen können separat 0 % aufweisen. Keine MAUI-/iOS-Coverage daraus ableiten. |
| Native Routingregression | [Bestanden](routing.txt) |
| Native Monitorregression | [Bestanden](monitors.txt) |
| Native Standortregression | [Vollständiger erfolgreicher Wiederholungslauf](locations-retry.txt) |
| Native Favoritenregression | [Bestanden einschließlich Prozessneustarts](favorites.txt) |
| Echte Intervallprüfung | [Bestanden](refresh.txt): reale 30-Sekunden-Wartezeiten, Aus/Wechsel, Speicherung/Fehler, Navigation/Deaktivierung/Resume, unabhängige Karten und keine Doppelabrufe |
| Erweiterte Wiederaufnahme | [Bestanden](lifecycle-final.txt): frisch/alt, Fehler, schnelle Reaktivierung, alte Antworten, unabhängige Favoriten, Aus/manuell, Ergebnis-/Detailerhalt und fehlende Fahrtidentität |
| Native Kartenregression am finalen Produktcode | [Bestanden](maps-retry.txt): Marker/Liste, fehlende Positionen, Offline/Recovery, Pan/Zoom, späte Kacheln, Geometrie und Zurücknavigation |
| iOS C# Compile-Target | [0 Warnungen/0 Fehler](ios-compile.txt). Kein signierter nativer Build, Simulator- oder Gerätelauf. |
| Format und XML | dotnet format FlowNRW.sln --verify-no-changes --severity error --no-restore erfolgreich; XML-Prüfer: 130 C#-/Projektdateien erfolgreich |
| Windows-Release-Skripte | 26 Node-Tests bestanden; Windows-Workflows unverändert. Der npm-PowerShell-Wrapper meldete eine gesperrte benutzerspezifische Prefix-Abfrage, die eigentliche Testsuite lief erfolgreich mit Exitcode 0. |

## Fehlversuche und Korrekturen

[Erster Standortlauf](locations.txt): unmittelbar nach Resize wurde noch die alte Schaltflächenbreite gelesen. Begrenztes Warten auf WinUI-Layout ergänzt; die Größenprüfung bleibt unverändert streng (positive Breite bis 430 Pixel). Erfolgreicher vollständiger Wiederholungslauf misst 382 Pixel für die Umgebungsschaltfläche.

[Erster finaler Kartenlauf](maps-final.txt): Missing StopQuery bei schneller Zurücknavigation. Der Harness wartet nun explizit auf die wieder erreichte Karte bzw. Haltestellensuche, bevor der nächste Schritt folgt. Alle ursprünglichen fachlichen Assertions bleiben bestehen. Vollständiger erneuter Kartenlauf erfolgreich. Ein zuvor erfolgreicher Kartenlauf allein wurde nicht als Nachweis für die später ergänzte Pauseprüfung verwendet.

## Sichtprüfung und Grenzen

- [Umgebung schmal](native-nearby-narrow.png): 430×900-Fenster, dunkles Thema, normale Schrift; Aktionen und Ergebnis-/Quelleninformationen lesbar.
- [Einstellungen schmal](native-refresh-settings-narrow.png): Aus/Intervall/Speichern und vollständige Erklärung der Wiederaufnahme-/iOS-Grenzen sichtbar.
- [Detail ohne bestätigte Fahrt](native-lifecycle-detail.png): verständlicher Status und deaktivierte Kartenaktion, keine erfundene Ersatzauswahl.

Szenariofelder und synthetische Kennungen sind Instrumente des UiTest-Builds. Diese Bilder sind funktionale Schritt-8-Belege, keine finale Designabnahme. Schritt 9 muss die vollständige Entwurfsmatrix gesondert erfüllen. Native iOS-Hintergrundausführung, echte OS-Planung, VoiceOver und Dynamic Type bleiben beim Nutzer: [Gerätecheckliste](../installation.md).

Reviews wurden nach Ausfall des separaten Review-Agenten am Nutzungslimit gemäß Skillfallback lokal durchgeführt. Keine unabhängige Agentenprüfung oder native Apple-Ausführung wird behauptet.