# Offene Aufgaben

Erstellt am: 2026-10-15
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

- [x] Task 27 — `WindowsJourneyUiTests.ps1` auf dem geänderten Code ausgeführt: alle fünf Modi bestanden (Default, `-Monitors`, `-Favorites`, `-Locations`, `-Maps`), jeweils via `schtasks /it` in der interaktiven Session (Logs: `artifacts/tests/continue-20261015-213718/`). Neue Assertionen nachgewiesen: Namens-Match ohne `de:`-Kennung, `FavoriteNotice`/`FavoriteMetadata`-Toggle, `ToggleFavorite`-Header, `map-hang`-Watchdog (`-Maps`), `MonitorNotice`/`MonitorMetadata`. Ein Skriptfehler wurde behoben: nach `RestartFavorites` stellte die Umgebung `favorite-cache-seed` (vollständige Antwort) wieder her — `SetText 'HomeScenario' 'success'` ergänzt, sonst erscheint `FavoriteNotice0` nie.
- [x] Task 28 — `WindowsDepartureCacheUiTests.ps1` (19 s, bestanden) und `WindowsRefreshUiTests.ps1` (448 s, bestanden) auf dem geänderten Code. `WindowsLifecycleUiTests.ps1`: zwei Skript-Robustheitsprobleme behoben (`Hold` ignoriert jetzt das noch minimierte Vordergrund-Fenster nach `SW_MINIMIZE`; `PauseAndResume` übergibt den Vordergrund vorher an die eigene Konsole) — der Lauf kommt jetzt über die frühere Fehlstelle hinaus, scheitert aber in dieser Session daran, dass die wiederhergestellte App kein `Activated`/`Resumed` erhält (`RefreshForeground: False`, kein Resume-Refresh). Umgebungsbedingt (VNC/Remote-Session, Fenster bleibt als ikonisches Vordergrundfenster hängen), kein Feature-Regression — am Gerät/nativer Konsole nachholen.
- [x] Task 29 — `WindowsDesignUiTests.ps1`-Matrix neu aufgenommen: alle sechs Kombinationen (light/dark × 430×900/1024×768 × 100 %/150 %) bestanden (je ~90 s, alle Assertionen + CAPTUREs), Ablage `docs/help/design/verification/icon-matrix/` inkl. `matrix.jsonl` mit aktuellem Commit/Build-Hash. Reste des abgebrochenen Laufs unter `matrix/` wurden nicht committet.
- [ ] Task 30 — iOS-Verifikation des Karten-Watchdogs auf Gerät/Simulator (Safari-Web-Inspector an der `HybridWebView`/WKWebView; `AppLog`-Diagnose ist eingebaut). Nicht möglich: kein iOS-Gerät/-Simulator in dieser Umgebung verfügbar — bleibt als manueller Gerätenachweis offen.

## Code-Review-Befunde

Keine — der letzte Befund (verwaiste `JourneyPresentation.Address`) wurde entfernt.

## Fehlgeschlagene Tests

- `WindowsLifecycleUiTests.ps1` (interaktiv, siehe Task 28): `AwaitCount` erwartet nach Pause/Resume einen neuen Provider-Request; die wiederhergestellte App erhält in dieser Session kein `Activated`/`Resumed`, daher läuft `lifecycle.SetActive(true)` nie an (`RefreshForeground: False`). Keine Feature-Regression — dokumentiert als Umgebungslimit.
