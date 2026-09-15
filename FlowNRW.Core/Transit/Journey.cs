namespace FlowNRW.Core.Transit;

/// <summary>Normalized journey.</summary>
public sealed record Journey
{
    /// <summary>Provider journey identifier.</summary>
    public string? Id { get; init; } = null;

    /// <summary>Ordered journey legs.</summary>
    public IReadOnlyList<JourneyLeg> Legs { get; init; } = [];

    /// <summary>Changes between transit legs.</summary>
    public IReadOnlyList<Transfer> Transfers { get; init; } = [];

}
