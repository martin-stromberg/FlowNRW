# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### .github/workflows/staging-ci.yml (Job `prerelease`, `needs:`) — vorab bestehend, nicht durch diesen Diff eingefuehrt

- **Hinweis (pre-existing auf `origin/staging`, hier zur Kenntnis)** — `prerelease` haengt ueber `needs: [detect-backmerge, version, package-ios]` am Job `package-ios`. Wird `package-ios` uebersprungen (sein `if` ist nur bei `vars.IOS_SIGNING_ENABLED == 'true'` erfuellt — der dokumentierte Default ist deaktiviert), dann wird `prerelease` wegen des impliziten `success()`-Statuschecks ebenfalls **uebersprungen**: GitHub-Regel "If a job fails or is skipped, all jobs that need it are skipped unless the jobs use a conditional expression that causes the job to continue." Konsequenz: ohne Signing-Variable entsteht gar kein RC-Prerelease (kein ZIP, kein `update.json`) — die Step-Bedingung `needs.package-ios.result == 'success'` im Download-Step antizipiert den Skip zwar, die Job-Bedingung tut es nicht. Der Diff aendert an `needs`/`if` nichts; die Zeilen sind unveraendert von `origin/staging` uebernommen. Falls `IOS_SIGNING_ENABLED` im Repo bereits gesetzt ist, ist der Pfad lebendig — sonst liegt das seit der Einfuehrung von `package-ios` so brach.

  Empfehlung (bei Gelegenheit, nicht diff-blockierend): `if` um Statusfunktionen erweitern, damit `prerelease` auch bei uebersprungenem `package-ios` laeuft, z. B. `if: !cancelled() && !failure() && needs.detect-backmerge.outputs.is_backmerge != 'true' && needs.version.outputs.changed == 'true'` (eine Statusfunktion unterdrueckt das implizite `success()`; `skipped` zaehlt nicht als `failure`).

## Verifikation vorheriger Befunde (alle behoben)

- review-code.1.md: `Assert-MacToolchain` fehlte im `device`-SSH-Pfad — jetzt vor `Invoke-DeviceViaSsh` (ps1 Z. 1471); Beta-Erkennung prueft jetzt `"$XVB $DEVDIR"` statt nur `xcodebuild -version` (check script); Fallback existiert jetzt real (dreistufiges Update + `RESOLVED_MAUI` aus `dotnet workload list`).
- review-code.2.md: `>= 10.0.110`-Pruefung laeuft jetzt unabhaengig vom Aufloesungspfad fuer `RESOLVED_MAUI` (action.yml Z. 158–162); die Warnung attributiert den MAUI-Pin erst, nachdem das iOS-only-Update erfolgreich war (Z. 129–136).
- review-code.3.md: `docs/CI-CD.md` beschreibt `dotnet workload list` jetzt als geparste Quelle (`RESOLVED_MAUI`), die `>= 10.0.110`-Verifikation und die dreistufige Retry-/Fallback-Logik.

## Extern verifizierte Annahmen

- `xcode-27`-Runner-Label: real, GA seit 2026-10-15 (GitHub Changelog); Image-Readme traegt `/Applications/Xcode_27.2.app` (27.2, Build 27B52), Default ist 27.1 — Kommentare stimmen.
- `macos-latest` = `macos-26` traegt ausschliesslich Xcode 26.x — Workflow-/Doku-Aussage stimmt.
- `Microsoft.NET.Sdk.iOS.Manifest-10.0.100` 27.2.10000 existiert; `...Manifest-10.0.400` liefert 404 — Band-Begruendung verifiziert. `Microsoft.NET.Sdk.Maui.Manifest-10.0.100` endet bei `10.0.110` — Kommentar/Doku stimmen.
- dotnet/macios-Release-Notes fuer 27.2.10000: "Xcode 27.2 required", "MAUI SR 11 10.0.110+ recommended", UIScene-Lifecycle-Pflicht — Paarung stimmt.
- awk-Korrektheit (`split(v,a,".")`-Literal, 3-Komponenten-Vergleich, `maui-ios`-Zeilenparse) und die generierte Toolchain-Check-Bash (`bash -n` + Packliste-Simulation) lokal verifiziert; PS1 parse sauber (`[Parser]::ParseFile`).

## Hinweise (kein Befund)

- Waehrend der Review erschien `M FlowNRW/FlowNRW.csproj` (`<ApplicationVersion>1 -> 5`) — verbrannter Buildnummer-Bump aus lokalem `store`-Lauf bzw. Test; vor dem Commit pruefen/verwerfen, gehoert nicht zum Feature-Diff.
- `Assert-MacToolchain` akzeptiert jedes `Microsoft.Maui.*`-Pack `>= 10.0.110` (any-of statt Kernpack) — toleriert, da MAUI-Packs pro SR einheitlich versioniert sind; keine Aenderung noetig.

## Geprüfte Dateien

- `.github/actions/package-ios/action.yml`
- `.github/workflows/staging-ci.yml`
- `scripts/iOS-Deployment.ps1`
- `scripts/iOS-Deployment.md`
- `docs/CI-CD.md`
