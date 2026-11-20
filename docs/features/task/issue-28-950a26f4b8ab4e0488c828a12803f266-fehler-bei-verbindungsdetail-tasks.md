# Tasks: Fehler bei Verbindungsdetail (Issue #28)

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Tests | `RefreshIfStale_JourneyWithWalkingLeg_KeepsSelection` in `JourneyResumeTests_Selection` | Offen | — |
| 2 | Tests | `RefreshIfStale_WalkingLegTurnedTransit_ClearsSelection` in `JourneyResumeTests_Selection` | Offen | — |
| 3 | Tests | `Details`-Fallback-Test (kein `Session.Status`-Echo) | Offen | — |
| 4 | Logik | `SameJourney`/`SameWalkingEvent` als `internal static`/`private static` nach `RealtimeConsolidator` verlegen | Offen | — |
| 5 | Logik | `ProviderOrchestrator`: private Matcher entfernen, `RealtimeConsolidator.SameJourney` nutzen | Offen | — |
| 6 | Logik | `JourneySearchViewModel.SameJourney` auf `RealtimeConsolidator.SameJourney` umstellen, `SameEvent` entfernen | Offen | — |
| 7 | Logik | `JourneyDetailViewModel.Details`: Fallback „Keine Verbindung ausgewählt." | Offen | — |
| 8 | Verifikation | `dotnet test FlowNRW.Tests -c Release` + `dotnet format`-Prüfung | Offen | — |
