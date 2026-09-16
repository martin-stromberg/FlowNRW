using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Provider-ordered native result list.</summary>
public sealed class ResultsPage : ContentPage
{
    private readonly ResultsViewModel model;
    private readonly VerticalStackLayout journeys = new() { Spacing = 16 };
    /// <summary>Creates the result view.</summary>
    /// <param name="model">Session projection.</param>
    public ResultsPage(ResultsViewModel model)
    {
        this.model = model; BindingContext = model.Session; Title = "Verbindungen";
        var layout = new VerticalStackLayout { Spacing = 16, Padding = 16 };
        var status = new Label { FontSize = 24, AutomationId = "ResultsStatus" }; status.SetBinding(Label.TextProperty, nameof(model.Session.Status)); layout.Children.Add(status);
        var metadata = new Label { AutomationId = "ResultsMetadata" }; metadata.SetBinding(Label.TextProperty, nameof(model.Session.Metadata)); layout.Children.Add(metadata);
        layout.Children.Add(journeys); Content = new ScrollView { Content = layout };
    }
    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing(); journeys.Children.Clear();
        for (var i = 0; i < model.Session.Journeys.Count; i++)
        {
            var journey = model.Session.Journeys[i];
            var command = new AsyncRelayCommand(() => model.OpenJourneyAsync(journey), () => true, _ => { Title = "Details konnten nicht geöffnet werden"; });
            journeys.Children.Add(new Button { Text = JourneyPresentation.Summary(journey), LineBreakMode = LineBreakMode.WordWrap, AutomationId = "Journey" + i, Command = command });
        }
    }
}
