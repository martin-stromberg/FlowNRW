namespace FlowNRW.Core.Transit;

/// <summary>Connection between successive transit legs.</summary>
public sealed record Transfer
{
    /// <summary>Transfer stop.</summary>
    public Stop? Stop { get; init; } = null;

    /// <summary>Available transfer time.</summary>
    public TimeSpan? Duration { get; init; } = null;

}
