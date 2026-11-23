using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Refresh;

/// <summary>Uses the configured realtime lifetime to decide whether retained data needs renewal.</summary>
public sealed class RefreshFreshness
{
    private readonly TimeSpan lifetime;
    private readonly TimeProvider clock;

    /// <summary>Creates the shared freshness policy without accessing a provider.</summary>
    /// <param name="options">Validated cache lifetimes.</param>
    /// <param name="clock">Optional deterministic clock.</param>
    public RefreshFreshness(TransitCacheOptions options, TimeProvider? clock = null)
    {
        options.Validate();
        lifetime = options.RealtimeTimeToLive;
        this.clock = clock ?? TimeProvider.System;
    }

    /// <summary>Checks missing, stale, expired or implausibly future-dated data.</summary>
    /// <typeparam name="T">Provider item type.</typeparam>
    /// <param name="result">Retained response.</param>
    /// <returns>Whether another request is needed.</returns>
    public bool IsStale<T>(ProviderResult<T>? result)
    {
        if (result is null || result.IsStale || result.ErrorCode is not null) return true;
        var age = clock.GetUtcNow() - result.RetrievedAt;
        return age < TimeSpan.Zero || age >= lifetime;
    }
}
