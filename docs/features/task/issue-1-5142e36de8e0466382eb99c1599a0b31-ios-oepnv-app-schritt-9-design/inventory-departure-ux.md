# Bestandsaufnahme – Abfahrtsanzeige und Favoriten

**Erfasst:** 04.10.2026  
**Bezug:** [Anforderung – Verdichtete mobile Abfahrtsanzeige](requirement-departure-ux.md)

## Standort und unbekannte Entfernung

Der Code erklärt das beobachtete Verhalten ohne Annahme über den GPS-Empfang: Beim Laden der Startseite lädt `FavoriteHomeViewModel.LoadAsync` die Favoriten und anschließend ruft `SortCards()` für jede Karte `SetDistance(...)` auf. Das private Feld `position` ist zu diesem Zeitpunkt `null`; damit wird jede `DistanceMeters` ebenfalls `null` und `DistanceLabel` liefert „Entfernung unbekannt“.

Eine Position für die Favoritenentfernungen wird ausschließlich durch `LocationCommand` angefordert, das an den Shortcut „Entfernungen aktualisieren“ gebunden ist. Die laufende Standortabfrage für nahe Haltestellen in `RefreshNearbyAsync()` ermittelt zwar ebenfalls eine Position, übergibt sie aber nur an die Umkreissuche. Sie setzt nicht `FavoriteHomeViewModel.position` und löst keine Neuberechnung der Favoritenentfernungen aus. Deshalb führt aktiviertes GPS beim Programmstart allein nicht zu berechneten Entfernungen. Nach einer expliziten Anforderung kann die Entfernung weiterhin unbekannt sein, wenn `Stop.Coordinate` fehlt: `SortCards()` berechnet nur bei vorhandener Position **und** Haltestellenkoordinate eine Distanz.

Der Plattformadapter `MauiCurrentLocationService` prüft die App-Berechtigung, den aktivierten Systemstandortdienst, fordert eine Position mit 15-Sekunden-Zeitlimit an und prüft deren Zeitstempel. Seine Ergebnisse unterscheiden verweigerte Berechtigung, deaktivierten Dienst, Zeitüberschreitung und fehlende Position. Die Standortkette selbst enthält also konkrete Fehlerzustände; die Startseitenlogik startet diese Abfrage für Favoriten aber nicht automatisch und zeigt die koordinatenlose Haltestelle nur als „Entfernung unbekannt“.

Die gespeicherten Favoriten werden als `Stop` serialisiert und können eine Haltestellenkoordinate enthalten. Der Abfahrtscache entfernt Koordinaten aus den Ereignissen, verändert aber nicht die separat gespeicherte Favoritenidentität. Eine belastbare Aussage zum konkreten Smartphone-Fall (ob Standortberechtigung erteilt ist oder ob der gewählte `Stop` Koordinaten enthält) lässt sich aus dem Quellcode allein nicht treffen; die deterministische Ursache des Startzustands ist die nicht initialisierte `position`.

## Abfahrtskarte

`DepartureCardView` erzeugt aktuell mehrere redundante Texte:

- Der Kopf zeigt `Realtime.ActualTime ?? PlannedTime` rechts neben Linie und Ziel.
- In der Detailkarte (`compact: false`) folgt eine Vergleichszeile mit Soll-Zeit und Ist-Zeit beziehungsweise „keine Echtzeit“.
- In der Favoritenkarte (`compact: true`) folgt ebenfalls eine Textzeile mit Soll und Ist. Die Ist-Zeit steht somit doppelt, wenn sie vorhanden ist.
- Eine weitere Statuszeile zeigt „Verspätung unbekannt“, „Pünktlich gemeldet“ oder „Abweichung … Min.“.
- Die Plattformzeile enthält immer aktuelle und geplante Gleisangabe, auch wenn beide gleich sind oder eine Angabe fehlt.
- Eine Quellenzeile befindet sich nicht in `DepartureCardView`; die Favoritenansicht zeigt die Provenienz separat über `FavoriteMetadata`. Die Detailseite zeigt `MonitorMetadata`.
- In `DeparturePage` werden Ladezustand als `MonitorStatus`-Text („Abfahrten werden aktualisiert …“) und separat als `ActivityIndicator` ausgegeben. Die Favoritenkarte zeigt den Ladezustand über `FavoriteStatus` als Text und hat dort kein Ladesymbol.

## Detailansicht und vorhandene Daten

`FavoriteHomeViewModel` lädt beim Start die gespeicherten `DepartureCacheEntry`-Ergebnisse. `FavoriteMonitorViewModel.Restore` übernimmt nur erfolgreiche, nicht veraltete Cacheeinträge mit mindestens einer noch bevorstehenden Abfahrt. Ein erfolgreicher Abruf ersetzt `Result`; Fehler behalten bisherige Daten der Karteninstanz bei.

