# Umsetzungsplan: Hintergrundaktualisierung und Wiederaufnahme

## Übersicht

Aktive Echtzeitansichten erneuern veraltete Daten nach Wiederaufnahme, ohne Navigation oder Suchauswahl zu ändern. iOS erhält eine registrierte, zeitlich begrenzte BGAppRefreshTask für gespeicherte Favoriten. Bestehende Abbruch-, Busy-, Cache- und Fehlerregeln bleiben die gemeinsame Datenversorgung; es gibt weder GPS im Hintergrund noch eine neue Datenhistorie.

## Designentscheidungen

| Bereich | Ansatz | Begründung |
|---|---|---|
| Frische | Gemeinsame RefreshFreshness-Regel mit TransitCacheOptions.RealtimeTimeToLive und injizierbarer Uhr | Gleiche technische Frischegrenze wie der vorhandene Echtzeitcache; deterministisch testbar. IsStale, fehlende und unplausibel zukünftige Zeitstempel gelten als erneuerungsbedürftig. |
| Lebenszyklus | Singleton RefreshLifecycle besitzt Hintergrundlauf und steuert den bestehenden ForegroundState | Bei Aktivierung zuerst Hintergrundlauf abbrechen, danach Vordergrund freigeben. Keine zweite periodische Hintergrundschleife. |
| Resume | Sichtbare Seiten rufen ihre vorhandenen Modelle über einen abbrechbaren, idempotenten Resume-Pfad auf | Kein Refresh verborgener Such-/Kartenseiten. Erststart und frisch geladene Daten verursachen keinen Zusatzabruf. |
| Hintergrunddaten | Bestehende Favoritenmodelle laden, veraltete Karten mit höchstens vier parallelen Requests erneuern | Resultate sind im bestehenden flüchtigen Sitzungsmodell nutzbar; keine zusätzliche Persistenz von Abfahrten, Routen oder Positionen. |
| Begrenzung | Höchstens 20 Sekunden pro Hintergrundlauf einschließlich Laden; vorhandenes Maximum 100 Favoriten | Endliche Arbeit auch bei vielen Favoriten. Token und Revision verwerfen verspätete Antworten. Provider-Timeouts und Retrygrenzen bleiben bestehen. |
| Nutzerwahl | Aus deaktiviert zusätzliche automatische Resume-/Hintergrundabrufe | Manuelle Aktualisierung und bisheriger initialer Seitenabruf bleiben möglich. Keine neue Einstellung erforderlich. |
| Verbindungsergebnisse | Aktuelle Suche ohne Navigation neu laden; ausgewählte Fahrt nur bei eindeutiger technischer Identität erhalten | Keine erfundene Fahrtzuordnung und keine unerwartete Navigation. Fehlende/mehrdeutige Fahrt wird ausdrücklich als nicht mehr bestätigt angezeigt. |
| Plattform | BGTaskScheduler/BGAppRefreshTaskRequest nur im iOS-Target | Windows-CI bleibt unabhängig von Apple. Native iOS-Abnahme bleibt beim Nutzer. |

## Programmabläufe

### Aktivierung, Pause und Navigation

1. App/CreateWindow leitet Activated, Deactivated, Stopped, Resumed und Destroying an RefreshLifecycle weiter; doppelte gleichartige Ereignisse bleiben idempotent.
2. Bei Deaktivierung stoppt ForegroundState die Timer. Sichtbare Seiten brechen zusätzlich laufende Resume-/manuelle Requests ab, auch bei Intervall Aus. Standortabfragen werden beendet; bei Wiederaufnahme erfolgt keine automatische Standortanforderung.
3. Bei Aktivierung beendet RefreshLifecycle Hintergrundarbeit vor Freigabe des Vordergrunds. Nur eine tatsächlich sichtbare Seite prüft ihre Resultate auf Frische.
4. DeparturePage und HomePage verwenden vorhandene RefreshAutomaticallyAsync-Pfade mit Busy-/Revisionsschutz. Nach dem Resume-Abruf laufen dieselben completion-relativen Timer weiter; keine Aufholserie.
5. ResultsPage und JourneyDetailPage erneuern die bestehende JourneySearchViewModel-Sitzung ohne ShowResultsAsync oder andere Navigation. Die aktive Auswahl wird anhand eindeutiger Fahrtidentitäten abgeglichen. Route, Details und gegebenenfalls die aktuell dargestellte Route werden über bestehende PropertyChanged-Bindungen aktualisiert; bei unsicherer Identität wird keine neue Fahrt als die alte ausgegeben.
6. OnDisappearing beendet seiteneigene Abos/Requests und verhindert spätes Rendern. Schnelle Wiederaktivierung, wiederholte Events und manuelle Aktualisierung teilen dieselbe Busy-Sperre.

