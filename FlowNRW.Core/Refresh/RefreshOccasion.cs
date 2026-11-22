namespace FlowNRW.Core.Refresh;

/// <summary>Refresh trigger mapped centrally to its functional refresh step.</summary>
public enum RefreshOccasion
{
    /// <summary>Page appears loaded; every card receives a regular provider refresh.</summary>
    Startup,

    /// <summary>Favorite membership changed; only stale or missing cards refresh.</summary>
    CardsAdded,

    /// <summary>Window returns to the foreground; stale data of the visible area refreshes.</summary>
    ForegroundReturn
}
