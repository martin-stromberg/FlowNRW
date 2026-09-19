# Umsetzungsplan Monitorintervalle

## Entscheidungen

Global gespeicherte Auswahl Aus/30/60/120/300 Sekunden, Standard60. Mindest30 entspricht der bestehenden Echtzeit-Cachefrist und begrenzt Anbieterlast; Maximal300 bleibt als überschaubare Aktualisierung auswählbar. Keine freie Zahleneingabe. Speichern erst nach erfolgreicher begrenzter atomarer Dateiersetzung wirksam. Ungültige oder beschädigte Settings melden Fehler und lassen sichere Standard60 aktiv; keine automatische Überschreibung. Explizites Speichern kann die beschädigte Einstellung ersetzen, anders als nutzererzeugte Favoriten sind keine fachlichen Daten zu verlieren.

Kleine Core-Klassen unter Refresh: IRefreshSettingsStore, JsonRefreshSettingsStore, RefreshSettingsViewModel, RefreshLoop. UI SettingsPage mit Picker und Speichern; von Home und Monitor erreichbar. Keine zusätzlichen Pakete oder Plattformtimer. Delay injizierbar als Func<TimeSpan,CancellationToken,Task>, produktiv Task.Delay. Fortsetzungen auf aufrufendem UI-Kontext; kein Task.Run für ViewModels.

RefreshLoop besitzt Start/Stop/Dispose und verarbeitet eine Aktion nach jedem vollständigen Intervall. Start ist idempotent und führt keine sofortige Aktualisierung aus. Nach Abschluss/Fehler wartet die Schleife erneut vollständig, kein Aufholstau. Stop cancelt Delay und ruft vorhandenes CancelPending auf; Revisionsschutz verwirft ignorierte Abbrüche. Bei Intervallwechsel alte Schleife stoppen und mit neuer Wartezeit starten. Aus erzeugt keinen Timer. Fehler werden beobachtet; kein unbegrenzter Sofortretry.

Jede sichtbare Favoritenkarte erhält eine unabhängige Schleife; langsame Karten blockieren andere nicht. Home verwaltet Schleifen nach aktueller Kartenreferenz, entfernt/entsorgt verschwundene Karten. Nur aktive Seiten im aktiven Fenster laufen. Window.Activated/Deactivated steuern einen kleinen gemeinsamen ForegroundState (INotifyPropertyChanged); OnAppearing/OnDisappearing entscheiden Seitenaktivität. Suspendierung/iOS-Hintergrundmechanismen und Datenwiederaufnahme bleiben Schritt8. Erste Aktivierung cancelt keinen initialen Monitorrequest (OpenStopAsync-Revisionsschutz).

RefreshAsync bleibt manueller Pfad; zusätzlich RefreshAutomaticallyAsync oder bool-Kernpfad benennt erfolgreichen Status korrekt als automatisch. Beide verwenden denselben IsBusy-Schutz und vorhandene Services: gleichzeitiger manueller/automatischer Aufruf startet keinen zweiten Abruf. Unabhängige Karten behalten ihre Services. ProviderOrchestrator-Zusammenfassung exakt gleicher Requests und HTTP-Retrygrenzen unverändert; keine falsche Behauptung des Zusammenfassens verschiedener Zeitstempel.

## Ablauf und Pakete

1. Settingsstore/ViewModel mit LoadAsync, SaveAsync, Intervallauswahl, Status und Changed erst nach erfolgreichem Speichern. Coretests für Standard, gültige/ungültige Auswahl, atomare Fehler, Neustart und Dateigrenze.
2. RefreshLoop mit injizierbarer Verzögerung, Versionsschutz und deterministischen Tests. Automatischer Refreshpfad in Einzel-/Favoritenmodell; Busy-Schutz, Fehlererhalt und Cancel beibehalten.
3. SettingsPage, DI und Navigation. Home/Departure koppeln Schleifen an Sichtbarkeit, Settings und gemeinsamen Vordergrundzustand. Pro aktuelle Karte eine Schleife, Zustandswechsel entfernen alte Abos/Schleifen. Intervallstatus sichtbar, Einstellungen nach Neustart laden bevor Timer aktiv werden. Keine temporäre falsche60s-Aktivierung bei gespeicherten Aus.
4. Native Fixture/Harness, Reviews, gesamte Tests/Coverage/Build/Format/XML, Hilfe/README/ReleaseNotes, Commit und unabhängige Projektabnahme.

