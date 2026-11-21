# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### docs/CI-CD.md (Abschnitt "iOS-Paketierung", Zeilen 86–92)

- **Inkonsistenz Doku vs. Verhalten** — Die Doku beschreibt `dotnet workload list` als reinen "Log-Nachweis". Tatsächlich wird die Ausgabe in `action.yml` funktional ausgewertet: `RESOLVED_MAUI` wird per awk aus der Manifest-Spalte ermittelt (Zeilen 141–143), erzwingt per `::error::` + `exit 1` das Minimum `>= 10.0.110` (Zeilen 152–156) und steuert, welche `Microsoft.Maui.*`-Packs die Bereinigung überleben (Zeilen 165–168). Zudem fehlt die dreistufige Retry-/Fallback-Logik (kombiniertes Rollback-Update → Retry → iOS-Pin allein, MAUI fällt auf den Band-Default, Zeilen 113–132) komplett — ein Maintainer, der die `::warning::MAUI-Pin ... nicht aufloesbar`-Meldung im CI-Log sieht, findet in der Doku keinen Hinweis auf diesen Pfad.

  Empfehlung: Den Absatz um die tatsächlichen Mechanismen ergänzen — `dotnet workload list` als Quelle der aufgelösten MAUI-Version samt `>= 10.0.110`-Verifikation (gilt für Pin und Band-Default) sowie den Fallback "MAUI-Pin nicht auflösbar → Band-Default" beschreiben, statt `workload list` nur als Log-Nachweis zu bezeichnen.

## Geprüfte Dateien

Liste aller geprüften Dateien:
- `.github/actions/package-ios/action.yml`
- `.github/workflows/staging-ci.yml`
- `scripts/iOS-Deployment.ps1`
- `scripts/iOS-Deployment.md`
- `docs/CI-CD.md`
