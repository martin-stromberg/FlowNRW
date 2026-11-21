# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### .github/actions/package-ios/action.yml (Install .NET MAUI workload)

- **Fehlende Validierung von Eingaben** — Die `>= 10.0.110`-Prüfung der aufgelösten MAUI-Version (Zeilen 141–145) läuft nur, wenn `MAUI_FALLBACK=1` ist. Ein explizit übergebener Pin `maui-workload-version` unterhalb des Minimums (z. B. `10.0.100`) löst im Erstversuch sauber auf, wird nie validiert und erzeugt — da die Action stets Xcode 27 selektiert — genau die Start-Crash-App, die der Pin verhindern soll. Der Constraint steht nur in der `description`, nicht im Code. Auch der Stille-Fallback `RESOLVED_MAUI="$MAUI_PIN"` (Zeile 139) unterläuft die Prüfung.

  Empfehlung: Die awk-Versionsprüfung auf `RESOLVED_MAUI` ohne die `MAUI_FALLBACK`-Bedingung ausführen (Pin-Pfad und Fallback gleich behandeln) oder `MAUI_PIN` vor dem Schreiben des Rollback-Files gegen `10.0.110` validieren und bei Verstoß mit `::error::` abbrechen.

- **Fehlermeldung ohne aussagekräftigen Kontext** — Das Rollback-File enthält zwei Pins (`microsoft.net.sdk.ios` und `microsoft.net.sdk.maui`). Scheitert `dotnet workload update` (Zeile 112), behauptet die Warnung in Zeile 118 trotzdem „MAUI-Pin '$MAUI_PIN' nicht aufloesbar" — die Ursache kann genauso der iOS-Pin oder ein transienter NuGet-Fehler sein. Im zweiten Fall wird ein eigentlich gültiger MAUI-Pin anschließend still auf den Band-Default fallen gelassen (die Deterministik, für die der Pin existiert, geht verloren) und der Diagnostiker wird auf die falsche Spur geschickt.

  Empfehlung: Warnung neutral formulieren (z. B. „Rollback-Update fehlgeschlagen - versuche ohne MAUI-Pin erneut (iOS- oder MAUI-Pin nicht aufloesbar)") oder die Fehlerausgabe des ersten `workload update` auf den scheiternden Manifest-Namen prüfen und nur dann in den MAUI-Fallback gehen.

## Geprüfte Dateien

Liste aller geprüften Dateien:
- `.github/actions/package-ios/action.yml`
- `.github/workflows/staging-ci.yml`
- `scripts/iOS-Deployment.ps1`
- `scripts/iOS-Deployment.md`
- `docs/CI-CD.md`
