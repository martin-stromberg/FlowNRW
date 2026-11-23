# Anforderung – Reduktion hinderlicher Informationstexte

**Erfasst:** 04.10.2026
**Status:** Verbindliche Ergänzung zu `requirement.md`

## Ziel

Die Startseite soll sich auf die unmittelbar nutzbaren Favoriten und deren Abfahrten konzentrieren. Wiederkehrende Erläuterungen und Bestandsangaben, die bei der täglichen Nutzung keinen Mehrwert liefern, dürfen keinen Raum beanspruchen oder wie ein Testmodus wirken.

## R1 – Favoritenanzahl entfernen

- Das Panel beziehungsweise der Text zur Anzahl gespeicherter Favoriten wird von der Startseite entfernt.
- Dazu gehören doppelte, grammatikalisch abgeleitete oder sonstige Varianten wie „1 Favorit“, „1 Favoriten“ und ähnliche Bestandsanzeigen.
- Die Favoritenkarten und ihre sichtbaren Abfahrten bleiben unverändert erreichbar.

## R2 – Kontextabhängige Hinweise

- Weitere rein beschreibende Persistenz- oder Startstatus-Texte sind zu entfernen oder auf eine unaufdringliche Kurzform zu reduzieren, wenn sie weder eine aktive Handlung ermöglichen noch einen aktuellen Fehler- oder Datenfrischezustand vermitteln.
- Hinweise, die nur beim Programmstart von Interesse wären, sollen nicht dauerhaft zwischen oder über den Favoriten erscheinen.
- Die leere Startseite darf weiterhin eine kurze, handlungsorientierte Erklärung zum Hinzufügen des ersten Favoriten enthalten.

## R3 – Informationen, die erhalten bleiben

- Statusinformationen bleiben sichtbar, solange sie für eine laufende Handlung, einen Fehler oder die Frische sichtbarer Abfahrtsdaten relevant sind. Beispiele sind ein noch laufender Abruf, ein fehlgeschlagener Abruf mit nutzbaren lokalen Daten oder ein klarer Leerzustand.
- Bedienelemente behalten ihre Beschriftung oder zugängliche semantische Beschreibung, wenn dies für die Bedienbarkeit erforderlich ist.
- Fachliche Daten einer Abfahrt, etwa Linie, Ziel, Zeit, Echtzeitabweichung und notwendige Betreiber- oder Quellenangaben, werden nicht allein wegen dieser Anforderung entfernt.

## Abnahme

1. Die Startseite zeigt keine Anzahl gespeicherter Favoriten und kein zugehöriges Panel mehr.
2. Bei mindestens einem gespeicherten Favoriten sind die Favoritenkarte und ihre Abfahrten ohne vorgeschaltete, wiederkehrende Startinformation erreichbar.
3. Ein leerer Zustand bleibt verständlich und bietet eine erkennbare nächste Handlung.
4. Laufende Aktualisierungen, Fehler und relevante Datenfrischehinweise bleiben verständlich, knapp und ohne technische Interna erkennbar.
5. Es werden keine fachlichen Funktionen, gespeicherten Daten oder bestehenden Zugänglichkeitsinformationen entfernt.

## Nicht-Ziele

- Keine Änderung an der Favoritenverwaltung, der Anbieterabfrage oder dem Abfahrtscache.
- Keine Entfernung von Fehler- oder Ladeinformationen, die für die Beurteilung der sichtbaren Daten erforderlich sind.
- Keine neuen Funktionen oder Navigationswege.
