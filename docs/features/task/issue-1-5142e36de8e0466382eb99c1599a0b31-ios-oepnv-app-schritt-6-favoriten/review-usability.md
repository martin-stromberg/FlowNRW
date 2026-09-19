# Usability-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine konkrete Bedienabweichung im geprüften Umfang gefunden.

## Geprüfte Abläufe und Darstellung

Die Startseite bietet verständliche Aktionen für Verbindungssuche, Haltestelle hinzufügen, Entfernungen und Favoritenkarte. Favoritenkarten zeigen benannten Ort, ausdrücklich als Luftlinie bezeichnete Entfernung, Abfahrten sowie Datenherkunft/-alter und verständliche Fehlerangaben. Unbekannte Entfernung und fehlende Standortberechtigung werden ohne fiktive Null behandelt. Hinzufügen/Entfernen zeigt erst nach erfolgreicher Speicherung Erfolg; Fehler und Wiederholung sind nativ geprüft.

Screenshot artifacts/step6-ui/native-favorites-narrow.png tatsächlich geöffnet und visuell geprüft: bei 430 Pixel Fensterbreite sind die Hauptaktionen lesbar und ausreichend hoch; Standorthinweise und Quellen-/Fallbacktexte umbrechen. Der erste Favorit ist mit Name, Luftlinie und Abfahrten erkennbar. Weiterer Inhalt liegt im vorgesehenen ScrollView. Die technischen Scenario-/Aufrufzähler oben sind ausschließlich Fixtureinstrumentierung und keine Release-Produktoberfläche.

Native Favoritentests belegen Tastaturbedienung, Karten-/Monitornavigation, Rücknavigation, Sortierung, Fehlerbehandlung und vier echte Prozessstarts. Die Startseite bleibt während des Abrufs einer Karte über die andere bedienbar. Keine allein farbliche Kennzeichnung für Lade-/Fehler-/Cachezustände.

## Grenzen

Ergänzende unabhängige Sichtprüfung durch denselben Agenten, der zuvor Plan und Code geprüft hat; kein frischer, gegenüber dem Plan blinder Usabilityagent. Kein vollständiger Accessibilityaudit. Globale Windows-Schriftvergrößerung und iOS-VoiceOver/Gerätebedienung nicht als ausgeführt behauptet; native iOS-Abnahme bleibt gemäß Nutzervereinbarung beim Nutzer. Noch laufende Regressionen gehören zum separaten Testabschluss.
