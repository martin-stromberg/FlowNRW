using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FlowNRW.Core.Transit;

/// <summary>Selects regional providers, consolidates results and shares identical in-flight requests.</summary>
public sealed class ProviderOrchestrator : IProviderOrchestrator
{
    private readonly ITransitProvider national;
    private readonly IEfaProvider regional;
    private readonly INrwRegionClassifier classifier;
    private readonly IRealtimeConsolidator consolidator;
    private readonly ITransitCache cache;
    private readonly TransitCacheOptions options;
    private readonly Dictionary<string, object> pending = new();
    private readonly object gate = new();

    /// <summary>Composes the provider policy.</summary>
    /// <param name="national">National provider.</param>
    /// <param name="regional">Regional provider.</param>
    /// <param name="classifier">Official NRW boundary classifier.</param>
    /// <param name="consolidator">Unique event matcher.</param>
    /// <param name="cache">Typed memory cache.</param>
    /// <param name="options">Cache age and size limits.</param>
    public ProviderOrchestrator(ITransitProvider national, IEfaProvider regional, INrwRegionClassifier classifier,
        IRealtimeConsolidator consolidator, ITransitCache cache, TransitCacheOptions options)
    {
        options.Validate();
        this.national = national;
        this.regional = regional;
        this.classifier = classifier;
        this.consolidator = consolidator;
        this.cache = cache;
        this.options = options;
    }

    /// <inheritdoc />
    public string Name => "orchestrator";

