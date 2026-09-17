### Schritt 4: Interaktive Haltestellenkarte und gelieferte Linienverläufe

**Kundenanforderung:** Ergänze eine interaktive Karte für manuell gesuchte Haltestellen und tatsächlich gelieferte Linien-/Verbindungsverläufe. Stationsauswahl öffnet ihren Monitor. Die Karte funktioniert zunächst in einer manuell gewählten Umgebung; GPS-Nahbereich folgt in Schritt 5.

**Betroffene Bereiche:** Karte, Stationsauswahl, Geometrie, Details/Monitor, UI-Tests.

**Abhängigkeiten:** 2, 3.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Keine lokale IIS-Präsentation oder gesonderte Zwischenpaket-Bereitstellung.

**Akzeptanzkriterien:**

1. Gesuchte Stationen stehen an tatsächlichen Koordinaten auf der interaktiven Karte; Auswahl öffnet den richtigen Monitor. Eine zugängliche Listenalternative bietet denselben Weg. Ohne Standort bleibt eine manuell gewählte Umgebung bedienbar.
2. Gelieferte Linien-/Verbindungsgeometrien sind zur gewählten Verbindung/Linie korrekt zugeordnet und auch aus Verbindungsdetails erreichbar. Fehlende Geometrie ist kenntlich statt erfunden. Quelle, Attribution und sichere technische Kartenkonfiguration sind dokumentiert.
3. Die technische Kartenlösung berücksichtigt iOS und Windows. Lade-/Offline-/Anbieterfehler und veraltete Daten sind verständlich; Cache-, Netzwerk- und Datenschutzgrenzen gelten auch für Kartenabrufe. Keine Bewegungshistorie wird gespeichert.
4. Automatisierte Tests prüfen Geometriezuordnung und Stationsauswahl. Native Windows-UI-E2E bedienen Umgebung → Karte → Monitor, Listenalternative, Details → Geometrie, Rücknavigation und Fehlerzustände; Routing/Monitor bleiben nutzbar.
