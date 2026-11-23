namespace FlowNRW.Core.Transit;

/// <summary>Normalized departure or arrival.</summary>
public sealed record StopEvent
{
    /// <summary>Event identity.</summary>
    /// <value>The configured or normalized value.</value>
    public TripIdentity Identity { get; init; } = new();

    /// <summary>Scheduled instant.</summary>
    public DateTimeOffset? PlannedTime { get; init; } = null;

    /// <summary>Optional realtime fields.</summary>
    /// <value>The configured or normalized value.</value>
    public RealtimeStatus Realtime { get; init; } = new();

    /// <summary>Line and operator.</summary>
    public Line? Line { get; init; } = null;

}
