# Kleinplan – Verdichtete mobile Abfahrtsanzeige

**Bezug:** [Anforderung](requirement-departure-ux.md), [Bestandsaufnahme](inventory-departure-ux.md)  
**Stand:** 04.10.2026  
**Ziel:** Die Rückmeldungen in kleine, überprüfbare Implementierungsschritte zerlegen. Jeder Schritt soll auf Windows mit UiTest-Fixtures nachvollziehbar sein; die Smartphone-Ansicht wird zusätzlich nach Bereitstellung visuell geprüft.

## Reihenfolge und Arbeitspakete

### Paket 1 – Positionsweitergabe und Entfernung

- Den Positionsabruf für nahe Haltestellen so zusammenführen, dass ein erfolgreich ermittelter Standort auch `FavoriteHomeViewModel` erreicht und dort die Favoritenentfernungen neu berechnet.
- Positionsstatus weiterhin nach Berechtigung, deaktiviertem Standortdienst, fehlgeschlagener/abgebrochener Bestimmung und Erfolg unterscheiden. Fehlende Haltestellenkoordinaten als eigenen Zustand behandeln, nicht als GPS-Fehler.
- Erfolgreiche Position und berechenbare Haltestellenkoordinaten müssen für alle betroffenen Favoriten eine Entfernung ergeben.
- **Prüfung:** Core-Tests für Distanzberechnung und fehlende Koordinaten; UI-Fixtures für Positionserfolg, Berechtigungsfehler, deaktivierte Dienste und Positionsfehler. Sicherstellen, dass Standortdaten weder zusätzlich übertragen noch dauerhaft gespeichert werden.
- **Abhängigkeit:** Keine.

### Paket 2 – Gemeinsame verdichtete Abfahrtskarte

- `DepartureCardView` in kompakter Startseiten- und ausführlicher Detaildarstellung auf dieselbe Zeitregel bringen: Ist-Zeit prominent, abweichende Soll-Zeit direkt darunter kleiner und durchgestrichen; ohne Abweichung nur eine Zeit.
- Vergleichs-, Abweichungs- und „Verspätung unbekannt“-Texte entfernen. Geplantes Gleis nur bei tatsächlicher Abweichung vom aktuellen Gleis zeigen. Quellenzeilen aus Karten und Favoritendetail entfernen.
- Linie, Ziel, relevante Zeit sowie tatsächliche Echtzeit- und Gleisänderungen erhalten.
- **Prüfung:** UiTest-Fixtures für abweichende/gleiche/fehlende Echtzeit, geändertes/unverändertes/fehlendes Gleis und beide Kartendarstellungen. Screenshot bei schmaler Smartphonebreite auf abgeschnittene oder überlappende Inhalte prüfen.
- **Abhängigkeit:** Keine; kann unabhängig von Paket 1 entwickelt werden.

### Paket 3 – Ladeindikator auf Favoritenkarte und Detailmonitor

- Text „Abfahrten werden aktualisiert“ in der Favoritenkarte entfernen und den Ladezustand durch einen ActivityIndicator oben rechts auf der jeweils betroffenen Karte zeigen.
- In `DeparturePage` den Aktualisierungstext aus `MonitorStatus` ebenfalls ausblenden und den bisherigen separaten Indikator durch einen ActivityIndicator rechts oben im Haltestellenkopf ersetzen. Fehler- und Leerzustände bleiben verständlich; vorhandene Abfahrten bleiben während des Abrufs darunter sichtbar.
- Den Indikator mit einem zugänglichen Namen beziehungsweise Status versehen. Während des Ladens müssen Karteninhalte und Bedienung erhalten bleiben.
- **Prüfung:** Verzögerte Fixture-Anfrage auf Startseite und Detailmonitor: Symbol erscheint nur im betroffenen Kopf, der Text erscheint nicht, Inhalt und Bedienung bleiben erhalten, Symbol verschwindet nach Erfolg, Fehler und Abbruch. Stabile AutomationIds und zugänglichen Namen des Indikators prüfen.
- **Abhängigkeit:** Paket 2 ist für die endgültige Kartenanordnung hilfreich, aber nicht fachlich erforderlich.

