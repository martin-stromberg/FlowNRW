# Umsetzungsplan: Apple-Meldung ITMS-90725 — iOS-Build auf SDK 27 (Xcode 27) heben

## Übersicht

Der iOS-Paketierungsjob `package-ios` wird so umgestellt, dass das erzeugte `release-ios.ipa` künftig mit dem iOS-27-SDK (Release-Xcode ≥ 27) gebaut wird. Parallel wird der lokale Auslieferungspfad `scripts/iOS-Deployment.ps1` (Pair-to-Mac) als weiterhin auslieferungsfähiger Pfad mit einer erzwingenden Werkzeugketten-Prüfung (Xcode ≥ 27, kein Beta; iOS-Workload 27.x) versehen. Betroffen sind ausschließlich CI-/Build-Artefakte und das Deployment-Skript samt Dokumentation: die Composite Action `.github/actions/package-ios/action.yml` (Workload-Pinning, SDK-Band, neue Xcode-Auswahl), der Job `package-ios` in `.github/workflows/staging-ci.yml` (Runner-Label), `scripts/iOS-Deployment.ps1` (neue Versionsprüfung) sowie `docs/CI-CD.md` und `scripts/iOS-Deployment.md`. Der Wirksamkeitsnachweis erfolgt außerplanmäßig über einen realen lokalen `store`-Lauf (Build + Upload + Apple-Antwortmail) plus manueller Laufzeit-Abnahme durch den Anwender — nicht erst mit dem nächsten Staging-RC. Kein App-Code, keine Testsuiten.

**Zum Planungszeitpunkt verifiziert (2026-11-20):**

- Finale `Microsoft.iOS.Sdk`-27.x-Packversionen für `net10.0` existieren: `27.0.10722` (erfordert Xcode 27.0), `27.1.10018` (Xcode 27.1), `27.2.10000` (Xcode 27.2; erschienen 2026-11-09, Quelle: dotnet/macios-Release-Notes `dotnet-10.0.1xx-xcode27.2.10000`). Die Release-Notes empfehlen für 27.2 „MAUI SR 11 `10.0.110`+".
- `Microsoft.Maui.Sdk` `10.0.110` (SR 11, 2026-09-22) enthält „[iOS/MacCatalyst] Adopt scene lifecycle and AppActions dispatch" — die von Apple ab Xcode 27 erzwungene UIScene-Lifecycle-Adoption; die MAUI-Manifest-Linie `microsoft.net.sdk.maui.manifest-10.0.100` endet bei `10.0.110` (ein `10.0.120`-Manifest existiert nicht).
- `macos-latest` = `macos-26` enthält nur Xcode 26.x (Default 26.6) — kein Xcode ≥ 27. Ein separates, seit 2026-10-15 allgemein verfügbares Image `xcode-27` (macOS 27.0.1) trägt die Release-Versionen Xcode 27.0, 27.1 (Default) und 27.2 sowie .NET SDKs u. a. `10.0.403` (für die Band-Wahl unerheblich — `setup-dotnet` und `global.json` pinnen das Band).
- Die iOS-Workload-Manifeste werden ausschließlich auf der Linie `microsoft.net.sdk.ios.manifest-10.0.100` publiziert (`26.5.10318`, `27.0.10722`, `27.1.10018`, `27.2.10000`) — `manifest-10.0.400`-Pakete (ebenso `-10.0.200`/`-10.0.300`) existieren nicht; auch das Workload-Set `10.0.401` referenziert iOS-Manifeste als `…/10.0.100`. `dotnet workload update --from-rollback-file` ist daher nur unter SDK-Band `10.0.1xx` auflösbar — unter `10.0.4xx` schlägt die Manifest-Auflösung fehl (empirisch verifiziert, 2026-11-20).

## Scope-Klärung

