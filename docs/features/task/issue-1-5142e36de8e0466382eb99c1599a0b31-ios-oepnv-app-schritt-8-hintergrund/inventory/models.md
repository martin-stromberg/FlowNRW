# Datenmodelle und Zustandsobjekte

## `ForegroundState`
Datei: `FlowNRW.Core/Refresh/ForegroundState.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `IsActive` | `bool` | Meldet, ob das App-Fenster aktiv ist; geänderte Werte lösen `PropertyChanged` aus. |

## `ProviderResult<T>`
Datei: `FlowNRW.Core/Transit/ProviderResult.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Items` | `IReadOnlyList<T>` | Normalisierte Datenelemente. |
| `Source` | `string` | Anbieterkennung. |
| `RetrievedAt` | `DateTimeOffset` | Abrufzeitpunkt; Cachetreffer behalten diesen Zeitstempel. |
| `Warnings` | `IReadOnlyList<string>` | Sichere technische Warncodes. |
| `ErrorCode` | `string?` | Sicherer Fehlercode oder `null`. |
| `IsFallback` | `bool` | Kennzeichnet die Verwendung einer Ersatzquelle. |
| `IsStale` | `bool` | Kennzeichnet Daten jenseits ihrer Frischefrist. |
| `HasData` | `bool` (berechnet) | Wahr, wenn Einträge vorhanden sind und kein Fehler vorliegt. |

## `StopEvent`
Datei: `FlowNRW.Core/Transit/StopEvent.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Identity` | `TripIdentity` | Ereignis-/Fahrtidentität. |
| `PlannedTime` | `DateTimeOffset?` | Geplante Ereigniszeit. |
| `Realtime` | `RealtimeStatus` | Optionale Echtzeitfelder. |
| `Line` | `Line?` | Zugehörige Linie und Betreiberinformationen. |

## `RealtimeStatus`
Datei: `FlowNRW.Core/Transit/RealtimeStatus.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `ActualTime` | `DateTimeOffset?` | Gemeldete Echtzeit. |
| `Delay` | `TimeSpan?` | Gemeldete Verspätung oder Vorlauf. |
| `Cancelled` | `bool?` | Ausfallstatus; `null` bedeutet unbekannt. |
| `Platform` | `string?` | Gemeldeter Ist-Steig. |
| `PlannedPlatform` | `string?` | Geplanter Steig. |
| `Source` | `string` | Echtzeitquelle. |
| `RetrievedAt` | `DateTimeOffset` | Abrufzeitpunkt der Echtzeit. |

## `RefreshSettingsViewModel` (Konfiguration/Zustand)
Datei: `FlowNRW.Core/Refresh/RefreshSettingsViewModel.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `IntervalSeconds` | `int` | Wirksames gespeichertes Intervall; Standard 60 Sekunden. |
| `SelectedSeconds` | `int` | Noch nicht gespeicherter Auswahlwert. |
| `IsLoaded` | `bool` | Gibt an, dass gespeicherte Einstellung oder Standard aufgelöst ist. |
| `IsSaving` | `bool` | Kennzeichnet laufenden Speichervorgang. |
| `Status` | `string` | Lade-, Validierungs- oder Speicherstatus. |
| `Description` | `string` (berechnet) | Beschreibt das wirksame Intervall; bei aktivem Intervall nennt es das aktive Fenster. |

Die gespeicherten Sekunden sind auf `0`, `30`, `60`, `120` oder `300` begrenzt. Es gibt keinen Zustandswert für iOS-Hintergrundregistrierung oder abgeschlossene Hintergrundarbeit.
