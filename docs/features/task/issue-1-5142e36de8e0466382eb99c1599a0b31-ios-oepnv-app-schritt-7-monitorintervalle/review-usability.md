# Usability-Review

## Ergebnis

**Status:** Keine Befunde

Im geprüften Umfang sind die geforderten Bedienhandlungen verständlich erreichbar. Dies ist ein unabhängiges Review anhand der fachlichen Anforderung, der tatsächlich implementierten Oberflächen und vorhandener nativer Windows-Screenshots; keine vollständige native Laufzeitabnahme.

## Geprüfte Interaktionen

- Einstellungen vom Einzelmonitor und von der Favoritenstartseite öffnen → unauffällig: jeweils sichtbar beschriftete Schaltfläche „Aktualisierung einstellen“.
- Automatische Aktualisierung auswählen, ändern oder ausschalten → unauffällig: endliche Klartextauswahl „Aus“, „30 Sekunden“, „60 Sekunden“, „2 Minuten“, „5 Minuten“, keine technischen Kennungen oder freie ungültige Eingabe erforderlich.
- Auswahl ausdrücklich speichern und wirksames Intervall erkennen → unauffällig: „Speichern“, Erfolgsmeldung und separate Beschreibung des wirksamen Intervalls. Speicherfehler benennen die unverändert wirksame Einstellung und die Wiederholungsmöglichkeit.
- Standard und Geltungsbereich verstehen → unauffällig: 60 Sekunden, Mindestabstand und Beschränkung auf aktive Monitore/Favoriten sind erklärt.
- Automatische Aktualisierung wahrnehmen → unauffällig hinsichtlich Rückmeldung: Monitorstatus unterscheidet automatisch und manuell; Datenstand ist sichtbar. Ein vorhandener nativer Teilnachweis bestätigt eine sichtbare Änderung nach echtem 30-Sekunden-Intervall.
- Manuell aktualisieren, auch bei ausgeschalteter Automatik → unauffällig: „Aktualisieren“ bleibt am Einzelmonitor und pro Favoritenkarte vorhanden.
- Nach Fehlern letzte bekannte Abfahrten und deren Alter erkennen → unauffällig: ausdrücklicher Fehler-/Bestandsdatenhinweis sowie Quelle, Datenstand und Datenalter; vorhandener Monitor-Testnachweis bestätigt Daten- und Metadatenerhalt.
- Zwischen Einstellungen, Einzelmonitor, Favoriten und Suche navigieren → unauffällig hinsichtlich sichtbarer Zugänge und Rücknavigation. Vollständige Laufzeitprüfung der beendeten bzw. nicht doppelten Schleifen gehört zum separaten Testlauf.
- Gespeicherte Auswahl nach Neustart wiederfinden → vorhandener nativer Teilnachweis bestätigt 30-Sekunden-Persistenz über einen neuen Prozess; nicht in diesem Review selbst ausgeführt.
- Bestehende Favoriten-/Monitor-/Routing-Zugänge weiterhin benutzen → sichtbare Aktionen bleiben erreichbar; vorhandene Monitor- und Favoritenprotokolle bestätigen ausgewählte Navigations- und Bestandsabläufe.

## Sichtprüfung

Die vorhandenen Screenshots mit 410 Pixel Breite zeigen umbrochene Erläuterungen, lesbare Auswahl und ausreichend breite Aktionsschaltflächen. Intervallstatus und Speicherrückmeldung sind nicht allein farblich codiert. Lange Abfahrtslisten liegen in einer Scrollansicht. Im Screenshot enthaltene Szenarioeingaben und Abrufzähler gehören zum bedingt kompilierten UI-Testaufbau, nicht zur normalen Bedienung.

Endgültiges Designmatching ist gemäß Reviewauftrag erst Gegenstand von Schritt 9; hier wurden Bedienbarkeit und Erreichbarkeit der Monitorintervalle geprüft.

## Prüfumfang und Grenzen

- Basisbranch explizit gemäß Auftrag: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`. Geänderte UI-Dateien wurden über den Merge-Base-Diff bestimmt.
- Fachliche Grundlage ausschließlich `requirement.md` dieses Features. `plan.md`, `review.md` und `review-code.md` wurden nicht gelesen.
- Kein eigener Build, keine GUI-Aktion und kein paralleler nativer Testlauf, da der andere Testagent die Windows-UI exklusiv verwendet.
- Gesehene Bildnachweise: `artifacts/step7-ui/native-refresh-settings-narrow.png`, `native-departures-narrow.png`, `native-favorites-narrow.png`.
- Gelesene vorhandene Testprotokolle: `artifacts/step7-ui/monitors.txt`, `favorites.txt`, `intervals-retry1.txt`. Das Intervallprotokoll war beim Lesen noch unvollständig und enthielt keinen abschließenden Gesamterfolg. Die dort bereits protokollierten PASS-Schritte werden ausdrücklich nur als Teilnachweis gewertet.
- „Keine Befunde“ bedeutet keine im geprüften Material erkannten Bedienbarkeitshindernisse; keine Bestätigung sämtlicher zeitabhängiger Akzeptanzkriterien, des kompletten Routingflusses oder erfolgreicher endgültiger E2E-Abnahme.
- iOS, VoiceOver/Narrator, stark vergrößerte Systemschrift und reale Touchbedienung wurden nicht ausgeführt. Native iOS-Abnahme bleibt beim Nutzer.

## Geprüfte Dateien

- `FlowNRW/RefreshSettingsPage.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/AppShell.xaml.cs`
- `FlowNRW/App.xaml.cs`
- `FlowNRW.Core/Refresh/RefreshSettingsViewModel.cs`
- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
