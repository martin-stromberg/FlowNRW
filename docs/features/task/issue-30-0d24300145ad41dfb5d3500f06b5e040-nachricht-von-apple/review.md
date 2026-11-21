# Plan-Review

Geprüft: Re-Review nach Plan-Korrektur 2026-11-20 (SDK-Band `10.0.1xx`, `global.json` `10.0.100`, MAUI-Pin `10.0.110` — kein `manifest-10.0.400`, kein `10.0.120`-MAUI-Manifest). Verglichen: `plan.md` (224 Zeilen) gegen `.github/actions/package-ios/action.yml`, `.github/workflows/staging-ci.yml`, `scripts/iOS-Deployment.ps1`, `scripts/iOS-Deployment.md`, `docs/CI-CD.md` sowie `issue-30-...-tasks.md`.

## Ergebnis

**Status:** Vollständig umgesetzt

Alle im Repo umsetzbaren Planelemente sind nachweisbar im Code bzw. in der Dokumentation vorhanden; die korrigierten Versionswerte (`10.0.1xx` / `10.0.100` / `10.0.110` / `27.2.10000` / `Xcode_27.2.app` / `xcode-27`) sind konsistent über alle Artefakte hinweg. Die offenen Tasks 14, 17, 18, 19 sind externe Laufzeit-/Anwenderschritte (siehe Hinweise), keine Implementierungslücken.

## Umgesetzte Planelemente

### `.github/actions/package-ios/action.yml`

- [x] Input `ios-workload-version`-Default `'26.5.10318'` → `'27.2.10000'` — Z. 44–47
- [x] Neuer Input `maui-workload-version`, optional, Default `'10.0.110'`, Kommentar verweist auf UIScene-Lifecycle-Pflicht ab Xcode 27 und auf das Ende der Manifest-Linie bei `10.0.110` — Z. 48–58
- [x] Step `Setup .NET`: `dotnet-version: '10.0.1xx'` beibehalten; Kommentar neu begründet (`manifest-10.0.100` als einzige Publikationslinie, `manifest-10.0.400` existiert nicht, Band `10.0.4xx` nicht auflösbar) — Z. 63–72
- [x] Step `Pin SDK band for this job`: `global.json` `10.0.100` / `rollForward: latestPatch` beibehalten; Kommentar angepasst (Resolver würde sonst höheres Image-SDK wählen, Manifest-Linie `10.0.100` muss getroffen werden) — Z. 74–85
- [x] Neuer Step `Select Xcode` vor `Install .NET MAUI workload`: `sudo xcode-select -s /Applications/Xcode_27.2.app`, `xcode-select -p` + `xcodebuild -version` ins Log, Fail-fast per `set -euo pipefail` — Z. 87–98
- [x] Rollback-File um `"microsoft.net.sdk.maui"` erweitert (`printf` schreibt iOS- + MAUI-Pin) — Z. 114–118
- [x] Fallback-Regel für MAUI-Pin umgesetzt (kombinierter Retry → iOS-only-Rollback → MAUI fällt auf Band-Default) — Z. 119–138; deckt die Plan-Vorgabe „Fallback: Band-Default ≥ `10.0.110`" ab
- [x] `dotnet workload list` als Log-Nachweis plus Verifikation der aufgelösten MAUI-Version ≥ `10.0.110` (`RESOLVED_MAUI`, harter `::error::`-Abbruch unterhalb, gilt für Pin und Fallback) — Z. 147–162
- [x] Pack-Bereinigung auf `Microsoft.Maui.*` erweitert, iOS-Glob `Microsoft.iOS.Sdk.net10.0_*` beibehalten; MAUI-Bereinigung behält exakt die aufgelöste Version — Z. 163–174
- [x] Überholte Kommentarbegründungen ersetzt (kein „`10.0.400`-Feed"-/„Beta-Xcode"-Vorbehalt mehr; Begründung Pin gegen TPV-Auflösung/Band-Drift) — Z. 63–68, 74–80, 100–110

### `.github/workflows/staging-ci.yml`

- [x] Job `package-ios`: `runs-on: macos-latest` → `xcode-27`, erklärender Kommentar (`macos-latest`/`macos-26` nur Xcode 26.x; `xcode-27`-Image trägt Xcode 27.0/27.1/27.2) — Z. 260–263
- [x] Kein `maui-workload-version`-Override im Aufruf — Action-Default gilt, wie im Plan festgelegt — Z. 272–283

### `scripts/iOS-Deployment.ps1`

