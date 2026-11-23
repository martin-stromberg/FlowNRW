# Umsetzungsplan – Favoriten, Haltestellensuche und Verbindungen

**Stand:** 04.10.2026  
**Grundlage:** `requirement-navigation-favorites.md`, `inventory-navigation-favorites.md`  
**Status:** In kleine, einzeln prüfbare Pakete aufgeteilt

## Leitlinien

- Erst die offene Abnahme und die Testlücken des bereits begonnenen Abfahrt-UX-/Cachepakets schließen. Dessen Datenintegritätskorrekturen sind im Arbeitsbaum, aber die E2E-Tests sind noch nicht abgenommen.
- Cache-Erweiterungen bleiben rückwärtskompatibel zu bestehenden `departure-cache.json`-Dateien. Neue Verbindungsfavoriten erhalten eine getrennte Datei und ein versioniertes, validiertes Speicherformat; vorhandene Haltestellenfavoriten werden nicht migriert oder verändert.
- Alle Monitorzugänge teilen einen begrenzten Sessioncache nach `(Source, StopId)`. Nur explizit gespeicherte Stopfavoriten dürfen zusätzlich in `departure-cache.json` geschrieben werden. Suchtexte, Trefferlisten und Standortkoordinaten werden nicht neu persistent gespeichert; nicht favorisierte Suchhaltestellen hinterlassen keinen Datenträgerverlauf. Nur nicht veraltete Abfahrten werden sofort angezeigt. Cachefehler, leere Cachetreffer, Providerfehler und Abbruch dürfen gültige Daten nicht verwerfen.
- Jedes Paket endet mit gezielten Core-Tests, UiTest-Build mit Warnings-as-Errors und aktualisierter Testdokumentation. Native Windows-Prozessläufe werden als tatsächlich ausgeführt protokolliert; Skript-Assertions allein gelten nicht als Abnahme. iOS bleibt Geräteabnahme.

## Paket 0 – Vorheriges Abfahrt-UX-/Cachepaket belastbar abschließen

**Zweck:** Keine neue Suchcache-Nutzung auf einem noch unzureichend geprüften Cachevertrag aufbauen.

1. `WindowsDepartureCacheUiTests.ps1`-Assertions verschärfen: bei Ersetzung durch B/C müssen alle alten Linien ausgeschlossen sein; beim leeren Ergebnis muss die Übersicht tatsächlich leer sein und nach Neustart leer bleiben.
2. Fehlende Szenarien ergänzen: vergangene Abfahrten plus zeitlose Linien über Neustart, zeitlose Linie ohne Uhrzeit, Detailfehler, Detailabbruch und kontrollierter Lifecycle-Abbruch. Prüfen, dass Erfolgs-, Fehler- und Abbruchpfade Start- und Detaildarstellungen konsistent behandeln.
3. Standort-Fixtures für verweigerte Berechtigung, deaktivierte Standortdienste und fehlgeschlagene Positionsbestimmung ergänzen. Sicherstellen, dass fehlende Haltestellenkoordinaten diagnostiziert werden.
4. `WindowsDesignUiTests.ps1` um die noch fehlenden schmalen Zustände und Light/Dark-/Textgrößenläufe erweitern, soweit Windows sie zuverlässig abbilden kann.
5. Core-Tests und UiTest-Build ausführen. Die Cache-Fixture in einer interaktiven Windows-Sitzung tatsächlich starten und Prozess-/Assertionsergebnis protokollieren; den bekannten früheren Lauf mit verstecktem Fenster nicht als Erfolg werten.

**Abnahmekriterium:** Alle in `review-code-departure-ux.md` und `review-departure-ux.md` benannten offenen mittleren Befunde sind entweder mit reproduzierbarem Test geschlossen oder ausdrücklich als manuelle Geräteabnahme abgegrenzt. Vor Paket 1 darf kein Datenintegritätsbefund offen sein.

## Paket 1 – Linienchips und Auf-/Zuklappen (R1)

- Kompakte Anzeige verwendet dieselben farbigen Linienchips wie Abfahrtskarten; Zeitangaben erscheinen nur in der eingeklappten Übersicht, nicht zusätzlich im geöffneten Zustand.
- Symbole und Automation-/Semantic-Properties für Auf-/Zuklappen vereinheitlichen. Beschriftung muss Status und Aktion ausdrücken; Touch-Ziel bleibt mindestens 44×44.
- Testen: Projektion für mehrere Linien mit/ohne Zeit, keine Zeit-Dopplung im offenen Zustand, standardmäßig eingeklappt, Umschalten und Accessibility-Name.
- Verifikation: Core-Tests für Projektion, UiTest-Build und Windows-UI-Szenario mit Screenshot in schmaler Ansicht. Expand-Zustand bleibt zunächst nicht persistent, da R1 das nicht fordert.

