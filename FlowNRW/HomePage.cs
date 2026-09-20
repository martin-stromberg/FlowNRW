using FlowNRW.Core.Favorites;
using FlowNRW.Core.Maps;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>Favorite departure boards with independent manual refresh and explicit distance sorting.</summary>
public sealed class HomePage : ContentPage
{
    private readonly FavoriteHomeViewModel model;
    private readonly VerticalStackLayout cards = new() { Spacing = 16 };
    private readonly List<Action> unsubscribe = [];
    private readonly List<AsyncRelayCommand> actions = [];
    private readonly StopMonitorViewModel monitor;
    private long appearance;
    private bool active;
    private readonly RefreshSettingsViewModel settings;
    private readonly ForegroundState foreground;
    private readonly Dictionary<FavoriteMonitorViewModel, RefreshLoop> refreshLoops = [];

    /// <summary>Creates the home view.</summary>
    /// <param name="model">Retained favorite state.</param>
    /// <param name="monitor">Shared single-stop monitor.</param>
    /// <param name="map">Validated favorite map snapshots.</param>
    /// <param name="settings">Persisted refresh interval.</param>
    /// <param name="foreground">Active window state.</param>
    public HomePage(FavoriteHomeViewModel model, StopMonitorViewModel monitor, MapViewModel map,
        RefreshSettingsViewModel settings, ForegroundState foreground)
    {
        this.model = model;
        this.monitor = monitor;
        this.settings = settings;
        this.foreground = foreground;
        BindingContext = model;
        Title = "Meine Haltestellen";
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 12 };
        layout.Children.Add(new Label { Text = "Deine nächsten Abfahrten", FontSize = 28, FontAttributes = FontAttributes.Bold });
        layout.Children.Add(new Button { Text = "Verbindung suchen", AutomationId = "OpenJourneySearch", Command = Navigate("search") });
        layout.Children.Add(new Button { Text = "Haltestelle hinzufügen", AutomationId = "OpenHomeStops", Command = Navigate("stops") });
        layout.Children.Add(new Button { Text = "Aktualisierung einstellen", AutomationId = "OpenRefreshSettings", Command = Navigate("refresh-settings") });
        var interval = new Label { AutomationId = "RefreshIntervalStatus", BindingContext = settings };
        interval.SetBinding(Label.TextProperty, nameof(settings.Description)); layout.Children.Add(interval);
        var status = new Label { AutomationId = "HomeStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.Status)); layout.Children.Add(status);
        var count = new Label { AutomationId = "FavoriteCount" }; layout.Children.Add(count);
        layout.Children.Add(new Button { Text = "Entfernungen aktualisieren", AutomationId = "SortFavorites", Command = model.LocationCommand });
        var location = new Label { AutomationId = "HomeLocationStatus" };
        location.SetBinding(Label.TextProperty, nameof(model.LocationStatus)); layout.Children.Add(location);
        var showMap = new AsyncRelayCommand(async () => { map.ShowFavorites(model); await Shell.Current.GoToAsync("map"); },
            () => model.Cards.Count > 0, _ => Title = "Karte konnte nicht geöffnet werden");
        layout.Children.Add(new Button { Text = "Favoriten auf Karte zeigen", AutomationId = "HomeMap", Command = showMap });
        layout.Children.Add(cards);
        model.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(model.Cards))
            {
                RenderCards();
                if (active) _ = RefreshMissingAsync();
                ReconcileRefreshLoops();
            }
            count.Text = $"{model.Cards.Count} Favoriten";
            showMap.Refresh();
            foreach (var action in actions.ToArray()) action.Refresh();
        };
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (layout.Children.Any(child => child.AutomationId == "HomeScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "HomeScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario)); layout.Children.Insert(0, scenario);
            var calls = new Label { AutomationId = "FavoriteCalls", BindingContext = fixture };
            calls.SetBinding(Label.TextProperty, nameof(fixture.DepartureCalls)); layout.Children.Insert(1, calls);
        };
