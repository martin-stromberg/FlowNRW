# Hinweis „Daten möglicherweise unvollständig oder veraltet." — Erzeugung und Anzeige

Deckt Anforderungspunkt 2 ab: Wo der Text/die Zustände gesetzt und wo sie gerendert werden.

## Erzeugung des Status-Textes

| Stelle | Zeile | Bedingung | Text |
|---|---|---|---|
| `FavoriteMonitorViewModel.RefreshCoreAsync` (`FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`) | Z.126–128 | `result.Warnings.Count > 0 \|\| result.IsFallback \|\| result.IsStale` | „Daten möglicherweise unvollständig oder veraltet." |
| `StopMonitorViewModel.RefreshCoreAsync` (`FlowNRW.Core/Presentation/StopMonitorViewModel.cs`) | Z.289–291 | identische Bedingung | identischer Text |

Auslösende Resultat-Flags stammen aus `ProviderResult<T>` (`FlowNRW.Core/Transit/ProviderResult.cs` Z.5–23): `Warnings` (Z.14), `IsFallback` (Z.18), `IsStale` (Z.20), `ErrorCode` (Z.16). Der Orchestrator (`ProviderOrchestrator`) erzeugt die Flags; die ViewModel-Diagnose loggt sie via `Diagnostics.AppLog.Write("favorite"/"monitor", …)` (`FavoriteMonitorViewModel` Z.117, `StopMonitorViewModel` Z.278).

## Weitere Status-Texte im selben `Status`-Kanal

| Text | Stelle |
|---|---|
| „Noch keine Abfahrten geladen." (Initial) | `FavoriteMonitorViewModel` Z.48 |
| „Letzter Stand wird aktualisiert …" | `FavoriteMonitorViewModel.Restore` Z.92 |
| „Abfahrten werden aktualisiert …" | `FavoriteMonitorViewModel` Z.109 / `StopMonitorViewModel` Z.269 |
| „Keine nächsten Abfahrten gefunden." / „{n} Abfahrten · … aktualisiert." | `FavoriteMonitorViewModel` Z.128 / `StopMonitorViewModel` Z.291 |
| „Aktualisierung abgebrochen." | `FavoriteMonitorViewModel` Z.143/155 / `StopMonitorViewModel` Z.296/319 |
| „Abfahrten konnten nicht geladen werden. Bitte erneut versuchen." / „Aktualisierung fehlgeschlagen. Letzte bekannte Daten werden angezeigt; bitte Datenstand beachten." | `FavoriteMonitorViewModel.SetFailure` Z.167–172 / `StopMonitorViewModel.SetFailure` Z.367–368 |
| „Bitte eine Haltestelle auswählen." (Initial) | `StopMonitorViewModel` Z.99 |

## Anzeigestellen

### Favoritenkarten (`FlowNRW/HomePage.cs`)

| Element | Zeile | Beschreibung |
|---|---|---|
| `FavoriteStatus{index}`-`Label` | Z.210–213 | Gebunden an `card.Status`; initial `IsVisible = ShowsFavoriteStatus(card) && !card.IsBusy` |
| `ShowsFavoriteStatus(card)` | Z.360–364 | Matcht (OrdinalIgnoreCase): „Keine nächsten", „Letzter Stand", „unvollständig", „fehlgeschlagen", „nicht geladen" |
| Aktualisierung | Z.279–284 in `SubscribeCardChanges` | Bei `Status`/`IsBusy`/`Result`/`IsExpanded`-Notify wird die Sichtbarkeit neu bewertet |
| `TransitVisuals.ApplyRoles` | `TransitVisuals.cs` Z.90–105 | Labels mit `AutomationId` endend auf „Status" erhalten `FontSize 13` + `Muted`/`MutedDark`-Textfarbe (Z.92–98) |

Die Karte bindet **kein** `Metadata`-Label: `card.Metadata` (`FavoriteMonitorViewModel` Z.39–42) existiert, ist aber in `BuildFavoriteCard` nicht verdrahtet.

