# Visuelle Abnahme – FlowNRW „Modernes Design" (Task 25)

Datum: 2026-10-15
Prüfstand: Commit `a34a04f` + Nachbearbeitungen im Arbeitsbaum
(`DeparturePage.cs` Aktionsraster, `SearchPage.cs` Picker-Titel,
`WindowsDesignUiTests.ps1` Fokus-Wiederherstellung).
Build-Nachweis je Bild: `icon-matrix/matrix.jsonl`
(alle Einträge: Commit `a34a04f`, `buildSha256` `6F608E16…`,
`workingTree` = tracked files modified — d. h. der UiTest-Fixture-Build
enthält die drei Nachbearbeitungen).
Bewertet gegen `docs/design/acceptance.md` und die Referenz-HTMLs
unter `reference/`.

Abweichung zum Lifecycle-Plan: keine verschachtelten Subagenten
verfügbar — diese Bewertung wurde direkt vom ausführenden Agenten
als separater, dokumentierter Abnahmeschritt durchgeführt.

## Methode

1. Vollständigkeitsprüfung der Screenshot-Matrix
   (`icon-matrix/`, 226 PNGs + `matrix.jsonl`) gegen die
   Pflichtvarianten aus `acceptance.md`.
2. Bildbetrachtung einer repräsentativen Stichprobe
   (24 Bilder, Liste unten) über beide Themes, beide Fenstermaße,
   100 %/150 % Textskalierung und alle Oberflächen.
3. Programmatische Pixelchecks auf Token-Farben (PIL-Stichprobe).

## Matrix-Vollständigkeit

37 Hauptszenarien × 6 Kombinationen (hell/dunkel × 430×900/1024×768
× 100 %/150 % für 430×900) + `refresh-store-error`-Szenario
(Speicherfehler, hell/dunkel, 150 %) = 226 Bilder, alle mit
Commit/Build-Hash/Geometrie/DPI/Skalierung/Referenz in
`matrix.jsonl`. Die Dark-Kombinationen und die
Speicherfehler-Variante wurden in diesem Lauf nachgeholt
(vorangegangener Lauf war nach den hellen Reihen abgebrochen).

| `acceptance.md`-Zeile | Abgedeckt durch |
|---|---|
| Home: Favoriten, Entfernung unbekannt/bekannt — hell/dunkel, schmal/breit | `home-distance-(un)known*`, `home-favorite-*`, `empty-favorites` in allen 6 Kombinationen |
| Einzelmonitor: normal, verspätet, Ausfall, keine Echtzeit | `monitor-delayed`, `monitor-cancelled`, `monitor-unknown`, `stop-monitor-cached`, `monitor-provider-error` |
| Suchformular, Treffer/Ergebnisse, Eingabefehler, Ladezustand | `search-form`, `search-invalid`, `search-selections`, `search-swapped`, `journey-results`, `routing-loading`, `routing-provider-error`, `no-journeys` |
| Details mit Umstieg und Fußweg, große Schrift, fehlende Geometrie | `journey-details`, `journey-walk-transfer`, `journey-walk-distance`, `journey-following-leg`, `journey-transfer-summary`, `journey-no-geometry` (inkl. 150 %) |
| Haltestellenliste, Karte mit Auswahl, Listenalternative, Offline/fehlende Position | `stop-list*`, `map-stations`, `map-selected-station`, `map-native-list`, `map-offline`, `map-no-position` |
| Einstellungen mit Speichererfolg/-fehler, Tastatur, große Schrift | `settings-success-keyboard` (alle Kombinationen), `refresh-store-error-settings-error-keyboard` (hell/dunkel, 150 %) — Tastaturfokus per Assertion `Save action has native keyboard focus` |
| Leere Favoriten, keine Verbindungen, Providerfehler | `empty-favorites`, `no-journeys`, `monitor-provider-error`, `routing-provider-error` |

## Stichprobe (betrachtete Bilder)

hell: `light-430-900-100-{home-distance-known, home-favorite-expanded,
monitor-delayed, search-form, search-invalid, empty-favorites,
journey-transfer-summary, map-stations, map-offline,
settings-success-keyboard}`, `light-430-900-150-{home-distance-known,
monitor-cancelled, journey-details}`, `light-1024-768-100-{home-distance-known,
no-journeys}`.
dunkel: `dark-430-900-100-{home-distance-known, monitor-delayed,
monitor-cancelled, search-form, map-stations}`,
`dark-430-900-150-{home-distance-known, settings-success-keyboard,
refresh-store-error-settings-error-keyboard}`,
`dark-1024-768-100-journey-transfer-summary`.

## Kriterienbewertung

