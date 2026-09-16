# Live-Routing vor Anbindung der Oberfläche

Ausgeführt am 15.09.2026 gegen die unveränderte Release-Assembly des abgenommenen Fahrplankerns. Ein temporäres .NET-10-Konsolenprogramm instanziierte `TransitHttpGateway`, beide realen Provider, `ProviderOrchestrator` und `RoutingService` wie die App. Keine Fixtures, Ergebnisgrenze 5, Standard-Zeitlimits und Standard-Endpunkte. Angefragt wurde jeweils die aktuelle UTC-Zeit. Die Probe betrifft öffentliche Beispielorte, keine Nutzerstandorte.

| Probe | Start UTC | Ergebnis |
|---|---|---|
| Berlin Hbf (52.5251, 13.3694) → Hamburg Hbf (53.5527, 10.0069) | 20:32:42.9979858 | Keine Verbindung; Quelle `db-rest`, Fehler `timeout`, Warnungen `provider_message`, `empty_response`, `timeout`, `provider_error`, `secondary-unavailable`. Kein Fallback-Ergebnis, kein Stale-Ergebnis. |
| Gelsenkirchen Hbf (51.504926, 7.102206) → Essen Hbf (51.451, 7.014) | 20:33:04.1055411 | 4 Verbindungen, Quelle `efa`, kein Fehler. Warnungen `provider_message`, `provider_information`, `timeout`, `secondary-unavailable`. Kein Stale-Ergebnis. |

Die erste NRW-Verbindung enthielt Fußweg → AST94 → 194 → S3 → Fußweg. Gelieferte Sollabfahrt des ersten Abschnitts: 20:50 UTC, Sollankunft des letzten Abschnitts: 21:32 UTC. Der regionale Dienst liefert somit aktuell nutzbare Daten trotz Ausfalls der ergänzenden nationalen Quelle. Die bundesweite Probe war in diesem Lauf nicht erfolgreich; daraus folgt keine bestätigte bundesweite Live-Verfügbarkeit.

Der Prozess endete mit Exit-Code 0, weil das Probeprogramm beide Providerergebnisse protokolliert und Providerfehler als Daten zurückgibt. Dieser Prozesscode bedeutet ausdrücklich nicht, dass beide Routen erfolgreich waren.

Die erste Ausführung scheiterte vor dem Abruf an gesperrtem Zugriff auf die Benutzer-`NuGet.Config`; die anschließend freigegebene Ausführung konnte beide Proben durchführen. Dies ist ein Service-Nachweis, kein nativer UI-Nachweis. Die reguläre App muss nach der Implementierung zusätzlich tatsächlich bedient werden.
