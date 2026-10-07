using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Favorites;

/// <summary>A directed, explicitly saved pair of stop endpoints.</summary>
public sealed record ConnectionFavorite
{
    /// <summary>Gets the saved start stop.</summary>
    /// <value>Start stop of the directed favorite.</value>
    public Stop Origin { get; init; } = new();

    /// <summary>Gets the saved destination stop.</summary>
    /// <value>Destination stop of the directed favorite.</value>
    public Stop Destination { get; init; } = new();

    /// <summary>Human-readable connection name.</summary>
    public string Name => Origin.Name + " → " + Destination.Name;
}
