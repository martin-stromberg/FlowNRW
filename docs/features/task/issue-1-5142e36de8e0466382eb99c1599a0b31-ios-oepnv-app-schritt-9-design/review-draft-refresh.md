# Code-Review – aktualisierter Stitch-Entwurf

**Geprüft:** 03.10.2026
**Grundlage:** `requirement-draft-refresh.md`, `draft-refresh-plan.md` und der aktuelle Gesamtdiff
**Status:** Keine umsetzungsblockierenden Codebefunde

## Prüfumfang

- Nearby-Drilldown von der Startseite bis zum vorhandenen Abfahrtsmonitor
- Stabilität von Commands und AutomationIds in Startseite, Monitor, Suche, Detailansicht und Karte
- zentrale helle/dunkle Farben und Kontrastrollen der neuen Karten- und Symbolaktionen
- Beschränkung auf den bereits vereinbarten Funktionsumfang
- Unit- und UI-Testabdeckung der veränderten Interaktion

## Befunde

Keine.

`OpenNearbyFromHomeAsync` akzeptiert nur eine vollständige technische Identität aus `Id` und `Source` und verwendet danach den vorhandenen Monitorpfad. Die strengere Instanzprüfung in `OpenAsync` für Suchergebnisse bleibt erhalten. Der Core-Test deckt sowohl die erfolgreiche externe Nearby-Identität als auch die Ablehnung einer unvollständigen Identität ab.

Die bestehenden AutomationIds und Commands für Suche, Standort, Favoriten, Monitor, Karten-/Listenumschaltung und Detailkarte bleiben bestehen. Die neuen Symbolaktionen haben mindestens 48 logische Pixel und jeweils eine semantische Beschreibung. Die Such- und Kartenansichten führen keine neuen Fach- oder Kartenfunktionen ein.

Für die gemeinsamen Sekundäraktionen, Status-/Metadaten und Kandidatenkarten werden die vorhandenen hellen und dunklen Farbrollen genutzt. Primäre Informationen behalten kontrastreiche Text- und Linienbadge-Darstellungen; Ausfall, Verspätung, fehlende Echtzeit und fehlende Kartengeometrie bleiben als Text sichtbar und werden nicht allein farblich signalisiert.

## Testabdeckung

- `FlowNRW.Tests/StopMonitorTests.cs` prüft den neuen Home-Nearby-Öffnungspfad und die Identitätsvalidierung.
- `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1` ergänzt im bestehenden `-Favorites`-Fixture-Lauf den Pfad `NearbyStop0` → Monitor mit `MonitorStop`, abgeschlossenem `MonitorStatus` und `MonitorMetadata` `fixture-nearby-0`.
- Die AutomationIds der bisherigen Regression bleiben unverändert.

## Offene Abnahme, kein Codebefund

Die geänderten Oberflächen brauchen noch den vorgesehenen nativen Windows-Designmatrixlauf in Hell und Dunkel, einschließlich eines Screenshots des Nearby-Drilldowns. Das ist die noch offene Nachweisarbeit aus Schritt 6 des Plans; sie lässt sich nicht durch den erfolgreichen Kompiliercheck ersetzen. Eine iOS-Geräteabnahme bleibt anschließend beim Nutzer.
