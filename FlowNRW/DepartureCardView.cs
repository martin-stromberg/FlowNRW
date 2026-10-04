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
        var layout = new VerticalStackLayout { Spacing = 10 };
        // Keep the destination on a full row. At large text sizes, placing it
        // between a line badge and time made otherwise short destinations wrap
        // into several narrow fragments.
        var heading = new Grid
        {
            RowDefinitions = [new(GridLength.Auto), new(GridLength.Auto)],
            ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)],
            ColumnSpacing = 12,
            RowSpacing = 4
        };
        heading.Add(TransitVisuals.Badge(line, item.Line?.Mode), 0);
        var destination = TransitVisuals.Text(item.Identity.Direction ?? "Ziel unbekannt", 17, true, id);
        SemanticProperties.SetDescription(destination, DeparturePresentation.Describe(item));
        heading.Add(destination, 0, 1);
        Grid.SetColumnSpan(destination, 2);
        var realtime = item.Realtime;
        var time = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.End };
        var prominentTime = TransitVisuals.Text(Clock(realtime.ActualTime ?? item.PlannedTime), 22, true);
        prominentTime.FontFamily = DeviceInfo.Platform == DevicePlatform.WinUI ? "Consolas" : "Menlo";
        if (realtime.Cancelled == true) prominentTime.TextDecorations = TextDecorations.Strikethrough;
        time.Children.Add(prominentTime);
        if (realtime.ActualTime is { } actual && item.PlannedTime is { } planned && actual != planned)
        {
            var scheduled = TransitVisuals.Secondary(Clock(planned));
            scheduled.FontSize = 13;
            scheduled.TextDecorations = TextDecorations.Strikethrough;
            time.Children.Add(scheduled);
        }
        heading.Add(time, 1);
        layout.Children.Add(heading);
        if (realtime.Cancelled == true) layout.Children.Add(TransitVisuals.Text("Fahrt fällt aus", 15, true));
        if (!string.IsNullOrWhiteSpace(realtime.Platform))
        {
            var platform = "Gleis/Steig: " + realtime.Platform;
            if (!string.IsNullOrWhiteSpace(realtime.PlannedPlatform) && realtime.Platform != realtime.PlannedPlatform)
                platform += " · geplant " + realtime.PlannedPlatform;
            layout.Children.Add(TransitVisuals.Secondary(platform));
        }
        if (compact && item.PlannedTime is { } instant)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            var date = TimeZoneInfo.ConvertTime(instant, zone);
            if (date.Date != TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).Date)
                layout.Children.Add(TransitVisuals.Secondary(date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));
        }
        Content = layout;
    }

    private static string Clock(DateTimeOffset? instant)
    {
        return instant is { } value
            ? TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")).ToString("HH:mm", CultureInfo.InvariantCulture)
            : "unbekannt";
    }
}
