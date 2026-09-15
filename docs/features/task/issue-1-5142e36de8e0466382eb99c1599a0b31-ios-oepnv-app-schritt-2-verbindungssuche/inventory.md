# Bestandsaufnahme: Manuelle Verbindungssuche mit NRW-Echtzeit

Diese Bestandsaufnahme bezieht sich auf `requirement.md` für Schritt 2 und den unveränderten Schritt-1-Core im aktuellen Branch. Der Schwerpunkt liegt auf den wiederverwendbaren Such-/Routing-Contracts, der MAUI-Einstiegsstruktur, Provider-/Cache-Komposition sowie Build- und Testnachweisen.

## Zusammenfassung

- Der Core enthält die normalisierten Endpunkt-, Haltestellen-, Fahrt-, Teilstrecken-, Ereignis-, Identitäts-, Echtzeit- und Ergebnismodelle (siehe [Modelle](inventory/models.md)).
- `IStopSearchService`/`StopSearchService` validieren Suchtexte, reichen Such- und Koordinatenanfragen abbrechbar weiter und verdrängen ältere Anfragen. `IRoutingService`/`RoutingService` validieren Endpunkte, filtern veraltete Fahrten, sortieren und begrenzen Ergebnisse.
- `ProviderOrchestrator` priorisiert anhand `NrwRegionClassifier` den NRW-Provider, nutzt den nationalen Provider als Ergänzung/Fallback, konsolidiert eindeutig identifizierte Fahrten/Ereignisse und kennzeichnet Warnungen, Fallback und veralteten Cache.
- Die MAUI-App ist aktuell eine Windows-only Vorlage: `MainPage` zeigt Begrüßung/Counter, `AppShell` hat nur die Home-Route; iOS-Target, Such-/Ergebnis-/Detail-Views und ViewModels fehlen.
- `MauiProgram.CreateMauiApp` registriert die Core-Provider, Gateway-, Cache-, Orchestrator-, Such- und Routing-Services einschließlich konfigurierbarer Endpunkte und Grenzen.
- CI baut die MAUI-App auf Windows mit Warnungen als Fehler und führt Core-Tests/Coverage sowie Release-Skripttests aus. Native iOS-Werkzeuge sind lokal nicht nachgewiesen; FlaUI 5.0 ist im NuGet-Cache (`flaui.core`, `flaui.uia3`) für mögliche native Windows-E2E vorhanden.

Test-Ausgangszustand: Der bestehende Nachweis belegt 106 erfolgreiche, 0 fehlgeschlagene und 0 übersprungene Core-Tests sowie 98,61 % Zeilenabdeckung und einen Windows-Build mit 0 Warnungen/0 Fehlern. Details und Nachweislinks stehen in [tests.md](inventory/tests.md). Es wurde für diese Bestandsaufnahme keine unveränderte Testsuite erneut ausgeführt.

## Details

- [Modelle](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests und Ausgangsnachweis](inventory/tests.md)

## MAUI- und Design-Bestand

`FlowNRW/FlowNRW.csproj` targetiert ausschließlich `net10.0-windows10.0.19041.0`; vorhanden sind Windows-Plattformdateien, `App`, `AppShell`, `MainPage`, Standardressourcen und OpenSans-Schriften. `FlowNRW/Platforms/iOS` existiert nicht. Die aktuelle `MainPage` verwendet `FlowNRW.Core.ClickCounter`; diese Vorlagenlogik ist für Schritt 2 zu ersetzen.

Das unversionierte `design-draft.zip` wurde lesend geprüft. Auswertbar sind `verbindungssuche/code.html`, `fahrtbegleiter_detail/code.html`, `flownrw_app_flow_dokumentation.md` und `trans_nrw_mobil_system/DESIGN.md`; die vier zugehörigen Screen-PNGs sind jeweils 28 Byte groß und daher kein visueller Abnahmenachweis. Die HTML/MD-Referenzen zeigen blaue Interaktionsfarbe, gruppierte Karten, Linienkennzeichnungen, textlich erkennbare Echtzeit-/Ausfallzustände und 16-Punkt-Abstände. Sie enthalten statische Beispieldaten und keine bestehende UI-Implementierung.

## Provider, Build und Plattformgrenzen