Verbindliche fachliche Abgrenzung vor der Umsetzung — übernommen bzw. konkretisiert aus dem Abschnitt `Fachlicher Scope` der `requirement.md`:

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Ein iOS-Release-Paketierungslauf — auf zwei auslieferungsfähigen Pfaden: (a) Job `package-ios` im Workflow `staging-ci.yml`, (b) lokaler Lauf von `scripts/iOS-Deployment.ps1 -Action store` über den Pair-to-Mac-Pfad. Beide publizieren `FlowNRW` als `net10.0-ios`-Binary, signieren, verpacken `.ipa` und laden per `iTMSTransporter` zu App Store Connect/TestFlight |
| Auslösende Ereignisse | CI-Pfad: Push auf `staging`, wenn `version`-Job eine neue Release-Version ermittelt (`needs.version.outputs.changed == 'true'`) und `vars.IOS_SIGNING_ENABLED == 'true'`. Lokaler Pfad: manueller Skriptaufruf durch den Anwender — für diese Änderung konkret der außerplanmäßige Wirksamkeitsnachweis-Lauf direkt nach der Umsetzung |
| Erwartete Wirkung | Hochgeladene `.ipa` ist mit iOS SDK ≥ 27 (Release-Xcode ≥ 27) gebaut; die nächste Apple-Antwortmail enthält kein ITMS-90725 mehr. Der Nachweis wird **sofort nach Umsetzung** über den lokalen `store`-Lauf erbracht, nicht erst fristnah (Apple-Deadline April 2027) |
| Umgang mit fehlenden Objekten | **CI:** Release-Xcode ≥ 27 wird durch Wechsel des Runner-Labels auf `xcode-27` bereitgestellt (in `macos-latest` nicht vorhanden) und zusätzlich per `xcode-select` explizit ausgewählt — schlägt die Auswahl fehl, bricht der Job ab statt mit falschem SDK zu bauen. Finale `Microsoft.iOS.Sdk`-27.x-Packversion: vorhanden (`27.2.10000`). MAUI für die UIScene-Lifecycle-Pflicht: `Microsoft.NET.Sdk.Maui` wird auf `10.0.110` gepinnt (Fallback: Band-Default, sofern ≥ `10.0.110`). **Lokaler Mac:** Xcode ≥ 27 (Release) muss auf dem Build-Mac installiert und per `xcode-select` aktiv sein; die passende .NET-SDK-/Workload-Kombination (Band `10.0.1xx`, iOS-Workload 27.x, MAUI ≥ SR 11) wird über die Pair-to-Mac-Provisionierung (Visual-Studio-Update auf Windows) bzw. alternativ eine Standalone-Installation auf dem Mac bereitgestellt. Die neue Skript-Versionsprüfung erzwingt diesen Zustand: Bei unpassender Werkzeugkette bricht `store` vor dem Build ab. Fehlt eines dieser Objekte zum Umsetzungszeitpunkt unerwartet, wird die Umstellung terminiert statt mit Beta-Werkzeugen zu bauen |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | Kein automatisierter Geräte-/Simulator-Test in CI: Die iOS-27-Laufzeit-Verifikation erfolgt als manuelle Abnahme durch den Anwender auf eigenem Gerät (TestFlight-Build installieren, App starten, Grundfunktionen bestätigen); ergänzend kann der lokale Simulator (`-Action simulator`, Screenshots) genutzt werden | Das Projekt hat keine Gerätetest-Infrastruktur; die manuelle Abnahme ist der vom Anwender gewählte, ausreichende Nachweis. Die UIScene-Lifecycle-Adoption kommt mit der gepinnten MAUI-Version (≥ `10.0.110`) | bestätigt (Anwender, 2026-11-20) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Zeitpunkt der Umstellung | Sofort umsetzen — kein Warten auf die Frist | Anwender-Entscheidung (2026-11-20). Die komplette Release-Kette ist verfügbar; frühes Umstellen lässt Zeit, etwaige Folgeprobleme (z. B. UIScene-Verhalten) bis April 2027 zu beobachten und nötigenfalls zurückzudrehen |
| Runner für `package-ios` | `runs-on` von `macos-latest` auf `xcode-27` wechseln | `macos-latest` (`macos-26`) enthält ausschließlich Xcode 26.x; `xcode-27` ist seit 2026-10-15 GA und trägt Release-Xcode 27.0/27.1/27.2. Eine Selbstinstallation von Xcode im Job ist langsam und fehleranfällig |
| Xcode-Auswahl | Neuer expliziter Step `sudo xcode-select -s /Applications/Xcode_27.2.app` + `xcodebuild -version`-Ausgabe in der Action | Der Image-Default (aktuell 27.1) driftet mit Image-Updates; der explizite Pin hält das Paar Xcode ↔ `Microsoft.iOS.Sdk`-Version konsistent und lässt den Job bei fehlendem Xcode sofort scheitern statt still gegen SDK 26 zu bauen |
| iOS-Workload-Pin | `ios-workload-version`-Default auf `'27.2.10000'` (neueste .NET-10-Servicing-Version für Xcode 27.2) | Passt exakt zum gepinnten Xcode 27.2; die Manifestversion liegt auf der Publikationslinie `manifest-10.0.100`. Bestehender Rollback-Mechanismus bleibt unverändert. Zum Umsetzungszeitpunkt wird die jeweils neueste stabile, paarweise kompatible Kombination gewählt — nie Beta-Xcode (Anwender-Vorgabe) |
| .NET-SDK-Band | `dotnet-version` `'10.0.1xx'` und `global.json`-Pin `10.0.100` (`rollForward: latestPatch`) — unverändert | iOS-Workload-Manifeste werden ausschließlich als `manifest-10.0.100` publiziert — `manifest-10.0.400` existiert nicht, ein Rollback-Pin unter Band `10.0.4xx` scheitert an der Manifest-Auflösung (empirisch verifiziert, 2026-11-20). Das Band wird von `setup-dotnet` bereitgestellt, unabhängig von den im Image vorinstallierten SDKs |
| MAUI-Workload-Pin | Rollback-File um `"microsoft.net.sdk.maui"` erweitern über neuen optionalen Input `maui-workload-version` (Default `'10.0.110'`); im CI-Lauf via `dotnet workload list` verifizieren. Fallback: Band-Default ≥ `10.0.110`, falls die gepinnte ID/Version nicht auflöst | Xcode-27-Builds stürzen ohne UIScene-Lifecycle-Adoption beim Start ab (Apple-Anforderung); erst MAUI SR 11 (`10.0.110`) adoptiert sie. Die bandseitig aufgelöste `maui-ios`-Version ist ohne Pin nicht deterministisch — der Pin macht sie es. Ziel-Pin `10.0.110` (zugleich das Maximum der Manifest-Linie `10.0.100` — ein `10.0.120`-Manifest existiert nicht) und Fallback-Regel sind Anwender-Vorgaben (2026-11-20) |
| Pack-Bereinigung | Bestehende Bereinigung von `Microsoft.iOS.Sdk.net10.0_*` beibehalten und auf `Microsoft.Maui.*`-Packs desselben Musters erweitern | Gleiche TPV-Falle: vorinstallierte neuere Maui-Packs würden den Pin unterlaufen. Die iOS-Glob passt unverändert weiter (`net10.0_27.2`-Packs matchen) |
| Lokaler Pair-to-Mac-Pfad | Bleibt auslieferungsfähig und wird als unterstützter Pfad dokumentiert; zusätzlich erhält `iOS-Deployment.ps1` eine erzwingende Werkzeugketten-Prüfung | Anwender-Entscheidung (2026-11-20): Der Pfad hat früher vollständig funktioniert (Deployment + Upload inkl. Simulator-Screenshots) und ist das Vehikel für den außerplanmäßigen Wirksamkeitsnachweis — deshalb reicht ein Doku-Hinweis nicht, die Prüfung muss im Skript liegen |
| Werkzeugprüfung im Skript | Neues `Assert-MacToolchain` + `Get-ToolchainCheckScript` nach dem Muster von `Assert-TransporterAvailable`/`Get-TransporterFindScript`: Bash-Prüfskript auf dem Mac via `Invoke-MacCapture`, Ausgabe als `KEY:value`-Zeilen; harter Abbruch für `store` (vor `Update-BuildNumber`, damit kein `CFBundleVersion`-Bump verbrannt wird), Warnung für `build`/`simulator`/`device`; Opt-out `-SkipToolchainCheck` | Wiederverwendung des etablierten Remote-Check-Musters (Base64-Bash via SSH bzw. lokal); die echten Versionswerte (`xcodebuild -version`, `xcrun --sdk iphoneos --show-sdk-version`, installierte Pack-Verzeichnisse) sind zuverlässiger als indirekte Annahmen. `store` ist die einzige Aktion, die Store-Artefakte erzeugt — dort muss der Check hart sein; die übrigen Aktionen erzeugen keine Upload-Artefakte (Warnung genügt). `upload` baut nichts, braucht den Check nicht. Der Opt-out verhindert, dass ein Check-Fehlschlag (z. B. SSH-Ausfall auf reinen `build`-Pfaden) legitime lokale Builds blockiert |
| Werkzeugbereitstellung auf dem Build-Mac | Zweistufig, manuell bzw. halbautomatisch: (a) Xcode ≥ 27 (Release) auf dem Mac installieren (Apple Developer Downloads/App Store) und per `sudo xcode-select -s` aktivieren; (b) .NET-Seite über Visual-Studio-Update auf Windows — Pair-to-Mac provisioniert dann das passende SDK/Workload-Set in den Remote-Cache (`_DotNetRootRemoteDirectory`); Alternative: Standalone-.NET-SDK `10.0.1xx` + `dotnet workload install maui-ios` direkt auf dem Mac. Der neue Skript-Check verifiziert das Ergebnis | Pair-to-Mac synchronisiert die .NET-Werkzeugkette ohnehin vom Windows-Host — der dokumentierte, bisher funktionierende Weg. Xcode selbst kann nicht gepairt werden und ist zwingend eine Mac-Installation |
| Wirksamkeitsnachweis | Außerplanmäßiger realer `store`-Lauf über `iOS-Deployment.ps1` direkt nach Umsetzung (Build auf dem Mac → IPA-Validierung → `iTMSTransporter`-Upload → Apple-Antwortmail ohne ITMS-90725) | Anwender-Entscheidung (2026-11-20): Der Pfad hat früher vollständig funktioniert, alle nötigen Informationen liegen in den Umgebungsvariablen. Der lokale Lauf liefert den Nachweis früher und unabhängig vom nächsten Staging-RC und prüft zugleich die neue lokale Werkzeugkette end-to-end |
| Plan-Korrektur 2026-11-20 | SDK-Band bleibt `10.0.1xx` (`global.json` `10.0.100`); MAUI-Pin ist `10.0.110` | Empirische Verifikation ergab: `microsoft.net.sdk.ios.manifest-10.0.400` existiert nicht (ebenso `-10.0.200`/`-10.0.300`) — die iOS-Manifest-Linie ist ausschließlich `10.0.100`, daher ist Band `10.0.4xx` für den Rollback-Pin-Mechanismus unmöglich; das MAUI-Manifest endet bei `10.0.110` (kein `10.0.120`-Manifest) |
| Plan-Erweiterung 2026-11-20 | App-seitige iOS-27-Migration im Umsetzungslauf ergänzt | Durch den echten `store`-Lauf empirisch gefunden und behoben: (a) **UIScene-Migration** — `MT7199` erzwingt `UIApplicationSceneManifest` + `MauiUISceneDelegate` (`SceneDelegate.cs` neu, `DidEnterBackground` vom App- zum Scene-Delegate, Manifest mit `__MAUI_DEFAULT_SCENE_CONFIGURATION__` in `Platforms/iOS/Info.plist`); (b) **`PrivacyInfo.xcprivacy`** — fehlte komplett, wird als `BundleResource` mit `LogicalName` ins Bundle-Root gepackt (Store-Validierung verlangt es); (c) **`CFBundleLocalizations` `en`+`de`** — Store-Validierungsinvariante; (d) **`EnableIos=true` in den Response-Files** — das iOS-TFM ist auf Windows sonst nicht im Restore-Spec (commit `d969806` hatte es hinter die Bedingung gelegt); (e) **Trailing-Slash-Normalisierung** für `_DotNetRootRemoteDirectory` (`.../dotnet` + `packs` → sonst `dotnetpacks`) |

