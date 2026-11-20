# Anforderung – Apple-Meldung ITMS-90725: iOS-Build auf SDK 27 (Xcode 27) heben

**Kundenanforderung (Original):** „We noticed one or more issues with a recent delivery for the following app: FlowNRW, App Apple ID 6820315191, Version 1.0.0, Build 156. Although delivery was successful, you may want to correct the following issues in your next delivery. Once you've corrected the issues, upload a new binary to App Store Connect. ITMS-90725: SDK version issue - This app was built with the iOS 26.5 SDK. Starting April 2027, iOS and iPadOS apps must be built with the iOS 27 SDK or later, included in Xcode 27 or later, in order to be uploaded to App Store Connect or submitted for distribution. – Apple Developer Relations"

**Korrektur/Annahmen bei der Übersetzung:** Die Meldung ist eine Vorab-Warnung mit Frist April 2027, kein akuter Fehler — die Auslieferung von Build 156 war erfolgreich. „Kunde" ist hier Apple Developer Relations; die Anforderung betrifft ausschließlich die iOS-Build-Pipeline, nicht den App-Code. Das gebaute iOS-SDK wird im Projekt nicht durch die App selbst, sondern durch die in `.github/actions/package-ios/action.yml` gepinnte `Microsoft.iOS.Sdk`-Workload-Version (aktuell `26.5.10318`) und die Xcode-Version des `macos-latest`-Runners bestimmt.

## Fachliche Zusammenfassung

Für den nächsten iOS-Upload an App Store Connect muss das Binary mit dem iOS 27 SDK (enthalten in Xcode 27 oder neuer) gebaut sein; der aktuelle CI-Build erzeugt noch ein Binary auf Basis des iOS 26.5 SDK. Umzustellen ist die iOS-Paketierungspipeline: die gepinnte `Microsoft.iOS.Sdk`-Workload-Version ist auf eine 27.x-Version zu heben, die zu einem Release-Xcode ≥ 27 kompatibel ist, und die Runner-/SDK-Band-Pinning-Logik ist entsprechend mitzuziehen. Es ist eine reine CI-/Build-Infrastruktur-Änderung ohne Auswirkung auf das Laufzeitverhalten der App.

## Fachlicher Scope

