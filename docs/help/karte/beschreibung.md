# Haltestellen und Verläufe auf der Karte

In **Abfahrten** eine Haltestelle oder einen Ort suchen und **Haltestellen auf Karte zeigen** wählen. Die Marker stehen an den gelieferten Koordinaten. Ein Marker öffnet den zugehörigen Abfahrtsmonitor. Über **Haltestellenliste anzeigen** ist dieselbe Auswahl mit nativen, per Tastatur bedienbaren Buttons möglich. Haltestellen ohne Kartenposition bleiben in dieser Liste auswählbar.

Mit den Plus-/Minusbuttons zoomen und die Karte verschieben. **Karte erneut laden / auf Auswahl zentrieren** lädt die Basiskarte erneut und stellt den ursprünglichen Ausschnitt her. Zurück erhält die Haltestellensuche. Eine neue Suche ersetzt die vorherigen Kandidaten.

In Verbindungsdetails öffnet **Verlauf auf Karte** ausschließlich die gelieferten Linien-/Fußweggeometrien dieser Verbindung. Teilstrecken bleiben getrennt. Fehlende oder nur teilweise gelieferte Verläufe werden ausdrücklich erklärt; gerade Ersatzlinien werden nicht erfunden.

Bei Kartenfehlern bleiben Haltestellenliste und vorhandene Verläufe bedienbar. Quelle, Fachdatenstand und Kartenfehler sind getrennt angezeigt. Veraltete Kacheln werden nur verwendet, wenn die Cachevorgaben des Anbieters das erlauben. Ohne Positionsfreigabe funktioniert die manuelle Karte; GPS folgt in einem späteren Schritt.

## Konfiguration und Datenschutz

Die Einstellungen werden wie die bestehenden Transitoptionen über `builder.Configuration` gelesen. Standard ist `https://tile.openstreetmap.org/{z}/{x}/{y}.png`; weitere Anbieter müssen passende PNG-Rasterkacheln über HTTPS und eine eigene korrekte Attribution anbieten. Es gibt keine Eingabe von Zugangsdaten in der Kartenoberfläche.

| Einstellung | Standard |
|---|---|
| `Map:TileUrl` | OSM-Template oben |
| `Map:Attribution` | © OpenStreetMap contributors · openstreetmap.org/copyright |
| `Map:MinZoom` / `Map:MaxZoom` | 1 / 18 |
| `Map:CacheMaxBytes` | 67108864 (64 MiB) |
| `Map:Timeout` | 00:00:10 |

Nur sichtbare Kacheln werden angefordert. Die Browserseite hält höchstens vier Abrufe ausstehend; der native Gateway serialisiert Cache/Netzzugriffe konservativ. Wegnavigation, Neuladen und das Verlassen eines Kachelausschnitts brechen überholte Abrufe ab. Keine Vorabrufe, kein Offline-Gebietsdownload, keine automatischen Retries.

Der technische Kachelcache liegt unter dem MAUI-Cacheverzeichnis in `MapTiles`, ist größenbegrenzt und respektiert Cache-Control, Expires, ETag und Last-Modified. Ohne Frischeangaben gelten sieben Tage. Suchtexte, gewählte Stationen, Verbindungen und Bewegungshistorien werden dort nicht gespeichert. Der Kartenanbieter erhält IP-Adresse und angeforderte Kachelregion. Windows-WebView-Daten liegen im beschreibbaren AppData-Unterordner `MapWebView`.

Leaflet 1.9.4 ist lokal mit BSD-Lizenz eingebunden; kein CDN-Abruf beim Appstart. Die Basiskarte stammt standardmäßig von OpenStreetMap. [Attribution/Lizenz](https://www.openstreetmap.org/copyright), [Kachelnutzungsregeln](https://operations.osmfoundation.org/policies/tiles/). Der native Client identifiziert sich als FlowNRW; öffentliche Serververfügbarkeit wird nicht garantiert.

## Technischer Aufbau

`MapViewModel`, `MapStation` und `MapSegment` projizieren Originalkandidaten und getrennte Providergeometrien. Sitzungskennung und Index werden gegen die aktuellen Kandidaten validiert. `MapPage` verwendet den vorhandenen MAUI-HybridWebView, native Listen und eine begrenzte JSON-Kommunikation. Kachelanfragen enthalten ausschließlich z/x/y, keine frei ausführbaren URLs oder Methoden. Providertexte werden als JSON und DOM-Text übertragen; CSP verhindert externe Webressourcen. Der Tilecache verwendet generierte JSON-Metadaten für den gemeinsamen iOS-Codepfad.

## iOS-Prüfliste

Native iOS-Abnahme bleibt beim Nutzer. Auf dem Gerät bitte manuelle Suche → Karte → Marker/Listenwahl → Monitor → Zurück sowie Details → Verlauf prüfen. Zusätzlich Pinch-Zoom/Verschieben, VoiceOver, große Systemschrift, Offlinezustand und erneutes Laden prüfen. Keine neue Standortberechtigung und keine ATS-Ausnahme erforderlich. Windows-CI bleibt erhalten; weder iOS-CI noch IIS oder Deployment wurden ergänzt.

[Windows-/Core-Prüfnachweise](verification/checks-2026-09-17.md)
