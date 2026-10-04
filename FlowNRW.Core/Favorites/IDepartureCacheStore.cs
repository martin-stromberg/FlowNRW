using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Favorites;

/// <summary>Persists bounded last-known departure boards for already saved favorite stops.</summary>
public interface IDepartureCacheStore
{
    /// <summary>Loads every valid cached board without accessing a transit provider.</summary>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Cached boards keyed by their technical favorite identities.</returns>
    Task<IReadOnlyList<DepartureCacheEntry>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Atomically replaces all cached boards after validating their technical identities.</summary>
    /// <param name="entries">Last-known successful boards.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Successful durable replacement completion.</returns>
    Task SaveAsync(IReadOnlyList<DepartureCacheEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>Atomically adds or replaces one successful board without removing other favorites' boards.</summary>
    /// <param name="entry">Last-known successful board.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Successful durable upsert completion.</returns>
    Task UpsertAsync(DepartureCacheEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Deletes one cached board when its favorite is removed.</summary>
    /// <param name="key">Technical favorite identity.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Successful durable deletion completion.</returns>
    Task RemoveAsync(DepartureCacheKey key, CancellationToken cancellationToken = default);

    /// <summary>Removes boards that no longer have a saved favorite identity.</summary>
    /// <param name="keys">Current saved favorite identities.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Successful durable cleanup completion.</returns>
    Task RemoveOrphansAsync(IReadOnlyCollection<DepartureCacheKey> keys, CancellationToken cancellationToken = default);
}

/// <summary>Technical provider-scoped favorite identity used as an on-device cache key.</summary>
/// <param name="Source">Provider namespace.</param>
/// <param name="Id">Provider stop identifier.</param>
/// <returns>A provider-scoped cache key.</returns>
public sealed record DepartureCacheKey(string Source, string Id);

/// <summary>One successful last-known board without location history.</summary>
public sealed record DepartureCacheEntry
{
    /// <summary>Provider namespace of the favorite stop.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Provider identifier of the favorite stop.</summary>
    public string StopId { get; init; } = string.Empty;

    /// <summary>Normalized successful result retained for immediate display.</summary>
    /// <returns>The successful departure board.</returns>
    public ProviderResult<StopEvent> Result { get; init; } = new();

    private DepartureCacheKey KeyForEntry => new(Source, StopId);

    /// <summary>Gets the technical key belonging to this entry.</summary>
    /// <returns>The provider-scoped cache key.</returns>
    public DepartureCacheKey Key => KeyForEntry;
}
