# Ergebnis: aktuelle Windows-Bildprüfung

## Aktueller Ergänzungsnachweis: heller Breitlauf

Unabhängig geprüft am 04.10.2026: `artifacts/step9-visual-final/wide-full-light/` enthält 23 PNGs und 23 Manifestzeilen für hell, 1024×768, DPI 96, 100 % Textskalierung. Die Bilder sind dem Basiscommit `81cdd69ae83f58133becfcd077deebaf0a1c36dc` und dem Build-SHA256 `D5523269D832EACCB89124BB7C49DD21DAC1364A7F762364EDBE98688BFB9B8B` zugeordnet. Die nachfolgende ältere Beschreibung des Vordergrundabbruchs betrifft einen früheren Versuch und beschreibt diesen neueren Lauf nicht.

| Bereich | Tatsächlicher Nachweis im neueren Lauf | Grenze |
|---|---|---|
| Startseite/Monitor | Bilder für Karten ein-/aufgeklappt, Entfernungen, Ist-/Sollzeit, fehlende Echtzeit und Ausfall; normaler Monitor ohne wiederkehrenden Erfolgs-/Intervalltext | Dateinamen „distance-unknown“ allein belegen den Zustand nicht, da Standortermittlung vorher abgeschlossen sein kann |
| Haltestellensuche/Rückkehr | `stop-list` und `stop-list-returned` zeigen beide Treffer vor/nach Monitoraufruf | Suchtext wird zum gewählten Stationsnamen; kein Prozessneustart |
| Haltestellenmonitor | `stop-monitor-cached` zeigt vorhandene Abfahrten | Runner wartet auf `Departure0`; kein belegter verzögerter Wiederholungsaufruf und damit kein visueller Cache-Frühanzeigenachweis |
| Kartenpfade | Stationen, native Liste, Monitor nach Stationswahl, Offlinekarte und Karte ohne Position aufgenommen | Synthetische Kacheln; kein echter Kartenanbieter-Nachweis |
| Monitorfehler | Verständlicher Fehler und erhaltene letzte Abfahrten sichtbar | Kein leerer Fehler-Erstaufruf in diesem Bild |
| Touch/Fokus | Log bestätigt Speichern 792×44, Intervallauswahl 766×70, Startsuche/Standort jeweils 48×48, Koordinatenschalter 62×48; nativer Tastaturfokus auf Speichern | Kein aktueller PASS für `SearchJourneys` oder die neuen Verbindungsaktionen in diesem Log |

`run.log` endet nach `PASS Touch target OriginCoordinateMode >=44x44 (62 x 48)`. Es enthält zuvor `CAPTURE map-offline`, `CAPTURE map-no-position`, `CAPTURE monitor-provider-error` und `CAPTURE search-form`. Deshalb wird dieser Lauf als Teilnachweis bis zum Suchformular geführt, ohne vollständigen Runner-PASS oder Verbindungsabnahme zu behaupten.

Die unabhängige Sichtprüfung öffnete zwölf ausgewählte Bilder dieser Gruppe und die intakten hellen Abfahrts-/Kartenreferenzen. Ergebnis und genaue Auswahl stehen in [review-visual-final.md](review-visual-final.md). Konkrete Restbefunde sind technische Identitäten/Koordinaten in Such- und Kartenlisten sowie überlange Quellen- und wiederkehrende Erfolgstexte in der Haltestellensuche. Die weiteren Matrixvarianten, ein belegter Cache während Refresh, aktuelle Verbindungsbilder und die nutzerseitige iOS-Abnahme bleiben offen. Es wurden für diese Dokumentationsprüfung keine Builds oder Tests neu ausgeführt.

Stand: 04.10.2026. Die Prüfung nutzte `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1` mit dem vorhandenen nativen `UiTest`-Build. Kein Produktcode wurde für diese Läufe verändert. Buildzuordnung: Commit `6eb05c25611506f176c6250812fe70227bbd3d2e`, SHA-256 der `FlowNRW.dll` `832B4C162E49BFA959AB9A7EC7CDDF283BA0881C3CDF7581A1ED5B91E0F38603`, Windows native MAUI, DPI 96. Die Matrixdateien geben Fenstermaße, Thema, Textskalierung und Szenario an. Der Runner hardcodiert ein veraltetes `workingTree`-Feld; die erzeugten JSONL-Manifeste wurden nach Prüfung von `git status` auf `tracked files clean; untracked files present` berichtigt.

## Läufe und Artefakte

