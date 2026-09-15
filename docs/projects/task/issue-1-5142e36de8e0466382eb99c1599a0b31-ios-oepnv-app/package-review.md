# Bewertung der Aufgabenpakete

Stand: 2026-09-15. Anlass: Nutzer meldet mehrere Sessionzyklen ohne Abschluss von Schritt 1 und bittet vor Fortsetzung um Neubewertung; IIS „ÖPNV“ ist als Begutachtungskanal freigegeben.

## Ergebnis

Die ursprünglichen vier Pakete bündeln zu viele unterschiedlich prüfbare Lieferungen. Besonders „Monitore + Favoriten + GPS + Intervalle“ und „Karte + Hintergrund + Gesamtabnahme“ verzögern sichtbares Feedback. Der Plan wird auf acht Schritte zugeschnitten. Schritt 1 bleibt mit allen sechs Akzeptanzkriterien unverändert; seine fast fertige Implementierung nachträglich zu zerlegen würde bereits geleistete Arbeit und Abnahmezuordnung beschädigen.

Der Stand vor der gezielten Nachbesserung ist nachweisbar: Abschlusscommit 0d7eb4b, 98 .NET-Tests, 98 % Core-Abdeckung und Windows-Build ohne Warnungen/Fehler (docs/help/fahrplanauskunft/verification/iteration2-checks.md). Die unabhängige Projektabnahme fand genau die fehlende bundesweite Mengen-Ergänzung bei regionalen Teilantworten. Das ist eine begrenzte Korrektur des bestehenden AK 3. MainPage.xaml zeigt dagegen weiterhin die MAUI-Vorlage: Der umfangreiche Datenkern ist für Stakeholder bisher nicht direkt bedienbar.

Die Dauer lässt sich ohne Zeitmessung nicht seriös prozentual aufteilen. Belegt sind mehrere Nutzungs-/Sessionunterbrechungen, Lifecycle-Plan-/Code-/Dokumentationsprüfungen und eine echte Projekt-Abnahmenachbesserung. Größere Pakete und der technische Grundlagenschnitt verzögern UI-Feedback; Unterbrechungen und verpflichtende Prüfrunden erklären zusätzlich Kalenderzeit. Kleine Pakete erhöhen selbst die Zahl der Lifecycle-Durchläufe. Deshalb werden fachlich zusammengehörige manuelle Suche und Verbindungsdetails zusammen geliefert; keine weitere Zerlegung auf Klassen-/Dateiebene.

## Lieferzuschnitt

| Schritt | Begutachtbares Ergebnis | Bewusst getrennte Folgearbeit |
|---|---|---|
| 1 | Geprüfte echte Datenservices | UI in 2 |
| 2 | Manuelle Verbindungssuche mit vollständigen gelieferten Details, iOS-Basis, erste Windows-App | GPS in 5 |
| 3 | Manuelle Haltestellensuche → Live-Monitor, manuelles Aktualisieren | GPS 5, Favoriten 6, Intervalle 7 |
| 4 | Manuelle Umgebungskarte → Monitor; gelieferte Liniengeometrie | GPS 5, Hintergrund 8 |
| 5 | GPS als Start/Ziel, nahe Stationen in Liste/Karte | Favoriten 6 |
| 6 | Persistente Favoriten und nach Entfernung sortierte Startseitenmonitore | Intervalle 7 |
| 7 | Einstellbare automatische Vordergrundaktualisierung | Hintergrund 8 |
| 8 | Hintergrund/Wiederaufnahme und integrierte Gesamtabnahme | Deploymentautomatisierung außerhalb Projekt |

Bestehende gültige Branchzuordnungen der Schritte 1–4 bleiben unverändert, obwohl Titel 2–4 präzisiert werden. Neue Branches 5–8 werden erst im Tracking durch den Projektleiter zugeordnet. Alle Abhängigkeiten zeigen auf frühere Nummern. Kein bereits abgeschlossener Funktionsumfang entfällt; keine ursprüngliche Anforderung wird gestrichen.

## Überleitung sämtlicher ursprünglicher Akzeptanzkriterien

Die Kennzeichnung „alt 2.1“ bedeutet ursprünglicher Schritt 2, AK 1. „Rahmen“ bezeichnet die ausdrücklich in jeder neuen Schrittbeschreibung mitgeführten verbindlichen Rahmenbedingungen.