#endif
        count.Text = $"{model.Cards.Count} Favoriten";
        Content = new ScrollView { Content = layout };
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        active = true;
        settings.Changed += SettingsChanged;
        foreground.PropertyChanged += ForegroundChanged;
        var version = ++appearance;
        try
        {
            await settings.LoadAsync();
            if (version != appearance) return;
            await model.LoadAsync();
            if (version != appearance) return;
            RenderCards(); ReconcileRefreshLoops(); await model.RefreshMissingAsync();
        }
        catch (Exception) { Title = "Favoriten konnten nicht geladen werden"; }
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        appearance++;
        active = false;
        settings.Changed -= SettingsChanged;
        foreground.PropertyChanged -= ForegroundChanged;
        ReconcileRefreshLoops();
        model.CancelPending();
        base.OnDisappearing();
    }

    private AsyncRelayCommand Navigate(string route) => new(() => Shell.Current.GoToAsync(route), () => true,
        _ => Title = "Seite konnte nicht geöffnet werden");

    private void SettingsChanged(object? sender, EventArgs args) => ReconcileRefreshLoops();

    private void ForegroundChanged(object? sender, PropertyChangedEventArgs args) => ReconcileRefreshLoops();

    private void ReconcileRefreshLoops()
    {
        var running = active && foreground.IsActive && settings.IsLoaded;
        foreach (var obsolete in refreshLoops.Keys.Where(card => !running || !model.Contains(card)).ToArray())
        {
            refreshLoops[obsolete].Dispose();
            refreshLoops.Remove(obsolete);
        }
        if (!running) return;
        foreach (var card in model.Cards)
        {
            if (!refreshLoops.TryGetValue(card, out var loop))
            {
                loop = new RefreshLoop(card.RefreshAutomaticallyAsync, card.CancelPending);
                refreshLoops.Add(card, loop);
            }
            loop.Start(settings.IntervalSeconds);
        }
    }

    private async Task RefreshMissingAsync()
    {
        try { await model.RefreshMissingAsync(); }
        catch (Exception) { Title = "Abfahrten konnten nicht geladen werden"; }
    }

    private void RenderCards()
    {
        foreach (var detach in unsubscribe) detach();
        unsubscribe.Clear(); actions.Clear(); cards.Children.Clear();
        for (var index = 0; index < model.Cards.Count; index++)
        {
            var card = model.Cards[index];
            var cardIndex = index;
            var layout = new VerticalStackLayout { Spacing = 8, BindingContext = card };
            AddLabel(layout, "FavoriteName" + index, nameof(card.Title), 22);
            AddLabel(layout, "FavoriteDistance" + index, nameof(card.DistanceLabel));
            AddLabel(layout, "FavoriteStatus" + index, nameof(card.Status));
            AddLabel(layout, "FavoriteMetadata" + index, nameof(card.Metadata));
            var departures = new VerticalStackLayout { Spacing = 8 };
            void RenderDepartures()
            {
                departures.Children.Clear();
                for (var item = 0; item < Math.Min(card.Items.Count, 5); item++)
                    departures.Children.Add(new Label { AutomationId = $"FavoriteDeparture{cardIndex}_{item}", Text = DeparturePresentation.Describe(card.Items[item]) });
            }
            PropertyChangedEventHandler changed = (_, args) => { if (args.PropertyName == nameof(card.Items)) RenderDepartures(); };
            card.PropertyChanged += changed;
            unsubscribe.Add(() => card.PropertyChanged -= changed);
            RenderDepartures(); layout.Children.Add(departures);
            layout.Children.Add(new Button { Text = "Aktualisieren", AutomationId = "RefreshFavorite" + index, Command = card.RefreshCommand });
            var open = new AsyncRelayCommand(() => monitor.OpenFavoriteAsync(model, card), () => model.Contains(card),
                _ => Title = "Monitor konnte nicht geöffnet werden");
            var remove = new AsyncRelayCommand(() => model.RemoveAsync(card), () => !model.IsSaving && model.Contains(card),
                _ => Title = "Favorit konnte nicht entfernt werden");
            actions.Add(open); actions.Add(remove);
            layout.Children.Add(new Button { Text = "Abfahrtsmonitor öffnen", AutomationId = "OpenFavorite" + index, Command = open });
            layout.Children.Add(new Button { Text = "Favorit entfernen", AutomationId = "RemoveFavorite" + index, Command = remove });
            cards.Children.Add(new Border { Padding = 16, Stroke = Color.FromArgb("#C7D7EC"), Content = layout });
        }
    }

    private static void AddLabel(VerticalStackLayout layout, string id, string property, double size = 14)
    {
        var label = new Label { AutomationId = id, FontSize = size };
        label.SetBinding(Label.TextProperty, property); layout.Children.Add(label);
    }
}
