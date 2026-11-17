# Bestandsaufnahme: GitHub Page – Anwendung mit modernem Stil präsentieren

Analysiert wurde das Repository bezogen auf die Anforderung aus `requirement.md` (GitHub Issue #26): eine statische Projektseite unter `https://martin-stromberg.github.io/FlowNRW/`, die FlowNRW mit modernem, an das App-Design angelehntem Stil präsentiert und aus dem Repository heraus gepflegt wird.

## Zusammenfassung

- **Kein GitHub-Pages-Deployment vorhanden.** `.github/workflows/` enthält keinen Pages-Workflow; es gibt keinen `site/`-, `docs/page/`- oder sonstigen Seitenquellordner, kein Jekyll und keine `_config.yml`. Die API-Abfrage `gh api repos/martin-stromberg/FlowNRW/pages` liefert 404, das Repo-Flag `has_pages` ist `false` — Pages ist im Repository derzeit nicht aktiviert.
- **Bestehende CI/CD-Landschaft:** 10 Workflows (CI-Gates, RC-/Stable-Release, Promotion, Back-Merge, Security-Scan, drei geplante Review-Issue-Workflows) plus 5 Composite Actions. Dokumentiert in `docs/CI-CD.md` mit Workflow-Tabelle und dem Abschnitt „Einmalige Repo-Einrichtung (GitHub)" — dem projektüblichen Ort für Pflege-/Einrichtungsdoku.
- **`docs/` ist belegt und groß:** ~31 MB Projektdokumentation, davon ~23 MB Screenshot-Verifikationsmatrizen. Eine klassische „Deploy from branch aus `/docs`"-Veröffentlichung würde den gesamten Dokumentationsbestand inklusive Arbeitsverzeichnisse (`docs/features/task/`) ausliefern.
- **Designquellen vollständig im Repo:** Farb-Tokens in `FlowNRW/Resources/Styles/Colors.xaml` (hell/dunkel inkl. dokumentierter Kontrastwerte), gebündelte Schriften Inter, JetBrains Mono und Material Symbols Rounded unter `FlowNRW/Resources/Fonts/` mit Lizenztexten in `docs/design/licenses/`; verbindliche Designkriterien in `docs/design/acceptance.md` und `docs/help/design/`.
- **Screenshot-Quellen vorhanden:** 523 PNGs unter `docs/`, darunter 36 `native-*.png`-Aufnahmen, 113 Bilder in `docs/help/design/verification/final-matrix/` und 227 Bilder in `icon-matrix/` (mit `matrix.jsonl`-Metadaten je Bild: Commit, Build-Hash, Thema, Geometrie, Textskalierung).
- **Download-/Release-Verweise verfügbar:** `release.yml` erzeugt GitHub-Releases mit den Assets `FlowNRW-vX.Y.Z-win-x64.zip` und `update.json`; RC-Prereleases entstehen auf `staging` (`vX.Y.Z-rc.N`). Stabile URL `releases/latest` ist möglich.
- **Historischer HTML-Vorläufer:** `docs/review/index.html` ist eine eigenständige statische Begutachtungsseite (früher per lokalem IIS bereitgestellt); laut `docs/review/README.md` vom Nutzer am 16.09.2026 gestrichen und fachlich veraltet.
- **Ziel-URL/Base-Pfad:** Projektseite erfordert den Base-Pfad `/FlowNRW/`; das Repository `martin-stromberg/FlowNRW` ist öffentlich, Default-Branch ist `main`, Veröffentlichungskette ist Feature-Branch → `staging` → `main`.

**Test-Ausgangszustand:** Beide lokal ausführbaren Testsuiten bestehen fehlerfrei — `dotnet test FlowNRW.Tests` (300/300 bestanden) und `npm run test:release-version` (26/26 bestanden). Keine nachgewiesenen Testfehler. Nicht ausgeführt: die Windows-UI-Test-Skripte unter `tests/WindowsJourneyUiTests/` (erfordern interaktive Desktopsitzung und `UiTest`-Fixture-Build) sowie die CI-Gates `dotnet format`, Security-Scan und MAUI-Solution-Build. Details und Nachweise: [Tests](inventory/tests.md).

## Details

- [CI/CD-Workflows, Releases und GitHub-Pages-Optionen](inventory/cicd-workflows.md)
- [Design- und Assetquellen](inventory/design-sources.md)
- [Dokumentationsstruktur und Screenshot-Quellen](inventory/docs-structure.md)
- [Tests](inventory/tests.md)
