# Umsetzungsplan: Manuelle Verbindungssuche mit NRW-Echtzeit

## Übersicht

Die MAUI-Vorlage erhält eine bedienbare MVVM-Verbindungssuche mit Trefferwahl, Ergebnisliste und Details über die bestehenden realen Services. Windows bleibt build- und prüfbar; iOS erhält die Plattformbasis und eine manuelle Nutzerprüfanleitung. Der vorhandene Provider-Core bleibt fachliche Quelle für Routing, NRW-Echtzeit, Fallback und Cache.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|---|---|---|
| MVVM | Plattformunabhängige ViewModels unter `FlowNRW.Core/Presentation`, MAUI-Views in `FlowNRW`; eigene kleine `ObservableObject`-/Command-Basis ohne zusätzliches MVVM-Paket | Bestehende Core-Tests können Eingabe und Zustände deterministisch prüfen; MAUI bleibt Präsentation. |
| Endpunktwahl | Zwei `EndpointViewModel`-Instanzen mit je eigener transienter `IStopSearchService`-Instanz; Klartextsuche mit expliziter Trefferliste sowie Koordinatenmodus mit zwei beschrifteten Zahlenfeldern | Verhindert gegenseitigen Abbruch; Endanwender müssen keine IDs eingeben. Vollständiges `Address` bleibt erhalten. |
| Kontext und Navigation | Ein `JourneySearchViewModel` als Sitzungszustand; `ResultsViewModel`/`JourneyDetailViewModel` lesen dieselbe Sitzung, `IJourneyNavigation` kapselt Shell | Rücknavigation erhält Eingaben, Auswahl, Ergebnisse und Metadaten ohne Adressen in Routenparametern. |
| Veraltete Antworten | Abbruchtoken plus monoton steigende Anfragerevision pro Endpunkt und Routing | Auch nicht kooperative Services dürfen nach Textänderung, neuer Suche oder Verlassen keine Zustände überschreiben. |
| UI-Muster | Vorhandene MAUI-Styles/Schriften wiederverwenden, Karten/Blau/16-Punkt-Abstände und Statusdarstellung aus den HTML-/MD-Referenzen der Bestandsaufnahme | Es gibt keine vorhandene fachliche Lookup-UI; festgelegt sind beschriftete Suche, lesbare Treffer und explizite Auswahl. |
| E2E | Separater nativer Windows-Testtreiber in Windows PowerShell 5.1 mit UIAutomationClient/UIAutomationTypes; Fixture-DI ausschließlich in separat erzeugtem Testbuild mit getrenntem Ausgabe-/Zwischenverzeichnis | Reproduzierbare UI-Fehlerfälle, keine Fixture-Aktivierung im ausgelieferten Build; reguläre DI verwendet immer reale Services. |
| Plattform | Windows-Target auf Windows, iOS-Target auf macOS bzw. explizit angefordert; Windows-Eigenschaften konditionieren | Bestehende Windows-/Linux-CI benötigt keinen Mac, iOS bleibt in Visual Studio mit Mac ausführbar. |

## Programmabläufe

### Endpunkt erfassen und auswählen

1. `EndpointViewModel` nimmt Modus und Suchtext beziehungsweise Breite/Länge entgegen. Jede Änderung invalidiert `SelectedAddress`, leert alte Treffer, erhöht die Revision, bricht laufende Suche ab und meldet der Sitzung die Änderung.
2. `SearchAsync` validiert Text gemäß bestehenden Grenzen und setzt Ladezustand. Die eigene `IStopSearchService.SearchAsync` liefert Treffer und sämtliche `ProviderResult<Address>`-Metadaten.
3. Nur die weiterhin aktuelle Revision darf Treffer, Leerzustand oder verständlichen Fehler setzen. Abbruch ist kein Providerfehler. Mehrdeutige Treffer erscheinen mit Name und verfügbaren fachlichen Zusatzdaten; `SelectAddress` übernimmt das vollständige Objekt samt Stop-ID/Quelle/DHID/Koordinate.
4. Im Koordinatenmodus validiert `CoordinateParser.TryParse` getrennte Zahlenfelder mit deutscher bzw. invarianten Dezimalschreibweise; gültige Werte werden als `Address` übernommen. `NearbyAsync` ist nicht nötig.

