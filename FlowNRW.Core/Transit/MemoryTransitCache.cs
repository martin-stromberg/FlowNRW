namespace FlowNRW.Core.Transit;

/// <summary>Thread-safe bounded cache; never persists query or location history.</summary>
public sealed class MemoryTransitCache : ITransitCache
{
    private readonly Dictionary<(Type, string), (object Value, DateTimeOffset Stored, TimeSpan Ttl)> entries = new();
    private readonly TransitCacheOptions options;
    private readonly TimeProvider clock;
    private readonly object gate = new();

    /// <summary>Creates an empty bounded cache.</summary>
    /// <param name="options">Size and age bounds.</param>
    /// <param name="clock">Testable clock.</param>
    public MemoryTransitCache(TransitCacheOptions options, TimeProvider? clock = null)
    {
        options.Validate();
        this.options = options;
        this.clock = clock ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public ProviderResult<T>? Get<T>(string key, bool allowStale = false)
    {
        lock (gate)
        {
            var typedKey = (typeof(T), key);
            if (!entries.TryGetValue(typedKey, out var entry)) return null;
            var age = clock.GetUtcNow() - entry.Stored;
            if (age < entry.Ttl) return (ProviderResult<T>)entry.Value;
            if (age > options.MaxStaleAge)
            {
                entries.Remove(typedKey);
                return null;
            }
            return allowStale ? ((ProviderResult<T>)entry.Value) with { IsStale = true, IsFallback = true } : null;
        }
    }

    /// <inheritdoc />
    public void Set<T>(string key, ProviderResult<T> result, TimeSpan timeToLive)
    {
        if (!result.HasData || result.IsStale || timeToLive <= TimeSpan.Zero) return;
        lock (gate)
        {
            var typedKey = (typeof(T), key);
            if (!entries.ContainsKey(typedKey) && entries.Count >= options.MaxEntries)
                entries.Remove(entries.MinBy(entry => entry.Value.Stored).Key);
            entries[typedKey] = (result, clock.GetUtcNow(), timeToLive);
        }
    }
}