## Programmabläufe

### iOS-Paketierungslauf CI (geändert)

1. Push auf `staging` → Job `version` ermittelt neue RC-Version → Job `package-ios` startet auf Runner `xcode-27` (macOS 27, Xcode 27.x installiert).
2. Action `package-ios`: `Setup .NET` installiert SDK-Band `10.0.1xx`; Step `Pin SDK band for this job` schreibt `global.json` `10.0.100`/`latestPatch`.
3. **Neu:** Step „Select Xcode": `sudo xcode-select -s /Applications/Xcode_27.2.app`, danach `xcodebuild -version` und `xcode-select -p` ins Log. Schlägt `xcode-select` fehl (Xcode-Pfad im Image nicht mehr vorhanden), bricht der Job hier ab.
4. `Install .NET MAUI workload`: Rollback-File enthält nun `microsoft.net.sdk.ios` = `ios-workload-version` **und** `microsoft.net.sdk.maui` = `maui-workload-version`; `dotnet workload update --from-rollback-file` → `dotnet workload install maui-ios ios --skip-manifest-update`; Bereinigung vorinstallierter `Microsoft.iOS.Sdk.net10.0_*`- und `Microsoft.Maui.*`-Packs anderer Version; `dotnet workload list` ins Log als Nachweis (verifiziert u. a. die aufgelöste MAUI-Version ≥ `10.0.110`, Ziel `10.0.110`).
5. `Restore`, Signing-Keychain, `Publish (ios)` — unverändert; `dotnet publish` kompiliert nun gegen das iOS-27.2-SDK des selektierten Xcode.
6. `Collect iOS package`, `Upload to TestFlight`, `Remove signing keychain` — unverändert.
7. Nachgelagert: Job `prerelease` hängt `release-ios.ipa` ans GitHub-Prerelease; Apple-Antwortmail zum Upload dient als externer Nachweis (kein ITMS-90725).

Beteiligte Artefakte: `.github/workflows/staging-ci.yml` (Job `package-ios`, `prerelease`), `.github/actions/package-ios/action.yml` (Steps `Setup .NET`, `Pin SDK band for this job`, neu `Select Xcode`, `Install .NET MAUI workload`, `Publish (ios)`, `Upload to TestFlight`).

### Lokaler Store-Lauf mit Werkzeugprüfung (geändert — Wirksamkeitsnachweis)

