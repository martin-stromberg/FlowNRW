namespace FlowNRW.Core.Transit;

/// <summary>Nearby stop with optional provider distance.</summary>
public sealed record NearbyStopResult
{
    /// <summary>Resolved stop.</summary>
    /// <value>The configured or normalized value.</value>
    public Stop Stop { get; init; } = new();

    /// <summary>Supplied distance in meters.</summary>
    public double? DistanceMeters { get; init; } = null;

}