### Abfahrtsmonitor (`FlowNRW/DeparturePage.cs`)

| Element | Zeile | Beschreibung |
|---|---|---|
| `MonitorStatus`-`Label` | Z.87–91 | Gebunden an `model.Status` innerhalb `monitorState`-Block zusammen mit `FavoriteToggleStatus` (Z.71–72, 89–91) |
| `UpdateMonitorStatus()` | Z.137–148 | `IsVisible = !model.IsBusy &&` Match auf „Keine nächsten", „unvollständig", „fehlgeschlagen", „nicht geladen", „abgebrochen", „Letzte bekannte", „Datenstand" |
| `ModelChanged` | Z.130–135 | `Status`/`IsBusy`-Notify → `UpdateMonitorStatus` |

## Verwandte Präsentationshelfer (`FlowNRW.Core/Presentation/JourneyPresentation.cs`)

| Element | Zeile | Inhalt |
|---|---|---|
| `Metadata<T>(ProviderResult<T>)` | Z.48 | „Quelle: {Source}; Datenstand: {dd.MM.yyyy HH:mm UTC±zz}; Datenalter: {n} Min." + optional „Ersatzquelle (Fallback)." / „Veralteter Cache." / „Anbieterwarnung: Daten sind möglicherweise unvollständig; eine Quelle konnte keine aktuellen Daten liefern." |
| `CompactWarning<T>(ProviderResult<T>?)` | Z.53–59 | `""` bei null/komplett; „Daten möglicherweise unvollständig" (`Warnings>0`), „Zwischengespeicherte Daten" (`IsStale`), „Ersatzquelle" (`IsFallback`) |

`Metadata`-Bindungen im UI: `card.Metadata` (ungerendert), `StopMonitorViewModel.Metadata` (Z.88–95), `EndpointViewModel.Metadata` (Z.96–102 → `{prefix}Metadata`, `SearchPage` Z.150/154/179), `JourneySearchViewModel.Metadata` (Z.45–49 → `RoutingMetadata`, `SearchPage` Z.97), `StopMonitorViewModel.SearchMetadata` (Z.69–72), `MapViewModel.Metadata` (→ `MapMetadata`, `MapPage` Z.51). `CompactWarning` wird gerendert in `StopSearchPage.SearchWarning` (Z.117–122 → `StopSearchMetadata`, Z.63/86) und `JourneyDetailViewModel.Session.CompactWarning` (`JourneySearchViewModel` Z.54–56 → `DetailMetadata`, `JourneyDetailPage` Z.39–40).

## Bestehende Symbol-/Detail-Mechaniken

- `TransitIcons.SetIcon(Button, glyph, description, filled, size)` (`FlowNRW/TransitIcons.cs` Z.84–91), `IconImage` (Z.101–111).
- `TransitVisuals.IconLabel(glyph)` (Z.140–149), `StatusChip(glyph, text, light, dark)` (Z.157–175), `SecondaryAction(icon, description, automationId, command)` (Z.177–188, 48×48).
- Ein-/Ausblendbares Detail existiert als Muster in `MapPage` Z.50–61: `information`-`VerticalStackLayout` (`IsVisible=false`) mit `MapInformation`-`Button` (`TransitIcons.Info`), Toggle aktualisiert Beschreibung („Kartendaten schließen"/„Quellen und Kartendaten").
- Semantikfarben `Warning`/`WarningDark`, `Error`/`ErrorDark` (`TransitVisuals` Z.49–63).

## Offen/unklar (Ist-Stand)

- Ob das Symbol-Muster auch `DeparturePage.MonitorStatus` betreffen soll, ist fachlich offen — beide Seiten zeigen denselben Text.
- `ShowsFavoriteStatus`/`UpdateMonitorStatus` blenden die gleiche Textzeile auch für „Letzter Stand", „Keine nächsten", „fehlgeschlagen" ein — der Hinweis „unvollständig" teilt sich den `Status`-Kanal mit echten Fehlern; eine technische Unterscheidung der Statusfälle existiert nicht (`Status` ist reiner String).