- **Verarbeitungseinheit:** Ein iOS-Release-Paketierungslauf — der Job `package-ios` im Workflow `staging-ci.yml` („Pre-Release"), der die `FlowNRW`-App als `net10.0-ios`-Binary publiziert, signiert, als `release-ios.ipa` verpackt und per `iTMSTransporter` an App Store Connect/TestFlight hochlädt.
- **Auslösende Ereignisse:** Push auf den `staging`-Branch, bei dem der `version`-Job eine neue Release-Version ermittelt (`needs.version.outputs.changed == 'true'`) und `vars.IOS_SIGNING_ENABLED == 'true'` gesetzt ist.
- **Erwartete Wirkung:** Die erzeugte und hochgeladene `.ipa` ist mit iOS SDK ≥ 27 (Xcode ≥ 27, Release-Version) gebaut; App Store Connect meldet für die nächste Auslieferung kein ITMS-90725 mehr. Nachgelagert sichtbar: kein entsprechender Hinweis in der Apple-Antwortmail nach dem Upload.
- **Umgang mit fehlenden Objekten:** Xcode 27 (Release) muss auf dem `macos-latest`-Runner-Image vorhanden bzw. auswählbar sein — falls nicht, ist die Xcode-Version im Workflow/der Action explizit zu selektieren oder das Runner-Image zu wechseln. Es muss eine veröffentlichte, finale `Microsoft.iOS.Sdk`-27.x-Packversion existieren, die mit einem Release-Xcode kompatibel ist (Beta-Xcode-Builds werden vom App Store abgelehnt). Fehlt eines dieser Objekte zum Umsetzungszeitpunkt noch, ist die Umstellung zu terminieren statt mit Beta-Werkzeugen zu bauen.
- **Scope-Ausschlüsse:** Keine Änderung am App-Code, an Features oder am Laufzeitverhalten; keine Anpassung des Windows-Builds (`build-and-package`, `maui-windows`-Workload); keine sofortige Neu-Auslieferung — Apple verlangt die Korrektur erst ab der nächsten Delivery bzw. ab April 2027; keine Verwendung von Xcode-Beta-Versionen; keine Änderung an Codesigning, Provisioning oder Upload-Mechanismus (Secrets/Keychain/iTMSTransporter bleiben unverändert).

## Betroffene Klassen und Komponenten

Es sind keine Datenmodell-, Logik- oder UI-Klassen betroffen. Betroffen sind ausschließlich CI-/Build-Artefakte:

- `.github/actions/package-ios/action.yml` – zentrale Stelle:
  - Input `ios-workload-version` mit Default `'26.5.10318'` (Z. 44–47): auf eine `Microsoft.iOS.Sdk`-27.x-Version zu heben, die mit Release-Xcode ≥ 27 kompatibel ist.
  - `Setup .NET`-Step `dotnet-version: '10.0.1xx'` (Z. 59) und `global.json`-Pinning auf `10.0.100` (Z. 68): Der Kommentar (Z. 52–55) beschreibt, dass 27.x-Manifeste nur im 10.0.400-Feed liegen — der Umstieg auf eine 27.x-Workload erfordert daher voraussichtlich das SDK-Band `10.0.4xx` (Band und Feed-Zuordnung zum Umsetzungszeitpunkt verifizieren).
  - Workload-Install-Step (Z. 78–97, Rollback-File + `--skip-manifest-update` + Pack-Bereinigung): Mechanismus bleibt, die gepinnte Version und ggf. die Pack-Glob `Microsoft.iOS.Sdk.net10.0_*` (Z. 94) sind auf die neue Band/Version abzustimmen.
  - Erklärende Kommentare (Z. 52–55, 72–77) sind mit der neuen Begründung zu aktualisieren.
- `.github/workflows/staging-ci.yml` – Job `package-ios` (Z. 256–287), `runs-on: macos-latest` (Z. 260): prüfen, ob das `macos-latest`-Image Xcode ≥ 27 als Default/Option enthält; ggf. explizite Xcode-Auswahl (z. B. `xcode-select` oder eine Setup-Action) ergänzen.
- `scripts/iOS-Deployment.ps1` / `scripts/iOS-Deployment.md` – lokaler Build-/Deploy-Pfad via Pair-to-Mac: die dort genutzte Mac-seitige Xcode-Installation muss für die nächste Auslieferung ebenfalls ≥ 27 sein; mindestens ein Dokumentationshinweis.
- `docs/CI-CD.md` – Pipeline-Dokumentation ggf. um den iOS-SDK-Pinning-Mechanismus bzw. die neue Version ergänzen.
- Tests: Keine Unit-/UI-Tests betroffen; Verifikation erfolgt über den nächsten CI-Lauf (Workload-/Xcode-Version im Build-Log) und die Apple-Antwortmail nach dem Upload.

## Implementierungsansatz

1. **Verfügbarkeit klären:** Zum Umsetzungszeitpunkt feststellen, welche finale `Microsoft.iOS.Sdk`-27.x-Packversion existiert und welches .NET-SDK-Band (`10.0.4xx` oder neuer) deren Manifeste ausliefert, sowie welche Xcode-Versionen das aktuelle `macos-latest`-Runner-Image enthält.
2. **Pinning heben:** `ios-workload-version`-Default in `package-ios/action.yml` auf die gewählte 27.x-Version setzen; `dotnet-version` und das `global.json`-Pinning auf das passende SDK-Band umstellen; Kommentare und ggf. die Pack-Bereinigungs-Glob anpassen.
3. **Xcode sicherstellen:** Auf dem Runner explizit eine Xcode-Version ≥ 27 auswählen (falls `macos-latest` noch ältere Defaults setzt), damit `dotnet publish` tatsächlich gegen das iOS-27-SDK kompiliert — der Workload-Pin allein garantiert die Xcode-Version nicht.
4. **Verifikation:** Nächster Staging-RC-Lauf: im Build-Log Workload-Version (`dotnet workload list`), Xcode-Version (`xcodebuild -version`) und das erzeugte `.ipa` prüfen; nach dem TestFlight-Upload sicherstellen, dass die Apple-Antwortmail kein ITMS-90725 mehr enthält.
5. **Dokumentation nachziehen:** `docs/CI-CD.md` und `scripts/iOS-Deployment.md` an die neue SDK-/Xcode-Basis anpassen.

## Konfiguration

Keine neue Anwendungs- oder Laufzeitkonfiguration erforderlich. Die SDK-Version ist bereits über den Action-Input `ios-workload-version` (Default in `package-ios/action.yml`, Z. 47) parametrisierbar; die Änderung erfolgt im Default-Wert. Eine zusätzliche Repository-Variable ist nicht zwingend nötig (Annahme: der bestehende Input-Default reicht als einzige Wahrheitsquelle). Die SDK-Band- und `global.json`-Pins bleiben absichtlich CI-intern und werden nicht committed.

## Offene Fragen

1. **Verfügbarkeit finaler Werkzeuge:** Existiert zum Umsetzungszeitpunkt bereits eine finale `Microsoft.iOS.Sdk`-27.x-Version und ein Release-Xcode 27 auf den GitHub-Runnern? Apple nennt als Deadline April 2027; das Issue stammt vom 20.11.2026 — zu diesem Zeitpunkt könnte Xcode 27 noch Beta sein, und Beta-Builds werden vom App Store abgelehnt (Annahme: Umstellung erst, wenn die Release-Kette verfügbar ist; spätestens rechtzeitig vor April 2027).
2. **Richtiges SDK-Band:** Liefert das 10.0.4xx-Band die finale 27.x-iOS-Workload, oder ist für iOS 27 bereits ein neueres .NET-SDK (z. B. .NET 11) vorgesehen? Der bisherige Kommentar in `action.yml` (Z. 52–55) beschreibt nur den Zustand „aktuell 27.x = Beta-Anforderung" und ist zum Umsetzungszeitpunkt neu zu bewerten.
3. **Zeitpunkt:** Soll der nächste iOS-Build sofort auf iOS 27 SDK umgestellt werden (sobald verfügbar), oder bewusst terminlich nahe an der Frist, um Kompatibilitätsrisiken mit `SupportedOSPlatformVersion` 15.0 (`FlowNRW/FlowNRW.csproj`) zu minimieren?
4. **Lokaler Deploy-Pfad:** Soll auch der Pair-to-Mac-Weg (`scripts/iOS-Deployment.ps1`) als auslieferungsfähig gelten und damit dokumentiert auf Xcode ≥ 27 gepflegt werden, oder ist ausschließlich der CI-Build maßgeblich?
