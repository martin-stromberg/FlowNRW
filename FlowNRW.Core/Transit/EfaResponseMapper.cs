using System.Text.Json;
using static FlowNRW.Core.Transit.TransitJson;

namespace FlowNRW.Core.Transit;

/// <summary>Normalizes EFA RapidJSON with explicit WGS84 coordinate output.</summary>
public sealed class EfaResponseMapper
{
    private readonly TransitProviderOptions options;
    /// <summary>Creates the mapper.</summary>
    /// <param name="options">Response limits.</param>
    public EfaResponseMapper(TransitProviderOptions options) { options.Validate(); this.options = options; }
    /// <summary>Maps resolved locations.</summary>
    /// <param name="json">RapidJSON response.</param>
    /// <returns>Addresses and stops.</returns>
    public ProviderResult<Address> Search(string json)
    {
        return Map(json, "efa", options.MaxResults, r => Get(r, "locations").ValueKind == JsonValueKind.Array, r => Array(Get(r, "locations")).Where(e => Text(e, "name") is not null).Select(e => new Address { Name = Text(e, "name")!, Coordinate = Pair(Get(e, "coord")), Stop = Text(e, "type") is "stop" or "platform" && Text(e, "id") is not null ? Stop(e) : null }));
    }
    /// <summary>Maps stops near a coordinate.</summary>
    /// <param name="json">RapidJSON response.</param>
    /// <returns>Nearby stops with supplied distances.</returns>
    public ProviderResult<NearbyStopResult> Nearby(string json)
    {
        return Map(json, "efa", options.MaxResults, r => Get(r, "locations").ValueKind == JsonValueKind.Array, r => Array(Get(r, "locations")).Where(e => Text(e, "id") is not null && Text(e, "type") is "stop" or "platform").Select(e => new NearbyStopResult { Stop = Stop(e), DistanceMeters = Number(Get(Get(e, "properties"), "distance")) }));
    }
    /// <summary>Maps departure monitor events.</summary>
    /// <param name="json">RapidJSON response.</param>
    /// <returns>Normalized departures.</returns>
    public ProviderResult<StopEvent> Departures(string json)
    {
        return Map(json, "efa", options.MaxResults, r => Get(r, "stopEvents").ValueKind == JsonValueKind.Array, r => Array(Get(r, "stopEvents")).Where(e => Text(Get(e, "location"), "id") is not null).Select(e => Event(e, Get(e, "location"), Get(e, "transportation"), "departure")));
    }
    /// <summary>Maps journey legs, walking, transfers and geometry.</summary>
    /// <param name="json">RapidJSON response.</param>
    /// <returns>Normalized connections.</returns>
    public ProviderResult<Journey> Journeys(string json)
    {
        return Map(json, "efa", options.MaxResults, r => Get(r, "journeys").ValueKind == JsonValueKind.Array, r => Array(Get(r, "journeys")).Select(e => { var legs = Array(Get(e, "legs")).Select(Leg).ToArray(); return new Journey { Id = Text(e, "id"), Legs = legs, Transfers = Transfers(legs) }; }).Where(j => j.Legs.Count > 0));
    }
    private static Stop Stop(JsonElement e)
    {
        var original = e;
        if (Text(e, "type") == "platform" && Text(Get(e, "parent"), "type") == "stop") e = Get(e, "parent");
        var id = Text(e, "id") ?? "";
        return new() { Id = id, Name = Text(e, "name") ?? "", Source = "efa", Dhid = Bool(Get(e, "isGlobalId")) == true && id.StartsWith("de:", StringComparison.Ordinal) ? id : null, Coordinate = Pair(Get(e, "coord")) ?? Pair(Get(original, "coord")) };
    }
    private static Line? Line(JsonElement t) => t.ValueKind != JsonValueKind.Object ? null : new() { Id = Text(t, "id"), Name = Text(t, "disassembledName") ?? Text(t, "name") ?? Text(t, "number"), Mode = Text(Get(t, "product"), "name"), Operator = Get(t, "operator").ValueKind == JsonValueKind.Object ? new Operator { Id = Text(Get(t, "operator"), "id"), Name = Text(Get(t, "operator"), "name") } : null };
    private static StopEvent Event(JsonElement e, JsonElement location, JsonElement transport, string prefix)
    {
        var planned = Time(e, prefix + "TimePlanned"); var actual = Time(e, prefix + "TimeEstimated"); var line = Line(transport);
        var codes = Array(Get(e, "realtimeStatus")).Select(String).ToArray();
        var trip = Text(Get(transport, "properties"), "tripCode"); var lineId = Text(transport, "id");
        return new() { PlannedTime = planned, Line = line, Identity = new() { Source = "efa", Stop = Stop(location), TripId = trip is not null && lineId is not null ? lineId + ":" + trip : null, Line = line?.Name, Operator = line?.Operator?.Name, Direction = Text(Get(transport, "destination"), "name"), PlannedTime = planned }, Realtime = new() { ActualTime = actual, Delay = actual - planned, Cancelled = codes.Any(c => c is "CANCELLED" or "DEPARTURE_CANCELLED" or "ARRIVAL_CANCELLED") ? true : Bool(Get(e, "cancelled")), Platform = Text(Get(location, "properties"), "platformName"), PlannedPlatform = Text(Get(location, "properties"), "plannedPlatformName"), Source = "efa" } };
    }
    private static JourneyLeg Leg(JsonElement e)
    {
        var origin = Get(e, "origin"); var destination = Get(e, "destination"); var transport = Get(e, "transportation");
        var points = Array(Get(e, "coords")).Select(p => Pair(p)).OfType<GeoCoordinate>().ToArray();
        GeoGeometry? geometry = points.Length > 0 ? new() { Coordinates = points } : null;
        var walking = Number(Get(Get(transport, "product"), "class")) is 99 or 100 || Number(Get(transport, "id")) == 99;
        var seconds = Number(Get(e, "duration"));
        return new() { Departure = Event(origin, origin, transport, "departure"), Arrival = Event(destination, destination, transport, "arrival"), Line = walking ? null : Line(transport), Geometry = geometry, Walking = walking ? new() { Duration = seconds is not null ? TimeSpan.FromSeconds(seconds.Value) : null, DistanceMeters = Number(Get(e, "distance")), Geometry = geometry } : null };
    }
}
