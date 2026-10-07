using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>Manual search restricted to identifiable public transport stops.</summary>
public sealed class StopSearchPage : ContentPage
{
    private readonly StopMonitorViewModel model;
    private readonly ForegroundState foreground;

    /// <summary>Creates the stop lookup view.</summary>
    /// <param name="model">Retained monitor session.</param>
    /// <param name="map">Shared map snapshot.</param>
    /// <param name="foreground">Shared active-window state.</param>
    public StopSearchPage(StopMonitorViewModel model, FlowNRW.Core.Maps.MapViewModel map, ForegroundState foreground)
    {
        this.foreground = foreground;
        this.model = model;
        BindingContext = model;
        Title = "Haltestelle suchen";
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") == "1") return;
            if (layout.Children.Any(child => child.AutomationId == "NearbyScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "NearbyScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario));
            layout.Children.Insert(0, scenario);
        };
#endif
        layout.Children.Add(TransitVisuals.Text("Haltestellen", 32, true));
        layout.Children.Add(TransitVisuals.Secondary("Suche eine Station oder finde Haltestellen in deiner Nähe."));
        var searchCard = new VerticalStackLayout { Spacing = 12 };
        layout.Children.Add(new Border { Padding = 16, Content = searchCard });
        searchCard.Children.Add(TransitVisuals.Text("Haltestelle finden", 20, true));
        var input = new Entry { AutomationId = "StopQuery", Placeholder = "z. B. Essen Hauptbahnhof" };
        SemanticProperties.SetDescription(input, "Haltestellenname oder Ort");
        input.SetBinding(Entry.TextProperty, "Lookup.Text");
        var showMap = new AsyncRelayCommand(async () => { map.ShowStops(); await Shell.Current.GoToAsync("map"); },
            () => model.Stops.Count > 0 && !model.Lookup.IsBusy && !model.IsNearbyBusy, _ => Title = "Karte konnte nicht geöffnet werden");
        model.PropertyChanged += (_, _) => showMap.Refresh();
        var find = new Button { Text = "⌕", AutomationId = "FindStops", Command = model.Lookup.SearchCommand, HeightRequest = 48, WidthRequest = 48, Padding = new Thickness(0) };
        SemanticProperties.SetDescription(find, "Haltestelle suchen");
        var nearby = new Button { Text = "⌖", AutomationId = "FindNearbyStops", Command = model.NearbyCommand, HeightRequest = 48, WidthRequest = 48, Padding = new Thickness(0) };
        SemanticProperties.SetDescription(nearby, "Haltestellen in meiner Nähe");
        var showMapButton = TransitVisuals.SecondaryAction("⌖", "Haltestellen auf Karte zeigen", "ShowStopMap", showMap);
        var inputRow = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto)], ColumnSpacing = 8 };
        inputRow.Add(input, 0); inputRow.Add(find, 1); inputRow.Add(nearby, 2); inputRow.Add(showMapButton, 3); searchCard.Children.Add(inputRow);
        var nearbyStatus = new Label { AutomationId = "NearbyStatus" }; nearbyStatus.SetBinding(Label.TextProperty, nameof(model.NearbyStatus)); layout.Children.Add(nearbyStatus);
        var nearbyBusy = new ActivityIndicator { AutomationId = "NearbyBusy" };
        nearbyBusy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(model.IsNearbyBusy)); nearbyBusy.SetBinding(IsVisibleProperty, nameof(model.IsNearbyBusy)); layout.Children.Add(nearbyBusy);
        var busy = new ActivityIndicator { AutomationId = "StopSearchBusy" };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, "Lookup.IsBusy");
        busy.SetBinding(IsVisibleProperty, "Lookup.IsBusy");
        layout.Children.Add(busy);
        var status = new Label { AutomationId = "StopSearchStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.SearchStatus));
        layout.Children.Add(status);
        var metadata = new Label { AutomationId = "StopSearchMetadata" };
        layout.Children.Add(metadata);
        var matches = new VerticalStackLayout { Spacing = 12 };
        void RenderMatches()
        {
            matches.Children.Clear();
            for (var index = 0; index < model.Stops.Count; index++)
            {
                var candidate = model.Stops[index];
                matches.Children.Add(TransitVisuals.Candidate(candidate, new Button
                {
                    Text = JourneyPresentation.Address(candidate) + model.DistanceLabel(candidate),
                    LineBreakMode = LineBreakMode.WordWrap,
                    AutomationId = "StopMatch" + index,
                    Command = new AsyncRelayCommand(() => model.OpenAsync(candidate), () => !model.IsBusy,
                        _ => Title = "Monitor konnte nicht geöffnet werden")
                }, model.DistanceLabel(candidate), showTechnicalIdentity: false));
            }
        }
        void RefreshInformation()
        {
            nearbyStatus.IsVisible = ShowsNearbyStatus();
            status.IsVisible = ShowsSearchStatus();
            metadata.Text = SearchWarning();
            metadata.IsVisible = !string.IsNullOrWhiteSpace(metadata.Text);
        }
        model.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(model.Stops)) RenderMatches();
            RefreshInformation();
        };
        RenderMatches();
        RefreshInformation();
        layout.Children.Add(matches);
        Content = TransitVisuals.Page(layout);
    }

    private bool ShowsNearbyStatus()
    {
        return model.IsNearbyBusy || model.IsNearbyFailure || model.NearbyResult is { Items.Count: 0 };
    }

    private bool ShowsSearchStatus()
    {
        if (model.IsNearbyBusy || model.NearbyResult is not null
            || string.Equals(model.SearchStatus, model.NearbyStatus, StringComparison.Ordinal)) return false;
        var value = model.SearchStatus;
        return !string.IsNullOrWhiteSpace(value)
            && !value.StartsWith("Adresse oder Haltestelle suchen", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("Bitte einen Treffer auswählen", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("Standort nur nach Aktion", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("Endpunkt übernommen", StringComparison.OrdinalIgnoreCase);
    }

    private string SearchWarning()
    {
        return model.NearbyResult is { } nearby
            ? JourneyPresentation.CompactWarning(nearby)
            : JourneyPresentation.CompactWarning(model.Lookup.Result);
    }

    /// <inheritdoc />
    protected override void OnAppearing() { base.OnAppearing(); foreground.PropertyChanged += ForegroundChanged; }

    private void ForegroundChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (!foreground.IsActive) { model.Lookup.CancelPending(); model.CancelNearbyPending(); }
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        foreground.PropertyChanged -= ForegroundChanged;
        base.OnDisappearing();
        model.Lookup.CancelPending();
        model.CancelNearbyPending();
    }
}
