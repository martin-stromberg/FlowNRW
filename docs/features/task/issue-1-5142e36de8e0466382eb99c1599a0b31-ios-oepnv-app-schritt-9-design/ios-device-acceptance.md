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
| Noch nicht ausgeführt |  |  |  | iOS-Geräteabnahme liegt beim Nutzer. |
