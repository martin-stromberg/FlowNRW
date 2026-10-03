using System.Globalization;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW;

/// <summary>A native departure card separating line, destination, times and operational status.</summary>
public sealed class DepartureCardView : Border
{
    /// <summary>Creates a flexible card from supplied departure data without inventing realtime values.</summary>
    /// <param name="item">The retained provider event.</param>
    /// <param name="id">Stable accessible identifier for this displayed departure.</param>
    /// <param name="compact">Whether the containing favorite card provides access to the full monitor.</param>
    public DepartureCardView(StopEvent item, string id, bool compact = false)
    {
        Padding = compact ? 12 : 16;
        var line = item.Line?.Name ?? item.Identity.Line ?? "Linie unbekannt";
        var layout = new VerticalStackLayout { Spacing = 8 };
        var heading = new Grid { ColumnDefinitions = [new(new GridLength(76)), new(GridLength.Star)], ColumnSpacing = 12 };
        heading.Add(TransitVisuals.Badge(line, item.Line?.Mode), 0);
        var destination = TransitVisuals.Text(item.Identity.Direction ?? "Ziel unbekannt", 17, true, id);
        SemanticProperties.SetDescription(destination, DeparturePresentation.Describe(item));
        heading.Add(destination, 1);
        layout.Children.Add(heading);

        var realtime = item.Realtime;
        var times = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 12 };
        times.Add(TimeColumn("Soll", item.PlannedTime, false), 0);
        times.Add(TimeColumn("Ist", realtime.ActualTime, realtime.Cancelled == true), 1);
        if (!compact) layout.Children.Add(times);
        else
        {
            static string Clock(DateTimeOffset? value) => value is { } time
                ? TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")).ToString("HH:mm", CultureInfo.InvariantCulture) : "unbekannt";
            var timing = TransitVisuals.Text("Soll " + Clock(item.PlannedTime) + " · Ist " + Clock(realtime.ActualTime), 18, true);
            timing.FontFamily = DeviceInfo.Platform == DevicePlatform.WinUI ? "Consolas" : "Menlo";
            layout.Children.Add(timing);
        }
        var delay = realtime.ActualTime is { } actual && item.PlannedTime is { } planned ? actual - planned : realtime.Delay;
        var status = realtime.Cancelled == true ? "Fahrt fällt aus"
            : delay is null ? "Verspätung unbekannt" : delay == TimeSpan.Zero ? "Pünktlich gemeldet"
            : "Abweichung " + delay.Value.TotalMinutes.ToString("+0;-0;0", CultureInfo.InvariantCulture) + " Min.";
        var statusLabel = TransitVisuals.Text(status, 15, true);
        if (realtime.Cancelled == true)
            statusLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#BA1A1A"), Color.FromArgb("#FFB4AB"));
        else if (delay is { } difference && difference != TimeSpan.Zero)
            statusLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#894D00"), Color.FromArgb("#FFB874"));
        else if (delay == TimeSpan.Zero)
            statusLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#006E28"), Color.FromArgb("#72FE88"));
        layout.Children.Add(statusLabel);
        var platform = "Gleis/Steig: " + (realtime.Platform ?? "unbekannt") + " · geplant " + (realtime.PlannedPlatform ?? "unbekannt");
        if (!string.IsNullOrWhiteSpace(realtime.Platform) && !string.IsNullOrWhiteSpace(realtime.PlannedPlatform) && realtime.Platform != realtime.PlannedPlatform)
            platform += " · Gleis-/Steigwechsel";
        layout.Children.Add(TransitVisuals.Secondary(platform));
        if (compact && item.PlannedTime is { } instant)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            var date = TimeZoneInfo.ConvertTime(instant, zone);
            if (date.Date != TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).Date)
                layout.Children.Add(TransitVisuals.Secondary(date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));
        }
        if (!compact)
        {
            layout.Children.Add(TransitVisuals.Secondary(realtime.Cancelled switch { true => "FÄLLT AUS", false => "Kein Ausfall gemeldet", null => "Ausfallstatus unbekannt" }));
            layout.Children.Add(TransitVisuals.Secondary("Betreiber: " + (item.Line?.Operator?.Name ?? item.Identity.Operator ?? "unbekannt")));
        }
        Content = layout;
    }

    private static VerticalStackLayout TimeColumn(string title, DateTimeOffset? instant, bool cancelled)
    {
        var local = instant is { } value ? TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")) : (DateTimeOffset?)null;
        var time = TransitVisuals.Text(local?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? (title == "Ist" ? "Keine Echtzeit" : "Unbekannt"), 18, true);
        if (local is not null) time.FontFamily = DeviceInfo.Platform == DevicePlatform.WinUI ? "Consolas" : "Menlo";
        if (cancelled && local is not null) time.TextDecorations = TextDecorations.Strikethrough;
        var column = new VerticalStackLayout { Spacing = 0 };
        column.Children.Add(TransitVisuals.Secondary(title));
        column.Children.Add(time);
        if (local is { } date) column.Children.Add(TransitVisuals.Secondary(date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));
        return column;
    }
}
