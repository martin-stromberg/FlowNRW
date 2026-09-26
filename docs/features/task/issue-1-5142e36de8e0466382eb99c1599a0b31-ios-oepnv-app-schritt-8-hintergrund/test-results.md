# Test-Ergebnisse

## Ergebnis

**Status:** Keine Fehler

## Zusammenfassung

236 Coretests bestanden, 0 fehlgeschlagen, 0 übersprungen. Vollständiger Releasebuild vor dem finalen --no-build-Test: 0 Warnungen/0 Fehler. Zeilenabdeckung 93,15 Prozent (1959/2103), Branches 80,05 Prozent. Keine Core-Quelldatei vollständig ohne Abdeckung; generierte Unterklassen sind nicht separate Produktdateien. UI/iOS werden nicht in diese Coverage einbezogen.

## E2E-Abdeckung

| Szenario | Nachweis | Ergebnis |
|---|---|---|
| Monitor frisch/veraltet, schneller Wechsel, Abbruch | lifecycle-final.txt | Bestanden |
| Unabhängige Favoriten, Fehlererhalt, Aus/manuell | lifecycle-final.txt | Bestanden |
| Ergebnisse/Details, frische Daten, keine Navigation, fehlende Fahrt | lifecycle-final.txt | Bestanden |
| Routing/Monitore/Standort/Favoriten/Karten | routing.txt, monitors.txt, locations-retry.txt, favorites.txt, maps-retry.txt | Bestanden |
| Echte Intervalle, Persistenz, Einstellung, Aktivität, Navigation | refresh.txt | Bestanden |
| Schmale Oberfläche, Tastatur, sichtbare Hinweise | Screenshots und native Läufe | Bestanden |
| iOS-Gerät, BGTaskScheduler-Ausführung | Nutzercheckliste | Vereinbarte externe Abnahme; hier nicht ausgeführt |

## Weitere Prüfungen

UiTest-Build, iOS-C#-Compile-Target, Format, XML und 26 Release-Skripttests erfolgreich. Kein signierter nativer iOS-Build. Windows-CI bleibt unverändert.

## Dauerhafte Belege und Fehlversuche

Alle Dateien und Details unter [verification-lifecycle](../../../help/monitorintervalle/verification-lifecycle/index.md). Layout-Wartefehler bei schmaler Umgebungssuche und zu frühe Zurücknavigation im Kartenharness wurden mit erhaltenen fachlichen Assertions korrigiert und jeweils vollständig erneut erfolgreich ausgeführt; ursprüngliche Fehlerlogs bleiben archiviert. Keine fehlgeschlagenen finalen Windows-Szenarien offen.