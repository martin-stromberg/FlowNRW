# Umsetzungsplan – Zwischenspeicher für Favoritenabfahrten

**Grundlage:** `requirement-departure-cache.md` und `inventory-departure-cache.md`
**Ziel:** Beim Start noch bevorstehende, zuletzt erfolgreiche Abfahrten gespeicherter Favoriten sofort lokal anzeigen und trotzdem für jede Karte unverzüglich eine aktuelle Anbieterabfrage ausführen.

## Schritt 1 – Eigenständiges, begrenztes Cachemodell und Store

**Dateien**

- Neu: `FlowNRW.Core/Favorites/IDepartureCacheStore.cs`
- Neu: `FlowNRW.Core/Favorites/JsonDepartureCacheStore.cs`
- Neu: Cache-Eintragsmodell und JSON-Quellkontext unter `FlowNRW.Core/Favorites/`
- `FlowNRW/MauiProgram.cs`

**Arbeit**

1. Einen separaten technischen Speicher neben `favorites.json` anlegen, dessen Schlüssel ausschließlich `(Stop.Source, Stop.Id)` ist.
2. Pro Schlüssel die normalisierte `ProviderResult<StopEvent>`-Darstellung, Providerherkunft und den erfolgreichen Abrufzeitpunkt speichern. Es werden keine Standortdaten, Suchdaten oder anderen Haltestellen persistiert.
3. Speicherung und Laden wie beim Favoritenstore größenbegrenzt, JSON-validiert und atomar ausführen. Defekte oder zu große Cache-Dateien dürfen Favoriten nicht unbrauchbar machen.
4. Eine Bereinigungsoperation vorsehen: einzelne Schlüssel löschen und Einträge entfernen, die nicht mehr zu einer geladenen Favoritenidentität gehören.
5. Den Store in Release unter `FileSystem.AppDataDirectory` und im UiTest mit einem eigenen, isolierten Testpfad registrieren.

**Abnahme**

- Gleichnamige Haltestellen unterschiedlicher Anbieter lesen niemals denselben Cache-Eintrag.
- Ein fehlerhafter Cache verhindert weder das Laden von Favoriten noch eine aktuelle Anbieterabfrage.

## Schritt 2 – Zeitgültige Hydrierung und Erfolgspersistenz im Monitor

**Dateien**

- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- Bei gemeinsamer Extraktion der Zeitlogik: eine kleine fachliche Hilfsklasse unter `FlowNRW.Core/Favorites/`

**Arbeit**

1. Die vorhandene effektive Abfahrtszeit als gemeinsame Regel verwenden: Echtzeit (`ActualTime`), sonst Sollzeit mit Verspätung, sonst keine verwendbare Abfahrtszeit.
2. Einen Wiederherstellungspfad ergänzen, der nur erfolgreiche Cache-Ergebnisse übernimmt, alle abgelaufenen Abfahrten verwirft, neu sortiert und Bindings aktualisiert.
3. Wiederhergestellte Daten mit einem knappen Hinweis als lokaler letzter Stand kennzeichnen; sie bleiben vollständig bedienbar.
4. Nach jeder erfolgreichen Providerantwort dieselbe Bereinigung anwenden, die neue Anzeige setzen und nur diesen bereinigten Erfolg asynchron persistieren. Eine leere erfolgreiche Antwort ersetzt bzw. entfernt einen älteren Cache-Eintrag.
5. Versions-/Busy-Schutz so erhalten, dass eine verspätete Antwort weder eine neuere Anzeige noch deren Cache überschreiben kann. Cache-Schreibfehler dürfen eine erfolgreiche Live-Anzeige nicht in einen Fehlerzustand versetzen.

**Abnahme**

- Keine vergangene Abfahrt wird nach Neustart angezeigt.
- Eine fehlgeschlagene Aktualisierung behält gültige lokale Abfahrten sichtbar und kennzeichnet nur die fehlende Aktualität.

## Schritt 3 – Favoritenintegration, Startrefresh und Bereinigung

**Dateien**

- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW.Core/Refresh/RefreshLifecycle.cs` nur falls für den bereits vorhandenen Hintergrundpfad erforderlich

**Arbeit**

1. Nach dem Laden der Favoriten die zugehörigen Cache-Einträge einmalig lesen, verwaiste Schlüssel bereinigen und die Karten vor dem ersten Rendern hydrieren.
2. Beim Entfernen eines Favoriten dessen Cache löschen. Wird derselbe Favorit später erneut gespeichert, beginnt er ohne alten Restcache.
3. Eine explizite Startaktualisierung aller sichtbaren Karten einführen. Sie nutzt den bestehenden automatischen Refreshpfad und Busy-Schutz, aber ist unabhängig von `Result`.
4. `HomePage.OnAppearing` so ordnen, dass erst hydrierte Karten sichtbar werden und danach die Startaktualisierung nicht blockierend angestoßen wird. `RefreshMissingAsync` allein darf dafür nicht verwendet werden, weil es Karten mit Cache-Ergebnis auslässt.
5. Intervall- und iOS-Hintergrundrefresh behalten ihre bisherigen Grenzen und dürfen keinen zweiten parallelen Abruf derselben Karte erzeugen.

**Abnahme**

- Beim Neustart sind gültige lokale Abfahrten ohne Warten auf das Netzwerk sichtbar.
- Pro Karte folgt auch bei vorhandenem Cache genau eine reguläre Startabfrage; deren Erfolg ersetzt die lokale Anzeige.

## Schritt 4 – Unit- und Persistenztests

**Dateien**

- Neu oder erweitert: `FlowNRW.Tests/DepartureCacheStoreTests.cs`
- `FlowNRW.Tests/FavoriteTests.cs`
- `FlowNRW.Tests/RefreshMonitorTests_Concurrency.cs`
- `FlowNRW.Tests/RefreshLifecycleTests_Background.cs` sofern der Hintergrundpfad angepasst wird

**Arbeit**

1. JSON-Roundtrip, Schlüsseltrennung, Größen-/Fehlergrenzen und Löschung einzelner sowie verwaister Einträge testen.
2. Hydrierung mit Sollzeit, Echtzeit und Verspätung testen; abgelaufene und zeitlose Elemente dürfen nicht erscheinen.
3. Cache sichtbar machen, während eine kontrollierte Providerantwort aussteht; danach aktuelle Ersetzung und neuen Persistenzstand prüfen.
4. Fehler, Abbruch, doppelte Refreshanforderung und verspätete Antworten gegen Datenverlust oder Rückschritt prüfen.
5. Entfernung und erneutes Anlegen eines Favoriten gegen Wiedereinblenden alter Cache-Daten prüfen.

**Abnahme**

- Die erweiterten Core-Tests decken Filterung, Zuordnung, Überschreiben, Fehlererhalt, Löschung und Nebenläufigkeit ab.

## Schritt 5 – Windows-Fixture-Ablauf und Regression

**Dateien**

- `tests/WindowsJourneyUiTests/UiTestFixtureServices.cs`
- `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`
- Bei erforderlicher Ablaufunterstützung: `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`
- `docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/test-results-departure-cache.md`

**Arbeit**

1. Einen Fixture-Ablauf bereitstellen, der zunächst für einen Favoriten einen dauerhaften zukünftigen Stand erzeugt und beim zweiten Appstart die frische Antwort verzögert.
2. Vor Abschluss der zweiten Antwort per AutomationId sichtbare Cache-Abfahrt und lokalen Aktualisierungshinweis prüfen.
3. Nach Abschluss der Antwort die sichtbare neue Abfahrt und den Abrufzähler prüfen.
4. Einen Ablauf für ausschließlich vergangene Cache-Abfahrten sowie einen für Providerfehler ergänzen.
5. Core-Test-Suite sowie Windows-UiTest-Build mit Warnungen als Fehler ausführen; die native Windows-Matrix als getrennten interaktiven Nachweis dokumentieren.

**Abnahme**

- Der Windows-Ablauf beweist: Cache zuerst sichtbar, Aktualisierung anschließend angefordert, erfolgreiche Antwort ersetzt ihn.
- Es erscheinen weder vergangene Abfahrten noch erfundene Offline-Daten.

## Reihenfolge und Grenzen

Die Schritte 1 und 2 bilden die Datenbasis. Schritt 3 darf erst darauf aufbauen, damit Start- und Löschpfad keinen unvollständigen Speicher sehen. Schritt 4 folgt jeder fachlichen Änderung; Schritt 5 prüft die tatsächliche native Darstellung.

Der Umfang bleibt auf Abfahrten bereits gespeicherter Favoriten beschränkt. Suchen, nahegelegene Haltestellen, Verbindungen, Karten, neue Standortdaten, Serverdienste und Synchronisierung sind nicht Teil dieses Plans.