Die bestehende fachliche Dokumentation unter [Fahrplanauskunft](../../../help/fahrplanauskunft/index.md) beschreibt Datenmodell, Provider-Proben, NRW-Grenze und Datenschutz. Der gültige Core-Nachweis ist [union-correction-checks.md](../../../help/fahrplanauskunft/verification/union-correction-checks.md), inklusive Rohreport `coverage-union.cobertura.xml`; vorhandene Live-Proben bleiben getrennt von Fixture-Nachweisen.

`pr-staging-ci.yml` und `docs/CI-CD.md` belegen Windows-Format/Security/Build, Linux-Core-Tests mit mindestens 70 % Coverage und Node-Release-Skripttests. Der lokale iOS-Workload ist installiert, `xcodebuild`, `xcrun` und `appium` sind nicht im PATH; ein Mac-/Simulatorzugang ist nicht belegt. Diese Grenzen sind keine Aussage über eine fertige iOS-App.

## Konkrete Anschlussstellen für die UI-Planung

- Treffer sind bereits `Address`-Objekte: `SearchAsync` liefert `Task<ProviderResult<Address>>`. Bei der Auswahl das vollständige Objekt einschließlich `Stop.Id`, `Stop.Source`, optionaler DHID und Koordinate erhalten; nicht aus dem angezeigten Namen rekonstruieren. Adresstreffer können ohne Stop-ID nur eine Koordinate besitzen.
- Koordinaten lassen sich nach Eingabevalidierung unmittelbar als `Address { Coordinate = new GeoCoordinate(latitude, longitude) }` an `RouteAsync` übergeben. `NearbyAsync` ist dafür nicht erforderlich. Parser, verständliche Validierungsanzeige und Auswahlzustand fehlen noch.
- `StopSearchService` besitzt genau einen laufenden Abbruchzustand pro Instanz; `SearchAsync` und `NearbyAsync` teilen ihn. Parallel bedienbare Start-/Zielfelder benötigen getrennte Service-Instanzen oder bewusst koordinierte Suchvorgänge. Die vorhandene transiente DI-Registrierung erlaubt getrennte Instanzen. Zusätzlich müssen Textänderung und Navigation die jeweilige Auswahl bzw. laufende Anfrage invalidieren.
- `ProviderResult<T>` enthält `ErrorCode`, `Warnings`, `Source`, `RetrievedAt`, `IsFallback` und `IsStale`; `HasData` setzt vorhandene Items und fehlenden Fehlercode voraus. Diese Metadaten gemeinsam mit den Ergebnissen in Such-/Ergebniszustand halten. Übersetzte Statusmeldungen und Zeitdarstellung sind neue UI-Aufgaben.
- `App.CreateWindow` erstellt `AppShell` aktuell direkt mit `new`; Shell und Seiten sind noch nicht über DI komponiert. Für Suche → Ergebnisse → Detail sind Navigation und ein erhaltener Such-/Ergebniszustand einzurichten. Die bestehenden Windows-Eigenschaften (`RuntimeIdentifier`, Mindestversion, AppSDK) müssen bei Ergänzung des iOS-Targets plattformspezifisch konditioniert werden.

## Konkrete native UI-Prüfmöglichkeit

Die bestehende Testsuite referenziert ausschließlich `FlowNRW.Core`; sie führt keine MAUI-Oberfläche aus. Ein gesonderter Windows-Testtreiber kann die ungepackte `FlowNRW.exe` starten und über FlaUI/UIA3 bedienen. Dafür benötigen Eingaben, Trefferwahl, Suchaktion, Ergebnisse, Detail und Rücknavigation stabile `AutomationId`-Werte. Der vorhandene Paketcache ist nur eine technische Einstiegsmöglichkeit, kein Beleg funktionierender UI-Automation.

Deterministische native Abläufe lassen sich mit explizit aktiviertem Test-DI für die vorhandenen Such-/Routing-Interfaces prüfen: Adresse, Haltestelle und Koordinate in beiden Feldern, Mehrdeutigkeit, langsame/verdrängte Anfrage, Validierung, Leer-/Fehlerzustand, Fallback/Stale, Ergebniswahl und Rücknavigation. Live-Suche über die reguläre DI bleibt ein separater Nachweis. Erster Prüfpunkt ist ein tatsächlich gestartetes Fenster mit lesbarem UIA-Baum; bei Scheitern sind Startbefehl, Laufzeitfehler bzw. UIA-Zugriffsproblem und betroffene Flüsse festzuhalten. iOS bleibt bei der vereinbarten manuellen Nutzerprüfung.
