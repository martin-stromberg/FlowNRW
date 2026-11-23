using FlowNRW.Core.Presentation;

namespace FlowNRW.Core.Refresh;

/// <summary>Shared foreground activity reported by the native window lifecycle.</summary>
public sealed class ForegroundState : ObservableObject
{
    private bool active;
    /// <summary>Whether the application window is currently active.</summary>
    public bool IsActive
    {
        get => active;
        set { if (active == value) return; active = value; Notify(); }
    }
}
