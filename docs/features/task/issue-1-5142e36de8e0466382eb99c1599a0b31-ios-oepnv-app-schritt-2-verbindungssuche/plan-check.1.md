# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan lückenhaft

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK 1: Vorlage entfernen, Plattformbasis, Navigation und Visual Studio | Schritte 3, 4 und 7; DI-Shell und konditionierte Plattformtargets | `Start`, Windows-Build und manuelle iOS-Prüfanleitung | Abgedeckt |
| AK 2: Eingabearten, Identität, Zustände und veraltete Anfragen | Endpunkt-/Routingrevisionen, getrennte Services, Parser und vollständige Address-Auswahl | Eingabe-E2E und deterministische Tests vorhanden; verspätete Routingantwort nur durch `IgnoresOldRoute` auf ViewModel-Ebene erfasst | Lücke |
| AK 3: Geordnete Ergebnisse, Zeit-/Statusdarstellung und Navigation | Sitzungszustand, Präsentationshelfer, Ergebnis-/Detailseiten | `ResultsAndDetails`, `BackNavigation`, `ProviderStatus` und Präsentationstests | Abgedeckt |
| AK 4: Reale Services, NRW-Priorität, sichere Konsolidierung und Live-Proben | Bestehender Orchestrator bleibt erhalten; Metadaten werden durchgereicht | Bestehende Transitfälle einschließlich Region/Identität/Union/Fallback werden abgeglichen und ausgeführt; `LiveSmoke` getrennt von Fixtures | Abgedeckt |
| AK 5: Native Windows-Flüsse, deterministische Tests, Dokumentation und Checks | Schritte 5 bis 7 mit isoliertem Fixture-Build und konkretem UIA-Harness | Fast alle Flüsse explizit, jedoch kein nativer Test der verdrängten Routingantwort | Lücke |
| AK 6: Geprüfte Windows-ZIP-Bereitstellung über IIS nach Abnahme | Schritt 7 nennt Abnahme, Site/Zielpfad, Download und lokalen Start sowie Unterlagen | Prüfhandlungen im Umsetzungsschritt vorhanden, aber kein konkreter Testnachweis im Abschnitt Tests/E2E-Tests | Lücke |

## Fehlende oder unvollständige Testanforderungen

- [ ] AK 2/5: Nativen E2E-Fall für eine verspätete **Routingantwort** ergänzen. Setup: erste Route im Fixture-Service verzögern und Abbruch ignorieren lassen. Aktion: während des Abrufs Endpunkt ändern, neu auswählen und eine zweite Route starten. Sichtbares Ergebnis: nur die zweite Route samt Metadaten bleibt sichtbar; die späte erste Antwort löst weder Ergebnisüberschreibung noch unerwartete Navigation aus. `LatestInputWins` beschreibt bisher verzögerte Trefferauflösung, `IgnoresOldRoute` ist ausschließlich ein ViewModel-Test.
- [ ] AK 6: Den bereits in Schritt 7 vorgesehenen Bereitstellungsnachweis als konkreten Test unter Tests/E2E-Tests aufnehmen: nach fachlicher Abnahme und geprüfter Site-/Pfadzuordnung ZIP über die IIS-Downloadadresse beziehen, in getrenntes Verzeichnis entpacken, die enthaltene App gemäß Startanleitung starten und Suchseite prüfen; Commit/Version und mitgelieferte Umfang-/Grenzendokumentation gegen den freigegebenen Stand prüfen. Datum, URL/Zielpfad, Ergebnis und Startnachweis festhalten.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Einstieg, keine Vorlage/Standortabfrage, begrenzte Navigation / AK 1, 2 | `Start` | Abgedeckt |
| Adresse in beiden Feldern und mehrdeutige Auswahl / AK 2, 5 | `AddressEndpoints` | Abgedeckt |
| Haltestelle in beiden Feldern und Auswahl / AK 2, 5 | `StopEndpoints` | Abgedeckt |
| Koordinaten in beiden Feldern / AK 2, 5 | `CoordinateEndpoints` | Abgedeckt |
| Ungültige Eingabe, gesperrte Suche und Korrektur / AK 2, 5 | `Validation` | Abgedeckt |
| Späte Trefferantwort, Auswahlverlust und parallele Endpunktsuche / AK 2, 5 | `LatestInputWins` | Abgedeckt |
| Späte Routingantwort nach Endpunktänderung und neuer Suche / AK 2, 5 | Bisher nur `IgnoresOldRoute` auf ViewModel-Ebene | Lücke |
| Laden, leere Treffer/Verbindungen, Fehler und Erholung / AK 2, 3, 5 | `EmptyAndErrorRecovery` | Abgedeckt |
| Ergebnisreihenfolge, Detailinhalt und unbekannte Werte / AK 3, 5 | `ResultsAndDetails` | Abgedeckt |
| Detail → Ergebnisse → Suche und anschließende Änderung / AK 2, 3, 5 | `BackNavigation` | Abgedeckt |
| Warnung, Fallback, Quelle, Datenalter und Stale / AK 3, 4, 5 | `ProviderStatus` | Abgedeckt |
| Bundesweite und NRW-Suche über reale DI / AK 4, 5 | `LiveSmoke` oder protokollierte native Bedienung | Abgedeckt |
| Download und Start des ausgelieferten Windows-Pakets / AK 6 | In Schritt 7 vorgesehen, konkreter Test fehlt | Lücke |

## Fehlende oder unvollständige Planbestandteile

Keine zusätzlichen Umsetzungslücken; die offenen Punkte betreffen die Konkretisierung der Testplanung.

## Hinweise

Geprüft wurden requirement.md, inventory.md einschließlich aller vier Detaildokumente, plan.md und der native UIA-Preflight. Der erfolgreiche Preflight belegt die technische Einstiegsmöglichkeit; die neuen Controls sind danach tatsächlich zu bedienen. Windows-CI bleibt erhalten. Die native iOS-Abnahme durch den Nutzer ist die vereinbarte Prüfaufteilung und keine Planlücke. Die IIS-Bereitstellung bleibt ausdrücklich an die fachliche Abnahme gebunden. Nicht-Anforderungen und Datenschutzgrenzen sind berücksichtigt.
