# Begutachtungsstände über IIS

Vom Nutzer am 15. September 2026 beauftragter Bereitstellungsort: vorbereitete IIS-Website „ÖPNV“ auf diesem Windows-Rechner. Keine neue CI-/Deploymentpipeline und keine Browserportierung der MAUI-App.

## Nachgewiesener lokaler Zugang

- Dokumentwurzel: `D:\Dashboard\ÖPNV` (zunächst leer).
- Lokal geprüfte URL: `http://localhost/%C3%96PNV/`.
- `index.html` am 15.09.2026 installiert, SHA256 von Quelle und Ziel identisch. HTTP 200, Microsoft-IIS/10.0, erwarteter Seitentitel und Überschrift geprüft.
- Appcmd kann die globalen IIS-Bindings wegen Windows-Dateirechten nicht lesen. Dafür wurden keine ACLs oder IIS-Einstellungen verändert.
- Der In-App-Browser meldet „Browser is not available: iab“. Daher derzeit HTTP-/Inhaltsprüfung, keine behauptete visuelle Browserabnahme.
- Eine externe Stakeholder-URL oder Erreichbarkeit aus anderen Rechnern ist damit nicht bestätigt. Nutzer nach der vorgesehenen URL gefragt; lokale Arbeit unabhängig fortgeführt.

## Veröffentlichung eines UI-Zwischenstands

Nach fachlicher Abnahme eines kleinen UI-Schritts einen Windows-Releasebuild als entpackbares ZIP bereitstellen. Download enthält Versions-/Commitkennung, kurze Start- und Prüfanleitung und bekannte Einschränkungen. iOS-Prüfung verbleibt beim Nutzer. Erst tatsächlich abgenommene UI-Builds anbieten; der bisherige Counter-Vorlagenstand ist keine ÖPNV-Vorschau.

Die beauftragte Installation darf den bestehenden Inhalt des Zielordners nicht pauschal löschen. Nur eigene Dateien ersetzen, Downloads mit eindeutiger Versionskennung ablegen. Vor jedem Schreiben den absoluten Zielpfad innerhalb `D:\Dashboard\ÖPNV` prüfen. Webquelle ist `docs/review/index.html`; Quellcode, Secrets und rohe Nutzer-/Standortdaten werden nicht bereitgestellt. Nach Kopieren HTTP-Abruf und Dateiprüfsumme prüfen. ZIP-Download zusätzlich auf korrekten Content-Type, Dateigröße und Prüfsumme kontrollieren. Noch keine Download-Datei installiert.
