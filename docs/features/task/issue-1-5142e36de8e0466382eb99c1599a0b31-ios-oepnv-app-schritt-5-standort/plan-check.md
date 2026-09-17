# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK 1: Standort als Start/Ziel nach Zustimmung; iOS/Windows; verständliche Fehler und manuelle Alternative | Programmablauf Standort 1–5, OS-Adapter, Endpunkt-/Seiten-/DI-Änderungen und iOS-Plist; Berechtigungsprüfung je Anfrage, getrenntes Positionstimeout | LocationTests für unabhängige Endpunkte und Fehlerzustände; native GPS-Start-/Ziel-Flüsse, Ablehnung/Entzug/Timeout/fehlende Position, reale Windows-Probe und iOS-Anleitung | Abgedeckt |
| AK 2: Tatsächliche Umgebung in Liste/Karte; richtige Monitore; manuelle Umgebung | Nearby-Kette mit eigener Serviceinstanz, aktive vollständige Kandidaten, gemeinsame Monitorvalidierung und Kartenmetadaten; fehlende Kartenposition lässt Listenwahl zu | Exakte an Nearby übergebene Fixturekoordinate, Stopidentität/Metadaten; native Liste → Monitor sowie Marker/Listenalternative → Monitor; manuelle Regression | Abgedeckt |
| AK 3: Nur angeforderter Standort, keine Historie/sensiblen Logs; keine veraltete Übernahme oder erfundenen Werte | Explizite Einzelabfrage ohne Tracking/LastKnown; flüchtiger Zustand; Revision für gesamte Standort-/Nearby-Kette; Koordinaten-/Zeit-/Genauigkeits-/Entfernungsvalidierung | Keine Anfrage beim Einstieg; verspätete Standort-/Providerantworten gegen manuelle Auswahl/neue Umgebung/Seitenabgang; bekannte/unbekannte Entfernung; getrennte datensparsame OS-Protokollierung | Abgedeckt |
| AK 4: Deterministische Fehler-/Zuordnungstests, native Windows-E2E, klare OS-/Fixture-Trennung, iOS-Prüfanleitung | Umsetzungsreihenfolge 4–5 und vollständiger Tests-/Dokumentationsabschnitt; Fixture-DI ausschließlich UiTest | Native Pflichtszenarien, Release-OS-Probe, tatsächliche Grenzen ausdrücklich kein PASS; iOS-Zustimmung/Ablehnung/Entzug/reduzierte Genauigkeit separat | Abgedeckt |
| Rahmenbedingungen: vorhandene Services/MVVM/DI, Windows-Release/-Tests, keine iOS-CI/Deployment/IIS | Bestehender Datenkern und unabhängiger Nearby-Scope; keine Paket-/Packagingumstellung; unveränderte Windows-Actions; keine zusätzliche Persistenz | Gesamte Core-Suite/Coverage, Format/XML-Prüfung, Windows-Build mit Warnungen als Fehler und betroffene native Regressionen | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

Keine.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Start ohne Standortzugriff / AK 1, 3 | Frischer UiTest-Einstieg ohne Standortaktion; keine Anfrage und erfolgreiche manuelle Suche | Abgedeckt |
| Aktueller Standort als Start und Ziel / AK 1 | Je Endpunkt Standortbutton, sichtbare Auswahl, Verbindungssuche, Ergebnisse, Details und Zurück | Abgedeckt |
| Berechtigungs-/Positionsfehler und Wiederholung / AK 1, 4 | Gesteuerte Ablehnung, Entzug nach Erfolg, deaktivierter Dienst, Timeout und fehlende Position; Status, Busy-Ende, Wiederholung, manuelle Suche | Abgedeckt |
| Umgebungsliste und Monitor / AK 2 | Nearby-Aktion, vollständigen Kandidaten wählen, richtigen Monitor samt Abfahrten/Metadaten prüfen, zurück | Abgedeckt |
| Umgebungskarte und beide Auswahlwege / AK 2 | Nearby-Aktion, Karte, tatsächlichen Marker bzw. native Listenalternative wählen; gleicher Stop im Monitor; fehlende Position betrifft nur Marker | Abgedeckt |
| Leere/fehlerhafte Umgebung / AK 2, 4 | Gesteuerter leerer bzw. fehlgeschlagener Providerabruf, verständliche Datenzuordnung und weiter nutzbare manuelle Suche | Abgedeckt |
| Verspätete Position/Umgebung / AK 3 | Langsame Antwort, zwischenzeitliche manuelle Änderung/neue Auswahl/neue Umgebung oder Zurück; kein Überschreiben und keine verspätete Navigation | Abgedeckt |
| Erreichbarkeit/Navigation / AK 1, 2 | Schmale Darstellung, Tastatur und Zurück; erreichbare Buttons und umbrechende Hinweise | Abgedeckt |
| Echte Windows-Integration / AK 1, 4 | Standortbutton im Release mit echtem Adapter, statusbezogene Protokollierung ohne private Position; bei Erfolg reale Nearby-Abfrage | Abgedeckt |
| Native iOS-Integration / AK 1, 4 | Konkrete Geräteprüfanleitung für Nutzer; lokal Plattformcode/Plist prüfen, Geräteergebnis nicht simuliert behaupten | Abgedeckt im ausdrücklich vereinbarten Plattformumfang |

## Fehlende oder unvollständige Planbestandteile

Keine.

## Hinweise

Unabhängige Planprüfung am 17.09.2026 anhand der vollständigen Featureanforderung, Bestandsaufnahme, Planung und Schritt 5 des Projektplans. Zusätzlich tatsächlich gelesen: `EndpointViewModel`, `StopMonitorViewModel`, `MapViewModel`, `StopSearchService`, `SearchPage`, `StopSearchPage`, native UIA-Hilfen und Windows-Vorprüfbericht. Die Bestandsaufnahme verlinkt keine weiteren Inventardetaildateien.

Die im Plan genannten Integrationsrisiken sind am Bestand bestätigt: `OpenAsync` prüft aktuelle vollständige Kandidaten und ruft derzeit `Lookup.SelectAddress` auf; Nearby darf diesen manuellen Übernahmeweg nicht unbeabsichtigt die aktive Menge invalidieren lassen. `ShowStops` liest derzeit ausschließlich `Lookup.Metadata`; die geplante Umstellung auf aktive Metadaten ist notwendig und vorhanden. `StopSearchService` bricht ältere Operationen derselben Instanz ab; der getrennte Nearby-Scope ist folgerichtig. Seitenabgang muss ausstehende Arbeit abbrechen und abgeschlossene Kandidaten erhalten.

Konkrete Frischegrenzen für Zeitstempel und AutomationId-Namen sind routinemäßige Implementierungsentscheidungen. Sie sind im Code konsistent zu definieren und mit den geplanten Validierungs-/E2E-Tests zu prüfen. Diese Planprüfung bestätigt die geplante Abdeckung, keine bereits erfolgte Implementierung oder Testausführung.
