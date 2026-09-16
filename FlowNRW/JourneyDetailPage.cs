using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Native itinerary including unknown and cancelled events.</summary>
public sealed class JourneyDetailPage : ContentPage
{
    /// <summary>Creates the detail page.</summary>
    /// <param name="model">Selected journey projection.</param>
    public JourneyDetailPage(JourneyDetailViewModel model)
    {
        BindingContext = model; Title = "Verbindungsdetails";
        var sections = new VerticalStackLayout { Padding = 16, Spacing = 16, AutomationId = "JourneyDetails" };
        var index = 0;
        foreach (var section in model.Details.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
            sections.Children.Add(new Border { Padding = 16, Stroke = Color.FromArgb("#C7D7EC"), Content = new Label { Text = section, FontSize = 18, AutomationId = "JourneyDetailSection" + index++ } });
        Content = new ScrollView { Content = sections };
    }
}
