# Usability-Review

## Ergebnis

**Status:** Keine Befunde

## Arbeitsmodus

26.09.2026: Lokale Prüfung wegen ausgefallenem Review-Agenten; keine unabhängige, vom Planwissen freie Agentenprüfung. Bewertet werden die tatsächlichen nativen Bedienläufe und Screenshots der aktuellen Lebenszyklusfunktion. Die gesonderte visuelle Gesamtabnahme gegen den Designentwurf bleibt Schritt 9.

## Geprüfte Interaktionen

- Automatik einstellen/speichern: verständliche beschriftete Auswahl, Erfolg/Fehler sichtbar, manuelle Aktualisierung bleibt erreichbar.
- Wiederaufnahme: frische Daten ohne Zusatzabruf; alte Daten werden in derselben Seite erneuert. Fehler behalten letzte bekannte Daten mit Quelle/Alter.
- Auswahl einer Verbindung: eindeutige Fahrt bleibt erhalten; fehlende Zuordnung erklärt erneute Auswahl, Kartenaktion ist dann deaktiviert.
- Standort/Umgebung und Favoriten: manuelle Alternativen, Zurücknavigation und schmale Bedienelemente im nativen Lauf bestanden.
- Hintergrundgrenzen: Erklärung zu Aus, Wiederaufnahme und nicht garantierter iOS-Ausführung im 430×900-Fenster vollständig lesbar (native-refresh-settings-narrow.png).
- Keine neuen technischen Nutzereingaben. Szenariofelder und Zähler sind ausschließlich Testinstrumente im UiTest-Build.

## Nachweise

artifacts/step8-ui/lifecycle.txt, locations-retry.txt, favorites.txt sowie native-lifecycle-detail.png, native-nearby-narrow.png und native-refresh-settings-narrow.png. iOS/VoiceOver ist hier nicht ausgeführt; Gerätecheckliste in docs/help/monitorintervalle/installation.md.

## Geprüfte Dateien

FlowNRW/HomePage.cs, DeparturePage.cs, ResultsPage.cs, JourneyDetailPage.cs, SearchPage.cs, StopSearchPage.cs, MapPage.cs und RefreshSettingsPage.cs sowie die zugehörigen geänderten Präsentationsmodelle.