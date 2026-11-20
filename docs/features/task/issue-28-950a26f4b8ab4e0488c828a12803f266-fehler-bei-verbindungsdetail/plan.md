# Umsetzungsplan: Fehler bei Verbindungsdetail (Issue #28)

## Übersicht

Die Auswahl-Wiedererkennung nach einer automatischen Ergebnisaktualisierung
(`RefreshIfStaleAsync`) wird auf das bereits im `ProviderOrchestrator`
bewährte Zuordnungsverfahren umgestellt (inkl. Fußwegabschnitten und
Haltestellen ohne `Stop.Id`). Zusätzlich wird die doppelte Anzeige der
Statusmeldung auf der Detailseite beseitigt.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Ausgewählte `Journey` in `JourneySearchViewModel` bei `RefreshIfStaleAsync`; Statusdarstellung der `JourneyDetailPage` |
| Auslösende Ereignisse | `OpenJourneyAsync` → `OnAppearing`/`ResumeAsync` → `RefreshIfStaleAsync` bei veraltetem `Result` |
| Erwartete Wirkung | Auswahl bleibt bei eindeutiger Wiedererkennung erhalten (auch mit Fußwegen/Adress-Endpunkten); Statusmeldung erscheint höchstens einmal |
| Umgang mit fehlenden Objekten | Fußweg-/Adress-Ereignisse ohne `TripId`/`Stop.Id` werden über Haltestellenähnlichkeit + geplante Zeit zugeordnet; nicht eindeutige Verbindungen → Auswahl weiterhin konservativ aufgehoben |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Keine Änderung an Refresh-Intervallen/Lebensdauern | Nicht Gegenstand des Fehlerberichts | offen |
| 2 | Keine automatische Ersatzverbindung bei verlorener Auswahl | Bewusst konservatives Verhalten, bleibt erhalten | offen |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Verbindungs-Zuordnung (`SameJourney`) | Als `internal static` auf `RealtimeConsolidator` verlegen (verbatim aus `ProviderOrchestrator`, inkl. `SameWalkingEvent`); `ProviderOrchestrator` und `JourneySearchViewModel` nutzen denselben Matcher | Eine zentrale, bereits für MergeJourneys erprobte Zuordnungslogik statt zweier divergierender Implementierungen; `Matches` deckt neben Fußwegen auch Ereignisse ohne `TripId`/`Stop.Id` ab (Linie+Betreiber+Richtung-Fallback, `SameStop` via Dhid/Name+Koordinate) |
| `Details`-Fallback ohne Auswahl | Fester Platzhalter „Keine Verbindung ausgewählt." statt `Session.Status` | `Session.Status` wird bereits durch das `DetailStatus`-Label angezeigt; identischer Text als `JourneyDetailSection*`-Karte erzeugt die gemeldete Doppelausgabe |

## Programmabläufe

### Auswahl-Wiedererkennung nach Aktualisierung

1. `RefreshIfStaleAsync` erhält neues `Result` und zählt Kandidaten per
   `SameJourney(selected, item)`.
2. `SameJourney` (= `RealtimeConsolidator.SameJourney`): gleiche
   `Legs.Count` > 0; pro Abschnittspaar: abweichende `Walking`-Belegung →
   `false`; Fußwegabschnitt → `SameWalkingEvent` (beide `PlannedTime`
   vorhanden, Differenz ≤ 60 s, `SameStop`); Verkehrsabschnitt →
   `RealtimeConsolidator.Matches` für `Departure` und `Arrival`.
3. Genau eine Zuordnung in beiden Richtungen → `SelectedJourney` bleibt/
   wird ersetzt; sonst `null` + Statusmeldung (bestehendes Verhalten).

Beteiligte Klassen: `JourneySearchViewModel`, `RealtimeConsolidator`,
`ProviderOrchestrator`.

### Einmalige Statusanzeige

1. `SelectedJourney == null` → `RenderDetails` rendert `model.Details`.
2. `Details` liefert „Keine Verbindung ausgewählt." (statt `Session.Status`).
3. `DetailStatus`-Label zeigt `Session.Status` einmalig; keine
   duplizierte `JourneyDetailSection*`-Karte mehr.

Beteiligte Klassen: `JourneyDetailViewModel`, `JourneyDetailPage` (unverändert).

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `RealtimeConsolidator` (Logikklasse)

- **Neue Methoden:** `internal static bool SameJourney(Journey left, Journey right)` — Beinpaar-Abgleich mit Walking-Differenzierung (Logik verbatim aus `ProviderOrchestrator`); `private static bool SameWalkingEvent(StopEvent left, StopEvent right)` — `PlannedTime` ≤ 60 s + `SameStop`.

### `ProviderOrchestrator` (Logikklasse)

- **Geänderte Methoden:** Aufrufe des privaten `SameJourney` (`Unmatched(..., SameJourney)`, `EnrichJourney`) nutzen `RealtimeConsolidator.SameJourney`.
- **Entfernt:** private `SameJourney`, private `SameWalkingEvent` (in `RealtimeConsolidator` verlegt).

### `JourneySearchViewModel` (Logikklasse)

- **Geänderte Methoden:** `SameJourney` delegiert an `RealtimeConsolidator.SameJourney`.
- **Entfernt:** private `SameEvent` (durch `RealtimeConsolidator.Matches`-Semantik ersetzt — identische Strenge bei vorhandenen Kennungen, plus konservative Fallbacks).

### `JourneyDetailViewModel` (Logikklasse)

