# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| AK 1: Intervall wirkt auf aktive Einzel-/Startseitenmonitore und bleibt nach Neustart erhalten; sichere Werte | Globale Auswahl Aus/30/60/120/300, Standard 60; begrenzter atomarer Settingsstore, getrennte Entwurfs-/Wirksamwerte; Settings laden vor Timerstart; sichtbarer Intervallstatus | Core fehlende/kaputte Datei, Wertevalidierung, Fehler und Neustart; native Standard60→30→Prozessneustart, tatsächlicher unbeschleunigter 30-Sekunden-Tick auf Monitor und mehreren Favoriten | Abgedeckt |
| AK 2: Navigation/Intervallwechsel stoppen alte Arbeit, keine Doppelabrufe/Anfragefluten, inaktive Seiten ohne Schleife | Idempotenter RefreshLoop, vollständiges Delay nach Abschluss/Fehler, Stop/Dispose/Revision, eigene Schleife je Karte, gemeinsame Seiten-/Fensteraktivität; gemeinsamer Busy-Schutz; vorhandene Providerzusammenfassung und begrenzte Retries | Deterministische Takt-/Wechsel-/Abbruch-/Busy-/Parallelitätsprüfungen; native langsame automatische Anfrage plus manuelle Aktion, Aus länger als30 Sekunden, Navigation, Deaktivierung/Reaktivierung und kein doppelter Takt | Abgedeckt |
| AK 3: Manuelle Aktualisierung, ehrliche Daten-/Fehler-/Cachezustände, begrenzte Logs/Cache | Vorhandener manueller Pfad bleibt; automatisch ausgelöster Refresh erhält passenden Status; bestehende Services, Cache und sichere Fehlerdarstellung bleiben; Fehler behalten Daten | Core Fehlerretention/kein Sofortretry; native Fehler mit sichtbaren letzten Daten und manueller Erholung; bisherige Monitorregression | Abgedeckt |
| AK 4: Deterministische und echte native Nachweise, bestehende Abläufe | Injizierbare Verzögerung für Coretests, ausschließlich produktive Zeitabstände im nativen Test; isolierter UiTest-Settingspfad, bisherige Modi mit Aus; Navigation und Settings-UI mit festen IDs | Native Einstellungen, Neustart, Speicherfehler/Retry, echte Taktung, manuelle Aktion, unabhängige Tafeln und Navigation; Routing-/Favoriten-/Monitorregression, schmale Ansicht/Tastatur | Abgedeckt |
| Verbindliche Rahmenbedingungen | MVVM/DI, keine Zusatzpakete, kleine technische Persistenz, keine Nutzerhistorie; Windows-Actions erhalten, kein IIS/Deployment/iOS-CI; Hintergrund in Schritt8 | Gesamte Tests/Coverage/Build/Format/XML, aktualisierte Hilfe und native iOS-Anleitung beim Nutzer | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

Keine.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| Settings öffnen, wählen und speichern / AK1 | Home→Settings, Standard60 sehen,30 wählen/speichern, echter Prozessneustart erhält30 | Abgedeckt |
| Automatischer Einzelmonitor / AK1,4 | Initiale Abfahrten sichtbar; nach echten30 Sekunden geänderter Fixturestand/Abrufzähler ohne Refreshklick | Abgedeckt |
| Automatische Favoriten / AK1,2 | Mindestens zwei Tafeln aktualisieren unabhängig | Abgedeckt |
| Manueller/automatischer Parallelaufruf / AK2,3 | Langsame automatische Anfrage, manuelle Aktion, nur ein tatsächlicher Abruf | Abgedeckt |
| Aus/Intervallwechsel/Wiederaktivierung / AK2 | Aus über mehr als30 Sekunden; erneutes Einschalten und Intervallwechsel ohne doppelten Takt | Abgedeckt |
| Navigation/Fensteraktivität / AK2 | Suche oder Settings stoppt Tafeln; Deaktivierung stoppt; Aktivierung genau eine Schleife | Abgedeckt |
| Speicherfehler/Wiederholung / AK1 | Wirksamer alter Wert bleibt bei Speicherfehler; Retry und Neustart prüfen erfolgreichen neuen Wert | Abgedeckt |
| Anbieterfehler und manuelle Erholung / AK3 | Letzte bekannte Daten behalten, danach manuell aktualisieren | Abgedeckt |
| Erreichbarkeit/Regression / AK4 | Tastatur/schmale Ansicht; vorhandene Routing-/Favoriten-/Monitorflüsse | Abgedeckt |
| Ungültige freie Eingabe / AK1 | Keine freie Zahleneingabe, nur erlaubte Pickerwerte; gespeicherte ungültige Daten zusätzlich im Core geprüft | Abgedeckt |

## Fehlende oder unvollständige Planbestandteile

Keine.

## Hinweise

Unabhängige Planprüfung am 19.09.2026 gegen vollständige requirement.md, inventory.md und plan.md. Die in der Bestandsaufnahme erwähnte inventory-Unterablage enthielt beim Lesen keine Dateien; die historische Ausgangstestzahl ist daher keine selbst nachgeprüfte neue Ausführung. Der Plan setzt eigene abschließende Checks voraus.

Tatsächlicher Codekontext zusätzlich bestätigt: App.CreateWindow erzeugt bislang nur Window(shell); OpenStopAsync merkt seine Revision vor der Navigation und beginnt danach den ersten Refresh. Die ausdrücklich geplante Erstaktivierung ohne Cancel verhindert dessen unbeabsichtigtes Unterdrücken. Die vorhandene Echtzeit-Cachefrist beträgt30 Sekunden und stützt die dokumentierte Mindestwahl. UI-Kontext, explizite Unterscheidung von Fenstervordergrund und Seitenaktivität, Abbau von Abonnements sowie vor Timerstart geladene Einstellungen sind im Plan berücksichtigt.

Für identische Abrufe verspricht der Plan ausschließlich gemeinsame Busy-Abwehr derselben Tafel und die bestehende Providerzusammenfassung exakt gleicher Schlüssel; verschiedene Anfragezeitpunkte werden nicht fälschlich als gemeinsam behandelt. Completion-relative Wartezeiten verhindern Aufholstau und sofortige Fehlerschleifen. Native unbeschleunigte Zeitmessung bleibt der primäre Funktionsnachweis; Core-Fakedelay ersetzt sie nicht.

Keine fachliche Rückfrage erforderlich. Routineentscheidungen wie konkrete innere Schedulerfelder bleiben der Implementierung überlassen. Keine Produktänderungen, Commits oder Testprozesse gestartet.
