# Usability-Review – Schritt 3: Vordergrund-Aktualisierung und Anlass-Zuordnung

**Status:** Keine Auswirkungen

## Bewertung

Dieser Schritt verändert ausschließlich die interne Komposition der
Vordergrund-Aktualisierung: Die vier Anzeige-Seiten (Start/Favoriten,
Abfahrtsmonitor, Verbindungsdetails, Verbindungsergebnisse) melden dem zentralen
`RefreshLifecycle` nur noch „sichtbar"/„verdeckt", statt Refresh-Gate,
Überlappungsschutz und Anlass-Zuordnung selbst zusammenzusetzen.

- **Keine geänderte Oberfläche:** Keine XAML-, Control-, Farb-, Text- oder
  Layout-Änderung. AutomationIds, Seitenstruktur und Interaktionen sind unverändert.
- **Nutzerseitiges Verhalten:** Unverändert beabsichtigt. Vordergrund-Rückkehr
  erneuert weiterhin veraltete Daten nur bei geladenem und aktivem Intervall;
  manuelles Aktualisieren, Fehlertexte im Seitentitel und Intervall-Schleifen
  bleiben gleich. Die einzige fachliche Neuerung — der Überlappungsschutz auf den
  Journey-Pfaden — ist für Nutzer nicht sichtbar (verhindert nur doppelte
  Hintergrundaufrufe).
- **Screenreader/Accessibility:** Keine Änderungen an Semantik-Properties.

Damit ist ein visuelles Usability-Review ohne Substanz; die Bewertung beschränkt
sich auf die dokumentierte Verhaltensneutralität, die durch die Unit-Tests
(327/327) und die geplante `WindowsLifecycleUiTests.ps1`-Regression abgesichert
wird.
