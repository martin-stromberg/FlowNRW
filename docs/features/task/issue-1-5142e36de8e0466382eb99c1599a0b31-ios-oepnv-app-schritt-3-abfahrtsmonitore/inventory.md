# Gezielter Bestand – Abfahrtsmonitor

- `EndpointViewModel` kapselt Suchtext, vollständige Address-Treffer, Identitätsauswahl, Abbruch/Revision und Status. Eine eigene Instanz kann für Haltestellen wiederverwendet werden; die Monitoransicht muss Adresstreffer ohne Stop-ID ausschließen.
- `IDepartureService.DeparturesAsync(Stop, DateTimeOffset, CancellationToken)` und `DepartureService` sind bereits per DI verfügbar. Orchestrator übernimmt Region, gemeinsame Abrufe, Cache und Fallback. Keine neue Providerimplementierung nötig.
- `StopEvent` enthält Linie, Richtung über Identity, Sollzeit sowie tatsächliche Zeit, Delay, Ausfall und beide Bahnsteigfelder. Null bedeutet unbekannt.
- `JourneyPresentation.Time/Metadata`, `ObservableObject` und `AsyncRelayCommand` sind wiederverwendbar. Neue Seiten können vorhandene Styles/Schrift und expliziten Textumbruch nutzen.
- Shell enthält Suche → Ergebnisse → Details. Ein erreichbarer Einstieg zur Haltestellensuche und eine Monitorroute fehlen. Routing darf durch die Ergänzung nicht verändert werden.
- Native Windows-Testinfrastruktur mit isolierter UiTest-Konfiguration ist vorhanden; Fixture-Service und UIA-Harness lassen sich um Stop-/Departure-Flüsse erweitern. Ausgangsnachweis: 118 Tests, 97,01 % Core-Coverage, native Routingabläufe erfolgreich. iOS-Abnahme bleibt beim Nutzer. IIS ist gestrichen.

Lokale Bearbeitung gemäß Skill-Fallback: Die zuletzt delegierten Implementierungs-/Abnahmeagenten brachen am Nutzungslimit ab. Die getrennten Planungs-, Implementierungs- und Prüfphasen werden beibehalten.
