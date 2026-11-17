# Dokumentationsstruktur und Screenshot-Quellen

Bestand unter `docs/` (~31 MB gesamt) — relevant für die Wahl des Seitenquellorts und die kuratierten Screenshot-/Inhaltsquellen.

## Verzeichnisstruktur

| Pfad | Inhalt |
|---|---|
| `docs/CI-CD.md` | Pipeline-/Branch-Doku, Workflow-Tabelle, „Einmalige Repo-Einrichtung (GitHub)", Quality Gates, Fehlersuche |
| `docs/RELEASE_NOTES.md` | Release Notes |
| `docs/design/` | `acceptance.md` (verbindliche visuelle Abnahme), `licenses/` (Font-Lizenzen, Codepoint-Manifest), `reference/` (HTML-Designentwürfe, `flownrw_icon.png`, `trans_nrw_mobil_system/`) |
| `docs/features/` | `task/` — Arbeitsverzeichnisse der Feature-Umsetzungen (requirement/plan/inventory je Aufgabe) |
| `docs/help/` | Strukturierte Feature-Dokumentation: `index.md` plus Themenordner `abfahrten`, `design`, `fahrplanauskunft`, `favoriten`, `karte`, `monitorintervalle`, `standort`, `verbindungssuche` — je mit `index.md`, `beschreibung.md`, `ablauf-*.md`, `api.md`, `business-rules.md`, `troubleshooting.md`, `verification/` |
| `docs/review/` | `index.html`, `README.md`, `START-WINDOWS.txt` — historischer Begutachtungskanal (siehe unten) |

`docs/help/index.md` bündelt die Hilfethemen und ist die inhaltliche Quelle für prägnante Feature-Beschreibungen; `README.md` (Projektwurzel) enthält den „Aktuellen Funktionsstand" (Zeilen 11–28) als Feature-Liste.

## Historischer statischer HTML-Vorläufer

- `docs/review/index.html` (29 Zeilen, Inline-CSS, deutsch): eigenständige Begutachtungsseite „Zwischenstände zur Begutachtung" mit heller Kartenoptik, Metriken und Schritten — ursprünglich für eine lokale IIS-Bereitstellung unter `D:\Dashboard\ÖPNV` gedacht.
- `docs/review/README.md` (Zeilen 1–3): „Historischer Begutachtungskanal – nicht mehr im Umfang. Am 16.09.2026 hat der Nutzer die lokale IIS-Präsentation ausdrücklich gestrichen." — die Seite ist fachlich veraltet (Stand 15.09.2026, „noch keine bedienbare ÖPNV-Version freigegeben").
- Fakt: Es existiert damit bereits eine handgeschriebene, buildfreie statische HTML-Seite im Repo — jedoch ohne Pages-Anbindung und mit überholtem Inhalt.

## Screenshot-Quellen (versionierte PNGs, 523 Dateien unter `docs/`)

| Quelle | Umfang | Charakter |
|---|---|---|
| `docs/help/design/verification/icon-matrix/` | 227 PNGs + `matrix.jsonl` | Neuester Stand (14./15.10.2026, Commit `a34a04f`): Material-Symbols-Icons, Inter/JetBrains Mono, Status-Chips; Themen `light`/`dark`, Geometrien 430×900 und 1024×768, Textskalierung 100 %/150 %; `matrix.jsonl` enthält je Bild Commit, Build-SHA256, Plattform, Thema, logische Fenstergröße |
| `docs/help/design/verification/final-matrix/` | 113 PNGs | Ältere Schritt-9-Abnahmematrix, Namensschema `<theme>-<width>-<height>-<scale>-success-<szenario>.png` (hell/dunkel, 430×900 und 1024×768) |
| `docs/help/design/verification/matrix/`, `revised-matrix/`, `locations/` | weitere Matrizen/Unterordner | Ältere Verifikationsstände |
| `docs/help/design/verification/native-*.png` | 13 PNGs | Native Windows-Aufnahmen einzelner Oberflächen (u. a. `native-departures.png`, `native-results.png`, `native-map.png`, `native-journey-map.png`, `native-search.png`, `*-narrow.png`) |
| `docs/help/<thema>/verification*/native-*.png` | 23 PNGs | Themenbezogene native Aufnahmen (abfahrten, karte, standort, verbindungssuche, favoriten, monitorintervalle) |
| `docs/help/design/verification/failure-*.png` | 3 PNGs | Fehlerzustands-Nachweise (nicht präsentationsgeeignet) |

Hinweise: Die Matrizen enthalten bewusst auch Fixture-, Leer-, Fehler- und Offlinezustände (z. B. `*-success-map-offline.png`, `*-routing-provider-error.png`); für eine Präsentation ist Kuratierung nötig. Aufnahmeprozess dokumentiert in `docs/help/design/verification/icon-matrix-run.md` (UiTest-Fixture-Build + `tests/WindowsJourneyUiTests/WindowsDesignUiTests.ps1` mit `-ScreenshotDirectory`, `-Theme`, `-Width`, `-Height`, `-TextScale`).

## Inhaltsquellen für Seitentexte

- Zweck/Einordnung: `README.md` Zeilen 1–9 („bundesweite Verbindungen und Abfahrten mit bevorzugten NRW-Echtzeitdaten", .NET 10 MAUI, Windows geprüft, iOS-Plattformbasis).
- Feature-Liste: `README.md` Zeilen 11–28; thematisch vertieft unter `docs/help/<thema>/index.md`.
- Plattform-/Standort-/Lizenzangaben: `README.md` Voraussetzungen (Zeilen 44–56), Lizenz (PolyForm Noncommercial 1.0.0, Zeilen 152–156).
- iOS-Status: „iOS-Plattformbasis, native Geräteabnahme steht aus" (`README.md` Zeilen 9, 27, 142; `docs/design/acceptance.md` Zeile 54).

## `docs/`-Konflikt für die Pages-Quellwahl

- `docs/` enthält neben der Feature-Doku auch Arbeitsstände (`docs/features/task/`), große Verifikationsmatrizen (~23 MB allein unter `docs/help/design/verification/`) und den veralteten `docs/review/`-Stand.
- Eine klassische „Deploy from branch aus `/docs`"-Veröffentlichung würde all diese Inhalte öffentlich ausliefern (inkl. `.md`-Arbeitsdateien und `index.html` unter `docs/review/`).
