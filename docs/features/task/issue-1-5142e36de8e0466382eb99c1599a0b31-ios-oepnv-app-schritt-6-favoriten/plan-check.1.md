# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan lückenhaft

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK 1: Hinzufügen/Entfernen, Duplikate, Neustart, nur technische Stationsdaten | JSON-Store mit Source+Id, begrenzter geordneter Stopliste, atomarem Schreiben; Erfolg erst nach Speicherung; Monitoraktion und Home-Entfernen | Core-Persistenz/Fehler/Duplikate; native Hinzufügen→Home, zweiter Prozess mit derselben Testdatei, Entfernen/Leerzustand | Lücke: sichtbare Speicherfehler noch nicht konkret nativ geprüft |
| AK 2: Entfernungssortierung, stabile Ordnung ohne Standort, Leerzustand | Explizite Standortaktion, Haversine, Unbekannte am Ende, stabile Gleichstände; manuelle Suchnavigation | Core bekannte/unbekannte/Gleichstand/Fehler; native mehrere Favoriten mit Fixture-Position, ohne Position und leerer Einstieg | Abgedeckt |
| AK 3: Echtzeit-/Fehler-/Cachezustände, manuelle Aktualisierung, keine Doppelabrufe, unabhängiges Laden, Navigation | Eigener Departure-Service und Zustand je Karte; Refreshschutz; Abbruch bei Entfernen/Seitenabgang; validierte Favoriten-/Kartenauswahl | Core unabhängige Refreshes, Doppelabrufe, Fehlererhalt/alte Antworten; native einzelne Refreshfehler und Navigation | Lücke: langsames paralleles Laden und Doppelklick noch nicht konkret nativ geprüft |
| AK 4: Persistenz-/Duplikat-/Sortiertests und native Neustart-/Navigationsflüsse | Neue Coretests und Erweiterung tatsächlicher UIA-Harness, getrennte UiTest-Datei, zweiter Prozess | Native Pflichtabläufe einschließlich Neustart, Entfernen, Leerzustand, Sortierung und Rücknavigation ausdrücklich geplant | Abgedeckt |
| Plattform-/Datenschutz-/Workflowrahmen | Keine Nutzerposition/Abfahrten persistieren, iOS-Sourcegeneration, unabhängige DI; keine neuen Pakete/IIS/Deployment/iOS-CI | Corecoverage, Release/UiTest, Format/XML, bisherige native Regressionen, iOS-Anleitung | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

- [ ] AK 1: Native UiTest-Speicherfehler für Hinzufügen und Entfernen gezielt auslösen (ausschließlich Teststore/Fixture), Aktion über tatsächlichen Button ausführen und sichtbare Fehlermeldung sowie unveränderten gespeicherten Favoritenbestand prüfen. Nach Aufhebung des Fehlers Wiederholung und korrekten Erfolg prüfen. Core-Dateifehlertests ersetzen diesen neuen sichtbaren Fehlerfluss nicht.
- [ ] AK 3: Native Fixture mit langsamem Refresh von Karte A und unabhängig erfolgreichem Refresh von Karte B vorsehen. Während A lädt, B bedienen und dessen Ergebnis prüfen; A doppelt aktivieren und nachweisbar nur einen Anbieterabruf bestätigen. Ein ausschließlich fehlgeschlagener Refresh belegt noch nicht, dass laufendes Laden die übrige Startseite freilässt.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Leerer Home-Einstieg → Suche → Monitor → Favorit → Home | Tatsächliche native Buttons, Favoritenkarte mit Abfahrten | Abgedeckt |
| Neustart/Persistenz | Zweiter Appprozess mit derselben isolierten UiTest-Datei | Abgedeckt |
| Duplikat/Entfernen/Leerzustand | Native Favoritenaktionen und sichtbarer leerer Home-Zustand | Abgedeckt |
| Persistenzfehler | Bisher nur Core-Datei-/Fehlernachweis; native Fehleraktion und Wiederholung fehlen | Lücke |
| Entfernung/fehlender Standort | Mehrere gespeicherte Stops, explizite Fixture-Position, sortierte Reihenfolge bzw. stabile Ordnung ohne Position | Abgedeckt |
| Einzelner Refreshfehler | Betroffene Karte fehlerhaft, andere Karte weiterhin nutzbar | Abgedeckt |
| Parallel laufende Refreshes und Doppelklick | Bisher explizit auf Coreebene; native langsame Karte plus zweite Karte fehlt | Lücke |
| Monitor/Karte/Routing/Zurück | Bestehende native Regressionen über neuen Home-Einstieg plus Favoritennavigation | Abgedeckt |
| Schmale Ansicht/Tastatur | Native Darstellung und Tastaturaktionen | Abgedeckt |

## Fehlende oder unvollständige Planbestandteile

Keine Umsetzungslücke; ausschließlich die zwei konkret benannten nativen Testergänzungen erforderlich.

## Hinweise

Unabhängige Prüfung am 19.09.2026 anhand vollständiger requirement.md, inventory.md und plan.md sowie tatsächlicher Klassen `AppShell`, `DeparturePage`, `DepartureService`, `Stop`, `MapViewModel` und bekannter Monitor-/Standortintegration. Keine verlinkten Inventardetaildateien vorhanden. `DepartureService` beendet ältere Operationen derselben Instanz; getrennte Instanzen je Favoritenkarte sind deshalb sachlich notwendig und geplant. Vollständige technische Stopdaten umfassen Id, Source, Name, optionale Dhid und Stationskoordinate; sie sind von der nicht zu speichernden aktuellen Nutzerposition unterschieden.

Die neue Home-Navigation, die validierte Aufnahme vollständiger Favoriten in Karten-/Monitorauswahl sowie die Verwendung derselben Testdatei über einen echten Prozessneustart sind ausführbar beschrieben. Konkrete AutomationIds und Dateigrößengrenzen sind routinemäßige Implementierungsentscheidungen. Keine fachlichen Rückfragen oder Architekturänderungen erforderlich. Keine Produktdateien geändert.
