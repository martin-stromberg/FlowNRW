# Abnahmeprüfung – Entwicklungsschritt 5

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Abweichungen

Keine.

## Hinweise

Unabhängige fachliche Abnahme am 18.09.2026 durch einen separaten Prüfagenten. Prüfgegenstand ist Produktcommit `5b96d81` auf `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-5-standort`, verglichen mit Projektbasis `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Grundlage sind die ursprüngliche Schrittanforderung mit vier Akzeptanzkriterien, der tatsächliche Branchdiff, relevante vollständige Quellen und ausgeführte Testnachweise; keine erneute Abnahme der bereits abgeschlossenen Schritte 1–4.

| Akzeptanzkriterium | Festgestelltes Verhalten und Nachweis |
|---|---|
| 1: Aktueller Standort als Start/Ziel, Plattformintegration, Fehler und manuelle Nutzung | `MauiCurrentLocationService`, `CurrentLocation`, `EndpointViewModel`, `SearchPage` und DI verbinden explizite Standortbuttons mit einer aktuellen Einzelabfrage. iOS-Nutzungsbeschreibung vorhanden; Disabled sowie iOS Denied/Restricted werden vor erneuter Anforderung behandelt. Cancellation wird auch unmittelbar vor dem MainThread-Berechtigungsaufruf geprüft. Fehler behalten eine abgeschlossene Auswahl und lassen manuelle Eingabe/Wiederholung zu. Native Fixture-Flüsse führen GPS-Start und GPS-Ziel jeweils durch Ergebnisse/Details/Zurück. Echter Release-OS-Nachweis bestätigt eine übernommene Position. |
| 2: Passende nahe Stationen, Liste/Karte und richtiger Monitor, manuelle Alternative | Eigene Nearby-Serviceinstanz erhält die tatsächliche Standortkoordinate. Vollständige Stopobjekte und aktive Quellenmetadaten bleiben erhalten; Kartenwahl und Liste verwenden dieselben Kandidaten mit Referenzmitgliedschaft. Native Tests prüfen Listenwahl, tatsächlichen Mausklick auf Marker, Tastaturwahl der Kartenliste und Stops ohne Kartenposition. Echter Release-Nahbereich liefert einen auswählbaren Kandidaten, der den Monitor öffnet. Manuelle Monitor-/Kartenregression ist erfolgreich. |
| 3: Angeforderter Zugriff, keine Historie/sensiblen Logs, Schutz vor alten Antworten und erfundenen Werten | Kein Standortaufruf beim Einstieg oder bei manueller Suche; keine neue Persistenz, kein Tracking. Unabhängige Revisionen/Token schützen Endpunkte und gesamte Standort→Nearby-Kette einschließlich Seitenabgang. Alte Karten dürfen wertgleiche neue Kandidaten nicht auswählen. Fehlende/ungültige Entfernungen bleiben unbekannt, fehlende Kartenpunkte werden nicht ersetzt. Kontrollierte Tests enthalten auch Anbieter, die Cancellation ignorieren, und sensible Exceptiontexte, die nicht in der UI erscheinen. |
| 4: Deterministische/native Tests, getrennte OS-Nachweise, iOS-Anleitung | 165/165 Coretests und 94,42 % Zeilenabdeckung dokumentiert. Native Standort- und Routing-/Monitor-/Kartenregressionen liegen vor. Fixture-Ablehnung/Entzug bleiben ausdrücklich simuliert. Echter Windows-OS-Erfolg separat protokolliert. iOS-Gerätecheckliste umfasst Zustimmung, Entzug, reduzierte Genauigkeit, deaktivierte Dienste, Fehler, manuelle Rückfälle und Navigation. |

Gelesene produktive Änderungen: `FlowNRW.Core/Presentation/CurrentLocation.cs`, `EndpointViewModel.cs`, `StopMonitorViewModel.cs`, `FlowNRW.Core/Maps/MapViewModel.cs`, `FlowNRW/Services/MauiCurrentLocationService.cs`, `MauiProgram.cs`, `SearchPage.cs`, `StopSearchPage.cs` und iOS-`Info.plist`. Zusätzlich `FlowNRW.Tests/LocationTests.cs`, native Harnessänderungen und deren Ergebnisprotokolle geprüft. Die drei früheren konkreten Adapterbefunde sind im abgenommenen Stand behoben.

Nachweise unter `docs/help/standort/verification/`: `checks-2026-09-18.md`, `core-cobertura.xml`, `native-location-fixtures.txt`, `native-location-os.txt`, `native-routing-regression.txt`, `native-monitor-regression.txt` und `native-map-regression.txt`. Die beiden schmalen Fixture-Screenshots wurden vom Prüfagenten zusätzlich tatsächlich visuell geöffnet: Standortaktion, Nahbereichsaktion und Hinweise sind lesbar; Texte umbrechen. Die sichtbaren Szenariofelder gehören ausschließlich zum UiTest-Build. Windows Release-/UiTest-Builds mit Warnungen als Fehler sowie Format-/XML-Prüfungen sind erfolgreich dokumentiert. Diese Abnahme hat bereits ausgeführte Nachweise geprüft und die gesamte Suite nicht nochmals ausgeführt.

Die echte Windows-Probe erfolgte nach ausdrücklicher Nutzerfreigabe für Standortabruf und Providerübermittlung; der vorherige Freigabeblock ist dadurch geklärt. Das Protokoll bestätigt eine echte Position und reale Nahbereichsauswahl bis zum geöffneten Monitor. Es bestätigt ausdrücklich keine erfolgreich gelieferten Live-Abfahrten. Tatsächliche Koordinaten, Stopidentitäten und Screenshots wurden dabei nicht dauerhaft protokolliert. OS-Dialogablehnung und Entzug wurden nicht zusätzlich durch Änderung privater Systemeinstellungen erzwungen; diese Zustände sind deterministisch und im nativen Fixture-Build geprüft.

Native iOS-Ausführung bleibt gemäß Nutzervereinbarung beim Nutzer und wird hier weder als ausgeführt noch als bestanden behauptet. Globale Windows-Schriftvergrößerung und iOS-VoiceOver/Pinch sind keine behaupteten Prüfungen. Windows-GitHub-Actions bleiben erhalten; kein IIS, keine iOS-CI und kein automatisiertes Deployment hinzugefügt. Diese verbleibenden vereinbarten Plattformgrenzen sind keine Abweichung vom Umfang dieses Schritts.
