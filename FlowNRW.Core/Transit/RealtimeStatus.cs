namespace FlowNRW.Core.Transit;

/// <summary>Realtime fields; null means unknown, including cancellation.</summary>
public sealed record RealtimeStatus
{
    /// <summary>Reported realtime instant.</summary>
    public DateTimeOffset? ActualTime { get; init; } = null;

    /// <summary>Reported delay.</summary>
    public TimeSpan? Delay { get; init; } = null;

    /// <summary>Reported cancellation state.</summary>
    public bool? Cancelled { get; init; } = null;

    /// <summary>Reported platform.</summary>
    public string? Platform { get; init; } = null;

    /// <summary>Scheduled platform.</summary>
    public string? PlannedPlatform { get; init; } = null;

    /// <summary>Realtime provider.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Retrieval instant.</summary>
    public DateTimeOffset RetrievedAt { get; init; } = DateTimeOffset.UtcNow;

}
