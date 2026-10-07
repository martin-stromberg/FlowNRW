# iOS-Geräteabnahme für Schritt 9

Diese Checkliste ist auf einem echten iPhone mit dem zu prüfenden Build auszuführen. Windows-Aufnahmen, ein iOS-Compile-Target und ein Simulator ersetzen diese Geräteabnahme nicht. Vor Beginn Build/Commit, iPhone-Modell, iOS-Version, Darstellungsmodus, Textgröße und Orientierung notieren.

## Abfahrten und Cache

1. App mit mindestens einem gespeicherten Haltestellenfavoriten starten. Erwartung: Die Favoritenkarte ist zunächst zugeklappt. Bereits gespeicherte Linien erscheinen sofort als farbige Linienbadges; bekannte nächste Uhrzeiten stehen daneben. Linien ohne Uhrzeit zeigen keine erfundene Zeit.
2. App beenden und erneut starten. Erwartung: Die gespeicherten Linienbadges sind bereits vor Abschluss der Aktualisierung sichtbar. Die Aktualisierung ergänzt Zeiten und entfernt Linien, die in einer vollständigen aktuellen Antwort nicht mehr vorkommen.
3. Favoritenkarte öffnen. Erwartung: Abfahrten erscheinen; ihre nächste Uhrzeit steht im Kopf. Im aufgeklappten Zustand werden Linienbadges und Uhrzeiten nicht zusätzlich in einer zweiten Übersicht wiederholt. Ladezustand erscheint als Symbol, ohne die Karte mit einer Quellen- oder Erfolgsmeldung zu füllen.
4. Haltestelle über die Haltestellensuche öffnen, Abfahrten laden lassen und zurück zur Suche gehen. Dieselbe Haltestelle erneut öffnen. Erwartung: vorhandene Cache-Abfahrten erscheinen sofort während der Aktualisierung. Die vorherigen Suchtreffer bleiben bei der Rückkehr erhalten.
5. Von der Startseite eine nahe Haltestelle öffnen und zurückkehren. Erwartung: Der Monitor öffnet sich; vorhandener Cache wird während des Refreshs angezeigt. Prüfen, dass eine verfügbare Geräteposition für die Entfernung genutzt wird und dass verweigerte/nicht verfügbare Position verständlich behandelt wird.

## Verbindungssuche

1. Start und Ziel auswählen, Suche ausführen und die Ergebnisliste öffnen. Erwartung: Die Kopfzeile nennt die gewählte Verbindung und bietet ein Favoritensymbol.
2. Verbindung als Favorit speichern, zur Suche zurückkehren und gespeicherte Verbindung auswählen. Erwartung: Start und Ziel werden ohne erneute Haltestellensuche übernommen.
3. Start und Ziel über die Tauschaktion wechseln und erneut suchen. Erwartung: Die Felder und Ergebnisse verwenden die vertauschte Reihenfolge.
4. Verbindungsdetails öffnen. Prüfen, dass die Timeline verständlich bleibt und redundante Status-/Metadaten die Fahrtabschnitte nicht verdrängen. Zurück zur Ergebnisliste: Verbindung und Suchkontext bleiben erhalten.

## Darstellung und Bedienbarkeit

Die Abfahrts-, Haltestellen- und Verbindungsschritte mindestens auf einem kleinen und einem großen iPhone jeweils in hellem und dunklem Erscheinungsbild prüfen. Auf beiden Geräten Hoch- und Querformat testen; zusätzlich eine große Dynamic-Type-Stufe einstellen.

