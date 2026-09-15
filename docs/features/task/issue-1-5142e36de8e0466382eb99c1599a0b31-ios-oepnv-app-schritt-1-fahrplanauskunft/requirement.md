# Anforderungsübersetzung – konservative Ergänzung partieller Providerergebnisse

## Fachliche Zusammenfassung

Die bestehende Schritt-1-Datenversorgung muss partielle regionale Ergebnisse korrekt mit nationalen Ergebnissen ergänzen. Liefert der regionale Provider beispielsweise Fahrt A mit Warnung und der nationale Provider Fahrt A plus die regional fehlende Fahrt B, muss das Ergebnis A mit bevorzugter regionaler Echtzeit und zusätzlich B enthalten. Doppelte Ergebnisse und fremde Fahrten dürfen nicht entstehen; leere Antworten, Fehler und bestehende Fallbacks bleiben erhalten.

Die Korrektur betrifft Routing und Abfahrten. Die zusammengeführten Ergebnisse werden fachlich dedupliziert, zeitlich sinnvoll sortiert und auf `MaxResults` begrenzt. Es werden keine weiteren Provider, keine UI und keine neuen Live-Abrufe eingeführt.

## Betroffene Klassen und Komponenten

- `ProviderOrchestrator`: partielle primäre Ergebnisse erkennen, sekundäre Ergebnisse ergänzend an die Merge-Logik übergeben und Fallback-/Warnungsstatus erhalten.
- `MergeJourneys`: Vereinigungslogik für Verbindungen einschließlich zusätzlicher nationaler Fahrten, Duplikatvermeidung, eindeutiger Fahrtzuordnung, Sortierung und Ergebnislimit.
- `MergeEvents`: entsprechende Vereinigungslogik für Abfahrtsereignisse einschließlich regional bevorzugter Echtzeitwerte und zusätzlicher nationaler Ereignisse.
- `RealtimeConsolidator`: weiterhin konservative Stop-/Fahrtzuordnung; fremde oder nicht eindeutig zuordenbare Fahrten werden nicht vermischt.
- `TransitProviderOptions`: bestehendes `MaxResults` als Grenze der normalisierten Ergebnislisten verwenden; keine neue Konfiguration.
- Bestehende Tests in `FlowNRW.Tests`: deterministische Regressionen für Routing, Abfahrten, Identität, NRW-Priorität, partielle Mengen, Duplikate, Sortierung, Limit und Fallback.

## Implementierungsansatz

1. `ProviderOrchestrator` ruft bei regionaler Priorität beide bereits konfigurierten Provider gemäß bestehender Fehler-/Warnungslogik auf.
2. Bei einer partiellen regionalen Antwort werden die primären regionalen Elemente und die nationalen Elemente als Kandidatenmenge vereinigt. Die regionale Version erhält bei eindeutiger Zuordnung Vorrang für Echtzeitfelder.
3. `MergeJourneys` ordnet jede nationale Verbindung nur dann einer regionalen Verbindung zu, wenn die vorhandene Identitätslogik eine eindeutige Fahrt-/Haltestellenübereinstimmung belegt. Nicht zuordenbare nationale Fahrten bleiben als eigenständige Ergebnisse erhalten; bereits enthaltene gleiche Fahrten werden nicht dupliziert.
4. `MergeEvents` verfährt analog für Abfahrten: passende Ereignisse werden durch `RealtimeConsolidator` konsolidiert, zusätzliche eindeutige Ereignisse werden angehängt, fremde oder mehrdeutige Fahrten nicht gemischt.
5. Beide Mergepfade sortieren die Vereinigungsmenge nach dem relevanten kommenden Soll-/Istzeitpunkt und begrenzen sie auf `TransitProviderOptions.MaxResults`.
6. Leere Antworten, HTTP-/Transportfehler und Warnungen behalten die bisherige Fallbacksemantik. Eine nicht vorhandene oder nicht eindeutig zuordenbare regionale Echtzeit bleibt unbekannt bzw. wird nicht ergänzt.

Beteiligte Komponenten: `ProviderOrchestrator`, `RealtimeConsolidator`, `Journey`, `JourneyLeg`, `StopEvent`, `TripIdentity`, `ProviderResult<T>`, `TransitProviderOptions`.

## Konfiguration

Keine neue Konfiguration. Das bereits vorhandene `TransitProviderOptions.MaxResults` begrenzt die finalen Routing- und Abfahrtslisten. Bestehende Provider-, Cache-, Timeout-, Retry- und NRW-Konfiguration bleibt unverändert.

## Offene Fragen

Keine. Die fachliche Regel ist festgelegt: regionale Echtzeit wird bei eindeutiger Fahrt-/Haltestellenzuordnung bevorzugt, nationale Ergebnisse ergänzen fehlende regionale Ergebnisse, und Duplikate sowie fremde Fahrten werden ausgeschlossen.

## Abnahmekriterien

1. Eine partielle regionale Routingantwort mit Fahrt A und Warnung wird mit einer nationalen Antwort A+B zu einer Liste A+B vereinigt; A erscheint einmal und behält die regionale Echtzeitpriorität.
2. Eine partielle regionale Abfahrtsantwort wird analog um fehlende eindeutige nationale Abfahrten ergänzt; konsolidierte Ereignisse erscheinen einmal.
3. Unterschiedliche oder mehrdeutige Fahrtidentitäten werden nicht zusammengeführt; ihre Daten bleiben getrennt oder unbekannte Echtzeit bleibt unverändert.
4. Routing- und Abfahrtslisten sind nach kommenden Soll-/Istzeiten sortiert und überschreiten `MaxResults` nicht.
5. Leere, fehlerhafte und warnende Providerantworten behalten die bisherige nachvollziehbare Fallbacksemantik; es werden keine Ergebnisse erfunden.
6. Deterministische Rot-Grün-Regressionstests decken Routing, Abfahrten, Identität, NRW-Priorität, Ergänzung, Deduplizierung, Sortierung, Limit und bestehende Fallbacks ab. Die vorhandenen Windows-/Core-Test- und Releaseabläufe bleiben unverändert.
