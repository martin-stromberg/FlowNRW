← [Zur Übersicht](index.md)

# Zustände, Navigation und Prüfung

`SearchPage`, `ResultsPage` und `JourneyDetailPage` verwenden die Core-Präsentationslogik. `MauiProgram` registriert `JourneySearchViewModel`, `ResultsViewModel`, `JourneyDetailViewModel`, `SearchPage` und `AppShell` als Singletons, Ergebnis-/Detailseiten transient. `IJourneyNavigation` wird durch `ShellJourneyNavigation` umgesetzt. Die gemeinsame Sitzung hält ausgewählte Adressen und Verbindung im Speicher; Routen transportieren keine Adressdaten.

Start und Ziel besitzen je ein `EndpointViewModel` mit eigener transienter `IStopSearchService`-Instanz. Text-/Koordinatenänderungen und Moduswechsel löschen Auswahl/Treffer und invalidieren das Routing. Abbruchtoken und steigende Revisionen verwerfen auch verspätete Antworten nicht kooperativer Dienste. Beim Verlassen der Suchseite werden laufende Abfragen abgebrochen; abgeschlossener Kontext bleibt erhalten.

`JourneySearchViewModel.SearchAsync` verlangt zwei ausgewählte Endpunkte und ruft `IRoutingService.RouteAsync` mit `DateTimeOffset.Now` auf. `ProviderResult` liefert Ergebnisse und Metadaten; die UI übernimmt die Core-Reihenfolge. Lade-, Leer-, Fehler-, Warn-, Fallback- und Stale-Zustände bleiben sichtbar. `JourneyPresentation` formatiert in `Europe/Berlin` einschließlich Datum/UTC-Offset; fehlende Echtzeit bleibt unbekannt. Details lesen die ausgewählte Fahrt aus derselben Sitzung.

Die [Providerarchitektur und Datenschutzgrenzen](../fahrplanauskunft/architektur.md) sowie [Konfiguration](../fahrplanauskunft/installation.md) gelten unverändert. Keine neue Datenbank oder dauerhafte Suchhistorie wird angelegt.

## Tests

```powershell
dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --collect:"XPlat Code Coverage"
dotnet build FlowNRW.sln -c Release -p:TreatWarningsAsErrors=true
```

Der [Prüfstand](verification/checks-2026-09-16.md) enthält 118 erfolgreiche Tests und 715/737 abgedeckte Core-Zeilen (97,01 %), einschließlich Präsentationslogik. Native Windows-Szenarien prüfen tatsächliche Eingabe, Zustandswechsel, Navigation und Darstellung. Ausführung: [UI-Harness](../../../tests/WindowsJourneyUiTests/README.md).

Nur `Configuration=UiTest` bindet `UiTestFixtureServices` und `UI_TEST_FIXTURES` ein; `bin/UiTest` und `obj/UiTest` trennen die Artefakte. Normale Debug-/Releasebuilds verwenden reale Services; kein Laufzeitschalter aktiviert Fixtures. [Native Live-Prüfung](verification/native-live-2026-09-16.md) ist getrennt vom Fixture-Nachweis: NRW und Berlin–Hamburg lieferten Verbindungen, letztere über EFA-Fallback. Das belegt weder dauerhafte Gatewayverfügbarkeit noch flächendeckende Versorgung. iOS und die spätere IIS-Paketprüfung sind damit nicht abgenommen.
