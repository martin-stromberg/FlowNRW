# Offene Aufgaben

Aktualisiert am: 05.10.2026

Die native Windows-Nachweiskette ist auf Commit `97b940f` vollständig bestanden: vier Designmatrizen (hell/dunkel, 430×900 bei 100 %/150 %, 1024×768), Cache-während-Refresh-Bilder, alle Journey-/Cache-/Refresh-/Lifecycle-Regressionen, UiTest-Build 0/0 und Core-Tests 279/279. Die visuelle Endprüfung findet keine offenen Produktbefunde mehr; Detailergebnisse stehen in `test-results-visual-final.md` und `review-visual-current.md`.

- [ ] iOS-Geräteabnahme durch den Nutzer: Safe Areas, Systemthema hell/dunkel, Dynamic Type inklusive großer Textgrößen, VoiceOver, Hoch-/Querformat, reale GPS-Nähe, Monitor während Refresh, Offline-/Positionsfehler und Start mit gespeichertem Favoritencache auf mindestens einem kleinen und einem großen iPhone. Ergebnisse in `ios-device-acceptance.md` eintragen.
- [ ] Dokumentation/README/Release Notes finalisieren (Schritt 12 der Aufgabenliste).
- [ ] Danach Projektabnahme, Abschlusscommit und Merge vorbereiten; Schritt 9 bleibt bis zur iOS-Abnahme „In Arbeit“.

Hinweis: `artifacts/` ist gitignoriert; die Manifeste der Endmatrix liegen unter `artifacts/step9-visual-final/` und verweisen auf Commit `97b940f` mit Build-SHA256 `175DBAB0…095D`.
