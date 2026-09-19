### Schritt 7: Konfigurierbare automatische Monitoraktualisierung

**Kundenanforderung:** Ergänze eine lokal gespeicherte Intervall-Einstellung für automatische Aktualisierung aktiver Einzel- und Favoritenmonitore im Vordergrund. Manuelle Aktualisierung bleibt möglich. Suspendierung und iOS-Hintergrundbetrieb folgen in Schritt 8.

**Betroffene Bereiche:** Intervall-Einstellung, Vordergrundaktualisierung, Netzlast, UI-Tests.

**Abhängigkeiten:** 3, 6.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Keine lokale IIS-Präsentation oder gesonderte Zwischenpaket-Bereitstellung.

**Akzeptanzkriterien:**

1. Das eingestellte Intervall wirkt tatsächlich auf aktive Einzel-/Startseitenmonitore und überlebt Neustarts. Standard und sichere Grenzen sind begründet dokumentiert; ungültige Werte werden verhindert.
2. Navigation und Intervallwechsel beenden überholte Aktualisierung. Identische Abrufe werden zusammengefasst; begrenzte Wiederholungen verhindern Anfragefluten. Inaktive Ansichten betreiben keine unnötigen Schleifen; Aktualisierung ist asynchron und abbrechbar.
3. Manuelle Aktualisierung bleibt verfügbar. Fehler machen letzte bekannte Daten und ihr Alter erkennbar, statt Daten kommentarlos zu löschen. Cache und Logs bleiben begrenzt und datensparsam.
4. Deterministische Tests prüfen Taktung, Intervallwechsel, Abbruch, gemeinsame Abrufe und Fehler. Native Windows-UI-E2E bedienen Einstellungen, sichtbar wirksame automatische Aktualisierung, manuelle Aktualisierung und Navigation ohne doppelte Schleifen; bestehende Routing-/Favoriten-/Monitorabläufe bleiben nutzbar.


