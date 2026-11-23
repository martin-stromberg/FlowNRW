# Umsetzungsplan: Visuelle Integration des Designentwurfs

## Übersicht

Die vorhandene native App erhält die verbindliche Bildhierarchie und die Tokens aus docs/design/acceptance.md. Kleine prüfbare Abschnitte ersetzen die bisherigen Texttafeln schrittweise; fachliche Dienste, Datenwahrheit, Datenschutz und Lebenszyklus bleiben erhalten. Erst die vollständige Bildmatrix und separate visuelle Abnahme schließen Schritt9 ab.

## Designentscheidungen

| Bereich | Ansatz | Begründung |
|---|---|---|
| Ressourcen | Bestehende Colors.xaml/Styles.xaml um helle/dunkle Flächen, Textrollen, Radien und primäre/sekundäre Aktionen ergänzen; native Systemschrift | Eine gemeinsame Quelle; keine externen Fonts oder Webframeworks im Produkt. |
| Abfahrten | Wiederverwendbare native DepartureCardView mit Linienbadge, Ziel, Soll/Ist, Status, Gleis und Betreiber; fachliche Projektion aus StopEvent | Derselbe Aufbau in Home und Monitor, keine unbekannten Echtzeitdaten ergänzen. Vollständige zugängliche Texte erhalten. |
| Verbindungen | Native gegliederte Ergebnis-Karten, Zeit-/Umstiegskopf und Detailtimeline aus tatsächlichen JourneyLegs/Transfers | Draftstruktur mit echten Daten; keine aktive Reisebegleitung. |
| Navigation | Drei Shell-Tab-Kernbereiche Abfahrten/Verbindungen/Haltestellen, vorhandene Modelle bleiben geteilt; Ergebnisse/Details/Monitor/Karte/Einstellungen als Drilldowns | Native persistente Navigation, keine funktionslosen Tabs. Root-Pages nicht gleichzeitig global als dieselbe Instanz pushen. |
| Suche/Umgebung | Vorhandene Endpoint-/Stop-Lookups in verständlichen Karten; Namenstreffer, Koordinaten/Standort als beschriftete Optionen | Keine neue Eingabefunktion oder technische Kennungen. |
| Karte | Kartenfläche priorisieren, kompakte native Kontroll-/Statusgruppe und erhaltene Listenalternative | Keine erfundenen Marker/Fahrzeuge oder zusätzlichen Kartendownloads für Dekoration. |
| Testbilder | UiTest-only Startoptionen für Thema und Ausblenden der Instrumente; Systemtextskalierung150Prozent tatsächlich versuchen, sonst konkrete Grenze dokumentieren | Produkt bekommt keine technischen Schalter. Bilder reproduzierbar und ohne Steuerfelder. |
| Referenz | Vier HTMLs separat rendern, Originalicon prüfen/übernehmen, Bilder nebeneinander mit Appbildern bewerten | Kein bloßer Tokenvergleich und kein generierter Ersatzentwurf. |

## Programmabläufe

1. App lädt gemeinsame Theme-Ressourcen; vorhandene Seiten verwenden gleiche Flächen-/Text-/Aktionsrollen. Themewechsel bleibt systemgeführt; Defaultschrift skalierbar, keine festen Kartenhöhen.
2. Home/Departure rendern DepartureCardView aus ihren aktuellen Items. Busy-/Status-/Metadata und Command-Abkopplung bleiben unverändert. Unbekannte, verspätete und ausgefallene Fahrten erhalten verständliche Beschriftung zusätzlich zu Farbe.
3. Suchkarten führen dieselben Lookup-/Standortaktionen aus. Ein Suchergebnis öffnet die echte Verbindungsliste, deren Karten zur Detailtimeline führen. Auswahl und Eingaben bleiben beim Zurück erhalten.
4. Tabwechsel verändert den sichtbaren Kernbereich und pausiert dessen alte Lifecycle-Abos; Rückkehr setzt passende Refreshpfade fort. Querverweise wechseln zur kanonischen Such-/Stationswurzel, Drilldowns bleiben auf der gewählten Tabnavigation. Bestehende Navigationstests werden durch gleichwertige Tab-/Zurückassertions aktualisiert.
5. Kartenmarker und native Listenauswahl öffnen weiterhin den validierten Monitor. Fehlende Position/Geometrie und Offline bleiben sichtbar. Kartensnapshotregel aus Schritt8 bleibt erhalten.
6. Einstellungen verwenden denselben Stil und echte Speichern-/Fehlerlogik. Keine neuen fachlichen Nutzeroptionen.
7. Nach jedem sichtbaren Abschnitt nativen Teilfluss prüfen; nach vollständigem Umbau alle5Regressionen, Intervall-/Lifecycleprüfung und vollständige Screenshotmatrix durchführen. Referenz-/Appbilder getrennt kennzeichnen.

