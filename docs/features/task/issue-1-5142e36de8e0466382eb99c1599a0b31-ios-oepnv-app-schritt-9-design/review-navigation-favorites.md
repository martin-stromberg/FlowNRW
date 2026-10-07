# Anforderungsreview – Navigation und Verbindungsfavoriten

**Nachprüfung:** 04.10.2026
**Grundlage:** `requirement-navigation-favorites.md`, `requirement-departure-ux.md`, `plan-navigation-favorites.md` und aktueller Arbeitsbaum
**Status:** Beide zuletzt geprüften Codebefunde sind behoben. Als Abnahmepunkt bleibt der erfolgreiche native UI-Nachweis offen.

## Nachverfolgung der vorherigen Blocker

- **R1, behoben:** Die kompakte Startseitenübersicht verwendet jetzt `TransitVisuals.Badge` und wird beim Aufklappen ausgeblendet. Die im offenen Zustand doppelte Uhrzeitanzeige ist damit im Code beseitigt.
- **R2 Cache-Fallback, behoben:** Der Suchmonitor liest bei Cachemiss jetzt den persistenten Abfahrtscache, aber nur wenn die Haltestelle in `IFavoriteStore` gespeichert ist. Sessioncache und Auswahlrevision bleiben zusätzlich bestehen.
- **R2 Teilantwort, behoben:** `StopMonitorViewModel` ersetzt das aktuelle Ergebnis nur noch bei vollständigen Antworten. Unvollständige Antworten setzen den Fehlerstatus und lassen den vorigen sichtbaren Bestand bestehen.
- **Abfahrtslimit, behoben:** `DepartureService` markiert nur dann `truncated-response`, wenn ein zusätzliches Ereignis tatsächlich beobachtet wurde. Ein Test für exakt `MaxResults` ist hinzugekommen.
- **Fehlerbehandlung, behoben:** `ResultsPage.OnAppearing` fängt Fehler beim Laden der Verbindungsfavoriten ab.
- **Konsistenz partieller Antworten, behoben:** `FavoriteMonitorViewModel` verwirft jetzt Antworten mit Fehlern oder Warnungen für die Datenübernahme. Ein Integrationstest prüft, dass bei einer Teilantwort sowohl der vorherige sichtbare Bestand als auch das vollständige Linieninventar bestehen bleiben.
- **Frische im Suchmonitor, behoben:** Der persistente Cache-Fallback in `StopMonitorViewModel` verwendet nun `RefreshFreshness` und verwirft abgelaufene Einträge.
- **Frische auf der Startseite, behoben:** `FavoriteHomeViewModel` übergibt `RefreshFreshness` an `FavoriteMonitorViewModel.Restore`; ein deterministischer Test stellt sicher, dass ein altes Ergebnis mit zukünftiger Planzeit nicht angezeigt wird. DI verwendet dieselbe konfigurierte Frischepolicy.

## Verbleibende Befunde

### Mittel – Native Abnahme der neuen Funktionen fehlt

`test-results-navigation-favorites.md` dokumentiert weiterhin, dass `WindowsJourneyUiTests.ps1` nach der Endpunktauswahl hing und beendet wurde. Das belegt nicht die Kopfzeile und das Favoritensymbol, Neustart und Auswahl gespeicherter Verbindungen, Start/Ziel-Tausch oder deren Negativassertionen für Provideraufrufe. Auch die erweiterten Abfahrtscache-Skripte wurden nicht mit einem erfolgreichen nativen Prozesslauf protokolliert. Die Skriptfälle allein schließen die Abnahme nicht.

### Mittel – Verbindungsfavoriten-Persistenz ist noch nicht Ende-zu-Ende geprüft

Der Core-Test deckt JSON-Roundtrip und Duplikate ab; Selection/Swap werden separat gegen ein kontrolliertes ViewModel getestet. Es gibt keinen nachgewiesenen Ablauf über das UI-Symbol, Prozessneustart, Auswahl im Suchformular und anschließende explizite Suche. Die dokumentierte native Journey-UI lief nicht bis zu diesen Schritten.

## Abfahrt-UX: verbleibende Prüfungen

- Die Standortdiagnose für fehlende Berechtigung, deaktivierte Dienste und fehlgeschlagene Positionsbestimmung ist im Testbestand weiterhin nicht vollständig nachgewiesen.
- Vollständige schmale Ansichtsmatrix mit Light/Dark, Textgröße und Abfahrts-/Gleiszuständen ist nicht protokolliert.
- Die Änderungen an `JourneyTimelineView` haben keinen aktualisierten Testnachweis für erhaltene Umstiege, Fußwege/-zeiten und notwendige Hinweise auf fehlende Daten.

## Prüfumfang und Entscheidung

Die aktuelle Testdokumentation nennt **276/276 Core-Tests** und einen UiTest-Build ohne Warnungen/Fehler. In dieser Nachprüfung wurden keine Tests oder Builds ausgeführt. Der native Lauf ist ausdrücklich unvollständig.

**Freigabe:** Die beiden geprüften Codekorrekturen sind angenommen. Offen bleibt der erfolgreiche interaktive Windows-Nachweis der neuen UI-Abläufe; die iOS-Geräteabnahme bleibt separat.
