# Anforderung – Zwischenspeicher für Favoritenabfahrten

**Erfasst:** 03.10.2026
**Status:** Verbindliche Ergänzung zu `requirement.md`

## Ziel

Beim Start soll die App für gespeicherte Favoriten ohne vermeidbare Wartezeit die zuletzt bekannten, noch bevorstehenden Abfahrten zeigen. Parallel wird weiterhin eine aktuelle Abfrage beim Anbieter ausgelöst. Der Zwischenspeicher verbessert allein die wahrgenommene Startgeschwindigkeit; er ersetzt keine Aktualisierung.

## R1 – Persistieren der Ergebnisse

- Nach einer erfolgreichen Abfrage der Abfahrten eines Favoriten speichert die App die für diesen Favoriten erhaltenen Abfahrtsdaten lokal und dauerhaft.
- Die gespeicherten Daten enthalten nur die Informationen, die zur bisherigen Darstellung der Abfahrten erforderlich sind, sowie einen Zeitpunkt der erfolgreichen Aktualisierung und die Haltestellenidentität.
- Ein Cache-Eintrag ist eindeutig dem jeweiligen Favoriten bzw. dessen Haltestellenidentität zugeordnet. Daten verschiedener Haltestellen dürfen nicht vermischt werden.
- Entfernt der Anwender einen Favoriten, entfernt die App dessen gespeicherte Abfahrtsdaten ebenfalls. Abgelaufene Abfahrten werden bei jeder Cache-Verwendung verworfen.

## R2 – Verhalten beim Start

- Beim Start lädt die App die gespeicherten Favoriten und deren lokale Abfahrtsdaten.
- Für jeden Favoriten zeigt sie unverzüglich alle gespeicherten Abfahrten an, deren planmäßige oder erwartete Abfahrtszeit noch nicht vergangen ist.
- Gleichzeitig startet sie wie bisher eine Aktualisierung beim Anbieter. Die Anzeige bleibt währenddessen bedienbar; ein zurückhaltender Ladehinweis darf zeigen, dass aktuellere Daten geladen werden.
- Trifft eine erfolgreiche Antwort ein, ersetzt sie die angezeigten Cache-Daten und wird als neuer Cache gespeichert.
- Enthält der Cache keine noch zukünftige Abfahrt, zeigt die App keine überholten Einträge als aktuelle Abfahrten. Bis zur Antwort gilt der vorhandene Lade- oder Leerzustand.

## R3 – Datenalter und Zeitbezug

- Maßgeblich für die Anzeige einer einzelnen Cache-Abfahrt ist ihre Abfahrtszeit, nicht allein das Alter der Cache-Datei. Vergangene Abfahrten dürfen nie als bevorstehend angezeigt werden.
- Die App berücksichtigt Echtzeitzeiten, sofern diese bereits im bisherigen Abfahrtsmodell vorhanden sind; andernfalls verwendet sie die planmäßige Zeit.
- Die gespeicherte erfolgreiche Aktualisierungszeit dient ausschließlich dazu, die Herkunft als lokal zwischengespeichert einzuordnen und Fehlerfälle verständlich darzustellen. Sie darf den Nutzer nicht mit technischen Details überfrachten.
- Es wird kein beliebig alter Tagesverlauf vorgehalten. Daten ohne zukünftige Abfahrt sind beim nächsten Zugriff zu bereinigen.

## R4 – Fehler, Offline-Betrieb und konkurrierende Aktualisierung

- Schlägt die Hintergrundaktualisierung fehl oder besteht keine Netzverbindung, bleiben vorhandene, noch zukünftige Cache-Abfahrten sichtbar.
- Die App macht in diesem Fall knapp und verständlich kenntlich, dass keine Aktualisierung möglich war; die letzte lokale Anzeige bleibt nutzbar.
- Gibt es weder verwertbare Cache-Daten noch eine erfolgreiche aktuelle Antwort, zeigt die App den bestehenden verständlichen Fehler- oder Leerzustand.
- Eine ältere Hintergrundantwort darf keine Daten überschreiben, die bereits durch eine neuere erfolgreiche Aktualisierung derselben Haltestelle ersetzt wurden.

## R5 – Datenschutz und Umfang

- Der Zwischenspeicher bleibt auf dem Gerät und nutzt die bestehende lokale Speichermöglichkeit der App.
- Es werden keine zusätzlichen Standortdaten erfasst, übertragen oder langfristig gespeichert. Die bestehende Positionsabfrage und Provider-Kommunikation bleiben unverändert.
- Der Umfang beschränkt sich auf Abfahrtsdaten gespeicherter Favoriten. Suchen, nahegelegene Haltestellen, Verbindungen, Karten- und sonstige Entwurfsfunktionen erhalten durch diese Anforderung keine neue Persistenz.
- Es entstehen keine neuen Server, Konten, Synchronisierung oder Analyseübertragungen.

## Abnahme

1. Ein gespeicherter Favorit mit erfolgreich geladenen zukünftigen Abfahrten zeigt diese nach Beenden und erneutem Start sofort aus dem lokalen Speicher an.
2. Beim selben Start wird zusätzlich eine Anbieterabfrage ausgelöst; deren erfolgreiche Antwort ersetzt die zunächst sichtbaren Daten.
3. Abfahrten, deren relevante Abfahrtszeit beim Neustart bereits vergangen ist, erscheinen nicht.
4. Bei verzögerter oder fehlgeschlagener Anbieterabfrage bleiben gültige Cache-Abfahrten sichtbar und der Zustand ist verständlich erkennbar.
5. Bei fehlendem oder ausschließlich abgelaufenem Cache zeigt die App weder erfundene noch vergangene Abfahrten.
6. Das Entfernen eines Favoriten entfernt auch dessen Cache; ein erneutes Anlegen beginnt ohne alte Fremd- oder Restdaten.
7. Automatisierte Core-Tests prüfen Filterung, Zuordnung, Überschreiben und Fehlerfall. Der Windows-UI-Ablauf weist die sofort sichtbare Cache-Anzeige bei künstlich verzögerter Aktualisierung nach.

## Nicht-Ziele

- Kein Offline-Fahrplan für beliebige Haltestellen.
- Keine fachliche Änderung der Anbieterabfrage, des Echtzeitmodells oder der Sortierung von Abfahrten.
- Keine Garantie, dass lokale Daten ohne Netzverbindung vollständig oder aktuell sind.
