# Umsetzungsplan – aktualisierter Stitch-Entwurf

**Grundlage:** `requirement-draft-refresh.md`, `inventory-nearby-interaction.md`, bestehender MAUI-Code und `stitch_nrw_transit_ios_app.zip`
**Ziel:** Den kaputten Klickpfad für nahe Haltestellen schließen und die vorhandenen, fachlich gedeckten Bereiche gegen den aktualisierten hellen/dunklen Entwurf vereinheitlichen.

## Vorbereitender Schritt – Referenzen dauerhaft bereitstellen

**Dateien**

- Neu bzw. ersetzen: `docs/design/reference/` mit den inhaltsvollen `code.html`, `screen.png` und `DESIGN.md` aus `stitch_nrw_transit_ios_app.zip`
- Aktualisieren: `docs/design/acceptance.md`
- Aktualisieren: `docs/help/design/` mit einer kurzen Zuordnung Entwurfsansicht → vorhandene App-Seite

**Arbeit**

1. Das neue Archiv kontrolliert in die Referenzablage übernehmen, einschließlich der vier dunklen Ansichtspaare und beider Design-System-Dateien.
2. Leere 28-Byte-Altscreenshots nicht mehr als Referenz oder Nachweis führen.
3. In der Abnahmematrix die maßgeblichen Ansichten festlegen: `abfahrtsmonitor_live`, `verbindungssuche`, `fahrtbegleiter_detail`, `umgebungskarte_stationen`, jeweils hell/dunkel.

**Abnahme**

- Jede referenzierte Bilddatei ist lesbar und größer als ein leerer Platzhalter.
- Die Abnahmedokumentation benennt nur umsetzbare, fachlich vorhandene Elemente.

## Schritt 1 – Nearby-Auswahl auf der Startseite reparieren

**Dateien**

- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW/HomePage.cs`
- Bestehende Core-Testklasse für `StopMonitorViewModel` unter `FlowNRW.Tests/`
- `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`
- Falls nötig: `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`

**Arbeit**

1. `StopMonitorViewModel` erhält eine klar benannte, öffentliche Einstiegsmethode ausschließlich für validierte Startseiten-Kandidaten. Sie prüft eine vollständige `Stop`-Identität (`Id`, `Source`) und verwendet anschließend den bestehenden privaten Öffnungs- und Navigationspfad.
2. Die vorhandene Referenzgleichheitsprüfung von `OpenAsync(Address)` für Such- und eigene Nearby-Ergebnislisten bleibt unverändert.
3. `HomePage.RenderNearby` ruft die neue Startseiten-Methode auf. Der sichtbare Nearby-Eintrag bleibt als zugängliches, mindestens 44×44 großes Ziel erhalten.
4. Core-Test: vollständiger externer Kandidat navigiert einmal, lädt genau diesen Stop; Kandidaten ohne `Id` oder `Source` bewirken weder Navigation noch Provideraufruf.
5. UI-Test: Fixture-Standort setzen, `NearbyStop0` aktivieren und auf Monitorroute, Haltestellenmetadaten `fixture-nearby-0` sowie den Abschluss des Abfahrtsladezustands warten.

**Abnahme**

- Ein Tipp, Mausklick und Tastaturaktivierung auf einen Nicht-Favoriten aus „Nächste Haltestellen“ öffnet dessen Abfahrtsmonitor.
- Bestehende Auswahl von Suchtreffer und Favorit funktioniert weiterhin.
- Der UI-Test verhindert die bisherige stille Rückkehr ohne Navigation.

## Schritt 2 – Gemeinsame native Designgrundlagen erneuern

**Dateien**

- `FlowNRW/Resources/Styles/Colors.xaml`
- `FlowNRW/Resources/Styles/Styles.xaml`
- `FlowNRW/TransitVisuals.cs`
- `FlowNRW/App.xaml.cs`
- Bei Bedarf: `FlowNRW/Resources/Fonts/` und `FlowNRW/MauiProgram.cs`

**Arbeit**

1. Farben, Textstufen, Kartenoberflächen, Eingaben, Trennlinien, Radien, Abstände, Buttonrollen und Linienbadges aus den neuen Design-System-Dateien als zentrale dynamische Ressourcen definieren.
2. Hellmodus verwendet helle gruppierte Fläche und klar abgegrenzte Karten; Dunkelmodus dunkle gruppierte Flächen, kontrastreiche Textstufen und keine Farbinversion.
3. Technische Status- und Quelltexte erhalten eine sekundäre Darstellung. Primäre Abfahrts-/Verbindungsinformationen und eine Aktion pro Kontext erhalten visuelle Priorität.
4. Barrierefreiheit erhalten: dynamische Schrift, ausreichender Kontrast, sichtbarer Tastaturfokus, 44×44 Mindestgröße, Status nicht nur über Farbe.

**Abnahme**

- Beide Modi zeigen eine einheitliche, ruhige Kartenhierarchie mit klarer Textpriorität.
- Komponenten benötigen keine ad-hoc Farb- oder Abstandsdefinitionen in den Seiten, soweit ein zentrales Token existiert.

## Schritt 3 – Abfahrten und Startseite gemäß Abfahrtsmonitor-Referenz

**Dateien**

- `FlowNRW/HomePage.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/DepartureCardView.cs`
- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs` nur bei notwendiger, textlicher Darstellungskorrektur

**Arbeit**