- [x] Neuer Switch-Parameter `-SkipToolchainCheck` mit HelpMessage — Z. 83–84
- [x] Neue Funktion `Get-ToolchainCheckScript` (Muster `Get-TransporterFindScript`): Bash-Check liefert `xcode-select -p`, `xcodebuild -version` (Major, Beta-Erkennung über Versionsstring + Developer-Dir), `xcrun --sdk iphoneos --show-sdk-version` und `Microsoft.iOS.Sdk.net10.0_*`-/`Microsoft.Maui.*`-Packversionen unter `_DotNetRootRemoteDirectory` + `/usr/local/share/dotnet` (+ `~/.dotnet`) als `KEY:value`-Zeilen; `PACK_SEARCH:0`-Signalisierung für nicht ermittelbare Packliste — Z. 555–611
- [x] Neue Funktion `Assert-MacToolchain` (Muster `Assert-TransporterAvailable`): `Invoke-MacCapture`-Ausführung, Versionsprotokoll, Verstoßliste (Xcode < 27, Beta, iOS-SDK < 27, kein `Microsoft.iOS.Sdk`-27.x-Pack, MAUI < `10.0.110`); `-Strict` = Abbruch mit Behebungshinweisen (Xcode-Installation/`xcode-select`, Pair-to-Mac-/VS-Update oder Standalone-`dotnet workload install`, `-SkipToolchainCheck`), sonst Warnung; Packliste nicht ermittelbar = Warnung; SSH-Fehlerbehandlung (`$LASTEXITCODE -eq 255`) — Z. 613–705
- [x] `Invoke-Store`: `Assert-MacToolchain -Strict` nach `Assert-TransporterAvailable` und vor `Update-BuildNumber` (kein `CFBundleVersion`-Verbrauch bei Abbruch) — Z. 938–945
- [x] Action-Dispatch: warnende `Assert-MacToolchain`-Aufrufe in `build` (Z. 1453), `simulator` (SSH- und Mac-Pfad, Z. 1458/1461) und `device` (SSH- und Mac-Pfad, Z. 1471/1477) vor dem Build-Schritt; `upload` und `list` ungeprüft; `-SkipToolchainCheck` wirkt global über die Funktion selbst — Z. 1449–1493
- [x] Header-Kommentar: Werkzeugvoraussetzung Xcode ≥ 27 / `Microsoft.iOS.Sdk` 27.x / MAUI ≥ `10.0.110`, Abbruch bei `store`, Warnung sonst, Opt-out — Z. 20–23

### `scripts/iOS-Deployment.md`

- [x] Neuer Abschnitt „Werkzeugvoraussetzungen (iOS 27 / Xcode 27)": Xcode ≥ 27 (Release, kein Beta), `Microsoft.iOS.Sdk` 27.x, MAUI ≥ SR 11 (`10.0.110`), Referenzpaarung `Xcode_27.2` / `27.2.10000` / `10.0.110` auf Band `10.0.1xx` (`global.json` `10.0.100`) — Z. 45–62
- [x] Bereitstellungswege dokumentiert: Xcode-Installation + `sudo xcode-select`; .NET-Seite via VS-Update/Pair-to-Mac-Remote-Cache oder Standalone-.NET `10.0.1xx` + `dotnet workload install maui-ios` — Z. 64–74
- [x] Verhalten von `Assert-MacToolchain` dokumentiert: Abbruch bei `store` vor Buildnummer-Bump, Warnung bei `build`/`simulator`/`device`, keine Prüfung bei `upload`/`list`, Opt-out `-SkipToolchainCheck` — Z. 76–86; zusätzlich im `store`-Ablauf verankert — Z. 122–126

### `docs/CI-CD.md`

- [x] Job `package-ios` in der Workflow-Übersicht/Tabelle und im Ablaufdiagramm — Z. 19, 44
- [x] Action `package-ios` in der Composite-Action-Liste — Z. 64–66
- [x] Abschnitt „iOS-Paketierung (Job `package-ios`)" mit Pinning-Mechanismus: Runner-Label `xcode-27`, `xcode-select`-Pin `Xcode_27.2.app`, Band `10.0.1xx` / `global.json` `10.0.100`, Inputs `ios-workload-version` `27.2.10000` + `maui-workload-version` `10.0.110`, dreistufiges Rollback-Update, `RESOLVED_MAUI`-Prüfung ≥ `10.0.110`, Pack-Bereinigung — Z. 68–104

### Konsistenz `...-nachricht-von-apple-tasks.md`

- [x] Alle Versionsangaben stimmen mit dem korrigierten Plan überein: Band `10.0.1xx`, `global.json` `10.0.100`, `ios-workload-version` `27.2.10000`, `maui-workload-version` `10.0.110`, `Xcode_27.2.app`, `xcode-27` — Z. 5–23; die einzige Nennung von `manifest-10.0.400` ist die korrekte Aussage „existiert nicht" (Z. 8)
- [x] Repo-seitige Tasks (1–13, 15, 16) als „Erledigt" markiert und im Code verifizierbar; externe Tasks (14, 17–19) korrekt als „Offen / Extern" gekennzeichnet

## Nicht oder abweichend umgesetzte Planelemente

Keine.

## Hinweise

- **Externe/manuelle Schritte (keine Implementierungslücken):**
  - Werkzeugkette auf dem Build-Mac bereitstellen (Xcode ≥ 27 installieren + `xcode-select`; .NET-Seite via VS-Update/Pair-to-Mac oder Standalone-`dotnet workload install`) — Tasks-Task 14, Plan-Schritt 5
  - Außerplanmäßiger Wirksamkeitsnachweis: lokaler `store`-Lauf + Apple-Antwortmail (Task 17, Plan-Schritt 7, Akzeptanzszenario 2)
  - Manuelle iOS-27-Laufzeit-Abnahme durch Anwender auf TestFlight-Build (Task 18, Plan-Schritt 8, Akzeptanzszenario 3)
  - Verifikation über den nächsten Staging-RC-CI-Lauf + Apple-Antwortmail (Task 19, Plan-Schritt 9, Akzeptanzszenario 1)
- **Implementierungsdetails über dem Plan-Minimum (deckungsgleich, nicht abweichend):** Die Action implementiert den MAUI-Fallback konkret als dreistufiges Update (kombiniert → Retry → iOS-only) mit anschließender harter `RESOLVED_MAUI ≥ 10.0.110`-Prüfung — das erfüllt die Plan-Regel „Fallback: Band-Default ≥ `10.0.110`, per `dotnet workload list` verifiziert" vollständig.
- `Assert-MacToolchain` sucht Pack-Verzeichnisse zusätzlich unter `$HOME/.dotnet` (neben `_DotNetRootRemoteDirectory` und `/usr/local/share/dotnet`) — harmlose Erweiterung der Suchpfade, konsistent mit der Plan-Absicht „Standalone-Installationsorte".
