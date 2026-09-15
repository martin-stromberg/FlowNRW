namespace FlowNRW.Core.Transit;

/// <summary>Bounded typed memory cache with explicit stale reads.</summary>
public interface ITransitCache
{
    /// <summary>Looks up a result without resetting its age.</summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="key">Opaque request key.</param>
    /// <param name="allowStale">Whether marked stale fallback is allowed.</param>
    /// <returns>The cached result or null.</returns>
    ProviderResult<T>? Get<T>(string key, bool allowStale = false);
    /// <summary>Stores a successful non-stale result.</summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="key">Opaque request key.</param>
    /// <param name="result">Result to cache.</param>
    /// <param name="timeToLive">Freshness period.</param>
    void Set<T>(string key, ProviderResult<T> result, TimeSpan timeToLive);
}
