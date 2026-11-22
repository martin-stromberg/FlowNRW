namespace FlowNRW.Core.Refresh;

/// <summary>Area and occasion payload of <see cref="RefreshLifecycle.RefreshFailed"/>, raised only while the area stays visible.</summary>
public sealed class RefreshFailedEventArgs : EventArgs
{
    /// <summary>Creates the event payload.</summary>
    /// <param name="area">Display area whose refresh failed.</param>
    /// <param name="occasion">Refresh trigger that failed.</param>
    public RefreshFailedEventArgs(RefreshArea area, RefreshOccasion occasion) { Area = area; Occasion = occasion; }

    /// <summary>Display area whose refresh failed.</summary>
    public RefreshArea Area { get; }

    /// <summary>Refresh trigger that failed.</summary>
    public RefreshOccasion Occasion { get; }
}