| Konfiguration | Ergebnis | Artefaktordner |
|---|---|---|
| Hell, 430×900, 100 %, Standardszenario `success` | Fixture-Steuerung verborgen, Leere Favoritenansicht und Einstellungsansicht erfasst; Touchziele und Tastaturfokus erfolgreich. Danach Abbruch bei `MonitorStatus expected manuell aktualisiert but was Abfahrten konnten nicht geladen werden. Bitte erneut versuchen.` | [light-narrow](../../../../artifacts/step9-visual-final/light-narrow) |
| Hell, 430×900, 100 %, `cache-lines-bc` | Leere Ansicht, Einstellungen und Monitorbild erfasst; Touchziele/Fokus erfolgreich. Danach erwartete der alte Runner `Departure3`, das vollständige `B`/`C`-Linien-Fixture hat jedoch nur zwei Abfahrten. Die Diagnoseaufnahme heißt `failure-missing-Departure3.png`. | [light-narrow-cachelines](../../../../artifacts/step9-visual-final/light-narrow-cachelines) |
| Hell, 430×900, 100 %, `favorite-cache-seed` | `-HomeOnly` vollständig erfolgreich: Leere Ansicht, Einstellungen, Monitor normal/verspätet/unbekannt/ausgefallen, drei Favoriten, bekannte und unbekannte Entfernung, aufgeklappte und eingeklappte Karte. | [light-narrow-valid](../../../../artifacts/step9-visual-final/light-narrow-valid) |
| Dunkel, 430×900, 100 %, `favorite-cache-seed` | Derselbe `-HomeOnly`-Lauf erfolgreich. | [dark-narrow](../../../../artifacts/step9-visual-final/dark-narrow) |
| Dunkel, 430×900, 150 %, `favorite-cache-seed` | Derselbe `-HomeOnly`-Lauf erfolgreich; erforderliche Touchziele und Tastaturfokus bestanden. | [dark-large](../../../../artifacts/step9-visual-final/dark-large) |
| Hell, 1024×768, 100 %, `favorite-cache-seed` | Derselbe `-HomeOnly`-Lauf erfolgreich. | [light-wide](../../../../artifacts/step9-visual-final/light-wide) |

Die erfolgreich abgeschlossenen vier `favorite-cache-seed`-Läufe enthalten jeweils 12 PNGs und ein `matrix.jsonl` (48 PNGs). Die zwei abgebrochenen Läufe enthalten vier weitere PNGs plus eine nicht ins Manifest aufgenommene Fehlerdiagnose. Insgesamt liegen 53 manifestierte Aufnahmen und eine Diagnoseaufnahme unter `artifacts/step9-visual-final/`. Das Standardszenario liefert synthetisch veraltete/teilweise Resultate (`IsFallback`, `IsStale`, Warnung); der Monitor weist sie im aktuellen Code korrekt als unvollständig zurück. Das `cache-lines-bc`-Szenario liefert absichtlich genau zwei Linien/Abfahrten, während der ältere Screenshotablauf weiter vier (`Departure3`) voraussetzt. Beides sind Fixture-/Runnerinkonsistenzen, keine bestandenen Zustandsbilder.

## Sichtprüfung der Bilder

Die Aufnahmen `home-favorite-collapsed` und `home-favorite-expanded` wurden im hellen und dunklen 430×900-Lauf visuell betrachtet; dazu `dark-large` bei 150 % sowie `light-wide` bei 1024×768.

- Eingeklappte Karten zeigen farbige Linienbadges mit nächster Uhrzeit und vier erreichbare Kartenaktionen. Mehrere Stationen und Entfernungen sind in einer nachvollziehbaren Scrollansicht sichtbar. Die helle breite Ansicht hält die Favoritenkarten kompakt.
- Aufgeklappt sind Abfahrtszeit und kleine durchgestrichene Soll-Zeit sichtbar. Das unveränderte Gleis hat keine zusätzliche „geplant“-Angabe; beim Gleiswechsel wird der geplante Wert genannt. Ein Ausfall ist textlich markiert. Die Linienübersicht verschwindet im aufgeklappten Zustand.
- Bei 150 % wird der Zieltext „Essen Hauptbahnhof“ im Abfahrtskopf sehr schmal und in mehrere kurze Zeilen zerlegt. Das Linienbadge „RE 1 · Stand 2“ bricht ebenfalls ungünstig um. Das ist ein konkreter offener Layoutmangel dieser Momentaufnahme und sollte mit einer breiteren Badge-Spalte bzw. flexiblerem Kopf korrigiert und erneut geprüft werden.
- Die Aufnahmen belegen nicht, dass Linien ohne Abfahrtszeit nach einem App-Neustart aus dauerhaft gespeicherten Daten erscheinen: der erfolgreiche `favorite-cache-seed`-Lauf fragt gültige Fixturedaten ab, statt einen Prozessneustart mit vorgefülltem persistentem Cache zu zeigen.

