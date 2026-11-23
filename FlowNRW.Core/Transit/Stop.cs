namespace FlowNRW.Core.Transit;

/// <summary>Provider stop with preserved identifiers.</summary>
public sealed record Stop
{
    /// <summary>Provider identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Identifier namespace.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Shared national stop identifier when supplied.</summary>
    public string? Dhid { get; init; } = null;

    /// <summary>Position when supplied.</summary>
    public GeoCoordinate? Coordinate { get; init; } = null;

}
