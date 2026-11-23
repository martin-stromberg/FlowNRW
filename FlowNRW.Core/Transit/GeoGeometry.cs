namespace FlowNRW.Core.Transit;

/// <summary>Provider supplied geometry in WGS84.</summary>
public sealed record GeoGeometry
{
    /// <summary>Ordered geometry points.</summary>
    public IReadOnlyList<GeoCoordinate> Coordinates { get; init; } = [];

}