### Paket 4 – Vorhandene Abfahrten in die Favoritendetails übernehmen

- Beim Öffnen eines Favoriten das bereits geladene, gültige Ergebnis an den Detailmonitor übergeben, bevor der neue Abruf startet. Möglichst die vorhandene Cache-/Restore-Regel wiederverwenden: nur noch bevorstehende und noch gültige Daten unmittelbar anzeigen.
- Den anschließenden Abruf wie bisher ausführen. Währenddessen Daten sichtbar lassen; Erfolg ersetzt sie und aktualisiert den lokalen Cache, Fehler oder Abbruch lassen noch gültige vorhandene Daten stehen.
- **Prüfung:** UI-Szenarien mit gültigem Startseiten-Cache und verzögertem, erfolgreichem, fehlerhaftem sowie abgebrochenem Abruf; zusätzlich abgelaufene und vergangene Daten dürfen nicht als nächste Abfahrten erscheinen.
- **Abhängigkeit:** Keine; vor dem Linienüberblick umsetzen, damit beide Ansichten dieselbe Datenquelle nutzen.

### Paket 5 – Persistiertes Linieninventar und sichere Cache-Erweiterung

- Das Cachemodell additiv um ein Linieninventar je bestehendem Schlüssel `(Source, StopId)` erweitern. Die Liniennamen werden aus der letzten **erfolgreichen vollständigen** Antwort gespeichert, unabhängig davon, ob für die Linie noch eine zukünftige Abfahrt beziehungsweise eine bekannte Zeit vorhanden ist.
- Eine gemeinsame Entscheidung für Start- und Detailaktualisierungen verwenden: Eine Antwort darf Inventar ersetzen, wenn `ErrorCode is null`, `Warnings.Count == 0`, `IsFallback == false` und `IsStale == false` gelten und keine bekannte Kürzung vorliegt. Keine Ausnahme für vermeintlich harmlose Warnungen: auch `provider_information`, `provider_message` oder `region-unknown` verhindern die Inventarersetzung. Brauchbare Ereignisse solcher Antworten dürfen angezeigt werden; das letzte persistierte vollständige Inventar und dessen Cache werden dadurch nicht ersetzt.
- Kürzungen über das bestehende `ProviderResult.Warnings`-Modell mit einem technischen Code `truncated-response` kenntlich machen und durch Mapping, Orchestrierung und Service erhalten. Die `Take(...)`-Pfade in `TransitJson.Map`, `ProviderOrchestrator.MergeEvents` und `DepartureService` müssen Überhang vor dem Abschneiden erkennen (begrenzt auf Limit plus ein Element genügt). Bereits vom Provider gemeldete Kürzung beziehungsweise eine erreichte externe Ergebnisobergrenze ohne Vollständigkeitsbestätigung zählt ebenfalls als unvollständig. Ein regulär angefragtes Zeitfenster ist hingegen kein Teilfehler: Vollständigkeit bezieht sich auf dieses Fenster und bedeutet kein gesamtes Fahrplaninventar.
- Reale Warnungs-/Fallbackfälle `partial-primary`, `primary-unavailable`, `secondary-unavailable` und `stale-fallback` bleiben unvollständig, auch wenn sie brauchbare Ereignisse liefern. Eine leere Liste ist nur dann ersetzungsfähig, wenn sie dieselbe gemeinsame Regel erfüllt. Das heutige synthetische `empty_response` darf für eine strukturell gültige, ausdrücklich leere Abfahrtsantwort entfallen; fehlende/ungültige Nutzdaten dürfen dadurch nicht als gültig leer gelten. Die Orchestrierung darf einen warnungsfreien leeren Erfolg als solchen erhalten, sofern alle tatsächlich benötigten Quellen sauber leer antworten; Fehler/Warnung/Fallback einer beteiligten Quelle dürfen niemals weg-normalisiert werden. Einen warnungsbehafteten leeren Bestand nicht löschen.
- Cache-Schreibvorgang atomar am bestehenden Speichermechanismus halten: erst nach als vollständig bestätigter Antwort Linienbestand ersetzen. Fehlgeschlagene, abgebrochene oder unvollständige Antworten ändern ihn nicht. Nur ein erfolgreiches vollständiges leeres Ergebnis darf ihn leeren.
- Bestehende Cacheeinträge ohne Linieninventar müssen weiter lesbar sein. Beim Laden fehlende/null-Werte als leeres Inventar interpretieren; gültige Abfahrtsdaten nicht verwerfen, neu serialisieren oder migrieren, nur um das neue Feld einzuführen. Inventar für nicht mehr gespeicherte Favoriten weiterhin bereinigen; beim Entfernen eines Favoriten seinen Eintrag samt Inventar löschen.
- Inventar unabhängig vom Erfolg von `FavoriteMonitorViewModel.Restore` wiederherstellen. Den Löschpfad in `FavoriteHomeViewModel.LoadAsync` ändern: Sind alle Ereignisse vergangen, zeitlos oder nicht mehr gültig, werden ausschließlich die nicht nutzbaren Abfahrten verworfen; vorhandene Linien bleiben im Speicher und auf Datenträger erhalten. Keine Abfahrtsuhrzeit aus solchen Ereignissen anzeigen. Nur Favoritenlöschung, verwaiste Identität oder eine erfolgreiche vollständige neue Antwort dürfen Linien entfernen. Neue Inventarwerte mit den vorhandenen Dateigrößen-/Atomaritätsregeln sowie begrenzter Anzahl und Länge absichern.
- Für erfolgreiche vollständige Antworten Inventar und Abfahrten konsistent aktualisieren. Das Inventar wird aus fachlichen Linienbezeichnungen dedupliziert und deterministisch sortiert; pro Linie wird im UI später die nächste bekannte zukünftige Zeit abgeleitet, während die Linie selbst auch ohne Zeit bestehen bleibt.
- **Prüfung:** Core-Tests für Lesen alter Einträge, fehlendes/null-Feld, erfolgreiche Ersetzung, neue/entfallene Linien, warnungsfrei leeres Ergebnis, Fehler/Abbruch/unvollständig, Zuordnung nach Quelle und Haltestelle sowie Favoritenlöschung. Reale Mapping-/Orchestratorpfade mit `partial-primary`, `secondary-unavailable`, `stale-fallback`, sonstiger Warnung und tatsächlichem Überhang testen; nicht allein künstliche Vollständigkeitsflags prüfen. Zusätzlich ausschließlich vergangene Ereignisse mit Inventar über zweimaliges Laden/Neustarten und verzögerten beziehungsweise fehlerhaften Abruf prüfen: Linien bleiben gespeichert und ohne Uhrzeit sichtbar. Alten Cache ohne Inventarfeld separat prüfen. Vorhandene Cache-Tests bleiben gültig.
- **Abhängigkeit:** Vor Paket 6. Kann unabhängig von UI-Kartenlayout umgesetzt werden.