- **Geänderte Member:** `Details` — `SelectedJourney == null` liefert „Keine Verbindung ausgewählt." (kein `Session.Status`-Echo mehr).

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Zuordnungsstrenge:** `Matches` toleriert `PlannedTime`-Abweichungen bis
  60 s (statt exakter Gleichheit). Gleiche `TripId` wird weiterhin verlangt,
  sobald beide Seiten eine besitzen — keine falsch-positive Zuordnung
  anderer Fahrten; nur fehlertolerantere Wiedererkennung derselben Fahrt.
- **MergeJourneys:** Die verlegte Logik ist identisch; Verhalten der
  Provider-Konsolidierung unverändert (durch Bestandstests
  `ProviderOrchestratorTests_*` abgesichert).
- **Leere Detailsektion:** Bei `SelectedJourney == null` zeigt die
  Detailseite „Keine Verbindung ausgewählt." als Karte; der eigentliche
  Status verbleibt im `DetailStatus`-Label.

## Umsetzungsreihenfolge

1. **Tests zuerst (Red):** neue Fälle in `JourneyResumeTests_Selection`
   (Fußweg-Verbindung bleibt ausgewählt; Fußweg→Verkehr-Wechsel hebt
   Auswahl auf) und `Details`-Fallback-Test.
   - Voraussetzungen: Keine.
   - Beschreibung: Fixture-Journey um `Walking`-Abschnitt mit `TripId`-/
     `Stop.Id`-losem Ereignis erweitern; Tests müssen gegen alten Code
     fehlschlagen.
2. **`RealtimeConsolidator`:** `SameJourney` + `SameWalkingEvent` als
   `internal static`/`private static` aus `ProviderOrchestrator` übernehmen.
   - Voraussetzungen: Schritt 1.
   - Beschreibung: Code verbatim verlegen.
3. **`ProviderOrchestrator`:** private Matcher entfernen, Aufrufe auf
   `RealtimeConsolidator.SameJourney` umstellen.
   - Voraussetzungen: Schritt 2.
4. **`JourneySearchViewModel`:** `SameJourney` auf
   `RealtimeConsolidator.SameJourney` delegieren, `SameEvent` entfernen.
   - Voraussetzungen: Schritt 2.
5. **`JourneyDetailViewModel`:** `Details`-Fallback auf festen Platzhalter.
   - Voraussetzungen: Keine.
6. **Grün machen:** `dotnet test FlowNRW.Tests -c Release`,
   `dotnet build` (Core+Tests), `dotnet format --verify-no-changes` für
   geänderte Projekte.
   - Voraussetzungen: Schritte 1–5.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `RefreshIfStale_JourneyWithWalkingLeg_KeepsSelection` | `JourneyResumeTests_Selection` | Verbindung mit Fußwegabschnitt (`TripId`/`Stop.Id` leer) bleibt nach Refresh ausgewählt, Status „automatisch aktualisiert" |
| `RefreshIfStale_WalkingLegTurnedTransit_ClearsSelection` | `JourneyResumeTests_Selection` | Abschnitt, der im Refresh kein Fußweg mehr ist → Auswahl konservativ aufgehoben |
| `Details_NoSelection_DoesNotEchoStatus` (o. ä.) | neue `JourneyDetailViewModelTests` oder `PresentationTests` | `Details` liefert bei `SelectedJourney == null` „Keine Verbindung ausgewählt.", nicht `Session.Status` |
| `WalkingJourney`-Fixture-Helfer | `JourneyResumeTests_Selection` | `Journey` mit `Walking != null`-Abschnitt ohne `TripId`/`Stop.Id` |

### Betroffene bestehende Tests

Keine — bestehende Test-Fixtures verwenden vollständige Identitäten bzw.
Transitabschnitte; deren Zuordnung bleibt unter `Matches` unverändert.

### E2E-Tests (primärer Funktionsnachweis)

| Priorität | Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium | Warum E2E nötig ist |
|-----------|----------|------------------------|-------------------------------|-------------------|
| Bestehend | Resume mit Fußweg-Verbindung, eindeutig/verloren | `tests/WindowsJourneyUiTests/WindowsLifecycleUiTests.ps1` (`resume-route-stale`, `resume-route-missing`) | Auswahl erhalten / „nicht mehr eindeutig" | Nativer End-to-End-Fluss |
| Lücke | Kein automatisch ausführbarer neuer E2E-Test in dieser Umgebung (MAUI-Windows-Build + UIAutomation erforderlich); Fixture-Journey enthält bereits Fußwegabschnitt → bestehender Lifecycle-Lauf deckt das geänderte Matching ab | – | – | – |

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Auswahl mit Fußweg bleibt nach Refresh erhalten (Happy Path) | Nutzer öffnet Verbindungsdetail nach > RealtimeTimeToLive in der Ergebnisliste | Ergebnisliste mit Verbindung inkl. Fußwegabschnitt, Daten veraltet | `OpenJourneyAsync` → `ResumeAsync` → `RefreshIfStaleAsync` → eindeutige Zuordnung | Verbindungsdetails der gewählten Fahrt sichtbar, Status „…automatisch aktualisiert.", Kartenaktion aktiv | Unit-Test `…_KeepsSelection` |
| 2 | Mehrdeutige/verschwundene Verbindung | Refresh liefert keine oder mehrere Zuordnungen | wie #1 | wie #1, Zuordnung ≠ 1 | Status „…nicht mehr eindeutig bestätigt…" einmalig im `DetailStatus`-Label; `JourneyDetailSection0` zeigt „Keine Verbindung ausgewählt." | Unit-Tests `…_ClearsSelection`, `Details_…` |

## Offene Punkte

Keine.
