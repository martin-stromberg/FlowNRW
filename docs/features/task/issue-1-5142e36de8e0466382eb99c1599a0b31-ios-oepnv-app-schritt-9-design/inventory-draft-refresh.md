# Bestandsaufnahme – aktualisierter Stitch-Entwurf

Stand: 03.10.2026
Quelle: `stitch_nrw_transit_ios_app.zip` im Projektstamm. Die neun enthaltenen PNGs sind echte Referenzbilder (382–657 px breit, 619–1600 px hoch; 225–646 KB). Sie ersetzen die früheren, unbrauchbaren 28-Byte-Platzhalter aus `design-draft.zip` für die visuelle Arbeit. Die begleitenden HTML- und `DESIGN.md`-Dateien sind Referenzmaterial, keine auszuliefernde Web-Oberfläche.

## Referenznachweis für den Lifecycle

| Nachweis | Ergebnis |
|---|---|
| Referenzartefakt | `stitch_nrw_transit_ios_app.zip` im Projektstamm, 3.686.573 Byte |
| Integrität | SHA-256 `B4046B3F733FB911F316247627470C08AA8448BA3B8A6B97C2CD6960E612D044` |
| Inhalt | 9 Bildreferenzen, 8 HTML-Referenzen, 2 Design-System-Beschreibungen |
| Umgang | Das Archiv und daraus abgeleitete PNG-/HTML-Dateien sind keine Produktassets und werden in diesem Umsetzungsschritt nicht in das Repository oder die App übernommen. Diese Bestandsaufnahme hält ausschließlich Herkunft, Umfang und die verbindliche Verwendung als Designreferenz fest. |

## Verwertbare Referenzoberflächen

| Referenz | Helles und dunkles Bild | Übernehmbare Gestaltung für vorhandene Funktionen |
|---|---|---|
| `abfahrtsmonitor_live` | ja | Abfahrten als großzügige, abgerundete Stationskarte mit kompakten Abfahrtszeilen: Linienbadge, Ziel, tabellarische Zeit, Pünktlichkeits-/Verspätungsstatus und Gleis in einer zweiten Zeile. Standort, Synchronisationsalter und Intervall erscheinen als dezente, zugeordnete Information statt Diagnoseabsatz. Die zweite Station zeigt die gewünschte kompakte „nahebei“-Hierarchie. |
| `verbindungssuche` | ja | Große Überschrift, eine zusammenhängende Eingabekarte für Start und Ziel, Abfahrt/Ankunft als Segmentauswahl, Zeitwahl als kompakte Zeile, primäre Suche als klarer CTA. Verbindungsergebnisse sind eigenständige Karten mit Zeiten, Dauer, Umstiegen, Linienbadges und Details-Aktion. |
| `fahrtbegleiter_detail` | ja | Für die bereits implementierte Verbindungsdetailseite: Zusammenfassung oben, zeitlich lesbare Kopfzeile, Echtzeithinweis und vertikale Haltestellen-/Abschnittstimeline. Verwendbar sind Kartenhierarchie, Linienfarben und die klare Trennung von Soll/Ist-Zeiten. |
| `umgebungskarte_stationen` | ja | Suchleiste über einer großflächigen Karte, sehr zurückhaltende Kartenaktionen, marker-/linienfarbene Stationen und ein angeheftetes unteres Stationspanel. Das Panel bündelt Stationsname, Entfernung, Abfahrten, Favorit und eine „Route planen“-Aktion. Die vorhandene native Listenalternative und Kartenattribution bleiben erhalten. |
| `flownrw_icon.png` | n/a | App-Marke für Kopfbereich und App-Icon. Vor Verwendung als Produktasset ist die Lizenz-/Herkunft gegenüber dem gelieferten Artefakt zu wahren; vorhandenes MAUI-App-Icon nicht blind überschreiben. |

## Dunkelmodus-Signale

Die Dunkelbilder sind eine vollwertige zweite Darstellung, keine bloße Invertierung:

- Canvas ist nahezu schwarz; primäre Karten sind `#1C1C1E`, verschachtelte Flächen etwa `#2C2C2E`.
- Texte bleiben weiß bzw. hellgrau, deaktivierte Metadaten gedämpft; blaue Interaktionen und Linienfarben sind leuchtender und kontrastreich.
- Status bleibt semantisch: grün für pünktlich/live, gelb/amber für Hinweise und mittlere Verspätungen, rot für Ausfall oder starke Störung. Jeder Status benötigt weiterhin Text.
- Kopf- und Tabbar wirken als ruhige, abgesetzte iOS-Chrome-Flächen. Karten erhalten feine Konturen statt sichtbarer schwerer Schatten.
- Kartenpanel auf der Karte ist dunkel und hebt sich dennoch klar vom Kartenuntergrund ab; Marker, aktuelle Auswahl und Routenlinie bleiben sichtbar.

Diese Cues bestätigen und konkretisieren `docs/design/acceptance.md`: Systemschrift, 16px Telefongutter, 4/8/12/16/24-Rhythmus, Kartenradius 16–20, Felder 10, Buttonradius 12, Linienbadges 6–8 und mindestens 44×44 logische Einheiten für Interaktionen.

## Elemente außerhalb des vereinbarten Umfangs

Die folgenden Elemente aus dem Entwurf dürfen keine scheinbare Funktion erhalten und werden ausgelassen. Sie sind in `issue.md` nicht gefordert und in `docs/design/acceptance.md` ausdrücklich ausgeschlossen oder durch vorhandene Funktionen ersetzt.

