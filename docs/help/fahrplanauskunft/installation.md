← [Zurück zur Übersicht](index.md)

# Fahrplanauskunft — Installation und Konfiguration

## Voraussetzungen

- .NET SDK 10.0 und die vorhandene Solution `FlowNRW.sln`.
- HTTPS-Erreichbarkeit der konfigurierten Provider.
- Für den VRR-OpenService gelten die dort beschriebenen Entwicklungs- und Nutzungsbedingungen. Produktiver öffentlicher Betrieb benötigt eine gesonderte Freigabe.

## Installation

1. Solution wiederherstellen und bauen: `dotnet build FlowNRW.sln -c Release`.
2. Die Schlüssel vor dem Erzeugen der Optionen über `builder.Configuration` zuführen; `MauiProgram.CreateMauiApp` liest sie mit den Präfixen `TransitProviders:`, `TransitHttp:`, `TransitProvider:` und `TransitCache:`. Ist ein Schlüssel nicht vorhanden, verwendet `MauiProgram` den unten dokumentierten Default.
3. Mit den Core-Tests und der dokumentierten Serviceprobe prüfen, ob Provider und Datenfelder erreichbar sind.

Im Repository ist keine produktive Geheimnis- oder Providerdatei enthalten. Die MAUI-Komposition liest die Werte aus der bereits vorhandenen Host-Konfiguration; konkrete Zuführungsquellen werden vom jeweiligen Host/Deployment bereitgestellt. `TransitProviderOptions.MaxSearchLength` hat aktuell nur den programmatischen Default `200` und wird nicht aus einem separaten Konfigurationsschlüssel gelesen. Die NRW-Grenzversion ist in der eingebetteten Ressource bzw. deren Dokumentation festgelegt und kein geladener Optionsschlüssel.

## Konfiguration

| Parameter | Typ | Standardwert | Beschreibung |
|-----------|-----|--------------|-------------|
| `TransitProviders:DbRest:BaseUrl` | URI | `https://v6.db.transport.rest/` | Bundesweiter Gateway für Suche, Nearby, Journeys und Departures. |
| `TransitProviders:Efa:BaseUrl` | URI | `https://openservice-test.vrr.de/openservice/` | EFA-Entwicklungsdienst für regionale Daten und Rückfälle. |
| `TransitProviders:Efa:FallbackBaseUrl` | optionale URI | leer | Alternativer EFA-Entwicklungsdienst mit gleicher Vertragsform. |
| `TransitProviders:Efa:Format` | Text | `rapidJSON` | Unterstütztes EFA-Ausgabeformat. |
| `TransitHttp:Timeout` | Dauer | `00:00:10` | Zeitlimit je HTTP-Versuch. Zulässig: größer 0 bis höchstens 1 Minute. |
| `TransitHttp:MaxRetries` | Ganzzahl | `1` | Zusätzliche Versuche; zulässig 0 bis 3. |
| `TransitProvider:MaxResults` | Ganzzahl | `100` | Maximale normalisierte Ergebnisse; zulässig 1 bis 100. |
| `TransitCache:MaxEntries` | Ganzzahl | `256` | Maximale Einträge im flüchtigen Speichercache; zulässig 1 bis 4096. |
| `TransitCache:StopTimeToLive` | Dauer | `1.00:00:00` | Frische für Haltestellen und Adressen. |
| `TransitCache:RealtimeTimeToLive` | Dauer | `00:00:30` | Frische für Verbindungen und Abfahrten. |
| `TransitCache:MaxStaleAge` | Dauer | `00:05:00` | Höchstalter eines als veraltet markierten Echtzeit-Rückfalls. |

Alle Endpunkte müssen absolute HTTPS-URIs ohne Benutzerinformationen, Query oder Fragment sein. Zugangsdaten werden nicht in dieser Dokumentation, im Quelltext oder in Logs hinterlegt.

## Überprüfung

Der aktuelle Nachweis steht unter [verification/iteration2-checks.md](verification/iteration2-checks.md). Er dokumentiert 98 erfolgreiche Core-Tests, 98,00 % Coverage, einen Windows-Solution-Build ohne Warnungen/Fehler und die Format-/XML-Prüfungen.
