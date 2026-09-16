# Usability-Review

## Ergebnis

**Status:** Keine Befunde

Die geforderten manuellen Such- und Navigationsaufgaben sind über verständlich beschriftete native Bedienelemente erreichbar. Treffer werden mit Namen ausgewählt; technische Kennungen müssen nicht eingegeben werden. Die aktuellen Ergebnisbilder einschließlich der schmalen Fensteransicht zeigen vollständig umbrochene Texte. Keine aufgabenverhindernden Usabilitybefunde im geprüften Umfang.

## Geprüfte Interaktionen

- Adresse als Start und Ziel suchen und einen mehrdeutigen Treffer auswählen → unauffällig.
- Haltestelle als Start und Ziel suchen, benannte Treffer unterscheiden und auswählen → unauffällig.
- Koordinatenmodus für beide Endpunkte aktivieren, Breite/Länge eingeben und übernehmen → unauffällig.
- Ungültige oder leere Eingaben verstehen, korrigieren und erneut suchen → unauffällig.
- Suchtext beziehungsweise Auswahl ändern und mit neuem Kontext suchen → unauffällig; alte Auswahl wird zurückgesetzt.
- Nach Auswahl beider Endpunkte Verbindungen suchen und Lade-, Leer- und Fehlerzustände erkennen → unauffällig.
- Zeitlich geordnete Verbindungen samt Umstiegen, Fußwegen, Linien, Betreibern und Soll-/Istzeiten lesen → unauffällig, auch im aktuellen schmalen Ergebnisfenster.
- Tageswechsel, Zeitzone, fehlende Echtzeit und Ausfall erkennen → unauffällig; textliche Angaben vorhanden.
- Quelle, Datenalter, Ersatzquelle, Warnung und veralteten Cache erkennen → unauffällig.
- Verbindung öffnen, Teilstrecken und Ereignisse lesen, zur Ergebnisliste und Suche zurückkehren → unauffällig; Suchkontext bleibt erhalten.
- Suche ohne Standortberechtigung und ohne unfertige zusätzliche Navigationsziele verwenden → unauffällig.

## Prüfgrundlage und Grenzen

Fachliche Grundlage ausschließlich `requirement.md` dieses Features. Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Gegen diesen Branch geänderte und unversionierte UI-Dateien berücksichtigt. Kein Plan- oder anderes Reviewdokument gelesen.

Die vier Bilder `native-search.png`, `native-results.png`, `native-results-narrow.png` und `native-details.png` unter `docs/help/verbindungssuche/verification/` wurden visuell geprüft. Ergänzend wurden dort `native-live-2026-09-16.md` und `windows-uia-fixture-2026-09-16.txt` als Nachweise tatsächlich bedienter nativer Windows-Flüsse gelesen. Dieses unabhängige Review hat die App nicht erneut gestartet. Die native iOS-Abnahme verbleibt gemäß Anforderung beim Nutzer.

## Geprüfte Dateien

- `FlowNRW/SearchPage.cs`
- `FlowNRW/ResultsPage.cs`
- `FlowNRW/JourneyDetailPage.cs`
- `FlowNRW/AppShell.xaml`
- `FlowNRW/AppShell.xaml.cs`
- `FlowNRW/App.xaml.cs`
- `FlowNRW/ShellJourneyNavigation.cs`
- `FlowNRW/MauiProgram.cs`
- `FlowNRW/Resources/Styles/Colors.xaml`
- `FlowNRW.Core/Presentation/EndpointViewModel.cs`
- `FlowNRW.Core/Presentation/JourneySearchViewModel.cs`
- `FlowNRW.Core/Presentation/ResultsViewModel.cs`
- `FlowNRW.Core/Presentation/JourneyDetailViewModel.cs`
- `FlowNRW.Core/Presentation/JourneyPresentation.cs`
- `FlowNRW.Core/Presentation/CoordinateParser.cs`
- `FlowNRW.Core/Presentation/AsyncRelayCommand.cs`
- `FlowNRW.Core/Presentation/RelayCommand.cs`
- `FlowNRW.Core/Presentation/ObservableObject.cs`
- `FlowNRW.Core/Presentation/IJourneyNavigation.cs`
