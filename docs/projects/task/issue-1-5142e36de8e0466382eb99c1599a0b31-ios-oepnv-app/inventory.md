# Bestandsaufnahme – iOS ÖPNV-App

Stand: 2026-09-07. Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`.
Eingaben: [Anforderung](requirement.md), `issue.md`, tatsächliche versionierte Dateien und lesend geöffnetes `design-draft.zip`.

## Ergebnis

Das Repository enthält eine lauffähig testbare .NET-10-Basis mit MAUI-Windows-Vorlage, jedoch noch keine ÖPNV-Funktion. iOS ist weder als Target noch mit Plattformdateien eingerichtet. Die vorhandene Trennung App/Core/Tests ist für das Projekt wiederverwendbar. Es fehlen die gesamte fachliche Datenintegration und UI, Standort/Karten, Persistenz sowie iOS- und UI-Testinfrastruktur.

## Quellen und relevante Bereiche

| Bereich | Belegter Ist-Zustand | Wiederverwendung / erforderliche Arbeit |
|---|---|---|
| Solution/Build | `FlowNRW.sln`, `Directory.Build.props`: drei Projekte, zentrale Version 0.0.1, C# latest | Solution und Versionsmechanik behalten; Version nicht manuell erhöhen |
| MAUI-Projekt | `FlowNRW/FlowNRW.csproj`: ausschließlich `net10.0-windows10.0.19041.0`, globale Windows-Runtime und Mindestversion, Controls und Debug-Logging | iOS-Target und Plattformdateien ergänzen; Windows-spezifische Properties konditionieren |
| Plattformen | Nur `FlowNRW/Platforms/Windows` vorhanden | iOS-Einstieg, Manifest, Berechtigungsbeschreibungen und Laufzeitintegration fehlen |
| Einstieg/DI | `MauiProgram.cs`: `UseMauiApp`, OpenSans-Fonts, Debug-Logger; keine fachlichen Service-Registrierungen | Vorhandenen DI-Container für Services, ViewModels und Views verwenden |
| Navigation/UI | `App.xaml.cs` erzeugt `AppShell`; Shell enthält nur Home/MainPage | Navigation, ViewModel-Anbindung und fachliche Views neu implementieren |
| Vorlagenfunktion | `MainPage.xaml`, `.xaml.cs`: Bot, Begrüßung, ClickCounter mit Code-behind | Vorlage inklusive fachlich nutzloser Counter-Logik ersetzen |
| Fachkern | `FlowNRW.Core/ClickCounter.cs` ist einzige Core-Klasse; net10.0 ohne Fremdpakete | Plattformunabhängige Modelle, Provider, Services, Cache und prüfbare Zustandslogik hier aufbauen |
| Datenversorgung | Keine Providerkonfiguration, HTTP-Clients, API-Modelle oder Zugangsdatenreferenzen im App-/Core-Code | Asynchrone Anbieteradapter, sichere Konfiguration, Normalisierung, regionale Auswahl, Konsolidierung und Fallback erforderlich |
| Standort/Karten | Keine Implementierung, kein Maps-Paket, keine iOS-Standortberechtigungen | Standortadapter mit Ablehnungszuständen; Karte und Haltestellenauswahl samt gelieferter Liniengeometrie ergänzen |
| Speicherung/Aktualisierung | Keine Favoriten-, Cache-, Intervall- oder Hintergrundimplementierung | Technische Speicherung, Lebenszyklussteuerung und datensparsame Diagnose hinzufügen |
| Gestaltung | `Resources/Styles/Colors.xaml`, `Styles.xaml`: MAUI-Standardfarben, Theme-Bindings, Styles mit Mindestgrößen; OpenSans lokal vorhanden | Ressourcenstruktur und Semantikansätze nutzbar, Gestaltung nach Entwurf ersetzen |
| Qualität | `FlowNRW.Tests/ClickCounterTests.cs`: fünf xUnit-Testfälle für Counter | Testprojekt mit xUnit 2.9.3, Test SDK 17.14.1, Coverlet 6.0.4 wiederverwenden; keine Service-/UI-Tests vorhanden |

## Designarchiv

Lesend mit `System.IO.Compression.ZipFile` inspiziert. Enthalten sind vier HTML-Screens (Abfahrtsmonitor, Verbindungssuche, Fahrtbegleiter-Detail, Umgebungskarte), `flownrw_app_flow_dokumentation.md`, `trans_nrw_mobil_system/DESIGN.md` und ein Icon-PNG (646143 Byte). Die vier Screen-PNGs sind jeweils nur 28 Byte und kein belastbarer Screenshot-Nachweis. HTML nutzt Tailwind-CDN, Google Fonts/Material Symbols und statische Beispieldaten; es ist eine Gestaltungsreferenz, keine vorhandene App-Implementierung.

Wiederverwendbare Designentscheidungen: blaue Interaktionsfarbe, helle/dunkle gruppierte Karten, kompakte farbige Linienkennzeichnungen, lesbare Soll-/Ist-Zeiten, textlich erkennbare Verspätung/Ausfall/Gleiswechsel, 16-Punkt-Seitenabstände und abgerundete Karten. Der Entwurf enthält widersprüchliche vier/fünf Tabs und zusätzliche Ticket-/Sharing-/Fahrtbegleiterfunktionen. Maßgeblich bleibt der abgegrenzte Kernumfang in requirement.md; aus den statischen Beispielen dürfen keine echten Echtzeitdaten abgeleitet werden. Extern geladene Fonts sind kein bestehendes lokales App-Asset. Barrierearmut muss an nativen Views überprüft werden, nicht aus dem HTML behauptet werden.

## Konventionen und CI

- Keine `AGENTS.md` im untersuchten Repository gefunden. `CONTRIBUTING.md`, `.editorconfig`, `.githooks/` und `docs/CI-CD.md` sind relevante Konventionsquellen.
- UTF-8/LF, vier Leerzeichen für C#/XAML/XML, zwei für Markdown/YAML/JSON. Nullable und implizite Usings aktiv.
- Öffentliche Member brauchen XML-Dokumentation; `CS1591` und `NU1605` sind Buildfehler. Konvention: Fachlogik in Core, App dünn halten.
- Lokale Hooks prüfen Dokumentation, Format, Stubs und Enum-Abdeckung; Conventional Commits. Projektworkflow verwendet den beauftragten Basisbranch und eigene Schrittbranches.
- `.github/workflows/pr-staging-ci.yml`: Windows-Format/Security/Build mit Warnungen als Fehler, Linux-Core-Tests, mindestens 70 % Zeilenabdeckung, Node-Release-Skripttests.
- `.github/actions/setup-maui-workload/action.yml` installiert ausschließlich maui-windows. Build/Package und bestehende Release-Artefakte sind Windows-spezifisch. Kein vorhandener macOS-/iOS-/Appium-Job. Verbindliche Nutzerergänzung: Windows-Release UND Tests in GitHub Actions bleiben erhalten; iOS-CI und automatisiertes iOS-Deployment werden hier nicht verlangt. Bestehende Windows-CI darf nicht durch ein unbedingtes iOS-Target unbrauchbar werden. Native iOS-Testnachweise sind getrennt von GitHub Actions zu ermöglichen und ehrlich zu dokumentieren.

## Erreichbare Umgebung und ausgeführte Prüfung

Windows/PowerShell; `dotnet --version` meldet 10.0.400. `dotnet workload list`: android 36.1.69, ios 26.5.10301, maccatalyst 26.5.10301, maui-windows 10.0.20 (Visual-Studio-Installationsquelle). `xcodebuild`, `xcrun` und `appium` sind nicht im PATH. Keine einschlägigen Buildhost-/Provider-Umgebungsvariablennamen gefunden; Werte wurden nicht ausgegeben. Es wurde kein externer Mac kontaktiert; ein verfügbarer verbundener Mac ist nicht belegt.

**Tatsächlich ausgeführt:** `dotnet test FlowNRW.Tests/FlowNRW.Tests.csproj -c Release --nologo`: Restore und Build erfolgreich, 5 bestanden, 0 fehlgeschlagen, 0 übersprungen. Dies bestätigt ausschließlich die bestehende Counter-Basis. Kein vollständiger Windows-App-Build, keine iOS-Kompilierung und kein UI-Test wurden in dieser Phase ausgeführt.

Sinnvolle spätere Nachweise: deterministische HTTP-Fixture-Tests für Normalisierung und Fallback; Soll-/Ist-Abgleich, Anbieter-ID-Zuordnung, Zeitzonen und Ausfälle; Cache/Abbruch/Intervalle; Standortverweigerung, Favoritensortierung und Datenschutz; ViewModel-Abläufe für Suche und Monitor. Native UI-Tests müssen tatsächliche App-Interaktionen überprüfen und dürfen nicht durch reine ViewModel-Tests ersetzt werden. Lokale Core-Tests sind nachgewiesen möglich; iOS-Simulator/Geräteabnahme benötigt erst eine belegte Ausführungsumgebung.

## Externe Datenversorgung – Recherche des Projektleiters

Das Repository legt keine Anbieter fest. [ÖPNV Open Data API-Dokumentation](https://www.opendata-oepnv.de/ht/de/api) beschreibt den VRR-EFA-Entwicklungszugang `openservice-test.vrr.de/openservice` mit rapidJSON ohne Registrierung; produktiver Zugang ist gesondert anzufragen. Das ist eine Entwicklungsoption, keine Zusage produktiver Versorgung.

Die [db.transport.rest-v6-Dokumentation](https://v6.db.transport.rest/api.html) beschreibt eine bundesweite REST-Schnittstelle ohne Authentifizierung und mit 100 Anfragen pro Minute. Sie ist eine zu prüfende Entwicklungsoption für den bundesweiten Adapter. Live-Erreichbarkeit, vollständige fachliche Abdeckung, NRW-Abdeckung außerhalb VRR, ID-Verknüpfung und konkrete Antwortfelder sind noch nicht durch diese Bestandsaufnahme nachgewiesen. Die Provider-Recherche wird vom Projektleiter fortgesetzt; deren Ergebnisse müssen in die Planung einfließen.

## Einschränkungen gegenüber Annahmen

**Belegte Lücken:** fehlendes iOS-Target, fehlende iOS-Dateien/Tests, fehlende Fachfunktionen und Adapter, keine Projekt-Providerkonfiguration, unbrauchbar kleine Screen-PNGs. Sie verhindern den Beginn der Implementierung nicht, gehören aber ausdrücklich in deren Umfang.

**Belegte Prüfgrenze:** keine lokale Apple-Werkzeugkette im PATH und keine nachgewiesene Mac-/Simulatorverbindung. Ein installierter iOS-Workload ist kein Testnachweis für die fertige iOS-App. Abnahme darf ungeschehene Plattformtests nicht als bestanden ausweisen.

**Noch nicht belegte Annahmen:** produktiver API-Zugang, tatsächliche Live-Verfügbarkeit und bundesweite/regionale Datenvollständigkeit, Mac-Zugang außerhalb dieser Umgebung. Keine erfundenen Zugangsdaten oder pauschale Behauptung eines endgültig fehlenden Macs. Entwicklungszugänge können ohne Nutzerkontakt geprüft werden. Notwendige Produktivfreigaben und native Testmöglichkeiten sind getrennt von implementierbaren lokalen Funktionen zu verfolgen.

Nachtrag zur Live-Probe des Projektleiters (2026-09-07): Eine lesende VRR-Beispielabfahrtsanfrage aus der offiziellen API-Seite lieferte HTTP 200 mit 91758 Byte rapidJSON und `locations`/`stopEvents`. db-rest `/locations?query=Berlin%20Hbf&results=1` lieferte HTTP 503. Daher ist VRR-Abfahrtszugriff punktuell nachgewiesen, db-rest aktuell nicht als funktionsfähige bundesweite Versorgung belegt. Diese Einzelproben belegen weder dauerhafte Verfügbarkeit noch vollständige regionale/bundesweite Abdeckung.

Weitere Probe des Projektleiters: VRR-STOPFINDER für Berlin Hbf liefert HTTP 200 und Berliner DHIDs (z. B. de:11000:900003201), zugleich BROKER-Systemfehler -8011. Einzelne bundesweite Suchtreffer sind damit belegt; bundesweite Routingverfügbarkeit folgt daraus nicht.


Weitere Routing-Probe (Projektleiter, 2026-09-07): VRR XML_TRIP_REQUEST2 zwischen Koordinaten Berlin Hbf und Alexanderplatz lieferte HTTP 200 und zwei journeys, zugleich BROKER-Warnung -10015 (itp-monomodal). Dies belegt punktuelles Routing außerhalb NRW, keine vollständige bundesweite Abdeckung.