    /// <inheritdoc />
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default) =>
        Run("search", text.Trim(), false, options.StopTimeToLive, (provider, token) => provider.SearchAsync(text.Trim(), token), null, cancellationToken);

    /// <inheritdoc />
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default) =>
        Run("nearby", coordinate, classifier.IsInNrw(coordinate), options.StopTimeToLive,
            (provider, token) => provider.NearbyAsync(coordinate, token), null, cancellationToken);

    /// <inheritdoc />
    public async Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        var resolvedOrigin = await ResolveRegion(origin, cancellationToken).ConfigureAwait(false);
        var resolvedDestination = await ResolveRegion(destination, cancellationToken).ConfigureAwait(false);
        origin = resolvedOrigin.Location;
        destination = resolvedDestination.Location;
        var result = await Run("route", new { origin, destination, departure }, IsRegional(origin) || IsRegional(destination), options.RealtimeTimeToLive,
            (provider, token) => provider.RouteAsync(origin, destination, departure, token), MergeJourneys, cancellationToken).ConfigureAwait(false);
        return result with { Warnings = result.Warnings.Concat(resolvedOrigin.Warnings).Concat(resolvedDestination.Warnings).Distinct().ToArray() };
    }

    /// <inheritdoc />
    public async Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        var resolved = await ResolveRegion(new Address { Name = stop.Name, Stop = stop, Coordinate = stop.Coordinate }, cancellationToken).ConfigureAwait(false);
        stop = resolved.Location.Stop ?? stop;
        var result = await Run("departures", new { stop, departure }, IsRegional(resolved.Location), options.RealtimeTimeToLive,
            (provider, token) => provider.DeparturesAsync(stop, departure, token), MergeEvents, cancellationToken).ConfigureAwait(false);
        return result with { Warnings = result.Warnings.Concat(resolved.Warnings).Distinct().ToArray() };
    }

    private async Task<(Address Location, IReadOnlyList<string> Warnings)> ResolveRegion(Address location, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (location.Coordinate is not null || location.Stop?.Coordinate is not null) return (location, []);
        string query = !string.IsNullOrWhiteSpace(location.Name) ? location.Name :
            !string.IsNullOrWhiteSpace(location.Stop?.Name) ? location.Stop.Name : location.Stop?.Id ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query)) return (location, ["region-unknown"]);
        var search = await SearchAsync(query, cancellationToken).ConfigureAwait(false);
        var candidates = search.HasData ? search.Items.Where(item => item.Coordinate is not null || item.Stop?.Coordinate is not null).ToArray() : [];
        if (location.Stop is { } selected)
        {
            candidates = candidates.Where(item => item.Stop is { } candidate &&
                ((selected.Source == candidate.Source && selected.Id == candidate.Id) ||
                 (!string.IsNullOrWhiteSpace(selected.Dhid) && selected.Dhid == candidate.Dhid))).ToArray();
        }
        var warnings = search.Warnings.Concat(search.ErrorCode is { } error ? new[] { error } : []).ToArray();
        return candidates.Length == 1 ? (candidates[0], warnings) : (location, warnings.Append("region-unknown").ToArray());
    }

    private bool IsRegional(Address address) => classifier.IsInNrw(address.Coordinate ?? address.Stop?.Coordinate);

    private async Task<ProviderResult<T>> Run<T>(string operation, object request, bool preferRegional, TimeSpan ttl,
        Func<ITransitProvider, CancellationToken, Task<ProviderResult<T>>> fetch,
        Func<IReadOnlyList<T>, IReadOnlyList<T>, IReadOnlyList<T>>? merge, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { operation, request, preferRegional }))));
        var cached = cache.Get<T>(key);
        if (cached is not null) return cached;
        Pending<T> work;
        lock (gate)
        {
            if (pending.TryGetValue(key, out var existing) && existing is Pending<T> found && !found.Cancellation.IsCancellationRequested)
                work = found;
            else
            {
                if (pending.Count >= options.MaxEntries) return new() { Source = Name, ErrorCode = "busy" };
                work = new Pending<T>();
                pending[key] = work;
                work.Task = Execute(key, preferRegional, ttl, fetch, merge, work.Cancellation.Token);
            }
            work.Waiters++;
        }
        try { return await work.Task.WaitAsync(cancellationToken).ConfigureAwait(false); }
        finally
        {
            lock (gate)
            {
                work.Waiters--;
                if (work.Waiters == 0)
                {
                    if (pending.TryGetValue(key, out var current) && ReferenceEquals(current, work)) pending.Remove(key);
                    if (!work.Task.IsCompleted) work.Cancellation.Cancel();
                    _ = work.Task.ContinueWith(_ => work.Cancellation.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }
            }
        }
    }

    private async Task<ProviderResult<T>> Execute<T>(string key, bool preferRegional, TimeSpan ttl,
        Func<ITransitProvider, CancellationToken, Task<ProviderResult<T>>> fetch,
        Func<IReadOnlyList<T>, IReadOnlyList<T>, IReadOnlyList<T>>? merge, CancellationToken cancellationToken)
    {
        var primary = await fetch(preferRegional ? regional : national, cancellationToken).ConfigureAwait(false);
        bool needsFallback = !primary.HasData || primary.Warnings.Count > 0;
        ProviderResult<T> result = primary;
        if (needsFallback || (preferRegional && merge is not null))
        {
            var secondary = await fetch(preferRegional ? national : regional, cancellationToken).ConfigureAwait(false);
            if (!primary.HasData && secondary.HasData)
                result = secondary with { IsFallback = true, Warnings = Codes(primary, secondary, "primary-unavailable") };
            else if (primary.HasData && secondary.HasData)
                result = primary with
                {
                    Items = merge is null ? (needsFallback ? secondary.Items : primary.Items) : merge(primary.Items, secondary.Items),
                    Source = merge is null && needsFallback ? secondary.Source : primary.Source + "+" + secondary.Source,
                    IsFallback = needsFallback,
                    Warnings = Codes(primary, secondary, needsFallback ? "partial-primary" : null)
                };
            else
                result = primary with { Warnings = Codes(primary, secondary, "secondary-unavailable") };
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (result.HasData) cache.Set(key, result, ttl);
        else if (cache.Get<T>(key, true) is { } stale)
            return stale with { Warnings = stale.Warnings.Concat(result.Warnings).Append("stale-fallback").ToArray() };
        return result;
    }

    private static string[] Codes<T>(ProviderResult<T> first, ProviderResult<T> second, string? extra) =>
        first.Warnings.Concat(second.Warnings).Concat(new[] { first.ErrorCode, second.ErrorCode, extra }.OfType<string>()).Distinct().ToArray();

    private IReadOnlyList<StopEvent> MergeEvents(IReadOnlyList<StopEvent> primary, IReadOnlyList<StopEvent> secondary) =>
        consolidator.Consolidate(consolidator.Consolidate(primary, secondary), primary);

    private IReadOnlyList<Journey> MergeJourneys(IReadOnlyList<Journey> primary, IReadOnlyList<Journey> secondary)
    {
        var secondaryEvents = secondary.SelectMany(journey => journey.Legs.SelectMany(leg => new[] { leg.Departure, leg.Arrival })).ToArray();
        return primary.Select(journey => journey with
        {
            Legs = journey.Legs.Select(leg =>
            {
                var events = MergeEvents(new[] { leg.Departure, leg.Arrival }, secondaryEvents);
                return leg with { Departure = events[0], Arrival = events[1] };
            }).ToArray()
        }).ToArray();
    }

    private sealed class Pending<T>
    {
        internal CancellationTokenSource Cancellation { get; } = new();
        internal Task<ProviderResult<T>> Task { get; set; } = null!;
        internal int Waiters { get; set; }
    }
}