### Paket 6 – Eingeklappte Favoriten und Linienübersicht

- Favoriten beim initialen Anzeigen standardmäßig eingeklappt rendern; Auf-/Zuklappen pro Karte über Touch ermöglichen.
- Eingeklappte Karte zeigt Linien des gespeicherten Inventars nebeneinander. Für jede Linie die nächste bekannte zukünftige Zeit anzeigen, andernfalls ausschließlich den Liniennamen.
- Aufgeklappte Karte rendert die Abfahrten über die gemeinsame verdichtete Karte aus Paket 2. Erfolgreiche Aktualisierungen erneuern beide Zustände aus demselben Ergebnis; nicht mehr gelieferte Linien verschwinden erst nach vollständigem Erfolg.
- Beim Programmstart zuerst lokales Linieninventar laden und darstellen, anschließend wie vorgesehen aktualisieren. Ein erstmaliger Favorit ohne Inventar bleibt ohne erfundene Linien; der leere Zustand darf kein Lade- oder Fehlerergebnis als erfolgreiches leeres Linieninventar persistieren.
- **Prüfung:** UI-Tests für Startzustand eingeklappt, Expansion, mehrere Linien nebeneinander, Zeiten vorhanden/fehlend, Neustart mit Linien ohne Abfahrt und Linienbestand nach Erfolg/Fehler. Prüfen, dass Aufklappzustand und Abfahrtsdaten nach Aktualisierung nicht auseinanderlaufen.
- **Abhängigkeit:** Paket 5 für persistierte Linien; Paket 2 für aufgeklappte Darstellung.

