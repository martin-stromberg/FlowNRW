namespace FlowNRW.Core.Transit;

/// <summary>Resolved address or coordinate usable as journey endpoint.</summary>
public sealed record Address
{
    /// <summary>Display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Resolved position.</summary>
    public GeoCoordinate? Coordinate { get; init; } = null;

    /// <summary>Optional provider stop identity.</summary>
    public Stop? Stop { get; init; } = null;

}
