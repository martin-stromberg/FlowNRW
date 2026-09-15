# Anforderung – Schritt 2: Manuelle bundesweite Verbindungssuche mit NRW-Echtzeit

## Fachliche Zusammenfassung

Die bestehende .NET-MAUI-Vorlage wird zu einer bedienbaren, in Visual Studio entwickelbaren ÖPNV-App für Windows und iOS erweitert. Nutzer erfassen Start und Ziel manuell als Adresse, Haltestelle oder gültige Koordinate, wählen bei Mehrdeutigkeiten einen Treffer und erhalten zeitlich geordnete bundesweite Verbindungen mit Details zu Umstiegen, Fußwegen, Linien, Betreibern und verfügbaren Soll-/Ist-Daten. Die vorhandenen asynchronen, abbrechbaren Services aus Schritt 1 liefern reale Daten; NRW-Echtzeit wird bei mindestens einem NRW-Endpunkt bevorzugt, eindeutig konsolidiert und bei fehlenden Daten nachvollziehbar ergänzt oder zurückgefallen.

Die Vorlagenbegrüßung, der Counter und zugehörige Logik entfallen. Navigation führt ausschließlich zu Suche, Ergebnissen und Verbindungsdetails. Die iOS-Plattformbasis wird eingerichtet, Windows bleibt baubar und früh begutachtbar; GPS, automatische Monitore, Favoriten, Karten und weitere Zusatzprodukte gehören nicht zu diesem Schritt.

## Betroffene Klassen und Komponenten

### Bestehende Fachkomponenten zur Wiederverwendung

- `Address`, `GeoCoordinate`, `Stop`, `Journey`, `JourneyLeg`, `StopEvent`, `TripIdentity`, `RealtimeStatus` und `ProviderResult<T>` als normalisierte Datenmodelle.
- `IRoutingService`/`RoutingService` für Validierung, abbrechbare Suche, Filterung alter Fahrten, Sortierung und Ergebnisgrenze.
- `IStopSearchService`/`StopSearchService` für Adress- und Haltestellensuche sowie Trefferauflösung.
- `IProviderOrchestrator`/`ProviderOrchestrator`, `DbRestProvider`, `EfaProvider`, `NrwRegionClassifier`, `RealtimeConsolidator`, `TransitHttpGateway` und `MemoryTransitCache` für Providerwahl, NRW-Priorität, Fallback, Konsolidierung, sichere Abrufe und begrenzten Cache.
- `MauiProgram.CreateMauiApp` als DI-Komposition; vorhandene Provider-/Cache-Konfiguration und Diagnose bleiben konfigurierbar und datensparsam.

### Voraussichtlich zu ergänzende oder zu ersetzende App-Artefakte

- `AppShell`/Shell-Routen und Navigation für Suche, Ergebnisliste und Verbindungsdetail.
- Eine Such-View, Ergebnis-View und Detail-View sowie zugehörige MVVM-ViewModels (konkrete Namen nach vorhandener Projektkonvention festlegen). Die ViewModels verwenden ausschließlich injizierte `IRoutingService` und `IStopSearchService` beziehungsweise die dafür erforderlichen Contracts.
- Eingabemodelle bzw. Auswahlzustände für Start und Ziel, einschließlich Adress-, Haltestellen- und Koordinateneingabe, Trefferlisten, ausgewählter Identität, Lade-, Leer- und Fehlerzustand sowie Abbruch veralteter Anfragen.
- `MainPage` einschließlich Code-behind und die `ClickCounter`-Vorlagenlogik werden durch den fachlichen Einstieg ersetzt bzw. entfernt. `App.xaml`/Ressourcen und vorhandene Styles werden für blaue Interaktionsfarbe, gruppierte Karten, Linienkennzeichnungen, Soll-/Ist-/Ausfallzustände, skalierbare Texte, Labels, Touch-Ziele und Kontraste angepasst.
- iOS-Target, Einstieg und erforderliche Plattformdateien einschließlich Berechtigungs-/Konfigurationsbasis; eine Standortberechtigung darf für die manuelle Suche nicht angefordert werden. Windows-spezifische Projektkonfiguration bleibt funktionsfähig.
- UI-E2E-Testartefakte für native Windows, soweit die vorhandene Umgebung dies technisch ermöglicht, sowie deterministische Service-/Modelltests. Reine ViewModel-Tests gelten nicht als Ersatz für UI-Ausführung.
- Aktualisierte Visual-Studio-/Plattform- und manuelle iOS-Prüfanleitung sowie Nachweise für Live-Verbindungsproben, Testausführung und technische Grenzen.

