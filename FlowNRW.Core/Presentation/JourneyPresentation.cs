using System.Globalization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Shared textual rendering preserving unknown realtime values.</summary>
public static class JourneyPresentation
{
    private static readonly TimeZoneInfo Berlin = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

    /// <summary>Converts an instant into the Europe/Berlin display zone.</summary>
    /// <param name="time">Supplied instant.</param>
    /// <returns>The same instant expressed in Europe/Berlin local time.</returns>
    public static DateTimeOffset LocalTime(DateTimeOffset time)
    {
        return TimeZoneInfo.ConvertTime(time, Berlin);
    }

    /// <summary>Formats an instant as a local wall-clock reading.</summary>
    /// <param name="time">Supplied instant.</param>
    /// <returns><c>HH:mm</c> in Europe/Berlin, or „unbekannt" when no instant was supplied.</returns>
    public static string Clock(DateTimeOffset? time) => time is { } instant ? LocalTime(instant).ToString("HH:mm", CultureInfo.InvariantCulture) : "unbekannt";

    /// <summary>Formats an instant with its date and offset.</summary>
    /// <param name="time">Supplied instant.</param>
    /// <returns>Explicit local date and offset.</returns>
    public static string Time(DateTimeOffset? time) => time is { } instant ? LocalTime(instant).ToString("dd.MM.yyyy HH:mm 'UTC'zzz", CultureInfo.InvariantCulture) : "unbekannt";
    /// <summary>Formats an instant as a clock reading relative to the journey day.</summary>
    /// <param name="time">Supplied instant.</param>
    /// <param name="reference">Reference instant of the searched or first planned departure.</param>
    /// <returns>Local clock text with a day marker only when the day differs.</returns>
    public static string EventTime(DateTimeOffset? time, DateTimeOffset? reference)
    {
        if (time is not { } instant) return "unbekannt";
        var local = LocalTime(instant);
        var clock = local.ToString("HH:mm", CultureInfo.InvariantCulture);
        var days = (int)(local.Date - (reference is { } day ? LocalTime(day).Date : local.Date)).TotalDays;
        return days switch { 0 => clock, -1 => "Vortag " + clock, 1 => "Folgetag " + clock, _ => local.ToString("dd.MM.", CultureInfo.InvariantCulture) + " " + clock };
    }
    /// <summary>Removes a literally repeated leading place name for display.</summary>
    /// <param name="name">Original provider stop name.</param>
    /// <returns>The trailing stop name when the part before the first comma is repeated at its start and ends on a word boundary; otherwise the unchanged name, or an empty string for <see langword="null"/>.</returns>
    public static string DisplayName(string? name)
    {
        if (name is null) return string.Empty;
        var comma = name.IndexOf(',', StringComparison.Ordinal);
        if (comma < 0) return name;
        var prefix = name[..comma].Trim();
        var rest = name[(comma + 1)..].Trim();
        if (prefix.Length == 0 || rest.Length == 0) return name;
        if (!rest.StartsWith(prefix, StringComparison.Ordinal)) return name;
        if (rest.Length != prefix.Length && rest[prefix.Length] is not (' ' or '(' or '-' or '/')) return name;
        return rest;
    }
    /// <summary>Formats an endpoint for display without technical identifiers.</summary>
    /// <param name="address">Candidate.</param>
    /// <returns>The cleaned display name plus optional coordinates; never the source, id or shared identifier.</returns>
    public static string DisplayAddress(Address address)
    {
        return DisplayName(address.Name) + CoordinateSuffix(address.Coordinate);
    }
    /// <summary>Formats safe provider provenance.</summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="result">Provider response.</param>
    /// <returns>Source, age and warnings.</returns>
    public static string Metadata<T>(ProviderResult<T> result) => $"Quelle: {result.Source}; Datenstand: {Time(result.RetrievedAt)}; Datenalter: {Math.Max(0, (DateTimeOffset.UtcNow - result.RetrievedAt).TotalMinutes):0} Min." + (result.IsFallback ? " Ersatzquelle (Fallback)." : "") + (result.IsStale ? " Veralteter Cache." : "") + (result.Warnings.Count > 0 ? " Anbieterwarnung: Daten sind möglicherweise unvollständig; eine Quelle konnte keine aktuellen Daten liefern." : "");
    /// <summary>Formats only the actionable freshness and completeness warning for a compact result list.</summary>
    /// <typeparam name="T">Provider item type.</typeparam>
    /// <param name="result">Optional provider response.</param>
    /// <returns>Empty text for a current complete primary response.</returns>
    public static string CompactWarning<T>(ProviderResult<T>? result)
    {
        if (result is null) return "";
        if (result.Warnings.Count > 0) return "Daten möglicherweise unvollständig";
        if (result.IsStale) return "Zwischengespeicherte Daten";
        return result.IsFallback ? "Ersatzquelle" : "";
    }
    /// <summary>Formats the effective line name of a leg through the shared fallback chain.</summary>
    /// <param name="leg">Journey leg.</param>
    /// <returns>The leg, departure or identity line name, or „Linie unbekannt" when none was supplied.</returns>
    public static string LegLineName(JourneyLeg leg)
    {
        return leg.Line?.Name ?? leg.Departure.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt";
    }
    /// <summary>Resolves the effective transport mode of a leg through the shared line fallback.</summary>
    /// <param name="leg">Journey leg.</param>
    /// <returns>The mode of the leg or departure line, or <see langword="null"/> for walking and unknown legs.</returns>
    public static string? LegLineMode(JourneyLeg leg)
    {
        return (leg.Line ?? leg.Departure.Line)?.Mode;
    }
    /// <summary>Formats a compact result.</summary>
    /// <param name="journey">Journey.</param>
    /// <returns>Journey label.</returns>
    public static string Summary(Journey journey)
    {
        var reference = journey.Legs.FirstOrDefault()?.Departure.PlannedTime;
        return $"Soll: {EventTime(journey.Legs.FirstOrDefault()?.Departure.PlannedTime, reference)} → {EventTime(journey.Legs.LastOrDefault()?.Arrival.PlannedTime, reference)}\nIst: {EventTime(journey.Legs.FirstOrDefault()?.Departure.Realtime.ActualTime, reference)} → {EventTime(journey.Legs.LastOrDefault()?.Arrival.Realtime.ActualTime, reference)}\n{journey.Transfers.Count} Umstiege · " + string.Join(" · ", journey.Legs.Select(LegName)) + (journey.Legs.Any(leg => leg.Departure.Realtime.Cancelled == true || leg.Arrival.Realtime.Cancelled == true) ? "\nAusfall gemeldet" : "");
    }
    /// <summary>Formats the entire supplied itinerary.</summary>
    /// <param name="journey">Journey.</param>
    /// <returns>Detailed itinerary.</returns>
    public static string Detail(Journey journey)
    {
        var reference = journey.Legs.FirstOrDefault()?.Departure.PlannedTime;
        return Summary(journey) + "\n\n" + string.Join("\n\n", journey.Legs.Select(leg => LegName(leg) + "\nAbfahrt " + Event(leg.Departure, reference) + "\nAnkunft " + Event(leg.Arrival, reference))) + "\n\n" + string.Join("\n", journey.Transfers.Select(t => $"Umstieg: {t.Stop?.Name ?? "Haltestelle unbekannt"} · {t.Duration?.TotalMinutes.ToString("0", CultureInfo.InvariantCulture) ?? "unbekannt"} Min."));
    }
    private static string CoordinateSuffix(GeoCoordinate? coordinate)
    {
        return coordinate is { } c ? FormattableString.Invariant($" · {c.Latitude:0.#####}, {c.Longitude:0.#####}") : "";
    }
    private static string LegName(JourneyLeg leg) => leg.Walking is { } walk ? $"Fußweg · {walk.DistanceMeters?.ToString("0", CultureInfo.InvariantCulture) ?? "unbekannt"} m · {walk.Duration?.TotalMinutes.ToString("0", CultureInfo.InvariantCulture) ?? "unbekannt"} Min." : $"{LegLineName(leg)} · Betreiber: {leg.Line?.Operator?.Name ?? leg.Departure.Line?.Operator?.Name ?? leg.Departure.Identity.Operator ?? "unbekannt"}";
    private static string Event(StopEvent item, DateTimeOffset? reference) => $"{DisplayName(item.Identity.Stop.Name)}\nSoll: {EventTime(item.PlannedTime, reference)} · Ist: {(item.Realtime.ActualTime is null ? "keine Echtzeitdaten" : EventTime(item.Realtime.ActualTime, reference))}\n" + (item.Realtime.Cancelled switch { true => "Ausfall", false => "Kein Ausfall gemeldet", null => "Ausfallstatus unbekannt" }) + $" · Bahnsteig Soll: {item.Realtime.PlannedPlatform ?? "unbekannt"}, Ist: {item.Realtime.Platform ?? "unbekannt"}" + (string.IsNullOrEmpty(item.Realtime.Source) ? "" : $" · Echtzeitquelle: {item.Realtime.Source}, Stand: {Time(item.Realtime.RetrievedAt)}");
}
