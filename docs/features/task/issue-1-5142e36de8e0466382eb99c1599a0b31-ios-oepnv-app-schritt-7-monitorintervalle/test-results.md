# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

Alle angeforderten Core- und nativen Windows-Prüfungen sind abgeschlossen. Der reproduzierte native Favoritenfehler wurde behoben und auf dem endgültigen Build erneut geprüft. Projektabnahme und Abschlusscommit sind gesonderte, noch nicht durch diesen Bericht erledigte Schritte.

## Fehlgeschlagene Tests und Wiederholungen

Keine offenen Testfehler. Frühere Fehlschläge bleiben dokumentiert: erster automatischer Tick, Favoriten-Entfernen nach Neustart, UIA-Invoke-Ausführung und externer Fokuswechsel. Der Favoritenfehler war ein echter Produktfehler durch nicht abgekoppelte Commands abgebauter WinUI-Buttons; HomePage koppelt sie jetzt vor der Entfernung ab. Die genaue Chronologie, Ursachen, Grenzen und unveränderten alten Logs stehen im [dauerhaften Prüfbericht](../../../help/monitorintervalle/verification/index.md). Für den ersten Tickfehler wurde keine konkrete Ursache nachgewiesen; die vollständige Wiederholung und spätere echte Taktungen bestanden.

## E2E-Abdeckung

| Szenario | Ergebnis / Nachweis im dauerhaften Ordner |
|---|---|
| Standard 60, schmale Auswahl, Tastaturspeichern, 30 über Prozessneustart | Bestanden: intervals-retry1.txt |
| Tatsächliche unbeschleunigte 30-Sekunden-Taktung, sichtbarer Standwechsel | Bestanden: intervals-retry1.txt |
| Langsamer automatischer Abruf und manuelle Überschneidung | Bestanden: intervals-retry1.txt und favorite-timers-retry.txt |
| Fehler behalten letzte Daten, Quelle/Alter; manuelle Erholung | Bestanden: beide Intervallnachweise |
| Aus, 30→60-Wechsel, Settings-/Suchnavigation, Fensterdeaktivierung/Reaktivierung | Bestanden: intervals-retry1.txt |
| Settings-Speicherfehler, wirksamer Altwert, Retry und Neustart | Bestanden: intervals-retry1.txt |
| Unabhängige Favoriten, Aus und mehrfache Navigation ohne zusätzliche Schleifen | Bestanden nach endgültigem Fix: favorite-timers-retry.txt |
| Hinzufügen/Entfernen, Duplikate, Distanzsortierung, Speicherfehler, vier Prozessstarts und Entfernen bei laufendem Abruf | Bestanden nach endgültigem Fix: favorites-final-bindings.txt |
| Routing mit allen Eingabearten, Fehlern, Details, Rücknavigation und veralteten Antworten | Bestanden: routing-final.txt |
| Manuelle Einzelmonitorregression | Bestanden: monitors.txt |

## Builds, Coretests und Abdeckung

- Vollständige Release-Solution: 0 Warnungen, 0 Fehler.
- UiTest-Build vor den finalen nativen Läufen: 0 Warnungen, 0 Fehler.
- Anschließende Core-Suite: 210 bestanden, 0 fehlgeschlagen, 0 übersprungen.
- Zeilenabdeckung: 92,77 % (1862/2007), Mindestgrenze 70 % erfüllt.
- Finale Root-Prüfung am 25.09.2026 nach reinen Whitespacekorrekturen: `dotnet format FlowNRW.sln --verify-no-changes --severity error --no-restore` Exit 0, XML-Checker `--all` PASS für 123 Dateien, `git diff --check` Exit 0. Diese drei Prüfungen haben Toolnachweise in der Sitzung, keinen separaten Logfile.

[Build-, TRX-, Cobertura- und native Nachweise](../../../help/monitorintervalle/verification/index.md) sind dauerhaft abgelegt. Keine neue Produktlogik nach den abschließenden nativen Läufen; die danach ausgeführte Formatierung änderte ausschließlich Einrückungen und Zeilenenden. Native iOS-Geräteabnahme bleibt beim Nutzer; kein lokaler iOS-PASS wird behauptet.

