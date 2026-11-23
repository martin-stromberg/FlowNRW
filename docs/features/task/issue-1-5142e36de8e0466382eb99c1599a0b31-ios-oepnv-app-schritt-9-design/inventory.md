# Bestandsaufnahme – Designintegration

Stand: Basis74a052f, Schritte1–8 integriert. Keine neue Providerimplementierung erforderlich.

## Produkt

- AppShell hat eine ShellContent-Startseite und globale Drilldown-Routen. Drei persistente Kernbereiche fehlen. SearchPage/StopSearchPage/HomePage sind DI-Singletons; dieselbe Page darf nicht gleichzeitig als Tab und globale Push-Seite verwendet werden.
- HomePage und DeparturePage rendern mehrzeilige DeparturePresentation.Describe-Labels; Ergebnisliste besteht aus großen Summary-Buttons, Details aus Textblöcken. Datenmodelle enthalten echte Linien, Ereignisse, Soll/Ist, Ausfall, Betreiber und Geometrie.
- SearchPage besitzt getrennte EndpointViewModels samt Namenstreffern, Koordinaten und explizitem Standort. StopSearchPage nutzt dieselben geprüften Such-/Nearby-Modelle; keine Kennungseingabe nötig.
- MapPage enthält HybridWebView, sichere JSON-Bridge, native Liste/Markerwahl, begrenzte Tiles, statische Snapshotgeometrie und Abbruch bei Pause. Karte bisher feste430Höhe innerhalb langer vertikaler Seite.
- RefreshSettingsPage besitzt native Auswahl/Speichern/Fehlerstatus. RefreshLifecycle/RefreshLoop und Seitenabos sind seit Schritt8 geprüft und müssen erhalten bleiben.
- Colors.xaml enthält teilweise Projektblau, aber Restfarben sind MAUI-Template. Styles.xaml verwendet OpenSans14, rechteckige Borders, ein gemischtes Dunkelthema und magentafarbene Tabressourcen. Designreferenz verlangt andere Flächen/Hierarchie/Radien.

## Wiederverwendung

Core/Transit liefert Journey/Leg/Line/StopEvent/RealtimeStatus; keine Darstellung darf fehlende Echtzeit ergänzen. DeparturePresentation/JourneyPresentation sind Referenz für vollständige fachliche/zugängliche Texte. AsyncRelayCommand/RelayCommand, bestehende Status-/Metadata-Bindings und Command-Abkopplung beim Entfernen bleiben erhalten.

Referenzen: docs/design/reference/trans_nrw_mobil_system/DESIGN.md, flownrw_app_flow_dokumentation.md, vier code.html und Originalicon. Konflikte sind in docs/design/acceptance.md bereits aufgelöst (drei Kernbereiche, keine Tickets/aktive Reisebegleitung). HTML verwendet externe Tailwind-/Fontquellen; Referenzrendering von Produktauslieferung trennen. Originalzip nicht verändern.

## Prüfgrundlage

Unmittelbar vor Schritt9 bestanden:236Coretests/93,15Prozent, Release/UiTest0/0, iOSCompile0/0, Format/XML/26Release-Skripttests. Alle5Windows-Modi, echterIntervalllauf und erweiterterLifecyclelauf erfolgreich; dauerhafte Belege docs/help/monitorintervalle/verification-lifecycle.

WindowsJourneyUiTests, WindowsRefreshUiTests und WindowsLifecycleUiTests verwenden bekannte AutomationIds, Namen und Zurückfolgen. Strukturänderungen dürfen diese Assertions nicht einfach entfernen; semantische Feldprüfungen/Tabflüsse gezielt anpassen. End-to-End-Nachweis bleibt nativ. Screenshotsteuerungen fehlen bisher (Thema, Schrift, Ausblenden der Fixturefelder); nur UiTest darf diese erhalten.

## Risiken

Tabwechsel/Drilldown müssen Lebenszyklusabos und Sucheingaben erhalten, ohne eine Page doppelt einzuhängen. Große Schrift braucht flexible Zeilen/Karten und verfügbare Scrollflächen. Metadaten dürfen kompakter aussehen, aber nicht verloren gehen. Dunkle Farben benötigen eigene Kontrastwerte. Originaldraft enthält fiktionale Zusatzfunktionen, die nicht übernommen werden. Unabhängige visuelle Abnahme muss gesondert stattfinden; Agentenlimit ist keine bestandene Prüfung.