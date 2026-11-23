namespace FlowNRW.Core.Presentation;

/// <summary>Status semantics of a departure derived only from supplied realtime fields.</summary>
public enum DepartureStatus
{
    /// <summary>No realtime information is available; only the scheduled time is known.</summary>
    Scheduled,

    /// <summary>Realtime reports the departure on schedule without deviation.</summary>
    OnTime,

    /// <summary>Realtime reports a deviation from the scheduled time.</summary>
    Delayed,

    /// <summary>Realtime reports the departure as cancelled.</summary>
    Cancelled
}