### Begrenzter Hintergrundlauf

1. IosBackgroundRefresh registriert genau eine feste Kennung vor Ende des iOS-Starts und meldet bei Eintritt in den Hintergrund eine Anfrage an. Bereits vorgemerkte Anfragen derselben Kennung werden ersetzt; frühester Beginn nach 15 Minuten ist eine Untergrenze, keine Ausführungsgarantie.
2. Der Callback setzt sofort ExpirationHandler und startet die begrenzte asynchrone Arbeit auf dem UI-Kontext, auf dem die bestehenden Modelle leben. Dienste werden aus DI aufgelöst; es wird keine zweite App aufgebaut.
3. RefreshLifecycle verweigert parallele Hintergrundläufe und Arbeit bei aktiver App. Einstellungen werden geladen; Aus führt zu keinem Providerabruf.
4. Favoriten werden aus dem bestehenden Store geladen. Nur aktuelle, veraltete Favoriten werden erneuert, maximal vier gleichzeitig; kein Standort, keine Navigation, kein Suchverlauf. Entfernte Karten und verspätete Antworten bleiben durch Identitäts-/Revisionsschutz ausgeschlossen.
5. Expiration, 20-Sekunden-Budget oder Vordergrundwechsel brechen den Lauf ab. Nicht kooperative Provider dürfen die Abschlussmeldung nicht unendlich blockieren und keine verspäteten UI-Ergebnisse übernehmen.
6. SetTaskCompleted wird genau einmal aufgerufen. Fehler, deaktivierte iOS-Hintergrundaktualisierung und fehlgeschlagene Planung werden ohne sensible Werte behandelt; eine folgende Anfrage wird ohne enge Wiederholungsschleife geplant.

### Sichtbare Hinweise

Vorhandene Status-/Metadata-Labels bleiben primäre Rückmeldung. Die Einstellungsseite erklärt, dass iOS den Zeitpunkt bestimmt und suspendierte Apps keinen festen Takt garantieren. Nach Resume wird das Datenalter neu angezeigt; Fehler behalten letzte bekannte Daten mit Quelle/Alter. Keine neuen Entitätskennungen oder Eingabemasken; vorhandene Picker, beschriftete Buttons, Scrollbereiche und Navigation werden wiederverwendet.

## Neue Klassen

| Klasse | Typ | Zweck |
|---|---|---|
| RefreshFreshness | Core-Hilfsklasse | Einheitliche Frischeentscheidung mit Testuhr. |
| RefreshLifecycle | Core-Service | Vordergrund-/Hintergrundzustand, ein begrenzter Hintergrundlauf, Abbruch und Handoff. |
| IosBackgroundRefresh | iOS-Adapter | Registrierung, Planung, Ablaufbeendigung und DI-/UI-Kontextanbindung. |
| WindowsLifecycleUiTests.ps1 | Nativer Testharness | Sichtbare Resume-, Fehler-, Abbruch- und Koordinationsflüsse mit isolierten Fixtures. |

## Änderungen an bestehenden Klassen

- ForegroundState: vorhandene idempotente Aktivitätsmeldung erhalten; nur nötige Resume-Information ergänzen.
- FavoriteMonitorViewModel und StopMonitorViewModel: abbrechbaren RefreshIfStaleAsync-Pfad ergänzen; automatische Requests nehmen optional einen externen Token auf. Fehler-/Busy-/Revision bleiben gemeinsam mit manuellem Refresh. Metadata kann ohne Providerabruf neu bekanntgegeben werden.
- FavoriteHomeViewModel: RefreshStaleAsync für sichtbare Karten bzw. begrenzten Hintergrundbatch; sicheres Laden und Abbruch ohne Standortzugriff. Keine zusätzlichen Speichervorgänge.
- JourneySearchViewModel: RefreshIfStaleAsync ohne Navigation; Ergebnis-/Auswahlabgleich und Fehlererhalt. ResultsViewModel/JourneyDetailViewModel auf geänderte Auswahl/Metadata prüfen und erforderliche Meldungen ergänzen.
- HomePage, DeparturePage, ResultsPage und JourneyDetailPage: sichtbare Aktivitätsabos, Resume-Aufruf, Abbruch beim Verlassen. Bestehende Command-Abkopplung der Favoritenkarten erhalten.
- SearchPage, StopSearchPage und MapPage: Deaktivierung beendet laufende Standort-/Such-/Kartenarbeit; keine automatische erneute Positionsfreigabe. Bereits geladene Daten dürfen erhalten bleiben.
- App: Windows-/MAUI-Lebenszyklus an RefreshLifecycle delegieren, ohne doppelte Aktivierungen.
- MauiProgram: neue Dienste mit gemeinsamer Uhr/Cacheoptionen registrieren; iOS-Adapter nur plattformspezifisch.
- AppDelegate/Info.plist: BGTaskScheduler-Registrierung und UIBackgroundModes fetch/BGTaskSchedulerPermittedIdentifiers.
- RefreshSettingsPage: knappe, sichtbare Hintergrund-/Resume-Erklärung.
- UiTestFixtureServices: deterministische alte/frische Resultate, langsame/fehlgeschlagene Resume-Antworten und Requestzähler; nur UiTest. Testfelder fehlen in Release.

