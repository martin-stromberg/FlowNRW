namespace FlowNRW.Core.Transit;

/// <summary>Limits for transient memory-only transit caching.</summary>
public sealed record TransitCacheOptions
{
    /// <summary>Maximum entries across all result types.</summary>
    public int MaxEntries { get; init; } = 256;
    /// <summary>Stop and address freshness.</summary>
    /// <value>The configured or normalized value.</value>
    public TimeSpan StopTimeToLive { get; init; } = TimeSpan.FromHours(24);
    /// <summary>Journey and departure freshness.</summary>
    /// <value>The configured or normalized value.</value>
    public TimeSpan RealtimeTimeToLive { get; init; } = TimeSpan.FromSeconds(30);
    /// <summary>Maximum total age of stale realtime fallback data.</summary>
    /// <value>The configured or normalized value.</value>
    public TimeSpan MaxStaleAge { get; init; } = TimeSpan.FromMinutes(5);
    /// <summary>Validates cache limits.</summary>
    public void Validate()
    {
        if (MaxEntries is < 1 or > 4096 || StopTimeToLive <= TimeSpan.Zero ||
            RealtimeTimeToLive <= TimeSpan.Zero || MaxStaleAge < RealtimeTimeToLive ||
            MaxStaleAge > TimeSpan.FromMinutes(5))
            throw new ArgumentException("Invalid transit cache configuration.");
    }
}
