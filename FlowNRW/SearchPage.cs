using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using FlowNRW.Core.Favorites;
using FlowNRW.Core.Transit;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>Manual endpoint input and connection search.</summary>
public sealed class SearchPage : ContentPage
{
    private readonly JourneySearchViewModel model;
    private readonly ForegroundState foreground;
    private readonly FavoriteHomeViewModel favorites;
    private readonly ConnectionFavoritesViewModel connectionFavorites;
    /// <summary>Creates the native search view.</summary>
    /// <param name="model">Retained search session.</param>
    /// <param name="foreground">Shared active-window state.</param>
    /// <param name="favorites">Persisted favorite stops used for suggestions.</param>
    /// <param name="connectionFavorites">Explicitly saved reusable journey endpoint pairs.</param>
    public SearchPage(JourneySearchViewModel model, ForegroundState foreground, FavoriteHomeViewModel favorites,
        ConnectionFavoritesViewModel connectionFavorites)
    {
        this.foreground = foreground;
        this.favorites = favorites;
        this.connectionFavorites = connectionFavorites;
        this.model = model; BindingContext = model; Title = "Verbindung suchen";
        var content = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        content.Children.Add(TransitVisuals.Text("Verbindungen", 32, true));
        content.Children.Add(TransitVisuals.Secondary("Plane deine Fahrt mit Start, Ziel und Abfahrtszeit."));
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") == "1") return;
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
        var searchForm = new VerticalStackLayout { Spacing = 12, Padding = 16 };
        searchForm.Children.Add(TransitVisuals.Text("Wohin möchtest du fahren?", 20, true));
        searchForm.Children.Add(Endpoint(model.Origin, "Origin", "Start"));
        var swap = TransitVisuals.SecondaryAction("⇄", "Start und Ziel vertauschen", "SwapEndpoints",
            new AsyncRelayCommand(() => { model.SwapEndpoints(); return Task.CompletedTask; }, () => true,
                _ => Title = "Start und Ziel konnten nicht vertauscht werden"));
        searchForm.Children.Add(swap);
        searchForm.Children.Add(Endpoint(model.Destination, "Destination", "Ziel"));
        var timeLayout = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(4, 8) };
        timeLayout.Children.Add(TransitVisuals.Text("Zeitpunkt", 17, true));
        var timeMode = new Picker { AutomationId = "RoutingTimeMode", Title = "Zeitpunkt", ItemsSource = new[] { "Jetzt", "Andere Zeit" }, SelectedIndex = 0 };
        timeMode.SelectedIndexChanged += (_, _) => model.UseCurrentTime = timeMode.SelectedIndex == 0;
        timeLayout.Children.Add(timeMode);
        var alternative = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 8, IsVisible = false };
        var date = new DatePicker { AutomationId = "RoutingDate", Date = model.SelectedDate };
        date.DateSelected += (_, args) => model.SelectedDate = args.NewDate ?? DateTime.Today;
        var clock = new TimePicker { AutomationId = "RoutingTime", Time = model.SelectedTime };
        clock.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(TimePicker.Time) && clock.Time is { } value) model.SelectedTime = value; };
        alternative.Add(date, 0); alternative.Add(clock, 1); timeLayout.Children.Add(alternative);
        var arrive = new Switch { AutomationId = "RoutingArriveBy" };
        SemanticProperties.SetDescription(arrive, "Ankunft statt Abfahrt");
        arrive.SetBinding(Switch.IsToggledProperty, nameof(model.ArriveBy));
        var arriveRow = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
        arriveRow.Add(TransitVisuals.Text("Ankunft statt Abfahrt", 15), 0); arriveRow.Add(arrive, 1); timeLayout.Children.Add(arriveRow);
        timeMode.SelectedIndexChanged += (_, _) => alternative.IsVisible = timeMode.SelectedIndex == 1;
        searchForm.Children.Add(timeLayout);
        content.Children.Add(new Border { Content = searchForm });
        var savedConnections = new VerticalStackLayout { Spacing = 4, AutomationId = "ConnectionFavorites" };
        content.Children.Add(savedConnections);
        void RenderConnectionFavorites()
        {
            savedConnections.Children.Clear();
            foreach (var favorite in connectionFavorites.Favorites)
            {
                var action = new Button { Text = favorite.Name, AutomationId = "ConnectionFavorite" + favorite.Origin.Source + favorite.Origin.Id + favorite.Destination.Source + favorite.Destination.Id, HeightRequest = 44 };
                SemanticProperties.SetDescription(action, "Gespeicherte Verbindung " + favorite.Name);
                action.Command = new Command(() => model.SelectConnection(
                    new Address { Name = favorite.Origin.Name, Stop = favorite.Origin, Coordinate = favorite.Origin.Coordinate },
                    new Address { Name = favorite.Destination.Name, Stop = favorite.Destination, Coordinate = favorite.Destination.Coordinate }));
                savedConnections.Children.Add(action);
            }
            savedConnections.IsVisible = savedConnections.Children.Count > 0;
        }
        connectionFavorites.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(connectionFavorites.Favorites)) RenderConnectionFavorites(); };
        var search = new Button { Text = "Verbindungen suchen", AutomationId = "SearchJourneys", Command = model.SearchCommand };
        SemanticProperties.SetDescription(search, "Verbindungen suchen");
        content.Children.Add(search);
        var busy = new ActivityIndicator { AutomationId = "RoutingBusy" }; busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(model.IsBusy)); busy.SetBinding(IsVisibleProperty, nameof(model.IsBusy)); content.Children.Add(busy);
        var status = new Label { AutomationId = "RoutingStatus" }; status.SetBinding(Label.TextProperty, nameof(model.Status)); content.Children.Add(status);
        var metadata = new Label { AutomationId = "RoutingMetadata" }; metadata.SetBinding(Label.TextProperty, nameof(model.Metadata)); content.Children.Add(metadata);
        Content = TransitVisuals.Page(content);
    }
    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing(); foreground.PropertyChanged += ForegroundChanged;
        try { await connectionFavorites.LoadAsync(); }
        catch (Exception) { Title = "Gespeicherte Verbindungen konnten nicht geladen werden"; }
    }
    /// <inheritdoc />
    protected override void OnDisappearing() { foreground.PropertyChanged -= ForegroundChanged; base.OnDisappearing(); CancelPending(); }
    private void ForegroundChanged(object? sender, PropertyChangedEventArgs args) { if (!foreground.IsActive) CancelPending(); }
    private void CancelPending() { model.Origin.CancelPending(); model.Destination.CancelPending(); model.CancelPending(); }
    private Border Endpoint(EndpointViewModel endpoint, string prefix, string title)
    {
        var layout = new VerticalStackLayout { Spacing = 8, BindingContext = endpoint };
        layout.Children.Add(TransitVisuals.Text(title, 17, true));
        var toggle = new Switch { AutomationId = prefix + "CoordinateMode" }; toggle.SetBinding(Switch.IsToggledProperty, nameof(endpoint.IsCoordinateMode));
        SemanticProperties.SetDescription(toggle, title + ": Koordinatenmodus");
        var text = new Entry { Placeholder = title + ": Adresse oder Haltestelle", AutomationId = prefix + "Text" }; text.SetBinding(Entry.TextProperty, nameof(endpoint.Text)); text.SetBinding(IsVisibleProperty, nameof(endpoint.IsTextMode));
        var lookup = new Button { Text = "⌕", AutomationId = prefix + "Search", Command = endpoint.SearchCommand, HeightRequest = 48, WidthRequest = 48 };
        SemanticProperties.SetDescription(lookup, title + " suchen");
        var locationButton = new Button { Text = "⌖", AutomationId = prefix + "Location", Command = endpoint.LocationCommand, HeightRequest = 48, WidthRequest = 48 };
        SemanticProperties.SetDescription(locationButton, title + " Mein Standort");
        var inputRow = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto)], ColumnSpacing = 8 };
        inputRow.Add(text, 0); inputRow.Add(lookup, 1); inputRow.Add(locationButton, 2); layout.Children.Add(inputRow);
        SemanticProperties.SetDescription(text, title + ": Adresse oder Haltestelle");
        var coordinates = new VerticalStackLayout { Spacing = 8 }; coordinates.SetBinding(IsVisibleProperty, nameof(endpoint.IsCoordinateMode));
        coordinates.Children.Add(new Label { Text = "Breite (−90 bis 90)" });
        var latitude = new Entry { Placeholder = "z. B. 51,4556", AutomationId = prefix + "Latitude", Keyboard = Keyboard.Numeric }; latitude.SetBinding(Entry.TextProperty, nameof(endpoint.Latitude)); coordinates.Children.Add(latitude);
        coordinates.Children.Add(new Label { Text = "Länge (−180 bis 180)" });
        var longitude = new Entry { Placeholder = "z. B. 7,0116", AutomationId = prefix + "Longitude", Keyboard = Keyboard.Numeric }; longitude.SetBinding(Entry.TextProperty, nameof(endpoint.Longitude)); coordinates.Children.Add(longitude); layout.Children.Add(coordinates);
        var coordinateMode = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], ColumnSpacing = 12 };
        coordinateMode.Add(TransitVisuals.Text("Koordinaten eingeben", 15), 0); coordinateMode.Add(toggle, 1); layout.Children.Add(coordinateMode);
        endpoint.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(endpoint.IsCoordinateMode)) { lookup.Text = endpoint.IsCoordinateMode ? "✓" : "⌕"; SemanticProperties.SetDescription(lookup, endpoint.IsCoordinateMode ? title + " Koordinaten übernehmen" : title + " suchen"); } };
        var locationStatus = new Label { AutomationId = prefix + "LocationStatus" }; locationStatus.SetBinding(Label.TextProperty, nameof(endpoint.LocationStatus)); layout.Children.Add(locationStatus);
        var busy = new ActivityIndicator { AutomationId = prefix + "Busy" }; busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(endpoint.IsBusy)); busy.SetBinding(IsVisibleProperty, nameof(endpoint.IsBusy)); layout.Children.Add(busy);
        var status = new Label { AutomationId = prefix + "Status" }; status.SetBinding(Label.TextProperty, nameof(endpoint.Status)); layout.Children.Add(status);
        var matches = new VerticalStackLayout { Spacing = 8, AutomationId = prefix + "Matches", IsVisible = endpoint.Matches.Count > 0 }; layout.Children.Add(matches);
        endpoint.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(endpoint.Matches)) return;
            matches.Children.Clear();
            matches.IsVisible = endpoint.Matches.Count > 0;
            for (var i = 0; i < endpoint.Matches.Count; i++)
            {
                var candidate = endpoint.Matches[i];
                matches.Children.Add(TransitVisuals.Candidate(candidate, new Button { LineBreakMode = LineBreakMode.WordWrap, AutomationId = prefix + "Match" + i, Command = endpoint.SelectCommand, CommandParameter = candidate }));
            }
            TransitVisuals.ApplyRoles(matches);
        };
        var selection = new Label { AutomationId = prefix + "Selection", FontAttributes = FontAttributes.Bold }; selection.SetBinding(Label.TextProperty, nameof(endpoint.Selection)); layout.Children.Add(selection);
        var metadata = new Label { AutomationId = prefix + "Metadata" }; metadata.SetBinding(Label.TextProperty, nameof(endpoint.Metadata)); layout.Children.Add(metadata);
        locationStatus.IsVisible = false;
        status.IsVisible = false;
        selection.IsVisible = false;
        metadata.IsVisible = !string.IsNullOrWhiteSpace(endpoint.Metadata);
        var favoritesList = new VerticalStackLayout { AutomationId = prefix + "Favorites", Spacing = 4, IsVisible = false };
        layout.Children.Add(favoritesList);
        void RenderFavorites()
        {
            var query = endpoint.Text.Trim();
            var matches = favorites.Cards.Where(card => string.IsNullOrWhiteSpace(query) || card.Stop.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)).Take(5).ToArray();
            favoritesList.Children.Clear();
            foreach (var card in matches)
            {
                var favorite = new Button { Text = card.Stop.Name, AutomationId = prefix + "Favorite" + card.Stop.Id, HeightRequest = 44 };
                SemanticProperties.SetDescription(favorite, "Favorit " + card.Stop.Name);
                favorite.Command = new Command(() => { endpoint.SelectFavorite(card.Stop); favoritesList.IsVisible = false; });
                favoritesList.Children.Add(favorite);
            }
            favoritesList.IsVisible = matches.Length > 0 && text.IsFocused && endpoint.IsTextMode;
        }
        text.Focused += async (_, _) => { await favorites.LoadAsync(); RenderFavorites(); };
        text.Unfocused += (_, _) => favoritesList.IsVisible = false;
        endpoint.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(endpoint.Text)) RenderFavorites();
            if (args.PropertyName == nameof(endpoint.LocationStatus)) locationStatus.IsVisible = !string.IsNullOrWhiteSpace(endpoint.LocationStatus);
            if (args.PropertyName == nameof(endpoint.Status)) status.IsVisible = !string.IsNullOrWhiteSpace(endpoint.Status);
            if (args.PropertyName == nameof(endpoint.Selection)) selection.IsVisible = true;
            if (args.PropertyName == nameof(endpoint.Metadata)) metadata.IsVisible = !string.IsNullOrWhiteSpace(endpoint.Metadata);
        };
        return new Border { Padding = 12, Content = layout };
    }
}
