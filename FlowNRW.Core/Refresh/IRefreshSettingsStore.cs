namespace FlowNRW.Core.Refresh;

/// <summary>Persists one bounded foreground-refresh interval.</summary>
public interface IRefreshSettingsStore
{
    /// <summary>Loads a valid interval, with null indicating an absent file.</summary>
    /// <param name="cancellationToken">Read cancellation.</param>
    /// <returns>Saved seconds or null for the default.</returns>
    Task<int?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Atomically saves an explicitly selected valid interval.</summary>
    /// <param name="seconds">Zero or a supported interval.</param>
    /// <param name="cancellationToken">Cancellation before replacement.</param>
    /// <returns>Durable write completion.</returns>
    Task SaveAsync(int seconds, CancellationToken cancellationToken = default);
}
