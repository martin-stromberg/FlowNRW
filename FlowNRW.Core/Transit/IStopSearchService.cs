namespace FlowNRW.Core.Transit;

/// <summary>Contract for StopSearchService.</summary>
public interface IStopSearchService
{
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

}
