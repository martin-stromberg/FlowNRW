# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### RefreshLifecycle.cs (RefreshLifecycle)

- **Kopplung / Ablaufverantwortung** — `visible` ist ein `HashSet<RefreshArea>` ohne Registrierungs-Eigentümer (Z. 17, 53–72). `RefreshArea.Journeys` wird von zwei gestapelten Shell-Pages (`ResultsPage`, `JourneyDetailPage`) gemeinsam genutzt — das ist so intendiert („sharing one session"), aber die Korrektheit hängt jetzt an der Reihenfolge von `OnAppearing`/`OnDisappearing` der beiden Pages: Feuert Shell das `OnAppearing` der eingehenden Page vor dem `OnDisappearing` der ausgehenden (für Shell-Rücknavigation dokumentiert, z. B. dotnet/maui#21274 — `Appearing` wird vor Abschluss der Plattform-Navigation ausgelöst), meldet `JourneyDetailPage.OnDisappearing` → `NotifyHidden(Journeys)` den Bereich ab, den `ResultsPage.OnAppearing` gerade registriert hat — inklusive `CancelArea` (Token-Cancel + `journeys.CancelPending()`). Die sichtbar bleibende Page verliert dadurch dauerhaft die Bereichs-Registrierung: kein `ResumeAsync` mehr bei `SetActive`, `RefreshFailed` wird durch den `visible.Contains`-Guard unterdrückt, `IsResuming` bleibt `false` — bis die Page erneut erscheint. Im alten Code überlebte der seitenlokale Resume-Zustand den Disappearing der anderen Page; nur das laufende Provider-Request der gemeinsamen Session wurde einmalig abgebrochen. Der Fehler ist also neu und persistiert (nicht nur transienter Abbruch).

  Empfehlung: Registrierung eigentümerbasiert machen — z. B. `NotifyVisibleAsync` einen Besitzer/ein Handle je Page entgegennehmen und `NotifyHidden` nur abmelden, wenn es die letzte eigene Registrierung war — oder einen Referenzzähler pro Bereich führen (`NotifyVisibleAsync` +1, `NotifyHidden` −1, `CancelArea` erst bei 0). Das deckt beide Event-Reihenfolgen ab, ohne die beabsichtigte gemeinsame Session zu zerlegen.

- **Fehlerbehandlung** — `ForegroundChanged` (Z. 160–170) prüft `args.PropertyName` nicht und reagiert auf jede `PropertyChanged`-Meldung von `ForegroundState`. Aktuell meldet `ForegroundState` ausschließlich `IsActive`, daher funktional korrekt — aber jede künftig hinzugefügte Eigenschaft löst unbemerkt Bereichs-Cancel/Resume aus. Die Seiten-Vorgänger ohne Prüfung riefen nur `ReconcileRefreshLoop`; die zentrale Variante fährt jetzt echte Abbruch- und Provider-Arbeit.

  Empfehlung: `if (args.PropertyName != nameof(ForegroundState.IsActive)) return;` als erste Zeile ergänzen.

- **Fehlerbehandlung** — Exceptions aus Event-Handlern landen auf verworfenen Tasks: `ResumeCompleted?.Invoke` im `finally` von `ResumeAsync` (Z. 137) und `RefreshFailed?.Invoke` im `catch` von `RunOccasionAsync` (Z. 146) propagieren bei einem werfenden Handler in die per `_ =` verworfenen Tasks (`ForegroundChanged` Z. 164, `FavoritesChanged` Z. 178, `NotifyVisibleAsync`-Aufrufer). Der Task faultet dann unbeobachtet (`TaskScheduler.UnobservedTaskException`), statt die App zu informieren.

  Empfehlung: Entweder Handler-Aufrufe defensiv einzeln in try/catch kapseln oder den Task-Fault bewusst behandeln (z. B. `.ContinueWith`/`SafeFireAndForget`-Helfer mit Logging), damit ein fehlerhafter Page-Handler nicht still untergeht.

### FavoriteHomeViewModel.cs (FavoriteHomeViewModel)

- **Namenskonventionen und Einheitlichkeit / Doku** — `RefreshForOccasionAsync(RefreshOccasion, RefreshFreshness, …)` (Z. 106–116) ignoriert den übergebenen `freshness`-Parameter für zwei von drei Anlässen: `RefreshAtStartupAsync` verwendet gar keine Policy, `RefreshMissingAsync` (Z. 120) nutzt das Instanzfeld `this.freshness` statt des Arguments. Produktiv sind beide dieselbe DI-Singleton-Instanz — funktional gleich —, aber die Signatur/Doku (`<param name="freshness">Shared realtime freshness policy.</param>`) suggeriert eine Wirkung für alle Anlässe, und ein abweichend übergebener Wert würde für `CardsAdded` still ignoriert.

  Empfehlung: `freshness` als Parameter an `RefreshMissingAsync`/`RefreshAtStartupAsync` durchreichen (oder Doku präzisieren: `Startup`/`CardsAdded` nutzen die Instanz-Policy).

## Geprüfte Schwerpunkte ohne Befund

- `ReferenceEquals`-Guard + `finally`-Semantik der Resume-Tokens (Z. 121–139): korrekt. `CancelArea` entfernt den Eintrag atomar vor `Cancel()`; der `finally` sieht dann keinen eigenen Eintrag und feuert `ResumeCompleted` nicht. Dispose-vs-Cancel-Race (`using var source` vs. `source.Cancel()`) ist auf dem UI-Thread durch den MAUI-`SynchronizationContext` serialisiert; in Tests laufen TCS-Continuations synchron — reale Interleavings gibt es nicht.
- `SetActive(false)` bricht jetzt über `ForegroundChanged` → `CancelArea` für alle sichtbaren Bereiche Resume-Token + `CancelPending` ab (bisher nur `favorites.CancelPending()`) — Vorgabe erfüllt; die Lifecycle-Subscription sitzt im Ctor und läuft vor den Page-Handlern, daher greift der Cancel vor deren Loop-Reconcile.
- `favorites.PropertyChanged(nameof(Cards))`-Guard (`visible && IsActive && !IsResuming`, Z. 172–181) bildet den bisherigen `active && foreground.IsActive && resume is null`-Guard exakt ab.
- Event-Abmeldung der transienten Pages vollständig: `DeparturePage` 6/6, `ResultsPage` 2/2, `JourneyDetailPage` 2/2 Subscriptions in `OnDisappearing` symmetrisch entfernt — kein Leak gegen den Singleton-Lifecycle.
- `HomePage`: `NotifyVisibleAsync(Favorites)` wird erst nach `model.LoadAsync()`/`RenderCards()` gerufen (Z. 113–116); der `version`/`appearance`-Guard ist erhalten; der entfallene `foreground.IsActive`-Check sitzt jetzt im `NotifyVisibleAsync`-Switch — gleichwertig. `RefreshFailed→Title`-Mapping je Occasion korrekt.
- `RunBackgroundAsync` ruft nur die Fassade mit `ForegroundReturn` (Z. 105) — verhaltensgleich; 12 bestehende Background-Tests unverändert grün.
- Tests: deterministisch (fester `TransitTestClock`, kontrollierte Pending-TCS, 2-s-Echtzeit-Timeouts nur als Obergrenze); 25/25 `RefreshLifecycleTests*` grün, `dotnet build` des MAUI-Projekts ohne Warnungen/Fehler.
- Kein toter Code: entfernte Felder (`freshness`, `resume`, `foreground`, `active`) haben keine Restreferenzen; usings der Pages konsistent.

## Geprüfte Dateien

- `FlowNRW.Core/Refresh/RefreshLifecycle.cs`
- `FlowNRW.Core/Refresh/RefreshArea.cs` (neu)
- `FlowNRW.Core/Refresh/RefreshAreaEventArgs.cs` (neu)
- `FlowNRW.Core/Refresh/RefreshFailedEventArgs.cs` (neu)
- `FlowNRW.Core/Refresh/RefreshOccasion.cs` (neu)
- `FlowNRW.Core/Favorites/FavoriteHomeViewModel.cs`
- `FlowNRW/HomePage.cs`
- `FlowNRW/DeparturePage.cs`
- `FlowNRW/ResultsPage.cs`
- `FlowNRW/JourneyDetailPage.cs`
- `FlowNRW.Tests/RefreshLifecycleTests_Background.cs`
- `FlowNRW.Tests/RefreshLifecycleTests_Cancellation.cs` (neu)
- `FlowNRW.Tests/RefreshLifecycleTests_Foreground.cs` (neu)
- `FlowNRW.Tests/RefreshLifecycleTests_Occasions.cs` (neu)
- `FlowNRW.Tests/RefreshLifecycleTests_Overlap.cs` (neu)
- `FlowNRW.Tests/FavoriteTests.cs`
- `FlowNRW.Tests/FavoriteDepartureCacheIntegrationTests.cs`

Nicht code-reviewt (Dokumentation): `docs/features/task/…-schritt-3-resume-anlass-zentral-tasks.md` sowie die Plan-Dokumente im Feature-Ordner.
