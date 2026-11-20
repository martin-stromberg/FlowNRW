# Tasks: Fehler bei Verbindungsdetail (Issue #28)

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Tests | `RefreshIfStale_JourneyWithWalkingLeg_KeepsSelection` in `JourneyResumeTests_Selection` | Erledigt | selbst |
| 2 | Tests | `RefreshIfStale_WalkingLegTurnedTransit_ClearsSelection` in `JourneyResumeTests_Selection` | Erledigt | selbst |
| 3 | Tests | `Details`-Fallback-Test (kein `Session.Status`-Echo) | Erledigt | `DetailsWithoutSelectionShowsPlaceholderNotStatus` |
| 4 | Logik | `SameJourney`/`SameWalkingEvent` als `internal static`/`private static` nach `RealtimeConsolidator` verlegen | Erledigt | `RefreshIfStale_JourneyWithWalkingLeg_KeepsSelection`, `ProviderOrchestratorTests_Union` |
| 5 | Logik | `ProviderOrchestrator`: private Matcher entfernen, `RealtimeConsolidator.SameJourney` nutzen | Erledigt | `ProviderOrchestratorTests_Union` |
| 6 | Logik | `JourneySearchViewModel.SameJourney` auf `RealtimeConsolidator.SameJourney` umstellen, `SameEvent` entfernen | Erledigt | `RefreshIfStale_*` (alle) |
| 7 | Logik | `JourneyDetailViewModel.Details`: Fallback „Keine Verbindung ausgewählt." | Erledigt | `DetailsWithoutSelectionShowsPlaceholderNotStatus` |
| 8 | Verifikation | `dotnet test FlowNRW.Tests -c Release` + `dotnet format`-Prüfung | Erledigt | 303/303, format clean, sln-Build warnfrei |
