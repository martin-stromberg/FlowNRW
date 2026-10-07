# Kurzreview: Bereinigung dauerhafter Informationstexte

**Geprüft:** 4. Oktober 2026
**Umfang:** `HomePage.cs`, `RefreshSettingsPage.cs`, `MapPage.cs` und Windows-UI-Tests.
**Methode:** unabhängige Quell- und Testprüfung; kein Produktcode verändert.

## Ergebnis

Die Startseitenbereinigung erreicht das Hauptziel: Der Favoritenzähler und sein Panel sind entfernt; erfolgreiche Nearby-Suchen und erfolgreiche Favoritenaktualisierungen werden nicht mehr sichtbar wiederholt. Fehler, Leerzustände und der Cachehinweis sind in der aktuellen Statuslogik grundsätzlich abgedeckt.

Zwei Punkte sind vor der Abnahme zu korrigieren beziehungsweise ausdrücklich nachzutesten.

| Schweregrad | Befund | Nachweis | Auswirkung / Empfehlung |
|---|---|---|---|
| Mittel | Windows-UI-Tests verlangen weiterhin vielfach erfolgreiche Favoriten-Statusmeldungen. | `WindowsJourneyUiTests.ps1:205,216,236,244,246,251,252,287`; `WindowsRefreshUiTests.ps1:211,215,220,228,235`; `WindowsLifecycleUiTests.ps1:190,195,197,201,208`; `WindowsDepartureCacheUiTests.ps1:86,98`. | Die Produktansicht setzt `FavoriteStatus{n}.IsVisible` nach erfolgreicher Aktualisierung auf `false`. Die Tests dürfen Erfolg daher nicht mehr als sichtbaren Status verlangen. Abrufzähler, geänderte `FavoriteDeparture{n}_0` und die sichtbaren Lade-/Fehler-/Cachezustände sind die passenden Prüfpunkte. Ohne Anpassung können die Tests fehlschlagen oder im günstigeren UIA-Fall eine nicht sichtbare Zeile als Erfolg werten. |
| Niedrig | Die Einstellungsseite zeigt einen erfolgreichen Start-/Ladestatus dauerhaft. | `RefreshSettingsPage.cs:35,50`; `RefreshSettingsViewModel.cs:53` setzt „Standard: …“ oder „Gespeicherte Einstellung geladen.“ | Das widerspricht dem Bereinigungsziel außerhalb der Startseite. `RefreshSettingsStatus` sollte bei erfolgreichem Laden verborgen sein; Validierungs-, Speicher- und Ladefehler bleiben sichtbar. Der effektive Wert in `RefreshIntervalStatus` bleibt sinnvoll und soll erhalten bleiben. |
| Niedrig | Ein Ausnahmefehler beim Erscheinen der Startseite wird nur als Seitentitel hinterlegt. | `HomePage.cs:109-117` | Der Text wäre nicht als kontextgebundene Fehlerrückmeldung in der Seite sichtbar. Falls dieser Catch erreichbar ist, sollte er `HomeStatus` mit einem Fehlerzustand versorgen oder anders sichtbar rückmelden. Die aktuell aufgerufenen View-Model-Methoden fangen ihre erwarteten Provider-/Speicherfehler selbst; deshalb derzeit kein Blocker. |

## Bestätigte Punkte

- `FavoriteCount` ist aus `HomePage` entfernt. Der angepasste Haupttest zählt Favoritenkarten und prüft die Abwesenheit der ID (`WindowsJourneyUiTests.ps1:147-190`).
- `ShowsHomeStatus()` zeigt bei leerer Liste, Speicher-/Ladefehlern sowie Identitäts- und Obergrenzenfehlern weiterhin eine Rückmeldung.
- `ShowsNearbyStatus()` hält den Status bei leerer Liste, Standort-/Providerfehlern und laufender Suche sichtbar; bei einer erfolgreichen nichtleeren Liste verschwindet die doppelte Anzahlmeldung.
- `ShowsFavoriteStatus()` hält `IsBusy`, „Keine nächsten Abfahrten“, „Letzter Stand wird aktualisiert“, „fehlgeschlagen“ und „nicht geladen“ sichtbar. Die Erfolgstexte und Abbrüche werden ausgeblendet.
- Die Kartenquelle (`FavoriteMetadata{n}`), Abfahrtskarten und die Symbolbutton-Semantik bleiben erhalten.
- Die Einstellungen behalten Auswahl, Speichern, effektives Intervall und Fehlerstatus. Die langen Erläuterungsblöcke sind entfernt.
- Die Karte zeigt die Anzahl gelieferter Punkte nicht mehr; die Tests prüfen nur noch die fachlich relevante Linienbezeichnung (`WindowsJourneyUiTests.ps1:567`). Kartenstatus, Attribution und die optionale Informationsansicht bleiben erhalten.

## Abnahmeempfehlung

Nach Anpassung der genannten Erfolgspfad-Tests sollte ein nativer Windows-Lauf mindestens diese Fälle belegen: erfolgreiche Favoritenaktualisierung ohne sichtbare Erfolgsmeldung, Cacheanzeige während Aktualisierung, leere Abfahrtsantwort, Fehler mit letzten Daten, leere und fehlerhafte Nearby-Suche sowie leere Favoritenstartseite.
