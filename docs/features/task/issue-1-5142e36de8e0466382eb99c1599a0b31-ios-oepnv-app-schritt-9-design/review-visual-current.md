# Unabhängige visuelle Nachprüfung – Schritt 9

## Finaler Nachweis – 05.10.2026

Geprüft wurde die vollständige finale Bildmatrix auf Commit `97b940f` (Build-SHA256 `175DBAB0…095D`, `tracked files clean; untracked files present`):

- `artifacts/step9-visual-final/light-narrow-final/` — hell, 430×900, 100 %
- `artifacts/step9-visual-final/dark-narrow-final-150/` — dunkel, 430×900, 150 %
- `artifacts/step9-visual-final/light-wide-final/` und `dark-wide-final/` — 1024×768, 100 %
- `artifacts/step9-visual-final/cache-refresh/` — Cache während verzögertem Refresh

Tatsächlich geöffnet und gegen `docs/design/acceptance.md` sowie die Stitch-Referenzen bewertet wurden u. a. die Favoriten-Startseite (eingeklappt/aufgeklappt, bekannte und unbekannte Entfernung), der Abfahrtsmonitor in den Zuständen normal/verspätet/abgebrochen/unbekannt/Providerfehler, Haltestellensuche samt Rückkehr, Stationskarte mit Auswahl, Offline- und Positionsfehlern sowie nativer Listenalternative, Verbindungsformular mit Validierung, Ergebnisliste und Verbindungsdetails, Einstellungen und beide Cache-Bilder.

### Stand der früheren Befunde

- Der überflüssige Erfolgstext „Endpunkt übernommen.“ in der Suchliste ist entfallen; sichtbar bleibt nur die kompakte Unvollständigkeitswarnung.
- Ergebnis- und Suchkarten nennen plausible Stations-/Betreibertexte (z. B. „Essen Hauptbahnhof“, „Düsseldorf Hauptbahnhof“, „Regionalverkehr NRW“) ohne technische IDs, DHIDs oder Koordinaten. Treffernamen wie „Essen Hauptbahnhof (Umgebung)“ sind verständlich.
- Ergebniskarten zeigen das Datum beschriftet („Abfahrt: 16.09.2026“) statt einer unbeschrifteten UTC-Angabe; Ausfälle sind textlich („Ausfall gemeldet“, „Fahrt fällt aus“) lesbar.
- Der Cache während des Refreshs ist mit dem eigenständigen Bildpaar `cache-start-slow-nearby-cached.png`/`cache-start-slow-nearby-live.png` belegt: gespeicherte Abfahrten bleiben mit sichtbarem Ladeindikator stehen und werden danach durch Livezeilen ersetzt.
- Schmale und große Schrift (430×900 bei 100 % und 150 %) sowie beide Themes sind jetzt für Startseite, Monitor, Suche, Verbindung, Detail, Karte und Einstellungen belegt. Bei 150 % bleibt alles ohne Überlagerung lesbar; in den Verbindungsdetails bricht `UTC +02:00` sauber um.
- Die Aktionsfläche „Haltestellen auf Karte zeigen“ steht in der Suchkarte und ist semantisch beschrieben; die Stationsliste bleibt nach der Monitorrückkehr erhalten.

### Verbleibende Beobachtungen (keine Produktbefunde)

- `monitor-provider-error` zeigt den Szenarionamen `monitor-error` als Stationskopf — nachvollziehbares Fixture-Artefakt der Fehlerbildung, kein Produktname.
- Die Karte nutzt absichtlich die synthetische Kachelfläche; reale OSM-Kacheln sind damit nicht bewiesen, die Attribution bleibt lesbar.
- UIAutomation erreicht keine Elemente unterhalb der sichtbaren Karten-WebView (Plattformgrenze); die Karteninformationsfläche wurde daher in der Listenansicht bzw. über den nachweislich umschaltenden Barrierefreiheitsnamen geprüft.

### Urteil

Die Windows-Oberfläche erfüllt die Abnahmekriterien anhand der sauberen, eindeutig zugeordneten Endmatrix in allen verlangten Größen-, Theme- und Skalierungsvarianten. Es gibt keine offenen Produktbefunde aus der Bildprüfung. Die iOS-Geräteabnahme bleibt separat ausstehend; bis dahin ist Schritt 9 nicht abgeschlossen.

---

Prüfdatum: 04.10.2026. Geprüft wurden die verbindlichen Kriterien in `docs/design/acceptance.md`, die Stitch-Referenzbilder für Haltestellensuche, Karte, Abfahrtsmonitor, Verbindungssuche und Verbindungsdetail sowie folgende native Windows-Aufnahmen:

- `artifacts/step9-visual-final/wide-full-light-clean-copy/`: insbesondere Haltestellenliste, Rückkehr aus dem Monitor, gecachter Haltestellenmonitor, Stationskarte, Auswahl, Offline- und Positionsfehlerzustand.
- `artifacts/step9-visual-final/dark-wide-connections/`: dunkles Suchformular, Ergebnisliste und Verbindungsdetail.
- `artifacts/step9-visual-final/wide-connection-light/`: helle Gegenstücke und weitere Such-/Detailzustände.

Die Bilder sind 1024×768, 96 DPI und 100 % Textskalierung. Die Aufnahmeordner bilden nicht alle denselben Build ab: `wide-full-light-clean-copy` nennt `b169797` als Basis und „tracked files modified“; `dark-wide-connections` nennt `5a1b847`; `wide-connection-light` nennt `a3e125c`. Die zuletzt betrachteten Suchbilder entstanden in einem modifizierten Arbeitsbaum. Ihre kompakte Warnung entspricht inhaltlich dem später committeten `a723e43`, aber das Manifest weist diesen Commit nicht als Basis aus. `b169797` korrigiert unter anderem Fixture-Geometrie und Windows-Runner; `a723e43` verdichtet die Suchwarnung. Für eine eindeutige Abnahme des exakten finalen Builds sollten mindestens die betroffenen Suchbilder einmal mit sauberer Commitzuordnung neu aufgenommen werden.

## Bestandene Kriterien anhand der Bilder

- Die Hauptnavigation zeigt die drei beschlossenen, beschrifteten Bereiche „Abfahrten“, „Verbindungen“ und „Haltestellen“. Die Verbindungsdetails sind eine Unterseite mit sichtbarem Zurückweg.
- Die betrachteten hellen Seiten verwenden den hellen Hintergrund, gerundete weiße Karten und graue innere Gruppen. Die dunklen Verbindungsseiten verwenden schwarzen Hintergrund, dunkle Karten und helle Schrift. Es gibt keine auffälligen Überlagerungen in den gezeigten 1024×768/100-%-Ansichten.
- Das Verbindungsformular gruppiert Start und Ziel in einer Karte; Start/Ziel-Tausch, Suche und Standortaktionen sind vorhanden. Treffer- und Ergebnisansichten zeigen korrekt „Düsseldorf“, ohne den früheren UTF-8-Darstellungsfehler.
- Die Ergebnisliste gliedert Verbindungskarten mit Dauer, Umstiegen und farbigen Linienbadges. Der Umstiegs-/Fußwegdetailpfad ist sichtbar. Ein Ausfall wird neben der gestrichenen Sollzeit zusätzlich als „Ausfall gemeldet“ beziehungsweise „Fahrt fällt aus“ bezeichnet.
- Die Haltestellensuche zeigt eine kompakte Unvollständigkeitswarnung. Technische IDs und Koordinaten erscheinen nicht in den geprüften Stationszeilen. Die Trefferliste bleibt nach Rückkehr aus dem Monitor erhalten.
- Die Kartenansicht hat eine zugängliche Listenalternative, erkennbare „Liste“-/„Karte“-Umschaltung und sichtbare Attribution. Der Offlinezustand erklärt, dass die Basiskarte fehlt/veraltet ist; der Zustand ohne Stationpositionen zeigt null Kartenpositionen statt erfundener Marker.
- Der Haltestellenmonitor zeigt die vorhandenen Abfahrten mit farbigen Linienkennzeichnungen, Ist-Zeit und kleiner durchgestrichener Sollzeit. Gleisänderungen zeigen den geplanten Wert; unveränderte Gleise zeigen ihn nicht.

## Konkrete visuelle Befunde

