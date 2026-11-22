# Anforderung: Einheitlicher Einstieg zum Wiederherstellen persistierter Favoriten-Boards

## Quelle

Projektschritt 2 des Projekts „Gemeinsame Refresh-/Restore-Fassaden (B1–B4)" —
Projektartefakte unter `docs/projects/task/issue-24-2398a0b2f3b74948b9f7d8d3829a7d34-refresh-anlass-schritte-an-fav/`.
Fachlicher Hintergrund: Issue #21 / Befund B1 aus dem Code-Review #19 (höchste Priorität).
Abhängigkeit: Schritt 1 (SaveAsync-Entfernung) ist gemergt.

## Ziel

Ein persistiertes Favoriten-Board wird heute auf zwei divergierenden Wegen
wiederhergestellt: In der Start-Übersicht werden beim Laden alle gespeicherten
Favoriten-Karten aus dem Cache gefüllt, wobei ein veraltetes Board angezeigt und als
veraltet markiert wird; beim direkten Öffnen eines einzelnen Haltestellen-Monitors wird
dagegen bei jedem Öffnen sowohl die Favoriten- als auch die Cache-Datei vollständig neu
gelesen (ohne Abbruchmöglichkeit und ohne gemeinsame Sperre mit den Schreibvorgängen),
und ein veraltetes Board wird verworfen statt angezeigt.

Es soll genau einen Einstiegspunkt für den Geschäftsvorgang „persistiertes Board einer
gespeicherten Favoriten-Haltestelle wiederherstellen" geben, der sämtliche
Nebenbedingungen zentral bündelt.

## Fachliche Anforderungen

- Ein einziger Einstiegspunkt (Cache-Lesefassade) für beide Aufrufpfade; die
  Eigenkompositionen entfallen.
- Zentral gebündelt:
  - Prüfung der Favoritenzugehörigkeit
  - gebündeltes Lesen des Cache-Bestands statt wiederholten Lesens pro Öffnen
  - Bewertung der Datenfrische
  - einheitliche Filterung der Fahrten: zeitlose Fahrten werden verworfen, nur künftige
    Fahrten werden gezeigt
  - einheitliche Stale-Semantik: veraltete Boards werden **angezeigt und als veraltet
    markiert**, nicht verworfen
- Die mehrfach duplizierte Ermittlung der wirksamen Abfahrtszeit wird zentralisiert.
- Ein Board ohne gültige künftige Fahrten liefert weiterhin sein Linieninventar, damit
  die Kartenanzeige ihre Linien behält.
- Die Bereinigung verwaister Cache-Einträge bleibt Verantwortung des initialen
  Ladevorgangs der Favoriten-Übersicht.
- Der Session-/Live-Pfad des Monitors (`retained`-Übergabe von Karte an Monitor)
  bleibt fachlich unverändert.
- Bestehende Tests, die die alte divergente Monitor-Semantik pinnen (veraltetes Board
  wird nicht angezeigt), werden auf die einheitliche Semantik „anzeigen und markieren"
  angepasst; die Testabdeckung zeigt nachweislich, dass beide Aufrufpfade denselben
  Einstieg und dieselbe Semantik verwenden.

## Randbedingungen / Vorgaben

- Als Orientierung empfiehlt die Bestandsaufnahme einen dedizierten,
  plattformunabhängigen Lese-Dienst in der Kernlogik (`FlowNRW.Core`), den beide bisherigen
  Aufrufpfade gemeinsam nutzen — statt die Fassade an das Favoriten-Home-ViewModel zu
  hängen (dessen Konstruktion der Monitor bewusst nicht kennt). Die konkrete technische
  Wahl trifft der Schritt, die Empfehlung ist begründet in der Projekt-Bestandsaufnahme.
- Konventionen: `FlowNRW.Core`, XML-Doku auf öffentlichen Membern, handgeschriebene
  Test-Fakes, `TreatWarningsAsErrors`, `dotnet format`.
- Bestehende Tests laufen; neue Logik ist getestet; ≥ 70 % Line-Coverage bleibt
  eingehalten.

## Nicht-Ziele

- Kein Umbau des produktiven Refresh-/Persist-Schreibpfads (`UpsertAsync` über
  `PersistAsync`).
- Keine Änderung des Session-Board-LRU bzw. des `OpenFavoriteAsync`-Fast-Path.
- Keine Verhaltensänderung der Live-Aktualisierung (`RefreshCoreAsync`-Filterung bleibt
  eigener Vorgang, nicht Teil der Harmonisierung).
- Keine UI-Änderungen.