### Paket 7 – Zusammenhängende UI-/Akzeptanzprüfung

- Einen Windows-UiTest-Journey vom Start mit gespeicherten Favoriten bis zur Detailansicht abdecken: Inventar zuerst, danach laufender Abruf mit Symbol, erfolgreiche Erneuerung, Expandieren und sofort sichtbare Cacheabfahrten in den Details.
- Alle folgenden E2E-Fälle sind verpflichtend. Fehlende steuerbare Fälle in `UiTestFixtureServices.cs` ergänzen; Verzögerung und Abschluss/Fehler/Abbruch müssen deterministisch steuerbar sein.
- `tests/WindowsJourneyUiTests/WindowsDepartureCacheUiTests.ps1`: App mit gespeicherten Linien und ausschließlich vergangenen Ereignissen starten; Karte eingeklappt, Linien ohne Uhrzeiten sofort sichtbar, Busy-Symbol aktiv, Aktualisierungstext fehlt. Fehler auslösen und zweimal neu starten; unverändertes Inventar bleibt persistent. Gültige zukünftige Ereignisse separat vorbereiten, Favoritendetail während verzögertem Abruf öffnen und sofortige Anzeige prüfen; Erfolg ersetzt, Fehler und Abbruch erhalten gültige Daten, vergangene Ereignisse erscheinen nie. Zusätzlich Linien A/B erfolgreich durch B/C ersetzen, neu starten und B/C prüfen; sauberes leeres Ergebnis leert auch nach Neustart, Teilantwort/Warnung/Fehler/Abbruch erhalten den gespeicherten Bestand.
- `tests/WindowsJourneyUiTests/WindowsJourneyUiTests.ps1`: Eingeklappte Karte per nativer Automation auf- und zuklappen, nächste Zeit je Linie und mehrere nebeneinander liegende Linien prüfen; anschließend Detail öffnen. Standorterfolg muss automatisch eine Entfernung ohne separate Entfernungsaktion liefern. Getrennte Fixtures für verweigerte Berechtigung, deaktivierten Standortdienst, erfolglose Position und fehlende Haltestellenkoordinate müssen passende sichtbare Hinweise ergeben.
- `tests/WindowsJourneyUiTests/WindowsRefreshUiTests.ps1` und `WindowsLifecycleUiTests.ps1`: laufende Aktualisierung auf Favoritenkarte beziehungsweise Detailmonitor prüfen (nur zugehöriges Symbol oben rechts, kein Aktualisierungstext, vorhandene Daten sichtbar). Erfolg, Fehler und durch Navigation/Lifecycle ausgelösten Abbruch jeweils abschließen; bei Rückkehr bleibt kein hängender Indikator zurück und gültige Cachewerte bleiben verfügbar.
- `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1`: schmale Fensterbreite 430 Pixel, Light/Dark und 100/150 Prozent Textgröße. Screenshots für eingeklappte Linienchips, expandierte Karte und Detailmonitor erstellen. Gleiche/abweichende/fehlende Echtzeit sowie unverändertes/geändertes/fehlendes Gleis prüfen. Ist-Zeit einmal im Kopf, abweichende Soll-Zeit kleiner und durchgestrichen darunter; keine Vergleichszeile, Abweichungstexte, unbekannte Verspätung oder Quellenzeile. Nur tatsächlich geändertes geplantes Gleis anzeigen. Screenshotprüfung bestätigt Symbolposition, Durchstreichung und fehlende Überlappungen; UI-Automation bestätigt Texte, Sichtbarkeit und Bedienung.
- **Prüfung:** Core-Testprojekt und Windows-UiTest-Build; anschließend alle genannten Skripte mit der gebauten UiTest-EXE erfolgreich ausführen und Ergebnisse/Screenshots dokumentieren. Fehlende, nicht ausführbare oder fehlgeschlagene Pflichtfälle bleiben offene Abnahmepunkte und werden nicht durch Build- oder Core-Erfolg ersetzt. Die native Smartphoneprüfung liegt beim Anwender; der lokale visuelle Nachweis erfolgt vollständig über die schmalen Windows-Screenshots.
- **Abhängigkeit:** Pakete 1–6.

