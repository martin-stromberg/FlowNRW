# Anforderung – Favoriten, Haltestellensuche und Verbindungen

**Erfasst:** 04.10.2026  
**Status:** Verbindliche Ergänzung zu `requirement.md`

## Ziel

Die Favoriten- und Suchabläufe sollen ihren Zustand beim Navigieren behalten und wiederverwendbare Verbindungen schneller zugänglich machen. Abfahrtsinformationen sollen in eingeklappten Favoriten kompakt, aber visuell eindeutig bleiben.

## R1 – Linienchips in eingeklappten Favoriten

- Eingeklappte Favoriten verwenden dieselbe farbige Linienkennzeichnung wie die Abfahrtszeilen im geöffneten Zustand.
- Die Linienkennzeichnung bleibt auch bei schmaler Smartphoneansicht lesbar und zeigt die Linien nebeneinander.
- Im geöffneten Zustand werden die nächsten Uhrzeiten aus der kompakten Linienübersicht nicht zusätzlich angezeigt. Dort stehen die einzelnen Abfahrten mit ihren Zeiten.
- Die Symbole zum Öffnen und Schließen werden durch eindeutig erkennbare, zugängliche Symbole ersetzt. Ihre Bedeutung ist auch ohne Farbe und für Screenreader verständlich.

## R2 – Abfahrtsdaten und Suchergebnisse bei Haltestellennavigation behalten

- Die Haltestellenansicht nutzt den vorhandenen lokalen Abfahrtscache. Gültige gespeicherte Abfahrten erscheinen sofort beim Öffnen einer Haltestelle, während eine Aktualisierung läuft.
- Wird dieselbe Haltestelle erneut aus der Suche geöffnet, bleiben gültige, zuvor geladene Abfahrten sofort sichtbar.
- Die zuletzt angezeigte Ergebnisliste der Haltestellensuche bleibt erhalten, wenn der Anwender eine Haltestelle öffnet und von dort zur Suche zurückkehrt.
- Suchtext und Auswahlzustand bleiben soweit sinnvoll erhalten, sodass eine erneute Suche nicht unnötig von vorn begonnen werden muss.
- Eine erfolgreiche Aktualisierung ersetzt die sichtbaren Abfahrten und den zugehörigen Cache. Fehlgeschlagene oder abgebrochene Abrufe löschen weiterhin gültige gespeicherte Ergebnisse nicht.

## R3 – Verbindung als Favorit speichern und wiederverwenden

- Wenn Verbindungsergebnisse angezeigt werden, zeigt eine Kopfzeile die gewählte Verbindung mit Start- und Zielhaltestelle.
- Die Kopfzeile enthält ein Favoritensymbol, mit dem diese Verbindung gespeichert und wieder entfernt werden kann.
- Gespeicherte Verbindungen stehen in der Verbindungssuche zur Auswahl bereit. Eine Auswahl befüllt Start und Ziel, ohne dass die Haltestellen erneut gesucht werden müssen.
- Favoriten lassen sich anhand der angezeigten Start- und Zielnamen unterscheiden. Technische Haltestellenkennungen werden nicht als nutzerseitiger Text dargestellt.
- Die Verbindungsfavoriten bleiben nach einem Neustart verfügbar.

## R4 – Start und Ziel vertauschen

- Die Verbindungssuche bietet eine gut erreichbare Aktion, um Start und Ziel zu vertauschen.
- Nach dem Vertauschen werden die beiden ausgewählten Haltestellen in den jeweils anderen Feldern angezeigt.
- Die Aktion funktioniert auch bei Auswahl aus einem gespeicherten Verbindungsfavoriten.
- Das Vertauschen startet nicht ungefragt eine neue Suche; der Anwender löst die Suche anschließend selbst aus.

## R5 – Verbindungsdetails auf relevante Angaben begrenzen

- Die Detailansicht einer Verbindung wird auf Informationen geprüft, die für Orientierung und Durchführung der Fahrt benötigt werden.
- Redundante Beschriftungen, wiederholte Zusammenfassungen und technische oder sonstige nicht handlungsrelevante Angaben werden entfernt.
- Start, Ziel, Reihenfolge der Fahrtabschnitte, Umstiege sowie relevante Gehwege und Umstiegszeiten bleiben verständlich.
- Die Bereinigung darf wesentliche Fahrtinformationen, Barrierehinweise oder notwendige Hinweise zu fehlenden beziehungsweise nicht verfügbaren Daten nicht entfernen.

## Annahmen und Abgrenzung

- Cache-Einträge gelten nur dann als sofort anzeigbar, wenn sie nach den bestehenden Regeln noch gültig und nicht veraltet sind.
- Ein gespeicherter Verbindungsfavorit enthält die stabile Identität beider Haltestellen sowie genug Anzeigeinformation, um ihn nach einem Neustart wieder auszuwählen.
- Es wird kein neuer Anbieter und keine Synchronisierung zwischen Geräten eingeführt.
- „Unnötige Informationen“ in Verbindungsdetails werden anhand der oben genannten Orientierungs- und Durchführungsaufgaben bereinigt; sicherheits- oder fahrtbezogene Angaben bleiben erhalten.

## Abnahme

1. Eine Linie erscheint eingeklappt als farbiger Chip wie in der geöffneten Abfahrtsliste. Im geöffneten Zustand wird ihre nächste Uhrzeit nicht zusätzlich in einer kompakten Kopfzeile wiederholt.
2. Auf- und Zuklappen ist durch verständliche Symbole erkennbar und per Touch sowie Screenreader bedienbar.
3. Beim Öffnen einer Haltestelle erscheinen gültige Cache-Abfahrten sofort, auch wenn anschließend aktualisiert wird.
4. Wird dieselbe Haltestelle nach Rückkehr zur Suche erneut geöffnet, erscheint der gültige gespeicherte Bestand ohne leere Zwischenansicht.
5. Nach Rückkehr aus einer Haltestellenansicht ist die vorherige Ergebnisliste der Haltestellensuche noch vorhanden.
6. Die Ergebnisansicht einer Verbindung zeigt Start und Ziel in einer Kopfzeile. Das Favoritensymbol fügt die Verbindung hinzu beziehungsweise entfernt sie.
7. Ein gespeicherter Verbindungsfavorit ist nach Neustart in der Suche auswählbar und befüllt Start und Ziel korrekt.
8. Die Vertauschaktion wechselt Start und Ziel vollständig und löst keine Suche aus.
9. Die Verbindungsdetailansicht enthält keine redundanten oder rein technischen Informationen; Fahrtabschnitte, Umstiege und relevante Gehwege bleiben nachvollziehbar.

## Vorgeschlagene Prüfungen

- UI-Tests für farbige Linienchips sowie angezeigte beziehungsweise nicht angezeigte Zeiten in beiden Karten-Zuständen.
- UI-Tests für Touch- und Accessibility-Verhalten der Expand-/Collapse-Symbole.
- Cache- und Navigationstests mit verzögertem Abruf: gültige Daten sind sofort sichtbar, Suchergebnisse bleiben nach Rückkehr erhalten, ein erneutes Öffnen derselben Haltestelle startet nicht mit einer leeren Ansicht.
- Persistenztests für gespeicherte Verbindungsfavoriten einschließlich Wiederherstellung der Start-/Zielauswahl nach Neustart.
- UI-Tests, dass Vertauschen nur die Felder wechselt und keine automatische Routensuche startet.
- Prüfung der Detailansicht mit mehrteiligen Verbindungen, Umstiegen und Gehwegen, damit die Textbereinigung keine relevanten Fahrtinformationen entfernt.
