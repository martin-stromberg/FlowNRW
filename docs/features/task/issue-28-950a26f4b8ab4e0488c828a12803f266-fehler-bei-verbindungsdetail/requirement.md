# Übersetzte Anforderung: Fehler bei Verbindungsdetail (Issue #28)

## Fachliche Zusammenfassung

Beim Öffnen der Verbindungsdetailansicht nach einem erneuten Laden der
Verbindungsergebnisse (automatische Aktualisierung bei veralteten Daten)
verliert die Anwendung die zuvor vom Anwender gewählte Verbindung und zeigt
fälschlich den Status „Die gewählte Verbindung ist nicht mehr eindeutig
bestätigt." an — obwohl dieselbe Fahrt in der erneuerten Ergebnisliste
weiterhin eindeutig vorhanden ist. Zusätzlich wird diese Statusmeldung auf
der Detailseite doppelt ausgegeben.

## Fachlicher Scope

- **Verarbeitungseinheit:** Eine ausgewählte `Journey` innerhalb der
  geteilten Sitzung `JourneySearchViewModel` bei der Wieder-Identifikation
  nach `RefreshIfStaleAsync`, sowie die Statusdarstellung der
  `JourneyDetailPage`.
- **Auslösende Ereignisse:** Nutzer wählt eine Verbindung in der
  Ergebnisliste (`OpenJourneyAsync` → `JourneyDetailPage`); beim
  `OnAppearing`/`ResumeAsync` der Detailseite läuft `RefreshIfStaleAsync`,
  weil `RefreshFreshness.IsStale` nach Ablauf der Echtzeit-Lebensdauer
  (RealtimeTimeToLive, ~30–60 s Betrachtungsdauer) greift.
- **Erwartete Wirkung:** Die ausgewählte Verbindung bleibt nach der
  Aktualisierung ausgewählt und ihre Details werden angezeigt, sofern sie in
  der erneuerten Liste eindeutig wiedergefunden wird — einschließlich
  Verbindungen mit Fußweg-Abschnitten (`JourneyLeg.Walking != null`) und
  Endpunkten ohne Fahrt-/Haltestellenkennung (z. B. Adressziele). Die
  Statusmeldung wird höchstens einmal angezeigt.
- **Umgang mit fehlenden Objekten:** Fußweg-Abschnitte und
  Adress-Haltestellen ohne `TripId`/`Stop.Id` müssen über die ihnen
  verfügbaren Identitätsmerkmale (Haltestelle + geplante Zeit) zugeordnet
  werden — analog zur bestehenden Zuordnung im `ProviderOrchestrator`;
  nicht eindeutig zuordenbare Verbindungen führen weiterhin konservativ zur
  Aufhebung der Auswahl.
- **Scope-Ausschlüsse:** Keine Änderung am Refresh-Zeitverhalten
  (Intervalle, Lebensdauern), kein Stellen einer Ersatzverbindung, keine
  Änderung an der Ergebnislisten-Darstellung.

## Betroffene Klassen und Komponenten

- `FlowNRW.Core/Presentation/JourneySearchViewModel.cs` — `SameJourney`,
  `SameEvent` (Wieder-Identifikation der Auswahl nach Refresh)
- `FlowNRW.Core/Presentation/JourneyDetailViewModel.cs` — `Details`
  (Fallback liefert `Session.Status` → doppelte Anzeige mit `DetailStatus`)
- `FlowNRW/JourneyDetailPage.cs` — `DetailStatus`-Label +
  `JourneyDetailSection*`-Karten (Anzeige-Duplikat)
- `FlowNRW.Core/Transit/ProviderOrchestrator.cs` — Referenzmuster
  `SameJourney`/`SameWalkingEvent` (Fußweg-Zuordnung)
- `FlowNRW.Core/Transit/RealtimeConsolidator.cs` — `SameStop`/`Matches`
  (interne Vergleichshelfer)
- Tests: `FlowNRW.Tests/JourneyResumeTests_Selection.cs` (Refresh-/
  Auswahlverhalten), ggf. neue Testfälle für Fußweg-Verbindungen

## Implementierungsansatz

- `SameJourney` in `JourneySearchViewModel` erweitern: Abschnitte mit
  `Walking != null` über Haltestellenidentität (`RealtimeConsolidator.SameStop`)
  und geplante Zeit (Toleranz wie im Orchestrator, ≤ 60 s) zuordnen;
  Verkehrsabschnitte behalten die bestehende strikte
  `SameEvent`-Zuordnung. Unterschiedliche Walking-Belegung eines Abschnitts
  gilt als unterschiedliche Verbindung.
- Doppelanzeige beseitigen: `JourneyDetailViewModel.Details` liefert bei
  fehlender Auswahl nicht mehr `Session.Status` (wird bereits vom
  `DetailStatus`-Label angezeigt), sondern einen neutralen Platzhalter.

## Konfiguration

Keine Konfigurationsänderungen erforderlich.

## Offene Fragen

Keine.
