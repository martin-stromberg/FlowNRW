# Anforderung – Verdichtete mobile Abfahrtsanzeige

**Erfasst:** 04.10.2026  
**Status:** Verbindliche Ergänzung zu `requirement.md`

## Ziel

Die Start- und Detailansicht sollen auf dem Smartphone schnell erfassbare Abfahrtsinformationen zeigen. Favoriten bleiben auch während und zwischen Aktualisierungen nützlich: zuletzt bekannte Abfahrten erscheinen sofort, und eingeklappte Favoriten geben einen kompakten Überblick über ihre Linien.

## R1 – Entfernung und Standortdiagnose

- Wenn die App trotz aktiviertem GPS „Entfernung unbekannt“ anzeigt, ist die gesamte Standortkette zu untersuchen: erteilte App-Berechtigung, System- und App-Standortdienste, erfolgreiche Positionsbestimmung, verfügbare Koordinaten der Haltestelle sowie Berechnung und Zuordnung der Entfernung.
- Die App darf eine fehlende Entfernung nicht allein aus der Tatsache ableiten, dass keine Position verfügbar ist, ohne den konkreten Zustand nachvollziehbar zu behandeln.
- Der Anwender erhält einen knappen, zutreffenden Hinweis, wenn eine Berechtigung fehlt, Standortdienste ausgeschaltet sind oder keine Position ermittelt werden konnte. Der Hinweis darf keinen GPS-Fehler behaupten, wenn tatsächlich Haltestellenkoordinaten oder die Entfernung fehlen.
- Eine erfolgreiche Positionsermittlung mit berechenbarer Entfernung darf nicht als „Entfernung unbekannt“ erscheinen.
- Standortdaten werden nur gemäß der bestehenden Standortfunktion verwendet; diese Anforderung führt keine zusätzliche Übertragung oder dauerhafte Speicherung ein.

## R2 – Abfahrtskarte verdichten

- Die Ist- beziehungsweise Echtzeit-Abfahrtszeit ist die prominent dargestellte Zeit im Kopf der Abfahrtskarte.
- Weicht die Soll-Zeit von der Ist-Zeit ab, steht die Soll-Zeit kleiner und durchgestrichen direkt unter der Ist-Zeit.
- Eine separate Soll-/Ist-Vergleichszeile entfällt. Ebenso entfallen Texte wie „Abweichung +1 Min.“ und „Verspätung unbekannt“.
- Wenn keine abweichende Ist-Zeit vorliegt, wird keine redundante Soll-Zeit wiederholt.
- Ein geplantes Gleis wird nur angezeigt, wenn es vom aktuellen Gleis abweicht. Ohne Gleisänderung erscheint keine „geplant“-Angabe.
- Während eines Abrufs entfällt der Text „Abfahrten werden aktualisiert“. Ein gut erkennbares, zugängliches Ladesymbol in der rechten oberen Ecke der betroffenen Favoritenkarte zeigt den laufenden Zustand an.
- Die Quelleninformation wird aus der Abfahrtskarte entfernt.
- Linie, Ziel, relevante Zeit sowie tatsächlich vorhandene Echtzeit- und Gleisänderungen bleiben erkennbar.

## R3 – Sofortige Anzeige in der Favoritendetailansicht

- Beim Öffnen der Detailansicht eines Favoriten zeigt die App unmittelbar bereits vorhandene, noch gültige Abfahrtsdaten an, einschließlich Daten, die auf der Startseite schon sichtbar sind.
- Die Detailansicht löst weiterhin die vorgesehene Aktualisierung aus. Währenddessen bleiben die vorhandenen Daten sichtbar und die Ansicht bedienbar; der Ladezustand wird gemäß R2 dargestellt.
- Eine erfolgreiche Antwort ersetzt die angezeigten Daten und aktualisiert den lokalen Bestand. Bei einem fehlgeschlagenen Abruf bleiben weiterhin gültige vorhandene Daten sichtbar.
- Vergangene Abfahrten werden nicht als bevorstehend dargestellt.

## R4 – Eingeklappte Favoriten mit persistentem Linienüberblick

- Favoriten sind beim erstmaligen Anzeigen der Startseite standardmäßig eingeklappt. Der Anwender kann jede Karte auf- und zuklappen.
- Eine eingeklappte Karte zeigt die bekannten Linien nebeneinander. Zu jeder Linie wird die nächste bekannte Abfahrtszeit angezeigt. Ist für die Linie keine Zeit bekannt, wird nur die Linienbezeichnung gezeigt.
- Eine aufgeklappte Karte zeigt die einzelnen Abfahrten in der verdichteten Darstellung nach R2.
- Die Linien eines Favoriten werden lokal dauerhaft gespeichert und nach einem Programmneustart auch vor einer erfolgreichen neuen Abfrage wieder angezeigt. Gespeicherte Linien dürfen dabei ohne gespeicherte Zeit erscheinen.
- Bei einer erfolgreichen, vollständigen Aktualisierung wird der gespeicherte Linienbestand durch die aktuell gemeldeten Linien ersetzt. Eine Linie, die in dieser Antwort nicht mehr vorkommt, verschwindet aus dem Überblick.
- Ein fehlgeschlagener, abgebrochener oder unvollständiger Abruf darf gespeicherte Linien nicht entfernen. Ein leeres Ergebnis darf die Linien nur entfernen, wenn der Anbieter dieses Ergebnis als erfolgreiche vollständige Antwort liefert.
- Die Linien- und Zeitdaten bleiben dem jeweiligen Favoriten beziehungsweise seiner Haltestellenidentität zugeordnet. Das Entfernen eines Favoriten entfernt auch dessen gespeicherten Linienüberblick.
- Eingeklappter Zustand und expandierte Detailansicht dürfen keine widersprüchlichen Datenstände zeigen: erfolgreiche Aktualisierungen müssen beide Darstellungen konsistent erneuern.

