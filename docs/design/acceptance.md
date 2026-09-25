# Verbindliche visuelle Abnahme – FlowNRW

Stand: 24.09.2026. Der Nutzer bestätigt ausdrücklich, dass die fertige App modern wie `design-draft.zip` aussehen soll. Die bisherigen funktionalen Abnahmen ersetzen diese visuelle Abnahme nicht. Schritt 9 ist vor Projektabschluss verpflichtend.

## Quellen und Konfliktentscheidungen

Originale unter `reference/`: `trans_nrw_mobil_system/DESIGN.md`, `flownrw_app_flow_dokumentation.md`, `abfahrtsmonitor_live/code.html`, `verbindungssuche/code.html`, `fahrtbegleiter_detail/code.html`, `umgebungskarte_stationen/code.html` und das tatsächliche App-Icon `flownrw_icon.png/screen.png`. Die vier HTML-Dateien sind visuelle Referenzen, keine auszuliefernde Web-App. Die 28-Byte-Screen-Platzhalter sind keine brauchbaren Bildvorlagen. Lokale Referenzrenderings müssen als solche gekennzeichnet werden; externe Tailwind-/Font-Abhängigkeiten aus dem Export sind keine Produktabhängigkeit.

Priorität: fachlicher Umfang aus issue.md und bestätigte Nutzerergänzungen; für visuelle Tokens die übereinstimmenden HTML-Konfigurationen und DESIGN.md-Frontmatter; für Formen/Hierarchie ergänzend die Designprosa. Alle HTMLs und das Flowdokument enthalten vier Tabs inklusive Fahrtbegleiter, die Designprosa nennt fünf inklusive Tickets. Verbindliche Umsetzung: drei persistente, beschriftete Kernbereiche **Abfahrten**, **Verbindungen**, **Haltestellen**. Abfahrten ist Start/Favoritenübersicht; Haltestellen verbindet Suche/Umgebung/Karte. Verbindungsdetails bleiben Unterseite mit Zurücknavigation; Einstellungen sind eine erreichbare sekundäre Aktion. Keine leeren Tabs. Damit wird die im Projektplan bereits gewählte Kernnavigation konkretisiert, keine neue Funktion eingeführt.

Tickets, aktive Reisebegleitung/GPS-Tracking, Wagenreihung, Umstiegsalarm, Push, Sharing-Verfügbarkeit, Mikrofon/QR, Auslastung und neue Filterprodukte werden nicht übernommen. Kein rein dekorativer Button suggeriert diese Funktionen. Die Timeline aus dem Fahrtbegleiter-Entwurf dient ausschließlich bereits gelieferten Verbindungsdetails. Keine erfundenen Echtzeitwerte, Kartengeometrien, Stationsausstattung oder Fahrzeugpositionen. Die im Flowtext genannten 15–30 Sekunden ersetzen nicht die geprüfte Intervalleinstellung aus Schritt 7.

## Tokens und Komponenten

| Rolle | Verbindliche Umsetzung |
|---|---|
| Heller Hintergrund / Karten | `#FAF9FE` / `#FFFFFF`; innere Gruppen `#F4F3F8`, stärkere Gruppierung `#EEEDF3` |
| Text | Primär `#1A1B1F`, sekundär `#414755`; kein blasser Text für notwendige Fahrtdaten |
| Interaktion | Primär `#0058BC`, aktiver Container `#0070EB`, weißer Text; ersetzt abweichendes Systemblau `#007AFF` aus der Prosa |
| Semantik | Bestätigung `#006E28`, Warnung `#894D00`, Fehler/Ausfall `#BA1A1A`; immer zusätzliche verständliche Beschriftung, keine Farbe als einziger Träger |
| Linienkennzeichnung | RE/RB `#BA1B1D`, S `#008D43`, U `#005A9C`, Tram `#E35205`, Bus `#6F2C91`, Fähre `#009AA6`; nur bei gelieferter erkennbarer Kategorie, sonst neutrales Badge. Helle Badgefarben bei Bedarf abdunkeln bzw. Textfarbe ändern, damit Kontrast besteht |
| Dunkle Darstellung | Hintergrund `#000000`, Karte `#1C1C1E`, innere Gruppe `#2C2C2E`; heller Text und kontrastgerechte Akzentvarianten. Nicht unverändert helle Tokens auf dunkle Flächen legen |
| Schrift | Native Systemschrift mit derselben Hierarchie wie Inter/SF-Pro-Referenz; keine extern geladene Schrift erforderlich. Titel 28–34, Abschnitt 22, Haupttext17, Nebeninformation15, Metadaten13 logische Einheiten bei Standardskalierung. Abfahrtszahlen tabellarisch/Monospace18, Verzögerung14 |
| Rhythmus | 4/8/12/16/24/32 Raster; Telefongutter16, breite Ansichten24; Kartenabstand12, Innenabstand16 |
| Form | Hauptkarten Radius16–20, Felder10, Aktionsbuttons12, Linienbadges6–8, Statuschips als Kapsel; dezente Separatoren statt schwerer Rahmen/Schatten |
| Bedienung | Mindestens44×44 logische Einheiten für interaktive Ziele, beschriftete Icons, sichtbarer Tastaturfokus, skalierbarer Text ohne Überdeckung; Safe Areas und genügend Platz über unterer Navigation |

Kontrastprüfung: normaler Text mindestens4,5:1, großer Text und bedienrelevante grafische Konturen mindestens3:1. Anpassungen zum Erreichen dieser Werte sind zu dokumentieren und haben Vorrang vor unleserlicher exakter Farbreproduktion. Keine starre Kartenhöhe bei großer Schrift. Glaseffekt/Blur ist optional plattformgerecht; erkennbare Flächenhierarchie und Lesbarkeit sind verbindlich.

