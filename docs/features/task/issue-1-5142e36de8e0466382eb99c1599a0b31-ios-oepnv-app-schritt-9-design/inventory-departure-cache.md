# Bestandsaufnahme – Zwischenspeicher für Favoritenabfahrten

**Erfasst:** 03.10.2026
**Bezug:** `requirement-departure-cache.md`

## Bestehender Ablauf

1. `MauiProgram.cs` registriert `IFavoriteStore` als `JsonFavoriteStore` unter `FileSystem.AppDataDirectory/favorites.json` und erzeugt daraus den Singleton `FavoriteHomeViewModel`.
2. `HomePage.OnAppearing` lädt Einstellungen und Favoriten, rendert die Karten und ruft danach `RefreshNearbyAsync` sowie `RefreshMissingAsync` auf.
3. `FavoriteHomeViewModel.LoadAsync` stellt ausschließlich `Stop`-Identitäten wieder her und erzeugt je Favorit ein unabhängiges `FavoriteMonitorViewModel`.
4. `FavoriteMonitorViewModel.RefreshCoreAsync` fragt `IDepartureService.DeparturesAsync` ab, verwirft bei erfolgreicher Antwort vergangene Einträge, setzt `Result` und behält bei Fehlern den vorherigen Stand sichtbar.
5. `RefreshLoop`, `RefreshLifecycle` und `RefreshFreshness` erneuern nur bereits geladene Karten zeitgesteuert beziehungsweise im iOS-Hintergrund.

Es gibt heute keinen dauerhaften Cache für Abfahrten. `MemoryTransitCache` ist absichtlich nur pro Prozess gültig und kann den Neustart nicht überbrücken. `JsonFavoriteStore` speichert laut Schnittstelle und JSON-Kontext nur die technischen `Stop`-Identitäten.

## Betroffene Dateien und Verantwortung

| Datei | Bestehende Verantwortung | Erforderliche Erweiterung |
|---|---|---|
| `FlowNRW.Core/Favorites/IFavoriteStore.cs` | Persistiert ausschließlich die Favoritenliste. | Unverändert als Mitgliedschaftsspeicher belassen; Abfahrten nicht in `favorites.json` vermischen. |
| `FlowNRW.Core/Favorites/JsonFavoriteStore.cs` | Begrenztes, atomares JSON für höchstens 100 `Stop`-Identitäten. | Unverändert lassen. Eine separate, ebenfalls begrenzte atomare Cache-Speicherklasse und Schnittstelle ergänzen. |
| `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs` | Hält `Result`, filtert und sortiert Providerantworten, schützt gegen späte Antworten. | Einen validierten Wiederherstellungspfad für Cache-Ergebnisse und eine Erfolgsmeldung an den Cache ergänzen. Die vorhandene Versionsnummer muss auch Cache-/Provider-Reihenfolgen schützen. |
| `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs` | Baut Karten, lädt Favoriten, aktualisiert fehlende bzw. veraltete Daten, entfernt Favoriten. | Beim Laden Cache-Einträge den Karten zuordnen; beim Entfernen Cache löschen; beim Start eine echte Hintergrundaktualisierung aller Karten starten, auch wenn ein Cache-Ergebnis vorhanden ist. |
| `FlowNRW/HomePage.cs` | Startreihenfolge und sichtbare Karten. | Cache-Karten vor dem Netzwerkrefresh rendern. Der nachfolgende Refresh darf nicht durch `RefreshMissingAsync` ausgelassen werden und darf die Seite nicht blockieren. |
| `FlowNRW/MauiProgram.cs` | Registriert den appweiten Speicher. | Neuen Cache-Store unter `FileSystem.AppDataDirectory` registrieren; UiTest-Pfad analog zu `FLOWNRW_UI_TEST_FAVORITES` isolieren. |
| `FlowNRW.Core/Transit/ProviderResult.cs`, `StopEvent.cs`, `RealtimeStatus.cs`, `TripIdentity.cs`, `Line.cs` | Enthalten alle fachlichen Felder der bestehenden Abfahrtsdarstellung. | Als serialisierbarer Inhalt eines dedizierten Cache-Eintrags verwenden; keine Standortdaten ergänzen. |
| `FlowNRW.Core/Refresh/RefreshFreshness.cs` und `RefreshLifecycle.cs` | Steuern Intervall- und Hintergrundaktualisierung. | Keine Änderung der Alterssemantik für Anbieterantworten; prüfen, dass wiederhergestellte Cache-Daten bei Hintergrund-/Foreground-Refresh nicht fälschlich als frisch gelten. |
| `FlowNRW.Tests/FavoriteTests.cs`, `RefreshMonitorTests_Concurrency.cs`, `RefreshLifecycleTests_Background.cs` | Decken Karten, Fehlererhalt, Nebenläufigkeit und Hintergrunderneuerung ab. | Um Cache-Roundtrip, Wiederherstellung, Filterung, Überschreiben, Entfernung und Startrefresh ergänzen. |
| `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`, `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1` | Liefert verzögerte Fixture-Abfahrten und steuert die native Windows-Matrix. | Neues Szenario: Cache sofort sichtbar, frische Antwort verzögert; Nachweis für sichtbaren Cache und anschließende Ersetzung. |

