using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Manual endpoint input and connection search.</summary>
public sealed class SearchPage : ContentPage
{
    private readonly JourneySearchViewModel model;
    /// <summary>Creates the native search view.</summary>
    /// <param name="model">Retained search session.</param>
    public SearchPage(JourneySearchViewModel model)
    {
        this.model = model; BindingContext = model; Title = "Verbindung suchen";
        var content = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        content.Children.Add(new Label { Text = "FlowNRW · Deine Verbindung", FontSize = 28, FontAttributes = FontAttributes.Bold });
        content.Children.Add(new Label { Text = "Start und Ziel wählen: Adresse, Haltestelle, Koordinate oder aktueller Standort." });
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (content.Children.Any(child => child.AutomationId == "LocationScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "LocationScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario));
            var calls = new Label { AutomationId = "LocationCalls", BindingContext = fixture };
            calls.SetBinding(Label.TextProperty, nameof(fixture.Calls));
            content.Children.Insert(0, calls);
            content.Children.Insert(0, scenario);
        };
#endif
        content.Children.Add(Endpoint(model.Origin, "Origin", "Start"));
        content.Children.Add(Endpoint(model.Destination, "Destination", "Ziel"));
        var search = new Button { Text = "Verbindungen suchen", AutomationId = "SearchJourneys", Command = model.SearchCommand };
        content.Children.Add(search);
        var busy = new ActivityIndicator { AutomationId = "RoutingBusy" }; busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(model.IsBusy)); content.Children.Add(busy);
        var status = new Label { AutomationId = "RoutingStatus" }; status.SetBinding(Label.TextProperty, nameof(model.Status)); content.Children.Add(status);
        var metadata = new Label { AutomationId = "RoutingMetadata" }; metadata.SetBinding(Label.TextProperty, nameof(model.Metadata)); content.Children.Add(metadata);
        Content = new ScrollView { Content = content };
    }
    /// <inheritdoc />
    protected override void OnDisappearing() { base.OnDisappearing(); model.Origin.CancelPending(); model.Destination.CancelPending(); model.CancelPending(); }
    private static Border Endpoint(EndpointViewModel endpoint, string prefix, string title)
    {
        var layout = new VerticalStackLayout { Spacing = 8, BindingContext = endpoint };
        layout.Children.Add(new Label { Text = title, FontSize = 22, FontAttributes = FontAttributes.Bold });
        var toggle = new Switch { AutomationId = prefix + "CoordinateMode" }; toggle.SetBinding(Switch.IsToggledProperty, nameof(endpoint.IsCoordinateMode));
        SemanticProperties.SetDescription(toggle, title + ": Koordinatenmodus");
        layout.Children.Add(new Label { Text = "Koordinaten eingeben" }); layout.Children.Add(toggle);
        var textLabel = new Label { Text = title + ": Adresse oder Haltestelle" }; textLabel.SetBinding(IsVisibleProperty, nameof(endpoint.IsTextMode)); layout.Children.Add(textLabel);
        var text = new Entry { Placeholder = title + ": Adresse oder Haltestelle", AutomationId = prefix + "Text" }; text.SetBinding(Entry.TextProperty, nameof(endpoint.Text)); text.SetBinding(IsVisibleProperty, nameof(endpoint.IsTextMode)); layout.Children.Add(text);
        SemanticProperties.SetDescription(text, title + ": Adresse oder Haltestelle");
        var coordinates = new VerticalStackLayout { Spacing = 8 }; coordinates.SetBinding(IsVisibleProperty, nameof(endpoint.IsCoordinateMode));
        coordinates.Children.Add(new Label { Text = "Breite (−90 bis 90)" });
        var latitude = new Entry { Placeholder = "z. B. 51,4556", AutomationId = prefix + "Latitude", Keyboard = Keyboard.Numeric }; latitude.SetBinding(Entry.TextProperty, nameof(endpoint.Latitude)); coordinates.Children.Add(latitude);
        coordinates.Children.Add(new Label { Text = "Länge (−180 bis 180)" });
        var longitude = new Entry { Placeholder = "z. B. 7,0116", AutomationId = prefix + "Longitude", Keyboard = Keyboard.Numeric }; longitude.SetBinding(Entry.TextProperty, nameof(endpoint.Longitude)); coordinates.Children.Add(longitude); layout.Children.Add(coordinates);
        layout.Children.Add(new Button { Text = "Suchen / Koordinate übernehmen", AutomationId = prefix + "Search", Command = endpoint.SearchCommand });
        layout.Children.Add(new Button { Text = "Aktuellen Standort verwenden", AutomationId = prefix + "Location", Command = endpoint.LocationCommand, LineBreakMode = LineBreakMode.WordWrap });
        var locationStatus = new Label { AutomationId = prefix + "LocationStatus" }; locationStatus.SetBinding(Label.TextProperty, nameof(endpoint.LocationStatus)); layout.Children.Add(locationStatus);
        var busy = new ActivityIndicator { AutomationId = prefix + "Busy" }; busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(endpoint.IsBusy)); layout.Children.Add(busy);
        var status = new Label { AutomationId = prefix + "Status" }; status.SetBinding(Label.TextProperty, nameof(endpoint.Status)); layout.Children.Add(status);
        var matches = new VerticalStackLayout { Spacing = 8, AutomationId = prefix + "Matches" }; layout.Children.Add(matches);
        endpoint.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(endpoint.Matches)) return;
            matches.Children.Clear();
            for (var i = 0; i < endpoint.Matches.Count; i++)
            {
                var candidate = endpoint.Matches[i];
                matches.Children.Add(new Button { Text = JourneyPresentation.Address(candidate), LineBreakMode = LineBreakMode.WordWrap, AutomationId = prefix + "Match" + i, Command = endpoint.SelectCommand, CommandParameter = candidate });
            }
        };
        var selection = new Label { AutomationId = prefix + "Selection", FontAttributes = FontAttributes.Bold }; selection.SetBinding(Label.TextProperty, nameof(endpoint.Selection)); layout.Children.Add(selection);
        var metadata = new Label { AutomationId = prefix + "Metadata" }; metadata.SetBinding(Label.TextProperty, nameof(endpoint.Metadata)); layout.Children.Add(metadata);
        return new Border { Padding = 16, Stroke = Color.FromArgb("#C7D7EC"), Content = layout };
    }
}
