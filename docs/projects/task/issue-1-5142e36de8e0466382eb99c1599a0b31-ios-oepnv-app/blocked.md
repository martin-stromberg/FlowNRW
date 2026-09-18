# Blockade: Freigabe echter Standortprobe

Datum: 18.09.2026. Schritt5, Testphase; Basisbranch task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app, Schrittbranch task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-5-standort.

Der Produktumfang ist implementiert, 165 Coretests und sämtliche Standort-Fixture-/Routing-/Monitor-/Kartenabläufe unter Windows bestehen. Der echte Release-Test wurde vor Start von der automatischen Freigabeprüfung abgelehnt: Präzise Positionsdaten könnten an die konfigurierten ÖPNV-Dienste übermittelt werden; eine ausdrückliche Freigabe dieses Payloads/der Ziele fehle.

Benötigt wird ausschließlich die Erlaubnis zum tatsächlichen Standortabruf und zur Nahbereichsübermittlung an openservice-test.vrr.de und/oder v6.db.transport.rest. Der vorbereitete Harness speichert keine echten Koordinaten, Stationsnamen oder Screenshots. Keine Umgehung und keine Änderung der OS-Einstellungen. Bis dahin bleiben Live-Test, finale fachliche Abnahme und Merge offen.

Lifecycle-Runde1; Projekt-Abnahmerunden0, fachliche Klärungsrunden0. Die technische Freigabesituation ist keine neue Produktentscheidung. Kein Merge läuft; auf dem Schrittbranch bleiben Code und Nachweise erhalten. Unbeteiligte .gitignore-Änderung und design-draft.zip bleiben unangetastet.