## API und Integration

IRefreshSettingsStore.LoadAsync(token) liefert int? (fehlende Datei null), SaveAsync(int,token). JsonRefreshSettingsStore(path) speichert ausschließlich Sekunden als kleine JSON-Zahl, maximal64Bytes und zugelassene Werte. RefreshSettingsViewModel: IntervalSeconds (wirksam), SelectedSeconds (Entwurf), IsLoaded, IsSaving, Status, Description, LoadAsync, SaveAsync sowie Changed. ForegroundState.IsActive meldet Änderungen. RefreshLoop(Func<Task> refresh, Action cancel, Func<TimeSpan,CancellationToken,Task>? delay=null): Start(int seconds), Stop(), Dispose(). ForegroundState in Core damit Aktivitätslogik testbar bleibt.

UI: eigene SettingsPage mit AutomationIds RefreshInterval, SaveRefreshSettings, RefreshSettingsStatus. Home/Monitor Buttons OpenRefreshSettings, sichtbares RefreshIntervalStatus. UiTest Settingspfad FLOWNRW_UI_TEST_REFRESH_SETTINGS isoliert von Release; vorhandene Modi mit Aus vorbelegen, neue Intervallprobe startet ohne Datei und prüft Standard60. Fixture-Speicherfehler nur im UiTest-Build. Vorhandener FavoriteCalls-Zähler auch im Monitor für Intervallprüfung verfügbar machen.

## Verbindliche Tests

Core: Persistenz/Neustart/Validierung/fehlende und kaputte Datei/Speicherfehler; erster Tick erst nach vollem Delay; Start idempotent; manueller/automatischer Busy-Schutz; unterschiedliche Karten unabhängig; completion-relative Taktung ohne Catchup; Intervallewechsel beendet alten Delay und ignoriert verspätete Antworten; Aus/Seitenwechsel/Foregroundverlust stoppen; erneute Aktivierung genau eine Schleife; Fehler behalten Daten und laufen erst nach nächstem Intervall weiter; keine Ausnahme aus Fire-and-forget.

Native Windows mit tatsächlichen unbeschleunigten30Sekunden: Home→Settings Standard60;30speichern, Prozessneustart erhält30; Einzelmonitor lädt initial und aktualisiert ohne Klick sichtbar (FixtureStand/Zähler); manuelle Aktualisierung parallel mit langsamer automatischer Anfrage zählt nur einmal; mindestens zwei Favoriten aktualisieren unabhängig; Fehlerretention und manuelle Erholung; Aus stoppt über mehr als30Sekunden; erneute30Aktivierung/Intervallwechsel/Navigation kein doppelter Takt. Settings-Speicherfehler behält wirksamen Wert, Retry und Neustart. Navigation auf Suche/Settings stoppt inaktive Monitore; Fensterdeaktivierung stoppt, Aktivierung startet genau einmal. Tastatur/schmale Ansicht und relevante bisherige Routing-/Favoriten-/Monitorregression erneut. UI-Fokus vor echten Wartephasen sicherstellen, ausschließlich eigener Prozess. Kein beschleunigterFixturetimer als Beleg produktiverTaktung.

## Grenzen und offene Punkte

Keine offenen fachlichen Fragen. Windows-Release/Actions bleiben, kein IIS/Deployment/iOS-CI. Native iOS-Abnahme beim Nutzer. Hintergrundeinbindung inSchritt8. Planungsagent nach Ausgangslauf am Nutzungslimit ausgefallen; Plan lokal vervollständigt, unabhängige Prüfung vorImplementierung.