### Nicht Teil dieses Schritts

GPS-Übernahme, nahe Haltestellen, Karten, Favoriten, automatische oder Hintergrundaktualisierung, Tickets, Sharing, Push, Reisebegleitung und andere Zusatzansichten werden nicht vorgezogen. Eine iOS-CI-/Deployment-Pipeline und neues automatisiertes Deployment werden nicht eingerichtet.

## Implementierungsansatz

1. Die bestehende MAUI-App wird auf eine MVVM-Shell mit klaren Routen für Verbindungssuche, Ergebnisse und Details umgestellt. DI registriert Views/ViewModels zusätzlich zu den bereits registrierten Core-Services.
2. Start und Ziel akzeptieren jeweils einen freien Suchtext für Adresse/Haltestelle und eine gültige WGS84-Koordinate. Suchtexte werden über `IStopSearchService.SearchAsync` aufgelöst; mehrere Treffer werden als fachliche Datensätze mit Name, Provider-/Stop-ID und Koordinate angezeigt und explizit vom Nutzer ausgewählt. Koordinaten werden validiert. Ungültige Eingaben erzeugen einen verständlichen Validierungszustand.
3. Erst nach eindeutiger Auswahl beider Endpunkte wird `IRoutingService.RouteAsync` mit abbrechbarem `CancellationToken` aufgerufen. Die bereits vorhandene „neuere Anfrage verdrängt ältere Anfrage“-Logik verhindert, dass veraltete Ergebnisse eine neue Auswahl überschreiben. Lade-, Leer-, Providerwarnungs-, Fehler-, Fallback- und veraltete-Cache-Zustände werden in der UI sichtbar gehalten.
4. Ergebnisse werden zeitlich sinnvoll geordnet dargestellt und enthalten, soweit geliefert, Umstiege, Fußwege, Linien, Betreiber sowie geplante und tatsächliche Zeiten einschließlich Tageswechsel und Zeitzone. Fehlende Werte bleiben unbekannt. Ein Ergebnis öffnet eine Detailansicht mit den Teilstrecken und ihren Ereignissen; Rücknavigation erhält den Suchkontext.
5. `ProviderOrchestrator` bleibt die zentrale Stelle für die Einordnung mindestens eines Endpunkts in NRW, regionale Priorität, bundesweite Ergänzung/Fallback, eindeutige Fahrt-/Haltestellenzuordnung, Warnungen, Quelle, Datenalter und Cachemarkierungen. Es werden keine Echtzeitwerte erfunden und keine Fahrten verschiedener Anbieter vermischt.
6. Die Gestaltung orientiert sich an den auswertbaren HTML-/Markdown-Referenzen des Designentwurfs. Die unbrauchbaren Screen-PNGs sind keine Abnahmevorlage. Statusinformationen werden zusätzlich textlich oder über Symbole vermittelt und nicht ausschließlich über Farbe.
7. iOS wird im Projekt als Zielplattform und mit den nötigen Einstieg-/Plattformdateien eingerichtet. Die lokale native iOS-Abnahme (Build, Gerät/Simulator, UI und Plattformverhalten) übernimmt zunächst der Nutzer; sie ist kein lokaler Abschlussblocker. Eine manuelle Prüfanleitung wird aktualisiert. Unter Windows werden alle manuellen Suchabläufe soweit technisch möglich tatsächlich als native UI-E2E ausgeführt; jeder nach ernsthaftem Versuch nicht ausführbare Fluss erhält Ursache und Versuchsnachweis.
8. Bestehende Windows-Format-, Build-, Test- und Releaseabläufe in GitHub Actions bleiben erhalten; Warnungen sind Fehler, die Repository-Coverage-Anforderung von mindestens 70 % bleibt maßgeblich. Nach fachlicher Abnahme wird ein geprüfter Windows-ZIP-Stand mit Startanleitung, Commit/Version, Umfang und Grenzen über die autorisierte IIS-Website „ÖPNV“ bereitgestellt; Site-Zuordnung und Zielpfad werden vorher geprüft, Download und lokaler Start nachgewiesen. IIS verteilt nur Paket und Unterlagen, die App bleibt nativ.

