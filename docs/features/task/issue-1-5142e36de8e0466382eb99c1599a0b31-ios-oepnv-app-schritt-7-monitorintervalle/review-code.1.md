# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine konkrete Produktabweichung im gelesenen Stand gefunden.

## Geprüfte Dateien

- Alle fünf Dateien unter FlowNRW.Core/Refresh: ForegroundState, IRefreshSettingsStore, JsonRefreshSettingsStore, RefreshLoop, RefreshSettingsViewModel.
- FlowNRW/App.xaml.cs, AppShell.xaml.cs, MauiProgram.cs, RefreshSettingsPage.cs.
- FlowNRW/HomePage.cs und DeparturePage.cs, einschließlich Sichtbarkeit, Settingsladen, Fensteraktivität und Abonnementabbau.
- FlowNRW.Core/Presentation/StopMonitorViewModel.cs und FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs, gemeinsame manuelle/automatische Refreshpfade.
- FlowNRW.Tests/RefreshTests.cs im zum Prüfzeitpunkt vorhandenen Stand.

## Hinweise

Unabhängige statische Prüfung am 24.09.2026. Basis: Produktcommit641a625 mit den beim Lesen vorliegenden Arbeitsbaumänderungen (Root-XML-Korrektur), verglichen mit Projektbasis task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app. Spätere parallel erfolgende Produktänderungen sind nicht automatisch durch diesen Bericht abgedeckt.

RefreshLoop wartet vor dem ersten Abruf und nach vollständig abgeschlossenem Abruf erneut; Start ist bei gleichem aktivem Intervall idempotent. Versionsschutz verwirft veraltete Delayantworten. Stop invalidiert vorhandene Monitorrequests nur bei zuvor tatsächlich gestarteter Schleife, daher keine grundlose Invalidierung des initialen OpenStopAsync beim ersten Aktivieren. Manuelle und automatische Aufrufe teilen IsBusy und Revisionsschutz. Jede Favoritenkarte besitzt ihre eigene Schleife und Serviceinstanz.

Settingsladen wird vor Timeraktivierung abgewartet. Entwurf und wirksamer Wert sind getrennt; fehlerhaftes Schreiben ändert den wirksamen Wert nicht. Dateigröße/zulässige Werte sind begrenzt, explizites Speichern darf defekte reine Einstellungen ersetzen. Keine neue sensible Persistenz. UI-Fortsetzungen verwenden keinen Task.Run; Seiten- und Fensterzustand bestimmen Schleifenaktivität. Automatische Statusmeldung ist von manueller Aktualisierung unterschieden.

Keine Builds oder UI-Tests durch diesen Prüfer. Testabschluss und ergänzte deterministische Fälle laufen separat; statisches Review behauptet weder deren Erfolg noch eine iOS-Ausführung. Noch keine Usabilityprüfung dieses Schritts.

Nachprüfung: Zusätzlich RefreshLoopTests_Scheduling.cs, RefreshMonitorTests_Concurrency.cs und RefreshSettingsTests_Persistence.cs vollständig gelesen; vorhandene deterministische Abdeckung im Planreview korrigiert. Keine weiteren Codebefunde. Kein eigener Testlauf.
