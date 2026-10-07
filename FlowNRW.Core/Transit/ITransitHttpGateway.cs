namespace FlowNRW.Core.Transit;

/// <summary>Contract for TransitHttpGateway.</summary>
public interface ITransitHttpGateway
{
    /// <summary>Fetch JSON with bounded execution and safe diagnostics.</summary>
    /// <param name="provider">provider input.</param>
    /// <param name="uri">uri input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<string>> GetAsync(string provider, Uri uri, CancellationToken cancellationToken = default);

}