Beteiligt: `EndpointViewModel`, `CoordinateParser`, `JourneySearchViewModel`, `IStopSearchService`.

### Verbindung suchen

1. `JourneySearchViewModel.SearchAsync` erlaubt Routing erst bei zwei gültigen ausgewählten Endpunkten. Jede Endpunktänderung invalidiert vorherige Route und gewählte Fahrt und bricht deren Abruf ab.
2. Eine neue Anfrage erfasst Endpunkte, aktuelle Uhrzeit als `DateTimeOffset` und Revision, setzt Ladezustand und ruft `IRoutingService.RouteAsync` auf. Keine zweite UI-Sortierung verändert die Core-Reihenfolge.
3. Nur die aktuelle Anfrage übernimmt Ergebnis und Metadaten. Erfolgreiche Suche navigiert zur Ergebnisansicht; leere/fehlerhafte Antworten erscheinen nachvollziehbar mit erneut ausführbarer Suche.
4. `ProviderOrchestrator` übernimmt NRW-Priorität, nationale Ergänzung und konservative Identitätszuordnung unverändert. Warnungen, Fallback, Quelle, Datenalter und Stale bleiben sichtbar, ohne personenbezogene Diagnose.

Beteiligt: `JourneySearchViewModel`, `ResultsViewModel`, `IRoutingService`, `IJourneyNavigation`.

### Details und Rücknavigation

1. `ResultsViewModel.OpenJourneyAsync` setzt die gewählte vorhandene `Journey` und navigiert zum Detail.
2. `JourneyDetailViewModel` stellt Teilstrecken, Umstiege, Fußwege, Linien, Betreiber, Haltestellen, Soll-/Istzeiten und Ausfall dar. `JourneyPresentation` formatiert Datum einschließlich Tageswechsel und expliziter Zeitzone/Offset; unbekannte Werte bleiben als unbekannt erkennbar.
3. Zurück führt über Ergebnisse zur Suche mit erhaltenem Sitzungskontext. Verlassen einer Seite bricht ihre noch laufenden Anfragen ab und erhöht Revisionen, ohne abgeschlossene Auswahl oder Ergebnisse zu löschen. Erneute Eingabe invalidiert diese fachlich abhängigen Ergebnisse.

Beteiligt: `ResultsViewModel`, `JourneyDetailViewModel`, `JourneyPresentation`, `ShellJourneyNavigation`.

## Neue Klassen

| Klasse | Typ | Zweck |
|---|---|---|
| `ObservableObject` | Basisklasse | `INotifyPropertyChanged` für plattformunabhängige ViewModels. |
| `RelayCommand`, `AsyncRelayCommand` | Command-Klassen | Auswahl und abbrechbare asynchrone Aktionen mit aktuellem `CanExecute`, beobachteten Fehlern und ohne Doppelstart. |
| `CoordinateParser` | Hilfsklasse | Validierte Koordinateneingabe ohne UI-Abhängigkeit. |
| `EndpointViewModel` | ViewModel | Text/Koordinatenmodus, Treffer, Auswahl, Revision, Metadaten und Zustände eines Feldes. |
| `JourneySearchViewModel` | ViewModel/Sitzung | Zwei Endpunkte, Routing, Ergebnisse und Navigation. |
| `ResultsViewModel` | ViewModel | Ergebnisliste, Status und Fahrtwahl aus Sitzung. |
| `JourneyDetailViewModel` | ViewModel | Gewählte Fahrt und Details aus Sitzung. |
| `JourneyPresentation` | Präsentationshelfer | Gemeinsame lesbare Zeit-, Linien-, Betreiber- und Statusdarstellung. |
| `IJourneyNavigation` | Interface | `ShowResultsAsync` und `ShowDetailAsync`; Rücknavigation bleibt Shell. |
| `ShellJourneyNavigation` | MAUI-Adapter | Umsetzung des Navigationsvertrags. |
| `SearchPage`, `ResultsPage`, `JourneyDetailPage` | MAUI-Views mit Code-behind | Bindings, zugängliche Darstellung und Lebenszyklus; keine Providerlogik. |
| `AppDelegate`, `Program` | iOS-Einstieg | MAUI-Start unter iOS. |
| `UiTestFixtureServices` | Nur Testbuild-Klasse | Deterministische Such-/Routingantworten inklusive Verzögerung und Fehlerzuständen. |
| `WindowsJourneyUiTests` | E2E-Testtreiber | Start und Bedienung der nativen App über Windows UI Automation. |

