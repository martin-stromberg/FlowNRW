using System.Globalization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Shared textual rendering preserving unknown realtime values.</summary>
public static class JourneyPresentation
{
    /// <summary>Formats an instant with its date and offset.</summary>
    /// <param name="time">Supplied instant.</param>
    /// <returns>Explicit local date and offset.</returns>
    public static string Time(DateTimeOffset? time) => time is { } instant ? TimeZoneInfo.ConvertTime(instant, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")).ToString("dd.MM.yyyy HH:mm 'UTC'zzz", CultureInfo.InvariantCulture) : "unbekannt";
    /// <summary>Formats an instant as a clock reading relative to the journey day.</summary>
    /// <param name="time">Supplied instant.</param>
    /// <param name="reference">Reference instant of the searched or first planned departure.</param>
    /// <returns>Local clock text with a day marker only when the day differs.</returns>
    public static string EventTime(DateTimeOffset? time, DateTimeOffset? reference)
    {
        if (time is not { } instant) return "unbekannt";
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var local = TimeZoneInfo.ConvertTime(instant, zone);
        var clock = local.ToString("HH:mm", CultureInfo.InvariantCulture);
        var days = (int)(local.Date - (reference is { } day ? TimeZoneInfo.ConvertTime(day, zone).Date : local.Date)).TotalDays;
        return days switch { 0 => clock, -1 => "Vortag " + clock, 1 => "Folgetag " + clock, _ => local.ToString("dd.MM.", CultureInfo.InvariantCulture) + " " + clock };
    }
    /// <summary>Formats endpoint identity.</summary>
    /// <param name="address">Candidate.</param>
    /// <returns>Disambiguating label.</returns>
    public static string Address(Address address) => $"{address.Name} · {address.Stop?.Source} {address.Stop?.Id} {address.Stop?.Dhid}".Trim() + (address.Coordinate is { } c ? FormattableString.Invariant($" · {c.Latitude:0.#####}, {c.Longitude:0.#####}") : "");
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
    private static string LegName(JourneyLeg leg) => leg.Walking is { } walk ? $"Fußweg · {walk.DistanceMeters?.ToString("0", CultureInfo.InvariantCulture) ?? "unbekannt"} m · {walk.Duration?.TotalMinutes.ToString("0", CultureInfo.InvariantCulture) ?? "unbekannt"} Min." : $"{leg.Line?.Name ?? leg.Departure.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt"} · Betreiber: {leg.Line?.Operator?.Name ?? leg.Departure.Line?.Operator?.Name ?? leg.Departure.Identity.Operator ?? "unbekannt"}";
    private static string Event(StopEvent item, DateTimeOffset? reference) => $"{item.Identity.Stop.Name}\nSoll: {EventTime(item.PlannedTime, reference)} · Ist: {(item.Realtime.ActualTime is null ? "keine Echtzeitdaten" : EventTime(item.Realtime.ActualTime, reference))}\n" + (item.Realtime.Cancelled switch { true => "Ausfall", false => "Kein Ausfall gemeldet", null => "Ausfallstatus unbekannt" }) + $" · Bahnsteig Soll: {item.Realtime.PlannedPlatform ?? "unbekannt"}, Ist: {item.Realtime.Platform ?? "unbekannt"}" + (string.IsNullOrEmpty(item.Realtime.Source) ? "" : $" · Echtzeitquelle: {item.Realtime.Source}, Stand: {Time(item.Realtime.RetrievedAt)}");
}