## Konfiguration

Provider-Endpunkte, gegebenenfalls Zugangsdaten, Zeitlimits, Wiederholungen, Ergebnis- und Cachegrenzen bleiben über die vorhandene sichere Konfiguration austauschbar. Die bestehende bundesweite Versorgung und NRW-EFA-Ausprägung werden verwendet; Produktivfreigaben und vollständige Abdeckung werden nicht vorausgesetzt. Manuelle Suche benötigt keine Standortberechtigung. Es wird keine personenbezogene Adress-, Such- oder Standortchronik gespeichert; Diagnose enthält keine Suchtexte, Adressen, Standortdaten, Antwortinhalte oder Geheimnisse. Cache, gemeinsame Abrufe, Abbruch und begrenzte Wiederholungen gelten unverändert.

## Akzeptanzkriterien

1. Vorlagenbegrüßung, Counter und zugehörige Logik sind entfernt. iOS-Target, Einstieg und Plattformdateien sind eingerichtet, Windows bleibt baubar. Navigation führt zu nutzbarer Suche, Ergebnissen und Details ohne unfertige Menüziele; Visual-Studio-Entwicklung ist dokumentiert.
2. Adresse, Haltestelle und gültige Koordinate funktionieren jeweils als Start und Ziel. Mehrdeutige Treffer sind auswählbar; die Auswahl identifiziert den fachlichen Datensatz über bestehende Stop-/Providerdaten und nicht nur über eine interne Eingabe. Ungültige Eingaben sowie Lade-, Leer- und Fehlerzustände sind verständlich. Veraltete Anfragen überschreiben keine neuen Ergebnisse. Manuelle Suche fordert keine Standortberechtigung an.
3. Verbindungen sind zeitlich sinnvoll geordnet und zeigen Umstiege, Fußwege, Linien, Betreiber und gelieferte Soll-/Ist-Zeiten einschließlich Tageswechsel und Zeitzone. Fehlende Echtzeit, Ausfall, Quelle und veralteter Cache bleiben nachvollziehbar; fehlende Werte werden nicht erfunden. Verbindungsdetails sind erreichbar und rückwärts navigierbar.
4. Die UI integriert reale bundesweite und regionale Services. Tests belegen NRW-Priorität für einen oder beide Endpunkte, Verhalten außerhalb NRW, eindeutige ID-Zuordnung, Ergänzung eindeutiger Fahrten, Warnungen sowie leere/fehlerhafte Rückfälle ohne Fahrtvermischung. Bundesweite und NRW-Live-Verbindungsproben werden separat mit Datum, Ergebnis und Grenzen dokumentiert; Fixtures sind kein Live-Nachweis.
5. Native Windows-UI-E2E bedienen alle manuellen Eingabearten, Trefferwahl, Ergebnisse, Details, Rücknavigation und Fehlerzustände tatsächlich, soweit technisch möglich. Eingabe- und Zustandslogik ist deterministisch geprüft. Sichere Konfiguration, Datenquellen, Cache-/Datenschutzgrenzen, Plattformgrenzen und manuelle iOS-Prüfung sind dokumentiert. Nicht ausführbare Windows-Flüsse erhalten nach ernsthaftem Versuch konkrete Ursache und Versuchsnachweis. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage werden geprüft.
6. Nach fachlicher Abnahme steht der erste bedienbare Windows-Stand als geprüfter ZIP über die autorisierte IIS-Website „ÖPNV“ bereit. Startanleitung, Commit/Version, Umfang, Grenzen, Site-Zuordnung, Zielpfad, Download und lokaler Start sind nachgewiesen; es wird keine Web-App oder weitere Veröffentlichung zugesagt.

## Offene Fragen

Keine fachlichen Entscheidungsfragen. Konkrete technische Werte und Providerfreigaben werden anhand der vorhandenen Konfiguration und Live-Proben festgelegt bzw. dokumentiert. Die bestätigte Prüfaufteilung (Windows-UI soweit möglich tatsächlich ausführen, native iOS-Abnahme zunächst beim Nutzer) und die IIS-Zwischenlieferung sind autorisiert und benötigen keine erneute Rückfrage.