## Neue Klassen

| Klasse | Zweck |
|---|---|
| DepartureCardView | Gemeinsame native strukturierte Darstellung eines StopEvent |
| JourneyCardView | Native Ergebniszusammenfassung mit zugänglicher Aktion |
| JourneyTimelineView | Gegliederte Abschnitte/Umstiege aus bestehenden Journey-Daten |
| TransitVisuals | Gemeinsame native Label-/Badge-/Karten-/Layoutfabriken und semantische Theme-Rollen |
| WindowsDesignUiTests.ps1 | Reproduzierbare Bild-/Tab-/Schrift-/Bedienmatrix mit synthetischen Daten |

Fachliche Darstellungshilfen werden nur bei tatsächlicher Wiederverwendung in DeparturePresentation/JourneyPresentation ergänzt; keine zweite Echtzeitlogik oder Providerdienste.

## Änderungen an bestehenden Klassen

- Colors/Styles/App: semantische Ressourcen, native Schrift, Mindestziele44, Themeflächen und klare Fokus-/Disabledzustände. Originalicon/Splash in MAUI-Ressourcen übernehmen.
- HomePage/DeparturePage: RenderCards/RenderItems verwenden neue Abfahrtsdarstellung, Header/Aktionen kompakter, Metadata zugeordnet. Keine neuen Timer.
- SearchPage/StopSearchPage: bestehende Auswahlkarten strukturieren, große Überschriften und sekundäre Aktionen; alle Bindings und Validierungen erhalten.
- ResultsPage/JourneyDetailPage: Karten/Timeline aus tatsächlicher Auswahl dynamisch neu rendern; entfernte Controls korrekt abkoppeln.
- MapPage: native Karte/Listenalternative responsiv ordnen, Status/Attribution dauerhaft erreichbar. Bridge/Providergrenzen unverändert.
- RefreshSettingsPage: gleiche visuelle Rollen und flexible Texte.
- AppShell/MauiProgram: drei kanonische Rootbereiche, passende DI-Pagelebensdauer, existierende Drilldown-Routen. Kein doppeltes Einhängen von Singletons.
- UiTestFixtureServices/App: ausschließlich Testbuild-Thema/Instrumentausblendung und synthetische Layoutfälle; keine sensiblen Daten. Bestehende Counter/Fixtureaktionen für Funktionsprüfungen erhalten.
- Native Harness: AutomationIds/semantische Textprüfungen an Struktur anpassen, Assertions gleichwertig behalten, explizite Tabwechsel statt nicht mehr vorhandener Root-Zurückbuttons.

## Migrationen, Validierung und Konfiguration

Keine Datenmigration, keine neuen produktiven Pakete oder Providerkonfiguration. Vorhandene Such-/Fahrt-/Favoritenvalidierung bleibt. Linienfarben nur anhand gelieferter erkannter Kategorie/Bezeichnung, sonst neutral. Kontrast normal4,5:1/groß und Konturen3:1; abweichende Badgefarben zur Lesbarkeit dokumentieren. Nur UiTest darf technische Screenshotparameter lesen.

## Risiken

Tabnavigation kann Seiten mehrfach erstellen oder Zustände verlieren: gemeinsame Modelle, klare Rootzuständigkeit und native Regression. Semantische Beschreibungen dürfen visuelle Informationen nicht verstecken oder duplizieren. Große Schrift benötigt Umbruch und Scrollen statt Abschneiden. Lange Namen/Providerfehler dürfen Aktionen nicht verdrängen. Native Windows- und iOS-Tabdarstellung unterscheiden sich; keine Apple-Geräteprüfung vortäuschen. Referenzexport benötigt ggf. lokale Hilfsmittel zum Rendern; deren Ergebnis transparent kennzeichnen.