## Änderungen an bestehenden Klassen

### `MauiProgram` (Komposition)

- `CreateMauiApp`: Views, ViewModels und Navigation registrieren; zwei getrennte Suchservice-Instanzen in die Sitzung injizieren; reale Providerregistrierung behalten. Test-DI nur per Buildsymbol im isolierten Testbuild kompilieren.
- Keine neuen fachlichen Service-Contracts, Providerfelder oder Provider-Events.

### `App`, `AppShell` (MAUI)

- Konstruktoren und `CreateWindow`: DI-komponierte Shell verwenden.
- Shell-Routen auf Suche, Ergebnisse und Details begrenzen; keine unfertigen Ziele.
- Seiten-Lebenszyklus ruft Abbruch/Revisionserhöhung der jeweils laufenden Aktion auf.

### `MainPage`, `ClickCounter` (Vorlage)

- `MainPage`/Code-behind und `ClickCounter` samt Counter-Test entfernen; `SearchPage` wird Einstieg.

### Projekt und Ressourcen

- `FlowNRW.csproj`: iOS-Target und bedingte Windows-Eigenschaften; iOS-`Info.plist`/Einstieg und nötige Assets ergänzen, keine Standortberechtigung.
- `App.xaml`/Styles: blaue Interaktion, Karten, skalierbare Texte, ausreichende Kontraste/Touch-Ziele und textliche Statuskennzeichnung; stabile `AutomationId` an allen E2E-Bedienelementen.
- Neuer Windows-Testtreiber außerhalb der plattformneutralen Core-Testausführung; keine neue Deploymentautomatisierung.

## Datenbankmigrationen

Keine. Keine Suchhistorie oder neue Persistenz.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---|---|---|
| Suchtext | Nicht leer, bestehende `MaxSearchLength` einhalten | Verständliche Meldung, kein Abruf. |
| Koordinate | Beide Zahlen endlich, Breite -90..90, Länge -180..180 | Feldbezogene Meldung, keine ausgewählte Adresse. |
| Start/Ziel | Jeweils vollständiger gewählter Treffer oder gültiges Koordinatenobjekt | Suche gesperrt; fehlende Auswahl kenntlich. |
| Textänderung | Alte Identität sofort ungültig | Keine Route mit alter Auswahl. |
| Async-Antwort | Revision und aktuelle Sitzung müssen übereinstimmen | Veraltete Antwort verwerfen. |
| Unbekannte Echtzeit | Null nicht als pünktlich, null Ausfall nicht als bestätigt fahrend darstellen | Text „keine Echtzeitdaten“ bzw. unbekannt. |

## Konfigurationsänderungen

Keine neuen Laufzeitkonfigurationen. Vorhandene sichere Provider-/Cachegrenzen bleiben erhalten. Nur Buildkonfiguration für iOS und expliziten isolierten E2E-Testbuild; keine auslieferbare Fixture-Umschaltung und keine Standortfreigabe.

