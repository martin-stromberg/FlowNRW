namespace FlowNRW.Core.Transit;

/// <summary>Transport line.</summary>
public sealed record Line
{
    /// <summary>Provider line identifier.</summary>
    public string? Id { get; init; } = null;

    /// <summary>Public line name.</summary>
    public string? Name { get; init; } = null;

    /// <summary>Transport mode.</summary>
    public string? Mode { get; init; } = null;

    /// <summary>Transport operator.</summary>
    public Operator? Operator { get; init; } = null;

}
