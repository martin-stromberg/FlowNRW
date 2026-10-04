# Unabhängige Prüfung der visuellen Nacharbeit

## Ergänzende unabhängige Breitprüfung, 04.10.2026

Die folgenden Feststellungen ergänzen und aktualisieren die älteren Evidenzgrenzen dieses Berichts. Tatsächlich geöffnet wurden aus `artifacts/step9-visual-final/wide-full-light/` die zwölf Bilder `home-favorite-expanded`, `home-favorite-collapsed`, `home-distance-known`, `monitor-cancelled`, `stop-list`, `stop-list-returned`, `stop-monitor-cached`, `map-stations`, `map-native-list`, `map-offline`, `map-no-position` und `monitor-provider-error`. Vergleichsgrundlage waren außerdem die intakten Originalbilder `abfahrtsmonitor_live/screen.png` und `umgebungskarte_stationen/screen.png` aus dem entpackten aktualisierten Stitch-Entwurf sowie `docs/design/acceptance.md`.

Der geprüfte Ordner enthält 23 PNGs mit 23 zugehörigen Manifestzeilen. Zuordnung: Basiscommit `81cdd69ae83f58133becfcd077deebaf0a1c36dc`, Build-SHA256 `D5523269D832EACCB89124BB7C49DD21DAC1364A7F762364EDBE98688BFB9B8B`, Windows MAUI, hell, 1024×768, DPI 96, Textskalierung 100 %. Das Manifest nennt einen sauberen versionierten Arbeitsbaum mit unversionierten Dateien. Dies ist kein vollständiger Matrix-PASS: `run.log` endet nach der erfolgreichen Größenprüfung von `OriginCoordinateMode`. Offlinekarte, Karte ohne Position, Monitorfehler und Suchformular wurden vorher aufgenommen. Ein Abbruch vor oder im Offlinezweig ist für diesen konkreten Lauf nicht belegt.

### Visuell bestätigt

- Breite Favoriten- und Monitoransichten übernehmen gerundete Karten, farbige Linienkennzeichnungen, hervorgehobene Zeiten und die abgestufte helle Flächenhierarchie des Entwurfs. Ziele, Badges und Zeitangaben überdecken sich in den betrachteten Bildern nicht. Die Smartphone-Referenz wird dabei als breite, auf 840 Einheiten begrenzte Inhaltsspalte adaptiert.
- Die eingeklappte Karte zeigt Linien und Zeiten nebeneinander; aufgeklappt erscheint stattdessen die Abfahrtsliste. Die abweichende Soll-Zeit ist klein und durchgestrichen unter der Ist-Zeit. Das unveränderte Gleis erscheint einmal, ein Gleiswechsel mit geplantem Wert. Der Ausfall ist zusätzlich zur gestrichenen Zeit als „Fahrt fällt aus“ lesbar.
- Nach Rückkehr aus dem Monitor sind beide Suchtreffer wieder sichtbar. Das Eingabefeld enthält dabei den ausgewählten Stationsnamen. Das belegt erhaltene Treffer im aufgezeichneten Rückweg.
- Karte und native Listenalternative sind aufgenommen. Der Offlinezustand nennt die fehlende/veraltete Basiskarte und hält Stationsmarker sichtbar. Bei fehlenden Positionen zeigt die Karte keine erfundenen Marker. Die OSM-Attribution bleibt sichtbar. Synthetische Kacheln beweisen keine echte Kartendarstellung und keine echten geografischen Daten.
- Der Providerfehler bleibt verständlich und gleichzeitig bleiben zuletzt bekannte Abfahrten sichtbar. Die normalen Monitorbilder zeigen keinen wiederkehrenden Aktualisierungserfolg oder Intervalltext mehr.

### Restbefunde und Nachweisgrenzen

