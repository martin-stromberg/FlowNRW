namespace FlowNRW.Core.Transit;

/// <summary>Contract for RoutingService.</summary>
public interface IRoutingService
{
    /// <summary>Find journeys.</summary>
    /// <param name="origin">origin input.</param>
    /// <param name="destination">destination input.</param>
    /// <param name="departure">departure input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <param name="arriveBy">Whether the time is an arrival deadline.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default, bool arriveBy = false);

}
