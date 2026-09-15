← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — Fehlerbehebung

## `primary-unavailable` oder `secondary-unavailable`

**Ursache:** Ein Provider ist nicht erreichbar, liefert HTTP 503, eine leere Antwort oder eine Warnung.

**Lösung:** Endpoint und HTTPS-Erreichbarkeit prüfen. Das Ergebnis kann als Fallback oder Stale-Daten vorliegen; die Quelle und Warnungen im `ProviderResult<T>` bleiben maßgeblich. Die dokumentierte db-rest-503-Grenze darf nicht durch Fixtures verschleiert werden.

## `region-unknown`

**Ursache:** Ein Name oder eine Stop-ID konnte nicht eindeutig mit Koordinate bzw. Anbieter-/DHID-Identität aufgelöst werden.

**Lösung:** Eine eindeutige Haltestelle aus der Suche verwenden. Unklare Regionen werden nicht stillschweigend als außerhalb NRW behandelt.

## `invalid-endpoint`

**Ursache:** Ein konfigurierter Endpoint ist nicht absolut, nicht HTTPS oder enthält Benutzerinformationen, Query oder Fragment.

**Lösung:** Nur sichere Basis-URIs ohne Geheimnisse in der URI konfigurieren.

## Unerwartete alte Ergebnisse

**Ursache:** Ein Aufruf wurde abgebrochen oder ein Ergebnis ist als Stale gekennzeichnet.

**Lösung:** `CancellationToken`, `IsFallback`, `IsStale`, `Warnings` und `RetrievedAt` auswerten. Stale-Echtzeit älter als fünf Minuten wird verworfen.

## Geheimnisse in Logs

Providerdiagnose darf nur Providername, Dauer, Status, Anzahl und technische Fehlercodes enthalten. Enthält ein Log Suchtext, Adresse, Koordinate, Antwortinhalt oder Zugangsdaten, ist dies ein Fehler der Diagnosekonfiguration und muss vor einer weiteren Verwendung behoben werden.
