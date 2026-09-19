using FlowNRW.Core.Presentation;
using FlowNRW.Core.Favorites;

namespace FlowNRW;

/// <summary>Manually refreshed departure board retaining the last known data on errors.</summary>
public sealed class DeparturePage : ContentPage
{
    private readonly StopMonitorViewModel model;
    private readonly FavoriteHomeViewModel favorites;
    private readonly Button favoriteButton;
    private readonly AsyncRelayCommand toggleFavorite;
    private readonly VerticalStackLayout items = new() { Spacing = 12 };

    /// <summary>Creates the departure board.</summary>
    /// <param name="model">Shared monitor session.</param>
    /// <param name="favorites">Persisted technical stop selections.</param>
    public DeparturePage(StopMonitorViewModel model, FavoriteHomeViewModel favorites)
    {
        this.model = model;
        this.favorites = favorites;
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
        };
#endif
        layout.Children.Add(new Button { Text = "Aktualisieren", AutomationId = "RefreshDepartures", Command = model.RefreshCommand });
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
    protected override void OnAppearing()
    {
        base.OnAppearing();
        model.PropertyChanged += ModelChanged;
        favorites.PropertyChanged += FavoritesChanged;
        RefreshFavorite();
        RenderItems();
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
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
