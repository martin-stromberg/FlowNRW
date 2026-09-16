# Anforderung Schritt 3

### Schritt 3: Haltestellensuche und manuell aktualisierter Abfahrtsmonitor

**Kundenanforderung:** Ergänze die Verbindungssuche um eine bundesweite manuelle Haltestellensuche. Die gewählte Haltestelle öffnet einen Monitor mit realen nächsten Abfahrten und manueller Aktualisierung. GPS, Favoriten und automatische Intervalle folgen getrennt in Schritten 5–7.

**Betroffene Bereiche:** Haltestellensuche, Monitor, Echtzeitstatus, Navigation, UI-Tests.

**Abhängigkeiten:** 2.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Keine lokale IIS-Präsentation oder gesonderte Zwischenpaket-Bereitstellung.

**Akzeptanzkriterien:**

1. Bundesweit gesuchte Haltestellen sind eindeutig auswählbar und öffnen den richtigen Monitor. Lade-, Leer-, Fehlerzustände und Rücknavigation funktionieren ohne Standortzugriff.
2. Abfahrten zeigen Linie/Ziel, Soll-/Ist-Zeiten, Verspätung, Ausfall und Gleis/Steig einschließlich Änderungen. Fehlende Echtzeit bleibt von pünktlicher Echtzeit unterscheidbar; Ausfälle erscheinen nicht als regulär fahrend.
3. NRW-Echtzeit und bundesweite Ergänzungen werden aus dem vorhandenen Datenkern korrekt angezeigt. Leere/partielle Antworten, Mehrdeutigkeiten, Anbieterfehler, Quelle und Datenalter sind nachvollziehbar. Manuelle Aktualisierung blockiert die UI nicht; bei Fehlern bleiben letzte bekannte Daten erkennbar statt kommentarlos zu verschwinden. Identische Abrufe werden zusammengefasst.
4. Automatisierte Tests prüfen Soll-/Ist-Anzeige, Ausfall/Gleiswechsel, Fehler/Fallback und manuelle Aktualisierung. Native Windows-UI-E2E bedienen Suche → Monitor, Aktualisieren und Fehlerzustände; Routing bleibt funktionsfähig. Ein NRW-Live-Abfahrtsabruf wird separat mit Ergebnis und Grenzen dokumentiert.