## Paket 2 – Trefferliste und Suchzustand bei Haltestellen-Navigation (R2, UI-Zustand)

- Suchtreffer unabhängig von der aktuell im Monitor angezeigten Haltestelle halten, statt sie durch `SelectAddress` zu leeren.
- Beim Zurückkehren die letzte Trefferliste und den Suchtext rekonstruieren; Nearby-Modus und explizite Textsuche dürfen einander nicht unbeabsichtigt überschreiben.
- Die Identität der ausgewählten Haltestelle bleibt getrennt von den Trefferobjekten. Veraltete Treffer werden erst bei einer neuen Suche ersetzt.
- Suchtext, Trefferliste und Nearby-Modus bleiben ausschließlich im laufenden Prozess. Jede Monitorwahl und jedes Schließen invalidiert eine Auswahlrevision; Cachelesen und Providerabruf erfassen diese Revision sowie `(Source, StopId)` vor ihrem Start. Vor jeder UI-Übernahme müssen beide noch aktuell sein. Suchtrefferzustand wird von einer Monitorantwort nicht verändert.
- Testen: Suche, Haltestelle öffnen, zurück, Treffer und Suchtext bleiben sichtbar; erneute Auswahl funktioniert. Zusätzlich Navigationsabbruch während eines Suchabrufs.
- Verifikation: ViewModel-Tests und Windows-UI-Regression. Dieses Paket ändert noch nicht den Persistenzumfang des Cache-Stores.

## Paket 3 – Abfahrtscache für aus der Suche geöffnete Haltestellen (R2, Session)

- `StopMonitorViewModel` erhält einen gemeinsamen Sessioncache. Beim Öffnen erfolgt zuerst dessen Lookup; nur für aktuell gespeicherte Stopfavoriten darf bei fehlendem Sessioneintrag der bestehende `IDepartureCacheStore` gelesen werden. Der gültige Bestand wird sofort projiziert, anschließend läuft der Anbieterabruf. Vor jeder Projektion wird die bestehende Frischeprüfung erneut angewandt; inzwischen vergangene Abfahrten verschwinden auch bei wiederholtem Sessionhit.
- Sessiongrenze: maximal 100 Haltestellen mit jeweils höchstens 100 Abfahrten und 100 Linien, entsprechend den vorhandenen Einzelbestandsgrenzen. Lesen und Schreiben aktualisieren eine monotone LRU-Reihenfolge. Bei neuer Identität wird der am längsten unbenutzte Eintrag entfernt; der gerade geöffnete Monitor bleibt währenddessen geschützt. Überschreitet ein kompletter Anbieterbestand eine Einzelgrenze, wird er nicht als vollständiger Cachebestand gespeichert und ersetzt keinen bestehenden vollständigen Bestand. Sessioneviction betrifft niemals die separate JSON-Favoritenablage.
- Cache wird nur nach erfolgreicher, vollständiger und nicht veralteter Antwort unter der tatsächlich angefragten Identität aktualisiert. Providerfehler, Teilantwort, Kürzung oder Lifecycle-Abbruch lassen vorherige valide Abfahrten und Linieninventare unangetastet. Eine überholte Auswahlrevision wird verworfen; ihre Antwort darf weder die neue UI noch deren Cacheeintrag überschreiben.
- Auswahlrevision und Identität werden auch nach einem verzögerten persistenten Cachelesen erneut geprüft. Ein später Cachehit darf keinen inzwischen angezeigten neueren Providerbestand ersetzen. Schließen/Zurücknavigation invalidiert die Revision ebenso wie ein Wechsel A → B.
- Die JSON-Persistenz bleibt auf aktuell gespeicherte Stopfavoriten begrenzt und formatkompatibel. Favoritenentfernung und `RemoveOrphansAsync` bereinigen nur diese Ablage und löschen keine unabhängigen Sessionbestände. Ein aus der Suche geöffnetes Nichtfavoritenergebnis wird niemals in JSON geschrieben, auch nicht über einen generischen Monitor-Erfolgspfad.
- Testen: frischer Hit vor verzögerter Antwort, identische Haltestelle nach erneutem Öffnen, stale/missing cache, Erfolg ersetzt vollständig, Fehler/Abbruch erhält Bestand, gleiche StopId bei anderer Quelle bleibt getrennt, konkrete LRU-/Einzelgrenzen und JSON-Altformat. Verzögerte Antwort A nach Wechsel zu B, Zurücknavigation während Cachelesen und Wiederöffnen nach Ablauf einer Abfahrt prüfen. Sichtbar bleibt ausschließlich die aktuelle Identität; Cachebestände bleiben ihren Anfragen korrekt zugeordnet.
- Datenschutztest: Nichtfavorit öffnen und erneut öffnen zeigt im selben Prozess gültige Abfahrten, fügt aber keinen JSON-Eintrag hinzu; nach Prozessneustart werden nur ausdrücklich persistierte Favoriten wiederhergestellt. Suchtext, Trefferliste und Sessionbestände sind dann leer. Favoritenentfernung/Orphanbereinigung lassen sonstige Sessioneinträge nutzbar.
- Verifikation: Store-/ViewModel-Integrationstests plus Windows-UI-Test mit künstlich verzögertem Anbieter, insbesondere Stopwechsel A → B und Wiederöffnen vor der Antwort. Aufzeichnungen müssen Cachezustand vor und nach der Antwort sowie die JSON-Persistenzgrenze belegen.