Beispielbilder: [helle eingeklappte Favoriten](../../../../artifacts/step9-visual-final/light-narrow-valid/light-430-900-100-favorite-cache-seed-home-favorite-collapsed.png), [helle aufgeklappte Abfahrten](../../../../artifacts/step9-visual-final/light-narrow-valid/light-430-900-100-favorite-cache-seed-home-favorite-expanded.png), [150-%-Detailansicht](../../../../artifacts/step9-visual-final/dark-large/dark-430-900-150-favorite-cache-seed-home-favorite-expanded.png), [breite helle Ansicht](../../../../artifacts/step9-visual-final/light-wide/light-1024-768-100-favorite-cache-seed-home-favorite-collapsed.png).

## Erforderliche Runnerergänzungen für die nächste Matrix

1. Das `success`-Szenario für Designbilder muss gültige vollständige Abfahrtsdaten liefern. Alternativ muss der Designrunner bei diesen Fällen explizit `favorite-cache-seed` verwenden.
2. Der Runner darf die Ankunft bei `Departure3` nicht als allgemeine Vorbedingung für ein Zweilinien-Fixture verwenden. Zustandsbilder sollen gezielt zu den für das jeweilige Szenario vorhandenen AutomationIds scrollen. `cache-lines-bc` braucht eigene Assertions für die Chips `B` und `C`, den leeren/nicht vorhandenen Zeitpunkt und die Abwesenheit weiterer Linien.
3. Für persistierte Linien ist ein echter Prozessneustart nötig: Cachedatei vor dem Start kontrolliert mit vollständiger Linienmenge und ohne zukünftige Abfahrten anlegen, Startseite erfassen, danach aktuelle Anfrage und Entfernung davon getrennt nachweisen. Keine `Lines` aus der gerade laufenden Anbieterantwort als Neustartnachweis verwenden.
4. Ergänzende native Flüsse und Screenshots fehlen: Monitor direkt mit gecachten Daten während verzögertem Refresh und erneutes Öffnen nach Haltestellensuche; Suche → Trefferliste → Monitor → Zurück mit erhaltenen Treffern; Verbindungsfavorit im Kopf, gespeicherte Verbindung auswählen, Endpunkte tauschen; verdichtete Verbindungsdetails. `-HomeOnly` beendet vor Haltestellen- und Verbindungsseiten und kann diese Bilder nicht erzeugen.
5. Die JSONL-Erzeugung in `WindowsDesignUiTests.ps1` muss `workingTree='uncommitted step 9'` durch echte Statusmetadaten ersetzen, damit künftige Manifestdaten dem aufgenommenen Build entsprechen.

Die Screenshots und Logs sind native Windows-Bilder mit synthetischen Daten. Es wurden keine Kontrastquotienten gemessen und keine vollständigen UIA-Bounds der neuen Verbindungsaktionen, Cache-/Rückkehrpfade oder 44×44-Ziele erhoben. Eine unabhängige Bildbewertung für diesen Stand fehlt. Es wurde keine iOS-Geräteprüfung durchgeführt; Safe Areas, Dynamic Type, Orientierung, Dunkelmodus und VoiceOver bleiben offen.

## Status für Schritt 9

Die aktuelle Startseitenmatrix ist teilweise visuell belegt und zeigt einen reproduzierbaren Umbruchmangel bei großer Schrift. Einzelmonitorbilder belegen die geänderte Abfahrtsdarstellung auf Windows. Die verlangten Cache-/Rückkehrzustände und die neuen Verbindungsfavoriten-/Tauschansichten wurden nicht aufgenommen. Wegen dieser Lücken und der noch offenen Referenzprüfung, Kontrast-/Boundsmessung und iOS-Geräteabnahme ist Schritt 9 weiterhin nicht visuell freigegeben.

## Nacharbeit: gezielte visuelle Korrekturen

Der Abfahrtskopf wurde so umgebaut, dass Linienbadge und Ist-/Sollzeit in der ersten Zeile stehen und das Ziel darunter die gesamte Kartenbreite erhält. Das verhindert bei 150 % Textskalierung die vorherigen schmalen Umbrüche von Ziel und Linienbadge. Linienbadges nutzen jetzt eine einzeilige, bei Bedarf abgeschnittene Beschriftung statt Wortumbruch.