## Schlüssel, Zeitstempel und Gültigkeit

Ein Cache-Eintrag muss mit der vollständigen Favoritenidentität `(Stop.Source, Stop.Id)` adressiert werden. Der Anzeigename, DHID und Koordinaten sind kein Primärschlüssel. Dadurch werden gleichnamige Haltestellen verschiedener Anbieter nicht vermischt.

Für jeden erfolgreichen Eintrag sind nötig:

- die Schlüsselidentität der Haltestelle,
- die bereits normalisierten `StopEvent`-Daten, die die heutige Abfahrtskarte benötigt,
- `ProviderResult.Source`, Warnungen und der Zeitpunkt der erfolgreichen Antwort `ProviderResult.RetrievedAt`,
- ein lokaler Speichermoment nur für Bereinigung/Diagnose, falls der Providerzeitpunkt fehlt oder offensichtlich ungültig ist.

Die fachliche Gültigkeit jedes einzelnen Eintrags kommt bereits aus `FavoriteMonitorViewModel.EffectiveTime`: `Realtime.ActualTime`, sonst `PlannedTime + Realtime.Delay`, sonst keine sortierbare Zeit. Für den Persistenz- und Wiederherstellungspfad muss dieselbe Regel zentral erreichbar sein. Beim Lesen und vor dem Speichern werden nur Einträge mit effektiver Zeit `>= now` in lokaler Zeitzone/als `DateTimeOffset` übernommen und nach effektiver Zeit sortiert. Ein Eintrag ohne zukünftige Abfahrt wird aus dem dauerhaften Cache entfernt oder als leerer Erfolg ersetzt; er darf nicht als aktuelle Abfahrt erscheinen.

`RefreshFreshness` verwendet die 30-Sekunden-`RealtimeTimeToLive` und ist für die Erneuerungsentscheidung gedacht. Diese Grenze allein eignet sich nicht als Sichtbarkeitsgrenze beim Neustart: Eine 31 Sekunden alte Cache-Datei mit einer Abfahrt in fünf Minuten soll sofort sichtbar sein. Der Cachezeitpunkt kennzeichnet deshalb Herkunft und Fehlerfall; die Abfahrtszeit entscheidet über die Anzeige.

## Start- und Refreshfolge

Die Zielreihenfolge sollte sein:

1. Favoritenidentitäten laden.
2. Zugehörige Cache-Einträge lesen, ungültige/abgelaufene Abfahrten bereinigen und die Karten synchron mit den verbleibenden Ergebnissen befüllen.
3. Karten sofort rendern; für Cache-Daten einen verständlichen, nichttechnischen Hinweis wie „Letzter Stand wird aktualisiert“ setzen.
4. Ohne die Anzeige abzuwarten, pro Karte eine reguläre automatische Anbieterabfrage starten. Die vorhandenen Busy- und Revisionsschutzregeln bleiben wirksam.
5. Eine erfolgreiche Antwort ersetzt den Kartenstand und schreibt den bereinigten neuen Erfolg. Fehler lassen die wiederhergestellten Einträge stehen und zeigen den bestehenden verständlichen Fehlerhinweis.

Der aktuelle Aufruf `RefreshMissingAsync` ist hierfür nicht ausreichend: Er lädt absichtlich nur Karten mit `Result is null`. Sobald ein Cache-Ergebnis `Result` setzt, würde beim Start keine sofortige Netzabfrage erfolgen. Dafür ist ein eigener, nicht blockierender Startrefresh für alle nicht belegten Karten nötig; er darf nicht mit der bereits aktiven Intervall- oder Hintergrundaktualisierung konkurrieren.

