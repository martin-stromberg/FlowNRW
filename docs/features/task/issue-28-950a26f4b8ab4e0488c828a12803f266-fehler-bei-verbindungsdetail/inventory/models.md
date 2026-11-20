# Datenmodell – Bestandsaufnahme

## `Journey`
Datei: `FlowNRW.Core/Transit/Journey.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `string?` | Provider-seitige Verbindungskennung |
| `Legs` | `IReadOnlyList<JourneyLeg>` | Geordnete Abschnitte |
| `Transfers` | `IReadOnlyList<Transfer>` | Umstiege zwischen Verkehrsabschnitten |

## `JourneyLeg`
Datei: `FlowNRW.Core/Transit/JourneyLeg.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Departure` | `StopEvent` | Start-Ereignis des Abschnitts |
| `Arrival` | `StopEvent` | Ziel-Ereignis des Abschnitts |
| `Line` | `Line?` | Verkehrslinie (bei Fußweg `null`) |
| `Walking` | `WalkingSegment?` | Fußweg-Information; `!= null` markiert Fußwegabschnitt |
| `Geometry` | `GeoGeometry?` | Provider-Geometrie |

## `StopEvent`
Datei: `FlowNRW.Core/Transit/StopEvent.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Identity` | `TripIdentity` | Ereignis-Identität |
| `PlannedTime` | `DateTimeOffset?` | Geplanter Zeitpunkt |
| `Realtime` | `RealtimeStatus` | Echtzeitfelder |
| `Line` | `Line?` | Linie/Betreiber |

## `TripIdentity`
Datei: `FlowNRW.Core/Transit/TripIdentity.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Source` | `string` | Provider-Namespace |
| `TripId` | `string?` | Provider-Fahrtkennung; bei Fußwegen `null` |
| `SharedTripId` | `string?` | Explizite providerübergreifende Kennung |
| `Stop` | `Stop` | Ereignis-Haltestelle |
| `Line` | `string?` | Linienname |
| `Operator` | `string?` | Betreibername |
| `Direction` | `string?` | Richtung laut Provider |
| `PlannedTime` | `DateTimeOffset?` | Geplanter Ereigniszeitpunkt |

## `Stop`
Datei: `FlowNRW.Core/Transit/Stop.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `string` | Provider-Kennung; bei Adresspunkten leer möglich |
| `Name` | `string` | Anzeigename |
| `Source` | `string` | Kennungs-Namespace |
| `Dhid` | `string?` | Gemeinsame nationale Haltestellenkennung |
| `Coordinate` | `GeoCoordinate?` | Position |

## `WalkingSegment`
Datei: `FlowNRW.Core/Transit/WalkingSegment.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Duration` | `TimeSpan?` | Gehdauer |
| `DistanceMeters` | `double?` | Gehdistanz |
| `Geometry` | `GeoGeometry?` | Geometrie |

## Mapper-Verhalten für Fußwegabschnitte

- `EfaResponseMapper` (`FlowNRW.Core/Transit/EfaResponseMapper.cs:54-63`):
  `TripId` nur `lineId + ":" + trip`, wenn beide vorhanden — bei Fußwegen `null`;
  `Walking != null` bei Produktklasse 99/100 oder `id == 99`.
- `DbRestResponseMapper` (`FlowNRW.Core/Transit/DbRestResponseMapper.cs:47-55`):
  `TripId = Text(e, "tripId")` — bei Fußweg-Elementen `null`;
  `Walking != null` bei `walking == true`.