## Datenbankmigrationen

Keine. Keine neue dauerhaft gespeicherte Echtzeit-, Standort- oder Routenhistorie.

## Validierungsregeln

| Objekt | Regel | Fehlerfall |
|---|---|---|
| Frische | Vorhandene validierte RealtimeTimeToLive; IsStale oder Alter ab TTL erneuern | Frische Daten unverändert; Zukunftszeitstempel nicht unbegrenzt frisch behandeln. |
| Hintergrund | Ein Lauf, vier parallele Requests, 20 Sekunden, höchstens vorhandene 100 Favoriten | Abbruch und spätes Ergebnis verwerfen; keine Anfrageflut. |
| Fahrtidentität | Alle Legs und Provider-/Fahrtidentitäten eindeutig | Alte Fahrt nicht still durch andere ersetzen; verständlicher Status. |
| Einstellungen | Aus/30/60/120/300 wie bisher | Ungültige Speicherung bleibt bestehender sicherer Default/Fehlerhinweis. |

## Konfigurationsänderungen

| Eintrag | Wert | Zweck |
|---|---|---|
| BGTaskSchedulerPermittedIdentifiers | de.martinstromberg.flownrw.refresh | Feste technische Kennung, identisch im Adapter und passend zur ApplicationId. |
| UIBackgroundModes | fetch | Unterstützter iOS-App-Refresh. |
| Interne Laufgrenzen | 20 Sekunden, Parallelität 4, frühester iOS-Beginn 15 Minuten | Endliche opportunistische Arbeit; keine neue Nutzeroption. |

ApplicationId ist de.martinstromberg.flownrw. Die Taskkennung ist kein Benutzerwert. Windows-Workflows bleiben unverändert.

## Seiteneffekte und Risiken

- Aktivierung tritt unter Windows häufiger als echte Suspendierung auf: Frischeprüfung und Busy-Schutz verhindern unnötige Abrufe.
- MAUI-Seiten können mehrfach erscheinen und native Controls abbauen: Abos und Commands dürfen entfernte Controls nicht weiter benachrichtigen.
- Hintergrundarbeit verwendet dieselben Modelle nur auf ihrem UI-Kontext; Token-/Revisionsschutz muss bei Expiration und Handoff gelten.
- Route-/Detaildaten können nach Resume andere Fahrten enthalten: nur eindeutige Identität übernehmen, sonst sichtbarer Verlust der Bestätigung.
- iOS kann Aufgaben nicht ausführen oder deutlich später planen. Registrierung/Code werden geprüft, echte Geräteausführung bleibt ausdrücklich offen beim Nutzer.
- Bestehender ProviderOrchestrator fasst nur exakt gleiche Requests zusammen. Lebenszykluskoordination darf nicht auf zeitlich verschiedenem Request-Key als Duplikatschutz beruhen.

## Umsetzungsreihenfolge

1. Frischeregel und deterministische Tests ergänzen; Voraussetzung: vorhandene ProviderResult/TransitCacheOptions.
2. Abbrechbare, frischeabhängige Modellpfade einschließlich Ergebnis-/Detailabgleich implementieren; Voraussetzung: Schritt1 und bestehende Busy-/Revisionslogik.
3. RefreshLifecycle mit begrenztem Hintergrundlauf und Handoff implementieren/testen; Voraussetzung: Schritt2, bestehender FavoriteStore/Settings.
4. App-/Seitenintegration und UiTest-Fixtures ergänzen; Voraussetzung: Schritt3, vorhandene native MAUI-Seiten und Harness.
5. iOS-Adapter/Plist nach offiziellen APIs implementieren und statisch abgleichen; Voraussetzung: Schritt3 und DI.
6. Native Windows-Resume-Szenarien und integrierte Regressionen ausführen; Voraussetzung: erfolgreicher UiTest-Build.
7. Reviews, vollständiger Releasebuild vor Coretests/Coverage, Format/XML und Release-Skripttests; anschließend Hilfe/README/Release Notes, iOS-Checkliste und Commit.

## Tests

### Neue Tests

