# Bestandsaufnahme: Apple-Meldung ITMS-90725 — iOS-Build auf SDK 27 (Xcode 27) heben

Analysiert wurden die CI-/Build-Artefakte der iOS-Paketierungspipeline, bezogen auf die Anforderung, das erzeugte iOS-Binary für die nächste App-Store-Connect-Auslieferung von iOS SDK 26.5 auf SDK ≥ 27 (Xcode ≥ 27) zu heben. Es sind keine Datenmodell-, Logik- oder UI-Klassen betroffen.

## Zusammenfassung

- **Zentrale Pinning-Stelle:** Der Composite-Action-Input `ios-workload-version` in `.github/actions/package-ios/action.yml` (Z. 44–47) trägt den Default `'26.5.10318'` — die einzige parametrisierte `Microsoft.iOS.Sdk`-Packversion im Repo.
- **SDK-Band-Pinning:** `Setup .NET` mit `dotnet-version: '10.0.1xx'` (Z. 56–59) plus einer nur im CI-Workspace erzeugten, nicht committeten `global.json` (`10.0.100`, Z. 66–70). Die Kommentare (Z. 52–55, 61–65, 72–77) begründen den Pin: 26.x-Manifeste existieren nur im 10.0.100-Feed; der 10.0.400-Feed liefere nur 27.x-Manifeste mit Beta-Xcode-Anforderung.
- **Workload-Install-Mechanismus:** Rollback-File (`microsoft.net.sdk.ios`) → `dotnet workload update --from-rollback-file` → `dotnet workload install maui-ios ios --skip-manifest-update` → Bereinigung vorinstallierter `Microsoft.iOS.Sdk.net10.0_*`-Packs anderer Version (Z. 78–91).
- **Kein Xcode-Versionspin:** Weder `staging-ci.yml` noch `package-ios/action.yml` noch `iOS-Deployment.ps1` enthalten ein `xcode-select` bzw. eine Xcode-Versionswahl. Die Xcode-Version des Builds ergibt sich implizit aus dem Default des `macos-latest`-Runner-Images (`staging-ci.yml` Z. 260) bzw. der lokalen Mac-Installation.
- **Job-Auslösung:** `package-ios` läuft nur bei Push auf `staging` mit ermittelter neuer Release-Version (`needs.version.outputs.changed == 'true'`) und `vars.IOS_SIGNING_ENABLED == 'true'` (`staging-ci.yml` Z. 256–287). Upload via `iTMSTransporter` mit Kandidatensuche und CDN-Fallback (Z. 186–236 der Action).
- **Lokaler Deploy-Pfad:** `scripts/iOS-Deployment.ps1` (1331 Zeilen) baut `net10.0-ios` (hartkodiert, Z. 85) via Pair-to-Mac/SSH ohne eigenes Workload- oder Xcode-Pinning — abhängig von der Xcode-/.NET-Installation auf dem Build-Mac.
- **Projektkonfiguration:** `FlowNRW/FlowNRW.csproj` zielt auf `net10.0-ios` nur unter macOS bzw. mit `EnableIos` (Z. 11); `SupportedOSPlatformVersion` für iOS ist `15.0` (Z. 44); Development-Signing-Defaults (Z. 49–53) werden in CI per `-p:` überschrieben. Kein `global.json` im Repo.
- **Dokumentationslücke:** `docs/CI-CD.md` beschreibt die App als „Windows-only MAUI-App" (Z. 6) und erwähnt den Job `package-ios` / die Composite Action `package-ios` nicht — der iOS-Pfad ist dort undokumentiert.
- **Keine Pipeline-Tests:** Für die CI-Artefakte (YAML-Actions, `iOS-Deployment.ps1`) existieren keine automatisierten Tests.
- **Test-Ausgangszustand:** Alle ausführbaren Projekttests grün — `dotnet test`: 303/303 bestanden (FlowNRW.Tests, xUnit, net10.0); `npm run test:release-version`: 26/26 bestanden; `dotnet build FlowNRW.sln -c Release` erfolgreich inkl. MAUI-App (net10.0-windows). Keine bekannten Fehlschläge. Verbleibende Lücke: Der betroffene iOS-Paketierungspfad ist auf Windows nicht ausführbar (macOS + Signing-Secrets erforderlich) — Nachweis in [inventory/tests.md](inventory/tests.md).

## Details

- [CI-/Build-Artefakte](inventory/ci-artifacts.md)
- [Tests](inventory/tests.md)
