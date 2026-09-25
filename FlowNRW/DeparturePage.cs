using FlowNRW.Core.Presentation;
using FlowNRW.Core.Favorites;
using FlowNRW.Core.Refresh;

namespace FlowNRW;

/// <summary>Departure board with manual and foreground refresh, retaining known data on errors.</summary>
public sealed class DeparturePage : ContentPage
{
    private readonly StopMonitorViewModel model;
    private readonly FavoriteHomeViewModel favorites;
    private readonly Button favoriteButton;
    private readonly AsyncRelayCommand toggleFavorite;
    private readonly VerticalStackLayout items = new() { Spacing = 12 };
    private readonly RefreshSettingsViewModel settings;
    private readonly ForegroundState foreground;
    private readonly RefreshLoop refreshLoop;
    private bool active;
    private long appearance;

    /// <summary>Creates the departure board.</summary>
    /// <param name="model">Shared monitor session.</param>
    /// <param name="favorites">Persisted technical stop selections.</param>
    /// <param name="settings">Shared refresh interval.</param>
    /// <param name="foreground">Active window state.</param>
    public DeparturePage(StopMonitorViewModel model, FavoriteHomeViewModel favorites,
        RefreshSettingsViewModel settings, ForegroundState foreground)
    {
        this.model = model;
        this.favorites = favorites;
        this.settings = settings;
        this.foreground = foreground;
        refreshLoop = new RefreshLoop(model.RefreshAutomaticallyAsync, model.CancelPending);
        BindingContext = model;
        SetBinding(TitleProperty, new Binding(nameof(model.Title)));
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        var stop = new Label { FontSize = 24, FontAttributes = FontAttributes.Bold, AutomationId = "MonitorStop" };
        stop.SetBinding(Label.TextProperty, nameof(model.Title));
        layout.Children.Add(stop);
        toggleFavorite = new AsyncRelayCommand(async () =>
        {
            var selected = model.SelectedStop;
            if (selected is not null) await favorites.ToggleAsync(selected);
        }, () => model.SelectedStop is not null && !favorites.IsSaving,
            _ => Title = "Favorit konnte nicht gespeichert werden");
        favoriteButton = new Button { AutomationId = "ToggleFavorite", Command = toggleFavorite, LineBreakMode = LineBreakMode.WordWrap };
        layout.Children.Add(favoriteButton);
        var favoriteStatus = new Label { AutomationId = "FavoriteToggleStatus", BindingContext = favorites };
        favoriteStatus.SetBinding(Label.TextProperty, nameof(favorites.Status)); layout.Children.Add(favoriteStatus);
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (layout.Children.Any(child => child.AutomationId == "FavoriteScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "FavoriteScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario)); layout.Children.Insert(0, scenario);
            var calls = new Label { AutomationId = "FavoriteCalls", BindingContext = fixture };
            calls.SetBinding(Label.TextProperty, nameof(fixture.DepartureCalls)); layout.Children.Insert(1, calls);
            var activity = new Label { AutomationId = "RefreshForeground", BindingContext = foreground };
            activity.SetBinding(Label.TextProperty, nameof(foreground.IsActive)); layout.Children.Insert(2, activity);
        };
#endif
        layout.Children.Add(new Button { Text = "Aktualisieren", AutomationId = "RefreshDepartures", Command = model.RefreshCommand });
        layout.Children.Add(new Button
        {
            Text = "Aktualisierung einstellen",
            AutomationId = "OpenRefreshSettings",
            Command = new AsyncRelayCommand(() => Shell.Current.GoToAsync("refresh-settings"), () => true,
                _ => Title = "Einstellungen konnten nicht geöffnet werden")
        });
        var interval = new Label { AutomationId = "RefreshIntervalStatus", BindingContext = settings };
        interval.SetBinding(Label.TextProperty, nameof(settings.Description)); layout.Children.Add(interval);
        var busy = new ActivityIndicator { AutomationId = "MonitorBusy" };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(model.IsBusy));
        layout.Children.Add(busy);
        var status = new Label { AutomationId = "MonitorStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.Status));
        layout.Children.Add(status);
        var metadata = new Label { AutomationId = "MonitorMetadata" };
        metadata.SetBinding(Label.TextProperty, nameof(model.Metadata));
        layout.Children.Add(metadata);
        layout.Children.Add(items);
        Content = new ScrollView { Content = layout };
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        active = true;
        var version = ++appearance;
        settings.Changed += SettingsChanged;
        foreground.PropertyChanged += ForegroundChanged;
        model.PropertyChanged += ModelChanged;
        favorites.PropertyChanged += FavoritesChanged;
        RefreshFavorite();
        RenderItems();
        await settings.LoadAsync();
        if (version == appearance) ReconcileRefreshLoop();
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        active = false;
        appearance++;
        settings.Changed -= SettingsChanged;
        foreground.PropertyChanged -= ForegroundChanged;
        refreshLoop.Stop();
        model.PropertyChanged -= ModelChanged;
        favorites.PropertyChanged -= FavoritesChanged;
        model.CancelPending();
        base.OnDisappearing();
    }

    private void ModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(model.Items)) RenderItems();
        if (args.PropertyName == nameof(model.SelectedStop)) RefreshFavorite();
    }

    private void FavoritesChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) => RefreshFavorite();

    private void SettingsChanged(object? sender, EventArgs args) => ReconcileRefreshLoop();

    private void ForegroundChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) => ReconcileRefreshLoop();

    private void ReconcileRefreshLoop()
    {
        if (active && foreground.IsActive && settings.IsLoaded) refreshLoop.Start(settings.IntervalSeconds);
        else refreshLoop.Stop();
    }

    private void RefreshFavorite()
    {
        favoriteButton.Text = model.SelectedStop is { } stop && favorites.IsFavorite(stop) ? "Favorit entfernen" : "Als Favorit speichern";
        toggleFavorite.Refresh();
    }

    private void RenderItems()
    {
        items.Children.Clear();
        for (var index = 0; index < model.Items.Count; index++)
        {
            items.Children.Add(new Border
            {
                Padding = 16,
                Stroke = Color.FromArgb("#C7D7EC"),
                Content = new Label { Text = DeparturePresentation.Describe(model.Items[index]), FontSize = 18, AutomationId = "Departure" + index }
            });
        }
    }
}
