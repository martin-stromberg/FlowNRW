using System.Globalization;
using System.Text.Json;

namespace FlowNRW.Core.Transit;

internal static class TransitJson
{
    internal static JsonElement Get(JsonElement e, string key) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var p) ? p : default;
    internal static string? Text(JsonElement e, string key) => String(Get(e, key));
    internal static string? String(JsonElement e) => e.ValueKind is JsonValueKind.String or JsonValueKind.Number ? e.ToString() : null;
    internal static IEnumerable<JsonElement> Array(JsonElement e) => e.ValueKind == JsonValueKind.Array ? e.EnumerateArray() : [];
    internal static double? Number(JsonElement e) => e.ValueKind == JsonValueKind.Number && e.TryGetDouble(out var n) && double.IsFinite(n) ? n : null;
    internal static bool? Bool(JsonElement e) => e.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null };
    internal static DateTimeOffset? Time(JsonElement e, string key)
    {
        var s = Text(e, key);
        return s is not null && (s.EndsWith('Z') || s.Length >= 6 && (s[^6] == '+' || s[^6] == '-')) && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? t : null;
    }
    internal static GeoCoordinate? Coord(double? lat, double? lon) => lat is >= -90 and <= 90 && lon is >= -180 and <= 180 ? new(lat.Value, lon.Value) : null;
    internal static GeoCoordinate? Pair(JsonElement e, bool reverse = false)
    {
        var a = Array(e).Take(2).ToArray();
        return a.Length == 2 ? Coord(Number(a[reverse ? 1 : 0]), Number(a[reverse ? 0 : 1])) : null;
    }
    internal static ProviderResult<T> Map<T>(string json, string source, int limit, Func<JsonElement, IEnumerable<T>> map)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var warnings = Array(Get(root, "systemMessages")).Select(_ => "provider_message").Distinct().ToList();
            var items = map(root).Take(limit).ToArray();
            if (HasInvalidTime(root)) warnings.Add("invalid_time");
            if (ContainsInformation(root)) warnings.Add("provider_information");
            if (items.Length == 0) warnings.Add("empty_response");
            return new() { Items = items, Source = source, Warnings = warnings, ErrorCode = items.Length == 0 && Array(Get(root, "systemMessages")).Any(m => Text(m, "type") == "error") ? "provider_error" : null };
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or FormatException or OverflowException)
        {
            return new() { Source = source, ErrorCode = "invalid_response", Warnings = ["invalid_response"] };
        }
    }
    private static bool HasInvalidTime(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array) return Array(element).Any(HasInvalidTime);
        if (element.ValueKind != JsonValueKind.Object) return false;
        return element.EnumerateObject().Any(p => ((p.Name is "plannedWhen" or "when" or "plannedDeparture" or "plannedArrival" or "departure" or "arrival" || p.Name.StartsWith("departureTime", StringComparison.Ordinal) || p.Name.StartsWith("arrivalTime", StringComparison.Ordinal)) && p.Value.ValueKind != JsonValueKind.Null && Time(element, p.Name) is null) || HasInvalidTime(p.Value));
    }
    private static bool ContainsInformation(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array) return Array(element).Any(ContainsInformation);
        if (element.ValueKind != JsonValueKind.Object) return false;
        return element.EnumerateObject().Any(p => (p.Name is "infos" or "remarks" && Array(p.Value).Any()) || ContainsInformation(p.Value));
    }
    internal static IReadOnlyList<Transfer> Transfers(IReadOnlyList<JourneyLeg> legs)
    {
        var transit = legs.Where(l => l.Walking is null).ToArray();
        return transit.Zip(transit.Skip(1), (a, b) => new Transfer { Stop = b.Departure.Identity.Stop, Duration = b.Departure.PlannedTime - a.Arrival.PlannedTime }).ToArray();
    }
    internal static Uri Uri(Uri baseUrl, string path, IEnumerable<KeyValuePair<string, string>> query) => new(baseUrl.AbsoluteUri.TrimEnd('/') + "/" + path + "?" + string.Join("&", query.Select(p => System.Uri.EscapeDataString(p.Key) + "=" + System.Uri.EscapeDataString(p.Value))));
    internal static string Decimal(double value) => value.ToString(CultureInfo.InvariantCulture);
    internal static ProviderResult<T> Propagate<T>(ProviderResult<string> raw, ProviderResult<T>? mapped = null) => (mapped ?? new ProviderResult<T>()) with { Source = raw.Source, RetrievedAt = raw.RetrievedAt, ErrorCode = raw.ErrorCode ?? mapped?.ErrorCode, Warnings = raw.Warnings.Concat(mapped?.Warnings ?? []).Distinct().ToArray(), IsFallback = raw.IsFallback, IsStale = raw.IsStale };
}

