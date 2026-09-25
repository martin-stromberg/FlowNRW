# Logik

## `RefreshLoop`
Datei: `FlowNRW.Core/Refresh/RefreshLoop.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `RefreshLoop(Func<Task>, Action, Func<TimeSpan, CancellationToken, Task>?)` | `public` | Erstellt einen inaktiven Loop für einen Aktualisierungsaufruf, Abbruchcallback und optionalen Test-Delay. |
| `Start(int)` | `public` | Startet genau einen verzögerten Loop für ein unterstütztes Intervall; gleiche Intervalle duplizieren ihn nicht. |
| `Stop()` | `public` | Bricht Wartezeit ab, invalidiert laufende Arbeit und ruft den Abbruchcallback auf. |
| `Dispose()` | `public` | Stoppt und gibt den Loop dauerhaft frei. |
| `RunAsync(...)` | `private` | Wartet completion-relativ und fängt Update-/Delay-Ausnahmen ab. |

Abonnierte Events: keine. Publizierte Events: keine.

## `RefreshSettingsViewModel`
Datei: `FlowNRW.Core/Refresh/RefreshSettingsViewModel.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `IsSupported(int)` | `public static` | Prüft die erlaubten Intervalle `0`, `30`, `60`, `120`, `300`. |
| `LoadAsync()` | `public` | Lädt die Einstellung einmal; bei Fehler bleibt ein sichtbarer sicherer Standardwert. |
| `SaveAsync()` | `public` | Persistiert zuerst erfolgreich und aktiviert danach die Auswahl. |
| `NotifyState()` | `private` | Meldet Änderungen an wirksamen Einstellungswerten. |

Abonnierte Events: keine. Publizierte Events: `Changed` nach erfolgreichem Speichern.

## `JsonRefreshSettingsStore`
Datei: `FlowNRW.Core/Refresh/JsonRefreshSettingsStore.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `JsonRefreshSettingsStore(string)` | `public` | Bindet atomare JSON-Integer-Speicherung an einen Pfad. |
| `LoadAsync(CancellationToken)` | `public` | Liest und validiert eine maximal 64 Byte große Intervall-Datei. |
| `SaveAsync(int, CancellationToken)` | `public` | Schreibt einen unterstützten Wert atomar und abbrechbar. |

Abonnierte Events: keine. Publizierte Events: keine.

## `DepartureService`
Datei: `FlowNRW.Core/Transit/DepartureService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `DeparturesAsync(Stop, DateTimeOffset, CancellationToken)` | `public` | Validiert Haltestelle, ruft den Provider ab und begrenzt Ergebnisanzahl; neue Requests ersetzen ältere in derselben Serviceinstanz. |
| `Latest<T>(...)` | `private` | Verknüpft Cancellation-Token und verwirft ersetzte Requests. |

Abonnierte Events: keine. Publizierte Events: keine.

## `StopMonitorViewModel`
Datei: `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OpenAsync(Address)` | `public` | Öffnet nur ein derzeit sichtbares, identifizierbares Suchergebnis und lädt dessen Abfahrten. |
| `OpenFavoriteAsync(FavoriteHomeViewModel, FavoriteMonitorViewModel)` | `public` | Öffnet nur einen aktuellen Favoritenmonitor. |
| `RefreshAsync()` | `public` | Führt manuelle Aktualisierung aus. |
| `RefreshAutomaticallyAsync()` | `public` | Führt Intervallaktualisierung über dieselbe Busy-/Revisionslogik aus. |
| `CancelPending()` | `public` | Bricht laufenden Request ab, erhöht Revision und verwirft verspätete Antwort. |
| `FindNearbyAsync()` | `private` | Ruft Standort und nahe Haltestellen nach expliziter Aktion ab. |
| `RefreshCoreAsync(bool)` | `private` | Gemeinsame manuelle/automatische Anforderung, behält letzte Daten bei Fehler. |
| `EffectiveTime(StopEvent)` | `private static` | Bestimmt Ist- bzw. abgeleitete effektive Zeit für Sortierung. |

Das ViewModel publiziert Zustände über `PropertyChanged` des `ObservableObject`; `RefreshCommand` und `NearbyCommand` sind Aktionen. Es abonniert `Lookup.PropertyChanged` und `Lookup.Changed`. Es speichert erfolgreichen `Result` und jüngsten `LastAttempt`; `Metadata` verwendet `JourneyPresentation.Metadata` für Abrufzeit, Datenalter, Fallback, Cachealter und Warnhinweis. Daten von `Result` werden bei Refreshfehler behalten.

