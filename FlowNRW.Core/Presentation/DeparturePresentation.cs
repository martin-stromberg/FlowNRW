using System.Globalization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Readable departure information without invented realtime values.</summary>
public static class DeparturePresentation
{
    /// <summary>Chooses a white-text badge color from a supplied mode or an unambiguous rail line name.</summary>
    /// <param name="line">Provider line name.</param>
    /// <param name="mode">Optional provider transport category.</param>
    /// <returns>An opaque hexadecimal background with normal-text contrast of at least 4.5:1.</returns>
    public static string BadgeColor(string line, string? mode)
    {
        var supplied = mode?.Trim().ToLowerInvariant();
        var color = supplied switch
        {
            "rail" or "train" or "regional" or "regionalbahn" or "regionalexpress" => "#BA1B1D",
            "suburban" or "suburbanrail" or "s-bahn" => "#007A39",
            "subway" or "metro" or "underground" or "u-bahn" => "#005A9C",
            "tram" or "streetcar" or "tramway" => "#B33F00",
            "bus" or "coach" => "#6F2C91",
            "ferry" or "boat" or "ship" => "#00777F",
            _ => null
        };
        if (color is not null) return color;
        var name = line.Trim().ToUpperInvariant();
        static bool NumberAfter(string value, string prefix)
            => value.StartsWith(prefix, StringComparison.Ordinal)
                && value[prefix.Length..].TrimStart() is { Length: > 0 } suffix
                && char.IsAsciiDigit(suffix[0]);
        if (NumberAfter(name, "RE") || NumberAfter(name, "RB")) return "#BA1B1D";
        if (NumberAfter(name, "S")) return "#007A39";
        if (NumberAfter(name, "U")) return "#005A9C";
        return "#414755";
    }

    /// <summary>Derives the display status of a departure from supplied realtime fields.</summary>
    /// <param name="item">Provider departure.</param>
    /// <returns>
    /// <see cref="DepartureStatus.Cancelled"/> for reported cancellations, <see cref="DepartureStatus.Scheduled"/>
    /// when no realtime time exists, <see cref="DepartureStatus.Delayed"/> when the actual time deviates
    /// from the plan or a positive delay is reported, otherwise <see cref="DepartureStatus.OnTime"/>.
    /// </returns>
    public static DepartureStatus Status(StopEvent item)
    {
        var realtime = item.Realtime;
        if (realtime.Cancelled == true) return DepartureStatus.Cancelled;
        if (realtime.ActualTime is not { } actual) return DepartureStatus.Scheduled;
        if (item.PlannedTime is { } planned && actual != planned) return DepartureStatus.Delayed;
        if (realtime.Delay is { } delay && delay > TimeSpan.Zero) return DepartureStatus.Delayed;
        return DepartureStatus.OnTime;
    }

    /// <summary>Derives the effective deviation: the actual/planned difference when both exist, else the reported delay.</summary>
    /// <param name="item">Provider departure.</param>
    /// <returns>The effective delay or <see langword="null"/> when realtime supplies neither.</returns>
    private static TimeSpan? EffectiveDelay(StopEvent item)
    {
        var realtime = item.Realtime;
        return realtime.ActualTime is { } actual && item.PlannedTime is { } planned
            ? actual - planned : realtime.Delay;
    }

    /// <summary>Formats a positive delay as compact text; nothing is invented for other states.</summary>
    /// <param name="item">Provider departure.</param>
    /// <returns>„+n Min." when realtime reports a real positive delay, otherwise <see langword="null"/>.</returns>
    public static string? DelayText(StopEvent item)
    {
        var realtime = item.Realtime;
        if (realtime.Cancelled == true) return null;
        var delay = EffectiveDelay(item);
        return delay is { } value && value > TimeSpan.Zero
            ? value.TotalMinutes.ToString("+0", CultureInfo.InvariantCulture) + " Min."
            : null;
    }

    /// <summary>Formats a departure including delay, cancellation and platform changes.</summary>
    /// <param name="item">Provider departure.</param>
    /// <returns>Multiline accessible departure text.</returns>
    public static string Describe(StopEvent item)
    {
        var realtime = item.Realtime;
        var delay = EffectiveDelay(item);
        var delayText = delay is null ? "Verspätung unbekannt" : delay.Value == TimeSpan.Zero
            ? "Pünktlich gemeldet" : "Abweichung: " + delay.Value.TotalMinutes.ToString("+0;-0;0", CultureInfo.InvariantCulture) + " Min.";
        var cancellation = realtime.Cancelled switch
        {
            true => "FÄLLT AUS",
            false => "Kein Ausfall gemeldet",
            null => "Ausfallstatus unbekannt"
        };
        var platformChange = !string.IsNullOrWhiteSpace(realtime.Platform) && !string.IsNullOrWhiteSpace(realtime.PlannedPlatform)
            && realtime.Platform != realtime.PlannedPlatform ? " · Gleis-/Steigwechsel" : "";
        return $"{item.Line?.Name ?? item.Identity.Line ?? "Linie unbekannt"} → {item.Identity.Direction ?? "Ziel unbekannt"}\n"
            + cancellation + "\nSoll: " + JourneyPresentation.Time(item.PlannedTime)
            + "\nIst: " + (realtime.ActualTime is null ? "keine Echtzeitdaten" : JourneyPresentation.Time(realtime.ActualTime))
            + " · " + delayText
            + $"\nGleis/Steig Soll: {realtime.PlannedPlatform ?? "unbekannt"} · Aktuell: {realtime.Platform ?? "unbekannt"}" + platformChange
            + $"\nBetreiber: {item.Line?.Operator?.Name ?? item.Identity.Operator ?? "unbekannt"}";
    }
}
