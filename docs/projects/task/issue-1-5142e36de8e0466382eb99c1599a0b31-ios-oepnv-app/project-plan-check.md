# Projektplanprüfung – iOS ÖPNV-App

## Ergebnis

**Status:** Projektplan vollständig

Unabhängige erneute Planprüfung am 24.09.2026, insbesondere des neu ergänzten verpflichtenden Designpakets in Schritt 9. Der Prüfer hat diesen Projektplan und die Design-Abnahmekriterien nicht verfasst. Dies ist eine Prüfung der geplanten Abdeckung, keine visuelle Abnahme der bestehenden App.

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderung | Abdeckung im aktuellen Plan | Ergebnis |
|---|---|---|
| A01 / K01 – Vorlage ersetzen; iOS, C#, MVVM, Visual Studio | Schritt 2 AK 1, gemeinsame Rahmen; Schritt 8 AK 5–6; Schritt 9 native MAUI-Oberflächen | Vollständig |
| A02–A03 / K02 – Routing mit Adresse, Haltestelle, Koordinate, Umstiegen, Fußwegen, Linien, Betreiber und Echtzeit | Schritte 1–2; Standortübernahme in 5; Darstellung und Regression in 9 AK 1–2/5 | Vollständig |
| A04, A09–A10 / K03 – NRW-Priorität, bundesweite Versorgung, Normalisierung, eindeutige Soll-/Ist-Konsolidierung und Fallback | Schritt 1 AK 1–4, Schritt 2 AK 4, Schritt 3 AK 3; Schritt 9 verbietet Änderungen der Datenwahrheit | Vollständig |
| A05 / K04 – Such-/Kartenauswahl und Abfahrtsmonitor mit Verzögerung, Ausfall, Gleis/Steig | Schritte 3–5; Schritt 9 AK 1–3/5 verlangt strukturierte Zustände und erneute native Auswahl-/Monitorregression | Vollständig |
| A06 / K05 – Persistente Favoriten, Entfernung und konfigurierbare Intervalle | Schritte 6–7; Schritt 8 Koordination; Schritt 9 erhält Erreichbarkeit und prüft Regression | Vollständig |
| A07–A08 / K06 – Bundesweite Haltestellensuche, GPS-Nähe, Stationen und gelieferte Liniengeometrie | Schritte 1, 3–5; Schritt 9 Haltestellen-/Kartenhierarchie, zugängliche Liste und ehrliche fehlende Position/Geometrie | Vollständig |
| A11 / K07 und Designklarstellung vom 24.09.2026 – Moderne finale Oberfläche entsprechend Designentwurf | Vorgehensentscheidung 2 und Schritt 9 mit sechs AK; verbindlicher Verweis auf docs/design/acceptance.md und tatsächliche HTML-/Markdown-Referenzen | Vollständig |
| A12 – Trennung Views/ViewModels/Services, DI und asynchrone APIs | Schritt 1, gemeinsame Rahmen 2–8 und ausdrücklicher Erhalt in 9 | Vollständig |
| A13 / K08 – Cache, Netzlast, Fehler und Hintergrundaktualisierung | Schritt 1 AK 4, Schritt 7, Schritt 8 AK 1–2; Designpaket verändert weder Intervallregeln noch Providerimplementierung | Vollständig |
| A14 / K09 – HTTPS, technische lokale Daten und keine personenbezogene Historie ohne Zustimmung | Schritt 1 AK 4, Schritt 5 AK 3, Schritt 6 AK 1, Schritt 8 AK 2; Schritt 9 synthetische Abnahmebilder ohne private Standortdaten | Vollständig |
| A15 / K10 – Erweiterbare Verbund-/Sharing-/Push-Grenzen | Schritt 1 AK 6 und Schritt 8 AK 6; Zusatzprodukte des Entwurfs werden in Schritt 9 ausdrücklich ausgeschlossen | Vollständig |
| A16 / K11 – Tests, native UI-Nachweise, datensparsame Diagnose | Schritt 1 AK 4–5, Prüfungen jedes UI-Schritts; Schritt 9 ergänzt echte Bildvergleiche und wiederholt betroffene native Flüsse | Vollständig |
| A17 / K12 – Windows-Test/Release erhalten, iOS-Geräteabnahme beim Nutzer, kein neues Deployment | Vorgehensentscheidung 8–9, gemeinsame Rahmen und Schritt 9 AK 5; iOS-Checkliste einschließlich Dynamic Type/VoiceOver | Vollständig |
| A18 / K13 – Lokale IIS-Präsentation gestrichen | Entfallen; in Schritt 9 ebenfalls ausdrücklich ausgeschlossen | Entfallen |
| A19 / K14 – Prüffähige Pakete und bestehende Fortschritte bewahren | Bisherige fachliche Pakete bleiben; Schritt 9 ist eine abgegrenzte visuelle Integration bestehender Funktionen mit überprüfbaren AK, eigener Branchzuordnung und Abschlussbedingung | Vollständig |

## Designprüfung