## `FavoriteMonitorViewModel`
Datei: `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `RefreshAsync()` | `public` | Aktualisiert manuell und behält bekannte Daten im Fehlerfall. |
| `RefreshAutomaticallyAsync()` | `public` | Aktualisiert über denselben Busy-/Revisionsschutz ohne manuelle Kennzeichnung. |
| `CancelPending()` | `public` | Bricht entfernte oder verlassene Karte ab und ignoriert verspätete Ergebnisse. |
| `RefreshCoreAsync(bool)` | `private` | Ruft eine einzelne Karte ab, sortiert effektive Abfahrtszeit und setzt Status. |
| `SetDistance(double?)` | `internal` | Aktualisiert das Entfernungslabel. |

Publiziert Eigenschaften über `PropertyChanged`, keine eigenen .NET-Events. Jede Karte hat unabhängigen Request-/Busy-Zustand.

## `FavoriteHomeViewModel`
Datei: `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `LoadAsync()` | `public` | Lädt Favoriten einmal und erstellt unabhängige Kartenmodelle. |
| `RefreshMissingAsync()` | `public` | Lädt Karten ohne erfolgreiches Resultat. |
| `ToggleAsync(Stop)` | `public` | Persistiert Hinzufügen/Entfernen einer technischen Haltestellenidentität. |
| `RemoveAsync(FavoriteMonitorViewModel)` | `public` | Entfernt eine weiterhin aktuelle Karte. |
| `CancelPending()` | `public` | Bricht Standort- und Kartenabrufe ab. |
| `UpdateDistancesAsync()` | `private` | Ermittelt angeforderten Standort und sortiert Karten. |
| `MutateAsync(...)` | `private` | Speichert Favoritenänderung vor Änderung des sichtbaren Bestands. |
| `SortCards()` | `private` | Sortiert nach bekannten Distanzen und bewahrt Reihenfolge bei unbekannter Entfernung. |

Das Modell publiziert Zustände über `PropertyChanged`; `LocationCommand` löst ausdrücklich angeforderte Standortabfrage aus.

## `App`
Datei: `FlowNRW/App.xaml.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `App(AppShell, ForegroundState)` | `public` | Übernimmt Shell und gemeinsam genutzten Fensterzustand. |
| `CreateWindow(IActivationState?)` | `protected override` | Erzeugt MAUI-Fenster und setzt `IsActive` bei `Activated`, `Deactivated`, `Destroying`. |

Diese Fensterereignisse aktualisieren nur den Vordergrundstatus; sie registrieren keine Hintergrundaufgabe und lösen bei Aktivierung keinen direkten Resume-Abruf aus.

## `DeparturePage` und `HomePage`
Dateien: `FlowNRW/DeparturePage.cs`, `FlowNRW/HomePage.cs`

`DeparturePage.OnAppearing` lädt Intervallsettings und reconciled danach einen `RefreshLoop`; `ReconcileRefreshLoop` startet ihn nur, wenn Seite aktiv, Fenster aktiv und Settings geladen sind. `OnDisappearing` stoppt den Loop und bricht Monitorarbeit ab. Settings- und Foreground-Änderungen führen zur erneuten Reconciliation.

`HomePage.OnAppearing` lädt Settings und Favoriten, reconciled unabhängige `RefreshLoop`-Instanzen je Favoritenkarte und ruft `RefreshMissingAsync` auf. Bei `OnDisappearing` werden Loops beendet und Kartenabfragen abgebrochen. Kartenänderungen reconciled Loops und startet fehlende Datenabrufe. Das sind Vordergrund-/Navigationsmechanismen; ein gemeinsamer einzelner Koordinator für Vordergrund-, Hintergrund- und Resumearbeit ist im Bestand nicht vorhanden.

Abonnierte Events: Seiten abonnieren `RefreshSettingsViewModel.Changed`, `ForegroundState.PropertyChanged`, ViewModel-/Favoriten-`PropertyChanged`; Abos werden beim Verlassen wieder gelöst. Publizierte Events: keine eigenen Lebenszyklus-Events.

## `AppDelegate`
Datei: `FlowNRW/Platforms/iOS/AppDelegate.cs`

`CreateMauiApp()` erzeugt über `MauiProgram.CreateMauiApp()` den DI-konfigurierten MAUI-Appbaum. In der vorhandenen Klasse wird kein iOS-Hintergrundereignis überschrieben oder registriert.