## Annahmen und Abgrenzung

- „Ist-Zeit“ bezeichnet die vom Datenanbieter erwartete Echtzeit-Abfahrtszeit. Wenn keine Echtzeitzeit vorliegt, ist die Soll-Zeit die primäre Zeit; sie wird dann nicht zusätzlich als durchgestrichene zweite Zeit wiederholt.
- „Linie“ ist die fachliche Linienbezeichnung aus den gelieferten Abfahrtsdaten. Linien ohne zukünftige Abfahrt können im eingeklappten Überblick bestehen bleiben, wenn sie in der letzten erfolgreichen vollständigen Antwort enthalten waren; eine nächste Zeit wird dann weggelassen.
- Ein abgebrochener oder fehlgeschlagener Abruf ist keine vollständige Antwort und darf daher keinen Linienbestand löschen.
- Das visuelle Verhalten wird für Touchbedienung und schmale Smartphoneansichten ausgelegt. Das Ladesymbol erhält einen zugänglichen Namen beziehungsweise Status für Screenreader.
- Nicht Teil dieser Anforderung sind neue Standortanbieter, eine Änderung der Fahrplandatenquelle, zusätzliche Hintergrundaktualisierung oder eine Synchronisierung der Favoriten zwischen Geräten.

## Abnahme

1. Bei aktiviertem Standortdienst und erteilter App-Berechtigung zeigt ein erfolgreicher Standortabruf bei vorhandenen Haltestellenkoordinaten eine berechnete Entfernung. Lassen sich die Voraussetzungen nicht erfüllen, ist erkennbar, ob Berechtigung, Standortbestimmung oder Haltestellenkoordinaten fehlen.
2. Eine Abfahrtskarte mit Ist-Zeit und abweichender Soll-Zeit zeigt die Ist-Zeit prominent und die Soll-Zeit kleiner sowie durchgestrichen direkt darunter.
3. Es erscheinen keine separate Soll-/Ist-Vergleichszeile, Abweichungs- oder „Verspätung unbekannt“-Texte und keine Quelleninformation.
4. Ohne Gleisänderung erscheint keine geplante Gleisangabe; bei einer Änderung sind aktuelles und geplantes Gleis weiterhin unterscheidbar.
5. Während eines verzögerten Abrufs bleibt die Karte bedienbar, das Ladesymbol erscheint oben rechts und der Text „Abfahrten werden aktualisiert“ wird nicht gezeigt.
6. Nach Öffnen einer Favoritendetailansicht erscheinen vorhandene, noch gültige Abfahrten sofort und bleiben während einer verzögerten oder fehlgeschlagenen Aktualisierung sichtbar.
7. Favoriten erscheinen eingeklappt. Vorhandene Linien und ihre jeweils nächste bekannte Zeit sind nebeneinander sichtbar; Linien ohne bekannte Zeit erscheinen ohne Zeit.
8. Nach Neustart sind zuvor gespeicherte Linien bereits vor Abschluss des Abrufs sichtbar, auch wenn keine Uhrzeit gespeichert ist.
9. Eine erfolgreiche vollständige Aktualisierung fügt neue Linien hinzu, erneuert Zeiten und entfernt nicht mehr gemeldete Linien. Fehler, Abbruch und unvollständige Antworten verändern den gespeicherten Linienbestand nicht.
10. Das Entfernen eines Favoriten entfernt dessen persistierten Linienbestand; kein anderer Favorit übernimmt diese Daten.

## Vorgeschlagene Prüfungen

- Core-Tests für Persistenz, Ersetzen des Linienbestands nach erfolgreicher vollständiger Antwort, Bewahren bei Fehler/Abbruch sowie Zuordnung je Haltestelle.
- UI-Tests mit verzögertem Abruf für sofortige Cacheanzeige in Start- und Detailansicht, eingeklappte Standarddarstellung, Ladeanzeige und Aktualisierung der Linienchips.
- UI-Fälle für fehlende Berechtigung, ausgeschaltete Standortdienste, fehlgeschlagene Positionsbestimmung und fehlende Haltestellenkoordinaten, damit die Ursache der unbekannten Entfernung unterscheidbar bleibt.
- Visuelle Prüfung der Abfahrtskarte auf schmaler Smartphonebreite und Touchbedienung.
