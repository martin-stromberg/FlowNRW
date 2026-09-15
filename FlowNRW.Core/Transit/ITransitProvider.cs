namespace FlowNRW.Core.Transit;

/// <summary>Contract for TransitProvider.</summary>
public interface ITransitProvider
{
    /// <summary>Stable provider namespace.</summary>
    string Name { get; }
    /// <summary>Find addresses and stops.</summary>
    /// <param name="text">text input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Find nearby stops.</summary>
    /// <param name="coordinate">coordinate input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default);

    /// <summary>Find journeys.</summary>
    /// <param name="origin">origin input.</param>
    /// <param name="destination">destination input.</param>
    /// <param name="departure">departure input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default);

    /// <summary>Find departures.</summary>
    /// <param name="stop">stop input.</param>
    /// <param name="departure">departure input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default);

}