1. **Textbereinigung noch unvollständig:** Suchtreffer und native Kartenliste zeigen weiterhin Providerkennung, Stop-ID/DHID und Koordinaten. Der Suchkopf enthält einen langen Quellen-/Fallback-/Altersabsatz sowie die wiederkehrenden Texte „Standort nur nach Aktion verwenden.“ und „Endpunkt übernommen.“ Das widerspricht dem Wunsch nach weniger technischen Alltagsinformationen. Fehler, veraltete/unvollständige Ergebnisse und fehlende Positionen müssen bei einer Verdichtung weiter verständlich bleiben; sichtbare Kartenattribution bleibt erforderlich.
2. **Cachebild nicht hinreichend:** `stop-monitor-cached` zeigt einen gefüllten Monitor. Im Runner ist es aber der erste dort aufgezeichnete Öffnungsvorgang, und vor dem Bild wird auf `Departure0` gewartet. Das beweist weder einen erneuten Aufruf derselben Station noch eine sofortige Cacheanzeige während eines verzögerten Refreshes.
3. **Unbekannte Entfernung nicht pauschal belegt:** Die Bilder mit Namen `home-distance-unknown` können bereits bekannte Entfernungen zeigen, weil die Standortbestimmung automatisch abgeschlossen ist. Die tatsächlich betrachtete Startansicht zeigt „Luftlinie: 111 m“ und einen separaten Hinweis auf einen Favoriten ohne Koordinaten. Der Dateiname allein ist kein Beleg des unbekannten Zustands aller Karten.
4. Für neue Verbindungsfavoriten, Endpunkttausch und Verbindungsdetails wurde in dieser Prüfung kein aktuelles Bild abgenommen. Auch dunkel, schmal und große Schrift für die neu aufgenommenen Haltestellen-/Kartenpfade sind hier nicht nachgewiesen. Es erfolgte keine iOS-Prüfung.

**Ergebnis:** Der helle breite Haltestellen-/Karten-/Abfahrtsstand ist teilweise visuell bestätigt; Suchrückkehr und erhaltene Fehlerdaten sind konkret sichtbar. Gesamtfreigabe von Schritt 9 bleibt wegen der genannten Textbefunde, Cache-/Verbindungs- und Matrixlücken sowie der offenen iOS-Geräteabnahme ausstehend. Bereits dokumentierte Token-Kontrastwerte werden durch diese Bildprüfung weder ersetzt noch erneut gemessen.

Stand: 04.10.2026. Geprüft: uncommitted Änderungen an `DepartureCardView.cs`, `TransitVisuals.cs` und `WindowsDesignUiTests.ps1` auf Basis von `6eb05c2`; `docs/design/acceptance.md`, `inventory-visual-final.md` und `test-results-visual-final.md`.

## Status

**Gezielte Layoutkorrektur visuell bestätigt; noch keine Gesamtfreigabe für Schritt 9.** Die erneute unabhängige Prüfung der `dark-large-fixed`-Bilder bestätigt die lesbare Kopfstruktur bei 150 %. Zwei konkrete Runnerfehler sind behoben. Die vollständige Matrix und die unten genannten globalen Abnahmepunkte bleiben offen.

## Befunde

1. **Behoben:** Nach `MapStation1` wartet der Runner jetzt ebenfalls auf `Departure0`. Die überholte Erfolgstext-Abhängigkeit ist entfernt. Dies ist im Code bestätigt; der neue HomeOnly-Lauf durchläuft den Kartenpfad nicht.
2. **Behoben:** Der Runner erzeugt getrennte Aufnahmen `monitor-unknown` und `monitor-cancelled`, mit Fokus auf `Departure2` beziehungsweise `Departure3`. Die neuen Bilder zeigen beide betroffenen Zeilen und den lesbaren Ausfallstatus. Die reguläre Aufnahme nutzt das vollständige Vier-Abfahrten-Fixture, nicht das ungeeignete Zweilinien-Fixture.
3. **Behoben:** Der Designrunner verwendet jetzt standardmäßig `favorite-cache-seed`. Das frühere Default-Szenario `success` liefert absichtlich Teil-/Altresultate und bleibt nur für gezielte Fehlerbilder verfügbar. Damit erzeugt der dokumentierte einfache Runneraufruf vollständige Abfahrtszustände.
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
