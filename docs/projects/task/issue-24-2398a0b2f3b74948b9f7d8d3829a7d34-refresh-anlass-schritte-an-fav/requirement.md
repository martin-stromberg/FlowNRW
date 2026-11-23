# Anforderung: Gemeinsame Refresh-/Restore-Fassaden (Befunde B1–B4 aus Review #19)

## Quelle / Auftrag

Issue #24 (Kommentar): „Erledige diese Aufgabe zusammen mit Issue #21, #22, #23. Nutze den
project-skill dafür. Dokumentiere den Status jeder Aufgabe in ihrem Issue. Erstelle am Ende
einen PR, der alle vier Issues schließt."

Alle vier Befunde stammen aus dem außerordentlichen Code-Review #19 und betreffen
inoffizielle Ausführungspfade / duplizierte Kompositionsregeln im Refresh- und
Favoriten-Restore-Bereich.

## Ziel

Jeder der vier Geschäftsvorgänge erhält genau einen Einstiegspunkt, an dem die fachlichen
Nebenbedingungen zentral abgesichert sind; die divergierenden Aufrufer-Kompositionen
entfallen. Ein PR schließt Issues #21, #22, #23, #24.

## Fachliche Anforderungen

### A (Issue #21, Befund B1 — höchste Priorität): Einheitlicher Einstieg für „persistiertes Favoriten-Board wiederherstellen"

Befund: Der Vorgang (Favoritenzugehörigkeit prüfen → Cache lesen → Freshness bewerten →
abgelaufene Items filtern → anzeigen) wird an zwei Stellen mit widersprüchlichen
Nebenbedingungen komponiert:

