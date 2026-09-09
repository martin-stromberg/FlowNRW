# Liveproben der Fahrplanadapter – 8. September 2026

Abrufzeit: ca. 10:21–10:22 UTC (12:21–12:22 Europe/Berlin). Die Dateien in `fixtures/` sind tatsächlich abgerufene HTTPS-Antworten, keine erzeugten Live-Daten. Anfragen betreffen ausschließlich öffentliche Bahnhöfe und Koordinaten; es wurden keine Zugangsdaten verwendet. Deterministische Tests lesen diese gespeicherten Antworten und stellen selbst keine Netzverbindung her.

## Quellen und Zugangsgrenzen

- [Offizielle VRR-OpenService-Dokumentation](https://www.opendata-oepnv.de/ht/de/api): registrierungsfreier Testzugang, RapidJSON empfohlen; produktive öffentliche Anwendungen benötigen registrierten Produktivzugang. Der Testserver kann laut Betreiber zeitweise unplausible Daten liefern. Eine einzelne bundesweite Verbindung belegt keine flächendeckende Abdeckung.
- [db.transport.rest v6](https://v6.db.transport.rest/api.html): Locations, Nearby, Journeys und Departures; dokumentiertes Limit 100 Anfragen/Minute, Burst 200. Beim aktuellen Suchabruf kam HTTP 503; dieser Adapter ist damit gegen dokumentiertes JSON deterministisch geprüft, aber aktuell nicht als live verfügbar bestätigt.

## Nachweise

| Probe | HTTP | Beobachtung | Gespeicherte Antwort |
|---|---|---|---|
| EFA Suche Berlin Hbf | 200 | Mehrere Berliner Bahnhofstreffer, DHID, WGS84. Systemmeldung -8011 weist auf Mehrdeutigkeit hin; Auswahl bleibt nötig. | `efa-search-live.json` |
| EFA Nähe Gelsenkirchen 51.505/7.096 | 200 | Haltestelle Wiehagen, 478 Meter, 51.503014/7.098874; weitere Stopps geliefert. | `efa-nearby-live.json` |
| EFA NRW-Abfahrten Gelsenkirchen Hbf | 200 | Linien, Betreiber, Fahrtcodes, Sollzeit, teilweise Istzeit, Steig/Gleis und Stations-DHID de:05513:5613. Nicht jede Fahrt liefert Echtzeit. | `efa-departures-live.json` |
| EFA Berlin Hbf → Hamburg Hbf-Koordinate | 200 | ICE 600, DB Fernverkehr AG, Berlin de:11000:900003200 → Hamburg de:02000:10950; erste Fahrt 11:37–13:24 UTC mit umfangreicher Geometrie. Brokerwarnung -10015 wird als Warnung weitergegeben. | `efa-journeys-live.json` |
| db-rest Suche Berlin | 503 | Service Unavailable. Keine Suchtreffer vorgetäuscht. | Fehlerstatus hier dokumentiert |

Der EFA-Server lieferte bei angefragten 14:00 Uhr Ortszeit auch eine frühere Fahrt (13:37 Uhr); die fachliche Serviceschicht muss kommende Ergebnisse gegen die Anfragezeit filtern. Suchanfragen mit bloßen Bahnhofsnamen ergaben im Tripendpoint zunächst „multiple matches“ ohne Journey. Erfolgreich war der mit der Suchantwort aufgelöste Berliner DHID und die Hamburger Zielkoordinate. Die Adapter geben mehrdeutige Namen als `unresolved_location` zurück, statt den ersten Treffer beliebig zu wählen.

## Reproduzierbare Endpunkte

Basis für EFA: `https://openservice-test.vrr.de/openservice/`. Allen vier Anfragen wurden `outputFormat=rapidJSON&version=10.4.18.18&coordOutputFormat=WGS84[DD.ddddd]` hinzugefügt. Parameter sind in einer tatsächlichen URL pro Wert URL-kodiert.

- Suche: `XML_STOPFINDER_REQUEST?name_sf=Berlin%20Hbf&type_sf=any&doNotSearchForStops_sf=0`
- Nähe: `XML_COORD_REQUEST?coord=7.096%3A51.505%3AWGS84%5BDD.ddddd%5D&coordListOutputFormat=STRING&max=5&inclFilter=1&radius_1=1000&type_1=STOP`
- Abfahrt: `XML_DM_REQUEST?place_dm=Gelsenkirchen&type_dm=stop&name_dm=Hbf&mode=direct&limit=10`
- Verbindung: `XML_TRIP_REQUEST2?type_origin=stop&name_origin=de:11000:900003200&type_destination=coord&name_destination=10.0069:53.5527:WGS84[DD.ddddd]&itdDate=20260908&itdTime=1400&calcNumberOfTrips=2`
- db-rest: `https://v6.db.transport.rest/locations?query=Berlin&results=2`

Datum und Uhrzeit müssen für erneute Proben an den aktuellen Fahrplan angepasst werden. Ohne explizites `coordOutputFormat=WGS84[DD.ddddd]` liefert der getestete EFA-Endpoint projizierte Zahlen (z.B. 5288900/790614); diese werden niemals als WGS84 interpretiert.

## Normalisierung und technische Grenzen

EFA-Plattformen werden über ihren Stop-Elternknoten einer Station zugeordnet; die Stations-DHID ist keine gemeinsame Fahrtkennung. Ein interner EFA-Fahrtcode wird nur zusammen mit der Anbieter-Linien-ID verwendet. Providerübergreifende Stop-IDs werden nicht blind verwendet: Routing nutzt vorhandene Koordinaten, andernfalls eindeutige Ortssuche. Abfahrten lösen fremde Stopps durch eindeutige Namenssuche auf.

Fehlende Soll-/Istzeiten und Ausfallwerte bleiben unbekannt; Zeitstrings ohne Offset werden nicht unter Annahme der Maschinenzeitzone normalisiert. EFA-Abfragen konvertieren das Abfrageinstant explizit nach Europe/Berlin. EFA-Koordinaten sind lat/lon, db-rest-GeoJSON ist lon/lat. Fußwege, Umstiege, Betreiber und gelieferte Geometriepunkte bleiben erhalten.

`EfaFallbackBaseUrl` wird bei leerem oder fehlgeschlagenem Primärabruf tatsächlich aufgerufen und mit `IsFallback`/`efa_endpoint_fallback` gekennzeichnet. Ein alternativer EFA-Server muss dieselbe RapidJSON-Vertragsform und die übergebenen Stopkennungen unterstützen; der konkrete bundesweite Ersatzserver benötigt eine gesonderte Abdeckungs-/Zugangsprüfung. Antwortobjekte werden auf `MaxResults` begrenzt. HTTP-Zeitlimit, Wiederholung, sichere Diagnose und Cache liegen in den separaten Gateway-/Servicekomponenten.

## Ergänzende echte Fachserviceprobe

Die vollständige Abendprobe vom8.September18:10–18:12UTC ist in [service-live-2026-09-08.jsonl](service-live-2026-09-08.jsonl) gesichert und führt den implementierten Gateway, beide Adapter, Orchestrator und Fachservices aus:5Suchtreffer,5naheHaltestellen,5NRW-Abfahrten und2kommende bundesweite Verbindungen. Einzelheiten und reproduzierbarer Konsolenquelltext sind im [Implementierungsnachweis](verification/implementation-checks.md) verlinkt. Bei dieser Probe war db-rest durch Timeout begrenzt; EFA-Fallback und Warnungen bleiben ausdrücklich sichtbar. Die Routingserviceprobe bestätigt den Filter früherer Fahrten anhand der angefragten Zeit.
