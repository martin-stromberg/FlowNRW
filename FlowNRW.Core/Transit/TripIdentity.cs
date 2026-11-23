namespace FlowNRW.Core.Transit;

/// <summary>Stop and trip identity used for conservative consolidation.</summary>
public sealed record TripIdentity
{
    /// <summary>Provider namespace.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Provider trip identifier.</summary>
    public string? TripId { get; init; } = null;

    /// <summary>Explicit cross-provider trip identifier; never inferred.</summary>
    public string? SharedTripId { get; init; } = null;

    /// <summary>Event stop.</summary>
    /// <value>The configured or normalized value.</value>
    public Stop Stop { get; init; } = new();

    /// <summary>Public line name.</summary>
    public string? Line { get; init; } = null;

    /// <summary>Operator name.</summary>
    public string? Operator { get; init; } = null;

    /// <summary>Direction supplied by provider.</summary>
    public string? Direction { get; init; } = null;

    /// <summary>Scheduled event instant.</summary>
    public DateTimeOffset? PlannedTime { get; init; } = null;

}
