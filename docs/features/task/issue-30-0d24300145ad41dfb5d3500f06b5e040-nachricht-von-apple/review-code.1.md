# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### scripts/iOS-Deployment.ps1 (Assert-MacToolchain / Action-Dispatcher)

- **Konsistenz / Fehlerbehandlung** — In der `switch`-Verzweigung für `device` (Zeilen 1461–1470) fehlt im Windows+SSH-Pfad (`Invoke-DeviceViaSsh`) der Aufruf von `Assert-MacToolchain`. Dieser Pfad baut die App ebenfalls auf dem Mac (`dotnet publish` via Pair-to-Mac, Zeile 1270) und ist genauso von der Werkzeugkette abhängig wie der `simulator`-SSH-Pfad, der die Prüfung korrekt aufruft (Zeile 1456). Auch die Dokumentation (`scripts/iOS-Deployment.md`: „build / simulator / device: nur eine Warnung") verspricht die Prüfung für `device`. Aktuell läuft ein `device`-Deployment via SSH ganz ohne Toolchain-Warnung.

  Empfehlung: `Assert-MacToolchain` (ohne `-Strict`) im `if`-Branch vor `Invoke-DeviceViaSsh` aufrufen, analog zum `simulator`-Zweig.

- **Fehlerbehandlung** — Die Beta-Erkennung in `Get-ToolchainCheckScript` (Zeile 575) grept nur die Ausgabe von `xcodebuild -version` nach `beta`. Beta-Xcodes geben dort jedoch kein „beta" aus — die Ausgabe ist z. B. `Xcode 27.0` / `Build version 17Axxxx`, identisch im Format zu Release-Builds. Erkennbar ist eine Beta zuverlässig am Developer-Dir-Pfad (Beta-Installationen heißen `Xcode-beta.app`, `$DEVDIR` enthält dann „beta"). Folge: Ein Beta-Xcode passiert die Prüfung auch im `-Strict`-Modus (`store`) unbemerkt — genau der Fall, den die Prüfung verhindern soll.

  Empfehlung: `$DEVDIR` (bzw. den App-Bundle-Namen) zusätzlich in die Beta-Prüfung einbeziehen, z. B. `if printf '%s' "$XVB $DEVDIR" | grep -qi 'beta'` oder `case "$DEVDIR" in *[Bb]eta*)`.

### .github/actions/package-ios/action.yml (Install .NET MAUI workload)

- **Inkonsistenz Kommentar vs. Verhalten** — Der Kommentar (Zeilen 100–102) dokumentiert einen Fallback: „Löst der MAUI-Pin nicht auf, gilt als Fallback der Band-Default, sofern >= 10.0.110." Dieser Fall kann faktisch nicht eintreten: Löst sich der Pin nicht auf, bricht `dotnet workload update --from-rollback-file` unter `set -euo pipefail` den Step ab; läge die aufgelöste Manifest-Version dagegen bei einer anderen Version als dem Input, würde die Prune-Schleife (Zeilen 118–121) genau die benötigten Packs wieder löschen, weil sie strikt auf `inputs.maui-workload-version` vergleicht. Zudem ist die „Verifizierung" des Band-Defaults nur ein `dotnet workload list`-Log ohne Auswertung.

  Empfehlung: Entweder den Kommentar an das tatsächliche Verhalten anpassen (Pin ist verbindlich, kein Fallback) oder den Fallback real machen — z. B. die Prune-Schleife an die per `dotnet workload list` ermittelte tatsächlich aufgelöste Version koppeln statt an den Input.

## Geprüfte Dateien

Liste aller geprüften Dateien:
- `.github/actions/package-ios/action.yml`
- `.github/workflows/staging-ci.yml`
- `scripts/iOS-Deployment.ps1`
- `scripts/iOS-Deployment.md`
- `docs/CI-CD.md`
