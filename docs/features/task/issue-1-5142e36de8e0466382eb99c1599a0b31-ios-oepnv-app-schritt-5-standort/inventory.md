# Bestandsaufnahme: Standort und Umgebung

Stand: 17.09.2026. Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Schritte 1–4 sind integriert.

| Bestand | Wiederverwendung / notwendige Änderung |
|---|---|
| `FlowNRW.Core/Presentation/EndpointViewModel.cs` | Unabhängige Start-/Zielsuchen, `Changed`, Revision und Cancellation; `SelectAddress` akzeptiert nur aktuelle Treffer bzw. Koordinatenmodus. Standort braucht einen kontrollierten Übernahmeweg und Invalidierung bei manueller Änderung. |
| `JourneySearchViewModel`, `SearchPage` | Vorhandene Endpunktkarten, Status, Auswahl und Suchkommando bleiben. Je Endpunkt eine explizite Standortaktion, keine automatische Anfrage beim Seitenaufruf. |
| `StopMonitorViewModel`, `StopSearchPage` | `Stops` stammt ausschließlich aus `Lookup.Matches`; Monitor prüft vollständige aktuelle Kandidatenidentität. Umgebung muss in dieselbe aktive Kandidatenmenge eingehen; kein Bypass anhand Name oder beliebiger ID. Vorhandene Klartext-Auswahlbuttons und Zustandslabels verwenden. |
| `FlowNRW.Core/Maps/MapViewModel.cs` | `ShowStops` erfasst `monitor.Stops` samt Metadaten, `SelectAsync` schützt mit Session und Monitorvalidierung. Standortumgebung in denselben Snapshot und zugängliche Liste integrieren. |
| `IStopSearchService.NearbyAsync`, `StopSearchService` | Abbrechbarer vorhandener Providerabruf mit neuester Anfrage pro Serviceinstanz. Standortumgebung erhält eigenen Service-Scope, damit Start/Ziel und andere Suchen nicht gegenseitig abbrechen. |
| `NearbyStopResult` | Vollständiger `Stop` und optionale gelieferte Entfernung. Keine Entfernung erfinden; fehlende Koordinaten bleiben nur in der Listenalternative. |
| `GeoCoordinate`, `ProviderResult<T>` | Validierte WGS84-Koordinate und Quellen-/Zeit-/Fallbackinformation wiederverwenden. Keine neue Persistenz nötig. |
| MAUI / Plattformdateien | MAUI liefert Geolocation/Permissions bereits. iOS-Beschreibung fehlt; Windows läuft unpackaged win-x64. Keine zusätzliche Bibliothek oder Packagingumstellung erforderlich. |
| `tests/WindowsJourneyUiTests` | Native UIA-Flüsse und separater `UiTest`-Build mit Fixture-DI vorhanden. Standort-Fixtures nur dort; Release nutzt den echten Adapter. Routing-/Monitor-/Kartenprüfungen gezielt wiederholen. |

Die offizielle [Geolocation-Dokumentation](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/geolocation?view=net-maui-10.0) wurde am 17.09.2026 geprüft: `GetLocationAsync(request, token)` liefert eine aktuelle, abbrechbare Einzelabfrage und kann `null` liefern; iOS benötigt `NSLocationWhenInUseUsageDescription`, Windows laut Dokumentation keine zusätzliche Einrichtung. Die [Permissions-Dokumentation](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/appmodel/permissions?view=net-maui-10.0) verlangt eine passende Plattformdeklaration und behandelt Statusprüfung sowie Anforderung. Dialoganforderungen gehören in den UI-Kontext nach expliziter Nutzeraktion. Kein `LocationAlways`, kein Listening, keine LastKnown-Position als aktuelle Position.

Root prüft separat die tatsächlichen Windows-Betriebssystemvoraussetzungen. Eine nicht verfügbare Position dieser Maschine ist ein zu dokumentierendes Testergebnis, keine Produktannahme und kein Anlass für erfundene Koordinaten.
`docs/help/standort/verification/windows-preflight.md` dokumentiert die getrennte Windows-Vorprüfung: lfsvc läuft; keine Berechtigungen geändert und keine Position abgefragt.
