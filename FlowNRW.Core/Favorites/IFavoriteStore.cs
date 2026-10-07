using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Favorites;

/// <summary>Persists only the ordered technical identities of selected stops.</summary>
public interface IFavoriteStore
{
    /// <summary>Loads the saved stop sequence or fails without replacing it.</summary>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Complete saved stops in stable order.</returns>
    Task<IReadOnlyList<Stop>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Atomically replaces the saved sequence after validation.</summary>
    /// <param name="stops">Technical stop identities.</param>
    /// <param name="cancellationToken">Cancellation before the atomic replacement.</param>
    /// <returns>Successful durable replacement completion.</returns>
    Task SaveAsync(IReadOnlyList<Stop> stops, CancellationToken cancellationToken = default);
}