Der Windows-Designrunner wartet bei regulären Fixture-Abfahrten nicht länger auf den fachlich überholten Text `manuell aktualisiert`, sondern auf eine gerenderte Abfahrt. Die Aufnahme `monitor-unknown-cancelled` verlangt keine nicht für jedes Fixture vorhandene vierte Abfahrt mehr. Das Matrixmanifest ermittelt seinen Arbeitsbaumstatus aus `git status --porcelain`, statt dauerhaft `uncommitted step 9` einzutragen.

Für den nächsten nativen Lauf ergänzt der Runner gezielte Aufnahmen für Haltestellensuche → Monitor → Rückkehr zur Trefferliste, Start/Ziel-Tausch und das Speichern einer Verbindung. Ein echter Neustart mit zwischengespeicherten Linien bleibt im separaten `WindowsDepartureCacheUiTests.ps1` nachgewiesen; die Designmatrix erzeugt dafür noch kein eigenes Bild.

Gezielte technische Prüfung nach der Nacharbeit: `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore` bestand mit 278/278 Tests. `dotnet build FlowNRW/FlowNRW.csproj -c UiTest -p:TreatWarningsAsErrors=true --no-restore` bestand mit 0 Warnungen und 0 Fehlern. Der PowerShell-Parser bestätigte `WindowsDesignUiTests.ps1`.

Der gezielte native Lauf `dark`, 430×900, 150 %, `favorite-cache-seed`, `-HomeOnly` lief danach vollständig durch. Er erzeugte 13 Bilder unter [dark-large-fixed](../../../../artifacts/step9-visual-final/dark-large-fixed), einschließlich [aufgeklappter Favoritenkarte](../../../../artifacts/step9-visual-final/dark-large-fixed/dark-430-900-150-favorite-cache-seed-home-favorite-expanded.png), [Fahrt ohne Echtzeitangabe](../../../../artifacts/step9-visual-final/dark-large-fixed/dark-430-900-150-favorite-cache-seed-monitor-unknown.png) und [Ausfall](../../../../artifacts/step9-visual-final/dark-large-fixed/dark-430-900-150-favorite-cache-seed-monitor-cancelled.png). Die neue Kopfzeile zeigt Ziel und Badge ohne die zuvor festgestellten schmalen Umbrüche. Der Monitorlauf verwendet die vollständige Vier-Abfahrten-Fixture und scrollt gezielt zu `Departure2` beziehungsweise `Departure3`; daraus folgt keine Aussage für das bewusst zweizeilige `cache-lines-bc`-Fixture.

## Windows-Breitmatrix und Kontrastnachweis, 04.10.2026

Ein neuer vollständiger nativer Lauf wurde für **hell, 1024×768, 100 %, `favorite-cache-seed`** mit der aktuellen `WindowsDesignUiTests.ps1` angesetzt. Das vollständige Fixture ist erforderlich, weil das Default-Fixture absichtlich unvollständige/alte Abfahrten liefert. Der Prozess fand das Fenster und prüfte erfolgreich, dass die Fixture-Steuerung verborgen ist. Vor der ersten Aufnahme brach der Runner bei seiner absichtlichen Schutzprüfung ab: `Physical input cancelled: app is not the foreground window.` Es wurden dadurch keine Eingaben, keine Aufnahmen und keine Matrixeinträge erzeugt. Der Fehler liegt in der nicht interaktiven Ausführungsumgebung dieses Laufs; er ist weder als bestandene noch als fehlgeschlagene Produktprüfung zu werten. Die dunkle 1024×768-Variante wurde aus demselben Grund nicht künstlich wiederholt.

Die Touchzielprüfung ist im aktuellen Runner weiterhin als echte UIA-Boundsprüfung vorhanden: `SaveRefreshSettings`, `RefreshInterval`, `OriginSearch`, `OriginLocation`, `OriginCoordinateMode` und `SearchJourneys` müssen jeweils mindestens 44×44 logische Einheiten groß sein. Der erfolgreiche native Journey-Lauf des Nutzers (`PASS all fixture native UI scenarios`) und die bisherigen erfolgreichen Designläufe belegen diese bereits ausgeführten Assertions. Für die neuen breiten Ansichten fehlen wegen des Vordergrundabbruchs weiterhin aktuelle UIA-Bounds und Bildschirmbilder.