- Safe Areas: Inhalt und Bedienelemente liegen frei von Notch/Dynamic Island, Statusleiste, Home-Indikator und Tabbar.
- Umbruch: Lange Haltestellen-, Ziel- und Liniennamen bleiben lesbar; Aktionen werden nicht abgeschnitten oder überlagert.
- Auf-/Zuklappen und Favorisieren: Symbole sind verständlich, tappbar und haben VoiceOver-Namen. VoiceOver-Reihenfolge folgt der visuellen Bedienfolge.
- Lade-/Fehlerzustände: Refreshsymbol ist wahrnehmbar und zugänglich angekündigt; kein dauerhaft unnötiger Statustext. Leere und Fehlerzustände bleiben unterscheidbar.
- Position: Standortberechtigung zulassen und verweigern; Nähe/Entfernung muss zum Berechtigungszustand passen und die App darf nicht hängen.

## Ergebnisprotokoll

Pro Gerätekonfiguration die Checkpunkte mit `Bestanden`, `Fehler` oder `Nicht ausgeführt` markieren. Bei Fehlern den Schritt, erwartetes und beobachtetes Verhalten sowie einen Screenshot notieren. Keine nicht ausgeführten Punkte als bestanden werten.

| Gerät / iOS | Build / Commit | Modus / Textgröße | Orientierung | Ergebnis / offene Punkte |
|---|---|---|---|---|
| iPhone (nutzerseitiger Test, 05.10.2026) | Commit `97b940f` | Standard | Hochformat | Teilweise bestanden; drei Produktbefunde in Arbeit — siehe Befundliste |
| iPhone (nutzerseitige Wiederholung, 05.10.2026) | Bugfix-Build (Commits `c23114a`, `3656f54`) | Standard | Hochformat | Abgenommen mit Vorbehalt: sporadische Abruffehler akzeptiert, Analyse über Diagnoseprotokoll („Protokoll senden") |

### Befundliste der Geräteprüfung, 05.10.2026

**Bestanden:** Favoritenkarten starten zugeklappt mit sofort sichtbaren gespeicherten Linienbadges; verweigerte GPS-Freigabe erzeugt einen verständlichen Hinweis; Favorit als Start/Ziel wählbar; unbekannte Haltestelle suchbar; „Jetzt“-Abruf liefert Verbindungen; Linienanzeige korrekt; Favoritenstatus in der Ergebnisliste umschaltbar; gespeicherte Verbindung übernimmt Start/Ziel; Endpunkttausch funktioniert; Detailansicht öffnet.

**Befund 1 (kritisch):** Der Abfahrtsabruf schlägt auf dem Gerät durchgehend fehl („Abfahrten konnten nicht geladen werden. Bitte erneut versuchen.“), während die Verbindungssuche funktioniert. Folgefehler: Cache-Wiederaufnahme, aufgeklappte Favoritenkarte und erneut geöffneter Haltestellenmonitor waren nicht testbar. Ursache: `IsComplete` lehnte jede degradierte Antwort (Ersatzquelle, Warnung, veraltete Daten) als Fehler ab — reale Abfahrtsantworten tragen regelmäßig solche Merkmale. Behoben: nutzbare Antworten ohne `ErrorCode` werden jetzt mit kompaktem Hinweis „Daten möglicherweise unvollständig oder veraltet.“ angezeigt; ein vorhandenes vollständiges Board wird wie bisher bevorzugt behalten; Persistenz/Sessioncache bleiben auf vollständige Antworten beschränkt (`StopMonitorViewModel.cs`, `FavoriteMonitorViewModel.cs`).

**Befund 2:** Verbindungsdetails zeigten `UTC+02:00`-Offsets. Behoben: Zeiten erscheinen als `HH:mm`; weicht der Tag vom ersten Fahrtabschnitt ab, steht `Vortag`/`Folgetag` davor (`JourneyPresentation.EventTime`).

**Befund 3:** Der Umstieg erschien als Sammelkarte am Ende der Timeline statt zwischen den Fahrten. Behoben: `JourneyTimelineView` rendert Umstiege jetzt positionsgetreu zwischen den Transitabschnitten.

**Wiederholungsbedarf:** Die iOS-Geräteabnahme ist nach dem Bugfix-Build erneut durchzuführen; insbesondere die Abfahrts-/Cache-Checkpunkte 1–5 stehen noch aus. Falls der Abfahrtsabruf weiterhin scheitert, wird ein Protokoll des Geräte-Netzwerkverkehrs benötigt, um eine echte Providerstörung von der Darstellungsschwelle zu unterscheiden.

### Wiederholungsprüfung, 05.10.2026 (zweiter Gerätedurchgang)

**Neu bestanden:** Abfahrten laden jetzt mit angezeigten Zeiten (Befund 1 wirkt); aufgeklappte Favoritenkarte zeigt Abfahrten; Stationsmonitor lädt und listet; beide Detailbefunde (UTC-Offset, Umstieg-Position) als erledigt markiert.

**Restbefund 1:** Der Abruf ist unzuverlässig — zwischendurch erscheint „konnten nicht geladen werden" trotz angezeigter Zeiten. Als vorläufig akzeptabel bewertet; Ursachenanalyse läuft über das neue Diagnoseprotokoll.

**Restbefund 2 (behoben):** Nach Neustart zeigten Favoritenkarten nur Linienbadges ohne Zeiten — der Restore-Pfad verwarf jede älter als 30 s liegende persistierte Antwort (`freshness.IsStale` in `FavoriteMonitorViewModel.Restore`). Behoben: zukünftige Abfahrten werden als `IsStale`-markiertes Board wiederhergestellt und sofort aktualisiert.

**Restbefund 3 (behoben):** Erneut geöffnete Station zeigte keine Session-Abfahrten — `StoreSession` speicherte nur vollständige Antworten, degradierte reale Antworten wurden verworfen. Behoben: jede fehlerfreie Antwort geht in den Session-Cache.

**Restbefund 4 (behoben):** Verbindungsdetails zeigten „Keine Verbindung gewählt" — der Fallback-Text überschrieb den eigentlichen Grund. Behoben: `JourneyDetailViewModel.Details` zeigt bei gelöschter Auswahl den konkreten Sitzungsstatus (z. B. „nicht mehr eindeutig bestätigt").