1. Anwender startet `scripts/iOS-Deployment.ps1 -Action store` (unter Windows via Pair-to-Mac + SSH oder direkt auf dem Mac); alle Secrets/Parameter kommen aus den `FLOWNRW_IOS_*`-Umgebungsvariablen.
2. `Invoke-Store`: `Assert-StorePrerequisites` (unverändert) → `Assert-CodesigningForAction` (unverändert) → `Assert-TransporterAvailable` (unverändert) → **neu:** `Assert-MacToolchain`: `Invoke-MacCapture` führt `Get-ToolchainCheckScript` auf dem Mac aus. Das Bash-Skript ermittelt `xcode-select -p`, `xcodebuild -version` (Major ≥ 27, Versionsstring ohne `beta`), `xcrun --sdk iphoneos --show-sdk-version` (≥ 27) und die installierten `Microsoft.iOS.Sdk.net10.0_*`-/`Microsoft.Maui.*`-Packversionen unter dem effektiven Remote-.NET-Root (`_DotNetRootRemoteDirectory`, Pair-to-Mac-Cache) sowie `/usr/local/share/dotnet/packs`. Ergebnis als `KEY:value`-Zeilen zurück; das Skript protokolliert die gefundenen Versionen und bricht bei Verstoß mit klaren Behebungshinweisen ab — **vor** `Update-BuildNumber`, damit kein `CFBundleVersion`-Wert verbrannt wird.
3. `Update-BuildNumber` → `Invoke-Build` (`dotnet publish` via Pair-to-Mac gegen die Mac-seitige iOS-27-Toolchain) → `Get-LatestIpa` → `Copy-ApiKeyToMac` → `Get-MacIpaPath` → `Invoke-IpaValidation` → `Invoke-StoreUpload` — unverändert.
4. Apple verarbeitet den Upload; die Antwortmail ohne ITMS-90725 ist der externe Wirksamkeitsnachweis.
5. Für `build`/`simulator`/`device` läuft dieselbe Prüfung warnend (kein Abbruch, Opt-out `-SkipToolchainCheck`); `upload` und `list` prüfen nicht.

Beteiligte Artefakte: `scripts/iOS-Deployment.ps1` (`Invoke-Store`, `Invoke-Build`, Dispatch, neu `Assert-MacToolchain`/`Get-ToolchainCheckScript`), `Invoke-MacCapture`, `Assert-TransporterAvailable` (Muster).

## Neue Klassen

Keine Klassen — es sind ausschließlich YAML-, PowerShell- und Dokumentations-Artefakte betroffen. Neu sind der Action-Input `maui-workload-version`, ein neuer Step in der Composite Action sowie zwei neue Funktionen und ein Parameter in `iOS-Deployment.ps1` (siehe unten).

## Änderungen an bestehenden Klassen

Es gibt keine Klassen im Code-Sinn. Die Änderungen betreffen CI- und Skript-Artefakte:

### `.github/actions/package-ios/action.yml` (Composite Action)

