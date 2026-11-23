using System.Text.Json;
using static FlowNRW.Core.Transit.TransitJson;

namespace FlowNRW.Core.Transit;

/// <summary>Normalizes documented db.transport.rest v6 JSON.</summary>
public sealed class DbRestResponseMapper
{
    private readonly TransitProviderOptions options;
    /// <summary>Creates a bounded mapper.</summary>
    /// <param name="options">Validated response limits.</param>
    public DbRestResponseMapper(TransitProviderOptions options) { options.Validate(); this.options = options; }
    /// <summary>Maps search locations.</summary>
    /// <param name="json">Provider JSON.</param>
    /// <returns>Locations and status.</returns>
    public ProviderResult<Address> Search(string json)
    {
        return Map(json, "db-rest", options.MaxResults, r => r.ValueKind == JsonValueKind.Array, r => Array(r).Where(e => Text(e, "name") is not null || Text(e, "address") is not null).Select(e => new Address { Name = Text(e, "name") ?? Text(e, "address")!, Coordinate = Position(e), Stop = Text(e, "type") is "stop" or "station" && Text(e, "id") is not null ? Stop(e) : null }));
    }
    /// <summary>Maps nearby stops.</summary>
    /// <param name="json">Provider JSON.</param>
    /// <returns>Nearby stops.</returns>
    public ProviderResult<NearbyStopResult> Nearby(string json)
    {
        return Map(json, "db-rest", options.MaxResults, r => r.ValueKind == JsonValueKind.Array, r => Array(r).Where(e => Text(e, "id") is not null).Select(e => new NearbyStopResult { Stop = Stop(e), DistanceMeters = Number(Get(e, "distance")) }));
    }
    /// <summary>Maps departures.</summary>
    /// <param name="json">Provider JSON.</param>
    /// <returns>Normalized events.</returns>
    public ProviderResult<StopEvent> Departures(string json)
    {
        return Map(json, "db-rest", options.MaxResults, r => Get(r, "departures").ValueKind == JsonValueKind.Array, r => Array(Get(r, "departures")).Where(e => Text(Get(e, "stop"), "id") is not null).Select(e => Event(e, Get(e, "stop"), "plannedWhen", "when", "delay", "platform", "plannedPlatform")));
    }
    /// <summary>Maps connections.</summary>
    /// <param name="json">Provider JSON.</param>
    /// <returns>Connections with geometry and transfers.</returns>
    public ProviderResult<Journey> Journeys(string json)
    {
        return Map(json, "db-rest", options.MaxResults, r => Get(r, "journeys").ValueKind == JsonValueKind.Array, r => Array(Get(r, "journeys")).Select(e => { var legs = Array(Get(e, "legs")).Select(Leg).ToArray(); return new Journey { Id = Text(e, "refreshToken"), Legs = legs, Transfers = Transfers(legs) }; }).Where(j => j.Legs.Count > 0));
    }
    private static GeoCoordinate? Position(JsonElement e) { var l = Get(e, "location"); if (l.ValueKind != JsonValueKind.Object) l = e; return Coord(Number(Get(l, "latitude")), Number(Get(l, "longitude"))); }
    private static Stop Stop(JsonElement e) => new() { Id = Text(e, "id") ?? "", Name = Text(e, "name") ?? Text(e, "address") ?? "", Source = "db-rest", Dhid = Text(Get(e, "ids"), "dhid"), Coordinate = Position(e) };
    private static Line? Line(JsonElement e) => e.ValueKind != JsonValueKind.Object ? null : new() { Id = Text(e, "id"), Name = Text(e, "name"), Mode = Text(e, "mode"), Operator = Get(e, "operator").ValueKind == JsonValueKind.Object ? new Operator { Id = Text(Get(e, "operator"), "id"), Name = Text(Get(e, "operator"), "name") } : null };
    private static StopEvent Event(JsonElement e, JsonElement stop, string planned, string actual, string delay, string platform, string plannedPlatform)
    {
        var time = Time(e, planned); var line = Line(Get(e, "line")); var seconds = Number(Get(e, delay));
        return new() { PlannedTime = time, Line = line, Identity = new() { Source = "db-rest", TripId = Text(e, "tripId"), Stop = Stop(stop), Line = line?.Name, Operator = line?.Operator?.Name, Direction = Text(e, "direction"), PlannedTime = time }, Realtime = new() { ActualTime = seconds is not null ? Time(e, actual) : null, Delay = seconds is not null ? TimeSpan.FromSeconds(seconds.Value) : null, Cancelled = Bool(Get(e, "cancelled")), Platform = Text(e, platform), PlannedPlatform = Text(e, plannedPlatform), Source = "db-rest" } };
    }
    private static JourneyLeg Leg(JsonElement e)
    {
        var points = Array(Get(Get(e, "polyline"), "features")).Select(f => Pair(Get(Get(f, "geometry"), "coordinates"), true)).OfType<GeoCoordinate>().ToArray();
        GeoGeometry? geometry = points.Length > 0 ? new() { Coordinates = points } : null;
        var dep = Event(e, Get(e, "origin"), "plannedDeparture", "departure", "departureDelay", "departurePlatform", "plannedDeparturePlatform");
        var arr = Event(e, Get(e, "destination"), "plannedArrival", "arrival", "arrivalDelay", "arrivalPlatform", "plannedArrivalPlatform");
        return new() { Departure = dep, Arrival = arr, Line = Line(Get(e, "line")), Geometry = geometry, Walking = Bool(Get(e, "walking")) == true ? new() { Duration = arr.PlannedTime - dep.PlannedTime, DistanceMeters = Number(Get(e, "distance")), Geometry = geometry } : null };
    }
}
