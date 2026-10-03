using System.Globalization;
using static FlowNRW.Core.Transit.TransitJson;

namespace FlowNRW.Core.Transit;

/// <summary>Complete configurable EFA RapidJSON development adapter.</summary>
public sealed class EfaProvider : IEfaProvider
{
    private readonly ITransitHttpGateway gateway;
    private readonly EfaResponseMapper mapper;
    private readonly TransitProviderOptions options;
    /// <summary>Creates the EFA adapter.</summary>
    /// <param name="gateway">Bounded HTTPS transport.</param>
    /// <param name="mapper">RapidJSON normalizer.</param>
    /// <param name="options">Primary and optional fallback endpoint.</param>
    public EfaProvider(ITransitHttpGateway gateway, EfaResponseMapper mapper, TransitProviderOptions options) { options.Validate(); this.gateway = gateway; this.mapper = mapper; this.options = options; }
    /// <inheritdoc/>
    public string Name => "efa";
    /// <inheritdoc/>
    public Task<ProviderResult<Address>> SearchAsync(string text, CancellationToken cancellationToken = default) => string.IsNullOrWhiteSpace(text) || text.Length > options.MaxSearchLength ? Task.FromResult(new ProviderResult<Address> { Source = Name, ErrorCode = "invalid_input" }) : Fetch("XML_STOPFINDER_REQUEST", new() { ["name_sf"] = text.Trim(), ["type_sf"] = "any", ["doNotSearchForStops_sf"] = "0" }, mapper.Search, cancellationToken);
    /// <inheritdoc/>
    public Task<ProviderResult<NearbyStopResult>> NearbyAsync(GeoCoordinate coordinate, CancellationToken cancellationToken = default) => Fetch("XML_COORD_REQUEST", new() { ["coord"] = Coordinate(coordinate), ["coordListOutputFormat"] = "STRING", ["inclFilter"] = "1", ["radius_1"] = "1000", ["type_1"] = "STOP", ["max"] = options.MaxResults.ToString(CultureInfo.InvariantCulture) }, mapper.Nearby, cancellationToken);
    /// <inheritdoc/>
    public async Task<ProviderResult<Journey>> RouteAsync(Address origin, Address destination, DateTimeOffset departure, CancellationToken cancellationToken = default, bool arriveBy = false)
    {
        var query = DateQuery(departure);
        if (!await Endpoint(origin, "origin", query, cancellationToken) || !await Endpoint(destination, "destination", query, cancellationToken)) return new() { Source = Name, ErrorCode = "unresolved_location" };
        query["calcNumberOfTrips"] = options.MaxResults.ToString(CultureInfo.InvariantCulture);
        query["itdTripDateTimeDepArr"] = arriveBy ? "arr" : "dep";
        return await Fetch("XML_TRIP_REQUEST2", query, mapper.Journeys, cancellationToken);
    }
    /// <inheritdoc/>
    public async Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default)
    {
        var resolved = stop;
        if (stop.Source != Name || string.IsNullOrWhiteSpace(stop.Id))
        {
            var matches = (await SearchAsync(stop.Name, cancellationToken)).Items.Where(a => a.Stop is not null).Select(a => a.Stop);
            resolved = matches.SingleOrDefaultSafe();
            if (resolved is null) return new() { Source = Name, ErrorCode = "unresolved_location" };
        }
        var query = DateQuery(departure); query["name_dm"] = resolved.Id; query["type_dm"] = "stop"; query["mode"] = "direct"; query["limit"] = options.MaxResults.ToString(CultureInfo.InvariantCulture); query["useRealtime"] = "1";
        return await Fetch("XML_DM_REQUEST", query, mapper.Departures, cancellationToken);
    }
    private async Task<bool> Endpoint(Address address, string suffix, Dictionary<string, string> query, CancellationToken ct)
    {
        if (address.Stop is { } stop && stop.Source == Name && !string.IsNullOrWhiteSpace(stop.Id)) { query["type_" + suffix] = "stop"; query["name_" + suffix] = stop.Id; return true; }
        if ((address.Coordinate ?? address.Stop?.Coordinate) is { } coord) { query["type_" + suffix] = "coord"; query["name_" + suffix] = Coordinate(coord); return true; }
        var matches = (await SearchAsync(address.Name, ct)).Items;
        return matches.Count == 1 && (matches[0].Stop is not null || matches[0].Coordinate is not null) && await Endpoint(matches[0], suffix, query, ct);
    }
    private static string Coordinate(GeoCoordinate c) => Decimal(c.Longitude) + ":" + Decimal(c.Latitude) + ":WGS84[DD.ddddd]";
    private static Dictionary<string, string> DateQuery(DateTimeOffset date)
    {
        var local = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"));
        return new() { ["itdDate"] = local.ToString("yyyyMMdd", CultureInfo.InvariantCulture), ["itdTime"] = local.ToString("HHmm", CultureInfo.InvariantCulture) };
    }
    private async Task<ProviderResult<T>> Fetch<T>(string path, Dictionary<string, string> query, Func<string, ProviderResult<T>> map, CancellationToken ct)
    {
        query["outputFormat"] = options.EfaFormat; query["version"] = "10.4.18.18"; query["coordOutputFormat"] = "WGS84[DD.ddddd]";
        var raw = await gateway.GetAsync(Name, Uri(options.EfaBaseUrl, path, query), ct);
        var primary = Propagate(raw, raw.HasData ? map(raw.Items[0]) : null);
        if (primary.HasData || options.EfaFallbackBaseUrl is null) return primary;
        ct.ThrowIfCancellationRequested();
        var fallback = await gateway.GetAsync(Name, Uri(options.EfaFallbackBaseUrl, path, query), ct);
        var result = Propagate(fallback, fallback.HasData ? map(fallback.Items[0]) : null);
        return result with { IsFallback = true, Warnings = primary.Warnings.Concat(result.Warnings).Append("efa_endpoint_fallback").Distinct().ToArray() };
    }
}
