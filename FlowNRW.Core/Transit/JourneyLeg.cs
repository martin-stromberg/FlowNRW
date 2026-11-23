namespace FlowNRW.Core.Transit;

/// <summary>Transit or walking leg.</summary>
public sealed record JourneyLeg
{
    /// <summary>Origin event.</summary>
    /// <value>The configured or normalized value.</value>
    public StopEvent Departure { get; init; } = new();

    /// <summary>Destination event.</summary>
    /// <value>The configured or normalized value.</value>
    public StopEvent Arrival { get; init; } = new();

    /// <summary>Transport line.</summary>
    public Line? Line { get; init; } = null;

    /// <summary>Walking information.</summary>
    public WalkingSegment? Walking { get; init; } = null;

    /// <summary>Provider supplied geometry.</summary>
    public GeoGeometry? Geometry { get; init; } = null;

}
