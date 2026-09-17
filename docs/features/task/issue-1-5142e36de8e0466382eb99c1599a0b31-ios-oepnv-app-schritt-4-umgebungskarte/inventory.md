# Bestandsaufnahme – Umgebungskarte

Stand: 17.09.2026. Schritte 2 und 3 sind auf der Projektbasis abgeschlossen. Aktueller Schritt ergänzt Karten und verändert keine Standortberechtigungen.

| Bereich | Vorhanden | Gezielte Ergänzung |
|---|---|---|
| Fachmodelle | `Stop.Coordinate`, `Address`, `GeoCoordinate`; `JourneyLeg.Geometry` und `WalkingSegment.Geometry` enthalten geordnete WGS84-Punkte. | Kartenprojektion vorhandener Punkte; keine erfundene Gesamtgeometrie und keine Modellmigration. |
| Dienste | `IStopSearchService.SearchAsync` und `NearbyAsync`; getrennte Suchinstanzen, abbrechbare Provider/Cache/Fallback. | Manuelle Haltestellensuche wiederverwenden; keine neue Provideranbindung erforderlich. |
| Stopps/Monitor | `StopSearchPage` mit Klartexttreffern, `StopMonitorViewModel.Lookup`, `Stops`, `OpenAsync(Address)`. Letzteres prüft Kandidatenmitgliedschaft. | Karte erhält denselben vollständigen Kandidatensatz; Marker und native Liste übergeben dieselbe Kandidateninstanz. |
| Verbindung | `JourneyDetailViewModel.Session.SelectedJourney` und `Session.Result` liefern Auswahl und Herkunft. `JourneyDetailPage` erzeugt bisher Textabschnitte. | Aktion zur Karte der ausgewählten Verbindung, getrennte Linien-/Fußwegsegmente und fehlende Geometrie. |
| Plattform | MAUI 10, Windows Release/UiTest, iOS-Plattformdateien, Shell/DI, native Buttons/Listen. | Gemeinsame eingebettete WebView-Karte plus native Navigation/Listen, keine neuen NuGet-Pakete. |
| Tests | Core/xUnit und `WindowsJourneyUiTests.ps1`, UiTest-Fixtures, native Monitor-/Routingabläufe. | Kartenszenarien, echte Pointer-/Tastaturbedienung im Windows-Appfenster und getrennte Live-Kartenprobe. |
| Fehlend | Keine Kartenassets, Kartenkonfiguration, Kachelzustände oder Kartenansicht vorhanden. | Lokale Leaflet-Assets samt Lizenz, sichere Konfiguration, Zustandsmodell und begrenztes Laden sichtbarer Kacheln. |

Vergleichbare UI-Muster sind die beschrifteten Suchfelder, mehrzeiligen Trefferbuttons, Status-/Metadatenlabels und Shell-Rücknavigation von `StopSearchPage`, `ResultsPage` und `DeparturePage`. Diese Muster weiterverwenden; keine Eingabe interner Stop-IDs.