1. **Erfolgstext erscheint nach der Auswahl weiter:** Im Bild `stop-list-returned` steht weiterhin „Endpunkt übernommen.“ oberhalb der kompakten Unvollständigkeitswarnung. Die Auswahl ist in diesem Zustand bereits erfolgt und der Anwender befindet sich nach Rückkehr wieder in der Trefferliste. Der dauerhafte Erfolgstext nimmt Raum ein und hilft bei der nächsten Aktion nicht.
2. **Detailansicht zeigt lange Providerdiagnose prominent:** In hell und dunkel steht oberhalb der Zusammenfassung ein mehrteiliger Text mit Fixture-Quelle, Datenstand, Datenalter, Ersatzquelle, veraltetem Cache und Anbieterwarnung. Das ist lesbar, aber visuell stärker präsent als die eigentliche Verbindung und widerspricht der Anforderung nach einer verdichteten Detailansicht. Die notwendige Warnung sollte kurz an den Datenzustand gebunden bleiben; technische Quelle/Zeitdetails gehören in eine sekundäre Informationsfläche.
3. **Ergebniszeit wiederholt die Abfahrtszeit technisch:** Die Ergebnisbilder zeigen unter „Ist: 23:58 → Unbekannt“ nochmals `16.09.2026 23:55 UTC+02:00`. Das wiederholt die sichtbare Sollabfahrt 23:55 und macht die Karte mit Datum und UTC-Offset unnötig technisch. Falls das Datum für Fahrten über Mitternacht benötigt wird, sollte es knapp und beschriftet erscheinen.
4. **Haltestellen-Kartenaktion wirkt losgelöst:** Auf der Suchseite steht die Aktion „Haltestellen auf Karte zeigen“ als einzelnes zentriertes Fadenkreuz unterhalb des Warntexts. Die semantische Beschreibung ist im Code vorhanden, der sichtbare Bezug zur Suche/Liste ist gering. Eine beschriftete Aktion oder ein neben der Liste platzierter Kartenknopf wäre visuell eindeutiger.
5. **Fixture-Namen wirken wie Testmodus:** Die Suchliste zeigt „Essen Hauptbahnhof Treffer 0/1“. Die Teststeuerung ist verborgen, aber diese Ergebnisnamen verraten den Fixture-Modus. Für dauerhaft nutzbare Abnahmebilder sollten plausible Stationsnamen ohne Ordnungsindex verwendet werden.
6. **Kartenbilder belegen nur die Layoutform:** Die neutralisierten Tile-Fixtures zeigen Raster, abstrakte grüne Linien und nummerierte Marker, keine echte geografische Basiskarte. Das ist ein nützlicher Layout-/Offline-Nachweis, aber kein visueller Beleg für reale OSM-Kacheln, geografische Einordnung oder Kartenlesbarkeit mit echten Stationsnamen.

## Noch offene Pflichtnachweise

- Die aktuellen Verbindungsbilder sind breit und mit 100 % Textskalierung. Für diese Formulare, Ergebnisse und Details fehlen in den geprüften aktuellen Ordnern schmale 430×900-Aufnahmen; ebenso fehlt ein 150-%-Lauf für Such- und Detailansichten. Die vorhandenen 430×900/150-%-Bilder decken hauptsächlich Home, Monitor und Einstellungen ab.
- Die hier geprüfte Stationskarte und Haltestellensuche sind hell. Eine aktuelle dunkle Haltestellen-/Kartenmatrix mit Offline- und fehlender-Position-Zuständen liegt hier nicht vor.
- Die Bilder `stop-monitor-cached` beweisen die Anzeige einer gefüllten Liste. Der Runner wartet auf `Departure0`, bevor er aufnimmt; er prüft nicht, dass die gecachten Daten vor einer verzögerten Refreshantwort sichtbar sind. Cache während eines laufenden Refreshes bleibt als visueller Zustand offen.
- Kontrastzahlen und UIA-Touchbounds sind separate Nachweise; sie wurden durch diese Sichtprüfung nicht neu gemessen. Insbesondere sind in diesen Bildserien keine VoiceOver-Prüfung oder iOS-Touchmessungen enthalten.
- Die iOS-Geräteabnahme bleibt offen. Es liegen hier keine iPhone-Aufnahmen für Safe Areas, Dynamic Type, VoiceOver, Hoch-/Querformat oder reale GPS-Entfernung vor.

## Gesamturteil

Die hellen und dunklen breiten Verbindungsansichten sowie die helle breite Haltestellen-/Kartenstrecke zeigen eine konsistente, moderne native Flächensprache und überwiegend klare Hierarchie. Die zentralen UI-Pfade und ihre Datenzustände sind anhand der Aufnahmen nachvollziehbar. Schritt 9 ist dennoch **nicht abnahmebereit**: Die genannten Textverdichtungen und der Fixture-Eindruck sollten behoben oder bewusst akzeptiert werden; außerdem fehlen aktuelle schmale/große Schrift-, dunkle Karten-, Cache-während-Refresh- und iOS-Gerätenachweise. Die Screenshot-Metadaten sind für die drei Gruppen nicht vollständig auf `a723e43` vereinheitlicht.
