# Restarbeiten – FlowNRW

Stand: 04.10.2026. Projektzweig: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design`.

Alle fachlichen Schritte 1–8 sind abgeschlossen. Offen ist ausschließlich Schritt 9: visuelle Integration und Abnahme des Designentwurfs. Die verbindlichen Kriterien stehen in [docs/design/acceptance.md](docs/design/acceptance.md). Der aktuelle unabhängige Befund steht in [review-visual-current.md](docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/review-visual-current.md).

## Ausgangslage

Der produktive UI-Stand ist bereits modernisiert: drei beschriftete Kernbereiche, gerundete Karten, helle und dunkle Tokens, farbige Linienbadges, verdichtete Abfahrten, Favoriten, Haltestellensuche, Karte, Verbindungssuche und Details.

Die letzten relevanten Commits sind:

- `5c019a5 feat: refine visual transit flows` – Textverdichtung, Kartenaktion in der Suche, Ergebnis- und Detailbereinigung.
- `fa9cdf1 test: stabilize visual fixture journeys` – robuste Journey- und Designrunner, plausible sichtbare Fixture-Daten.
- `258e280 test: complete dark visual matrix` – portable Buildhash-Ermittlung und dokumentierter Dark-Mode-Vollauf.

Folgende native Windows-Prüfungen sind zuletzt erfolgreich gelaufen:

```powershell
dotnet build .\FlowNRW\FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore
dotnet test .\FlowNRW.Tests\FlowNRW.Tests.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore
.\tests\WindowsJourneyUiTests\WindowsJourneyUiTests.ps1 -Exe 'FlowNRW\bin\UiTest\net10.0-windows10.0.19041.0\win-x64\FlowNRW.exe'
.\tests\WindowsJourneyUiTests\WindowsDepartureCacheUiTests.ps1 -Exe 'FlowNRW\bin\UiTest\net10.0-windows10.0.19041.0\win-x64\FlowNRW.exe'
```

Erwartete Endzeilen: `PASS all fixture native UI scenarios` beziehungsweise `PASS native departure-cache startup regression`.

`WindowsDesignUiTests.ps1` verwendet jetzt standardmäßig das vollständige Szenario `favorite-cache-seed`; der Aufruf benötigt deshalb keinen `-Scenario`-Parameter mehr.

## Arbeitsbaum bereinigen

Nicht committen:

- `design-draft.zip`
- `stitch_nrw_transit_ios_app.zip`
- `stitch_nrw_transit_ios_app/`
- die von lokalen Matrixläufen erzeugten PNGs und Manifeste unter `docs/help/design/verification/matrix/`

Diese lokalen Matrixartefakte haben bei früheren Läufen bereits versionierte Beispielbilder überschrieben. Vor neuen Läufen ein separates, unversioniertes Ziel verwenden, zum Beispiel:

```powershell
$out = 'artifacts\step9-visual-final\<bezeichner>'
.\tests\WindowsJourneyUiTests\WindowsDesignUiTests.ps1 `
  -Exe 'FlowNRW\bin\UiTest\net10.0-windows10.0.19041.0\win-x64\FlowNRW.exe' `
  -ScreenshotDirectory $out -Theme dark -Width 430 -Height 900 -TextScale 150