Die Detailansicht verwendet jedoch einen getrennten Datenzustand im gemeinsam registrierten `StopMonitorViewModel`. `OpenFavoriteAsync` übergibt aus der Startseite lediglich `card.Stop`. `OpenStopAsync` setzt `Result` und `LastAttempt` explizit auf `null`, navigiert zur Seite und startet anschließend den Providerabruf. Es gibt weder eine Cache-Abhängigkeit im `StopMonitorViewModel` noch eine Übernahme von `card.Result`. Dadurch bleibt die Detailansicht leer, bis ihre eigene Anfrage zurückkehrt, obwohl die Favoritenkarte schon Daten hält.

## Persistenz und eingeklappte Favoriten

Die Startseite rendert derzeit alle geladenen Favoritenabfahrten sofort: `RenderCards()` ruft `RenderDepartures()` auf und hängt die Abfahrtsliste direkt an jede Karte. Ein expandierter/eingeklappter Zustand existiert nicht.

Das aktuelle Cache-Schema (`DepartureCacheEntry`) speichert pro technischer Identität (`Source` + `StopId`) ein vollständiges `ProviderResult<StopEvent>`. Es gibt kein separates persistiertes Linieninventar. Beim Cache-Schreiben werden nur Abfahrten mit zukünftiger effektiver Zeit übernommen; beim Wiederherstellen werden vergangene/zeitlose Events herausgefiltert, und ein Cache ohne verbleibende Zukunftsabfahrt wird verworfen. Damit kann die App heute keine Linie ohne nächste bekannte Zeit über einen Neustart hinweg anzeigen. Der Cache ist auf 100 Favoriten und 100 Ereignisse je Eintrag begrenzt und enthält keine Haltestellenkoordinaten in den Abfahrtsereignissen.

Das Entfernen eines Favoriten speichert die neue Favoritenliste und entfernt anschließend den Cacheeintrag unter derselben Identität. Beim Laden werden Cacheeinträge entfernt, deren Schlüssel nicht zu den gespeicherten Favoriten gehören.

## Vorhandene UI-Prüfungen

- `WindowsDepartureCacheUiTests.ps1` prüft, dass eine noch gültige gespeicherte Abfahrt vor Abschluss einer verzögerten Hintergrundanfrage sichtbar ist. Diese Prüfung nutzt die Startseite.
- `WindowsJourneyUiTests.ps1` prüft die unbekannte Entfernung vor einer expliziten Standortaktion, eine anschließend bekannte Entfernung bei verfügbaren Fixture-Koordinaten sowie Fälle ohne bekannte Koordinate und Locationfehler.
- `WindowsDesignUiTests.ps1` nimmt Startseiten-/Detailansichten als Screenshots auf und prüft Standortsortierung.
- `WindowsRefreshUiTests.ps1` und `WindowsLifecycleUiTests.ps1` prüfen Aktualisierung, Fehlerbeibehaltung und Statusmeldungen für Favoritenkarten.
- In den gefundenen UI-Skripten gibt es keine Prüfung für standardmäßig eingeklappte Favoriten, persistierte Linien ohne Uhrzeit oder das Löschen einer Linie nach erfolgreicher vollständiger Antwort.
- Die vorhandene Cache-UI-Prüfung deckt nicht das unmittelbare Übernehmen der Cacheabfahrten in die Favoritendetailansicht ab.

## Konsequenzen für die Umsetzung

1. Eine erfolgreiche Standortabfrage muss den gemeinsamen Positionszustand für Favoritendistanzen aktualisieren; die Umkreissuche darf nicht als Ersatz dafür missverstanden werden. Fehlende Haltestellenkoordinaten sind getrennt von Standortfehlern zu behandeln.
2. `DepartureCardView` braucht eine gemeinsame verdichtete Zeitdarstellung für kompakte und vollständige Karten. Ladeanzeige und Quellen-/Status-Metadaten müssen getrennt von den eigentlichen Abfahrtsdaten behandelt werden.
3. Die bereits geladene Karteninstanz oder ihr gültiges Ergebnis muss beim Öffnen in den Detailmonitor übernommen werden, bevor der Aktualisierungsabruf startet.
4. Für den kompakten Linienüberblick reicht das aktuelle Cacheformat nicht aus. Ein persistiertes Linieninventar muss je Favoritenidentität separat beziehungsweise als eigenständiger Teil des Cacheeintrags geführt und nur durch erfolgreiche vollständige Antworten ersetzt werden.
