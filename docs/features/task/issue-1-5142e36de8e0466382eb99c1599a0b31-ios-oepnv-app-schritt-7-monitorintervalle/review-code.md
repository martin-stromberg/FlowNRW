# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine offenen Codebefunde nach erneuter Prüfung des korrigierten Command-Lebenszyklus.

## Geprüfte Dateien

- FlowNRW/HomePage.cs, vollständig gelesen einschließlich RenderCards, Abonnements, Kartenwechsel und RefreshLoop-Verwaltung.
- FlowNRW.Core/Presentation/AsyncRelayCommand.cs: CanExecuteChanged bei Refresh, InvalidateExecution und Abschluss einer laufenden Aktion.
- FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs: MutateAsync, Persistenz vor Modellübernahme, CancelPending und SortCards.
- FlowNRW/DeparturePage.cs: aktuelle zusätzliche UiTest-Foregroundanzeige und sonstige Änderungen seit vorheriger Prüfung.
- Vorheriger vollständiger Schritt7-Core-/Integrations-/Testreview bleibt im archivierten review-code.1.md nachvollziehbar.

## Nachprüfung und Prüfbasis

25.09.2026, tatsächlicher Arbeitsbaum nach Stabilmeldung des UI-Agenten und erneutem Lesen. HomePage SHA256: `938E7090621A07F58E24FDA3AD8245E70120600EB7C73F6BC7A07B1C267E9DF9`. Alle drei neu erzeugten Kartenschaltflächen (Refresh, Öffnen, Entfernen) werden über gespeicherte unsubscribe-Aktionen von ihren Commands gelöst. Diese Aktionen laufen vor dem Leeren der Kartenliste; auch das PropertyChanged-Abonnement für die Abfahrtsdarstellung wird entfernt. Danach werden ausschließlich neue Buttons an Commands gebunden.

Der ursprüngliche Fehlerpfad ist anhand des Codes nachvollziehbar: Ein abgehängter Refreshbutton blieb an einem weiterlebenden Kartencommand, dessen CancelPending erneut CanExecuteChanged auslöste. Auch Remove kann nach dem Kartenneubau im finally von AsyncRelayCommand erneut Refresh auslösen; deshalb sind die analogen Abkopplungen von Öffnen/Entfernen erforderlich und jetzt vorhanden. Der Fix beseitigt die alten Command-Abonnements statt die UI-Ausnahme zu verschlucken oder einen erfolgreichen Speicherzustand zurückzunehmen. Favoritenpersistenz und Revisionslogik wurden hierfür nicht verändert; keine diagnostische Exceptionausgabe im Core-Diff verbleibt.

Die zusätzliche RefreshForeground-Anzeige in DeparturePage liegt ausschließlich unter UI_TEST_FIXTURES und exponiert keine personenbezogenen Daten. Keine zusätzliche Releaseoberfläche eingeführt.

Der UI-Agent meldete einen erfolgreichen Favoritenlauf nach dem ersten Refreshbutton-Fix; nach dem vollständigen Dreifach-Fix liefen die abschließenden Regressionen bei Berichtserstellung noch. Dieser statische Bericht behauptet diese nicht vorzeitig als bestanden. Keine Builds, UI-Tests, Gitmutationen oder Produktkorrekturen durch diesen Prüfer. Planreview bleibt bis zum abschließenden Testnachweis unverändert offen.
