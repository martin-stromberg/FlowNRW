# Abnahmeprüfung – Entwicklungsschritt 6

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Abweichungen

Keine.

## Hinweise

Unabhängige fachliche Abnahme am 19.09.2026 für Commit `1edca4b` auf dem Schritt-6-Branch. Originalanforderung: Schritt 6 des Projektplans. Tatsächliche Diffbasis: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app...HEAD`. Beurteilt wurden die produktiven Quellen und dauerhaften Testnachweise, nicht lediglich die Ergebnisse früherer Reviews.

| Akzeptanzkriterium | Tatsächliche Erfüllung |
|---|---|
| 1: Favoriten hinzufügen/entfernen, Duplikate, Neustarts, nur technische Stationsdaten | Monitoraktion und Home-Entfernen verwenden vollständige technische Stops. JsonFavoriteStore begrenzt Zahl/Größe, dedupliziert Source+Id und ersetzt atomar; Fehler verändern die sichtbare dauerhafte Mitgliedschaft nicht. Keine aktuelle Nutzerposition oder Abfahrten werden gespeichert. Core-Dateineustart und native vier Prozessstarts bestätigen Bestand, Entfernen und Leerzustand. Native Speicherfehler/Wiederholung für Hinzufügen und Entfernen sind erfolgreich. |
| 2: Nach Entfernung sortierte Startseitenmonitore, stabile Ordnung ohne Standort, Leerzustand | Home ist tatsächlicher Shell-Einstieg. Explizite Standortaktion berechnet Luftlinie, bekannte Entfernungen aufsteigend, unbekannte am Ende; gleiche Werte behalten die Speicherreihenfolge. Standortfehler setzen Entfernung auf unbekannt und stellen gespeicherte Ordnung her. Leere Startseite führt zur Haltestellensuche. Core und native Fixtures bestätigen Sortierung, fehlende Position/Berechtigung und manuelle Nutzung. |
| 3: Echtzeit-/Fehler-/Cachezustände, manuelle unabhängige Aktualisierung, keine Doppelabrufe, Navigation | Jede Favoritenkarte besitzt einen durch DI neu erzeugten transienten DepartureService und eigenen Lade-/Fehler-/Revisionszustand. Quelle und Datenalter beziehen sich auf angezeigte Daten; fehlgeschlagene Aktualisierung behält bekannte Abfahrten. Native Abrufzähler bestätigen Doppelklickschutz und unabhängige zweite Karte während langsamer erster Karte. Entfernen/Seitenabgang invalidieren Antworten. Monitor-/Karten-/Routingwege und Rücknavigation sind nativ geprüft; alte Favoriteninstanzen/Kartensessions werden im Core verworfen. |
| 4: Automatisierte und native Windows-Nachweise | Gesamtsuite 173 erfolgreiche Tests, 92,57 % Zeilenabdeckung. Native Favoritenfolge umfasst Hinzufügen→Home, Persistenz über echte Prozessneustarts, Duplikat, Entfernen, Leerzustand, Sortierung, Fehler und Navigation. Standort-, Routing-, Monitor- und Kartenregression über den neuen Einstieg vollständig erfolgreich. |

Geprüfte Produktbereiche: alle Klassen unter `FlowNRW.Core/Favorites`, aktuelle Änderungen an `MapViewModel`, `StopMonitorViewModel`, `HomePage`, `DeparturePage`, `AppShell` und `MauiProgram`; zusätzlich `FavoriteTests` und native Harnessänderungen. Die `Contains`-Prüfung verwendet aktuelle Kartenreferenzen; die bisherige Such-/Kartenmitgliedschaft bleibt erhalten. Der Test `EqualDistancesAndStaleMapMembership` prüft insbesondere wertgleich neu hinzugefügte Stops nach Entfernung und den gültigen neuen Auswahlweg.

Dauerhafte Nachweise: `docs/help/favoriten/verification/checks-2026-09-19.md`, `core-cobertura.xml`, `favorites.txt`, `locations.txt`, `routing.txt`, `monitors.txt`, `maps.txt` und `native-favorites-narrow.png`. Der Prüfagent hat Protokolle und den schmalen Screenshot tatsächlich gelesen/geöffnet. Hauptaktionen und Hinweise sind lesbar; weiterer Karteninhalt liegt im ScrollView. Die sichtbaren technischen Szenario-/Zählerfelder sind ausschließlich UiTest-Instrumentierung. Release und UiTest wurden mit Warnungen als Fehler erfolgreich gebaut; Format/XML und Diffprüfung sind dokumentiert. Die Tests wurden für diese Abnahme nicht nochmals gestartet, keine Windows-Fokussteuerung verwendet.

Windows-Release und GitHub-Actions bleiben erhalten. Keine iOS-CI, kein automatisiertes Deployment oder IIS ergänzt. Die iOS-Geräteanleitung unter `docs/help/favoriten/installation.md` umfasst Persistenz, Sortierung, Aktualisierung, Navigation und Bedienung; native iOS-Ausführung bleibt vereinbarungsgemäß beim Nutzer. Globale Schriftvergrößerung und VoiceOver werden nicht als ausgeführt behauptet. Die Standortintegration wurde hier mit deterministischen Fixtures regressionsgeprüft; der separat autorisierte echte OS-Nachweis aus Schritt 5 bleibt unverändert erhalten und wird nicht als erneuter Live-Test dieses Schritts ausgegeben.

Abnahme bestätigt ausschließlich Schritt 6. Automatische Intervalle und Hintergrund-/Resume-Verhalten bleiben den geplanten Schritten 7 und 8 vorbehalten. Keine Produktänderungen oder Commits durch den Prüfagenten.