## Risiken und Schutzmaßnahmen

- **Alte Einträge:** Filterung nach effektiver Abfahrtszeit sowohl beim Lesen als auch beim erfolgreichen Speichern; Cache nach der Filterung zurückschreiben oder Eintrag löschen.
- **Zeit-/Zeitzonenfehler:** Durchgehend `DateTimeOffset` verwenden; keine Umwandlung in lokale `DateTime` für Vergleiche.
- **Späte Antworten:** Der vorhandene Revisionsschutz darf nicht von einer Cache-Wiederherstellung zurückgesetzt werden. Eine ältere, nach einer neueren Antwort eintreffende Providerantwort darf weder Anzeige noch Cache überschreiben.
- **Fehlerhafte/zu große Cache-Datei:** Wie `JsonFavoriteStore` begrenzt lesen, JSON-Fehler abfangen, die defekte Datei nicht überschreiben und mit leerem Cache weiterarbeiten. Favoriten bleiben davon unabhängig nutzbar.
- **Verwaiste Daten:** Beim Entfernen eines Favoriten den passenden Eintrag löschen; beim Start Einträge entfernen, deren Schlüssel nicht mehr in der geladenen Favoritenliste vorkommt.
- **Teilweise Speicherung:** Favoritenmitgliedschaft und Abfahrtscache bleiben getrennt. Scheitert das Cache-Schreiben nach einer erfolgreichen Providerantwort, bleibt die frische Anzeige gültig; die App meldet keinen irreführenden Ladefehler.
- **Datenschutz:** Keine Koordinaten oder Suchhistorie in den Cache aufnehmen. Nur Abfahrtsdarstellung eines bereits gespeicherten Favoriten speichern.
- **Startleistung:** Favoriten und Cache einmalig/bounded laden; keine Einzeldatei pro Karte und keine serielle Netzabfrage. Die bestehende Begrenzung von vier gleichzeitigen Hintergrund-Updates kann beibehalten werden.

## Testplan

### Core-Tests

1. Cache-Store schreibt und lädt getrennte Einträge für gleiche Namen mit unterschiedlichem `(Source, Id)`.
2. Wiederherstellung zeigt nur Einträge, deren effektive Echtzeit- oder Sollzeit noch nicht vergangen ist; reine Vergangenheitsdaten ergeben eine leere Karte.
3. Ein gültiger Cache wird vor Abschluss einer kontrolliert verzögerten Anbieterabfrage als `Result` sichtbar.
4. Die Startaktualisierung ruft trotz gesetztem Cache-Ergebnis genau eine Anbieterabfrage auf.
5. Eine erfolgreiche aktuelle Antwort ersetzt Cache und Anzeige; ein nachfolgender Neustart liefert den neuen, nicht den alten Stand.
6. Fehler, Offline und Abbruch behalten einen gültigen wiederhergestellten Stand, ohne private Ausnahmeinhalte anzuzeigen.
7. Entfernen eines Favoriten löscht dessen Cache; erneutes Speichern derselben Identität stellt keine alten Einträge wieder her.
8. Defekte, übergroße und verwaiste Cache-Daten blockieren weder Favoritenladen noch eine frische Abfrage.
9. Nebenläufigkeit: Cache-/Providerreihenfolge sowie doppelte Start-, Intervall- und Hintergrundanfragen erzeugen keine Überschreibung durch eine ältere Antwort.

### Windows-UI-Test

1. In einem ersten Fixture-Lauf einen Favoriten mit zukünftigen Abfahrten erfolgreich laden und die App-Daten beibehalten.
2. Beim zweiten Lauf die Anbieterantwort künstlich verzögern. Direkt nach Start müssen die gespeicherten Abfahrtszeilen sichtbar sein und ein zurückhaltender Aktualisierungshinweis vorhanden sein.
3. Nach Fixture-Antwort müssen sichtbare Zeilen und Datenstand durch die neue Antwort ersetzt sein.
4. Wiederholung mit nur vergangenen Cache-Abfahrten: Vor der Antwort keine alte Abfahrtszeile.
5. Wiederholung mit Providerfehler: gültiger Cache bleibt sichtbar, Fehlermeldung verständlich.
