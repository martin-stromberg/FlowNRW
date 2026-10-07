# Code Review – Verdichtete mobile Abfahrtsanzeige

**Nachprüfung:** 04.10.2026
**Umfang:** zuvor dokumentierte Befunde gegen den aktuellen Arbeitsbaum
**Status:** Frühere Codebefunde behoben; Paket-7-Abdeckung deutlich erweitert, aber noch nicht freigabefähig.

## Nachverfolgung früherer Befunde

### Behoben – Fehlende Nutzdaten als leere Antwort

`TransitJson.Map` verlangt jetzt je Mapper ein strukturell vorhandenes Array. Fehlende oder anders typisierte Felder liefern `invalid_response`; ein vorhandenes leeres Array kann weiterhin einen gültigen vollständigen Leerstand darstellen. Neue Mapper-Tests decken ein fehlendes Pflichtfeld für DB-REST-Abfahrten und EFA-Suche ab. Die Cachelogik akzeptiert Fehler nicht als vollständige Antwort.

### Behoben – Kürzung durch den Provider-Orchestrator

`MergeEvents` und `MergeJourneys` liefern bis zu `maxResults + 1` Elemente. `Execute` erkennt einen Überhang, kappt auf das konfigurierte Limit und ergänzt `truncated-response`, wodurch die bestehende Inventarregel diese Antwort nicht als vollständig speichert. Der Union-Test erwartet die Markierung für Routen und Abfahrten.

### Behoben – Diagnose bei fehlenden Haltestellenkoordinaten

Nach erfolgreicher Positionsbestimmung meldet `FavoriteHomeViewModel` die Anzahl der Favoriten ohne Koordinaten. Die Startseite lässt diesen Zustand sichtbar; mit vorhandenen Koordinaten wird die allgemeine Erfolgsmeldung ausgeblendet. Die Position aus `RefreshNearbyAsync` wird außerdem zur Berechnung der Favoritenentfernungen verwendet.

## Verbleibende Reviewpunkte

### Mittel – Assertions beweisen Linienersetzung und Leeren nicht vollständig

Das neue Skript prüft B/C über `-match 'B.*C'`, was auch dann besteht, wenn alte Linien zusätzlich erhalten bleiben. Nach dem vollständigen leeren Ergebnis prüft es lediglich das Fehlen von B/C; ein falscher alter Bestand wie RE 1/S2 würde ebenfalls bestehen. Die Assertions müssen den erwarteten vollständigen Linienbestand prüfen und die alte Linie gezielt ausschließen; nach dem leeren Ergebnis muss die Karte tatsächlich leer sein und ein Neustart die Leere bestätigen.

### Mittel – Geforderte Szenarien bleiben ungetestet

`WindowsDepartureCacheUiTests.ps1` deckt jetzt sinnvolle Hauptpfade ab: initiales Persistieren, eingeklappte Startanzeige, verzögerter Startabruf, sofortige Detailübernahme, Ladeindikatoren, Startseitenfehler, Restart, Warnungsantwort, vollständige Ersetzung und vollständiges Leeren. Offen bleiben explizit nur vergangene Abfahrten mit zeitlosen Linien über Neustarts, Detailfehler und Detailabbruch, kontrollierter Lifecycle-Abbruch statt erzwungenem Prozessende, explizite Prüfung dass Zeit bei einer zeitlosen Linie fehlt, und die geforderte Erfolgs-/Fehler-/Abbruchbehandlung samt Aktualisierung beider Darstellungen. Die Standort-Fixtures für fehlende Berechtigung, deaktivierte Dienste und fehlgeschlagene Position sind im neuen Diff nicht erweitert.

Die Designtests capturen jetzt expandierte und eingeklappte Favoriten und prüfen den Koordinatenhinweis. Die geforderte vollständige schmale Ansichtsmatrix (Light/Dark und 100/150 Prozent Textgröße samt Zuständen gleicher/abweichender/fehlender Echtzeit und Gleisänderung) ist im neuen Diff nicht nachgewiesen.

### Mittel – Aktuelle Testausführung nicht belegt

Das vorhandene `test-results-departure-cache.md` dokumentiert einen älteren Stand des Skripts und hält ausdrücklich fest, dass der native Lauf noch aussteht. Für die jetzt geänderten Fixture-Skripte liegt kein erfolgreicher nativer Prozessnachweis im Arbeitsbaum vor. Es wurden während dieser Prüfung keine Tests oder Builds ausgeführt.

## Reviewentscheidung

**Vorherige Codeblocker geschlossen; Gesamtfreigabe weiterhin ausstehend.** Die neuen UI-Skripte sind ein sinnvoller Fortschritt, ihre Inventarassertions müssen gegen falsche positive Ergebnisse gehärtet werden. Danach fehlen noch die genannten Anforderungen und erfolgreiche Prozess-/Screenshot-Nachweise.
