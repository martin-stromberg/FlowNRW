# Bestandsaufnahme: Hintergrundaktualisierung und Wiederaufnahme

Diese Bestandsaufnahme erfasst den bestehenden Aktualisierungs- und App-Lebenszyklus bezogen auf Schritt 8 der iOS-ÖPNV-App. Sie dokumentiert den Codezustand und die projektweiten Testausgangsläufe vor Änderungen.

## Zusammenfassung

- Vorhanden sind `RefreshLoop`, begrenzte persistierte Vordergrundintervalle und ein gemeinsamer `ForegroundState`; `DeparturePage` und `HomePage` binden diese Timer an Sichtbarkeit und aktives MAUI-Fenster.
- Monitore und Favoriten besitzen Abbruch-, Busy- und Revisionsschutz für konkurrierende Abrufe. Abfahrtsantworten enthalten Abrufzeit, Cachefrische und Metadaten zum Datenalter.
- In `FlowNRW/Platforms/iOS` gibt es `AppDelegate`, `Program` und `Info.plist`; `AppDelegate` meldet keine Hintergrundaufgabe an. Es ist kein iOS-Hintergrundabrufcode vorhanden. Die vorhandene MAUI-Fensteraktivierung setzt nur `ForegroundState.IsActive`.
- Resume löst aktuell keinen gezielten Abruf veralteter Daten aus. Der Einzelmonitor setzt seinen Loop beim erneuten Erscheinen fort; erfolgreiche alte Ergebnisdaten behalten ihre ursprüngliche Abrufzeit. Die Startseite initialisiert fehlende Favoritendaten bei jedem Erscheinen.
- Es existiert keine iOS-spezifische Hintergrund- oder Resume-Testabdeckung. Bestehende Core-Tests decken Vordergrundtimer, Begrenzungen, Resume-nahe Zustandswechsel, Überschneidungsschutz, Abbruch und verspätete Ergebnisse ab.
- Test-Ausgangszustand: aktueller Core-Lauf 210 bestanden, 0 fehlgeschlagen, 0 übersprungen; Release-Skriptlauf 26 bestanden, 0 fehlgeschlagen, 0 übersprungen. Vorhandener vollständiger Windows-Release-/Coverage-/UI-Nachweis liegt auf einem inhaltlich codegleichen Produktivstand vor. Einzelheiten und Prüfgrenzen stehen in [Tests](inventory/tests.md).

## Details

- [Datenmodelle](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
