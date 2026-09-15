namespace FlowNRW.Core.Transit;

/// <summary>Transport operator.</summary>
public sealed record Operator
{
    /// <summary>Provider identifier.</summary>
    public string? Id { get; init; } = null;

    /// <summary>Operator name.</summary>
    public string? Name { get; init; } = null;

}
