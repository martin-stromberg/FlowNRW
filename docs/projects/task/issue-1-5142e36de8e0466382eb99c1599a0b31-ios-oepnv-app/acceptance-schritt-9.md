# Abnahmeprüfung – Entwicklungsschritt 9

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Prüfmodus und Stand

07.10.2026, Abnahmerunde 1. Fachliche Gegenprüfung von Projektplan Schritt 9 gegen Basisbranch...`782b5a2`, Produktquellen, native Testbelege, Geräteergebnisse und Dokumentation. Endstand-Schritt-Branch `f22e50a` (Docs/Lizenz) auf Produktcode `2eafa7f`.

Wie bei Schritt 8 stand kein unabhängiger Prüfagent zur Verfügung; diese Abnahme wurde als lokale Prüfphase im dokumentierten Skillfallback durchgeführt und ist ausdrücklich keine unabhängige Agentenabnahme.

## Abgleich

| Kriterium | Tatsächliche Umsetzung und Prüfung |
|---|---|
| 1: Farben, Typografie, Karten | Designressourcen (Farben, Typografie, Abstände, Radien, Linienbadges, Soll-/Ist-/Ausfallzustände) entsprechen `docs/design/acceptance.md` und sind in allen vier Matrizen hell/dunkel konsistent; strukturierte Abfahrts-/Verbindungskarten ersetzen die Textform. Keine Akzentfarbenänderung. Belege: `test-results-visual-final.md`, `review-visual-final.md`. |
| 2: Bereiche und Bildhierarchie | Drei persistente beschriftete Kernbereiche Abfahrten, Verbindungen, Haltestellen; Details als Unterseite, Einstellungen sekundär. Drilldown/Zurück erhalten Eingaben und Auswahl — nativ belegt durch die journey-Regressionen. Kein funktionsloser Entwurfs-Tab; alle bestehenden GPS-/Favoriten-/Intervall-/Kartenfunktionen erreichbar. |
| 3: Barrierearmut | Kontrast-, Touchziel- und Textskalierungsnachweis über die dunkle 150-%-Matrix und die dokumentierte Prüfung; Zustände sind textuell gekennzeichnet, nicht allein farblich. Karten ohne Position/Echtzeit zeigen verständliche Hinweise statt erfundener Daten. |
| 4: Screenshotmatrix | Vier Varianten (430×900 hell, 430×900 dunkel 150 %, 1024×768 hell, 1024×768 dunkel), je 37 PNGs, mit Manifestzeilen auf sauberem Commit `782b5a26` und Build-SHA256 `3FFA3809…`; Cache-Refresh-Bildpaar auf demselben Stand. Visueller Prüfbefund dokumentiert; offene Punkte (synthetische Kacheln, Fixture-Szenarioname im Fehlerbild) benannt. Keine sensiblen Standortbilder oder Steuerungen in finalen Bildern. |
| 5: Regressionsflüsse und Qualität | Alle 12 Nacht-Runner-Items auf dem Endstand grün: journey-default/-monitors/-favorites/-maps/-locations, departure-cache, refresh, lifecycle sowie vier Matrizen. Windows-UiTest-Build 0 Warnungen/0 Fehler, 280 Coretests bestanden; Coverage-Grenze aus früheren Schritten unverändert eingehalten. iOS-Geräteabnahme wurde vom Nutzer tatsächlich durchgeführt und als abgenommen bewertet (`ios-device-acceptance.md`, Ergebnisprotokoll). |
| 6: Dokumentation | `docs/help/design`, `ios-device-acceptance.md`, `test-results-*` und Release Notes dokumentieren Tokens, Nachweise und Plattformgrenzen. Diagnoseprotokoll für Geräteferndiagnose ergänzt. |

## Abweichungen

Keine fachlichen Abweichungen im vereinbarten Schritt-9-Umfang offen.

## Hinweise

- **Externe Einschränkung, kein Schritt-9-Abweichung:** `v6.db.transport.rest` ist upstream derzeit vollständig nicht erreichbar (503 auf allen Endpunkten; DB-HAFAS-Abschaltung und laufende db-vendo-Migration, public-transport/transport.rest#34/#35). NRW-Abfahrten/-Verbindungen werden über die EFA deckend gezeigt; ein Per-Host-Circuit-Breaker begrenzt die Wartezeit auf den toten Zweig. Die Basiskonfiguration `TransitProviders:DbRest:BaseUrl` erlaubt eine alternative Instanz.
- Die iOS-Geräteabnahme umfasste ein Gerät im Hochformat; Dunkelmodus-, Querformat- und Dynamic-Type-Prüfung auf echter Hardware wurden nicht separat abgehakt, jedoch wurde die Gesamtabnahme vom Nutzer als abgenommen bewertet.
- Sporadische Abfahrtsabruf-Fehler wurden über das neu eingeführte Diagnoseprotokoll auf die externe db.rest-Störung zurückgeführt; die Anzeige degradiert korrekt.
- Zusätzlich zum Entwurfsumfang wurde auf Gerätebefund ein Diagnoseprotokoll (opt-in, „Protokoll senden") eingeführt; daraus folgt keine visuelle Anforderungsabweichung.

Freigabe zur lokalen Integration von Schritt 9 auf dem Projektbasisbranch unter dokumentiertem Skillfallback. Gesamtabschluss des Projekts nach dieser Integration möglich.
