namespace FlowNRW.Core.Transit;

/// <summary>Validated RoutingService operations with cancellation of superseded requests.</summary>
public sealed class RoutingService : IRoutingService
{
    private readonly IProviderOrchestrator provider;
    private readonly TransitProviderOptions options;
    private readonly object gate = new();
    private CancellationTokenSource? current;

    /// <summary>Creates a service scope whose newer requests supersede older ones.</summary>
    /// <param name="provider">Provider policy.</param>
    /// <param name="options">Validation limits.</param>
    public RoutingService(IProviderOrchestrator provider, TransitProviderOptions options)
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
    public Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default, bool arriveBy = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Valid(origin) || !Valid(destination))
            return Latest(_ => Task.FromResult(new ProviderResult<Journey> { Source = "validation", ErrorCode = "invalid-location" }), cancellationToken);
        return Latest(async token =>
        {
            var result = await provider.RouteAsync(origin, destination, departure, token, arriveBy).ConfigureAwait(false);
            return result with
            {
                Items = result.Items.Where(journey =>
                (journey.Legs.FirstOrDefault()?.Departure.Realtime.ActualTime ?? journey.Legs.FirstOrDefault()?.Departure.PlannedTime) >= departure)
                .OrderBy(journey => journey.Legs.FirstOrDefault()?.Departure.Realtime.ActualTime ?? journey.Legs.FirstOrDefault()?.Departure.PlannedTime ?? DateTimeOffset.MaxValue)
                .ThenBy(journey => journey.Legs.LastOrDefault()?.Arrival.PlannedTime ?? DateTimeOffset.MaxValue).Take(options.MaxResults).ToArray()
            };
        }, cancellationToken);
    }

    private bool Valid(Address address) => address is not null && address.Name.Length <= options.MaxSearchLength &&
        (address.Coordinate is not null || address.Stop?.Coordinate is not null ||
        !string.IsNullOrWhiteSpace(address.Stop?.Id) || !string.IsNullOrWhiteSpace(address.Name));
}
