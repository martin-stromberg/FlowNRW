← [Zurück zur Übersicht](index.md)

# Design — Öffentliche Präsentationsschnittstellen

Der Design-Refresh erweitert die öffentliche Oberfläche von `FlowNRW.Core.Presentation` um die Statussemantik der Status-Chips und um gemeinsame Uhrzeitformatierung. Alle neuen Member lesen ausschließlich gelieferte Echtzeitfelder und erfinden keine Werte.

## `DepartureStatus` (Enum, `FlowNRW.Core/Presentation/DepartureStatus.cs`)

| Wert | Bedeutung |
|---|---|
| `Scheduled` | Keine Echtzeitinformation verfügbar; nur die Sollzeit ist bekannt. |
| `OnTime` | Echtzeit meldet die Abfahrt planmäßig ohne Abweichung. |
| `Delayed` | Echtzeit meldet eine Abweichung vom Plan. |
| `Cancelled` | Echtzeit meldet die Abfahrt als ausgefallen. |

## `DeparturePresentation` (Erweiterungen, `FlowNRW.Core/Presentation/DeparturePresentation.cs`)

| Signatur | Rückgabe |
|---|---|
| `Status(StopEvent item) → DepartureStatus` | `Cancelled` bei gemeldetem Ausfall, `Scheduled` ohne `Realtime.ActualTime`, `Delayed` bei `ActualTime ≠ PlannedTime` oder positivem `Realtime.Delay`, sonst `OnTime`. |
| `DelayText(StopEvent item) → string?` | `"+n Min."` bei echtem positiven Delay (Ist-/Solldifferenz bevorzugt, sonst gemeldetes Delay), sonst `null` — auch bei Ausfall. |

Unverändert: `BadgeColor(line, mode)` und `Describe(item)`.

## `JourneyPresentation` (Erweiterungen, `FlowNRW.Core/Presentation/JourneyPresentation.cs`)

| Signatur | Rückgabe |
|---|---|
| `LocalTime(DateTimeOffset time) → DateTimeOffset` | Derselbe Zeitpunkt in Europe/Berlin (einmalig gecachte Zone). |
| `Clock(DateTimeOffset? time) → string` | `HH:mm` in Europe/Berlin oder `„unbekannt"`. |
| `DisplayName(string? name) → string` | Anzeigename ohne wörtlich wiederholten Ortspräfix („X, X …" → „X …" nur bei Wortgrenze am Restanfang); `null` → `string.Empty`, sonst unverändert. |
| `DisplayAddress(Address address) → string` | `DisplayName(Name)` plus optionale Koordinaten ` · {Lat}, {Lon}`; enthält nie `Source`/`Id`/`Dhid`. |

`Time`, `EventTime`, `Summary` und übrige Member nutzen intern jetzt `LocalTime`/`Clock`; ihr Verhalten ist unverändert. Das frühere technische Format `Address(Address)` mit Quelle/Id/DHID ist entfallen — Anzeigestellen verwenden `DisplayName`/`DisplayAddress`.

## Datenqualitätshinweis und Entfernung (Erweiterungen)

| Signatur | Rückgabe |
|---|---|
| `FavoriteMonitorViewModel.HasDataNotice` / `StopMonitorViewModel.HasDataNotice` → `bool` | `true`, wenn das **angezeigte** `Result` Warnungen, `IsFallback` oder `IsStale` trägt (gemeinsame Ableitung in der internen Extension `DataNotice.HasDataQualityNotice`); `null`- oder Fehlerergebnis → `false`. |
| `FavoriteHomeViewModel.NearbyDistanceLabel(Address candidate) → string` | Suffix ` · Entfernung: X m` (Provider-`DistanceMeters`), ` · Luftlinie: X m` (Haversine-Fallback) oder ` · Entfernung unbekannt`. |

## Interne App-Helfer (kein öffentlicher Vertrag)

`TransitIcons` (Glyph-Konstanten, `SetIcon`, `IconImage`, `TabIcon`), `Fonts` (Alias-Konstanten) und `TransitVisuals` (`StatusChip`, `IconLabel`, `StrikethroughTime`, `SecondaryAction`, `Badge`, `Candidate`, `NoticeToggle`, `Page`, `ApplyRoles`) sind `internal` zur FlowNRW-App und kein freigegebenes API.

## Tests

`FlowNRW.Tests/DepartureStatusTests.cs` deckt die Statusableitung und Verspätungstexte ab; `FlowNRW.Tests/PresentationTests.cs` ergänzt Fälle für `LocalTime`/`Clock` sowie `DisplayName`/`DisplayAddress`. `HasDataNotice` und `NearbyDistanceLabel` sind in `FavoriteTests`/`StopMonitorTests`, die Linien-Merge-Semantik in `FavoriteDepartureCacheIntegrationTests` abgedeckt.