## Paket 4 – Persistente Verbindungsfavoriten (R3, Datenvertrag zuerst)

- Eigenständigen `ConnectionFavorite`-Datentyp mit Start-/Zielidentität, Quelle und Anzeigenamen definieren. Technische Kennungen bleiben intern.
- Nur ausdrücklich gespeicherte Verbindungen werden persistiert; ihr Inhalt beschränkt sich auf die zur Wiederverwendung nötigen Endpunktdaten. Suchtexte, Ergebnislisten, Abrufverlauf und aktuelle Standortkoordinaten gehören nicht in diese Datei. Eine Verbindung mit einem reinen Standortendpunkt wird erst nach Auswahl einer speicherbaren Haltestellenidentität favorisiert.
- Separaten `IConnectionFavoriteStore`/JSON-Store einführen. Formatversion, Validierung, atomisches Schreiben, Duplikatregel und eine begrenzte Anzahl festlegen. Ungültige/neue unbekannte Felder dürfen vorhandene gültige Favoriten nicht beschädigen.
- Keine Migration einzelner Stopfavoriten: bestehende `favorites.json` bleibt unverändert. Für neue Verbindungsfavoriten wird bei fehlender Datei eine leere Sammlung verwendet; alte Installationen erhalten dadurch keine künstlichen Einträge.
- Testen: Roundtrip, Neustart, Duplikate, Löschen, beschädigtes JSON, unbekannte Version, fehlende Namen/Identitäten und Schreibfehler.
- Verifikation: Store-Unit-Tests vor jeder UI-Anbindung.

## Paket 5 – Verbindungsfavoriten anzeigen und auswählen (R3, UI)

- Kopfzeile in der Ergebnisansicht zeigt Start und Ziel sowie ein zugängliches Favoritensymbol, das Speichern/Entfernen umschaltet.
- Suchformular zeigt gespeicherte Verbindungen als Auswahl; Auswahl setzt beide Endpunkte einschließlich stabiler Adresse direkt und startet keine Haltestellensuche.
- Nach Auswahl bleiben Abfahrtszeit und Ankunftsmodus wie bisher im Suchsessionzustand. Nicht verfügbare Favoriten werden verständlich angezeigt und können entfernt werden.
- Testen: Kopfzeile bei mehreren Ergebnissen, Toggle, Reload aus dem Store nach Neustart, Endpunktbefüllung, doppelte Verbindung, leerer Zustand und ungültiger Favorit.
- Verbindlicher nativer Windows-Ablauf zusammen mit Paket 6: A → B ausdrücklich suchen, Ergebnisüberschrift prüfen, Favorit speichern, App beenden und mit derselben isolierten Testablage neu starten. Im Suchformular Datum/Zeit und Ankunftsmodus setzen, Favorit wählen, beide sichtbaren Namen und zugrunde liegenden Identitäten prüfen. Auswahl erhält diese Zeitparameter und löst weder Haltestellen- noch Routinganfrage aus. Fixture-Aufrufzähler vor/nach der Auswahl müssen unverändert bleiben.
- Danach Start/Ziel vertauschen: B → A mit passenden Identitäten, Zeitparametern und unveränderten Haltestellen-/Routingzählern. Erst ausdrücklich suchen; genau dann wird Routing ausgelöst und die Überschrift zeigt B → A. Anschließend den ursprünglichen A → B-Favoriten entfernen und nach erneutem Prozessneustart dessen Abwesenheit prüfen.
- Verifikation: ViewModel-Tests, UiTest-Build und der vollständige native Ablauf einschließlich Neustarts, Entfernung und Negativassertionen. Tatsächliche Prozessausführung protokollieren; falls nicht ausführbar, Versuch und konkrete Ursache dokumentieren und die Abnahme offen lassen.

## Paket 6 – Start und Ziel vertauschen (R4)

