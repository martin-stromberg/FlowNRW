# Anforderung: Ungenutzten Schreibvorgang aus der Cache-Ablage entfernen

## Quelle

Projektschritt 1 des Projekts „Gemeinsame Refresh-/Restore-Fassaden (B1–B4)" —
Projektartefakte unter `docs/projects/task/issue-24-2398a0b2f3b74948b9f7d8d3829a7d34-refresh-anlass-schritte-an-fav/`.
Fachlicher Hintergrund: Issue #23 / Befund B3 aus dem Code-Review #19.

## Ziel

Die Ablage für zwischengespeicherte Abfahrts-Boards bietet aktuell einen Schreibvorgang
an, der den kompletten Cache-Bestand in einem Schritt ersetzt. Dieser Vorgang wird
produktiv nie aufgerufen — produktiv werden Cache-Einträge ausschließlich einzeln
eingefügt bzw. aktualisiert sowie gezielt oder als verwaiste Reste entfernt. Der
ungenutzte Voll-Ersetzungs-Schreibvorgang soll aus der öffentlichen Schnittstelle der
Cache-Ablage entfernt oder so gekapselt werden, dass er von außen nicht mehr erreichbar
ist, sodass die Schnittstelle nur noch die tatsächlich produktiv genutzten Zugriffswege
anbietet.

## Fachliche Anforderungen

- Der Voll-Ersetzungs-Schreibvorgang ist am öffentlichen Vertrag der Cache-Ablage nicht
  mehr erreichbar (entfernt oder verintern).
- Bestehende Tests, die den Voll-Ersetzungs-Pfad direkt prüfen oder Prüfdoubles mit
  diesem Vorgang bereitstellen, werden angepasst.
- Die bisher über den ungenutzten Pfad charakterisierten Eigenschaften (Rundreise,
  Integrität, Validierung) sind, soweit fachlich relevant, über den produktiven
  Einzel-Einfüge-/Aktualisierungspfad abzudecken.
- Das Ändern einzelner Einträge, das Entfernen einzelner Einträge und die Bereinigung
  verwaister Einträge bleiben unverändert verfügbar und getestet.
- Alle Tests bleiben grün; Projekt-Checks (`dotnet format`, Build mit
  TreatWarningsAsErrors) bestehen.

## Nicht-Ziele

- Kein Umbau des produktiven Schreibpfads selbst.
- Keine Änderung am Dateiformat oder an den Persistenz-Limits.
