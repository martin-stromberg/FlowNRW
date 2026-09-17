### Schritt 5: Aktueller Standort und nahe Haltestellen

**Kundenanforderung:** Ergänze die manuellen Abläufe um den aktuellen GPS-Standort als Start oder Ziel sowie GPS-nahe Haltestellen in Liste und Karte. Nach Betriebssystemzustimmung können Nutzer ihre Umgebung erkunden und den gewählten Monitor öffnen; ohne Standort bleiben manuelle Abläufe verfügbar.

**Betroffene Bereiche:** Standortberechtigung, Routing-Eingabe, Nahbereich, Umgebungskarte, UI-Tests.

**Abhängigkeiten:** 2, 3, 4.

**Verbindliche Rahmenbedingungen:** C#/.NET MAUI, MVVM und DI; vorhandene asynchrone abbrechbare Services aus Schritt 1 weiterverwenden. NRW-Priorität bei mindestens einem Endpunkt in NRW, sichere konfigurierbare bundesweite/EFA-Versorgung, eindeutige Fahrtzuordnung, Fallback, technische Cachegrenzen und datensparsame Fehler-/Laufzeitdiagnose erhalten. Keine erfundene Echtzeit, personenbezogene Historie oder Zusatzprodukte. Design-HTML/Markdown für minimalistische Navigation, skalierbare Texte, Labels, Touch-Ziele, Kontraste und nicht rein farbliche Statusangaben verwenden. Windows-Release und Tests in GitHub Actions bleiben funktionsfähig, ohne Apple-Werkzeugkette in Windows-Jobs. Keine iOS-CI und kein neues automatisiertes Deployment. Native iOS-Abnahme liegt beim Nutzer und blockiert lokal nicht; iOS-Code prüfen und manuelle Prüfanleitung aktualisieren. Alle UI-Abläufe dieses Schritts unter Windows soweit technisch möglich als echte native UI-E2E tatsächlich ausführen; reine ViewModel-Tests ersetzen sie nicht. Nach ernsthaftem Versuch nicht ausführbare Flüsse mit Ursache und Versuchsnachweis dokumentieren. Relevante Service-/Modelltests, Format, Windows-Build mit Warnungen als Fehler und Repository-Coverage prüfen. Vorhandene Nachweise gezielt bei betroffenem Verhalten erneuern. Keine lokale IIS-Präsentation oder gesonderte Zwischenpaket-Bereitstellung.

**Akzeptanzkriterien:**

1. Aktueller Standort ist nach Berechtigung als Start oder Ziel nutzbar. iOS-Berechtigungsbeschreibungen und Windows-Standortintegration sind vorhanden. Ablehnung, Entzug, Timeout und fehlende Position erzeugen verständliche Zustände und verhindern keine manuelle Suche.
2. GPS-nahe Stationen erscheinen passend zur tatsächlichen Position in Liste/Karte. Beide Auswahlwege öffnen den richtigen Monitor; eine manuell gewählte Umgebung bleibt möglich.
3. Standort dient nur angeforderten Funktionen, ohne persistente Bewegungs-/Adress-/Suchhistorie und sensible Logs. Veraltete Standort-/Suchergebnisse überschreiben keine neuere Auswahl. Unbekannte Positionen/Entfernungen werden nicht erfunden.
4. Deterministische Tests prüfen Berechtigungs-/Positionsfehler und Nahbereichszuordnung. Native Windows-UI-E2E bedienen GPS als Start/Ziel, Nahbereich → Monitor/Karte sowie Ablehnungs-/Fehlerwege soweit möglich. Tatsächliche Betriebssystemversuche und Fixture-Anteile bleiben getrennt; iOS-Prüfanleitung umfasst Berechtigung/Entzug.


