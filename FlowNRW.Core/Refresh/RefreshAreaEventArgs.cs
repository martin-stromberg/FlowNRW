namespace FlowNRW.Core.Refresh;

/// <summary>Area payload of <see cref="RefreshLifecycle.ResumeCompleted"/>.</summary>
public sealed class RefreshAreaEventArgs : EventArgs
{
    /// <summary>Creates the event payload.</summary>
    /// <param name="area">Display area whose protected resume finished.</param>
    public RefreshAreaEventArgs(RefreshArea area) { Area = area; }

    /// <summary>Display area whose protected resume finished.</summary>
    public RefreshArea Area { get; }
}