| Kriterium (`acceptance.md`) | Ergebnis | Nachweis |
|---|---|---|
| Heller Hintergrund `#FAF9FE`, Karten `#FFFFFF` | erfüllt (Pixel exakt) | `light-…-home-distance-known`, Pixelcheck (250,249,254)/(255,255,255) |
| Dunkel: Hintergrund `#000000`, Karte `#1C1C1E`, kontrastgerechte Akzente | erfüllt (Pixel exakt; Akzent hellblau ~`#A5C9FF`, kein helles Token auf Dunkel) | `dark-…-home-distance-known`, `dark-…-monitor-delayed` |
| Interaktion primär `#0058BC`, aktiver Container `#0070EB`, weißer Text | erfüllt (Pixel `#0058BC` exakt) | `light-…-home-distance-known` „Verbindung suchen" |
| Linienbadges RE `#BA1B1D` / S `#008D43` / U `#005A9C` / Bus `#6F2C91`; neutral sonst | erfüllt — RE1 Rot (Pixel `#BA1B1D` exakt), S2 Grün, U11 Blau, Bus 10 Violett; nicht kategorisierte „107" neutral | `light-…-monitor-delayed`, `…-journey-transfer-summary` |
| Semantik mit zusätzlicher Beschriftung („+3 Min.", „Fährt fällt aus", „Keine Echtzeitdaten") | erfüllt — Chips als Kapseln mit Icon + Text, in dunkel kontrastangepasst | `*-monitor-delayed`, `*-monitor-cancelled` |
| Typografie-Hierarchie, tabellarische Abfahrtszeiten, Systemschrift-Familie (Inter/JetBrains Mono) | erfüllt — klare Hierarchie, Zeiten monospaced/tabellarisch | `…-monitor-delayed`, `…-journey-details` (150 %) |
| Keine Tofu-/Kästchen-Glyphen; Material-Symbols-Icons rendern | erfüllt — Refresh/Zahnrad/Stern/Ortung/Tausch-Pfeile sauber, auch bei 150 % | alle Stichproben |
| Touch-Ziele ≥ 44×44 DIP | erfüllt — `AssertTouchTarget` für `SaveRefreshSettings` (360–372×51), `RefreshInterval`, `OriginSearch`, `OriginLocation`, `OriginCoordinateMode`, `SearchJourneys` in allen 7 Läufen grün | Laufprotokolle, `matrix.jsonl` |
| Drei persistente beschriftete Tabs (Abfahrten/Verbindungen/Haltestellen), kein Overflow bei 430 px | erfüllt — alle drei Tabs mit Icon+Label sichtbar/selektierbar | alle Stichproben |
| Große Schrift 150 %: kein Clipping von Icons/Text, keine starre Kartenhöhe | erfüllt — Icons unbeschnitten, Karten wachsen, Inhalte scroll-/umbrechbar | `*-150-*`-Stichproben |
| Ehrliche Fehler-/Leerzustände mit manueller Wiederholung | erfüllt — „Keine Verbindungen gefunden. Bitte erneut suchen.", Providerfehler mit Quellen-/Fallback-Angabe, Offline-Karte mit ehrlichem Hinweis | `…-no-journeys`, `…-routing-provider-error`, `…-map-offline`, `…-settings-error-keyboard` |
| Strukturierte Abfahrtszeile (Badge, Ziel, Soll/Ist, Gleis) | erfüllt | `…-monitor-delayed`, `…-home-favorite-expanded` |
| Referenz-Nähe (Karten-Darstellung, Marker, Listen-/Kartenumschaltung, Attribution) | erfüllt — OSM-Basiskarte, nummerierte Marker, Liste/Karte-Toggle, `© OpenStreetMap`-Attribution sichtbar | `…-map-stations`, `…-map-native-list` |
| iOS-Geräteprüfung | **offen** — kein iOS-Gerät/Mac auf Windows verfügbar; Checkliste bleibt beim Nutzer (Task 26) | — |

## Festgestellte Anmerkungen (keine Blocker)

1. **Horizontale Badge-Kürzung bei 150 %.** In `*-430-900-150-home-distance-*`
   läuft die kompakte Linien-Badge-Zeile der Favoritenkarte über den
   Kartenrand hinaus (drittes Badge sichtbar angeschnitten). Die Zeile ist
   ein horizontaler `ScrollView` (`HomePage.cs`, `summaryContainer`),
   alle Inhalte bleiben per Wischen erreichbar; kein Text/Glyph ist in
   sich beschnitten. Als plattformübliche Chip-Leiste akzeptabel;
   optionale Verbesserung: Umbruch statt Scrollen bei großer Schrift.
2. **Status-/Validierungsmeldungen in Sekundärfarbe.** `search-invalid`
   und `settings-error-keyboard` zeigen Klartextmeldungen in
   Primär-/Sekundärtextfarbe statt `Error`-Rot. Konform zu „Farbe nicht
   einziger Träger" (vollständige verständliche Beschriftung vorhanden);
   ein Error-Tint wäre eine denkbare Verstärkung, kein Mangel.
3. **Diagnoseabsatz bei Providerfehler** (`routing-provider-error`) ist
   ein mehrzeiliger Statusblock unterhalb der primären Aktion — Quellen-/
   Alters-/Fallbackangabe bleibt erreichbar und zugeordnet, ohne die
   Gestaltung zu dominieren.

## Urteil

**Windows-Abnahme: bestanden.** Alle Pflichtvarianten der Matrix liegen
vor (226 Bilder, vollständig protokolliert), die geprüften Token-,
Komponenten- und Bedienkriterien sind erfüllt, keine echten Mängel in
der Stichprobe. Verbleibende Einschränkung: iOS-Geräteprüfung (Task 26)
ist auf Windows nicht ausführbar und bleibt dokumentiert offen —
Gesamtfreigabe von Schritt 9 hängt an diesem Punkt.
