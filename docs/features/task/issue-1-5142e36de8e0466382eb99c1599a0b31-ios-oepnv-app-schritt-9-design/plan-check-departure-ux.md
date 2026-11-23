# Planprüfung – Verdichtete mobile Abfahrtsanzeige

**Stand:** 04.10.2026

**Status:** Plan vollständig

Die überarbeitete [Planung](plan-departure-ux.md) wurde erneut vollständig gegen die [Anforderung](requirement-departure-ux.md), die [Bestandsaufnahme](inventory-departure-ux.md) und die zuvor dokumentierten Lücken geprüft. Alle vier Planlücken sind geschlossen. Es wurden keine Codeänderungen vorgenommen.

## Prüfung der bisherigen Lücken

| Punkt | Ergänzung im Plan | Ergebnis |
| --- | --- | --- |
| L1: Vollständigkeit einer Antwort | Paket 5 definiert eine gemeinsame konservative Regel für Fehler, Warnungen, Fallback, Veraltung und Kürzung. Die drei bestehenden Abschneidepfade, strukturell gültige leere Antworten und reale Mapping-/Orchestratorfälle sind ausdrücklich berücksichtigt. | Erledigt |
| L2: Unabhängige Lebensdauer der Linien | Paket 5 trennt die Inventarwiederherstellung vom Erfolg der Abfahrtswiederherstellung und korrigiert den bisherigen Löschpfad. Vergangene/zeitlose Ereignisse dürfen keine Linien löschen. Zweimaliger Neustart und Fehlerbeibehaltung werden geprüft. | Erledigt |
| L3: Ladeanzeige im Detailmonitor | Paket 3 umfasst nun ausdrücklich den Kopf von `DeparturePage`, den entfallenden Aktualisierungstext und den zugänglichen Indikator. Erfolg, Fehler und Abbruch sind für beide Ansichten eingeplant. | Erledigt |
| L4: Verpflichtende E2E-Nachweise | Paket 7 ordnet alle geforderten Benutzerflüsse konkreten Windows-Skripten zu. Fehlende Fixture-Steuerung ist zu ergänzen; fehlende oder nicht ausgeführte Fälle bleiben offene Abnahme. | Erledigt |

## Vollständigkeit nach Anforderung

- **R1 – Standort:** Die erfolgreiche Position aus der Umkreissuche erreicht künftig auch die Distanzberechnung der Favoriten. Getrennte Fälle für fehlende Berechtigung, deaktivierten Standortdienst, erfolglose Positionsbestimmung und fehlende Haltestellenkoordinaten sind mit sichtbaren, zutreffenden Hinweisen geplant.
- **R2 – Darstellung:** Beide Kartendarstellungen erhalten die einmalige primäre Zeit und nur bei Abweichung die kleinere durchgestrichene Soll-Zeit. Entfernte Vergleichs-, Abweichungs-, unbekannte Verspätungs- und Quellentexte sowie die Gleisregeln werden ausdrücklich geprüft. Die Ladeanzeige ist auf Start- und Detailansicht berücksichtigt.
- **R3 – Detailübernahme:** Bereits vorhandene gültige Daten werden vor Beginn des neuen Abrufs übergeben. Erfolg, Fehler, Abbruch und vergangene Ereignisse haben definierte Ergebnisse und eigene UI-Nachweise.
- **R4 – Favoriten:** Standardmäßig eingeklappte Karten, Touchbedienung, Linien mit/ohne nächste Zeit, persistenter Neustart, Ersetzung und Entfernung nach vollständigem Erfolg sowie Bewahrung bei Teilantworten sind vollständig eingeplant. Haltestellenschlüssel und Favoritenlöschung bleiben Bestandteil der Datenintegrität.

## Migration und Persistenz

Die additive Cache-Erweiterung berücksichtigt alte Dateien ohne Inventarfeld sowie fehlende/null-Werte. Gültige alte Abfahrten werden nicht wegen der Schemaerweiterung gelöscht. Das Linieninventar bleibt erhalten, wenn alle gespeicherten Zeiten verstrichen sind. Atomare Schreibvorgänge, Größenbegrenzungen, Identitätsprüfung und Bereinigung verwaister Favoriten sind berücksichtigt. Für neue Linienwerte werden Anzahl und Länge begrenzt.

Die konservative Vollständigkeitsregel ist eine bewusste technische Entscheidung: Auch brauchbare Antworten mit Warnungen dürfen das letzte vollständige Inventar nicht ersetzen. In der Umsetzung ist diese Regel gemeinsam für Start- und Detailaktualisierung anzuwenden. Die vorgesehenen Tests der echten Mapping-/Orchestratorpfade müssen bestätigen, dass strukturell ungültige oder gekürzte Antworten niemals als vollständiger leerer Erfolg behandelt werden.

## Positionserklärung

Die Bestandsaufnahme belegt einen Fehler in der bestehenden Datenweitergabe: Die automatische Umkreissuche erhält eine Position, setzt aber nicht den Positionszustand für Favoritenentfernungen. Aktiviertes GPS allein ist daher im aktuellen Code nicht ausreichend. Paket 1 korrigiert diesen Ablauf ohne zusätzliche Standortübertragung oder dauerhafte Speicherung.

Die tatsächliche App-Berechtigung und die Haltestellenkoordinaten auf dem Smartphone sind aus dem Quellcode nicht beweisbar. Der Plan behandelt diese Voraussetzungen getrennt und behauptet keinen beleglosen GPS-Fehler.

## Verbindlicher Testbedarf

Die Core-Tests decken Migration, Persistenz, Identitätszuordnung, Antwortvollständigkeit, Entfernungsberechnung und Datenbeibehaltung ab. Für sichtbare Benutzerflüsse sind zusätzlich sämtliche in Paket 7 benannten Szenarien in `WindowsDepartureCacheUiTests.ps1`, `WindowsJourneyUiTests.ps1`, `WindowsRefreshUiTests.ps1`, `WindowsLifecycleUiTests.ps1` und `WindowsDesignUiTests.ps1` erfolgreich auszuführen.

Die visuelle Prüfung umfasst schmale Fenster, Light/Dark und vergrößerten Text sowie Ist-/Soll-Hierarchie, Durchstreichung, Gleisänderung, Linienanordnung und Symbolposition. Die Smartphoneprüfung bleibt entsprechend der Nutzerentscheidung beim Anwender. Ein erfolgreicher Build oder Core-Testlauf ersetzt keine fehlenden Windows-E2E-Nachweise.

## Offene Planpunkte

Keine. Der Plan kann umgesetzt werden. Diese Freigabe bewertet die Planung; sie nimmt weder Implementierung noch Tests vorweg.
