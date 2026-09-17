using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Manual search restricted to identifiable public transport stops.</summary>
public sealed class StopSearchPage : ContentPage
{
    private readonly StopMonitorViewModel model;

    /// <summary>Creates the stop lookup view.</summary>
    /// <param name="model">Retained monitor session.</param>
    /// <param name="map">Shared map snapshot.</param>
    public StopSearchPage(StopMonitorViewModel model, FlowNRW.Core.Maps.MapViewModel map)
    {
        this.model = model;
        BindingContext = model;
        Title = "Haltestelle suchen";
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        layout.Children.Add(new Label { Text = "Nächste Abfahrten", FontSize = 28, FontAttributes = FontAttributes.Bold });
        layout.Children.Add(new Label { Text = "Haltestellenname oder Ort" });
        var input = new Entry { AutomationId = "StopQuery", Placeholder = "z. B. Essen Hauptbahnhof" };
        SemanticProperties.SetDescription(input, "Haltestellenname oder Ort");
        input.SetBinding(Entry.TextProperty, "Lookup.Text");
        layout.Children.Add(input);
        layout.Children.Add(new Button { Text = "Haltestellen suchen", AutomationId = "FindStops", Command = model.Lookup.SearchCommand });
        var busy = new ActivityIndicator { AutomationId = "StopSearchBusy" };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, "Lookup.IsBusy");
        layout.Children.Add(busy);
        var status = new Label { AutomationId = "StopSearchStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.SearchStatus));
        layout.Children.Add(status);
        var metadata = new Label { AutomationId = "StopSearchMetadata" };
        metadata.SetBinding(Label.TextProperty, "Lookup.Metadata");
        layout.Children.Add(metadata);
        var showMap = new AsyncRelayCommand(async () => { map.ShowStops(); await Shell.Current.GoToAsync("map"); },
            () => model.Stops.Count > 0 && !model.Lookup.IsBusy, _ => Title = "Karte konnte nicht geöffnet werden");
        model.PropertyChanged += (_, _) => showMap.Refresh();
        layout.Children.Add(new Button { Text = "Haltestellen auf Karte zeigen", AutomationId = "ShowStopMap", Command = showMap, LineBreakMode = LineBreakMode.WordWrap });
        var matches = new VerticalStackLayout { Spacing = 12 };
        void RenderMatches()
        {
            matches.Children.Clear();
            for (var index = 0; index < model.Stops.Count; index++)
            {
                var candidate = model.Stops[index];
                matches.Children.Add(new Button
                {
                    Text = JourneyPresentation.Address(candidate),
                    LineBreakMode = LineBreakMode.WordWrap,
                    AutomationId = "StopMatch" + index,
                    Command = new AsyncRelayCommand(() => model.OpenAsync(candidate), () => !model.IsBusy,
                        _ => Title = "Monitor konnte nicht geöffnet werden")
                });
            }
        }
        model.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(model.Stops)) RenderMatches(); };
        RenderMatches();
        layout.Children.Add(matches);
        Content = new ScrollView { Content = layout };
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        model.Lookup.CancelPending();
    }
}
