namespace FlowNRW.Core.Transit;

/// <summary>Validated DepartureService operations with cancellation of superseded requests.</summary>
public sealed class DepartureService : IDepartureService
{
    private readonly IProviderOrchestrator provider;
    private readonly TransitProviderOptions options;
    private readonly object gate = new();
    private CancellationTokenSource? current;

    /// <summary>Creates a service scope whose newer requests supersede older ones.</summary>
    /// <param name="provider">Provider policy.</param>
    /// <param name="options">Validation limits.</param>
    public DepartureService(IProviderOrchestrator provider, TransitProviderOptions options)
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
    public Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (stop is null || string.IsNullOrWhiteSpace(stop.Id))
            return Latest(_ => Task.FromResult(new ProviderResult<StopEvent> { Source = "validation", ErrorCode = "invalid-stop" }), cancellationToken);
        return Latest(async token =>
        {
            var result = await provider.DeparturesAsync(stop, departure, token).ConfigureAwait(false);
            var ordered = result.Items.OrderBy(item => item.PlannedTime ?? DateTimeOffset.MaxValue).Take(options.MaxResults + 1).ToArray();
            return result with { Items = ordered.Take(options.MaxResults).ToArray(), Warnings = ordered.Length > options.MaxResults ? result.Warnings.Append("truncated-response").Distinct().ToArray() : result.Warnings };
        }, cancellationToken);
    }
}
