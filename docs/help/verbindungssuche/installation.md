← [Zur Übersicht](index.md)

# Installation und native Plattformprüfung

## Windows und Visual Studio

Benötigt werden .NET 10 SDK, die MAUI-Windows-Workload, Windows 10 ab Build 17763 oder Windows 11 und für Visual Studio die MAUI-Entwicklungsworkload. `FlowNRW.sln` öffnen, `FlowNRW` als Startprojekt und „Windows Machine“ als Ziel wählen; mit F5 starten.

Aus der Repositorywurzel:

```powershell
dotnet workload install maui-windows
dotnet restore FlowNRW.sln
dotnet build FlowNRW.sln -c Release -p:TreatWarningsAsErrors=true
dotnet build FlowNRW/FlowNRW.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Ein regulärer Debug-/Releasebuild verwendet echte Fahrplandienste. Providerzugriff benötigt Internet. Die [Providerkonfiguration](../fahrplanauskunft/installation.md) erläutert tatsächliche Konfigurationsquellen, HTTPS-Grenzen und Defaults; eine automatisch geladene lokale Einstellungsdatei ist nicht eingerichtet. [Datenschutz und Cache](../fahrplanauskunft/architektur.md#zuverlässigkeit-und-datenschutz) gelten weiterhin. Die Suche fordert keine Standortberechtigung an.

## iOS-Basis

Das Projekt wählt auf macOS automatisch `net10.0-ios`; unter Windows aktiviert `EnableIos=true` dieses Ziel anstelle des Windows-Ziels. Die Mindestversion ist iOS 15.0. AppDelegate, Program und Info.plist sind vorhanden. Native iOS-Builds, Simulator-/Gerätestarts und Signierung wurden hier nicht ausgeführt; die Abnahme übernimmt der Nutzer.

Für native iOS-Entwicklung sind ein Mac mit zur installierten .NET-iOS-Workload passendem Xcode, Simulator bzw. Gerät und bei Geräteausführung passende Apple-Signierung erforderlich. Auf Windows Visual Studio mit dem Mac verbinden; `EnableIos=true` beim Projektladen übergeben, etwa aus einer PowerShell, aus der Visual Studio anschließend gestartet wird:

```powershell
$env:EnableIos = 'true'
```

Die bereits geöffnete Solution neu laden bzw. Visual Studio aus diesem Kontext starten, `FlowNRW` als Startprojekt und den verfügbaren iOS-Simulator oder das konfigurierte Gerät wählen. Die Variable für spätere Windows-Sitzungen wieder entfernen: `Remove-Item Env:EnableIos`. Die Eigenschaft aktiviert nur das Ziel; sie ersetzt weder Mac-Verbindung noch Xcode oder Signierung.

Alternativ auf dem Mac aus der Repositorywurzel (Beispiel Apple-Silicon-Simulator):

```bash
dotnet workload install maui-ios
dotnet build FlowNRW/FlowNRW.csproj -c Debug -f net10.0-ios -p:EnableIos=true -p:RuntimeIdentifier=iossimulator-arm64
```

Den installierten Simulator über die lokale Entwicklungsumgebung auswählen und starten. Intel-Macs benötigen den passenden Simulator-Runtime-Identifier. Für Geräte das Ziel `ios-arm64` und lokale Signierungsdaten verwenden. Diese Befehle sind Einrichtungshinweise, kein bestandener iOS-Nachweis.

Für den wiederholbaren Geräte-Deploy steht zusätzlich [`scripts/iOS-Deployment.ps1`](../../../scripts/iOS-Deployment.ps1) mit der zugehörigen [Anleitung](../../../scripts/iOS-Deployment.md) bereit. Das Skript baut auf dem Mac und kann die signierte App mit `xcrun devicectl` auf ein angegebenes Gerät installieren und starten. Von Windows aus kann derselbe Ablauf über SSH auf einem vorbereiteten Mac angestoßen werden.

## iOS-Prüfliste für den Nutzer

Datum, Commit, SDK-/Xcode-Version, iOS-Version und Gerät/Simulator sowie Ergebnis und Auffälligkeiten protokollieren:

- [ ] App bauen und starten; Einstieg „Verbindung suchen“, kein Counter, kein Standortdialog.
- [ ] Adresse und Haltestelle jeweils als Start und Ziel suchen; bei mehreren Treffern bewusst auswählen.
- [ ] Koordinaten in beiden Endpunkten übernehmen; leere, ungültige und grenzwertige Zahlen prüfen und korrigieren.
- [ ] Ohne zwei ausgewählte Endpunkte bleibt „Verbindungen suchen“ gesperrt; geänderte Eingaben heben die Auswahl wieder auf.
- [ ] NRW-Verbindung und bundesweite Verbindung außerhalb NRW suchen; Quelle, Datenalter, Warnungen und gegebenenfalls Ersatzquelle festhalten.
- [ ] Ergebnisse öffnen; Teilstrecken, Betreiber, Fußwege, Umstiege, Soll-/Istzeiten und unbekannte Angaben prüfen, soweit geliefert.
- [ ] Detail → Ergebnisse → Suche zurück; Auswahl erhalten, nach Eingabeänderung alte Ergebnisse verworfen.
- [ ] Während einer laufenden Suche Eingabe ändern; kein späteres Überschreiben oder unerwarteter Seitenwechsel.
- [ ] Ohne Netz Fehlerzustand und nach Wiederherstellung erneute Suche prüfen; leere Treffer mit ungeeignetem Suchtext prüfen.
- [ ] Touch-Ziele, Tastatur/Dezimalzeichen, Scrollen, schmale Ansicht, große Systemschrift und VoiceOver auf allen drei Seiten prüfen.

Nicht durch Live-Daten erzwingbare Ausfall-/Tageswechsel-/Cachefälle als nicht geprüft festhalten; die Windows-Fixture-Prüfung belegt keine iOS-Darstellung.

## Lieferstand und Nachweise

[118 Tests, 97,01 % Core-Coverage und native Windows-Prüfung](verification/checks-2026-09-16.md) sind dokumentiert. iOS-Ausführung bleibt offen. Die lokale IIS-Präsentation samt gesonderter Zwischenpaket-Bereitstellung wurde am 16.09.2026 auf Nutzerwunsch aus dem Umfang genommen. Bestehende Windows-GitHub-Actions-Tests und Releases bleiben unverändert; es gibt keine neue iOS-CI. Das manuelle Deployment-Skript ist eine lokale Entwicklerhilfe und keine GitHub-Actions-Pipeline.