1. Favoriten und nahe Haltestellen als klar getrennte, gestaltete Kartenbereiche mit erkennbarem Klickziel zeigen; entfernte technische Buttons/Textblöcke durch zurückhaltende Kontextaktionen ersetzen.
2. Abfahrtskarten an der Referenz ausrichten: Linienbadge, Ziel, Zeit/Echtzeit, Steig/Gleis und Status als lesbare Ebenen; Lade-, Leer-, Offline- und Fehlerzustand bleibt ehrlich.
3. Quellen, technische Kennungen und sekundäre Statusinformationen nicht prominent in den Kartenkopf legen, aber zugänglich erhalten.

**Abnahme**

- Auf schmaler Breite bleiben mehrere Abfahrten und die nächste Haltestelle ohne Formularcharakter erfassbar.
- Ausfall, Verspätung, Plattformwechsel und fehlende Echtzeit werden semantisch und textlich verständlich dargestellt.

## Schritt 4 – Verbindungssuche und Ergebnisse gemäß Such- und Detailreferenz

**Dateien**

- `FlowNRW/SearchPage.cs`
- `FlowNRW/ResultsPage.cs`
- `FlowNRW/JourneyCardView.cs`
- `FlowNRW/JourneyDetailPage.cs`
- `FlowNRW/JourneyTimelineView.cs`
- `FlowNRW.Core/Presentation/JourneySearchViewModel.cs` nur für vorhandene Zustandsbindung, keine neue Routingfunktion

**Arbeit**

1. Start/Ziel, Favoritenvorschläge, Standortaktion und vorhandene Abfahrt-/Ankunftszeit in kompakte, klar priorisierte Suchoberfläche überführen.
2. Bereits vorhandene Suchauswahl ersetzt Ergebnisse sichtbar; technische Haltestellenkennungen bleiben intern/sekundär.
3. Ergebnis- und Detailkarten mit Linienbadges, Zeitachsen, Umstiegen und Fußwegen nach dem neuen Entwurf strukturieren. Nur tatsächlich gelieferte Betreiber-, Echtzeit- und Geometriedaten zeigen.
4. Keine Fahrtbegleitungs- oder Trackingdarstellung implementieren: Der Detailentwurf wird als statische Reiseinformation ohne Live-Verfolgung adaptiert.

**Abnahme**

- Suche, Zeitmodus, Ergebniswahl, Detailöffnung und Rücknavigation funktionieren in beiden Modi ohne Textüberlagerung oder unverständliche technische Werte.
- Umstiege, Fußwege, Linien und Betreiber sind in der bestehenden Datenwahrheit klar lesbar.

## Schritt 5 – Haltestellensuche und Karte gemäß Umgebungsreferenz

**Dateien**

- `FlowNRW/StopSearchPage.cs`
- `FlowNRW/MapPage.cs`
- `FlowNRW.Core/Maps/MapViewModel.cs`
- `FlowNRW/Resources/Raw/map/map.css`
- `FlowNRW/Resources/Raw/map/map.js` nur falls für vorhandene Marker-/Listen-Darstellung erforderlich

**Arbeit**

1. Haltestellensuche, Umgebungsaktion, Ergebnisliste und Karten-/Listenalternative mit denselben Karten-, Such- und Statusmustern gestalten.
2. Bestehende Auswahl eines Karten- oder Listeneintrags öffnet weiterhin den Monitor; fehlende Position, Offlinekarte und fehlende Geometrie bleiben erklärte Zustände.
3. Keine im Entwurf vorhandenen, aber nicht angeforderten Kartenfunktionen wie Tracking, Tickets oder erfundene Auslastungsdaten ergänzen.

**Abnahme**

- Suchtreffer, Nearby-Standortsuche, Kartenmarker und Listenalternative bleiben in Hell/Dunkel bedienbar und führen zum richtigen Monitor.
- Ohne Position oder Netz bleibt die Seite inhaltlich hilfreich und erfindet keine Karte/Daten.

## Schritt 6 – Windows-UI-Testmatrix, Build und Dokumentation

**Dateien**

- `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`
- Bei Ablauftrennung: `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`
- `docs/design/acceptance.md`
- `docs/help/design/verification/` für Screenshots und Protokolle
- `docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/test-results.md`

**Arbeit**

1. Nearby-Klickpfad aus Schritt 1 verbindlich in die native Matrix aufnehmen.
2. Für Abfahrten/Startseite, Suche/Ergebnisse, Reisedetail und Haltestellen/Karte je einen belastbaren Daten-, Lade-/Leer- oder Fehlerzustand in Hell und Dunkel erfassen.
3. Windows schmal 430×900, breit 1024×768 und erhöhte Textskalierung prüfen; Screenshots mit Testschnittstelle frei von Steuerhilfen erfassen.
4. `dotnet build` der Windows-UiTest-Konfiguration mit Warnungen als Fehler und die Core-Test-Suite ausführen. iOS-Geräteabnahme als separaten Nutzercheck dokumentieren, nicht simulieren.

**Abnahme**

- Alle Matrixabläufe bestehen, einschließlich `NearbyStop0` → korrekter Monitor.
- Screenshots belegen das aktualisierte Erscheinungsbild in beiden Modi.
- Keine Testschnittstelle oder Entwurfselemente außerhalb des Scopes sind im Produktbild sichtbar.

## Ausdrücklich nicht umsetzen

Die folgenden im Stitch-Archiv gezeigten Elemente sind nicht Bestandteil dieser Umsetzung: Tickets/Wallet, zusätzliche Ticket- oder Favoriten-Tabs, aktive Fahrtbegleitung und Echtzeittracking, Auslastungsanzeigen, QR-/Mikrofonaktionen, Sharing, Push-/Störungsprodukte und erfundene Betriebsdaten. Sie werden weder als funktionslose Bedienelemente noch als Platzhalter in die App aufgenommen.
