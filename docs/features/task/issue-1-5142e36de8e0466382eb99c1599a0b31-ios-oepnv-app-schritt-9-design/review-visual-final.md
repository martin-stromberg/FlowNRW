# Unabhängige Prüfung der visuellen Nacharbeit

Stand: 04.10.2026. Geprüft: uncommitted Änderungen an `DepartureCardView.cs`, `TransitVisuals.cs` und `WindowsDesignUiTests.ps1` auf Basis von `6eb05c2`; `docs/design/acceptance.md`, `inventory-visual-final.md` und `test-results-visual-final.md`.

## Status

**Gezielte Layoutkorrektur visuell bestätigt; noch keine Gesamtfreigabe für Schritt 9.** Die erneute unabhängige Prüfung der `dark-large-fixed`-Bilder bestätigt die lesbare Kopfstruktur bei 150 %. Zwei konkrete Runnerfehler sind behoben. Die vollständige Matrix und die unten genannten globalen Abnahmepunkte bleiben offen.

## Befunde

1. **Behoben:** Nach `MapStation1` wartet der Runner jetzt ebenfalls auf `Departure0`. Die überholte Erfolgstext-Abhängigkeit ist entfernt. Dies ist im Code bestätigt; der neue HomeOnly-Lauf durchläuft den Kartenpfad nicht.
2. **Behoben:** Der Runner erzeugt getrennte Aufnahmen `monitor-unknown` und `monitor-cancelled`, mit Fokus auf `Departure2` beziehungsweise `Departure3`. Die neuen Bilder zeigen beide betroffenen Zeilen und den lesbaren Ausfallstatus. Die reguläre Aufnahme nutzt das vollständige Vier-Abfahrten-Fixture, nicht das ungeeignete Zweilinien-Fixture.
3. **Betriebsbedingung bleibt:** `Scenario='success'` ist weiterhin Default und liefert laut Fixturecode Teil-/Altresultate. Der dokumentierte erfolgreiche Lauf verwendet ausdrücklich `-Scenario favorite-cache-seed`. Auch die noch ausstehende Vollmatrix muss dieses passende Fixture explizit wählen, solange der Default unverändert bleibt.
4. **Gezielter Bildnachweis bestanden:** Im neuen 150-%-Bild sind `RE 1 · Stand 2`, Ist-/Soll-Zeit und `Essen Hauptbahnhof` vollständig lesbar. Ziel und Zeit überdecken sich nicht; der schmale Wortumbruch ist beseitigt. Die eingeklappte Ansicht zeigt weiterhin einzeilige farbige Linienchips mit Uhrzeiten. Die horizontale Chipfolge reicht über den sichtbaren Ausschnitt hinaus, wie vor der Änderung. Die gemeinsame Badge-Komponente benötigt im globalen Restpaket noch aktuelle Verbindungsbilder.

## Tatsächlich betrachtete Bilder

Unabhängig geöffnet und visuell verglichen wurden:

- `artifacts/step9-visual-final/dark-large/dark-430-900-150-favorite-cache-seed-home-favorite-expanded.png`
- `artifacts/step9-visual-final/light-narrow-valid/light-430-900-100-favorite-cache-seed-home-favorite-expanded.png`
- Original `stitch_nrw_transit_ios_app/stitch_nrw_transit_ios_app/abfahrtsmonitor_live_dark_mode/screen.png`

Die App übernimmt die gerundeten Stationsgruppen, farbigen Linienkennzeichnungen, hervorgehobenen Abfahrtszeiten und abgestuften dunklen Flächen erkennbar. Das Referenzbild enthält zusätzliche, vom Nutzer nicht beauftragte Funktionen; deren Weglassen ist korrekt. Die helle Aufnahme zeigt die verdichteten Zeit-/Gleisangaben ohne frühere Vergleichszeile. Das 150-%-Bild bestätigt den dokumentierten Umbruchmangel deutlich: das Ziel wird in schmale Fragmente und das RE-Badge in mehrere Zeilen zerlegt. Diese Bilder belegen den Anlass der Korrektur, nicht deren Erfolg.

Für die Nachprüfung wurden zusätzlich die neuen Bilder `home-favorite-expanded`, `home-favorite-collapsed`, `monitor-unknown` und `monitor-cancelled` unter `artifacts/step9-visual-final/dark-large-fixed/` tatsächlich geöffnet. Ihr Manifest nennt Windows MAUI, 430×900, DPI 96, dunkel, App-Textskalierung 150 %, Basiscommit `6eb05c2`, `tracked files modified` und Build-SHA256 `6DEF1924EECFC9BC354D173B7634CF7C414D2CB0683BD37785B019B6D0AB4210`. Sie belegen den Erfolg der gezielten Korrektur. Die neue zweizeilige Hierarchie weicht zugunsten vollständiger Lesbarkeit bei großer Schrift von der kompakten einzeiligen Entwurfsdarstellung ab; das ist im Sinne der Abnahmekriterien vertretbar.

## Evidenzgrenzen und Abschlussbedingungen

Der Testbericht nennt 278 erfolgreiche Core-Tests, einen warnungsfreien UiTest-Build und einen erfolgreichen PowerShell-Parserlauf nach der Änderung. Diese Läufe wurden in dieser unabhängigen Prüfung nicht erneut ausgeführt. Der vom Nutzer gemeldete vollständige Journey-PASS bezieht sich auf den vorherigen Stand und ist kein Screenshotnachweis.

Die aktuelle Matrix ist überwiegend eine HomeOnly-Matrix. Noch erforderlich sind aktuelle Bilder von Haltestellen-Rückkehr/Cache, Verbindungsfavoriten, Endpunkttausch und verdichteten Details; die zusätzlichen Capture-Aufrufe allein sind keine ausgeführten Nachweise. `stop-monitor-cached` öffnet im neuen Runner die Haltestelle zunächst einmalig und beweist ohne verzögerten Wiederholungsaufruf keine sofortige Cacheanzeige.

Außerdem offen: Kontrastmessungen, aktuelle Bounds/Fokusnachweise für neue Aktionen, dauerhafte Ablage der finalen Matrix unter `docs/help/design/verification`, sowie die ausdrücklich beim Nutzer verbleibende iOS-Geräteabnahme. Diese Punkte dürfen nicht als bestanden markiert werden. Nach Aufnahme der restlichen Bilder ist eine weitere unabhängige Bildprüfung nötig.

Weitere Beobachtung für die globale Textbereinigung: Die neuen Monitorbilder zeigen weiterhin „4 Abfahrten · manuell aktualisiert.“, „Noch keine Favoriten gespeichert.“ und „Automatische Aktualisierung: Aus.“ oberhalb der Fahrtdaten. Dies ist kein neuer Fehler der Kopfkorrektur, bleibt aber gegenüber dem Wunsch nach weniger wiederkehrenden Informationstexten zu bewerten. Die Fahrt ohne gelieferte Echtzeit zeigt keine erfundene Verspätung; ein expliziter Unbekannttext wurde auf Nutzerwunsch zuvor entfernt.

Die Nachprüfung schließt die konkreten Umbruch- und Zustandsaufnahmebefunde dieses kleinen Pakets. Für die Gesamtfreigabe bleiben aktuelle Bilder der anderen geänderten Oberflächen und ihre unabhängige Bewertung erforderlich; iOS wird weiterhin ausdrücklich als ausstehende Nutzerabnahme geführt.
