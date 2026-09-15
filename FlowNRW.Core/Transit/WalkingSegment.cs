namespace FlowNRW.Core.Transit;

/// <summary>Walking segment supplied by a provider.</summary>
public sealed record WalkingSegment
{
    /// <summary>Walking duration.</summary>
    public TimeSpan? Duration { get; init; } = null;

    /// <summary>Walking distance.</summary>
    public double? DistanceMeters { get; init; } = null;

    /// <summary>Supplied walking geometry.</summary>
    public GeoGeometry? Geometry { get; init; } = null;

}
