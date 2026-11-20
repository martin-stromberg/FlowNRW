# CI-/Build-Artefakte der iOS-Paketierung

Bestandsaufnahme der von der Anforderung betroffenen Artefakte (nur vorhandene Inhalte, keine Bewertung).

## `.github/actions/package-ios/action.yml` (Composite Action „Package iOS", 248 Zeilen)

### Inputs

| Input | Pflicht | Default | Zweck |
|-------|---------|---------|-------|
| `release-version` | ja | — | Version ohne führendes `v`, als `-p:Version` |
| `release-tag` | ja | — | Release-Tag (stabile Aufrufsignatur über die Packaging-Actions) |
| `codesign-key` | nein | `''` | Signing-Identität (`-p:CodesignKey`) |
| `provisioning-profile` | nein | `''` | Provisioning-Profil (`-p:CodesignProvision`) |
| `api-key-id` / `api-issuer-id` / `api-key-p8` | nein | `''` | App-Store-Connect-API-Key; wenn gesetzt, Upload via `iTMSTransporter` |
| `certificate-p12-base64` / `certificate-p12-password` | nein | `''` | Distribution-Zertifikat für die temporäre Keychain |
| `provisioning-profile-base64` | nein | `''` | `.mobileprovision`-Datei |
| `ios-workload-version` | nein | `'26.5.10318'` | **`Microsoft.iOS.Sdk`-Pack-/Manifestversion; die Beschreibung (Z. 45) verlangt Kompatibilität zu einem Release-Xcode („App Store rejects builds from beta Xcode")** |

### Steps

| Step | Zeilen | Inhalt |
|------|--------|--------|
| `Setup .NET` | 56–59 | `actions/setup-dotnet@v6` mit `dotnet-version: '10.0.1xx'`. Kommentar Z. 52–55: 26.x-Manifeste existieren nur im 10.0.100-Feed; der 10.0.400-Feed kenne nur 27.x-Workloads mit Beta-Xcode-Anforderung. |
| `Pin SDK band for this job` | 66–70 | Schreibt `global.json` im Workspace: `{"sdk":{"version":"10.0.100","rollForward":"latestPatch","allowPrerelease":false}}`, dann `dotnet --version`. Kommentar Z. 61–65: Das Runner-Image bringt zusätzlich ein 10.0.4xx-SDK mit; ohne `global.json` würde der Host-Resolver das höchste Band wählen. Die Datei wird absichtlich nicht committed. |
| `Install .NET MAUI workload` | 78–91 | Schreibt Rollback-File `$RUNNER_TEMP/workload-rollback.json` (`{"microsoft.net.sdk.ios": "<ios-workload-version>"}`) → `dotnet workload update --from-rollback-file` → `dotnet workload install maui-ios ios --skip-manifest-update`. Anschließend werden vorinstallierte iOS-Packs anderer Versionen aus `$HOME/.dotnet/packs/Microsoft.iOS.Sdk.net10.0_*` entfernt (TPV-Resolution würde sonst das neueste Pack wählen). Kommentar Z. 72–77 begründet das `--skip-manifest-update` (Restore würde Manifeste auf „latest" heben, aktuell 27.x mit Beta-Xcode-Anforderung). |
| `Restore` | 95–97 | `dotnet restore FlowNRW/FlowNRW.csproj -r ios-arm64` |
| `Install signing certificate and provisioning profile` | 103–128 | Temporäre Keychain mit Zufallspasswort; Import `.p12` + `.mobileprovision`; `set-key-partition-list` gegen GUI-Prompts. Nur wenn `certificate-p12-base64 != ''`. |
| `Publish (ios)` | 139–154 | `dotnet publish FlowNRW/FlowNRW.csproj -p:Version=<release-version> -p:ApplicationDisplayVersion=<version ohne Suffix> -p:ApplicationVersion=<git rev-list --count>+<run_attempt>-1 --configuration Release --framework net10.0-ios -p:RuntimeIdentifier=ios-arm64 -p:ArchiveOnBuild=true -p:CodesignKey=... -p:CodesignProvision=... --no-restore` |
| `Collect iOS package` | 156–180 | Sucht `*.ipa` unter `FlowNRW/bin/Release`, erfordert genau einen distinct Basename (Fehler bei 0 oder >1), bevorzugt `publish/`-Pfad, kopiert nach `release-ios.ipa`. |
| `Upload to TestFlight` | 186–236 | Nur wenn `api-key-p8 != ''`. Legt `AuthKey_<id>.p8` unter `~/.appstoreconnect/private_keys/`, sucht `iTMSTransporter` an Kandidatenpfaden (Transporter.app `/Applications` + `~/Applications`, `/usr/local/itms`, `ContentDeliveryServices.framework` unter `xcode-select -p`), Fallback: Standalone-`.pkg` von Apples CDN via `sudo installer`. Upload: `iTMSTransporter -m upload -assetFile release-ios.ipa -apiKey ... -apiIssuer ...`. |
| `Remove signing keychain` | 239–248 | `if: always()` — räumt Keychain und temporäre Signing-Dateien ab. |

**Relevanter Befund:** Die Action enthält **keine Xcode-Auswahl** (`xcode-select`, `xcodebuild -version`, Setup-Action o. ä.). Die Xcode-Version des Builds ergibt sich ausschließlich aus dem Runner-Default.

## `.github/workflows/staging-ci.yml` („Pre-Release", 352 Zeilen)

| Stelle | Zeilen | Inhalt |
|--------|--------|--------|
| Job `package-ios` | 256–287 | `needs: [detect-backmerge, version]`; Bedingung `is_backmerge != 'true' && version.outputs.changed == 'true' && vars.IOS_SIGNING_ENABLED == 'true'`; `runs-on: macos-latest`; `timeout-minutes: 45`; ruft `./.github/actions/package-ios` mit den Secrets `IOS_CODESIGN_KEY`, `IOS_PROVISIONING_PROFILE`, `IOS_API_KEY_ID`, `IOS_API_ISSUER_ID`, `IOS_API_KEY_P8`, `IOS_CERTIFICATE_P12_BASE64`, `IOS_CERTIFICATE_P12_PASSWORD`, `IOS_PROVISIONING_PROFILE_BASE64`; lädt `release-ios.ipa` als Artifact `release-ios` hoch. Kommentar Z. 254–255: Aktivierung ausschließlich über die Repo-Variable `IOS_SIGNING_ENABLED`. |
| Job `prerelease` | 289–344 | `needs: [..., package-ios]`; lädt das iOS-Artefakt herunter (nur bei `package-ios.result == 'success'`) und hängt `release-ios.ipa` an das GitHub-Prerelease an (Z. 300–303, 338). |
| Übrige Jobs | 20–252, 346–352 | `detect-backmerge`, `static-checks` (Windows, `maui-windows`-Workload), `build-and-test` (ubuntu, `FlowNRW.Tests` + Coverage ≥ 70 %), `release-scripts-test` (node), `version` (semantic-release-Dry-Run, RC-Nummer), `back-merge-skip`. Kein iOS-Bezug. |

**Relevanter Befund:** Auch im Workflow gibt es **keine Xcode-Versionswahl**; `macos-latest` allein bestimmt die Xcode-Version des iOS-Builds.

## Weitere Workflows

- `.github/workflows/release.yml` (156 Zeilen): Windows-only Release-Pfad (`build-and-package`, win-x64). Kein iOS-Bezug — iOS-Paketierung findet ausschließlich über `staging-ci.yml` statt (Repo-weite Suche nach `package-ios|ios-workload|xcode|macos` trifft nur `staging-ci.yml` und `package-ios/action.yml`).
- `.github/workflows/pr-staging-ci.yml` (196 Zeilen): identische Quality Gates wie `staging-ci.yml` (static checks, build & test, release scripts), ohne iOS-Jobs.
- `.github/actions/setup-maui-workload/action.yml` (20 Zeilen): `dotnet workload install maui-windows --skip-sign-check` — Windows-Workload, kein iOS-Pinning-Mechanismus.
- `.github/actions/build-and-package`, `security-scan`, `create-review-issue`: kein iOS-Bezug.

## `scripts/iOS-Deployment.ps1` (1331 Zeilen)

Lokaler Build-/Deploy-Pfad (Windows mit Pair-to-Mac oder direkt auf macOS). Aktionen `build`, `simulator`, `device`, `store`, `upload`, `list`, `menu` (Z. 44–45).

| Aspekt | Zeilen | Inhalt |
|--------|--------|--------|
| Framework | 85 | `$framework = "net10.0-ios"` — hartkodiert |
| Pair-to-Mac | 56–61, 90–95, 155–168 | `ServerAddress`/`ServerUser`/`ServerPassword`/`TcpPort`/`_DotNetRootRemoteDirectory` (Default: `~/Library/Caches/maui/PairToMac/SDKs/dotnet/`); Env-Variablen `FLOWNRW_IOS_MAC_*` |
| `Assert-StorePrerequisites` | 428–473 | Pflichtparameter, `.p8` außerhalb Repo, kein Development-Zertifikat für `store`/`upload`, SSH-Voraussetzung unter Windows |
| `Get-TransporterFindScript` | 475–522 | Bash-Suchliste für `iTMSTransporter` (Transporter.app, `/usr/local/itms`, `xcrun -f`, PATH, `ContentDeliveryServices.framework`, Spotlight) — dieselbe Kandidatenliste wie in `package-ios/action.yml` |
| `Update-BuildNumber` | 548–562 | Erhöht `<ApplicationVersion>` im csproj (Opt-out `-NoBumpBuildNumber`) |
| `Invoke-Build` | 840–913 | `dotnet build` bzw. `dotnet publish -f net10.0-ios -p:ArchiveOnBuild=true` mit Response-File (`RuntimeIdentifier`, Pair-to-Mac-Props, Signing-Props) |
| `Invoke-IpaValidation` | 687–757 | Mac-seitige Gates: `codesign --verify --deep --strict`, `get-task-allow=false`, `PrivacyInfo.xcprivacy` vorhanden + plist-Lint, `Info.plist`-Invarianten (`UIDeviceFamily` iPhone-only, `CFBundleLocalizations` en+de, `ITSAppUsesNonExemptEncryption=false`), Icon-Encoding (Info), `iTMSTransporter -m verify` mit dokumentiertem Fallback |
| `Invoke-StoreUpload` | 759–777 | `iTMSTransporter -m upload -assetFile <ipa> -apiKey ... -apiIssuer ...` |
| `Invoke-Store` / `Invoke-Upload` | 779–805 | Orchestrierung: Prerequisites → BuildNumber-Bump → Build → IPA → API-Key zum Mac → Validierung → Upload |
| `Invoke-DeviceViaSsh` / `Invoke-SimulatorViaSsh` | 1088–1238 | SSH-Delegierung der Geräte-/Simulator-Installation (`devicectl`/`simctl`) |

**Relevanter Befund:** Kein Workload-Pinning und keine Xcode-Versionswahl im lokalen Pfad — der Build verwendet, was auf dem Mac installiert ist (Visual-Studio-/Pair-to-Mac-.NET-Cache). Xcode-Abhängigkeit nur implizit über `xcode-select -p`/`xcrun`-Aufrufe (Z. 505, 1213 u. a.).

## `scripts/iOS-Deployment.md` (234 Zeilen)

Begleitdoku zu `iOS-Deployment.ps1`: Voraussetzungen (Distribution-Zertifikat, App-Store-Profil, API-Key `.p8` außerhalb Repo, `iTMSTransporter` seit Xcode 16 nicht mehr in Xcode enthalten — Suchpfade dokumentiert Z. 62–70, 159–179), Ablauf `store`/`upload` (Z. 77–107), bekannte Probleme (`hostpolicy.dll` unter Windows, `Microsoft.iOS`-Pack-Pfade, `ServerUser` = macOS-Kurzname, `altool`→`iTMSTransporter`), SSH-Delegation. **Kein Abschnitt zur benötigten iOS-SDK-/Xcode-Version.**

## `docs/CI-CD.md` (186 Zeilen)

Pipeline-Dokumentation. Beschreibt das Projekt als „Windows-only MAUI-App" (Z. 6). Die Workflow-Tabelle (Z. 38–50) listet `staging-ci.yml` nur als „Quality Gates + RC-Prerelease"; die Composite-Action-Liste (Z. 52–61) enthält `setup-maui-workload`, `create-review-issue`, `security-scan`, `build-and-package` — **`package-ios` und der Job `package-ios` sind dort nicht dokumentiert.** Keine Erwähnung von iOS-SDK- oder Xcode-Versionierung.

## `FlowNRW/FlowNRW.csproj` (82 Zeilen) — iOS-relevante Eigenschaften

| Eigenschaft | Zeilen | Wert / Bedingung |
|-------------|--------|-------------------|
| `TargetFrameworks` | 11 | `net10.0-ios` nur wenn `IsOSPlatform('osx')` oder `EnableIos == 'true'` |
| `TargetFrameworks` (Windows) | 46–48 | `net10.0-windows10.0.19041.0` unter Windows |
| `SupportedOSPlatformVersion` (iOS) | 43–45 | `15.0` (Bedingung `TargetPlatformIdentifier == 'ios'`) |
| `SupportedOSPlatformVersion`/`TargetPlatformMinVersion` (Windows) | 40–41 | `10.0.17763.0` |
| `CodesignKey`/`CodesignProvision`/`ProvisioningType` | 49–53 | `Apple Development: Created via API (466MVK852A)` / `VS: WildCard Development` / `automatic` — lokale Development-Defaults für `net10.0-ios`; CI überschreibt per `-p:` |
| `ApplicationDisplayVersion` | 30 | aus `$(Version)` mit Suffix-Stripping (`-rc.N`) |
| `ApplicationVersion` | 31 | `1` (wird in CI per `-p:` bzw. lokal per `Update-BuildNumber` gesetzt) |

## `global.json`

**Nicht im Repository vorhanden.** Wird nur im CI-Workspace der `package-ios`-Action erzeugt (`action.yml` Z. 68). Lokal bestimmt der Resolver das höchste installierte SDK.

## Versionierte Stellen im Überblick

| Stelle | Datei:Zeile | Aktueller Wert |
|--------|-------------|----------------|
| iOS-Workload-Version | `.github/actions/package-ios/action.yml:47` | `26.5.10318` |
| .NET-SDK-Band (Setup) | `.github/actions/package-ios/action.yml:59` | `10.0.1xx` |
| .NET-SDK-Pin (`global.json`, CI-generiert) | `.github/actions/package-ios/action.yml:68` | `10.0.100` |
| iOS-Pack-Bereinigungs-Glob | `.github/actions/package-ios/action.yml:87` | `Microsoft.iOS.Sdk.net10.0_*` |
| iOS-TargetFramework | `FlowNRW/FlowNRW.csproj:11`; `scripts/iOS-Deployment.ps1:85` | `net10.0-ios` |
| iOS-Mindestversion | `FlowNRW/FlowNRW.csproj:44` | `SupportedOSPlatformVersion 15.0` |
| iOS-Runner | `.github/workflows/staging-ci.yml:260` | `macos-latest` (Xcode-Version implizit, nicht gepinnt) |
