# Abnahmeprüfung – Entwicklungsschritt 8

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Prüfmodus und Stand

26.09.2026, Abnahmerunde 1. Fachliche Gegenprüfung von Projektplan Schritt 8 und requirement.md gegen Basisbranch...24b4d20, Produktquellen und dauerhafte Testbelege. Produktcommit 12cb5a2, Testtimerkorrektur 6f5be95, TRX-Beleg 24b4d20.

Der separate Abnahmeagent acceptance_background_final ist am Nutzungslimit ausgefallen. Diese Abnahme wurde gemäß Ausweichregel des Projekt-Skills lokal als eigene Prüfphase durchgeführt. Sie ist ausdrücklich keine unabhängige Agentenabnahme; diese Einschränkung bleibt erhalten.

## Abgleich

| Kriterium | Tatsächliche Umsetzung und Prüfung |
|---|---|
| 1: iOS und Resume | IosBackgroundRefresh/AppDelegate registrieren vor Launchabschluss; Info.plist enthält fetch und gleiche Kennung. RefreshLifecycle begrenzt auf 20 Sekunden/Vierergruppen, Expiration beendet Arbeit. Native Windows-Lifecycleprüfung bestätigt frisch/alt, Datenalter und sofortige Erneuerung. iOS-C#-Compile erfolgreich; native Ausführung bleibt Nutzeraufgabe. |
| 2: Koordination und Datenschutz | Ein Hintergrundlauf, Abbruch vor Aktivmeldung, geteilte Busy-/Revisionssperren, keine neuen Schleifen oder GPS-/Speicheraufrufe. Coretests prüfen Handoff, Deadline, abgebrochenes Laden, Fehler/verspätete Resultate. Nativer Intervall-/Lifecyclelauf prüft echte Zeiten, Navigation, schnelles Reaktivieren, Off und unabhängige Favoriten. |
| 3: konsistente Bedienung | Native Suche, Auswahl, Karte/Monitor, Favoriten und Einstellungen bleiben navigierbar. Schmale Ansichten und Tastatur geprüft; Status/Datenalter und unsichere Fahrtzuordnung verständlich. Keine neue funktionslose Produktansicht. Die vom Nutzer ergänzte vollständige visuelle Integration einschließlich Themen-/Schriftmatrix bleibt verbindlich Schritt 9 und wird hier nicht als erfüllt vorweggenommen. |
| 4: integrierte Tests | Alle fünf WindowsJourneyUiTests-Modi, vollständiger WindowsRefreshUiTests-Lauf und erweiterter WindowsLifecycleUiTests-Lauf bestanden. Fehlversuche bei Layout-/Navigationswartebedingungen sind archiviert; die vollständigen korrigierten Modi bestehen. |
| 5: Qualität und Windows | Release/UiTest 0 Warnungen/0 Fehler, 236 Coretests, 93,15 Prozent Zeilenabdeckung, Format/XML und 26 Release-Skripttests bestanden. Windows-Workflows unverändert. Kein nativer iOS-Build/Simulator-/Gerätelauf behauptet. |
| 6: Dokumentation | Bestehende Visual-Studio-/Provider-/Karten-/Favoriten-/Cachehilfe bleibt erhalten. Monitorhilfe um Resume, Hintergrundgrenzen, Datenlebensdauer, Testbelege und konkrete iOS-Prüfanleitung erweitert. Kein IIS oder neues Deployment. |

## Abweichungen

Keine fachlichen Abweichungen im vereinbarten Schritt-8-Umfang offen.

## Hinweise

Dauerhafte Belege: docs/help/monitorintervalle/verification-lifecycle/index.md. Die Karte ist ein Snapshot der beim Öffnen gewählten Geometrie; Resume erneuert die Basiskacheln. Aktuelle Details und das erneute Öffnen übernehmen eine eindeutig zugeordnete aktuelle Route. Keine Hintergrundposition oder zusätzliche Datenhistorie.

Freigabe zur lokalen Integration von Schritt 8 auf dem Projektbasisbranch unter dokumentiertem Skillfallback. Kein Gesamtprojektabschluss: Schritt 9 und die externe native iOS-Geräteabnahme bleiben offen.