| Tests | Inhalt |
|---|---|
| RefreshFreshnessTests | Frisch, TTL-Grenze, veraltet, IsStale, fehlend, Zukunftszeit, Uhr deterministisch. |
| RefreshLifecycleTests | Hintergrund nur inaktiv/einmal, Aus, leere/defekte Favoriten, Parallelität/Budget, Expiration/Handoff, Providerfehler, unkooperative verspätete Antworten, keine Standort-/Navigations-/Persistenzaufrufe. |
| ResumeMonitorTests | Einzel-/Favoriten nur veraltet, manuell/automatisch Busy-Sperre, Fehlererhalt, Navigation/Remove, schneller Aktivitätswechsel. |
| JourneyResumeTests | Keine Navigation, Ergebnisfehler erhält letzte Daten, eindeutige Auswahl bleibt, fehlende/mehrdeutige Identität bestätigt keine falsche Fahrt, geänderte Endpunkte verwerfen alte Antwort. |

### Betroffene bestehende Tests

RefreshLoop-/RefreshMonitor-/Favorite-/StopMonitor-/Journey-Tests behalten ihre bestehenden Zusicherungen. Erwartete Aktivierungsabrufe im Intervallharness müssen um die neue Frischeregel präzisiert werden; keine alten Assertions ohne gleichwertigen Ersatz entfernen. Provider-/Cache-/Standorttests sichern unveränderte Daten- und Datenschutzregeln.

### E2E-Tests (primärer Funktionsnachweis)

| Priorität | Szenario | Datei/Nachweis | Kriterium |
|---|---|---|---|
| Pflicht | Monitor laden, frisch deaktivieren/reaktivieren: kein Zusatzabruf; veraltet wiederaufnehmen: genau ein Abruf und sichtbarer neuer Stand | WindowsLifecycleUiTests.ps1 | Resume/Frische |
| Pflicht | Zwei Favoriten, veraltet wiederaufnehmen, eine langsame/fehlerhafte Karte; andere aktualisiert; alter Stand/Quelle bei Fehler sichtbar | WindowsLifecycleUiTests.ps1 | Unabhängigkeit/Fehler |
| Pflicht | Aus, Resume, manuelle Aktion; kein automatischer Zusatzabruf, manuell erfolgreich | WindowsLifecycleUiTests.ps1 | Einstellung |
| Pflicht | Laufenden Resume verlassen/minimieren, sofort wieder aktivieren; verspätete Antwort überschreibt nichts, keine doppelte Schleife | WindowsLifecycleUiTests.ps1 | Handoff/Navigation |
| Pflicht | Verbindung suchen, Ergebnis/Detail öffnen, veraltet reaktivieren: aktualisierte Daten ohne Seitensprung; nicht eindeutig wiedergefundene Fahrt verständlich markiert | WindowsLifecycleUiTests.ps1 | Ergebnisse/Details |
| Pflicht | Integrierte Routing-, Monitor-, Favoriten-, Karten- und Standortflüsse samt Ablehnung/Entzug, Lade-/Leer-/Fehlerzuständen | WindowsJourneyUiTests.ps1 alle Modi | Gesamtablauf |
| Pflicht | Vorhandene echte Intervalltests angepasst an Resume prüfen; schmale Ansicht, Tastatur, sichtbare Erklärungen/Datenalter | WindowsRefreshUiTests.ps1 und Screenshots | Timer/UI |
| Nutzer iOS | Build/Start, Registrierung, Hintergrund ein/aus, Expiration, Resume während Background, keine GPS-Nutzung, echter Provider-/Offlinefall | Konkrete manuelle Gerätecheckliste | Plattformgrenze |

Windows prüft den Core-Hintergrundkoordinator zusätzlich mit kontrollierten Tokens; daraus wird keine ausgeführte native iOS-Aufgabe abgeleitet. Jeder nicht ausführbare native Windows-Fluss erhält Versuch und konkrete Ursache. Release-Build zeigt keine Fixturebedienung.

## Offene Punkte

Keine fachlichen Rückfragen. iOS-Geräteprüfung ist bereits dem Nutzer zugeordnet. Schritt9 behält die gesonderte visuelle Gesamtabnahme.

## Quellen und Arbeitsmodus

- Apple: https://developer.apple.com/documentation/uikit/using-background-tasks-to-update-your-app
- .NET-iOS-Bindings: https://learn.microsoft.com/en-us/dotnet/api/backgroundtasks.bgtaskscheduler
- https://learn.microsoft.com/en-us/dotnet/api/backgroundtasks.bgapprefreshtaskrequest

25.09.2026: Delegierte Agenten nach abgeschlossener Bestandsaufnahme am Nutzungslimit ausgefallen. Planung und folgende Gegenprüfung werden als getrennte lokale Phasen gemäß Skillfallback geführt; eine lokale Gegenprüfung ist keine unabhängige Agentenprüfung.
