# Interfaces

## `IRefreshSettingsStore`
Datei: `FlowNRW.Core/Refresh/IRefreshSettingsStore.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `LoadAsync` | `CancellationToken cancellationToken = default` | `Task<int?>` | Lädt gespeicherte Sekunden; `null` steht für fehlende Datei bzw. Standard. |
| `SaveAsync` | `int seconds`, `CancellationToken cancellationToken = default` | `Task` | Persistiert atomar ein unterstütztes Intervall. |

## `IDepartureService`
Datei: `FlowNRW.Core/Transit/IDepartureService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `DeparturesAsync` | `Stop stop`, `DateTimeOffset departure`, `CancellationToken cancellationToken = default` | `Task<ProviderResult<StopEvent>>` | Ruft normalisierte Abfahrten ab. |

## `ITransitCache`
Datei: `FlowNRW.Core/Transit/ITransitCache.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `Get<T>` | `string key`, `bool allowStale = false` | `ProviderResult<T>?` | Liest typisierten Eintrag; veraltete Daten sind nur bei ausdrücklicher Freigabe erlaubt. |
| `Set<T>` | `string key`, `ProviderResult<T> result`, `TimeSpan timeToLive` | `void` | Speichert erfolgreiche, nicht veraltete Daten befristet im Speicher. |

`IDepartureNavigation` und `IRefreshSettingsStore` sind die weiteren vom vorhandenen Monitor-/Settingspfad verwendeten Grenzen; für Lebenszyklus- bzw. Hintergrundregistrierung ist derzeit kein Interface vorhanden.