**Neue Funktion (nutzerseitig angefordert):** Diagnoseprotokoll in den Einstellungen — Schalter „Protokollierung aktivieren" plus „Protokoll senden"-Button, der das Protokoll per Mail an `mstromberg84+flow@gmail.com` öffnet (Log-Datei als Anhang, Inhalt zusätzlich im Text). Protokolliert werden HTTP-Aufrufe mit Status/Dauer, Monitorergebnisse (Fehler/Fallback/Veraltung/Warnungen) und Fenster-Lifecycle. Umgesetzt in `FlowNRW.Core/Diagnostics/AppLog.cs` und `RefreshSettingsPage.cs`.

### Protokollanalyse, 07.10.2026

Das erste Geräteprotokoll zeigte die Ursache der sporadischen Fehler: die `efa`-Abrufe (`XML_DM_REQUEST`) antworteten in ~700 ms, während `db-rest /locations` je Karte ~20 s in den Timeout lief. Die ausgelöste Meldung „konnten nicht geladen" hing an der Retention-Regel — eine degradierte, aber frische EFA-Antwort traf auf ein bestehendes Board. Beobachtung: die gespeicherten Favoriten tragen keine `db-rest`-Quelle, die Namensauflösung über `/locations` ist für sie auf diesem Netz wiederholt gescheitert; die Fremd-ID-Regel (keine fremden IDs in db.rest) bleibt bewusst erhalten.

Behoben: eine degradierte neue Antwort ersetzt ein vorhandenes Board, wenn das Board selbst nicht mehr vollständig ist (`IsComplete(Result)`-Schwelle in beiden Monitor-ViewModels). Ein weiterhin komplettes Board bleibt gegen Teil-/Ersatzantworten geschützt. Damit zeigt die Karte frische EFA-Daten mit kompaktem Hinweis statt einer Fehlermeldung bei vorliegenden Abfahrten.
