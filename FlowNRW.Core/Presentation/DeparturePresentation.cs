using System.Globalization;
using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Readable departure information without invented realtime values.</summary>
public static class DeparturePresentation
{
    /// <summary>Formats a departure including delay, cancellation and platform changes.</summary>
    /// <param name="item">Provider departure.</param>
    /// <returns>Multiline accessible departure text.</returns>
    public static string Describe(StopEvent item)
    {
        var realtime = item.Realtime;
        var delay = realtime.ActualTime is { } actual && item.PlannedTime is { } planned
            ? actual - planned : realtime.Delay;
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
