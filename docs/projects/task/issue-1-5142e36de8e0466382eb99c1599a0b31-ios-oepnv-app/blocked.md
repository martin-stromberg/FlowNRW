# Offene Entscheidung – Abnahmestrategie

Datum: 2026-09-07
Phase: Projektplanung / offene Punkte vor endgültiger Planprüfung
Basisbranch: `task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app`
Schritt-Branch: noch keiner angelegt

## Grund

Die lokale native Teststrategie ist offen. Kein Mac/Xcode/iOS-Simulator ist in der erreichbaren Umgebung nachgewiesen. Der Nutzer hat Windows-Release und Tests in GitHub Actions ausdrücklich erhalten und iOS-CI/neues Deployment ausgeschlossen. Dies beantwortet nicht ausdrücklich, ob die native iOS-Abnahme auf später verschoben werden darf.

## Klärung

Bestandsaufnahme ausgeführt; .NET/Windows vorhanden, Core-Basistests 5/5 bestanden. Eine gebündelte Frage zur Abnahme gestellt (Windows native UI-E2E und iOS später manuell oder vorhandenen Mac nutzen); Antwort ausstehend. Keine Lifecycle-Klärungs- oder Abnahmerunden begonnen (je 0).

## Benötigte Entscheidung

Windows als automatisierte UI-Abnahmeplattform für gemeinsame Abläufe mit ausdrücklich späterer manueller iOS-Abnahme bestätigen oder erreichbare Mac-Testumgebung benennen. Danach Plan aktualisieren und erneut unabhängig prüfen.

## Gesicherter Stand

Anforderung, Bestandsaufnahme und Projektplan mit vier Entwicklungsschritten vorhanden. Keine Implementierung, kein Schrittbranch, kein Merge begonnen. Fremde Änderungen .gitignore und design-draft.zip bleiben unberührt. Endgültiger Planungscommit erfolgt erst nach vollständiger aktueller Planprüfung und Klärung.