- Atomare Sessionoperation ergänzt ausgewählte Adressen und sichtbare Feldtexte; Trefferlisten werden passend zurückgesetzt oder neu zugeordnet.
- Swap aus einem gespeicherten Verbindungsfavoriten muss genauso funktionieren. Zeit, Datum und Ankunftsmodus bleiben unangetastet.
- Der Swap invalidiert ein altes Ergebnis nach den bestehenden Regeln, startet aber keine Routinganfrage.
- Testen: beide Felder gefüllt, nur ein Feld gefüllt, Koordinatenmodus, Favoritenauswahl, bestehendes Ergebnis und explizite Aufrufzählung mit null neuen Provideranfragen.
- Verifikation: Core-Sessiontests und der in Paket 5 festgelegte Windows-UI-Test einschließlich Fokus-/Accessibilityverhalten. Fixture-Aufrufzähler belegen auch nach Swap ausdrücklich null neue Haltestellen- und Routinganfragen; eine anschließende bewusste Suche verwendet die vertauschten Identitäten.

## Paket 7 – Detailansicht auf Nutzwert prüfen (R5)

- Journey-Timeline und Karten auf wiederholte Zusammenfassungen, technische Kennungen und nicht handlungsrelevante Metadaten prüfen.
- Nur belegte Dopplungen entfernen. Fahrtfolge, Start/Ziel, Umstiege, relevante Gehwege/-zeiten, Echtzeit-/Ausfallhinweise, Barriereinformationen und Hinweise auf fehlende Daten erhalten.
- Textfallback separat mit der Timeline vergleichen und nur dann kürzen, wenn er tatsächlich erreichbar ist; keine nicht genutzten Pfade allein aus ästhetischen Gründen umbauen.
- Testen: mehrteilige Verbindung mit Umstieg und Fußweg, Echtzeitabweichung, Ausfall, fehlende Geometrie und fehlende Betreiberinformation; Assertions belegen sowohl entfernte Wiederholungen als auch erhaltene Kerninformationen.
- Verifikation: Core-Präsentationstests, Windows-UI-Screenshots und Accessibility-Texte.

## Paket 8 – Abschlussprüfung und Dokumentation

- Gesamte Core-Suite und UiTest-Warnings-as-Errors-Build ausführen; die korrigierte native Windows-Cache-/Navigationsjourney in einer sichtbaren Desktop-Sitzung durchführen.
- Regressionslauf für `WindowsJourneyUiTests.ps1` und Screenshots der Favoriten-, Such-, Ergebnis- und Detailzustände aktualisieren.
- Ergebnisse, Umgebung und verbleibende Grenzen in `test-results-navigation-favorites.md` dokumentieren. iOS-Geräteprüfung für Cache, Favoriten, Navigation und VoiceOver bleibt separat offen.
- Review der finalen Änderungen gegen Anforderungen und Persistenz-/Cacheverträge; erst danach Abschlusscommit.

## Reihenfolge und Abhängigkeiten

`Paket 0 → Paket 1 → Paket 2 → Paket 3 → Paket 4 → Paket 5 → Paket 6 → Paket 7 → Paket 8`.

Paket 1 kann nach abgeschlossenem Paket 0 parallel zu Paket 2 umgesetzt werden, die Integration und Abnahme bleibt jedoch in obiger Reihenfolge. Pakete 2 und 3 sind voneinander getrennt, damit Navigationszustand und Cachefehler unabhängig diagnostizierbar bleiben. Paket 5 hängt vom abgeschlossenen Persistenzvertrag aus Paket 4 ab. Paket 6 nutzt dieselbe Endpunkt-Sessionlogik und folgt deshalb der Favoritenauswahl. Die Textbereinigung in Paket 7 beginnt erst, wenn Ergebnis-/Favoritenkopfzeile feststeht.

## Offene technische Entscheidungen

1. **Cache-Limit für Suchhaltestellen (festgelegt):** Ausschließlich Sessioncache mit 100 Haltestellen, je maximal 100 Abfahrten und 100 Linien, LRU bei Aufnahme einer neuen Identität; der geöffnete Monitor ist geschützt. Der persistente Favoritencache bleibt unabhängig und wird durch Sessioneviction nicht verändert.
2. **Verbindungsfavoriten-Duplikate:** Identität als gerichtetes Paar `(Source, OriginId, DestinationSource, DestinationId)` behandeln. Umgekehrte Verbindung bleibt ein eigener Favorit; exakte Duplikate werden aktualisiert statt erneut eingefügt.
3. **Migrationsverhalten:** Verbindungsfavoriten starten mit einer neuen versionierten Datei; kein automatisches Umdeuten bestehender Stopfavoriten. Departure-Cache-Altdateien werden getestet und bei optional neuen Feldern mit sicheren Defaults eingelesen.
4. **Native E2E:** Der aktuell dokumentierte versteckt gestartete Cachelauf zählt nicht. Der reparierte sichtbare Fixture-Start und die verschärften Linien-/Leerheitsassertions müssen im tatsächlichen Prozesslauf bestätigt werden.