## Inkrementelle Lieferpunkte

1. Nach Paket 1 ist die Ursache „GPS aktiv, Entfernung unbekannt“ in der Startseitenkette behoben und Fehlerursachen bleiben unterscheidbar.
2. Nach Paket 2 und 3 sind Karten auf Start- und Detailansicht verdichtet und der Aktualisierungszustand ist kompakt sichtbar.
3. Nach Paket 4 bleiben gültige Abfahrten beim Öffnen des Favoritendetails und während des Abrufs erhalten.
4. Nach Paket 5 und 6 startet die Startseite mit einem persistenten, eingeklappten Linienüberblick und ersetzt Linien nur nach einer vollständigen erfolgreichen Antwort.
5. Paket 7 sichert die Ende-zu-Ende-Abläufe und schließt mit geprüften schmalen Windows-Ansichten ab; die zusätzliche Smartphoneprüfung erfolgt beim Anwender.

## Migrations- und Datenintegrität

Die Cache-Erweiterung ist rückwärtskompatibel und additiv: alte JSON-/Dateieinträge ohne Linienfeld bleiben lesbar, bestehende Abfahrten werden nicht gelöscht und ein fehlendes Inventar bedeutet ausschließlich „noch keine gespeicherten Linien“. Linieninventar und Abfahrtsdaten teilen weiterhin denselben Haltestellenschlüssel. Updates werden erst nach erfolgreicher vollständiger Providerantwort übernommen; Fehler, Abbruch und unvollständige Antworten behalten sowohl bestehende Linien als auch den letzten brauchbaren Abfahrtscache. Die bereits vorhandene Bereinigung verwaister Favoritenschlüssel gilt auch für das neue Inventar. Es ist keine separate destructive Migration oder Datenbank-Neuanlage erforderlich.

## Abnahmekriterien

- Alle Punkte R1–R4 sind durch die benannten Core-Tests und sämtliche verpflichtenden E2E-Fälle aus Paket 7 abgedeckt; UI-Flüsse benötigen ihren eigenen erfolgreichen E2E-Nachweis.
- Alte Cacheeinträge laden ohne Ausnahme und behalten ihre gültigen Abfahrtsdaten.
- Nur erfolgreiche vollständige Antworten dürfen das persistierte Linieninventar verändern.
- Windows-Build, Core-Tests und alle benannten Windows-E2E-Skripte laufen erfolgreich; die schmalen Windows-Screenshots sind visuell geprüft. Die ergänzende Smartphoneprüfung verbleibt beim Anwender.
- **Umfangsregel:** Jede Implementierungseinheit bleibt auf das benannte Paket begrenzt; insbesondere werden keine Standortanbieter, Hintergrundaktualisierung oder Gerätesynchronisierung ergänzt.
