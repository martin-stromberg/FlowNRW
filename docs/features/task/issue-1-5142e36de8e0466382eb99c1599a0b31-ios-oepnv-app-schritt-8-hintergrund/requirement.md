# Anforderungsübersetzung – Schritt 8: Hintergrundaktualisierung und Wiederaufnahme

## Fachliche Zusammenfassung

Die bestehende ÖPNV-App soll Echtzeitdaten über den unterstützten iOS-App-Lebenszyklus aktualisieren: Die App bindet iOS-Hintergrundaktualisierung mit Registrierung und begrenzter Ausführung an und erneuert beim Wiederaufnehmen veraltete Daten. Vordergrundintervalle, Hintergrundarbeit, Navigation und Wiederaufnahme müssen dieselben Abrufe koordinieren, sodass keine doppelten Aktualisierungsschleifen entstehen. Die vollständigen Kernabläufe werden gemeinsam auf Windows soweit technisch möglich als native UI-E2E geprüft; native iOS-Abnahme verbleibt beim Nutzer und wird durch eine konkrete Anleitung unterstützt.

## Betroffene Klassen und Komponenten

- **Vorhandene Aktualisierungslogik:** `RefreshLoop`, `ForegroundState`, `RefreshSettingsViewModel`, `IRefreshSettingsStore` und `JsonRefreshSettingsStore` in `FlowNRW.Core.Refresh`; Verhalten für Vordergrundintervalle ist in die Lebenszykluskoordination einzubeziehen.
- **Vorhandene Daten- und Anzeigeebene:** `IDepartureService`/`DepartureService`, `ITransitCache`/`MemoryTransitCache`, `StopMonitorViewModel` sowie `FavoriteHomeViewModel` und `FavoriteMonitorViewModel`; diese liefern Aktualisierung und Datenalter für Monitore und Favoriten.
- **Plattformintegration:** iOS-App-Target, iOS-Einstieg und -Lebenszyklus in `FlowNRW/Platforms/iOS`, `App`/`MauiProgram` und die MAUI-Lebenszyklusereignisse. Konkrete iOS-Hintergrund-API und neue Adaptertypen sind Implementierungsentscheidungen und werden hier nicht vorweggenommen.
- **UI:** bestehende Shell-/Navigationsbereiche für Verbindungssuche und Details, Haltestellenmonitor, Karte, Favoriten und Einstellungen; Anzeige des Datenalters sowie Lade-, Leer- und Fehlerzustände.
- **Tests und Nachweise:** Core-Tests für Wiederaufnahme, Koordination, Abbruch, Cachegrenzen, Fehler- und Datenschutzverhalten; native Windows-UI-E2E für integrierte Kernabläufe und Wiederaufnahme, soweit technisch ausführbar; Windows-Release-/Testworkflows.
- **Dokumentation:** Betriebs- und Aktualisierungsgrenzen sowie konkrete manuelle native iOS-Prüfanleitung.

## Implementierungsansatz

Die vorhandene asynchrone, abbrechbare Datenversorgung und der konfigurierbare Vordergrund-Refresh werden wiederverwendet. Eine iOS-spezifische Lebenszyklusanbindung registriert unterstützte Hintergrundarbeit und begrenzt deren Ausführung nach den Plattformregeln; beim Wechsel zurück in den Vordergrund wird anhand Aktualität/Cache erneuert, was veraltet ist. Eine gemeinsame Koordination zwischen Vordergrundtimer, Hintergrundausführung, Resume und Navigation verhindert parallele Duplikatabrufe und beendet oder verwirft veraltete Arbeit sicher. Die UI macht Datenalter und Fehlerzustände verständlich.

Die integrierte Windows-Abnahme führt Verbindungssuche samt Details, Haltestellensuche, GPS, Karte zum Monitor, Favoriten, Einstellungen sowie Lade-/Leer-/Fehlerzustände und Resume soweit technisch möglich tatsächlich aus. Nicht ausführbare Windows-Flüsse werden nach ernsthaftem Versuch mit konkreter technischer Grenze und Versuchsnachweis festgehalten; ViewModel-Tests allein gelten nicht als UI-Nachweis. iOS-Target und Plattformcode werden geprüft, während native iOS-Ausführung ausdrücklich beim Nutzer liegt.

## Konfiguration

Das bereits konfigurierbare Vordergrundintervall und begrenzte Cache-Lebensdauern bleiben maßgeblich. iOS-Hintergrundausführung unterliegt Betriebssystemregistrierung, Laufzeitbudget und opportunistischer Planung; ein garantiertes dauerhaftes Intervall bei suspendierter App ist ausgeschlossen. Die nutzerseitig einstellbare Hintergrundfrequenz ist nicht vorgegeben. Vorhandene sichere Intervall-/Cachegrenzen sind zu bewahren; etwaige iOS-spezifische Registrierungswerte müssen der tatsächlichen Plattformvorgabe entsprechen.

## Offene Fragen

Keine fachlichen Entscheidungsfragen. Die native iOS-Abnahme erfolgt wie vereinbart durch den Nutzer. Vorhandene Apple-Werkzeugketten-/Simulatorgrenzen sind als Prüfgrenze zu dokumentieren, nicht als Implementierungsfreigabe oder bestandener Test auszulegen. Die umfassende visuelle Integration und Designabnahme ist Schritt 9; Schritt 8 integriert die Lebenszyklusfunktion in die vorhandenen Kernansichten, zieht aber die gesonderte Designabnahme nicht vor.

## Abnahmerahmen

- iOS-Hintergrundaktualisierung ist registriert/konfiguriert und führt begrenzte unterstützte Arbeit aus; bei Resume werden veraltete Echtzeitdaten aktualisiert und Datenalter wird angezeigt.
- Vordergrundintervall, Hintergrundarbeit, Resume und Navigation erzeugen keine doppelten Abrufschleifen. Abbruch, Offline-/Providerfehler und Cachegrenzen bleiben korrekt; personenbezogene Historie und sensible Logs werden vermieden.
- Automatisierte Tests decken Resume, Koordination, Fehler und Datenschutzgrenzen ab. Native Windows-UI-E2E decken die integrierten Flüsse samt Resume soweit möglich ab; technische Ausnahmen erhalten konkrete Versuchsbelege.
- Lokale Format-, Build- und relevante Testprüfungen folgen den Repository-Konventionen. Die bestehenden Windows-Release- und Testworkflows bleiben funktionsfähig und unabhängig von Apple-Werkzeugketten. Keine iOS-CI, kein neues automatisiertes Deployment und keine lokale IIS-Präsentation.
- iOS-Code wird geprüft und eine manuelle iOS-Abnahmecheckliste übergeben; native iOS-Build-, Geräte-, Simulator-, UI- und Hintergrundtests werden nicht als lokal ausgeführt behauptet.
- Dauerhafte Dokumentation beschreibt Hintergrundgrenzen, Testnachweise und iOS-Abnahme. Produktweite Designintegration wird in Schritt 9 abgenommen.
