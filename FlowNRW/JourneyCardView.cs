using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;
using System.Windows.Input;

namespace FlowNRW;

/// <summary>Accessible result card using the provider's actual itinerary and transfer count.</summary>
public sealed class JourneyCardView : Border
{
    private readonly Button open;

    /// <summary>Creates a result card with its detail action.</summary>
    /// <param name="journey">Provider itinerary.</param>
    /// <param name="id">Stable action identifier.</param>
    /// <param name="command">Detail navigation action.</param>
    public JourneyCardView(Journey journey, string id, ICommand command)
    {
        Padding = 16;
        var layout = new VerticalStackLayout { Spacing = 12 };
        var first = journey.Legs.FirstOrDefault()?.Departure;
        var last = journey.Legs.LastOrDefault()?.Arrival;
        layout.Children.Add(TransitVisuals.Text($"{Clock(first?.PlannedTime)} → {Clock(last?.PlannedTime)}", 24, true));
        var duration = first?.PlannedTime is { } start && last?.PlannedTime is { } end && end >= start
            ? $"{(end - start).TotalMinutes:0} Min. · " : "Dauer unbekannt · ";
        layout.Children.Add(TransitVisuals.Text(duration + $"{journey.Transfers.Count} " + (journey.Transfers.Count == 1 ? "Umstieg" : "Umstiege"), 15));
        var badges = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start };
        foreach (var leg in journey.Legs)
        {
            var badge = TransitVisuals.Badge(leg.Walking is not null ? "Fußweg" : leg.Line?.Name ?? leg.Departure.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt", leg.Walking is null ? (leg.Line ?? leg.Departure.Line)?.Mode : null);
            badge.Margin = new Thickness(0, 0, 8, 8); badges.Children.Add(badge);
        }
        layout.Children.Add(badges);
        layout.Children.Add(TransitVisuals.Secondary("Ist: " + Clock(first?.Realtime.ActualTime) + " → " + Clock(last?.Realtime.ActualTime), id + "Realtime"));
        if (first?.PlannedTime is { } date) layout.Children.Add(TransitVisuals.Secondary("Abfahrt: " + TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")).ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture), id + "Date"));
        if (journey.Legs.Any(leg => leg.Departure.Realtime.Cancelled == true || leg.Arrival.Realtime.Cancelled == true))
            layout.Children.Add(TransitVisuals.Text("Ausfall gemeldet", 15, true));
        open = new Button { Text = "Verbindung öffnen", AutomationId = id, Command = command, LineBreakMode = LineBreakMode.WordWrap };
        SemanticProperties.SetDescription(open, JourneyPresentation.Summary(journey));
        layout.Children.Add(open);
        Content = layout;
        TransitVisuals.ApplyRoles(layout);
    }

    /// <summary>Disconnects the command before removing a native result card.</summary>
    public void Detach() => open.Command = null;

    internal static string Clock(DateTimeOffset? instant) => instant is { } value
        ? TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")).ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture) : "Unbekannt";
}
