# Kleinplan: Bereinigung dauerhafter Informationstexte

**Grundlage:** `requirement-text-cleanup.md` und `inventory-text-cleanup.md`
**Umfang:** Nur Präsentation und Sichtbarkeit; keine Änderung an Favoriten, Cache, Providerabrufen oder Navigation.

## 1. Startseiten-Übersicht entfernen

In `FlowNRW/HomePage.cs` das umrandete Übersichts-Panel samt `FavoriteCount`, `HomeStatus`, `HomeLocationStatus` und `RefreshIntervalStatus` aus dem normalen Seitenaufbau entfernen. Ebenso die einleitende Wiederholung „Abfahrten“ und den Beschreibungssatz prüfen und entfernen, weil Tabtitel und Favoritenkarten den Kontext bereits vermitteln.

Die bestehenden Schnellaktionen, Favoritenkarten, leere Seite und der Weg zum Hinzufügen einer Haltestelle bleiben erhalten. Für Fehler beim Laden oder Speichern ist eine knappe, kontextgebundene Rückmeldung vorzusehen, die kein dauerhaftes Panel benötigt.

**Zu aktualisierende Tests:**

- `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`: Annahmen zu `FavoriteCount` durch Sichtbarkeitsprüfung der Favoritenkarten ersetzen; leeren Zustand über handlungsorientierten Inhalt statt `HomeStatus` absichern.
- Fehler beim Entfernen eines Favoriten weiter über eine sichtbare Fehlerrückmeldung prüfen, ohne die frühere permanente Statuszeile vorauszusetzen.
- `WindowsDesignUiTests.ps1`: Standortaktion weiterhin anhand ihrer zeitlich begrenzten Rückmeldung testen.

## 2. Status nur im relevanten Moment einblenden

In der Favoritenkarte in `HomePage.cs` und dem zugehörigen `FavoriteMonitorViewModel` erfolgreiche Abschlussmeldungen wie „manuell/automatisch aktualisiert“ nicht dauerhaft als eigene Zeile darstellen. Sichtbar bleiben:

- laufende Aktualisierung,
- ein Leerzustand,
- Fehler einschließlich nutzbarer letzter lokaler Daten,
- ein knapper Datenstand, wenn er die Frische der Abfahrten erläutert.

Die interne Statusinformation und die Tests für Nebenläufigkeit bleiben bestehen; die UI-Sichtbarkeit wird vom Erfolgspfad entkoppelt. Damit bleiben Tests nicht auf eine Erfolgsmeldung als sichtbare Produktfunktion angewiesen.

**Zu aktualisierende Tests:**

- `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1` und `WindowsDepartureCacheUiTests.ps1`: laufende Zustände, Fehler und Cacheersetzung weiter testen; Erfolg über die aktualisierte Abfahrtskarte bzw. die Fixture-Abrufzählung prüfen, nicht über `FavoriteStatus{n}`.
- Die AutomationIds `FavoriteStatus{n}` können für relevante Zustände erhalten bleiben; bei erfolgreichem Abschluss wird eine leere bzw. ausgeblendete Darstellung erwartet.

## 3. Erfolgreiche Nearby-Anzahl ausblenden

`NearbyHeading` bleibt als Abschnittsüberschrift bestehen. `NearbyStatus` ist nach einer erfolgreichen Umgebungssuche mit mindestens einem Treffer nicht sichtbar, weil die Liste die Information bereits zeigt. Die Meldungen „keine …“, Standortfehler und Ladezustände bleiben sichtbar.

**Zu aktualisierende Tests:**

- In `WindowsJourneyUiTests.ps1` die erfolgreiche Nearby-Suche über `NearbyStop0` bzw. die Trefferliste prüfen statt über „nahe Haltestellen gefunden“.
- Leer-, Fehler- und Ladeabläufe mit `NearbyStatus` unverändert prüfen.
- Den Home-Nearby-Drilldown über `NearbyStop0` beibehalten, damit die bereits korrigierte Interaktion regressionsgeschützt bleibt.

## 4. Sichere Kürzungen außerhalb der Startseite

Nur die folgenden Kürzungen sind unabhängig vom jetzigen Produktfluss sicher:

1. In `RefreshSettingsPage.cs` die langen Erläuterungsblöcke zu Standardintervall, Hintergrundverhalten und iOS aus der dauerhaften Ansicht entfernen. `RefreshSettingsStatus`, Auswahl und Speichern bleiben bestehen.
2. In `MapPage.cs` technische Detailzeilen wie gelieferte Punktanzahl nicht standardmäßig anzeigen. Kartenfehler, fehlende Geometrie und rechtlich erforderliche Attribution bleiben erhalten.

Die Such-, Routing-, Detail- und Monitor-Metadaten werden in diesem Paket nicht weiter verändert: Sie sind teils durch vorhandene UI-Tests als Diagnose-/Nachweisweg verwendet und erfordern eine eigene Abnahme der fachlichen Datenstandsanzeige.

## 5. Abnahme

1. Startseite mit Favoriten: Kein Favoritenzähler und kein Übersichts-Panel; erste Favoritenkarte ohne vorgeschaltete Verwaltungsinformationen erreichbar.
2. Leere Startseite: kurze verständliche Aufforderung zum Hinzufügen bleibt vorhanden.
3. Favorit: Während Abruf, bei Fehler und bei leeren Ergebnissen sichtbarer Status; nach erfolgreichem Abruf keine dauerhafte Erfolgsmeldung.
4. Nearby: Erfolgreiche Liste ohne doppelte Anzahlmeldung; leere, fehlerhafte und laufende Suche verständlich.
5. Einstellungen/Karte: Die zwei sicheren Kürzungen entfernen keine Speichern-, Fehler-, Kartenverfügbarkeits- oder rechtlich nötigen Hinweise.
6. `dotnet test .\FlowNRW.Tests\FlowNRW.Tests.csproj -c UiTest --no-restore`, UiTest-Build mit `TreatWarningsAsErrors=true` sowie die angepassten Windows-PowerShell-UI-Skripte erfolgreich ausführen.