```

Nach lokalen Läufen `git status --short` prüfen. Bereits geänderte, versionierte Beispielbilder unter `docs/help/design/verification/matrix/` nur zurücksetzen, wenn ihre Änderungen allein aus einem lokalen Testlauf stammen.

## Verbleibende Windows-Abnahme

1. **Schmal und große Schrift vollständig aufnehmen.**
   Die vollständige Matrix in mindestens diesen Varianten ausführen und jeweils auf `PASS native design matrix sequence` prüfen:

   ```powershell
   # schmal, hell, Standardgröße
   .\tests\WindowsJourneyUiTests\WindowsDesignUiTests.ps1 -Exe 'FlowNRW\bin\UiTest\net10.0-windows10.0.19041.0\win-x64\FlowNRW.exe' -ScreenshotDirectory artifacts\step9-visual-final\light-narrow-final -Theme light -Width 430 -Height 900 -TextScale 100

   # schmal, dunkel, große Schrift
   .\tests\WindowsJourneyUiTests\WindowsDesignUiTests.ps1 -Exe 'FlowNRW\bin\UiTest\net10.0-windows10.0.19041.0\win-x64\FlowNRW.exe' -ScreenshotDirectory artifacts\step9-visual-final\dark-narrow-final-150 -Theme dark -Width 430 -Height 900 -TextScale 150
   ```

   Der zweite Lauf war zum Übergabezeitpunkt bereits gestartet, aber noch nicht abschließend ausgewertet. In den Bildern insbesondere Formular, Suchergebnisse, Verbindungsdetail, Haltestellenkarte, Offlinezustand und fehlende Position auf Überdeckung, abgeschnittene Texte und mindestens 44×44 große Ziele prüfen.

2. **Finale Buildzuordnung sicherstellen.**
   Alle final verwendeten Matrixbilder sollen nach einem sauberen Commit entstehen. Das erzeugte `matrix.jsonl` enthält Commit, Build-SHA, Thema, Fenstergröße, DPI, Textskalierung und Szenario. Keine Bilder mit `workingTree: tracked files modified` als finale Abnahme verwenden.

3. **Cache während Refresh visuell belegen.**
   Der Funktionsnachweis ist vorhanden: `WindowsDepartureCacheUiTests.ps1` prüft gespeicherte Abfahrten vor der verzögerten Antwort, die Ersetzung durch Live-Daten und die Detailübergabe. Es fehlt ein zugehöriges Bild. Entweder:
   - den Cachetest um optionales, sauber gekapseltes Capturing erweitern, oder
   - im Designrunner einen dedizierten Modus `cache-start-slow-nearby` ergänzen, der zuerst die gespeicherte Abfahrt plus Ladeindikator aufnimmt und danach die Live-Abfahrt.

   Das Bild muss zeigen, dass vorhandene Abfahrten nicht leer werden, während die Aktualisierung läuft.

4. **Visuelle Nachprüfung wiederholen.**
   Nach den Bildern eine unabhängige Prüfung gegen `docs/design/acceptance.md` und die Stitch-Referenzen durchführen. Den Bericht unter `docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/` aktualisieren. Nicht als abgeschlossen markieren, solange die nachstehenden iOS-Punkte offen sind.

## Bekannte Designbefunde

Die nachstehenden Befunde wurden bereits im Produktcode bearbeitet, müssen aber auf Bildern des finalen Builds erneut kontrolliert werden:

- Nach Rückkehr aus dem Haltestellenmonitor darf „Endpunkt übernommen.“ nicht sichtbar bleiben.
- Verbindungsdetails dürfen nur den kompakten Hinweis „Daten möglicherweise unvollständig“ zeigen, keine mehrzeilige Provider-/Cache-/Zeitdiagnose oberhalb des Inhalts.
- Verbindungskarten sollen das Datum knapp und beschriftet als `Abfahrt: TT.MM.JJJJ` zeigen, nicht mit unbeschrifteter UTC-Zeit wiederholen.
- Die Kartenaktion der Haltestellensuche gehört neben Suche und Standortaktion.
- Sichtbare Fixture-Namen sollen plausibel sein: Stationsname bzw. „(Umgebung)“, Betreiber „Regionalverkehr NRW“ und „Stadtwerke Essen“. Interne Fixture-IDs dürfen weiterhin in nicht sichtbaren Automationsdaten vorkommen.
- Die Kartenfixtures sind nur Layoutnachweise; eine echte Karte ist separat auf iOS beziehungsweise mit Release/Netzwerk zu prüfen.

## iOS-Abnahme durch den Nutzer

Windows ersetzt keine iOS-Abnahme. Die Checkliste liegt in [ios-device-acceptance.md](docs/features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/ios-device-acceptance.md).

Auf mindestens einem kleinen und einem großen iPhone prüfen und dokumentieren:

- hell/dunkel, Hoch-/Querformat, obere und untere Safe Areas;
- Dynamic Type mindestens große Stufe;
- VoiceOver-Namen, Reihenfolge und Statusmeldungen;
- reale GPS-Entfernung und Haltestellen in der Nähe;
- Favoriten-Cache beim Start und beim Öffnen des Monitors während Refresh;
- Verbindungssuche, Favorisieren und Start/Ziel-Tausch;
- Kartenansicht, Attribution, Offline- und fehlende-Position-Zustand.

Pro Ergebnis Gerät, iOS-Version, Thema, Textgröße, Orientierung und beobachtetes Verhalten festhalten. Fehler als konkrete Korrekturaufgabe erfassen.

## Abschluss von Schritt 9

Schritt 9 erst dann in `docs/projects/.../steps.md` auf `Fertig` setzen, wenn:

1. die finale Windows-Matrix einschließlich schmaler/großer Schrift und Dark-Mode-Karte vorliegt,
2. der Cache-Refresh visuell belegt ist,
3. die unabhängige Bildprüfung keine offenen Produktbefunde mehr hat oder begründete Grenzen dokumentiert,
4. die iOS-Geräteabnahme dokumentiert ist,
5. alle betroffenen Windows-Regressionen und der UiTest-Build erfolgreich sind.
