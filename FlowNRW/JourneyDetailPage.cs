using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Native itinerary including unknown and cancelled events.</summary>
public sealed class JourneyDetailPage : ContentPage
{
    /// <summary>Creates the detail page.</summary>
    /// <param name="model">Selected journey projection.</param>
    /// <param name="map">Shared map snapshot.</param>
    public JourneyDetailPage(JourneyDetailViewModel model, FlowNRW.Core.Maps.MapViewModel map)
    {
        BindingContext = model; Title = "Verbindungsdetails";
        var sections = new VerticalStackLayout { Padding = 16, Spacing = 16, AutomationId = "JourneyDetails" };
        sections.Children.Add(new Button
        {
            Text = "Verlauf auf Karte",
            AutomationId = "ShowJourneyMap",
            Command = new AsyncRelayCommand(async () => { if (model.Session.SelectedJourney is { } journey) { map.ShowJourney(journey, model.Session.Metadata); await Shell.Current.GoToAsync("map"); } },
                () => model.Session.SelectedJourney is not null, _ => Title = "Karte konnte nicht geöffnet werden")
        });
        var index = 0;
        foreach (var section in model.Details.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
            sections.Children.Add(new Border { Padding = 16, Stroke = Color.FromArgb("#C7D7EC"), Content = new Label { Text = section, FontSize = 18, AutomationId = "JourneyDetailSection" + index++ } });
        Content = new ScrollView { Content = sections };
    }
}
