using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW;

/// <summary>Native itinerary timeline built only from provider-supplied journey legs.</summary>
public sealed class JourneyTimelineView : VerticalStackLayout
{
    /// <summary>Creates accessible timeline cards for the supplied itinerary.</summary>
    /// <param name="journey">The selected provider journey.</param>
    public JourneyTimelineView(Journey journey)
    {
        Spacing = 16;
        var descriptions = JourneyPresentation.Detail(journey).Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var summary = TransitVisuals.Text("Deine Verbindung", 22, true, "JourneyDetailSection0");
        SemanticProperties.SetDescription(summary, JourneyPresentation.Summary(journey));
        var first = journey.Legs.FirstOrDefault()?.Departure.PlannedTime;
        var last = journey.Legs.LastOrDefault()?.Arrival.PlannedTime;
        var summaryCard = new VerticalStackLayout { Spacing = 6 };
        summaryCard.Children.Add(summary);
        summaryCard.Children.Add(TransitVisuals.Text(JourneyCardView.Clock(first) + " → " + JourneyCardView.Clock(last), 28, true));
        var duration = first is { } start && last is { } end && end >= start ? (end - start).TotalMinutes.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " Min. · " : "Dauer unbekannt · ";
        summaryCard.Children.Add(TransitVisuals.Secondary(duration + journey.Transfers.Count + (journey.Transfers.Count == 1 ? " Umstieg" : " Umstiege")));
        Children.Add(new Border { Padding = 16, Content = summaryCard });
        var index = 1;
        foreach (var leg in journey.Legs)
        {
            var title = leg.Walking is not null ? "Fußweg" : leg.Line?.Name ?? leg.Departure.Line?.Name ?? leg.Departure.Identity.Line ?? "Linie unbekannt";
            var origin = string.IsNullOrWhiteSpace(leg.Departure.Identity.Stop.Name) ? "Start unbekannt" : leg.Departure.Identity.Stop.Name;
            var destination = string.IsNullOrWhiteSpace(leg.Arrival.Identity.Stop.Name) ? "Ziel unbekannt" : leg.Arrival.Identity.Stop.Name;
            var heading = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 12 };
            var badge = TransitVisuals.Badge(title, leg.Walking is null ? (leg.Line ?? leg.Departure.Line)?.Mode : null);
            if (badge.Content is Label lineLabel) lineLabel.AutomationId = "JourneyLine" + index;
            heading.Add(badge, 0);
            var names = TransitVisuals.Text(origin + " → " + destination, 17, true, "JourneyDetailSection" + index);
            if (index < descriptions.Length) SemanticProperties.SetDescription(names, descriptions[index]);
            heading.Add(names, 1);
            var content = new VerticalStackLayout { Spacing = 10 };
            content.Children.Add(heading);
            content.Children.Add(Event("Abfahrt", leg.Departure));
            content.Children.Add(Event("Ankunft", leg.Arrival));
            if (leg.Walking is { } walk)
                content.Children.Add(TransitVisuals.Secondary("Fußweg · " + (walk.DistanceMeters?.ToString("0", System.Globalization.CultureInfo.InvariantCulture) ?? "unbekannt") + " m · " + (walk.Duration?.TotalMinutes.ToString("0", System.Globalization.CultureInfo.InvariantCulture) ?? "unbekannt") + " Min.", "JourneyWalk" + index));
            else
                content.Children.Add(TransitVisuals.Secondary("Betreiber: " + (leg.Line?.Operator?.Name ?? leg.Departure.Line?.Operator?.Name ?? leg.Departure.Identity.Operator ?? "unbekannt"), "JourneyOperator" + index));
            content.Children.Add(TransitVisuals.Secondary((leg.Geometry?.Coordinates.Count ?? leg.Walking?.Geometry?.Coordinates.Count ?? 0) > 1 ? "Verlauf auf der Karte verfügbar" : "Kein gelieferter Kartenverlauf"));
            Children.Add(new Border { Padding = 16, AutomationId = "JourneyTimeline" + index++, Content = content });
        }
        if (journey.Transfers.Count > 0)
            Children.Add(new Border { Padding = 16, Content = TransitVisuals.Text(string.Join("\n", journey.Transfers.Select(transfer => "Umstieg: " + (string.IsNullOrWhiteSpace(transfer.Stop?.Name) ? "Haltestelle unbekannt" : transfer.Stop.Name) + " · " + (transfer.Duration?.TotalMinutes.ToString("0", System.Globalization.CultureInfo.InvariantCulture) ?? "unbekannt") + " Min.")), id: "JourneyDetailSection" + index) });
    }

    private static VerticalStackLayout Event(string title, StopEvent item)
    {
        var stack = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(4, 0) };
        var times = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)], ColumnSpacing = 10 };
        times.Add(TransitVisuals.Text(title, 15, true), 0);
        times.Add(TransitVisuals.Secondary("Soll " + JourneyPresentation.Time(item.PlannedTime)), 1);
        var actualTime = TransitVisuals.Text(item.Realtime.ActualTime is null ? "keine Echtzeit" : "Ist " + JourneyPresentation.Time(item.Realtime.ActualTime), 15, true);
        times.Add(actualTime, 2);
        stack.Children.Add(times);
        var delay = item.Realtime.ActualTime is { } actual && item.PlannedTime is { } planned ? actual - planned : item.Realtime.Delay;
        stack.Children.Add(TransitVisuals.Secondary(delay is { } difference ? "Abweichung " + difference.TotalMinutes.ToString("+0;-0;0", System.Globalization.CultureInfo.InvariantCulture) + " Min." : "Verspätung unbekannt"));
        stack.Children.Add(TransitVisuals.Text(item.Realtime.Cancelled switch { true => "Fahrt fällt aus", false => "Kein Ausfall gemeldet", null => "Ausfallstatus unbekannt" }, 15, true));
        stack.Children.Add(TransitVisuals.Secondary("Bahnsteig Soll: " + (item.Realtime.PlannedPlatform ?? "unbekannt") + " · Ist: " + (item.Realtime.Platform ?? "unbekannt")));
        if (!string.IsNullOrWhiteSpace(item.Realtime.Source)) stack.Children.Add(TransitVisuals.Secondary("Echtzeitquelle: " + item.Realtime.Source + " · Stand: " + JourneyPresentation.Time(item.Realtime.RetrievedAt)));
        return stack;
    }
}