| Entwurfselement | Behandlung |
|---|---|
| eigener Tab „Fahrtbegleiter“, aktive Fahrt, GPS-Live-Sync und Fahrzeugfortschritt | Kein zusätzlicher Tab und keine aktive Fahrtverfolgung. Die Timeline bleibt Teil der Verbindungsdetails mit tatsächlich gelieferten Daten. |
| Tickets, Deutschlandticket-Gültigkeitszusage, Tarif-/Ticketprodukt | Auslassen. |
| Wagenreihung, Klassen, Komfort, Auslastung | Auslassen, weil die Providerdaten dies nicht verlässlich liefern. |
| Umstiegsalarm, Live-Update-/Benachrichtigungsbutton, Push | Auslassen. |
| Fahrt teilen, Profil/Avatar und Schnellfilter ohne vorhandene Funktion | Auslassen. |
| Mikrofon, QR-Scanner, 3D-Karte, Kartenlayer und Fernverkehr-/Barrierefrei-/Leihradfilter | Auslassen. Bestehende Suche, Standortaktion und Listen/Kartenumschaltung bleiben klar beschriftet. |
| Stationsausstattung, Bahnhofszugänge, Nextbike/Sharing, Fußweg-Navigation | Auslassen, weil keine verlässliche Datenquelle oder Funktion vorliegt. |
| CO₂-Angaben, „schnellste“ Prädikate, zusätzliche Verkehrsmodi/Fahrradfilter und erfundene Pünktlichkeits-/Streckenmonitorwerte | Auslassen, sofern nicht aus dem gelieferten Routing-Ergebnis ableitbar. |

## Konkrete Umsetzungsziele

1. **Nearby-Drilldown reparieren.** In `HomePage.RenderNearby()` wird ein `FavoriteHomeViewModel.NearbyStops`-Kandidat an `StopMonitorViewModel.OpenAsync()` übergeben. Diese Methode akzeptiert derzeit jedoch nur Einträge, die per Referenz in `StopMonitorViewModel.Stops` liegen; die Home-Nearby-Liste ist eine andere Liste. Der Tipp endet daher ohne Navigation. Der Monitor braucht einen expliziten, identitätsvalidierten Öffnungspfad für einen vollständigen Nearby-Kandidaten, der die vorhandene Stop-ID/Quelle erhält, zum Monitor navigiert und danach Abfahrten lädt. Der native E2E-Test muss den Tipp auf `NearbyStop0` bis zum sichtbaren Einzelmonitor abdecken.
2. **Gemeinsames App-Chrome schaffen.** Testmodus-artige Textschaltflächen wie „Verbindung suchen“, „Haltestelle hinzufügen“, „Aktualisierung einstellen“, „Entfernungen aktualisieren“ und „Favoriten auf Karte zeigen“ werden zu einer klaren, priorisierten Kopf-/Kartenhierarchie. Fachliche Aktionen bleiben zugänglich und automatisierbar, erscheinen aber als beschriftete Symbolaktion, primärer CTA oder Sekundäraktion nach dem Entwurf.
3. **Home und Monitor neu komponieren.** Favoriten und nahe Haltestellen werden als Stationskarten dargestellt. Abfahrten folgen der Zeilenstruktur des Drafts; Quellen, Aktualität und Fehlerstatus sind kurz, lesbar und beim betreffenden Ergebnis verortet. Nahe Haltestellen sind keine bloße Buttonliste, sondern kompakte anwählbare Karten mit Entfernung, falls geliefert.
4. **Verbindungssuche und Ergebnislisten verdichten.** Start/Ziel, Favoritvorschläge, Zeit und Ankunftsmodus werden in einer Eingabekarte mit visueller Gruppierung belassen. Ergebnis- und Detailkarten übernehmen die klare Zeit-/Dauer-/Umstiegs-Hierarchie, ohne nicht gelieferte Besetzungs-, CO₂- oder Tarifwerte zu erfinden.
5. **Haltestellen/Karte als Karte zuerst.** Suchleiste und Standortaktion liegen über der großflächigen Karte. Die Auswahl wird als zugängliches, unteres Stationspanel mit echten Monitor-/Favorit-/Routingaktionen dargestellt. Der derzeitige Listenmodus bleibt als echte Alternative erhalten.
6. **Dunkelmodus sichtbar abnehmen.** Alle neu gestalteten gemeinsamen Komponenten müssen die dunklen Oberflächen, Texte, Konturen, Statusfarben und Auswahlzustände des neuen Drafts verwenden. Neben dem hellen Bild sind mindestens Home/Monitor, Suche/Ergebnisse, Detail und Karte/Panel im dunklen Windows-Matrixlauf zu vergleichen.
7. **Keine Test-Steuerelemente im Produktbild.** Fixture-Steuerungen bleiben nur für automatisierte Szenarien verfügbar und werden bei regulären Builds sowie Abnahmescreens ausgeblendet. AutomationIds bleiben stabil, damit der Umbau die vorhandenen nativen Tests nicht entwertet.

## Abnahmerisiken und Nachweise

- Vorhandene Bilder aus `docs/help/design/verification` sind nicht länger als alleinige visuelle Referenz ausreichend. Die neue Matrix muss jeden Screenshot dem gleichnamigen neuen PNG in Hell oder Dunkel zuordnen.
- Die UI darf den Draft nicht als statische Kopie nachbauen: echte Fehler-, Leer-, Offline-, fehlende-Position- und fehlende-Echtzeit-Zustände bleiben sichtbar und müssen im neuen Layout geprüft werden.
- iOS-spezifische Chrome-Effekte können nativ angenähert werden. Die Beurteilung konzentriert sich auf Hierarchie, Flächen, Kontrast, Abstände, erreichbare Aktionen und Datenwahrheit; der Windows-Nachweis ersetzt keine iOS-Geräteabnahme.
