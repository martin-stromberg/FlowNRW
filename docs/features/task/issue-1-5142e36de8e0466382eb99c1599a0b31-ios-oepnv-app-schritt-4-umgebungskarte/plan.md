# Umsetzungsplan: Interaktive Haltestellenkarte und gelieferte Verläufe

## Übersicht

Eine eingebettete geografische Karte ergänzt die manuelle Haltestellensuche und Verbindungsdetails. Stationsmarker und zugängliche native Liste öffnen den vorhandenen Monitor; Strecken werden ausschließlich aus den tatsächlich gelieferten Geometrien der ausgewählten Verbindung dargestellt. Kein GPS, keine Favoriten, keine IIS-Arbeit.

## Designentscheidungen

| Bereich | Ansatz | Begründung |
|---|---|---|
| Kartenrenderer | MAUI `HybridWebView` mit lokal gebündeltem Leaflet 1.9.4 einschließlich CSS/Bildern/Lizenz, native Such-/Listen-/Statusbedienung. | Gemeinsame Lösung auf WebView2/Windows und WKWebView/iOS. MAUI `Map` unterstützt Windows nicht; kein weiteres NuGet erforderlich. |
| Geografische Orientierung | Konfigurierbare HTTPS-Rasterbasiskarte, standardmäßig OpenStreetMap, sichtbare Attribution. Zoomen, Verschieben, auf Auswahl einpassen. | Reale Straßen/Ortsnamen statt leerer Koordinatenfläche. Leaflet-Funktionen weiterverwenden. |
| Kacheln | Begrenzter nativer Tile-Gateway/Cache, nur aktuell sichtbare Kacheln; Renderer erhält Bilddaten über eng definierte Bridge. | App-User-Agent, HTTP-Cache und Fehler sind auf beiden Plattformen kontrollierbar. Direkte WebView-Abrufe nicht ohne Nachweis dieser Anforderungen freigeben. |
| Auswahl | Wiederverwendung `StopMonitorViewModel.Stops` und exakt derselben vollständigen `Address`-Instanzen. | Bestehende Identitätsprüfung bleibt erhalten; keine Monitorzuordnung allein nach Name oder Browserdaten. |
| Geometrie | Getrennte Segmente aus `JourneyLeg.Geometry`, ersatzweise dessen `Walking.Geometry`, ohne Verbindung nicht gelieferter Lücken. | Richtige Fahrt-/Linienzuordnung und ehrliche partielle Darstellung. |
| Datenschutz | Karten-/Suchsitzung nur im Speicher, ausschließlich begrenzter technischer Kachelcache auf Platte. | Keine Such-/Bewegungshistorie; Kartenanbieter erhält unvermeidbar IP und angeforderte Kachelregion, dokumentiert. |

