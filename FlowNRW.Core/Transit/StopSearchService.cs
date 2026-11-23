namespace FlowNRW.Core.Transit;

/// <summary>Validated StopSearchService operations with cancellation of superseded requests.</summary>
public sealed class StopSearchService : IStopSearchService
{
    private readonly IProviderOrchestrator provider;
    private readonly TransitProviderOptions options;
    private readonly object gate = new();
    private CancellationTokenSource? current;

    /// <summary>Creates a service scope whose newer requests supersede older ones.</summary>
    /// <param name="provider">Provider policy.</param>
    /// <param name="options">Validation limits.</param>
    public StopSearchService(IProviderOrchestrator provider, TransitProviderOptions options)
    {
        options.Validate();
        this.provider = provider;
        this.options = options;
    }

    private async Task<ProviderResult<T>> Latest<T>(Func<CancellationToken, Task<ProviderResult<T>>> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var request = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        lock (gate)
        {
            current?.Cancel();
            current = request;
        }
        try
        {
            var result = await action(request.Token).ConfigureAwait(false);
            request.Token.ThrowIfCancellationRequested();
            return result;
        }
        finally
        {
            lock (gate)
            {
                if (ReferenceEquals(current, request)) current = null;
            }
        }
    }
    /// <inheritdoc />
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > options.MaxSearchLength)
            return Latest(_ => Task.FromResult(new ProviderResult<Address> { Source = "validation", ErrorCode = "invalid-search" }), cancellationToken);
        return Latest(token => provider.SearchAsync(text.Trim(), token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default) =>
        Latest(token => provider.NearbyAsync(coordinate, token), cancellationToken);
}