## Bildaufbau je Oberfläche

- **Abfahrten/Home und Einzelmonitor:** klare große Überschrift, kompakte Standort-/Intervallinformation, stationsweise weiße bzw. dunkle gerundete Karten. Abfahrtszeile als strukturierte Reihe: Linienbadge, Ziel, Soll/Ist-Zeit und Verspätung, Gleis/Steig darunter. Quellen-/Alter-/Fallbackinformation bleibt erreichbar und zugeordnet; keine langen zusammengeklebten Diagnoseabsätze als primäre Gestaltung. Favorit/Refresh als klare sekundäre Aktionen. Ausfall und unbekannte Echtzeit ausdrücklich lesbar.
- **Verbindungssuche/Ergebnisse:** zusammengehörige Start-/Zielfelder in einer Eingabekarte, klarer primärer Suchbutton, Standort-/Koordinatenoptionen zugänglich; Ergebnisse als gegliederte Verbindungskarten mit Dauer, Umstiegen, Linien und Zeiten. Manuelle Eingabe, Mehrdeutigkeit und Validierung vollständig erhalten.
- **Verbindungsdetail:** zusammenfassender Kopf, vertikale gegliederte Reiseabschnitte mit Linien-/Fußwegkennzeichnung und nachvollziehbarem Soll/Ist. Nur gelieferte Route, keine aktive Fahrtverfolgung. Vorhandener Kartenweg und Rückweg bleiben erhalten.
- **Haltestellen/Umgebung:** Suchleiste, explizite Standortaktion und erkennbare Listen-/Kartenumschaltung. Karte erhält möglichst viel Fläche, unaufdringliche gut erreichbare Kontrollen; selektierte Station als unterer bzw. angedockter Informationsbereich oder vorhandener Monitor-Drilldown. Native zugängliche Listenalternative und sichtbare Attribution erhalten. Fehlende Position/Offlinezustand ehrlich darstellen.
- **Einstellungen:** dieselbe Typografie/Flächensprache, übersichtliche Intervallauswahl und Speichern mit Erfolg/Fehler; kein neues Einstellungsprodukt.

## Verbindliche Screenshot- und Bedienmatrix

Deterministische synthetische Daten verwenden, ohne privaten Standort. Pro Bild festhalten: Commit, Plattform, Fenstermaß, Thema, Textskalierung, Datenszenario und zugehörige Referenz. Test-Szenariofelder in den Abnahmebildern ausblenden, ohne fachlichen Zustand oder Fehler zu verstecken. Originalbild und Appbild nebeneinander beurteilen; keine reine Token-/Codeprüfung.

| Oberfläche / Szenario | Referenz | Pflichtvarianten |
|---|---|---|
| Home: mehrere Favoriten, Entfernung unbekannt/bekannt | abfahrtsmonitor_live | hell/dunkel; schmal und breit |
| Einzelmonitor: normal, verspätet, Ausfall, keine Echtzeit | abfahrtsmonitor_live | schmal; beide Themen mindestens einmal |
| Suchformular, Treffer/Verbindungsergebnisse | verbindungssuche | schmal/breit; Eingabefehler und Ladezustand |
| Details mit Umstieg und Fußweg | fahrtbegleiter_detail | schmal; große Schrift; fehlende Geometrie |
| Haltestellenliste, Karte mit Auswahl und Listenalternative | umgebungskarte_stationen | hell/dunkel; Offline/fehlende Position |
| Einstellungen mit Speichererfolg/-fehler | abgeleitete gemeinsame Tokens | schmal; Tastatur und große Schrift |
| Leere Favoriten, keine Verbindungen, Providerfehler | jeweilige Oberfläche | je ein Bild; manuelle Wiederholung sichtbar |

Windows tatsächlich mindestens schmal430×900 und breit1024×768 prüfen; schmal bezieht sich auf logische Fenstergröße, tatsächliche DPI im Nachweis nennen. Große Schrift mit mindestens150 % über erreichbare System-/App-Textskalierung ernsthaft prüfen und konkrete Grenzen dokumentieren. Automatisierte native E2E für Tab-/Drilldown-/Zurücknavigation, manuelle/GPS-Suche, Marker-/Listenwahl, Favorit/Refresh/Intervall sowie Fehlerwege nach Umbau wiederholen. Kein Screenshot ersetzt Funktionsnachweis, kein bestandener Funktionstest ersetzt Bildvergleich.

iOS-Build/Geräteprüfung bleibt beim Nutzer: Checkliste für kompakte/große iPhones, Safe Areas, Hoch-/Querformat, hell/dunkel, Dynamic Type und VoiceOver bereitstellen; nicht ausgeführte Gerätebilder nicht behaupten. Windows-Limits erst nach konkretem Versuch nennen.

## Freigaberegel

Schritt9 wird erst fertig, wenn ein separater visueller Prüfer die vollständige Matrix gegen diese Kriterien und die HTML-/Designreferenz bewertet, Abweichungen behoben oder konkrete technische Grenzen transparent festgehalten sind und die betroffenen nativen Funktionsregressionen bestehen. Bericht und Screenshots dauerhaft unter docs/help/design/verification ablegen. Bestehende fachliche Abnahmen1–6 bleiben gültig; UI-Umbau erfordert gezielte Regression statt Wiederholung ihrer Providerimplementierung. Vorher kein Gesamtabschluss.