- **Neuer Input:** `maui-workload-version` (optional, Default `'10.0.110'`) — Version des `Microsoft.NET.Sdk.Maui`-Manifests im Rollback-File; Kommentar verweist auf die UIScene-Lifecycle-Pflicht ab Xcode 27.
- **Geänderter Input:** `ios-workload-version` — Default `'26.5.10318'` → `'27.2.10000'`; Beschreibung bleibt (Release-Xcode-Kompatibilität).
- **Step `Setup .NET`:** `dotnet-version` bleibt `'10.0.1xx'`; Kommentar wird neu begründet (iOS-Manifeste werden ausschließlich als `manifest-10.0.100` publiziert — `manifest-10.0.400` existiert nicht, Band `10.0.4xx` ist für den Rollback-Pin nicht auflösbar).
- **Step `Pin SDK band for this job`:** `global.json`-Version bleibt `10.0.100`; Kommentar angepasst (Resolver würde sonst ggf. ein 11.x-SDK des Images wählen; die Manifest-Linie `10.0.100` muss getroffen werden).
- **Neuer Step `Select Xcode`** (vor `Install .NET MAUI workload`): `xcode-select` auf `Xcode_27.2.app`, Versionsausgabe ins Log.
- **Geänderter Step `Install .NET MAUI workload`:** Rollback-File um `microsoft.net.sdk.maui` erweitert; Pack-Bereinigung um `Microsoft.Maui.*` ergänzt; `dotnet workload list` als Log-Nachweis (u. a. MAUI-Version = Pin `10.0.110` bzw. Band-Default ≥ `10.0.110`); Kommentare Z. 72–77 auf die neue Begründung aktualisiert (Pin gegen TPV-Auflösung und Band-Drift, kein „Beta-Xcode"-Vorbehalt mehr).

### `.github/workflows/staging-ci.yml` (Workflow „Pre-Release")

- **Job `package-ios` (Z. 256–287):** `runs-on: macos-latest` → `xcode-27`; Kommentar ergänzt, warum ein dediziertes Xcode-27-Image nötig ist (`macos-latest` enthält nur Xcode 26.x). Übergabe des neuen Inputs `maui-workload-version` ist nicht nötig — der Action-Default gilt (Annahme: keine Override-Notwendigkeit).

### `scripts/iOS-Deployment.ps1` (Deployment-Skript, 1331 Zeilen)

- **Neuer Parameter:** `[switch]$SkipToolchainCheck` (param-Block, Z. 43–81) — Opt-out für die Werkzeugprüfung in `build`/`simulator`/`device`/`store`.
- **Neue Funktion `Get-ToolchainCheckScript`** (im Stil von `Get-TransporterFindScript`, Z. 475–522): Liefert Bash-Code, der auf dem Mac `xcode-select -p`, `xcodebuild -version` (Major-Version, Beta-Erkennung über Versionsstring), `xcrun --sdk iphoneos --show-sdk-version` sowie die Verzeichnisse installierter `Microsoft.iOS.Sdk.net10.0_*`- und `Microsoft.Maui.*`-Packs unter dem effektiven Remote-.NET-Root (`_DotNetRootRemoteDirectory`) und `/usr/local/share/dotnet/packs` ausliest und als `KEY:value`-Zeilen ausgibt (Muster: `FOUND:`-Zeile in `Assert-TransporterAvailable`, Z. 524–546).
- **Neue Funktion `Assert-MacToolchain`** (im Stil von `Assert-TransporterAvailable`): Ruft `Invoke-MacCapture` (Z. 170–180) mit dem Check-Skript, wertet die Zeilen aus, gibt gefundene Versionen protokolliert aus. Bei Verstoß (Xcode < 27 oder Beta, iOS-SDK < 27, keine `Microsoft.iOS.Sdk`-27.x-Packversion, MAUI < `10.0.110`): für `store` Abbruch mit Behebungshinweisen (Xcode-Installation + `xcode-select`, Workload-Bereitstellung via Pair-to-Mac/VS-Update oder `dotnet workload install` auf dem Mac, Hinweis auf `-SkipToolchainCheck`); für `build`/`simulator`/`device` Warnung. SSH-Fehlerbehandlung analog Z. 532–536.
- **Geänderte Funktion `Invoke-Store` (Z. 779–792):** Aufruf `Assert-MacToolchain -Strict` nach `Assert-TransporterAvailable` und **vor** `Update-BuildNumber` — ein Abbruch darf keinen `CFBundleVersion`-Bump verbrennen.
- **Geänderter Action-Dispatch (Z. 1287–1325):** Für `build`, `simulator` und den macOS-Pfad von `device` warnender Aufruf von `Assert-MacToolchain` vor dem Build-Schritt (nach `Assert-PairToMacAvailable`); respektiert `-SkipToolchainCheck`. `upload` und `list` bleiben ungeprüft (kein Build). Header-Kommentar (Z. 3–41): Werkzeugvoraussetzung Xcode ≥ 27 / iOS-Workload 27.x kurz benennen.

### `scripts/iOS-Deployment.md` (Dokumentation)

- Neuer Abschnitt „Werkzeugvoraussetzungen (iOS 27 / Xcode 27)": Auslieferungen via Pair-to-Mac erfordern auf dem Build-Mac Xcode ≥ 27 (Release, kein Beta) und `Microsoft.iOS.Sdk` 27.x samt MAUI ≥ SR 11 (`10.0.110`); Verweis auf die CI-Pinning-Versionen als Referenzpaarung (`Xcode_27.2` / `27.2.10000` / `10.0.110`, Band `10.0.1xx`).
- Bereitstellungswege dokumentieren: Xcode-Installation auf dem Mac + `sudo xcode-select`; .NET-/Workload-Seite über Visual-Studio-Update auf Windows (Pair-to-Mac provisioniert den Remote-Cache) oder Standalone-.NET + `dotnet workload install` auf dem Mac.
- Verhalten der neuen `Assert-MacToolchain`-Prüfung dokumentieren: Abbruch bei `store`, Warnung bei `build`/`simulator`/`device`, Opt-out `-SkipToolchainCheck`.

### `docs/CI-CD.md` (Dokumentation)

- Workflow-Tabelle und Composite-Action-Liste um Job/Action `package-ios` ergänzen; kurzer Abschnitt zum iOS-SDK-/Xcode-Pinning (Action-Input `ios-workload-version`, `maui-workload-version`, Runner-Label `xcode-27`, `xcode-select`-Pin).

## Datenbankmigrationen

Keine.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| Xcode-Pfad im `Select Xcode`-Step (CI) | `/Applications/Xcode_27.2.app` muss auf dem Runner existieren | `xcode-select` schlägt fehl → Job bricht ab (bewusstes Fail-fast statt stillem SDK-26-Build) |
| `ios-workload-version` / `maui-workload-version` im Rollback-File (CI) | Version muss im `10.0.100`-Manifest (Publikationslinie unter SDK-Band `10.0.1xx`) veröffentlicht sein; falls `microsoft.net.sdk.maui`/`10.0.110` nicht auflöst: Fallback auf Band-Default, sofern ≥ `10.0.110` (per `dotnet workload list` verifiziert) | `dotnet workload update --from-rollback-file` schlägt fehl → Job rot, kein Upload; bzw. `dotnet workload list` zeigt MAUI < `10.0.110` → Pin/Fallback nachziehen |
| Xcode-Version auf dem Build-Mac (lokal, `Assert-MacToolchain`) | `xcodebuild -version`: Major ≥ 27, kein `beta` im Versionsstring; `xcrun --sdk iphoneos --show-sdk-version` ≥ 27 | `store`: Abbruch vor Buildnummer-Bump mit Behebungshinweis; `build`/`simulator`/`device`: Warnung |
| iOS-Workload auf dem Build-Mac (lokal) | Mindestens eine `Microsoft.iOS.Sdk.net10.0_27.*`-Packversion im Remote-.NET-Root oder `/usr/local/share/dotnet/packs`; MAUI-Pack ≥ `10.0.110` | Wie oben: Abbruch bei `store`, Warnung sonst; nicht ermittelbare Packliste = Warnung (kein harter Fehler, da Pfade je nach Installation variieren) |
| Erzeugtes `.ipa` | Genau ein distinct Basename (bestehende Regel, unverändert) | Job/Validierung schlägt in `Collect iOS package` bzw. `Invoke-IpaValidation` fehl |

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `ios-workload-version` (Action-Input, `package-ios/action.yml`) | bestehender Input, neuer Default | `'27.2.10000'` | `Microsoft.iOS.Sdk`-Pack-/Manifestversion für iOS-27-SDK |
| `maui-workload-version` (Action-Input, `package-ios/action.yml`) | neuer Input | `'10.0.110'` | `Microsoft.NET.Sdk.Maui`-Manifestversion ≥ SR 11 (UIScene-Lifecycle); Fallback Band-Default ≥ `10.0.110` |
| `dotnet-version` (`Setup .NET`-Step) | Step-Parameter | `'10.0.1xx'` | SDK-Band der Manifest-Publikationslinie `10.0.100` |
| `global.json`-Pin (`Pin SDK band`-Step) | CI-generierte Datei | `10.0.100`, `rollForward: latestPatch` | Band-Pinning innerhalb des Jobs |
| `SkipToolchainCheck` (`iOS-Deployment.ps1`) | neuer Switch-Parameter | aus (Prüfung aktiv) | Opt-out der lokalen Werkzeugprüfung |

Keine neue Repository-Variable, keine `appsettings`- oder Laufzeitkonfiguration — `global.json` wird weiterhin nur im CI-Workspace erzeugt und nicht committed. Für den lokalen Pfad werden keine neuen Umgebungsvariablen benötigt: Der Anwender bestätigt, dass alle notwendigen Informationen bereits in den `FLOWNRW_IOS_*`-Variablen liegen.

## Seiteneffekte und Risiken

- **UIScene-Lifecycle (Apple-Pflicht ab Xcode 27):** Apps, die mit Xcode 27 gebaut werden, benötigen die UIScene-Lifecycle-Adoption, sonst Crash beim Start. Sie kommt mit MAUI `10.0.110`+ — darum der `maui-workload-version`-Pin. Restrisiko: Das Zusammenspiel mit App-seitigem Lebenszyklus-Code wird nicht automatisiert verifiziert; es wird durch die manuelle Geräte-Abnahme des Anwenders (Akzeptanzszenario 3) abgedeckt.
- **Runner-Image-Drift:** Image-Updates des `xcode-27`-Labels können installierte Xcode-Nebenversionen oder vorinstallierte .NET-SDKs ändern. Der explizite `xcode-select`-Pin scheitert dann laut sichtbar (gewollt) statt still falsch zu bauen; Gegenmittel: gepinnte Version in der Action anpassen.
- **`xcode-27`-Label-Verfügbarkeit:** Das Label ist GA, aber ein separates Image — sollte GitHub das Label-Modell ändern, muss `runs-on` nachgezogen werden (gleiches Muster wie der jetzige Wechsel).
- **Lokale Werkzeugkette hinterher:** Xcode 27 und die 27.x-Workload müssen auf dem Build-Mac erst bereitgestellt werden (VS-Update/Pair-to-Mac-Sync oder Standalone-Installation) — das ist ein manueller Schritt außerhalb des Repos, den der Plan als eigenen Umsetzungsschritt führt und den `Assert-MacToolchain` verifiziert. Bis dahin würde `store` bewusst abbrechen.
- **Fehlprüfung des lokalen Checks:** Variieren die Pack-Pfade (Pair-to-Mac-Cache vs. Standalone-.NET), könnte die Workload-Erkennung leer ausfallen — darum ist sie warnend statt hart; Xcode-/SDK-Version bleibt das harte Gate, weil sie das tatsächliche SDK des Builds bestimmt.
- **Buildnummer-Verbrauch:** Der Wirksamkeitsnachweis-`store`-Lauf erhöht `ApplicationVersion` im csproj (Opt-out `-NoBumpBuildNumber`); der diff geht in den Repo-Stand ein und wird ggf. vom nächsten CI-Lauf erneut hochgezählt — gewollt, da Apple jede `CFBundleVersion` nur einmal akzeptiert.
- **Build-Dauer/Kosten:** `xcode-27` ist arm64; das bisherige `macos-latest` war ebenfalls arm64 — keine Änderung der Architektur oder des Minutenfaktors zu erwarten. `timeout-minutes: 45` bleibt.
- **Andere Jobs/Workflows:** `release.yml`, `pr-staging-ci.yml`, `static-checks`, `build-and-test`, `build-and-package` haben keinen iOS-Bezug und laufen nicht auf macOS — unberührt. Der `maui-windows`-Workload-Pfad (`setup-maui-workload`) ist unberührt.
- **Überholte Kommentare:** Frühere Kommentarbegründungen („27.x-Manifeste nur im 10.0.400-Feed" bzw. „Workload-Set `10.0.404`") sind faktisch überholt — es existiert kein `manifest-10.0.400`-Feed; die einzige Publikationslinie ist `10.0.100`, und Xcode 27 ist Release. Die Begründungen werden entsprechend ersetzt.

## Umsetzungsreihenfolge

1. **Ziel-Paarung verifizieren**
   - Voraussetzungen: keine
   - Beschreibung: Zum Implementierungszeitpunkt prüfen: (a) neueste finale `Microsoft.iOS.Sdk`-27.x-Version + zugehöriges Workload-Set/SDK-Band in den dotnet/macios-Release-Notes, (b) im `xcode-27`-Image-Readme installierte Release-Xcode-Versionen und Pfade, (c) `Microsoft.NET.Sdk.Maui`-Manifestversion ≥ `10.0.110` im Band-Manifest, (d) korrekte Rollback-ID für MAUI (`microsoft.net.sdk.maui`). Ergebnis: bestätigtes Tripel Xcode-Pfad / `ios-workload-version` / `maui-workload-version` — verbindlich die neueste stabile, paarweise kompatible Kombination, nie Beta-Xcode (Planungsstand: `Xcode_27.2.app` / `27.2.10000` / `10.0.110`, Band `10.0.1xx` / `global.json` `10.0.100`). Steht keine Release-Kette, Umstellung terminieren statt Beta zu bauen.

2. **Action `package-ios` umstellen**
   - Voraussetzungen: Schritt 1 (bestätigte Versionspaarung); `actions/setup-dotnet@v6` (vorhanden)
   - Beschreibung: In `.github/actions/package-ios/action.yml`: Input `ios-workload-version`-Default heben; Input `maui-workload-version` ergänzen; `Setup .NET` bleibt auf `10.0.1xx`; `global.json`-Pin bleibt `10.0.100`; neuen Step `Select Xcode` vor dem Workload-Install einfügen; Rollback-File um `microsoft.net.sdk.maui` erweitern; Pack-Bereinigung um `Microsoft.Maui.*` ergänzen; `dotnet workload list`-Ausgabe ergänzen; Kommentare Z. 52–55, 61–65, 72–77 neu begründen.

3. **Runner-Label im Workflow wechseln**
   - Voraussetzungen: keine
   - Beschreibung: `staging-ci.yml` Job `package-ios` (Z. 260): `runs-on: macos-latest` → `xcode-27`, inkl. erklärendem Kommentar.

4. **Werkzeugprüfung in `iOS-Deployment.ps1` einbauen**
   - Voraussetzungen: Schritt 1 (Schwellwerte/Xcode-Pfad bekannt); vorhandene Helfer `Invoke-MacCapture`, `Get-TransporterFindScript`, `Assert-TransporterAvailable` als Muster
   - Beschreibung: `Get-ToolchainCheckScript` + `Assert-MacToolchain` neu anlegen; Parameter `-SkipToolchainCheck`; Aufruf in `Invoke-Store` vor `Update-BuildNumber` (strikt) sowie warnend in den `build`-/`simulator`-/`device`-Dispatch-Pfaden; Header-Kommentar aktualisieren.

5. **Werkzeugkette auf dem Build-Mac bereitstellen** (manueller Schritt, durch Anwender bzw. assistiert)
   - Voraussetzungen: Schritt 1 (Ziel-Paarung bekannt)
   - Beschreibung: Xcode ≥ 27 (Release) auf dem Mac installieren (Apple Developer Downloads/App Store) und per `sudo xcode-select -s` aktivieren; .NET-Seite per Visual-Studio-Update auf Windows aktualisieren, damit Pair-to-Mac SDK-Band `10.0.1xx` + iOS-Workload 27.x + MAUI ≥ `10.0.110` in den Remote-Cache provisioniert (alternativ Standalone-.NET + `dotnet workload install maui-ios` auf dem Mac). Ergebnis mit dem neuen Check aus Schritt 4 verifizieren (`-Action build` als Trockenlauf).

6. **Dokumentation nachziehen**
   - Voraussetzungen: Schritte 2–5 (damit die Doku den realen Stand beschreibt)
   - Beschreibung: `docs/CI-CD.md`: Job `package-ios` + Action `package-ios` in die Tabellen aufnehmen, Pinning-Mechanismus kurz beschreiben. `scripts/iOS-Deployment.md`: Abschnitt Werkzeugvoraussetzungen (Xcode ≥ 27, iOS-Workload 27.x, MAUI ≥ SR 11), Bereitstellungswege auf dem Mac und Verhalten/Opt-out der neuen Prüfung dokumentieren.

7. **Außerplanmäßiger Wirksamkeitsnachweis (lokaler `store`-Lauf)**
   - Voraussetzungen: Schritte 4–5 (Check eingebaut, Mac-Toolchain bereit); `FLOWNRW_IOS_*`-Umgebungsvariablen gesetzt (laut Anwender vollständig vorhanden); Distribution-Zertifikat/Profil/API-Key auf dem Mac (früher funktionsfähig)
   - Beschreibung: `scripts/iOS-Deployment.ps1 -Action store` ausführen: Toolchain-Check → Buildnummer-Bump → `dotnet publish` via Pair-to-Mac → IPA-Validierung → `iTMSTransporter`-Upload. Transcript unter `logs/ios-deploy-*.log` auf Xcode-27-/SDK-27-Nachweis prüfen; danach Apple-Antwortmail abwarten: kein ITMS-90725. Bei Check-Abbruch: Mac-Toolchain nach Schritt 5 nachziehen und wiederholen.

8. **Manuelle iOS-27-Laufzeit-Abnahme durch Anwender**
   - Voraussetzungen: Schritt 7 (TestFlight-Build in Verarbeitung/verfügbar)
   - Beschreibung: Anwender installiert den TestFlight-Build auf eigenem Gerät (iOS 27), startet die App und bestätigt die Grundfunktionen (insb. kein Start-Crash → UIScene-Lifecycle greift). Ergänzend kann `-Action simulator` auf dem Mac genutzt werden (Simulator-Screenshots waren bereits möglich). Kein automatisierter CI-Gerätetest.

9. **Verifikation über nächsten Staging-RC-Lauf (CI-Pfad)**
   - Voraussetzungen: Schritte 2–3 gemergt auf `staging`; `vars.IOS_SIGNING_ENABLED == 'true'`; Secrets `IOS_*` gesetzt (vorhanden)
   - Beschreibung: Nächsten `staging`-Push mit Versionsänderung abwarten bzw. auslösen. Im `package-ios`-Log prüfen: `xcodebuild -version` = 27.x, `dotnet workload list` zeigt iOS `27.x` + MAUI `10.0.110` (bzw. Band-Default ≥ `10.0.110`), `Publish (ios)` erfolgreich, Upload erfolgreich. Danach Apple-Antwortmail abwarten: kein ITMS-90725. Bei Fehlschlag: Log-Diagnose (Manifest-Auflösung, TPV-Pack-Wahl), ggf. Versionen nachziehen.

## Tests

### Neue Tests

Keine neuen Testmethoden — es gibt keine Testinfrastruktur für CI-Artefakte und PowerShell-Deployment-Skripte, und die Anforderung ändert keinen ausführbaren Anwendungscode. Der Funktionsnachweis läuft über die realen Läufe selbst:

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| Lokaler `store`-Lauf-Transcript (`logs/ios-deploy-*.log`) | — (manueller Log-Check, Schritt 7) | `Assert-MacToolchain` meldet Xcode ≥ 27 / iOS-SDK ≥ 27 / Workload 27.x; `dotnet publish` erfolgreich; Upload erfolgreich |
| Toolchain-Check-Negativprobe | — (manuell, optional: Aufruf des Check-Skripts auf dem Mac vor der Toolchain-Aktualisierung) | `store` bricht bei Xcode < 27 vor dem Buildnummer-Bump ab; `build` warnt nur |
| Build-Log-Nachweis CI (`xcodebuild -version`, `dotnet workload list`, Publish-Erfolg) | — (manueller Log-Check im nächsten `package-ios`-Lauf) | Dass das Binary tatsächlich mit Xcode 27.x / Workload 27.x + MAUI `10.0.110` erzeugt wurde |
| Apple-Antwortmail-Check | — (manuell, nach TestFlight-Upload) | Kein ITMS-90725-Hinweis mehr |

### Betroffene bestehende Tests

Keine. Die Änderungen berühren weder `FlowNRW.Tests` (xUnit) noch die Node-Testskripte noch `tests/WindowsJourneyUiTests` — keine Signatur-, Verhaltens- oder Datenstrukturänderung im Code.

### E2E-Tests (primärer Funktionsnachweis)

**Keine automatisierten E2E-Tests erforderlich — Begründung:** Die Anforderung berührt keinen über UI oder Nutzeraktion in der App erreichbaren Ablauf; geändert werden ausschließlich Build-/Deployment-Artefakte. Der dem E2E-Gedanken entsprechende End-to-End-Nachweis ist der **reale außerplanmäßige `store`-Lauf über `iOS-Deployment.ps1`** (Build auf dem Mac → IPA-Validierung → App-Store-Upload → Apple-Antwortmail, Akzeptanzszenario 2) sowie der nächste Staging-RC-Lauf für den CI-Pfad (Akzeptanzszenario 1) — beide existieren nur als echte Läufe, nicht als automatisierbarer Test. Die iOS-27-Laufzeit-Verifikation ist als **manuelle Abnahme durch den Anwender** verankert (Akzeptanzszenario 3); ein automatisierter CI-Gerätetest ist ausdrücklich nicht Teil des Scopes (Scope-Ausschluss 1, bestätigt).

Anzupassende bestehende E2E-Tests: Keine.

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path CI: iOS-Build mit SDK 27 | Push auf `staging` mit neuer RC-Version, `IOS_SIGNING_ENABLED == 'true'` | Gemergte Umstellung; `xcode-27`-Image mit Release-Xcode 27.x; finale Workload-Versionen verfügbar | `package-ios` startet → SDK-Band `10.0.1xx` + `global.json`-Pin `10.0.100` → `xcode-select` auf Xcode 27.2 → Workload-Install iOS `27.x` + MAUI `10.0.110` → Restore → Signing → Publish gegen iOS-27-SDK → `.ipa` → `iTMSTransporter`-Upload | Build-Log zeigt Xcode 27.x und Workload 27.x + MAUI `10.0.110`; `release-ios.ipa` wird hochgeladen; Apple-Antwortmail enthält kein ITMS-90725 | Manueller Check CI-Log + Apple-Mail (Warte-/Folgeschritt: Mail trifft nach Upload ein — Zustand „Upload done, Mail ausstehend"; Wiederaufnahmeweg = Mail-Eingang prüfen, kein erneuter CI-Lauf nötig) |
| 2 | Wirksamkeitsnachweis lokal: `store`-Lauf über Pair-to-Mac | Anwender/Agent ruft `scripts/iOS-Deployment.ps1 -Action store` auf | Mac-Toolchain bereitgestellt (Xcode ≥ 27 aktiv, iOS-Workload 27.x, MAUI ≥ `10.0.110`); `FLOWNRW_IOS_*`-Variablen gesetzt; neue `Assert-MacToolchain`-Prüfung im Skript | Toolchain-Check grün → `Update-BuildNumber` → `dotnet publish` via Pair-to-Mac → `.ipa` → `Copy-ApiKeyToMac` → `Invoke-IpaValidation` → `iTMSTransporter -m upload` | Transcript `logs/ios-deploy-*.log` zeigt geprüfte Toolchain-Versionen und erfolgreichen Upload; Build erscheint nach Verarbeitung in TestFlight; Apple-Antwortmail enthält kein ITMS-90725 | Transcript-Log + Apple-Mail (Warte-/Folgeschritt wie Szenario 1: Zustand „Upload done, Mail ausstehend", Wiederaufnahme über Mail-Eingang) |
| 3 | Manuelle iOS-27-Laufzeit-Abnahme | TestFlight-Build aus Szenario 2 (oder 1) ist in App Store Connect verarbeitet | Anwender-Gerät mit iOS 27; TestFlight-Zugang eingerichtet | Anwender installiert den Build per TestFlight → startet App → prüft Grundfunktionen (Start ohne Crash, Hauptfluss bedienbar); optional ergänzend `-Action simulator` mit Screenshot auf dem Mac | App startet und funktioniert unter iOS 27 — insbesondere kein Start-Crash durch fehlende UIScene-Lifecycle-Adoption | Manuelle Abnahme durch Anwender (kein automatisierter Test) |
| 4 | Fehlende Voraussetzung lokal: Mac-Toolchain zu alt | `store`-Aufruf wie Szenario 2 | Mac hat noch Xcode 26.x aktiv bzw. keine 27.x-Workload | `Assert-MacToolchain` läuft vor `Update-BuildNumber` | Skript bricht mit roter Meldung + Behebungshinweisen ab; `ApplicationVersion` im csproj bleibt unverändert (kein Bump verbrannt); kein Build, kein Upload | Transcript-/Konsolen-Ausgabe |
| 5 | Fehlende Voraussetzung CI: gepinntes Xcode nicht im Image | Derselbe Job-Auslöser wie Szenario 1 | Image-Update hat `Xcode_27.2.app` entfernt/umbenannt | `Select Xcode`-Step läuft, `xcode-select` schlägt fehl | Job bricht früh und sichtbar rot ab — kein Upload, keine stille SDK-26-Auslieferung; Fehlermeldung zeigt den fehlenden Pfad | CI-Log des fehlgeschlagenen Steps |
| 6 | Fehlerfall CI: Workload-Version nicht im Band-Manifest | Derselbe Job-Auslöser wie Szenario 1 | Gepinnte `ios-workload-version`/`maui-workload-version` im `10.0.100`-Manifest nicht (mehr) verfügbar | `dotnet workload update --from-rollback-file` scheitert | Job rot im `Install .NET MAUI workload`-Step; kein Upload; Diagnose über Fehlermeldung + `dotnet workload search`/NuGet; für MAUI Fallback auf Band-Default ≥ `10.0.110` | CI-Log |
| 7 | Regression: Apple meldet erneut SDK-Problem | Upload erfolgreich (CI oder lokal), danach Apple-Mail | Build lief grün, aber `.ipa` trug SDK < 27 (z. B. TPV wählte vorinstalliertes Pack) | Mail-Auswertung → Build-Log/Transcript: welches `Microsoft.iOS.Sdk`-Pack hat `dotnet publish` tatsächlich verwendet → Pack-Bereinigung unvollständig? Lokaler Check umgangen? | ITMS-90725 in der Mail trotz grünem Builds → Folge-Issue zur Bereinigungs-/TPV-Analyse | Apple-Antwortmail + CI-Log/Transcript |

## Offene Punkte

Keine.
