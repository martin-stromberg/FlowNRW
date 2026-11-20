# Bestandsaufnahme: Fehler bei Verbindungsdetail (Issue #28)

Analysiert wurde die Wieder-Identifikation der ausgewählten Verbindung nach
automatischer Ergebnisaktualisierung sowie die Statusdarstellung der
Detailseite, bezogen auf die gemeldete Fehlermeldung und deren Doppelausgabe.

## Zusammenfassung

- `JourneySearchViewModel.RefreshIfStaleAsync` erneuert `Result` bei
  veralteten Daten (RealtimeTimeToLive ~30 s) und ordnet `SelectedJourney`
  per `SameJourney`/`SameEvent` neu zu.
- `SameEvent` verlangt für **jedes** Abschnittsereignis nicht-leere
  `Source`, `TripId`, `Stop.Source`, `Stop.Id` und exakt gleiche
  `PlannedTime`. Fußwegabschnitte (`Walking != null`, `TripId = null` bei
  beiden Mappern) sowie Endpunkte ohne Haltestellenkennung (Adressziele)
  können damit **nie** zugeordnet werden → Auswahl geht nach jeder
  Aktualisierung verloren, Status „…nicht mehr eindeutig bestätigt…".
- `ProviderOrchestrator` besitzt bereits das passende Zuordnungsmuster:
  `SameWalkingEvent` (geplante Zeit ≤ 60 s + `RealtimeConsolidator.SameStop`)
  für Fußwegabschnitte.
- Doppelausgabe: `JourneyDetailPage` zeigt `Session.Status` im
  `DetailStatus`-Label; `JourneyDetailViewModel.Details` liefert bei
  `SelectedJourney == null` denselben `Session.Status`, der zusätzlich als
  `JourneyDetailSection0`-Karte gerendert wird.

Test-Ausgangszustand: 300/300 Tests erfolgreich (siehe
[tests.md](inventory/tests.md)).

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Tests](inventory/tests.md)
