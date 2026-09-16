# Abfahrten einer Haltestelle

Auf der Suchseite **Abfahrten** öffnen, Haltestellenname oder Ort eingeben und **Haltestellen suchen** wählen. Bei mehreren Treffern anhand Name, Kennung und Koordinaten die passende Haltestelle auswählen. Adresstreffer ohne Haltestelle werden nicht angeboten. Ein Standortzugriff ist nicht erforderlich.

Der Monitor zeigt die nächsten gelieferten Abfahrten mit Linie, Ziel, Soll-/Istzeit, Verspätung, Ausfall und Gleis/Steig. Ein Gleiswechsel und ein Ausfall werden ausdrücklich als Text angezeigt. „Keine Echtzeitdaten“ und „Ausfallstatus unbekannt“ sind keine Pünktlichkeits- oder Betriebsgarantie.

**Aktualisieren** lädt manuell neu. Quelle, Datenstand und Datenalter stehen oberhalb der Liste. Bei einem Fehler bleiben letzte bekannte Daten mit einem Fehlerhinweis stehen; eine erfolgreiche leere Antwort zeigt den Leerzustand. Zurück führt zur erhaltenen Haltestellensuche. Ein anderer Stop startet mit eigenen Daten. Automatische Intervalle, GPS und Favoriten folgen in späteren Schritten.

## Einrichtung und iOS-Abnahme

Es gibt keine neue Konfiguration. Es gelten [Providerkonfiguration](../fahrplanauskunft/installation.md) und [Windows-/iOS-Einrichtung](../verbindungssuche/installation.md). Windows-Release und Tests in GitHub Actions bleiben erhalten; iOS-CI, automatisches Deployment und IIS-Präsentation sind nicht Bestandteil dieses Schritts.

Auf dem iOS-Gerät bitte Suche → richtige Haltestelle → Monitor → Aktualisieren → Zurück prüfen, außerdem schmale Darstellung/große Schrift, Scrollen, VoiceOver und Fehler bei unterbrochener Verbindung. Native iOS-Abnahme bleibt beim Nutzer.

## Technischer Aufbau

`StopMonitorViewModel` verwendet eine unabhängige Suche und den vorhandenen `IDepartureService`. Vollständige Stopidentitäten bleiben erhalten. Abbruch und Revisionsnummer verhindern, dass verspätete Antworten eine neue Auswahl überschreiben. Erfolgreiche Ergebnisse und fehlgeschlagene Abrufversuche werden getrennt gehalten. Die vorhandene Providerpriorisierung, Zusammenfassung identischer Anfragen und Cachegrenzen gelten weiterhin. Bekannte vergangene Abfahrten werden anhand Istzeit bzw. Sollzeit plus Verspätung ausgefiltert; unbekannte Zeiten bleiben ausdrücklich unbekannt.

[Prüfnachweise](verification/checks-2026-09-16.md)