| Prüfaspekt | Konkrete Planung | Ergebnis |
|---|---|---|
| Verbindliche Vorlage statt allgemeiner Designabsicht | Vier konkrete HTML-Ansichten, DESIGN.md und Flowdokument sind benannt; unbrauchbare 28-Byte-Screenbilder werden nicht als Abnahmegrundlage vorausgesetzt | Abgedeckt |
| Referenzkonflikte | Einheitliche HTML-/Frontmatter-Tokens haben Vorrang; native Systemschrift übernimmt die Hierarchie; drei beschriftete Kernbereiche ersetzen widersprüchliche vier/fünf Tabs ohne Zusatzprodukte | Abgedeckt |
| Erkennbarer Bildaufbau | Abfahrtsreihen mit Linienbadge/Ziel/Zeit/Status, gegliederte Verbindungskarten, vertikale Detailabschnitte und großflächige Karte mit Auswahl-/Listenweg ausdrücklich gefordert | Abgedeckt |
| Barrierearmut | Kontrast mindestens 4,5:1 bzw. 3:1, mindestens 44×44 logische Ziele, Fokus, Labels, Safe Areas, große Schrift und nicht allein farbliche Zustände | Abgedeckt |
| Tatsächliche visuelle Nachweise | Vollständige Matrix einschließlich leer/ladend/Fehler, schmal 430×900 und breit 1024×768, hell/dunkel sowie mindestens 150 % Textskalierung mit konkreten Versuchsnachweisen | Abgedeckt |
| Nachvollziehbarkeit und Datenschutz | Commit, Plattform, Fenstermaß/DPI, Thema, Skalierung, Datenszenario und Referenz je Bild; synthetische Daten; keine privaten Standortbilder oder Teststeuerungen in finalen Bildern | Abgedeckt |
| Funktionsschutz | Native Regression von Tabs/Drilldown/Zurück, manueller/GPS-Suche, Karte/Liste, Favoriten, Aktualisierung und Einstellungen nach UI-Umbau | Abgedeckt |
| Verbindliche Freigabe | Separater visueller Prüfer, dauerhafte Berichte/Bilder unter docs/help/design/verification und unabhängige fachliche Abnahme; kein Gesamtabschluss vor Schritt 9 | Abgedeckt |

Die auf den fachlichen Kern begrenzte Übernahme ist durch issue.md gedeckt. Tickets, aktive GPS-Reisebegleitung, Wagenreihung, Umstiegsalarm, Sharing-Verfügbarkeit, Auslastung, Push sowie Mikrofon/QR oder zusätzliche Filter werden nicht aus statischen Referenzbeispielen zu neuen Produktzusagen. Die Fahrtbegleiter-Timeline wird ausschließlich als Darstellung vorhandener Verbindungsdetails übernommen. Der Entwurf darf keine erfundenen Live-Werte, Stationsausstattung oder Geometrien erzeugen.

## Abhängigkeitsprüfung

| Schritt | Abhängigkeiten | Prüfung |
|---|---|---|
| 1 | Keine | Fachlicher Startpunkt |
| 2 | 1 | Datenkern vorhanden |
| 3 | 2 | App-/Navigationsbasis vorhanden |
| 4 | 2, 3 | Details und Monitor vorhanden |
| 5 | 2, 3, 4 | Manuelle Abläufe und Karte vor Standort |
| 6 | 3, 5 | Monitor und Entfernungsvoraussetzung |
| 7 | 3, 6 | Einzel- und Favoritenmonitore |
| 8 | 2, 3, 4, 5, 6, 7 | Lebenszyklus und integrierte Funktionsprüfung |
| 9 | 2, 3, 4, 5, 6, 7, 8 | Abschließende Gestaltung aller vorhandenen Oberflächen und erneute UI-Regression |

Alle Abhängigkeiten existieren, zeigen auf frühere Schritte und sind zyklenfrei. Schrittbeschreibungen, Übersichtstabelle und steps.md stimmen überein. Die vorhandenen Branchzuordnungen sind erhalten; Schritte 1–6 bleiben Fertig, 7 In Arbeit, 8 und 9 Offen. Schritt 9 besitzt eine eigenständig verständliche Kundenanforderung, fachliche Bereiche, Rahmen, Abhängigkeiten und überprüfbare AK samt eindeutigem Referenzpfad und eignet sich damit unverändert zur Lifecycle-Übergabe.

Schritt 8 bleibt die integrierte funktionale Lebenszykluslieferung. Seine Designkern-/Barrierearmutsprüfungen heben die zusätzliche verbindliche Bildabnahme von Schritt 9 nicht auf. Schritt 9 AK 6 und die Freigaberegel in acceptance.md schließen den Gesamtabschluss vorher ausdrücklich aus. Bestehende fachliche Abnahmen 1–6 werden nicht rückwirkend als visuelle Gesamtabnahme umgedeutet oder aufgehoben.

## Fehlende oder unvollständige Punkte

Keine.

## Hinweise

Prüfgrundlage: project-Skill, ursprüngliche issue.md, aktuelle Projektanforderung einschließlich Designklarstellung, ursprüngliche Bestandsaufnahme, vollständige fachliche Schrittbeschreibungen und Tracking sowie docs/design/acceptance.md. Die tatsächlichen HTML-Referenzen für Monitor, Verbindungssuche, Detail und Karte sowie Designsystem und Flowdokument wurden auf Tokens, Struktur, Navigation und Zusatzumfang abgeglichen. Die Bestandsaufnahme vom 07.09.2026 ist erkennbar historischer Ausgangsstand; der aktuelle Fertig-Status ergibt sich aus steps.md und wird hier nicht neu fachlich abgenommen.

Die Prüfung bestätigt die Planung, keine bereits moderne finale Oberfläche, bestandene Screenshotmatrix oder ausgeführte iOS-Geräteprüfung. Native iOS-Abnahme verbleibt beim Nutzer; technische Windows-Grenzen bedürfen konkreter Versuche und dürfen nicht als PASS bezeichnet werden. Der bisherige Prüfbericht wurde unverändert und hashgleich als project-plan-check.4.md archiviert. Keine Plan-, Produkt- oder Trackingkorrekturen, Builds, UI-Ausführungen oder Commits durch diesen Prüfer.
