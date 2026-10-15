# Offene Aufgaben

Erstellt am: 2026-10-15
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

- [ ] Task 27 — `WindowsJourneyUiTests.ps1` auf dem geänderten Code ausführen (interaktive Windows-Session erforderlich; Skript wurde angepasst: Kennungs-Assertion → Namens-Match, `FavoriteNotice`-/`FavoriteMetadata`-Toggle, `ToggleFavorite`-Header, `map-hang`-Watchdog, `MonitorNotice`/`MonitorMetadata`).
- [ ] Task 28 — `WindowsDepartureCacheUiTests.ps1`, `WindowsLifecycleUiTests.ps1`, `WindowsRefreshUiTests.ps1` auf dem geänderten Code erneut ausführen.
- [ ] Task 29 — `WindowsDesignUiTests.ps1`-Matrix neu aufnehmen (alle Kombinationen light/dark × 430×900/1024×768 × 100 %/150 %; bisherige `matrix.jsonl`-Einträge referenzieren Commit `ad90d8d` ohne dieses Feature).
- [ ] Task 30 — iOS-Verifikation des Karten-Watchdogs auf Gerät/Simulator (Safari-Web-Inspector an der `HybridWebView`/WKWebView; `AppLog`-Diagnose ist eingebaut).

## Code-Review-Befunde

Keine — der letzte Befund (verwaiste `JourneyPresentation.Address`) wurde entfernt.

## Fehlgeschlagene Tests

Keine.
