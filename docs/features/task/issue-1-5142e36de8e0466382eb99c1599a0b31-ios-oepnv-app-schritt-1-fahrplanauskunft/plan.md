# Umsetzungsplan: Partielle Providerergebnisse ergänzen

## Übersicht

`ProviderOrchestrator` soll regionale Teilantworten mit fehlenden nationalen Journeys und Abfahrten ergänzen. Die bestehende regionale Echtzeit bleibt bei eindeutiger Identität vorrangig; fremde oder mehrdeutige Fahrten werden getrennt gehalten. Fallback-, Fehler-, Cache- und Cancellation-Verträge sowie die vorhandenen Provider bleiben unverändert.

## Designentscheidungen

| Bereich | Gewählter Ansatz | Begründung |
|---|---|---|
| Journey-/Event-Union | Bestehende Merge-Methoden erweitern, keine neue öffentliche Klasse | Die Orchestrator-Verträge und `RealtimeConsolidator` decken den Ablauf bereits ab. |
| Identität | Eindeutige Übereinstimmung über Fahrtkennung oder vollständige Kombination aus Linie, Betreiber, Richtung, Sollzeit und Stop; DHID allein identifiziert nur den Stop | Verhindert Provider-ID-Verwechslungen und falsche Fahrt-Deduplizierung. Bei mehreren Kandidaten keine Zusammenführung. |
| Priorität | Regionales Element bleibt führend; nationale Elemente ohne eindeutigen Treffer werden angehängt | Bewahrt regionale Echtzeit und erhält zusätzliche nationale Fahrt B. |
| Ergebnisform | Legs, Transfers und Geometrie des jeweils übernommenen Journey-Objekts bleiben unverändert; danach Zeit-Sortierung und `MaxResults` | Keine fachlichen Daten werden durch eine neue Projektion verloren. |

## Programmabläufe

### Routing-Union

1. `ProviderOrchestrator.Execute` lädt wie bisher regionale Primär- und nationale Sekundärantwort.
2. `MergeJourneys` bildet Kandidaten aus beiden Listen und prüft jede Sekundär-Journey gegen die Primärmenge über alle relevanten Legs/Stops und Fahrtidentitäten.
3. Ein eindeutiger Treffer wird einmal als regionale Journey mit regionalen Echtzeitwerten ausgegeben; ein eindeutiger nationaler Nichttreffer bleibt als vollständige nationale Journey erhalten. Mehrdeutige Treffer werden nicht dedupliziert.
4. Die Union wird nach der relevanten kommenden Zeit stabil sortiert und auf `TransitProviderOptions.MaxResults` gekürzt. ProviderResult-Warnungen, Quelle und Fallbackstatus bleiben aus `Execute` erhalten.

### Abfahrts-Union

1. `MergeEvents` lässt passende Events durch `RealtimeConsolidator` konsolidieren.
2. Nicht zuordenbare, eindeutige nationale Events werden ergänzt; gleiche Events erscheinen nur einmal, bei Ambiguität bleibt die Echtzeit unbekannt beziehungsweise das Event getrennt.
3. Die Ausgabe wird stabil nach Soll-/Istzeit sortiert und auf `MaxResults` begrenzt.

Beteiligte Komponenten: `ProviderOrchestrator`, `RealtimeConsolidator`, `Journey`, `JourneyLeg`, `StopEvent`, `TripIdentity`, `TransitProviderOptions`.

## Neue Klassen

Keine. Vorhandene Contracts und private Mergepfade werden erweitert.

## Änderungen an bestehenden Klassen

### `ProviderOrchestrator` (Core-Service)

- **Geänderte Methoden:** `MergeJourneys` und `MergeEvents` führen eine konservative Union, Deduplizierung, Sortierung und Begrenzung aus.
- **Verträge:** `Execute` behält Warnungs-, Fehler-, Fallback-, Cache- und Cancellation-Semantik.

### `RealtimeConsolidator` (Fachlogik)

