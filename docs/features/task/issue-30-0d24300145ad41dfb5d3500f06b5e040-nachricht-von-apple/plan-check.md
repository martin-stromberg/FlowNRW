# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

Da die `requirement.md` keine nummerierte Akzeptanzkriterien-Liste enthält, wurden die fachlichen Muss-Anforderungen aus `Fachliche Zusammenfassung`, `Fachlicher Scope` und `Betroffene Klassen und Komponenten` als Prüfbasis herangezogen.

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| iOS-Binary für nächste Auslieferung mit iOS SDK ≥ 27 (Release-Xcode ≥ 27) gebaut; kein ITMS-90725 mehr in der Apple-Antwortmail | Runner-Wechsel `macos-latest` → `xcode-27` (Schritt 3), neuer `Select Xcode`-Step (`Xcode_27.2.app`, Fail-fast), `ios-workload-version`-Default `27.2.10000` (Schritt 2) | Akzeptanzszenarien 1+2: CI-Log (`xcodebuild -version`, `dotnet workload list`) + Transcript `logs/ios-deploy-*.log` + Apple-Antwortmail | Abgedeckt |
| `Microsoft.iOS.Sdk`-Workload auf kompatible 27.x-Version heben; Band-/Feed-Zuordnung zum Umsetzungszeitpunkt verifizieren | Schritt 1 „Ziel-Paarung verifizieren" (Tripel Xcode-Pfad / iOS-Workload / MAUI-Version, nie Beta); `dotnet-version` → `10.0.4xx`, `global.json` → `10.0.400`/`latestPatch` | `dotnet workload list`-Ausgabe im Job (Schritt 2) als Log-Nachweis; Szenario 6 (Version nicht im Band-Manifest → Job rot) | Abgedeckt |
| Keine Beta-Werkzeuge (App Store lehnt Beta-Xcode ab); bei fehlender Release-Kette Umstellung terminieren | Explizit in Schritt 1 („nie Beta-Xcode", sonst terminieren), `Assert-MacToolchain` prüft Beta-String lokal; Scope-Klärung „Umgang mit fehlenden Objekten" | Szenarien 4+5 (fehlendes/zu alles Xcode → sichtbarer Abbruch statt stiller Falsch-Build) | Abgedeckt |
| Runner-/SDK-Band-Pinning-Logik mitziehen; Xcode-Version ggf. explizit selektieren (Workload-Pin allein garantiert Xcode nicht) | `runs-on` → `xcode-27` plus expliziter `xcode-select`-Pin mit `xcodebuild -version`-Ausgabe (Designentscheidungen Z. 36–37) | Akzeptanzszenario 1 (Log zeigt Xcode 27.x); Szenario 5 (Pin scheitert → Job bricht früh ab) | Abgedeckt |
| Lokaler Deploy-Pfad `scripts/iOS-Deployment.ps1`: Mac-seitige Xcode-Installation ≥ 27, mindestens Doku-Hinweis | Übererfüllt: Pfad bleibt auslieferungsfähig, neue erzwingende `Assert-MacToolchain`-Prüfung (harter Abbruch bei `store`, Warnung sonst, Opt-out `-SkipToolchainCheck`) plus Doku-Abschnitt in `iOS-Deployment.md` (Schritte 4–6) | Toolchain-Check-Negativprobe (manuell); Szenario 4 (Abbruch vor Buildnummer-Bump) | Abgedeckt |
| `docs/CI-CD.md` um iOS-Pfad/Pinning ergänzen | Schritt 6: Job `package-ios` + Action in Tabellen, Pinning-Mechanismus beschrieben | Dokumentationsschritt (kein Test erforderlich) | Abgedeckt |
| Scope-Ausschlüsse: kein App-Code, kein Windows-Build, kein Signing-/Upload-Mechanismus, keine Beta-Xcode | Plan ändert ausschließlich YAML/PS/MD-Artefakte; `Publish`/`Upload`/`Keychain` unverändert; Windows-Pfad (`maui-windows`) explizit unberührt (Seiteneffekte Z. 145); „nie Beta" als Vorgabe verankert | Szenarien 1–2 bestätigen unveränderten Upload-/Signing-Ablauf | Abgedeckt |
| Verifikation: nächster CI-Lauf (Workload-/Xcode-Version im Build-Log) + Apple-Antwortmail | Schritt 9 (Staging-RC-Verifikation CI-Pfad) und Schritt 7 (außerplanmäßiger lokaler `store`-Lauf als früherer Wirksamkeitsnachweis, Anwender-Entscheidung) | Szenarien 1+2 mit explizitem Warteschritt „Upload done, Mail ausstehend" und Wiederaufnahmeweg über Mail-Eingang | Abgedeckt |
| iOS-27-Laufzeit-Verifikation (UIScene-Lifecycle-Pflicht ab Xcode 27) | MAUI-Pin `10.0.120` (≥ SR 11 mit UIScene-Adoption), Fallback Band-Default ≥ `10.0.110`; Schritt 8 manuelle Geräte-Abnahme durch Anwender | Szenario 3 (TestFlight-Install, App-Start, Grundfunktionen); Scope-Ausschluss 1 (kein CI-Gerätetest, `bestätigt`) | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

Keine — Status ist `Plan vollständig`.

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| CI-Pfad: `staging`-Push → `package-ios` → Upload | Kein automatisierbarer Test möglich; Nachweis über realen nächsten RC-Lauf (Build-Log `xcodebuild -version`/`dotnet workload list`, Upload-Erfolg, Apple-Antwortmail) — Schritt 9, Szenario 1 | Nicht erforderlich mit Begründung: kein in-App-Nutzerfluss; reiner Infrastrukturlauf, der nur real auf macOS-Runner mit Secrets existiert (Plan Z. 203–205) |
| Lokaler Pfad: `iOS-Deployment.ps1 -Action store` End-to-End | Realer außerplanmäßiger `store`-Lauf als Wirksamkeitsnachweis (Toolchain-Check → Build → IPA-Validierung → `iTMSTransporter`-Upload → Apple-Mail) — Schritt 7, Szenario 2 | Nicht erforderlich mit Begründung: der End-to-End-Nachweis ist der reale Lauf selbst; es gibt keine Testinfrastruktur für das Deployment-Skript (Plan Z. 203–205) |
| iOS-27-Laufzeit-Verhalten (kein Start-Crash) | Manuelle Geräte-Abnahme durch Anwender (TestFlight-Install, Start, Grundfunktionen); optional `-Action simulator` mit Screenshot — Schritt 8, Szenario 3 | Nicht erforderlich mit Begründung: Scope-Ausschluss 1 (kein automatisierter CI-Gerätetest, Status `bestätigt` vom Anwender) |

Die Begründung für das Fehlen automatisierter E2E-Tests ist im Abschnitt `E2E-Tests` nachvollziehbar dokumentiert (Plan Z. 203–207): Die Anforderung berührt keinen über UI oder Nutzeraktion in der App erreichbaren Ablauf. Betroffene bestehende E2E-/UI-Tests: keine (Plan Z. 199–201 konsistent mit `inventory/tests.md` — `tests/WindowsJourneyUiTests` betreffen nur Windows).

## Fehlende oder unvollständige Planbestandteile

Keine — Status ist `Plan vollständig`.

Geprüft und als vollständig befunden:

- **Scope-Klärung** vollständig gefüllt (Verarbeitungseinheit inkl. beider Auslieferungspfade, auslösende Ereignisse, erwartete Wirkung, Umgang mit fehlenden Objekten inkl. Terminierungs-Fallback).
- **Scope-Ausschluss** mit Begründung und Status `bestätigt (Anwender, 2026-11-20)`.
- **Akzeptanzszenarien**: 7 Szenarien mit Auslöser, Ausgangszustand, Schritten, sichtbarer Wirkung und Nachweis; Happy Path (Szenario 1) vollständig vom Push bis zur Apple-Mail; fehlende-Objekte-Fälle (Szenarien 4–6) explizit benannt; Regressionsszenario 7 (erneutes ITMS-90725) mit Diagnoseweg.
- **Wartesituationen/externe Abhängigkeiten**: Apple-Antwortmail als Zustand „Upload done, Mail ausstehend" mit Wiederaufnahmeweg; Runner-Image-Drift, `xcode-27`-Label-Verfügbarkeit, TPV-Pack-Falle und lokale Toolchain-Hinterherhinkung als Risiken benannt.
- **Alle offenen Fragen** der `requirement.md` (Verfügbarkeit finaler Werkzeuge, SDK-Band, Zeitpunkt, lokaler Deploy-Pfad) sind durch Verifikation zum Planungszeitpunkt bzw. Anwender-Entscheidungen beantwortet; `Offene Punkte: Keine`.
- **Betroffene bestehende Tests**: vollständig benannt („Keine"), konsistent mit der Bestandsaufnahme (keine Tests für CI-Artefakte; `FlowNRW.Tests`/Node-Tests unberührt).
- **Testdaten/Fixtures/Hilfsmethoden**: nicht erforderlich (keine automatisierten Tests); benötigte Infrastruktur (`Invoke-MacCapture`, `Get-TransporterFindScript`-Muster) ist als Wiederverwendung benannt.

## Hinweise

- Schritt 1 („Ziel-Paarung verifizieren") ist bewusst als erster Umsetzungsschritt geführt: Die im Plan genannten Versionen (`27.2.10000` / `10.0.120` / `Xcode_27.2.app`) sind Planungsstand und sollen zum Implementierungszeitpunkt gegen die jeweils neueste stabile, paarweise kompatible Kombination geprüft werden — korrekt und notwendig, da Band-/Feed-Zuordnung driften kann.
- Der außerplanmäßige lokale `store`-Lauf (Schritt 7) geht über die Minimalanforderung hinaus (Apple verlangt Korrektur erst ab nächster Delivery/April 2027), ist aber als Anwender-Entscheidung dokumentiert und liefert den frühesten Wirksamkeitsnachweis — kein Widerspruch zum Scope-Ausschluss „keine sofortige Neu-Auslieferung *erforderlich*".
- Der Buildnummer-Bump durch den Nachweis-Lauf (Seiteneffekt) ist im Plan benannt und als gewollt begründet.