- `FavoriteMonitorViewModel.Restore` (Home-Pfad): Stale Board wird angezeigt und markiert
  (`IsStale`, Status „Letzter Stand…"); Favoriten-/Cache-Datei einmal beim Load; zeitlose
  Items verworfen (`EffectiveTime is { } time && time >= now`).
- `StopMonitorViewModel` (Monitor-Pfad): Stale Board wird verworfen; Favoriten-/Cache-Datei
  bei jedem `OpenStopAsync` komplett neu gelesen (ohne CancellationToken); zeitlose Items
  in `RefreshCoreAsync` behalten — zwei Filtersemantiken in derselben Klasse.

Anforderung: Ein einziger Einstiegspunkt `RestoreBoardAsync(Stop, RefreshFreshness)` als
einzige Cache-Lesefassade (z. B. an `FavoriteHomeViewModel` oder dediziertem
`DepartureBoardCache`-Service):

- eine Stale-Semantik: Markieren-und-Anzeigen (Home-Verhalten, fachlich besser)
- eine einheitliche Item-Filterung (zeitlose Items konsistent behandeln)
- gebündelter Dateizugriff statt wiederholtem Lesen pro `OpenStopAsync`
- `StopMonitorViewModel` nutzt denselben Einstieg statt eigener Komposition

### B (Issue #22, Befund B2): Resume-Gate aus den Pages in den RefreshLifecycle zentralisieren

Befund: Der Vorgang „bei Vordergrund-Rückkehr veraltete Daten erneuern, sofern automatische
Aktualisierung geladen und aktiv" wird von vier Aufrufern selbst zusammengesetzt. Die Regel
`IsLoaded && IntervalSeconds != 0` existiert an fünf Stellen (vier Pages +
`RefreshLifecycle`). Die Kompositionen divergieren: nur `HomePage`/`DeparturePage` haben
den `resume is not null`-Overlap-Guard und `ReconcileRefreshLoops`;
`JourneyDetailPage`/`ResultsPage` rufen `Session.RefreshIfStaleAsync` ohne
Überlappungsschutz.

Anforderung: `ResumeForegroundAsync` am `RefreshLifecycle` (oder dedizierter
`ForegroundRefreshService`): kapselt das `IsLoaded && IntervalSeconds != 0`-Gate, den
Overlap-Guard und die Delegation an registrierte Refreshables. Pages melden nur noch
„sichtbar/verdeckt" statt die Sequenz selbst zu komponieren.

### C (Issue #23, Befund B3): `IDepartureCacheStore.SaveAsync` ohne produktiven Aufrufer entfernen

Befund: `SaveAsync` (Voll-Ersetzungsschritt) ist am Interface exportiert und getestet,
wird aber produktiv nie aufgerufen. Der Test charakterisiert einen Pfad, den die
Produktion nicht fährt.

Anforderung: `SaveAsync` aus `IDepartureCacheStore` entfernen (oder verintern); den
zugehörigen Test entsprechend anpassen bzw. über den produktiven Schreibpfad abdecken.

### D (Issue #24, Befund B4 — Grenzfall): Refresh-Anlass-Schritte an FavoriteHomeViewModel hinter gemeinsamer Fassade bündeln

Befund: `FavoriteHomeViewModel` exportiert drei Refresh-Anlass-Schritte
(`RefreshMissingAsync`, `RefreshAtStartupAsync`, `RefreshStaleAsync`). Die Zuordnung
Anlass → Schritt ist Aufrufer-Wissen und existiert nur in `HomePage` (OnAppearing →
AtStartup, Cards-Changed → Missing, Foreground-Resume → Stale).

Anforderung: Wird mit der Lösung zu B2 mitaufgeräumt — der Pass-Einstieg
(`ResumeForegroundAsync`) übernimmt die Anlass-Regeln zentral, sodass die Trigger-Triplik
entfällt; alternativ die drei Schritte hinter einer einzigen anlassbezogenen Fassade
bündeln. Kein Aufrufer muss die Anlass-Regeln mehr kennen.

## Betroffene Bereiche

- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW.Core/Favorites/FavoriteMonitorViewModel.cs`
- `FlowNRW.Core/Favorites/IDepartureCacheStore.cs` (+ Implementierung)
- `FlowNRW.Core/Presentation/StopMonitorViewModel.cs`
- `FlowNRW.Core/Refresh/RefreshLifecycle.cs`
- `FlowNRW/HomePage.cs`, `FlowNRW/DeparturePage.cs`, `FlowNRW/JourneyDetailPage.cs`,
  `FlowNRW/ResultsPage.cs`
- `FlowNRW.Tests` (u. a. `DepartureCacheStoreTests.cs`)

## Akzeptanzkriterien

- Pro Geschäftsvorgang genau ein zentraler Einstiegspunkt; die genannten
  Duplikat-/Divergenz-Kompositionen sind entfernt.
- Eine einheitliche Stale-Semantik (Markieren-und-Anzeigen) und einheitliche
  Item-Filterung für den Restore-Pfad.
- Pages komponieren keine Refresh-Sequenz mehr selbst; kein divergierender
  Overlap-Guard.
- `IDepartureCacheStore.SaveAsync` ist nicht mehr öffentlich erreichbar.
- Alle bestehenden Tests laufen; Testabdeckung für geändertes Verhalten ist vorhanden;
  Projekt-Checks (`dotnet format`, Build mit TreatWarningsAsErrors, `dotnet test`)
  bestehen.
- Ein PR gegen `staging` schließt #21, #22, #23, #24 (Closing-Tags).
- Issue-Status wird in jedem der vier Issues dokumentiert.

## Explizite Nicht-Ziele

- Keine Änderung des produktiven Refresh-Verhaltens über die Harmonisierung hinaus
  (kein neues Feature, kein UI-Umbau).
- Keine Änderungen an CI/Release-Workflows.
- Keine Abschaffung der fachlichen Refresh-Schritte selbst — nur ihre Komposition
  wird zentralisiert.

## Annahmen (nicht belegt, zu prüfen)

- Die Stale-Semantik „Markieren-und-Anzeigen" des Home-Pfads ist die gewünschte
  Zielsemantik für beide Pfade (steht so im Issue-Vorschlag: „fachlich besser").
- Zeitlose Items sollen einheitlich wie im Home-Pfad verworfen werden — ggf. ist die
  Monitor-Semantik (behalten) die richtige; bei Unklarheit anhand der
  Filter-Begründung entscheiden, nicht per Rückfrage blockieren.

## Offene Fragen

- Fassaden-Ort für `RestoreBoardAsync`: `FavoriteHomeViewModel` vs. dedizierter
  `DepartureBoardCache`-Service — aus Bestandsaufnahme ableiten.
