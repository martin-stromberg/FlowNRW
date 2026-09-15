using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

internal sealed class TransitTestProvider : IProviderOrchestrator, IEfaProvider
{
    public string Name { get; init; } = "test";
    internal int Calls;
    internal Func<CancellationToken, Task<ProviderResult<Address>>> Search { get; set; } = _ => Task.FromResult(new ProviderResult<Address>());
    internal Func<CancellationToken, Task<ProviderResult<Journey>>>? RoutePending { get; set; }
    internal Func<CancellationToken, Task<ProviderResult<StopEvent>>>? DeparturesPending { get; set; }
    internal ProviderResult<Journey> Journeys { get; set; } = new();
    internal ProviderResult<StopEvent> Departures { get; set; } = new();
    internal ProviderResult<NearbyStopResult> Nearby { get; set; } = new();
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Calls);
        return Search(cancellationToken);
    }
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Calls);
        return Task.FromResult(Nearby);
    }
    public Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Calls);
        return RoutePending?.Invoke(cancellationToken) ?? Task.FromResult(Journeys);
    }
    public Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Calls);
        return DeparturesPending?.Invoke(cancellationToken) ?? Task.FromResult(Departures);
    }
}

internal sealed class TransitTestClock : TimeProvider
{
    internal DateTimeOffset Now { get; set; } = new(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}

internal sealed class TransitTestHandler : HttpMessageHandler
{
    internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Respond { get; set; } = (_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") });
    internal int Calls;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref Calls);
        return Respond(request, cancellationToken);
    }
}


