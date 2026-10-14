← [Zurück zur Übersicht](index.md)

# Design — Anwenderablauf

## Voraussetzungen

Keine. Das Design gilt appweit; helles/dunkles Erscheinungsbild folgt der Systemeinstellung.

## Schritt für Schritt

1. **Tab-Leiste unten:** Die drei Bereiche „Abfahrten", „Verbindungen" und „Haltestellen" zeigen nun ein Symbol vor dem Titel (Abfahrtstafel, Route, Ortssymbol). Die Bedienung ist unverändert.
2. **Aktionsbuttons:** Statt Textzeichen tragen alle Aktions- und Symbolschalter echte Icons — z. B. Plus (Haltestelle hinzufügen), Karten- und Standortsymbol, Aktualisieren, Zahnrad (Einstellungen), Lupe (Suchen), Häkchen (Koordinaten übernehmen), Vertausch-Pfeile, Stern. Die Schalter sind 48×48 groß und bleiben per Tastatur und Hilfstechnologie erreichbar.
3. **Abfahrts- und Verbindungskarten:** Linienbadge, Ziel und Uhrzeit stehen in einer klareren Zeilenstruktur; die Hauptzeit erscheint in einer festen Ziffernschrift. Weicht die gemeldete Zeit ab, steht die Sollzeit durchgestrichen darunter.
4. **Status lesen:**
   - „Fahrt fällt aus" bzw. „Ausfall gemeldet" erscheint als Chip mit Symbol — nie nur als Farbe.
   - „+n Min." zeigt eine gemeldete Verspätung als Chip.
   - „Keine Echtzeitdaten" bedeutet ausdrücklich, dass keine aktuelle Meldung vorliegt — nicht, dass die Fahrt pünktlich ist.
5. **Favoriten:** Der Stern ist umrandet, solange die Haltestelle oder Verbindung nicht gespeichert ist, und gefüllt, sobald sie gespeichert ist. Auf der Startseite klappen die Chevron-Symbole die Abfahrtsliste einer Favoritenkarte auf oder zu.

## Ergebnis

Alle Aktionen und Zustände sind als Symbole mit Text oder Beschreibung erkennbar; keine Information hängt allein an einer Farbe. Inhalte, Daten und Navigationsziele haben sich nicht geändert.

## Barrierefreiheit

- Jeder Symbolschalter trägt eine gesprochene Beschreibung (z. B. „Abfahrten aktualisieren", „Als Favorit speichern"); Zustandswechsel aktualisieren die Beschreibung mit.
- Interaktive Ziele sind mindestens 44×44 (Symbolschalter 48×48) groß.
- Status werden als Symbol plus Text dargestellt; Kontraste der neuen Semantikfarben erreichen ≥4,5:1 auf den jeweiligen Flächen (im Dunkelmodus geprüfte Aufhellungen).
- Bei 150 % Textskalierung rendern die Icons vollständig ohne Abschneiden (geprüft in der [Icon-Matrix](verification/icon-matrix-run.md)).
