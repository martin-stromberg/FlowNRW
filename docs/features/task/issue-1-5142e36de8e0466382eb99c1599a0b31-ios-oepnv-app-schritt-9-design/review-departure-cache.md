# Review – Zwischenspeicher für Favoritenabfahrten

**Geprüft:** 03.10.2026
**Grundlage:** `requirement-departure-cache.md`, aktueller Departure-Cache-Diff

## Ursprüngliche Befunde und Recheck

### B1 – Bereinigt: Abgelaufene Cache-Einträge werden beim Lesen gelöscht

`FavoriteMonitorViewModel.Restore` filtert vergangene Abfahrten korrekt aus und zeigt sie nicht. Liefert die Filterung jedoch keine zukünftige Abfahrt, gibt die Methode lediglich `false` zurück. `FavoriteHomeViewModel.LoadAsync` bereinigt nur verwaiste Schlüssel, speichert den durch `Restore` verworfenen Eintrag aber nicht zurück und löscht ihn auch nicht.

Damit bleibt ein Cache mit ausschließlich vergangenen Abfahrten dauerhaft auf dem Gerät, wenn die anschließende Anbieterabfrage scheitert oder die App beendet wird. Das verletzt R3: „Daten ohne zukünftige Abfahrt sind beim nächsten Zugriff zu bereinigen.“

**Recheck:** `FavoriteHomeViewModel.LoadAsync` löscht jetzt für jeden passenden, von `Restore` abgewiesenen Entry dessen Schlüssel aus dem Store. `LoadAsync_RemovesCacheBoardWithOnlyExpiredDepartures` prüft sowohl die leere Karte als auch den leeren Speicher. **Kein offener Befund.**

### B2 – Bereinigt: Startrefresh beginnt vor Standort-/Nearby-Abfrage

`HomePage.OnAppearing` ruft nach der Cache-Hydrierung zuerst `await model.RefreshNearbyAsync()` auf und startet `RefreshAtStartupAsync` erst danach. Die Nearby-Abfrage enthält Standortzugriff und Providerzugriff und kann langsam sein, fehlschlagen oder auf eine Berechtigung warten.

Der aktuelle Cache bleibt zwar sichtbar, die laut R2 gleichzeitig verlangte Anbieteraktualisierung beginnt aber erst nach dieser unabhängigen Nebenfunktion. Im „nearby-slow“-Szenario beträgt die künstliche Verzögerung allein sechs Sekunden.

**Recheck:** `HomePage.OnAppearing` rendert nach `LoadAsync`, startet `RefreshAtStartupAsync` ohne `await` und beginnt erst danach `RefreshNearbyAsync`. Das Fixture-Szenario `cache-start-slow-nearby` verzögert Nearby und die Liveantwort jeweils sechs Sekunden; der Windows-Test verlangt, dass die Startanfrage innerhalb von drei Sekunden gezählt wird. **Kein offener Befund.**

## Geprüfte, nicht blockierende Punkte

- Der Schlüssel `(Source, Id)` trennt gleichnamige Haltestellen verschiedener Anbieter.
- Der Store ist getrennt von `favorites.json`, begrenzt, atomar und entfernt Koordinaten aus den gespeicherten Ereignissen.
- Erfolgreiche nicht-stale Antworten werden zeitbereinigt persistiert; Fehler behalten ein zuvor wiederhergestelltes Ergebnis sichtbar.
- Die Home-Sperre serialisiert Cache-Upsert und Favoritenentfernung. Nach `CancelPending` kann ein entfernter Karten-Callback wegen `Contains(card)` keinen neuen Cache-Eintrag schreiben.
- Verwaiste Schlüssel werden nach dem Favoritenladen berücksichtigt, und das Entfernen eines Favoriten löst eine Cache-Löschung aus. Ein Cache-I/O-Fehler verfälscht nicht den erfolgreichen Favoritenstatus.
- Die neue Windows-Fixture isoliert Favoriten-, Cache- und Einstellungsdateien und unterscheidet Cache-Seed von verzögerter Live-Antwort.

## Nachweisstatus

Die Core-Suite ist nach dem Recheck mit 267 bestandenen Tests, 0 Fehlern und aktivierten Warnings-as-Errors erfolgreich. Der UiTest-Build hat 0 Warnungen und 0 Fehler; die PowerShell-Syntax der geänderten Fixture-Skripte ist geprüft. Der native Cache-Fixture-Prozessnachweis ist weiterhin offen: Der erste Lauf startete das Fenster versteckt und erhielt deshalb keinen Foreground-Lifecycle; der Script-Start wurde anschließend korrigiert, jedoch noch nicht erfolgreich erneut ausgeführt.

## Reviewurteil

**Fachlich freigabefähig.** Die zwei Codebefunde sind bereinigt. Vor Abschluss bleibt der erfolgreiche native Windows-Fixturelauf als ausstehender Nachweis erforderlich.
