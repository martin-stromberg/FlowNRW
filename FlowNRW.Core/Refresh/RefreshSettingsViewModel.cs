using FlowNRW.Core.Presentation;

namespace FlowNRW.Core.Refresh;

/// <summary>Draft selection and separately committed foreground refresh settings.</summary>
public sealed class RefreshSettingsViewModel : ObservableObject
{
    private readonly IRefreshSettingsStore store;
    private readonly SemaphoreSlim gate = new(1, 1);
    private int selectedSeconds = 60;

    /// <summary>Creates settings with a safe 60-second default, without starting a timer.</summary>
    /// <param name="store">Settings persistence.</param>
    public RefreshSettingsViewModel(IRefreshSettingsStore store) { this.store = store; }

    /// <summary>Reports successful saves of the effective interval.</summary>
    public event EventHandler? Changed;
    /// <summary>Saved or default interval; zero disables automatic refresh.</summary>
    public int IntervalSeconds { get; private set; } = 60;
    /// <summary>Draft selection that does not affect running monitors until saved.</summary>
    public int SelectedSeconds
    {
        get => selectedSeconds;
        set { if (selectedSeconds == value) return; selectedSeconds = value; Notify(); }
    }
    /// <summary>Whether the saved interval or safe fallback has been resolved.</summary>
    public bool IsLoaded { get; private set; }
    /// <summary>Whether a write is in progress.</summary>
    public bool IsSaving { get; private set; }
    /// <summary>Load, validation or save outcome.</summary>
    public string Status { get; private set; } = "Einstellungen werden geladen …";
    /// <summary>Description of the effective interval, not the unsaved draft.</summary>
    public string Description => IntervalSeconds == 0 ? "Automatische Aktualisierung: Aus."
        : $"Automatische Aktualisierung: alle {IntervalSeconds} Sekunden im aktiven Fenster.";

    /// <summary>Accepts only the documented finite interval choices.</summary>
    /// <param name="seconds">Candidate interval.</param>
    /// <returns>Whether the selection is supported.</returns>
    public static bool IsSupported(int seconds) => seconds is 0 or 30 or 60 or 120 or 300;

    /// <summary>Resolves persisted settings once before foreground timers may start.</summary>
    /// <returns>Load completion, including a safe fallback on error.</returns>
    public async Task LoadAsync()
    {
        await gate.WaitAsync();
        try
        {
            if (IsLoaded) return;
            var stored = await store.LoadAsync();
            if (stored is { } value && !IsSupported(value)) throw new InvalidDataException("Unsupported refresh interval.");
            IntervalSeconds = stored ?? 60;
            SelectedSeconds = IntervalSeconds;
            Status = stored is null ? "Standard: 60 Sekunden. Auswahl ändern und speichern." : "Gespeicherte Einstellung geladen.";
        }
        catch (Exception)
        {
            IntervalSeconds = 60; SelectedSeconds = 60;
            Status = "Einstellung konnte nicht geladen werden. Sicherer Standard: 60 Sekunden. Zum Ersetzen bitte ausdrücklich speichern.";
        }
        finally { IsLoaded = true; NotifyState(); gate.Release(); }
    }

    /// <summary>Commits a valid draft only after successful atomic storage.</summary>
    /// <returns>Save completion; errors remain visible without changing the effective interval.</returns>
    public async Task SaveAsync()
    {
        if (IsSaving) return;
        var selected = SelectedSeconds;
        if (!IsSupported(selected)) { Status = "Bitte Aus, 30, 60, 120 oder 300 Sekunden auswählen."; Notify(nameof(Status)); return; }
        IsSaving = true; Notify(nameof(IsSaving));
        await gate.WaitAsync();
        try
        {
            await store.SaveAsync(selected);
            IntervalSeconds = selected; IsLoaded = true;
            Status = "Einstellung gespeichert.";
            NotifyState();
            Changed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception) { Status = "Einstellung konnte nicht gespeichert werden. Das bisherige Intervall bleibt wirksam; bitte erneut versuchen."; Notify(nameof(Status)); }
        finally { IsSaving = false; Notify(nameof(IsSaving)); gate.Release(); }
    }

    private void NotifyState()
    {
        Notify(nameof(IntervalSeconds)); Notify(nameof(Description)); Notify(nameof(IsLoaded)); Notify(nameof(Status));
    }
}