- **Geänderte Methoden:** Konsolidierung darf zusätzlich eindeutig zuordenbare sekundäre Events zurückgeben; mehrdeutige oder fremde Fahrten werden nicht verschmolzen.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine neuen Eingabefelder. Für die Union gilt: DHID allein reicht nicht als Fahrtidentität; Fahrtkennung oder die eindeutige Kombination aus Linie, Betreiber, Richtung, Sollzeit und Stop ist erforderlich. Nicht eindeutige Kandidaten werden nicht zusammengeführt.

## Konfigurationsänderungen

Keine. Das vorhandene `TransitProviderOptions.MaxResults` wird auf die finalen Routing- und Abfahrtslisten angewandt.

## Seiteneffekte und Risiken

- Bestehende regionale Prioritäts- und Fallbacktests müssen weiterhin dieselben Statusfelder und Warnungscodes sehen.
- Die Union darf Legs, Transfers und Geometrien nicht neu aufbauen; nur die Journey-/Event-Auswahl wird geändert.
- Zeitgleich eintreffende oder unvollständige Providerdaten können weiterhin unbekannte Echtzeit ergeben; das ist die konservative Vertragssemantik.

## Umsetzungsreihenfolge

1. **A/A+B-Regressionen für Routing und Abfahrten anlegen und rot belegen**
   - Voraussetzungen: `ProviderOrchestrator`, `RealtimeConsolidator`, Modelle, Testfakes und `MaxResults` sind vorhanden.
   - Beschreibung: Zuerst die minimalen Tests für regionale A plus nationale A/B schreiben und ihren erwarteten Fehlschlag gegen den aktuellen Mergepfad dokumentieren.
2. **Konservative Merge-Korrektur implementieren und grün belegen**
   - Voraussetzungen: Die roten Regressionen aus Schritt 1 liegen vor.
   - Beschreibung: Journey-/Event-Union, eindeutige Identität, Priorität, Erhalt der vollständigen Objekte, Sortierung und `MaxResults` umsetzen; die Regressionen müssen danach grün laufen.
3. **Restliche Identitäts-, Duplikat-, Limit-, Fehler- und Fallbackprüfungen ausführen**
   - Voraussetzungen: Implementierung aus Schritt 2 und grüne A/A+B-Regressionen.
   - Beschreibung: Mehrdeutige/fremde Fahrten, bestehende Fallbacks und Grenzfälle absichern; anschließend vorhandene Windows-/Releaseabläufe unverändert prüfen.
4. **Bestehende Fahrplanauskunft-Dokumentation aktualisieren**
   - Voraussetzungen: Implementierung und vollständige Tests aus den vorherigen Schritten.
   - Beschreibung: Unionregel, regionale Echtzeitpriorität und Grenzen in den bestehenden Help-Dokumenten sowie den vorhandenen README-/Release-Notes-Verweisen präzisieren.

## Tests

### Neue Tests

| Test | Testklasse | Prüfung |
|---|---|---|
| Regionale A + nationale A/B | `ProviderOrchestratorTests_Fallback` | A einmal mit regionaler Echtzeit, B zusätzlich |
| Abfahrten A + A/B | `ProviderOrchestratorTests_Fallback` | Event-Union und regionale Priorität |
| Fremde/mehrdeutige Fahrt | `RealtimeConsolidatorTests_Identity` | keine falsche Deduplizierung |
| Sortierung und `MaxResults` | `ProviderOrchestratorTests_Fallback` | stabile Zeitreihenfolge und Grenze |
| Fehler/leere Primärantwort | bestehende Fallbacktests | Warnungs- und Fallbacksemantik bleibt erhalten |

### Betroffene bestehende Tests

Keine Signaturänderung geplant. Bestehende regionale Prioritäts-, Fallback-, Identity- und Concurrency-Tests sind nach der Änderung auszuführen und nur bei geänderter erwarteter Ergebnisanzahl anzupassen.

### E2E-Tests (primärer Funktionsnachweis)

Keine. Schritt 1 liefert keinen UI-Fluss; die Änderung ist eine interne Core-Datenunion. Deterministische Core-Regressionen sind der passende Nachweis, bestehende Windows-/Releaseabläufe bleiben unverändert.

## Offene Punkte

Keine.
