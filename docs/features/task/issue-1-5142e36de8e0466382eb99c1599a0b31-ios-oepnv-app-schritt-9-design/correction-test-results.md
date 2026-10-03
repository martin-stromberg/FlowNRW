# Tester-Korrekturen – Zwischenprüfung

## Ergebnis

**Status:** Bestanden auf Windows

Die Core- und Testprojekte bauen mit `TreatWarningsAsErrors=true` ohne Warnungen oder Fehler. Der Core-Testbestand läuft mit 255/255 Tests erfolgreich, einschließlich der Auswahl-/Zeit-/Ankunftsmodus-Tests. Die Windows-UI-Test-Fixture wurde auf die erweiterte Routing-Signatur mit Ankunftsmodus aktualisiert. Der Windows-UiTest-Build und der vollständige native Designmatrix-Lauf wurden auf der Entwicklerumgebung erfolgreich ausgeführt.

## Bereits statisch bzw. im Modell geprüft

- K1: kompakte Symbolaktionen mit zugänglichen Beschreibungen und gekürzter, semantisch vollständiger Provenienz.
- K2: zusätzliche Nearby-Projektion, getrennt von gespeicherten Favoriten und ohne Duplikate.
- K3: eindeutige Favoritenzähler-/Statusformulierungen.
- K4/K5: Favoritenfilter bei Fokus, Übernahme des Namens ins Feld und Leeren der Trefferliste nach Auswahl.
- K6: horizontale Such-/Standortaktionen neben dem Eingabefeld.
- K7: „Jetzt“, alternatives Datum/Zeit und Ankunftsmodus mit Weitergabe an die Providerabstraktion.

## Nachweis

- `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`: `PASS native design matrix sequence`.
- 30 native Screenshots und Touch-Target-Prüfungen für Suche, Home, Haltestellen, Karten und Journey-Details.
- K1/K2 Nearby- und Home-Regression bestanden.
- K4–K7 Fokus-, Auswahl-, Zeit- und Routingregression im Matrixlauf bestanden.
- iOS-Geräteabnahme bleibt gemäß Projektvorgabe offen.
