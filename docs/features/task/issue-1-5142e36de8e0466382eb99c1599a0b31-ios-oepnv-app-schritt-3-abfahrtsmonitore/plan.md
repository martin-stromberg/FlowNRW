# Plan – Haltestellensuche und manueller Monitor

## Entscheidungen, Dateien und Reihenfolge

1. `StopMonitorViewModel` verwendet eine eigene vorhandene `EndpointViewModel`-Instanz, `IDepartureService` und neuen `IDepartureNavigation`. Nur Treffer mit gültiger Stop-ID werden angeboten; die komplette Identität bleibt erhalten. `DeparturePresentation` formatiert Linie/Ziel, Soll/Ist, Verspätung, Ausfall, Gleiswechsel und unbekannte Angaben über die vorhandenen Zeit-/Metadatenhelfer.
2. `StopSearchPage` zeigt beschriftete Suche, gefilterte native Trefferbuttons mit WordWrap und Lade-/Leer-/Fehlerzustände. Auswahl öffnet den richtigen Monitor. `DeparturePage` zeigt Karten, Status, Quelle/Datenalter und „Aktualisieren“. `ShellDepartureNavigation`, DI, Shell-Routen und erreichbarer Einstieg „Abfahrten“ werden ergänzt; Routing bleibt unverändert.
3. Beim Öffnen und manuellen Aktualisieren asynchron laden. Fehler erhalten den letzten erfolgreichen Datenstand samt Quelle und kennzeichnen ihn ausdrücklich als letzte bekannte Daten; erfolgreiche leere Antworten zeigen den Leerzustand. Neue Stopauswahl verwirft alte Ergebnisse. Abbruch plus Revision schützen vor verspäteten Antworten, Busy/Command vor Doppelstarts. Rücknavigation erhält den Suchtext. Nur nächste bekannte Abfahrten anzeigen, unbekannte Zeiten ausdrücklich kennzeichnen. Gemeinsame Abrufe/Cache/NRW-Auswahl bleiben im vorhandenen Core.
4. Deterministische Coretests und isolierte UiTest-Fixtures erweitern; nativen Monitor-Harness ergänzen und vorhandenen Routing-Harness als Regression ausführen. Reale native NRW-Abfahrtsprobe, Windows-Warnings-as-errors, Format/XML und gesamte Core-Coverage prüfen. Dann getrennte Reviews, Hilfe/README/Release Notes und Lifecycleabschluss.

Keine Datenbankmigration, neue Konfiguration, GPS, Favoriten, Timer, Hintergrundmechanismen oder IIS. Native iOS-Abnahme bleibt beim Nutzer; Prüfanleitung wird ergänzt.

## Tests je Akzeptanzkriterium

| AK | Deterministische Prüfung | Native Windows-E2E: Setup → Aktion → sichtbares Ergebnis |
|---|---|---|
| 1 | Stop-Filter, vollständige Identität, Such-Revision | Einstieg „Abfahrten“, mehrdeutigen Text suchen, Treffer wählen → richtiger Stop im Monitor; zurück → Text erhalten. Adresstreffer ohne Stop-ID werden nicht angeboten. Leere/fehlerhafte Suche anzeigen und erfolgreich wiederholen. |
| 2 | Ist unbekannt/pünktlich, positive/negative Verspätung, Ausfall, Gleiswechsel/Tageswechsel | Fixture mit diesen Abfahrten öffnen → sämtliche Status textlich und in schmalem Fenster lesbar. |
| 3 | Fehler erhält alte Daten/Quelle, Leerantwort ersetzt, Stopwechsel verwirft Daten, alte Antwort ignoriert, Doppelstart verhindert | Aktualisieren: Erfolg → neue Daten, Fehler → alte Daten mit Hinweis, leer → Leerzustand. Langsamen Abruf verlassen und anderen Stop öffnen → keine späte Überschreibung. Fallback/Warnung/Datenalter sichtbar. |
| 4 | Bestehende Provider-/Konsolidierungstests plus neue Monitortests, mindestens 70 % gesamte Core-Coverage | Neue Monitorflüsse und vorhandene Routingabläufe nativ bedienen. Reguläre App nach NRW-Stop suchen und reale Abfahrten abrufen; Datum, Ergebnis und Grenzen protokollieren. |

UI-Tests verwenden tatsächliche native Controls über bestehende UIA-Infrastruktur; ViewModeltests ersetzen sie nicht. Fixture-DI ausschließlich im getrennten UiTest-Build. Bestehende Windows-GitHub-Actions unverändert.

## Offene Punkte

Keine.
