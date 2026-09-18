# Usability-Review

## Ergebnis

**Status:** Keine Befunde

## Geprüfte Interaktionen

- Aktueller Standort als Start/Ziel mit verständlich beschrifteten Buttons; manuelle Alternative bleibt verfügbar.
- Fehler/Entzug und Wiederholung mit erhaltener Auswahl und lesbaren Hinweisen.
- Nahe Haltestellen als Klartextliste; Kartenmarker und native Kartenliste öffnen den richtigen Monitor.
- Quellen, vorherige Ergebnisse und unbekannte Entfernungen/Positionen erkennbar.
- Tastaturauswahl, Rücknavigation und schmale Fensterbreite 430 × 900 im nativen Windows-Harness bestanden; Aufnahmen visuell geprüft.

## Geprüfte Dateien

SearchPage, StopSearchPage, EndpointViewModel, StopMonitorViewModel und bestehende Karten-/Monitoransicht im tatsächlichen nativen Ablauf. Fixture-Scenariofelder sind ausschließlich im UiTest-Build vorhanden.

Getrennte lokale Prüfung nach Agentenlimit; keine unabhängige Usability-Agentenprüfung. Reale OS-Dialoge wurden wegen Freigabesperre nicht bedient. iOS-VoiceOver und globale Schriftvergrößerung bleiben ausdrücklich nicht ausgeführt.
