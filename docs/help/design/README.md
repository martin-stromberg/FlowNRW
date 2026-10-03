# Designintegration der FlowNRW-App

Schritt 9 führt den Entwurf als native .NET-MAUI-Oberfläche weiter. Die drei persistenten Bereiche heißen Abfahrten, Verbindungen und Haltestellen. Fahrtdetails bleiben Unterseiten; Einstellungen sind sekundär erreichbar.

Die verbindlichen Farben, Abstände, Zustände und Screenshotvarianten stehen in [`docs/design/acceptance.md`](../../design/acceptance.md). Original-HTMLs und lokal gerenderte Referenzen liegen unter [`verification/reference`](verification/reference). Native Windows-Nachweise, revidierte PNGs, Kontrastwerte und die unabhängigen Bildreviews liegen unter [`verification`](verification).

Die Windows-Matrix verwendet synthetische Daten, blendet Fixture-Steuerungen aus und dokumentiert Fenstermaß, DPI, Thema und Textskalierung. `design-draft.zip` bleibt unverändert. Die iOS-Geräteprüfung (Safe Areas, Dynamic Type, VoiceOver, Themen und Orientierung) muss auf echten Nutzergeräten erfolgen.

Der aktuelle Lifecycle-Testbericht weist die noch offenen Build-/Regressionseinschränkungen aus; leere oder nicht ausgeführte Logs gelten nicht als bestanden.
