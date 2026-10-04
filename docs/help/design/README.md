# Designintegration der FlowNRW-App

Schritt 9 führt den Entwurf als native .NET-MAUI-Oberfläche weiter. Die drei persistenten Bereiche heißen Abfahrten, Verbindungen und Haltestellen. Fahrtdetails bleiben Unterseiten; Einstellungen sind sekundär erreichbar.

Die verbindlichen Farben, Abstände, Zustände und Screenshotvarianten stehen in [`docs/design/acceptance.md`](../../design/acceptance.md). Original-HTMLs und lokal gerenderte Referenzen liegen unter [`verification/reference`](verification/reference). Native Windows-Nachweise, revidierte PNGs, Kontrastwerte und die unabhängigen Bildreviews liegen unter [`verification`](verification).

Die Windows-Matrix verwendet synthetische Daten, blendet Fixture-Steuerungen aus und dokumentiert Fenstermaß, DPI, Thema und Textskalierung. `design-draft.zip` bleibt unverändert. Die iOS-Geräteprüfung (Safe Areas, Dynamic Type, VoiceOver, Themen und Orientierung) muss auf echten Nutzergeräten erfolgen.

Der aktuelle Lifecycle-Testbericht weist die noch offenen Build-/Regressionseinschränkungen aus; leere oder nicht ausgeführte Logs gelten nicht als bestanden.

Die aktuelle manuelle iPhone-Abnahme für Schritt 9 steht in [`ios-device-acceptance.md`](../../features/task/issue-1-5142e36de8e0466382eb99c1599a0b31-ios-oepnv-app-schritt-9-design/ios-device-acceptance.md). Sie deckt Favoriten- und Haltestellenmonitor-Cache, erhaltene Suchtreffer, Verbindungsfavoriten, Start-/Zieltausch sowie Darstellung und Bedienung auf echten Geräten ab. Die Prüfung ist noch nicht ausgeführt.
