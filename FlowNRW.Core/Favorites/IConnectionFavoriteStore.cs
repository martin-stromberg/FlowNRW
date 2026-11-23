namespace FlowNRW.Core.Favorites;

/// <summary>Stores explicitly saved reusable journey endpoint pairs.</summary>
public interface IConnectionFavoriteStore
{
    /// <summary>Loads all valid saved connections.</summary>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Saved connections in their stable order.</returns>
    Task<IReadOnlyList<ConnectionFavorite>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Atomically replaces saved connections after validation.</summary>
    /// <param name="favorites">Connections to persist.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Durable replacement completion.</returns>
    Task SaveAsync(IReadOnlyList<ConnectionFavorite> favorites, CancellationToken cancellationToken = default);
}
