using FlowNRW.Core.Favorites;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Retained explicit journey favorites independent from stop favorites and search history.</summary>
public sealed class ConnectionFavoritesViewModel : ObservableObject
{
    private readonly IConnectionFavoriteStore store;
    private bool loaded;

    /// <summary>Creates a retained connection favorite projection.</summary>
    /// <param name="store">Durable explicit connection store.</param>
    public ConnectionFavoritesViewModel(IConnectionFavoriteStore store) => this.store = store;

    /// <summary>Saved directed endpoint pairs.</summary>
    public IReadOnlyList<ConnectionFavorite> Favorites { get; private set; } = [];

    /// <summary>Loads the store once without replacing valid in-memory data on failure.</summary>
    /// <returns>Load completion.</returns>
    public async Task LoadAsync()
    {
        if (loaded) return;
        Favorites = await store.LoadAsync();
        loaded = true;
        Notify(nameof(Favorites));
    }

    /// <summary>Determines whether an exact directed stop pair is saved.</summary>
    /// <param name="origin">Candidate resolved start address.</param>
    /// <param name="destination">Candidate resolved destination address.</param>
    /// <returns>Whether the directed pair is saved.</returns>
    public bool Contains(Address? origin, Address? destination) => origin?.Stop is { } first && destination?.Stop is { } second
        && Favorites.Any(item => Same(item.Origin, first) && Same(item.Destination, second));

    /// <summary>Adds or removes an exact directed pair.</summary>
    /// <param name="origin">Resolved stop origin.</param>
    /// <param name="destination">Resolved stop destination.</param>
    /// <returns>Save completion.</returns>
    public async Task ToggleAsync(Address? origin, Address? destination)
    {
        if (origin?.Stop is not { } first || destination?.Stop is not { } second) return;
        await LoadAsync();
        var existing = Favorites.FirstOrDefault(item => Same(item.Origin, first) && Same(item.Destination, second));
        var next = existing is null ? Favorites.Append(new ConnectionFavorite { Origin = first, Destination = second }).ToArray()
            : Favorites.Where(item => !ReferenceEquals(item, existing)).ToArray();
        await store.SaveAsync(next);
        Favorites = next;
        Notify(nameof(Favorites));
    }

    private static bool Same(Stop left, Stop right) => left.Source == right.Source && left.Id == right.Id;
}
