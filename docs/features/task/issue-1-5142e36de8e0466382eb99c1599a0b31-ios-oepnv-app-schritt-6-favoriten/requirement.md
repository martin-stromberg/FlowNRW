# Schritt 6: Favoriten und nach Entfernung sortierte Startseitenmonitore

**Kundenanforderung:** Ermögliche technische Haltestellenfavoriten und zeige ihre Abfahrtsmonitore auf der Startseite. Bei erlaubtem verfügbarem Standort werden sie nach Entfernung sortiert. Manuelle Aktualisierung bleibt verfügbar; automatische Intervalle folgen in Schritt 7.

**Betroffene Bereiche:** Favoritenpersistenz, Startseite, Entfernungssortierung, UI-Tests.

**Abhängigkeiten:** 3, 5.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Keine lokale IIS-Präsentation oder gesonderte Zwischenpaket-Bereitstellung.

**Akzeptanzkriterien:**

1. Favoriten lassen sich in der Stations-/Monitorbedienung hinzufügen und entfernen, vermeiden Duplikate und überleben Neustarts. Nur erforderliche technische Stationsdaten werden gespeichert.
2. Die Startseite zeigt Favoritenmonitore aufsteigend nach Entfernung bei verfügbarem erlaubtem Standort. Ohne Standort gilt eine stabile Reihenfolge ohne erfundene Entfernung. Leere Favoriten bieten einen verständlichen Weg zur Haltestellensuche.
3. Monitore erhalten Echtzeit-/Fehler-/Cachezustände und manuelle Aktualisierung. Identische Abrufe werden vermieden; einzelne Ladezustände blockieren nicht die gesamte Startseite. Navigation zu Monitor, Routing und Karte bleibt konsistent.
4. Automatisierte Tests prüfen Persistenz, Duplikate, Entfernungssortierung und fehlenden Standort. Native Windows-UI-E2E bedienen Hinzufügen → Startseite, Neustart/Persistenz, Entfernen, Leerzustand, Sortierung und Navigation.
