using System.Globalization;
using static FlowNRW.Core.Transit.TransitJson;

namespace FlowNRW.Core.Transit;

/// <summary>Configurable db.transport.rest v6 adapter.</summary>
public sealed class DbRestProvider : ITransitProvider
{
    private readonly ITransitHttpGateway gateway;
    private readonly DbRestResponseMapper mapper;
    private readonly TransitProviderOptions options;
    /// <summary>Creates a provider.</summary>
    /// <param name="gateway">Bounded HTTP transport.</param>
    /// <param name="mapper">Response normalizer.</param>
    /// <param name="options">Endpoint and limits.</param>
    public DbRestProvider(ITransitHttpGateway gateway, DbRestResponseMapper mapper, TransitProviderOptions options) { options.Validate(); this.gateway = gateway; this.mapper = mapper; this.options = options; }
    /// <inheritdoc/>
    public string Name => "db-rest";
    /// <inheritdoc/>
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default) => string.IsNullOrWhiteSpace(text) || text.Length > options.MaxSearchLength ? Task.FromResult(new ProviderResult<Address> { Source = Name, ErrorCode = "invalid_input" }) : Fetch("locations", new() { ["query"] = text.Trim(), ["addresses"] = "true", ["stops"] = "true" }, mapper.Search, cancellationToken);
    /// <inheritdoc/>
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default) => Fetch("locations/nearby", new() { ["latitude"] = Decimal(coordinate.Latitude), ["longitude"] = Decimal(coordinate.Longitude) }, mapper.Nearby, cancellationToken);
    /// <inheritdoc/>
    public async Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default, bool arriveBy = false)
    {
        var query = new Dictionary<string, string> { ["departure"] = departure.ToString("O", CultureInfo.InvariantCulture), ["arriveBy"] = arriveBy.ToString(), ["polylines"] = "true" };
        if (!await Endpoint(origin, "from", query, cancellationToken) || !await Endpoint(destination, "to", query, cancellationToken)) return new() { Source = Name, ErrorCode = "unresolved_location" };
        return await Fetch("journeys", query, mapper.Journeys, cancellationToken);
    }
    /// <inheritdoc/>
    public async Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        var resolved = stop.Source == Name && !string.IsNullOrWhiteSpace(stop.Id) ? stop : (await SearchAsync(stop.Name, cancellationToken)).Items.Where(a => a.Stop is not null).Select(a => a.Stop).SingleOrDefaultSafe();
        if (resolved is null) return new() { Source = Name, ErrorCode = "unresolved_location" };
        return await Fetch("stops/" + System.Uri.EscapeDataString(resolved.Id) + "/departures", new() { ["when"] = departure.ToString("O", CultureInfo.InvariantCulture) }, mapper.Departures, cancellationToken);
    }
    private async Task<bool> Endpoint(Address address, string prefix, Dictionary<string, string> query, CancellationToken ct)
    {
        if (address.Stop is { } stop && stop.Source == Name && !string.IsNullOrWhiteSpace(stop.Id)) { query[prefix] = stop.Id; return true; }
        if ((address.Coordinate ?? address.Stop?.Coordinate) is { } coord) { query[prefix + ".latitude"] = Decimal(coord.Latitude); query[prefix + ".longitude"] = Decimal(coord.Longitude); query[prefix + ".address"] = string.IsNullOrWhiteSpace(address.Name) ? "Coordinate" : address.Name; return true; }
        var matches = (await SearchAsync(address.Name, ct)).Items;
        return matches.Count == 1 && (matches[0].Stop is not null || matches[0].Coordinate is not null) && await Endpoint(matches[0], prefix, query, ct);
    }
    private async Task<ProviderResult<T>> Fetch<T>(string path, Dictionary<string, string> query, Func<string, ProviderResult<T>> map, CancellationToken ct)
    {
        query["results"] = options.MaxResults.ToString(CultureInfo.InvariantCulture);
        var raw = await gateway.GetAsync(Name, Uri(options.DbRestBaseUrl, path, query), ct);
        return Propagate(raw, raw.HasData ? map(raw.Items[0]) : null);
    }
}

internal static class TransitSingleMatch
{
    internal static T? SingleOrDefaultSafe<T>(this IEnumerable<T?> items) where T : class { var matches = items.Take(2).ToArray(); return matches.Length == 1 ? matches[0] : null; }
}