## Seiteneffekte und Risiken

- **Service-Lebensdauer:** Gemeinsamer `StopSearchService` würde Felder gegenseitig abbrechen; getrennte Instanzen sind Pflicht.
- **Navigation:** Revisionsschutz und Erhalt abgeschlossener Zustände müssen zusammen geprüft werden.
- **Provider:** Liveverfügbarkeit/Freigaben sind externe Grenzen; Fixtures belegen keine Liveversorgung. Bestehende Identitäts-/Uniontests bleiben verbindlich.
- **CI:** Konditionierte Plattformtargets verhindern zusätzliche iOS-Workloadpflicht auf Windows/Linux. Bestehende Format-, Security-, Windows-Build-, Core-Coverage- und Releasechecks bleiben bestehen.
- **Tests:** Counter-Test entfällt fachlich, Transit-Tests sollen unverändert bleiben; neue Präsentationslogik zählt zur Core-Coverage, Mindestwert 70 % bleibt.
- **Plattform:** Native Windows-UIA-Ausführung ist durch den aktuellen Preflight nachgewiesen: Release-App gestartet, Fenster gefunden, Counter per InvokePattern bedient und sichtbare Textänderung geprüft. iOS-Abnahme durch Nutzer ist dokumentierte Prüfaufteilung, kein lokaler Abschlussblocker.

## Umsetzungsreihenfolge

1. **MVVM-Basis und Parser:** Voraussetzungen: vorhandenes `FlowNRW.Core`, .NET- und Testprojekte. Kleine Observable-/Command-Basis, Parser und Navigationsvertrag erstellen; keine neuen Produktpakete nötig.
2. **Endpunkt- und Sitzungszustand:** Voraussetzungen: Schritt 1, bestehende Modelle und Serviceinterfaces. Endpunkt-, Such-, Ergebnis-/Detail-ViewModels und Präsentationshelfer mit Abbruch/Revision und Metadaten implementieren; deterministische Tests ergänzen.
3. **MAUI-Oberfläche und DI:** Voraussetzungen: Schritt 2, bestehende MAUI-Ressourcen und Shell. Views, Bindings, AutomationIds, Navigation und getrennte Serviceinstanzen ergänzen; Vorlage/Counter entfernen. HTML-/MD-Muster der Bestandsaufnahme verbindlich wiederverwenden.
4. **iOS-Basis und Windows-Kompatibilität:** Voraussetzungen: vorhandenes MAUI-Projekt und Schritt 3. iOS-Dateien/Target ergänzen, Windows-Eigenschaften konditionieren, Standortberechtigungen ausschließen; expliziten Windows-Build durchführen.
5. **Native E2E-Infrastruktur:** Voraussetzungen: Schritt 3/4, Windows-Desktop/App-Runtime, Windows PowerShell 5.1 und die vorhandenen UIAutomation-Assemblies. Separaten PowerShell-UIA-Harness und ausschließlich isolierten Fixture-Testbuild einrichten; den bereits erfolgreichen Prozessstart/UIA-Preflight auf die neuen Seiten übertragen.
6. **Nachweise:** Voraussetzungen: Schritt 5 und deterministische Tests. Unten genannte E2E-Flüsse tatsächlich bedienen, Live-Proben mit regulärem Build separat durchführen; Format, Coretests/Coverage, Windows-Warnings-as-errors und relevante bestehende Releasechecks ausführen. Nicht ausführbare UI-Flüsse mit konkreten Versuchen dokumentieren.
7. **Dokumentation und Projektübergabe:** Voraussetzungen: Nachweise aus Schritt 6. Hilfe/README/Release Notes, sichere Konfiguration, Datenschutz-/Providergrenzen und Visual-Studio-/iOS-Prüfung aktualisieren. Geprüften Windows-ZIP mit Version/Commit/Startanleitung vorbereiten. Erst nach fachlicher Projektabnahme in autorisierter IIS-Site „ÖPNV“ bereitstellen; Site/Zielpfad zuvor prüfen, Download und lokalen Start nachweisen. Keine neue Deploymentautomatisierung.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|---|---|---|
| `ParsesValidCoordinates`, `RejectsInvalidCoordinates` | `CoordinateParserTests` | Grenzen, leere Werte, NaN/Infinity, deutsche/invariante Dezimalzahlen. |
| `TextChangeInvalidatesSelection`, `PreservesSelectedIdentity` | `EndpointViewModelTests` | Vollständige `Address`-Identität und sofortige Invalidierung. |
| `IgnoresLateResponse`, `SearchesEndpointsIndependently` | `EndpointViewModelTests` | Verzögerte nicht kooperative Antwort und getrennte Serviceinstanzen. |
| `RepresentsEmptyErrorAndMetadata` | `EndpointViewModelTests` | Lade-/Leer-/Fehler-/Warnzustände. |
| `RequiresTwoSelections`, `EndpointChangeInvalidatesRoute`, `IgnoresOldRoute` | `JourneySearchViewModelTests` | Kein Routing mit unaufgelösten Eingaben; Token plus Revision. |
| `PreservesContextThroughNavigation` | `JourneySearchViewModelTests` | Eingaben, Identität, Ergebnisse und Detailauswahl bei Rücknavigation. |
| `DisplaysMidnightOffsetAndUnknownRealtime` | `JourneyPresentationTests` | Tageswechsel, Offset, fehlende Zeiten, Ausfall, Betreiber/Fußwege/Umstiege. |
| `ControlledSearchService`, `ControlledRoutingService`, `RecordingNavigation` | Testhilfen | Kontrollierte Verzögerung, Fehler, Metadaten und Navigationsbeobachtung. |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|---|---|
| `ClickCounter`-Tests | Entfallen mit der fachlich entfernten Vorlage. |
| Bestehende Transit-Tests | Keine Signaturänderung vorgesehen. Unverändert ausführen; vorhandene NRW-Endpunkt-, Outside-NRW-, Identitäts-, Union-, Warnungs-/Fallbackfälle auf AK 4 abgleichen und nur echte Lücken ergänzen. |