| Ursprüngliche AK | Neue Abdeckung |
|---|---|
| alt 1.1–1.6 | Schritt 1 unverändert einschließlich Reihenfolge und Wortlaut |
| alt 2.1 | 2.1 (Vorlage, iOS-Basis, Navigation, MVVM/DI); Standortbeschreibungen ergänzend 5.1 |
| alt 2.2 | 2.2 (manuelle Eingabearten/Zustände), 5.1/5.3 (GPS/Berechtigung/veraltete Position) |
| alt 2.3 | 2.3 (Details, Reihenfolge, Soll/Ist, Zeitzone/Tageswechsel) |
| alt 2.4 | 2.4 und vorhandener Schritt 1 (regionale Integration/Konsolidierung/Fallback) |
| alt 2.5 | Rahmen aller UI-Schritte, 2.5 und 8.6; Schritt 1.4/1.6 bleibt bestehen |
| alt 2.6 | 2.4/2.5, 5.4 und Rahmen (Tests aller Eingaben inkl. GPS, Live-Proben) |
| alt 2.7 | Rahmen, 2.1/2.5 und 8.5/8.6 (Build/CI/iOS-Prüfanleitung) |
| alt 2.8 | Rahmen, 2.5, 8.3/8.6 (Barrierearmut/Dokumentation) |
| alt 3.1 | 3.1 und 5.1/5.2 (manuelle und GPS-nahe Stationsauswahl) |
| alt 3.2 | 3.2 (Abfahrtsfelder/Ausfall) |
| alt 3.3 | 3.3 und Rahmen (regionale Konsolidierung/Fehler/Quelle/Alter) |
| alt 3.4 | 6.1/6.2 (Persistenz/Startseite/Entfernung) |
| alt 3.5 | 7.1–7.3 sowie 3.3/6.3 (Intervalle/Abbruch/manuell) |
| alt 3.6 | 3.3, 6.1/6.3, 7.2/7.3 und Rahmen (Datenschutz/asynchron/letzte Daten) |
| alt 3.7 | 3.4, 5.4, 6.4, 7.4 und Rahmen (jeweilige Tests/NRW-Liveprobe/Regression) |
| alt 3.8 | Rahmen, 8.3/8.5/8.6 (Barrierearmut/Windows/iOS-Abnahmegrenze) |
| alt 4.1 | 4.1, 5.2 (Karte/Liste/Monitor/manuelle Umgebung/GPS) |
| alt 4.2 | 4.2 (Geometrie/Quelle/Attribution/Konfiguration) |
| alt 4.3 | 8.1/8.2 (Hintergrund/Wiederaufnahme/Koordination) |
| alt 4.4 | 4.3, 5.1/5.3, 8.2 und Rahmen (Standort/Datenschutz/Fehler/Netzlast) |
| alt 4.5 | 8.3 und Rahmen (integrierte Navigation/Gestaltung/Barrierearmut) |
| alt 4.6 | 4.4, 5.4, 6.4, 7.4, 8.4 und Rahmen (UI-Ausführung/Geometrie/Koordination/Regression) |
| alt 4.7 | 8.5 und Rahmen (lokale Checks/Windows-CI/iOS-Code und Nutzerabnahme) |
| alt 4.8 | 8.6 und Rahmen (dauerhafte Dokumentation/Erweiterbarkeit/Abnahmecheckliste) |

## Begutachtung und schlanker Ablauf

Ab Schritt 2 nach abgenommenen UI-Lieferungen einen Windows-ZIP-Stand über die vorbereitete IIS-Website bereitstellen, mit Startanleitung, Commit/Version, Umfang und bekannten Grenzen. Download und lokaler App-Start werden geprüft. Im Verzeichnis D:\Dashboard\ÖPNV ist inzwischen die eigene statische Begutachtungsseite installiert. http://localhost/%C3%96PNV/ liefert HTTP 200 mit erwartetem Inhalt und identischem Quell-/Zielhash; docs/review/README.md dokumentiert den Nachweis. Noch kein nativer UI-Build wird angeboten, da die Vorlage keine fachlich bedienbare Lieferung wäre. Die externe Stakeholder-URL wird separat geklärt und blockiert die lokale Entwicklung nicht. Eine visuelle Browserabnahme wird nicht behauptet. IIS liefert Paket und Unterlagen; es wird keine native MAUI-App als Browser-Webanwendung behauptet.

Vorhandene Services, UI-Testinfrastruktur ab Schritt 2 und gültige Nachweise weiterverwenden. Neue Tests decken das neue Verhalten und betroffene Regressionen ab; vollständige Neuimplementierung, neue Vollinventur und Wiederholung unveränderter Prüfungen sind nicht nötig. Die fachliche unabhängige Abnahme je Schritt bleibt bestehen. Die geänderte Projektplanung benötigt eine neue unabhängige Prüfung vor Umsetzung der neu geschnittenen Schritte. Tracking und Status werden danach durch den Projektleiter abgeglichen.
