# Design

Die native .NET-MAUI-Oberfläche von FlowNRW folgt dem verbindlichen, in Schritt 9 abgenommenen Token-System aus [`docs/design/acceptance.md`](../../design/acceptance.md). Der Nachschärfungsschritt „Modernes Design auf AAA-Niveau" ersetzt Unicode-Textglyphen durch die Icon-Font Material Symbols Rounded, führt die lizenzfreien Schriften Inter und JetBrains Mono ein und stellt Abfahrts- und Verbindungsstatus als Status-Chips aus Symbol und Text dar.

- [Beschreibung](beschreibung.md) — Zweck, Funktionsweise und Grenzen des Design-Refreshs
- [Anwenderablauf](ablauf-anwender.md) — was sich in Bedienung und Darstellung ändert
- [Technischer Ablauf](ablauf-technisch.md) — Font-Registrierung, Icon-Auflösung, Statusableitung, Theme-Färbung
- [Öffentliche Präsentationsschnittstellen](api.md) — `DepartureStatus`, `DeparturePresentation.Status`/`DelayText`, `JourneyPresentation.LocalTime`/`Clock`
- [Fachliche Regeln](business-rules.md) — Statussemantik, Informationsdarstellung, Datenbeschränkungen
- [Fehlerhilfe](troubleshooting.md) — Icon-Font-Registrierung, FILL-Varianten, Codepoints, Tab-Überlauf, Clipping
- [Schritt-9-Designintegration](README.md) — ursprüngliche Integration und Abnahmerahmen
- [Icon-/Font-Matrix-Neuaufnahme](verification/icon-matrix-run.md) — Screenshot-Nachweis der neuen Darstellung (`verification/icon-matrix/`)

Font-Lizenzen und das Codepoint-Manifest der Icon-Font liegen unter [`docs/design/licenses/`](../../design/licenses/). Ältere Nachweismatrizen aus der Schritt-9-Abnahme bleiben unter [`verification/`](verification/) erhalten.