### E2E-Tests (primärer Funktionsnachweis)

Alle Szenarien liegen in `WindowsJourneyUiTests`; jeder Lauf dokumentiert Buildmodus, Befehl, Ergebnis und gegebenenfalls Fehler/UIA-Nachweis. Fixtures werden ausschließlich im isolierten Testbuild benutzt.

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|---|---|---|---|---|
| Pflicht | Native App starten; Suchseite ohne Counter/Standortdialog und ohne unfertige Menüziele bedienen | `WindowsJourneyUiTests.Start` | 1, 2 | Belegt tatsächlichen Einstieg und Plattformbindung. |
| Pflicht | Adresse als Start und Adresse als Ziel tippen, mehrdeutige Treffer wählen, suchen | `WindowsJourneyUiTests.AddressEndpoints` | 2, 5 | Tippen, Trefferliste und Auswahlbindung. |
| Pflicht | Haltestelle als Start und Ziel suchen/auswählen, Route starten | `WindowsJourneyUiTests.StopEndpoints` | 2, 5 | Fachliche Identität über native Auswahl. |
| Pflicht | Koordinatenmodus in beiden Feldern wählen, gültige Breite/Länge eingeben und suchen | `WindowsJourneyUiTests.CoordinateEndpoints` | 2, 5 | Moduswechsel und beide Zahlenfelder. |
| Pflicht | Leere/ungültige Koordinaten und ungewählte Texte eingeben; Meldungen/Suchfreigabe prüfen; anschließend korrigieren | `WindowsJourneyUiTests.Validation` | 2, 5 | UI-Validierung und Erholung. |
| Pflicht | Suche langsam beantworten; Feld ändern, alte Auswahl verlieren, neue Treffer wählen; verspätete Antwort darf nichts überschreiben; Start-/Zielsuche parallel | `WindowsJourneyUiTests.LatestInputWins` | 2, 5 | Reale Ereignisreihenfolge und Zustandsbindung. |
| Pflicht | Such- und Routingladezustand, leere Treffer/Verbindungen und Providerfehler auslösen; erneut erfolgreich suchen | `WindowsJourneyUiTests.EmptyAndErrorRecovery` | 2, 3, 5 | Sichtbare Statusmeldungen und Wiederbedienbarkeit. |
| Pflicht | Geordnete Verbindungen mit Umstieg/Fußweg/Linie/Betreiber öffnen; Detail mit Soll/Ist, Tageswechsel/Offset, Ausfall und unbekannter Echtzeit prüfen | `WindowsJourneyUiTests.ResultsAndDetails` | 3, 5 | Native Darstellung und Detailbindung. |
| Pflicht | Detail → Ergebnisse → Suche zurück; identische Auswahl/Ergebnisse prüfen; Endpunkt ändern und alte Route invalidieren | `WindowsJourneyUiTests.BackNavigation` | 2, 3, 5 | Shell-Stapel und Sitzungszustand. |
| Pflicht | Providerwarnung, Fallback, Stale, Quelle und Datenalter in Such-/Ergebnisdarstellung prüfen | `WindowsJourneyUiTests.ProviderStatus` | 3, 4, 5 | Lesbare statt nur farbliche Metadaten. |
| Pflicht | Mit regulärer realer DI bundesweite sowie NRW-Route suchen und Ergebnis/Fehler dokumentieren | `WindowsJourneyUiTests.LiveSmoke` oder protokollierte native Bedienung | 4, 5 | Reale Komposition; unabhängig von Fixtures, Datum/Ergebnis/Grenzen festhalten. |
| Pflicht | Erste Routingantwort verzögern und Abbruch ignorieren lassen; während des Abrufs Endpunkt ändern, neu auswählen und zweite Route starten. Nach Eintreffen beider Antworten bleiben ausschließlich zweite Route und deren Metadaten sichtbar; alte Antwort überschreibt nichts und navigiert nicht unerwartet. | `WindowsJourneyUiTests.LatestRouteWins` | 2, 5 | Prüft Routingrevision, sichtbare Ergebnisse und Navigation im tatsächlichen UI-Ablauf. |
| Pflicht nach fachlicher Projektabnahme | Vorbereiteten ZIP nach geprüfter IIS-Site-/Pfadzuordnung über Downloadadresse beziehen, in separates Verzeichnis entpacken, App gemäß Startanleitung starten und Suchseite prüfen. Commit/Version sowie Umfang-/Grenzendokumentation mit freigegebenem Stand vergleichen; Datum, URL/Zielpfad, Ergebnis und Startnachweis dokumentieren. | `WindowsPackageDeliveryCheck` (protokollierter Download-/Starttest) | 6 | Belegt die tatsächlich ausgelieferte Datei und den nativen Start des heruntergeladenen Pakets. |

Bestehende E2E-Tests: Keine. Bei technischer Blockade zuerst echten Prozessstart und UIA-Zugriff versuchen; Startbefehl, Runtime-/UIA-Fehler und jeden nicht ausgeführten Fluss dokumentieren, niemals ViewModel-Tests als UI-Pass ausgeben. iOS-Prüfanleitung enthält dieselben manuellen Kernflüsse sowie Build/Simulator/Gerät, Touch, Textskalierung, Navigation und Berechtigungsfreiheit; Ausführung übernimmt der Nutzer.

## Offene Punkte

Keine. Providerverfügbarkeit und native Laufzeitgrenzen werden durch Ausführung ermittelt und dokumentiert; es bestehen keine offenen fachlichen Entscheidungen.


