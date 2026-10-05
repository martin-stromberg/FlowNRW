# Offene Aufgaben

Aktualisiert am: 05.10.2026 (Abend)

Native Windows-Nachweiskette komplett grün auf Commit `97b940f`. Die erste iOS-Geräteabnahme lieferte drei Produktbefunde, die im Folgecommit behoben sind:

1. Degradierte Abfahrtsantworten (Ersatzquelle/Warnung/veraltet) wurden als Fehler angezeigt statt mit Hinweis gerendert — behoben in `StopMonitorViewModel`/`FavoriteMonitorViewModel` (vollständige Boards bleiben bevorzugt, Persistenz nur für komplette Antworten).
2. `UTC+02:00`-Offset in Verbindungszeiten — `JourneyPresentation.EventTime` zeigt `HH:mm` mit `Vortag`/`Folgetag`-Präfix.
3. Umstiege hingen als Sammelblock unter der Timeline — `JourneyTimelineView` rendert sie positionsgetreu zwischen den Transitabschnitten.

- [ ] iOS-Geräteabnahme auf dem Bugfix-Build wiederholen; vor allem Abfahrts-/Cache-Checkpunkte 1–5 aus `ios-device-acceptance.md` erneut durchlaufen. Scheitert der Abfahrtsabruf weiter, Geräte-Netzwerklog sichern (echte Providerstörung vs. Darstellungsschwelle).
- [ ] Native Refresh-/Lifecycle-/Departure-Cache-Regressionen sowie eine Designmatrix auf dem neuen Build erneut laufen lassen (Nacht-Runner `artifacts/night-runner.ps1`) und die Manifeste aktualisieren.
- [ ] Dokumentation/README/Release Notes finalisieren (Schritt 12).
- [ ] Danach Projektabnahme, Abschlusscommit und Merge vorbereiten; Schritt 9 bleibt „In Arbeit“.

Hinweis: `artifacts/` ist gitignoriert; Endmatrix-Manifeste verweisen auf Commit `97b940f` mit Build-SHA256 `175DBAB0…095D`.
