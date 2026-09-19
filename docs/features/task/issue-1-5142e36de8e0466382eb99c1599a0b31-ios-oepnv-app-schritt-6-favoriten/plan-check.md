# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK 1: Hinzufügen/Entfernen, Duplikate, Neustart, technische Daten | Begrenzter atomarer JSON-Store mit Source+Id und Sourcegeneration; Erfolg erst nach Speicherung; Monitoraktion/Home-Entfernen | Core-Datei-/Identitäts-/Fehlertests; native Hinzufügen→Home, Duplikate/Entfernen, echter zweiter Prozess; ergänzte sichtbare Speicherfehler samt Wiederholung und Neustart | Abgedeckt |
| AK 2: Aufsteigende Entfernung, stabile Ordnung ohne Standort, Leerzustand | Expliziter Standortbutton, Haversine, unbekannte Entfernungen am Ende, stabile Gleichstände; Weg zur manuellen Suche | Core bekannte/unbekannte/Gleichstand/Fehler; native mehrere Favoriten mit Fixture-Position, ohne Position und leerer Home-Einstieg | Abgedeckt |
| AK 3: Echtzeit-/Fehler-/Cachezustände, manuelle Aktualisierung, keine Doppelabrufe, unabhängiges Laden, Navigation | Eigener Departure-Service und Zustand pro Karte, Refreshschutz, Abbruch bei Entfernen/Seitenabgang; validierte Favoriten-/Kartenauswahl | Core Parallelität/Fehlererhalt/alte Antworten; native einzelne Fehler, ergänzter langsamer Refresh A bei bedienbarem B und Doppelklick mit Aufrufzähler; Navigationsregression | Abgedeckt |
| AK 4: Deterministische und tatsächliche native Nachweise einschließlich Neustart | Erweiterte Core-Suite und echter UIA-Harness mit isolierter persistenter Testdatei | Hinzufügen, zweiter Prozess, Entfernen, Leerzustand, Sortierung und Navigation ausdrücklich geplant | Abgedeckt |
| Rahmenbedingungen | MVVM/DI, technische Stops statt Position/Abfahrtenhistorie, bestehende Provider, Windows-Actions, keine Pakete/IIS/Deployment/iOS-CI | Coverage mindestens 70 %, Release/UiTest, Format/XML, alle betroffenen nativen Regressionen, iOS-Anleitung | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

Keine.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Leerer Einstieg → Suche → Monitor → Favorit → Home | Tatsächliche native Buttons und Karte mit Abfahrten | Abgedeckt |
| Persistenz/Neustart | Zweiter Appprozess verwendet dieselbe isolierte UiTest-Datei und zeigt Favoriten | Abgedeckt |
| Duplikat/Entfernen/Leerzustand | Native Favoritenaktionen und sichtbarer leerer Home-Zustand | Abgedeckt |
| Speicherfehler/Wiederholung | UiTest-Fehler beim Hinzufügen/Entfernen, sichtbarer Misserfolg, Wiederherstellung, erneute Aktion und Neustartprüfung | Abgedeckt |
| Entfernung/fehlender Standort | Mehrere Favoriten mit expliziter Fixture-Position sortieren; ohne Position stabile Reihenfolge | Abgedeckt |
| Fehler/Parallelität/Doppelklick | Einzelner Refreshfehler; langsame Karte A, separat aktualisierbare B; Doppelklick auf A mit UiTest-Aufrufzähler | Abgedeckt |
| Monitor/Karte/Routing/Zurück | Favoritennavigation plus bestehende native Regressionen über neuen Home-Einstieg | Abgedeckt |
| Schmale Ansicht/Tastatur | Native Darstellung und Tastaturaktionen | Abgedeckt |

## Fehlende oder unvollständige Planbestandteile

Keine.

## Hinweise

Unabhängige erneute Prüfung am 19.09.2026. Der erste Bericht ist als `plan-check.1.md` archiviert. Beide dort genannten Testlücken wurden im aktuellen Plan unter „Zusätzliche native Pflichtfälle“ gezielt geschlossen; keine weitere Nachplanung erforderlich.

Prüfgrundlage: vollständige requirement.md, inventory.md und aktualisierte plan.md sowie tatsächliche `AppShell`, `DeparturePage`, `DepartureService`, `Stop`, `MapViewModel` und bekannte Monitor-/Standortintegration. Keine verlinkten Inventardetaildateien vorhanden. Die unabhängigen Serviceinstanzen vermeiden das vorhandene gegenseitige Abbrechen derselben DepartureService-Instanz. Vollständige technische Stopidentität, begrenzte Persistenz, isolierte Testdatei und echte Neustartprüfung sind nachvollziehbar geplant. Mitgliedschaft für vorhandene Karten-/Suchkandidaten bleibt erhalten; Favoriten erhalten einen eigenen validierten Übernahmeweg.

Konkrete AutomationIds und Dateigrößengrenzen sind routinemäßige Implementierungsentscheidungen. Keine fachliche Rückfrage erforderlich. Diese Prüfung bewertet den Plan, keine bereits umgesetzte oder getestete Funktion. Ausschließlich Prüfberichte geschrieben bzw. archiviert; keine Produktänderung oder Commits.
