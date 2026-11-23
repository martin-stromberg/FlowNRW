← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — Beschreibung

## Zweck

Die Anwendung erhält eine einheitliche Datenbasis für den öffentlichen Verkehr. Sie kann Adressen, Haltestellen und Koordinaten auflösen, nahe Haltestellen liefern sowie Verbindungen und Abfahrten mit verfügbaren Echtzeitinformationen bereitstellen.

## Funktionsweise

Die Datenbasis nutzt einen bundesweiten Dienst und einen regionalen NRW-Dienst. Wenn regionale Daten für NRW verfügbar sind, werden sie bevorzugt und mit bundesweiten Soll-Daten zusammengeführt. Fehlende Echtzeit, Warnungen, Providerfehler und Rückfälle bleiben erkennbar.

Die Daten werden vor der Anzeige durch die spätere Oberfläche vereinheitlicht. Nicht gelieferte Angaben werden als unbekannt behandelt. Fahrtzeiten, Umstiege, Fußwege, Linien, Betreiber und gelieferte Geometrien bleiben erhalten.

## Beispiele

- Eine Suche nach einem Bahnhof kann mehrere Adressen oder Haltestellen liefern; die spätere Oberfläche kann daraus eine eindeutige Auswahl anbieten.
- Eine Koordinatensuche liefert Haltestellen in der Umgebung einschließlich der gelieferten Entfernung.
- Eine Verbindung zwischen Berlin und Hamburg kann mehrere Teilstrecken einschließlich Fußweg und Geometrie enthalten.
- Ein NRW-Abfahrtsmonitor kann Sollzeit, Istzeit, Verspätung, Ausfall und Steig anzeigen, soweit der Anbieter diese Werte liefert.

## Einschränkungen

Die verwendeten Datenquellen sind Entwicklungszugänge und einzelne Proben belegen keine flächendeckende oder produktive Verfügbarkeit. Der bundesweite Abruf war in der dokumentierten Probe nicht verfügbar; ein regionaler Entwicklungsdienst diente als gekennzeichneter Rückfall. Schritt 1 enthält keine Bedienoberfläche, keine Favoritenverwaltung und keine native iOS-Abnahme.
