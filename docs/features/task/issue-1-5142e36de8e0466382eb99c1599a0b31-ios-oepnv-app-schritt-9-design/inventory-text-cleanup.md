# Textinventar: Bereinigung dauerhafter Hinweise

Stand: 4. Oktober 2026

Die Anwendung soll im Normalfall die nächsten Abfahrten, Suchfelder und Ergebnisse zeigen. Dauerhafte Betriebs- und Verwaltungsinformationen werden nur angezeigt, wenn sie eine Handlung auslösen oder die Aussage der sichtbaren Daten einschränken.

## Startseite

| Fundstelle | Sichtbarer Inhalt | Bewertung | Empfehlung |
|---|---|---|---|
| `HomePage.cs:44` | „Abfahrten“ | Seite und Tab benennen den Bereich bereits. | Entfernen. |
| `HomePage.cs:45` | „Deine gespeicherten Stationen und Abfahrten auf einen Blick.“ | Einmalige Erläuterung ohne Handlungswert. | Entfernen. |
| `HomePage.cs:57-66` / `RefreshIntervalStatus` | Aktuelles Aktualisierungsintervall | Dauerhafte Konfiguration, für die tägliche Nutzung nicht relevant. | Aus dem Startseiten-Panel entfernen; in den Einstellungen belassen. |
| `HomePage.cs:59-66` / `HomeStatus` | Allgemeiner Lade-/Speicherstatus | Während Laden oder Fehler nötig, nach erfolgreichem Laden nicht. | Nur bei laufendem Ladevorgang oder Fehler sichtbar machen. |
| `HomePage.cs:61,79,96` / `FavoriteCount` | „N Favoriten gespeichert“ | Der Nutzer kennt seine gespeicherten Favoriten; das Panel bringt keinen Nutzwert. | Entfernen. |
| `HomePage.cs:62-66` / `HomeLocationStatus` | Standort-, Sortier- und Genauigkeitshinweise | Beim aktiven Aktualisieren bzw. wenn Distanzen nicht ermittelbar sind relevant; sonst nicht. | Nur als temporären Status bei Standortaktion oder bei einem Fehler anzeigen. |
| `HomePage.cs:63-66` | Umrandetes Übersichts-Panel | Es bündelt ausschließlich die obigen Verwaltungsinformationen. | Vollständig entfernen. |
| `HomePage.cs:68-69` / `NearbyHeading`, `NearbyStatus` | „Nächste Haltestellen“, Anzahl weiterer Haltestellen | Überschrift hilft der Gliederung. Die Anzahl wiederholt die sichtbare Liste. | Überschrift beibehalten; Anzahl bei erfolgreicher Liste ausblenden. Fehlermeldung und „keine …“ weiter anzeigen. |
| `HomePage.cs:222-225` / `FavoriteMetadata{n}` | Quelle, Zeitpunkt und technische Metadaten je Favorit | Der Datenstand kann die Verlässlichkeit der Abfahrten einordnen; technische Angaben dürfen aber nicht die Karte dominieren. | Nur eine knappe Datenstand-/Quellenzeile zeigen, wenn sie tatsächlich vorhanden oder relevant ist; keine technischen Kennungen. |
| `HomePage.cs:208-211` / `FavoriteStatus{n}` | Aktualisierungsstatus der einzelnen Karte | „Abfahrten werden aktualisiert“, Fehler und leere Ergebnisse sind handlungsrelevant. Erfolgsmeldungen auf jeder Karte sind Lärm. | Nur Busy-, Fehler- und Leerzustand anzeigen; Erfolgsmeldung nach Abruf unterdrücken. |

## Weitere dauerhaft sichtbare Erläuterungen

| Fundstelle | Sichtbarer Inhalt | Bewertung und Empfehlung |
|---|---|---|
| `RefreshSettingsPage.cs:48,52-53` | Mehrere lange Erklärtexte zur Aktualisierung, iOS-Hintergrundverhalten und Standardwerten | Der eingestellte Wert und eine Rückmeldung beim Speichern genügen im Normalfall. Die Mindestgrenze kann als kurze Eingabehilfe bleiben. Den einleitenden Absatz, die iOS-Einschränkung und die Wiederholungen entfernen oder in eine optionale Hilfe verschieben. |
| `MapPage.cs:51-55` / `MapMetadata`, Kartenanbieter-Hinweis, Segmentpunktzahl | Technische Herkunft und Punktanzahl sind für den Fahrgast nicht entscheidungsrelevant. | `MapMetadata`, „Kartendaten …“ und „gelieferte Punkte“ nicht dauerhaft anzeigen. Einen Lizenz-/Attributionstext nur behalten, wenn rechtlich erforderlich. Fehler- und Datenverfügbarkeitsstatus bleiben sichtbar. |
| `SearchPage.cs:116-117` / `OriginSelection`, `DestinationSelection`, `OriginMetadata`, `DestinationMetadata` | Gewählter Halt und technische Zusatzinformationen unter den Feldern | Nach einer Auswahl soll der sichtbare Text im Eingabefeld stehen; eine zweite Auswahlzeile wiederholt ihn. Kennungen sind nicht nutzerrelevant. | Auswahl- und Metadatenzeilen nach erfolgreicher Auswahl ausblenden. Status bei Suche, Fehler oder unklarer Auswahl beibehalten. |
| `SearchPage.cs:68-69` / `RoutingStatus`, `RoutingMetadata` | Routingzustand und technische Providerinformationen | Ladezustand, Validierungsfehler und „keine Verbindung“ sind relevant. Metadaten sind es im Erfolgspfad nicht. | Status kontextabhängig zeigen; Metadaten im Erfolgspfad ausblenden. |
| `ResultsPage.cs:26-27`, `JourneyDetailPage.cs:37-39` | Wiederholte Status- und Metadatenzeilen | Ergebnis- oder Detailkarten sollen die Verbindung zeigen. | Nur Fehler, Laden und fehlende Daten anzeigen; technische Metadaten aus dem Erfolgspfad entfernen. |
| `DeparturePage.cs:77,82,84` / `RefreshIntervalStatus`, `MonitorStatus`, `MonitorMetadata` | Intervall, Monitorstatus und Metadaten | Datenstand und Fehler können relevant sein; das allgemeine Intervall und technische Metadaten nicht bei jeder Nutzung. | Intervall nur in Einstellungen; Status nur für Laden/Fehler/keine Abfahrten; Metadaten auf eine kurze Datenstand-/Quellenzeile beschränken. |
| `StopSearchPage.cs:49,56,59` / `NearbyStatus`, `StopSearchStatus`, `StopSearchMetadata` | Such- und technische Zusatzinformationen | Während Suche und bei Fehlern nötig. Technische Metadaten nicht. | Status bedingt anzeigen; Metadaten im Erfolgspfad entfernen. |

## Beibehalten

Diese Texte sind während eines konkreten Zustands nötig und sollen nicht pauschal entfernt werden:

- Ladehinweise, solange ein Abruf läuft.
- Verständliche Fehler mit einer konkreten nächsten Handlung, etwa erneutes Laden oder Wechsel zur Liste.
- Hinweise, dass keine Abfahrten, Haltestellen oder Verbindungen gefunden wurden.
- Datenstand bzw. Quelle, falls sie die Aktualität oder Belastbarkeit der angezeigten Abfahrten tatsächlich einschränken.
- Beschriftungen und barrierefreie Beschreibungen für reine Symbolbuttons. Diese können visuell unsichtbar bleiben, müssen für Screenreader erhalten werden.