Die Kontrastwerte der tatsächlich verwendeten Farbtokens wurden unabhängig nach WCAG-Relativluminanz berechnet; normale Schrift muss mindestens 4,5:1 erreichen. Die Werte sind eine Tokenprüfung und ersetzen keinen Bildvergleich.

| Vordergrund auf Hintergrund | Kontrast | Ergebnis |
|---|---:|---|
| `#1A1B1F` auf `#FAF9FE` | 16,42:1 | bestanden |
| `#414755` auf `#FAF9FE` | 8,88:1 | bestanden |
| `#0058BC` auf `#EEEDF3` | 5,78:1 | bestanden |
| `#F5F5F7` auf `#1C1C1E` | 15,63:1 | bestanden |
| `#C5C7D0` auf `#1C1C1E` | 10,09:1 | bestanden |
| `#A8C8FF` auf `#2C2C2E` | 8,21:1 | bestanden |
| Weiß auf Linienbadges (RE/S/U/Tram/Bus/Fähre/neutral) | 5,32–9,30:1 | bestanden |

Die Linienfarben stammen aus `DeparturePresentation.BadgeColor`; die hellste Kombination ist Weiß auf Fähre `#00777F` mit 5,32:1. Damit erfüllen auch die kleineren Badge-Beschriftungen die Grenze für normalen Text.

### Noch offene, tatsächlich auszuführende Nachweise

- Die breite Home-Matrix lief mit aktuellem Build und `favorite-cache-seed` erfolgreich: hell und dunkel bei 1024×768, 100 %, jeweils mit `-HomeOnly`. Die Bilder liegen unter `artifacts/step9-visual-final/wide-visible-light` und `artifacts/step9-visual-final/wide-visible-dark`.
- Für Suchrückkehr, gecachten Haltestellenmonitor, Endpunkttausch, Verbindungsfavorit und verdichtete Detailansicht fehlen weiterhin die Bilder aus dem vollständigen, nicht auf Home begrenzten Matrixlauf. Danach folgt die unabhängige Sichtprüfung gegen die Referenzen.
- iPhone-Geräteabnahme durch den Nutzer: kleines und großes iPhone, hell und dunkel, Hoch- und Querformat; Safe Areas oben/unten und Tabbar; Dynamic Type mindestens große Stufe; VoiceOver-Namen/Reihenfolge/Status; echte GPS-Entfernung; Cacheanzeige beim Favoriten und aus der Haltestellensuche geöffneten Monitor während eines laufenden Refreshes.
- Die Ergebnisse dieser iOS-Prüfung müssen als Gerät, iOS-Version, Thema, Textgröße, Orientierung und beobachtetes Resultat festgehalten werden. Ein Simulator- oder Windows-Bild ersetzt diese Punkte nicht.

Die manuelle Schrittfolge für den iOS-Nutzercheck ist in [ios-device-acceptance.md](ios-device-acceptance.md) aktualisiert. **Status: nicht ausgeführt; iOS-Abnahme bleibt nutzerseitig offen.** Es wurden in diesem Arbeitspaket keine iOS-Gerätebilder oder iOS-Bedienergebnisse erzeugt.

## Verbindungsansichten – breite Windows-Matrix, 04.10.2026

Der isolierte native Lauf **hell, 1024×768, 100 %, `favorite-cache-seed`, `-ConnectionOnly`** lief vollständig durch. Er erzeugte aktuelle Aufnahmen für Formular, ungültige Suche, gewählte Endpunkte, Start/Ziel-Tausch, Ergebnisliste, gespeicherte Verbindung und die verdichteten Detailzustände unter [wide-connection-light](../../../../artifacts/step9-visual-final/wide-connection-light).

Die UIA-Prüfung bestätigte Touchziele für Startsuche (48×48), Standort (48×48), Koordinatenmodus (62×48) und die zunächst unterhalb des sichtbaren Scrollbereichs liegende Verbindungssuche (792×44). Der Runner scrollt diese Kontrolle nun vor der Boundsmessung sichtbar; das ist eine Testkorrektur, keine Produktänderung.

Ein anfänglicher Lauf blieb am nicht mehr vorhandenen `JourneyDetailSection4` stehen, obwohl die vorangehenden Detailbilder bereits erzeugt waren. Die aktuelle Detailansicht endet bei `JourneyDetailSection3`; der letzte Capture referenziert daher diesen vorhandenen Abschlusszustand. Ein weiterer Lauf wurde einmal durch eine noch laufende asynchrone Bindungsaktualisierung nach zweimaligem Tausch abgebrochen. Nach einer kurzen Stabilisierung vor der Routensuche lief die gesamte Serie erfolgreich durch.