Offizielle Quellen, geprüft am 17.09.2026: [MAUI Map](https://github.com/dotnet/docs-maui/blob/main/docs/user-interface/controls/map.md), [MAUI WebView](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/webview?view=net-maui-10.0), [Leaflet stabile Version/Assets](https://leafletjs.com/download.html), [Leaflet API](https://leafletjs.com/reference), [OSM Tile Policy](https://operations.osmfoundation.org/policies/tiles/). OSM erfordert Attribution, identifizierenden User-Agent und Cache; automatisierte Pan-/Zoom-Tests verwenden ausschließlich lokale Fixtures, keine öffentliche Kachellast.

## Programmabläufe

### Haltestellen und manuelle Umgebung

1. Benutzer sucht mit bestehendem beschriftetem Feld nach Haltestellenname/Ort und erhält native Klartexttreffer.
2. Neue Aktion `Auf Karte zeigen` übergibt aktuellen Kandidatensatz und Metadaten an `MapViewModel`; gültige Koordinaten werden eingepasst. Kein Standortzugriff.
3. Karte zeigt Marker an tatsächlichen Koordinaten, bietet Zoom, Verschieben und Rücksetzen auf Auswahl. Native Liste enthält alle identifizierbaren Stops, auch ohne Koordinate (Hinweis „Keine Kartenposition vorhanden“).
4. Marker bzw. Listenbutton identifiziert einen aktuellen Sitzungsindex. `MapViewModel` löst diesen ausschließlich zum gespeicherten Originalkandidaten auf und ruft `StopMonitorViewModel.OpenAsync` auf. Zurück führt zur erhaltenen Karten-/Suchauswahl.
5. Neue Suche ersetzt Sitzung und Revision; alte Marker-/Kachelnachrichten werden ignoriert. Leer-/Fehlersuche bleibt über bestehende Suche erklärbar, Kartenaktion ohne verwertbare Treffer verständlich deaktiviert.

### Verbindungsverlauf

1. `JourneyDetailPage` bietet `Verlauf auf Karte`; `JourneyDetailViewModel.Session` liefert ausgewählte Verbindung und Quellenmetadaten.
2. `MapViewModel` erstellt Linien-/Fußwegsegmente mit Klartextlabel und originaler Reihenfolge. Nur vorhandene gültige Punktfolgen mit mindestens zwei Punkten zeichnen; ungültige Punkte erzeugen Unterbrechung, keine künstliche Brücke.
3. Ankunfts-/Abfahrtsstationen mit gelieferten Koordinaten dienen der Orientierung; kein Ersatz der Strecke durch gerade Start-Ziel-Linien. Fehlende/partielle Geometrie ausdrücklich melden. Segmentlegende und Texte bleiben auch ohne Basiskarte nutzbar.
4. Andere Verbindung auswählen erzeugt neuen Snapshot ohne alte Geometrie. Rücknavigation erhält Detail-/Suchzustand.

### Kacheln und Fehler

1. Lokale Assets laden ohne Netz; native Statusanzeige meldet Ladezustand und begrenzten Timeout. Nur sichtbare Kacheln der aktuellen Revision werden angefragt.
2. `MapTileService` validiert Provider/Zoom/X/Y, nutzt HTTPS ohne unsichere Redirects, stabilen `FlowNRW/<Version>`-User-Agent und begrenzte parallele Abrufe. Keine automatischen Wiederholungsschleifen.
3. Cache berücksichtigt Cache-Control, Expires, ETag/Last-Modified und bedingte Anfragen; ohne auswertbare Header mindestens sieben Tage. Bytebegrenzter technischer Cache, keine Offline-Downloadfunktion/Vorabrufe. Ein dedizierter Cache hält wiederholte Ansichten ohne unnötige Downloads nutzbar.
4. Offline/Timeout/Anbieterfehler melden fehlende Basiskarte explizit; bestehende gültige Marker, Geometrie und native Liste bleiben bedienbar. Veraltete Fachdaten haben weiter Quelle/Datenstand. Ein manueller Wiederholungsbutton ermöglicht Erholung ohne Neu-Suche.
5. Wegnavigation bricht Abrufe ab, trennt Handler und ignoriert späte Nachrichten. Native Such-/Routing-/Monitorabläufe bleiben verwendbar, auch wenn WebView initial nicht verfügbar ist.

## Neue Klassen

| Klasse/Datei | Typ | Zweck |
|---|---|---|
| `MapViewModel` | Core-ViewModel | Kartensitzung, Kandidatenidentität, Geometriesnapshot, Status/Quelle und Auswahl. |
| `MapPresentation` | Core-Projektion | Validierte Marker/Segmente und testbare Darstellungsdaten ohne Plattformabhängigkeit. |
| `IMapNavigation` / `ShellMapNavigation` | Navigation | Kartenroute mit vorbereiteter Sitzung. |
| `MapOptions` | Konfiguration | HTTPS-Kacheltemplate, Attribution, Zoom-/Cache-/Netzgrenzen. |
| `MapTileService` | Gateway | Validierte abbrechbare Downloads, begrenzter HTTP-Cache und Status. |
| `MapPage` | MAUI-Page | HybridWebView, native Liste/Status/Aktionen und sichere Bridge. |
| `Resources/Raw/map/` | Assets | Lokal gebündeltes Leaflet mit Lizenz und schmaler eigener Render-/Bridge-Datei. |

## Änderungen an bestehenden Klassen

- `StopSearchPage`: Kartenaktion, aktuelle Stops und Herkunft an Sitzung übergeben; bestehende Trefferbuttons unverändert nutzbar.
- `JourneyDetailPage`/`JourneyDetailViewModel`: Kartenaktion für ausgewählte Verbindung samt Metadaten; kein spekulativer Providerabruf.
- `AppShell`, `MauiProgram`: Route und DI, getrennte Sitzung und Gateway. Plattform-Handler nur soweit zum tatsächlichen WebView-Betrieb erforderlich.
- `UiTestFixtureServices` und native Testskripte: definierte Koordinaten, Linien-/Fußweggeometrie sowie Fehlerfälle; öffentlicher Tileanbieter wird im UiTest-Build durch lokale Kachelfixtures ersetzt.
- Bestehende Fachmodelle und Monitoridentitätsregeln nicht erweitern, solange Kandidatenwiederverwendung ausreicht.

## Datenbankmigrationen

Keine.

## Validierungsregeln

| Objekt | Regel / Reaktion |
|---|---|
| Koordinaten | Endliche WGS84-Werte innerhalb gültiger Grenzen; Web-Mercator-Darstellung behandelt Polbereich explizit. Ungültige Koordinaten nicht als Nullpunkt zeichnen. |
| Geometrie | Nur gelieferte Punkte derselben Teilstrecke; Lücken bleiben sichtbar. Keine Vermischung unterschiedlicher Verbindungen. |
| Bridge | Erlaubte Nachrichtentypen und aktuelle Sitzungskennung; Stop-ID ist nur allowlisted lokaler Index. Keine frei übergebenen URLs, Dateipfade oder JS-Ausdrücke. Grenzen für Nachrichtengröße und Kachelanfragen. |
| Providertexte | JSON-Serialisierung und DOM `textContent`; niemals ungeprüftes HTML/Script. |
| WebView | Navigation auf lokale Karte begrenzen; externe Links nur explizit erlaubte Attribution, ohne Ausführung unbekannter Schemes. |
| Tile-Optionen | Nur absolute HTTPS-Templates mit genau z/x/y; keine eingebetteten Credentials, ungültige Zoom-/Cachewerte zurückweisen. Geheimnisse nie im Log. |

## Konfigurationsänderungen

| Eintrag | Standard | Zweck |
|---|---|---|
| `Map:TileUrl` | `https://tile.openstreetmap.org/{z}/{x}/{y}.png` | Austauschbarer Basiskartenanbieter. |
| `Map:Attribution` | `© OpenStreetMap contributors` samt Lizenzlink | Immer sichtbare Herkunft. |
| `Map:MinZoom` / `MaxZoom` | 1 / 18 | Anbieterkompatible Abrufgrenzen. |
| `Map:CacheMaxBytes` | 67108864 (64 MiB) | Begrenzter technischer Cache. |
| `Map:Timeout` / `MaxConcurrent` | 10 Sekunden / 4 | Begrenzte Netzlast und verständlicher Fehlerabschluss. |

Cachegrenzen begründen, kein SLA für öffentliche Kacheln versprechen; produktive Anbieterwahl bleibt austauschbar. iOS benötigt keine zusätzliche Standortberechtigung für diese manuellen Karten.

## Seiteneffekte und Risiken

- WebView und native Liste dürfen Navigation/Scrollen bei schmalem Fenster und Textskalierung nicht blockieren. Feste nutzbare Kartenhöhe, erreichbare Buttons und sichtbare Attribution prüfen.
- OSM-Cache-/UA-Anforderungen müssen tatsächlich auf Windows und im gemeinsamen iOS-Pfad implementiert sein. Plattformverhalten nicht aus Browserstandard nur vermuten.
- Der bestehende Monitor prüft Kandidatenmitgliedschaft; veraltete Kartenauswahl muss wirkungslos bleiben. Rücknavigation darf die Suchtreffer nicht löschen.
- WebView-Initialisierung und Bridge sind asynchron; Revision/Abbruch und Fehleranzeige schützen gegen späte Antworten.

## Umsetzungsreihenfolge

1. **Kartenprojektion und Sitzung:** vorhandene Modelle/Commands vorausgesetzt; Identität, Geometrie und Fehlerzustände samt Core-Tests erstellen.
2. **Assets und Tile-Gateway:** vor WebView erforderlich; Leaflet 1.9.4 offiziell beziehen, Lizenz/Prüfsummen ablegen, Optionen/Cache/Downloadtests implementieren. Keine Paketinstallation notwendig.
3. **Kartenpage und sichere Bridge:** setzt 1–2 voraus; echte Basiskarte, Marker, Segmente, Zoom/Pan, Attribution, native Liste und Fehlerwiederholung implementieren.
4. **Navigation integrieren:** setzt 3 voraus; Suche/Details/Shell/DI verbinden, ursprüngliche Abläufe erhalten.
5. **Windows-Abnahme:** setzt kompilierbare UiTest- und Release-App voraus; lokale Tilefixtures und sämtliche unten genannten E2E ausführen, separate menschlich ausgelöste Live-Kartenprobe dokumentieren.
6. **Reviews und Dokumentation:** erfüllten Plan, Usability und Code unabhängig prüfen; `docs/help/karte/`, README, Release Notes und iOS-Checkliste aktualisieren. Format/XML/Build/Coverage abschließen.

## Tests

### Neue Tests

| Testgruppe | Nachweis |
|---|---|
| `MapPresentationTests` | Exakte Koordinaten/Segmentreihenfolge, Fußweg, fehlende/partielle/ungültige Geometrie, keine künstlichen Brücken, Verbindungswechsel. |
| `MapViewModelTests` | Marker und Liste gleicher Originalstop, ohne Koordinate nur Liste, doppelte Namen/verschiedene IDs, unbekannte/stale Bridge-ID abgewiesen, Auswahl/Rückkehr. |
| `MapTileServiceTests` | HTTPS/Template/Bounds, User-Agent, Cachehit/Expiry/Conditional-304, Speichergrenze, Timeout/Abbruch/Offline und überholte Antworten; nur Fixture-HTTP. |
| `MapBridgeTests` | Escaping von HTML-/Scripttext, ungültige Nachrichten, Revision und Allowlist; kein beliebiger URL-/JS-Aufruf. |

### Betroffene bestehende Tests

Core-Monitor-/Journey-Tests bleiben gültig; native Routing- und Monitortests nach Navigationserweiterung als Regression ausführen. UiTest-Fixtures erweitern ohne Änderung der vorhandenen Szenariobedeutung.

### E2E-Tests (primärer Funktionsnachweis)

Alle Pflichtszenarien im nativen Windows-Appfenster (`WindowsJourneyUiTests.ps1` oder ergänzendem `WindowsMapUiTests.ps1`), UIA und für eingebettete Karte erforderlichenfalls echte Maus/Tastatur. DOM/VM-Aufrufe allein ersetzen keine Markerbedienung. Tilefixtures eindeutig als Testdaten kennzeichnen.

| Priorität | Szenario | AK / Erwartung |
|---|---|---|
| Pflicht | Suche Essen → Karte; Marker tatsächlich sichtbar; Zoom/Pan/Rücksetzen per UI | AK1/3/4: geografische Basiskarte, beschriftete Stationen an richtigen Koordinaten. |
| Pflicht | Zweiten gleichnamigen Marker auswählen → Monitor → zurück | AK1/4: richtige vollständige Stop-ID und Karten-/Suchkontext erhalten. |
| Pflicht | Native Liste → selben Monitor; Stop ohne Koordinate | AK1/4: identische Zuordnung, fehlende Position explizit, Liste bleibt nutzbar. |
| Pflicht | Suche leer/fehlerhaft, keine gültigen Koordinaten, neue Suche nach alter Karte | AK1/3/4: klare Meldungen, keine alten Marker/Monitore. |
| Pflicht | Verbindung → Details → Verlauf; andere Verbindung; teilweise/keine Geometrie | AK2/4: richtige Linien-/Fußwegsegmente, keine Verbindungsvermischung/erfundene Linien, Rücknavigation. |
| Pflicht | Kachel langsam/fehlgeschlagen/offline → Liste/Monitor → Wiederholen | AK3/4: Fehler und Ladeende sichtbar, alte Fachdaten erklärt, Bedienung bleibt möglich. |
| Pflicht | Schmale Fensterbreite, Textskalierung, Tastaturfokus und Attribution | AK1/2/3: erreichbare Alternativliste und lesbare Herkunft/Status. |
| Pflicht | Bestehende native Routing- und Monitorflows | AK4: keine Regression. |
| Pflicht | Release-App: reale manuelle Stopsuche/Karte und Kartenauswahl | AK1/2/3: echte geografische Orientierung/Attribution, reale Tileantwort; Fixture und Live klar getrennt. |

Screenshots eigener App visuell prüfen. Nicht ausführbare native Flüsse nach ernsthaftem Versuch mit konkreter Ursache protokollieren; nie als bestanden markieren. Core-Suite mit mindestens 70 % Zeilenabdeckung, Format, XML-Dokumentation und Windows Release/UiTest-Build mit Warnungen als Fehler. Windows-GitHub-Actions unverändert funktionsfähig halten. iOS-Build und native Bedienung beim Nutzer; manuelle Anleitung für Zoom/Pan, Marker/Liste, Details, Offline und Rücknavigation übergeben.

## Offene Punkte

Keine.

Technische Präzisierung vor Implementierung: HybridWebView mit RawMessageReceived/SendRawMessage und fester JSON-Antwortfunktion vermeidet URL-Navigationsrennen und reflektionsbasierte .NET-Aufrufe. Lokaler CSP blockiert externe Webressourcen; Kartenkacheln kommen ausschließlich aus dem nativen Gateway. Windows-WebView-Datenordner liegt unter AppData. Gemeinsamer iOS-Pfad benötigt keine ATS-Ausnahme. Quelle: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-10.0