## Kleine Umsetzungsabschnitte

1. Gemeinsame Tokens und Abfahrtskarten; Voraussetzung vorhandene StopEvent-/Presentationmodelle. Monitor/Home nativ ansehen und Fehlerzustände prüfen.
2. Such-/Verbindungskarten und Timeline; Voraussetzung1, bestehende Lookup-/Journey-Sitzung. Routing/Details nativ prüfen.
3. Drei Kernbereiche und Haltestellen/Kartenlayout; Voraussetzung1/2, bestehende Shell-Routen/Bridge. Tab-/Drilldown-/Zurückflüsse und Standort/Favoriten prüfen.
4. Einstellungen, Icon, Themen/Schrift und Testinstrumentausblendung; Voraussetzung1–3. Reproduzierbare Matrix aufnehmen.
5. Finale funktionale Regression und separater visueller Vergleich; Voraussetzung vollständige Matrix. Abweichungen korrigieren, keine bloße blaue Akzentfarbe als Abschluss.
6. Dauerhafte docs/help/design-Hilfe, Referenzentscheidungen, Vergleichsbilder, iOS-Checkliste, README/Release Notes, Commit und Projektabnahme.

## Tests

| Pflichtnachweis | Konkreter Ablauf |
|---|---|
| Abfahrten | Home mit mehreren Favoriten und Einzelmonitor: normal/verspätet/ausgefallen/unbekannt, manuell/automatisch, Fehler/leer; Datenwahrheit und Bedienung nach Kartenumbau |
| Verbindungen | Start/Ziel manuell/Koordinaten/GPS wählen, Ergebnisse/Timeline/Karte öffnen, zurück und Tab wechseln, Auswahl/Eingaben bleiben; Fehler/leer/Busy und unsichere Resume-Auswahl |
| Haltestellen | Name/Nearby, Ablehnung/Entzug, Liste/Marker→Monitor→Favorit; Offline/fehlende Position/Geometrie, wiederholte Tabwechsel |
| Einstellungen | Aus/30/60/120/300 speichern, Fehler/Retry, Neustart, Tastatur; Intervall-/Lifecycle-Läufe auf finaler Navigation |
| Screenshotmatrix | Alle Zeilen docs/design/acceptance.md:430×900 und1024×768, hell/dunkel,150Prozent Schriftversuch; normale/leere/fehlerhafte Fälle, lange Texte, Tabfokus; proBild Commit,DPI,Thema,Skalierung,Referenz und synthetisches Szenario |
| Visueller Vergleich | Vier Original-HTML-Referenzen gerendert neben Appbildern; separater Prüfer bewertet tatsächliche Bilder und volle Matrix. Bei Agentenlimit keine unabhängige Prüfung behaupten. |
| Qualität | Vollständiger Releasebuild0/0 vor Coretests/Coverage mindestens70Prozent, UiTest0/0, Format/XML/Stub/Enum,26Release-Skripttests; WindowsCI unverändert |
| iOS extern | Kleine/große iPhones, SafeAreas/Hoch-/Querformat, hell/dunkel, DynamicType/VoiceOver nach konkreter Nutzercheckliste; kein lokaler nativer Beleg |

Bestehende WindowsJourneyUiTests in allen5Modi, WindowsRefreshUiTests und WindowsLifecycleUiTests bleiben die primären Funktionsregressionen. Neue Unit-Tests nur für ergänzte fachliche Projektionen/entscheidende Farbzuordnung, nicht für triviale Layoutsetter. Kontrastwerte zusätzlich rechnerisch dokumentieren. Keine Snapshotdatei ersetzt native Bedienung.

## Offene Punkte

Keine fachlichen Fragen. Verbindliche Nutzerentscheidungen und Scopekonflikte sind in docs/design/acceptance.md geklärt. Separate visuelle Abschlussprüfung bleibt erforderlich; Agentenlimit wird dokumentiert und nicht als bestanden gewertet.