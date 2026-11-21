# Test-Ergebnisse

**Zeitstempel:** 2026-11-20 22:58:30
**Änderungsstand:** iOS-Plattformdateien (`SceneDelegate.cs` neu, `AppDelegate.cs`, `Info.plist`, `PrivacyInfo.xcprivacy` neu), `FlowNRW.csproj` (iOS-`BundleResource`), `scripts/iOS-Deployment.ps1` (`EnableIos`, Slash-Normalisierung), CI-YAMLs + Doku.

## Zusammenfassung

**Status:** Alle Tests bestanden

Der Windows-Solution-Build liefert 0 Fehler und 0 Warnungen — die neuen iOS-Plattformdateien brechen den Windows-TFM-Build nicht (sie kompilieren wie erwartet nicht für `net10.0-windows`). Alle 303 .NET-Tests und alle 26 Node-Tests bestehen. Die geänderten Skript-/CI-/Plist-Artefakte bestehen ihre Syntax-Checks. Ein iOS-TFM-Compile wurde bewusst nicht ausgeführt (erfordert Mac); der reale iOS-27-Build via Pair-to-Mac ist bereits im Feature-Verzeichnis dokumentiert.

## Ergebnisse

| Befehl | Ergebnis | Anzahl |
|--------|----------|--------|
| `dotnet build FlowNRW.sln -c Release` | Bestanden | 0 Fehler, 0 Warnungen (9,0 s) |
| `dotnet test FlowNRW.sln -c Release --no-build` | Bestanden | 303/303 Tests (0 Fehler, 0 übersprungen) |
| `npm run test:release-version` (`node --test scripts/resolve-release-version.test.mjs scripts/generate-update-manifest.test.mjs`) | Bestanden | 26/26 Tests (0 Fehler) |
| `[scriptblock]::Create` auf `scripts/iOS-Deployment.ps1` | Bestanden | 0 Parse-Fehler |
| `python yaml.safe_load` auf `.github/actions/package-ios/action.yml` | Bestanden | valides YAML |
| `python yaml.safe_load` auf `.github/workflows/staging-ci.yml` | Bestanden | valides YAML |
| `python xml.dom.minidom.parse` auf `FlowNRW/Platforms/iOS/Info.plist` | Bestanden | wohlgeformtes XML |
| `python xml.dom.minidom.parse` auf `FlowNRW/Platforms/iOS/PrivacyInfo.xcprivacy` | Bestanden | wohlgeformtes XML |

**Kennzahlen gesamt:**

- Tests gesamt: 329 (303 xUnit + 26 Node)
- Bestanden: 329
- Fehlgeschlagen: 0
- Übersprungen: 0
- Syntax-/Wellformedness-Checks: 5/5 bestanden

## Hinweise

- `node_modules` war vorhanden; kein `npm ci` erforderlich.
- Kein iOS-TFM-Build ausgeführt (benötigt macOS-Build-Host). Die Kompilierbarkeit der iOS-Plattformdateien ist über den dokumentierten Pair-to-Mac-Build nachgewiesen.
- Die reale End-to-End-Abnahme (CI-Lauf auf `xcode-27`, TestFlight-Upload, Apple-Antwortmail) ist in dieser Umgebung nicht durchführbar und bleibt Anwender-Abnahme bzw. dokumentiertem CI-Lauf vorbehalten.

## Externe Abnahme – ausgeführter Pair-to-Mac-`store`-Lauf (2026-11-20)

| Schritt | Ergebnis |
|---|---|
| `Assert-MacToolchain` (strict) | bestanden: Xcode 27.2 (Release, Build 27B52), iPhoneOS-SDK 27.2, `Microsoft.iOS.Sdk.net10.0_27.2`/`27.2.10000`, `Microsoft.Maui.Sdk` `10.0.110` |
| Remote-Build `net10.0-ios` Release (Pair-to-Mac, `/usr/local/share/dotnet`, SDK 10.0.103) | erfolgreich, `FlowNRW.ipa` erzeugt |
| IPA-Validierung auf dem Mac | `codesign --verify` gültig; `embedded.mobileprovision` = App-Store-Profil „Flow AppStore"; `PrivacyInfo.xcprivacy` gültig; `CFBundleLocalizations` en/de; `ITSAppUsesNonExemptEncryption`=false; `UIApplicationSceneManifest` vorhanden |
| SDK-Nachweis im Bundle | `DTPlatformName=iphoneos`, `DTPlatformVersion=27.2`, `DTXcode=2720` (`27B52`), `MinimumOSVersion=15.0` |
| Upload via `iTMSTransporter` | erfolgreich (23,4 MB), Package-ID `798e9168-…` |
| App Store Connect Verarbeitung | Build `0.0.1 (9)` → `processingState: VALID` (App ID 6820315191) — **kein ITMS-90725** |

Offene anwendergebundene Abnahmen (außerhalb der Umgebung):

- Manuelle Laufzeit-Abnahme: TestFlight-Build `0.0.1 (9)` auf eigenem Gerät installieren, starten, Kernfunktion prüfen.
- Apple-Antwortmail (Eingang beim Anwender) — API-Status `VALID` ist bereits Nachweis; Mail dient der Dokumentation.
- Nächster regulärer `staging`-CI-Lauf: Log-Check `xcodebuild -version`/`dotnet workload list`/Upload.
