← [Zur Übersicht](index.md)

# Favoriten — Technischer Ablauf

Diese Seite beschreibt die seit dem iOS-Nutzungsfeedback geltenden Darstellungs- und Datenregeln der Startseiten-Favoritenkarten. Persistenz (`IFavoriteStore`, `DepartureCacheEntry`), Intervallverwaltung und Sortierung sind unverändert.

## Linienbestand der Tafel (`Lines`)

`FavoriteMonitorViewModel.Lines` speist die kompakte Linienzeile im zugeklappten Zustand (`HomePage.RenderFavoriteSummary`).

1. **Vollständige Antwort** (`ErrorCode is null`, keine `Warnings`, nicht `IsFallback`, nicht `IsStale`): `Lines` wird **ersetzend** aus den Liniennamen der Antwort berechnet (dedupliziert, ordinal sortiert) und zusammen mit dem Ergebnis in `DepartureCacheEntry.Lines` persistiert. Der Cache enthält ausschließlich vollständige Inventare.
2. **Degradierte, aber angezeigte Antwort** (`ErrorCode is null`, jedoch Warnungen, Fallback oder Stale): `EnrichLines` bildet die **Vereinigung** des bisherigen Bestands mit den Linien der angezeigten Items. Es wird nichts persistiert — der letzte vollständige Bestand bleibt die einzige Cache-Quelle.
3. **Cache-Wiederherstellung** (`Restore`): `Lines` kommt aus dem gespeicherten Inventar bzw. den Items des Cache-Eintrags.

Damit füllt sich die Übersicht auch auf Geräten, deren zweite Quelle regelmäßig ausfällt (degradierte Antworten), und es werden nie Linien erfunden: Jede angezeigte Linie stammt aus einer echten gelieferten oder zuletzt vollständig persistierten Antwort.

## Datenqualitäts-Hinweis (`HasDataNotice`/`NoticeToggle`)

- `FavoriteMonitorViewModel.HasDataNotice` und `StopMonitorViewModel.HasDataNotice` leiten beide über die interne Extension `DataNotice.HasDataQualityNotice` aus dem **angezeigten** `Result` ab: `Warnings.Count > 0 || IsFallback || IsStale`. Ein `null`- oder Fehler-`Result` ergibt `false`; die Eigenschaft wird in `RefreshBindings` mitnotifiziert.
- `TransitVisuals.NoticeToggle` kapselt je Stelle einen 48×48-`Button` (`TransitIcons.Warning`, Beschreibung „Hinweis zu den Daten anzeigen"/„Hinweis schließen") und ein anfangs unsichtbares `Label` mit dem `Metadata`-Text (Quelle, Datenstand, Fallback, Cache, Anbieterwarnung). `Update(visible)` zeigt den Schalter nur bei `HasDataNotice && !IsBusy` und klappt das Detail automatisch zu, sobald der Hinweis entfällt. `ApplyWarningColors()` muss **nach** `TransitVisuals.ApplyRoles` laufen, da die gemeinsame Button-Rolle die Glyphenfarbe sonst überschreibt.
- Verdrahtung: `HomePage.BuildFavoriteCard` erzeugt `FavoriteNotice{i}`/`FavoriteMetadata{i}` neben bzw. unter der `FavoriteStatus{i}`-Zeile; `DeparturePage` erzeugt `MonitorNotice`/`MonitorMetadata` im `monitorState`-Block. `ShowsFavoriteStatus` (`HomePage`) und `UpdateMonitorStatus` (`DeparturePage`) blenden den Text „…unvollständig…" nicht mehr als Zeile ein — der `Status`-String des ViewModels bleibt unverändert (Semantik, Tests, Logging); echte Fehler- und Leerzustände bleiben sichtbare Textzeilen.

## Auf-/Zuklapp-Schalter im Kartenkopf

`ToggleFavorite{i}` liegt in `HomePage.BuildCardHeader` in der dritten Kopfspalte `[Titel | FavoriteBusy{i} | ToggleFavorite{i}]`. `AutomationId`, 48×48-Bedienziel, `expand_more`/`expand_less`-Glyphwechsel und der `SemanticProperties.Description`-Wechsel („Abfahrten aufklappen"/„einklappen") sind unverändert; die Aktionsleiste am Kartenende enthält nur noch Aktualisieren, Monitor öffnen und Entfernen.

## Entfernung in „Nächste Haltestellen" (`NearbyDistanceLabel`)

`FavoriteHomeViewModel` hält die gefilterten `NearbyStopResult`-Einträge der letzten Abfrage in `nearbyItems`; `NearbyStops` bleibt die `Address`-Projektion derselben Menge. `NearbyDistanceLabel(Address)` liefert den Suffix der Detailzeile (`NearbyDistance{i}`):

1. `NearbyStopResult.DistanceMeters` endlich und ≥ 0 → ` · Entfernung: X m` (de-DE-Formatierung).
2. Sonst, falls Position und Zielkoordinate vorliegen → ` · Luftlinie: X m` (Haversine über die interne `Distance`-Hilfsfunktion).
3. Sonst → ` · Entfernung unbekannt`.

Die Zuordnung erfolgt über `ReferenceEquals` bzw. `SameIdentity` auf `Stop`. Provider-Distanz und Luftlinie sind durch unterschiedliche Beschriftung getrennt und werden nie vermischt.

## Anzeigename und Kennungen

`TransitVisuals.Candidate` formatiert Treffer nur noch als `JourneyPresentation.DisplayName(candidate.Name) + suffix` — die technische Kennung (`Source`/`Id`/`Dhid`) ist aus allen Trefferlisten (Haltestellensuche, „Nächste Haltestellen", Verbindungssuche, Kartenliste) entfernt; die Detailzeile trägt nur noch den `suffix` (z. B. Entfernung) und erhält optional eine eigene `AutomationId` (`detailsId`). Die Bereinigungsregel selbst steht in [Design — Öffentliche Präsentationsschnittstellen](../design/api.md) und [Design — Fachliche Regeln](../design/business-rules.md).
