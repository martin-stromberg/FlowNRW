namespace FlowNRW.Core.Transit;

/// <summary>Provider data with non-sensitive status and provenance.</summary>
/// <typeparam name="T">Normalized item type.</typeparam>
public sealed record ProviderResult<T>
{
    /// <summary>Normalized items; never fabricated on errors.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];
    /// <summary>Provider identifier.</summary>
    public string Source { get; init; } = string.Empty;
    /// <summary>Retrieval instant; cache reads preserve it.</summary>
    public DateTimeOffset RetrievedAt { get; init; } = DateTimeOffset.UtcNow;
    /// <summary>Safe technical warning codes.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
    /// <summary>Safe technical error code, or null on success.</summary>
    public string? ErrorCode { get; init; }
    /// <summary>Whether a secondary source was used.</summary>
    public bool IsFallback { get; init; }
    /// <summary>Whether data is beyond its freshness period.</summary>
    public bool IsStale { get; init; }
    /// <summary>Whether any usable normalized items exist.</summary>
    public bool HasData => Items.Count > 0 && ErrorCode is null;
}
