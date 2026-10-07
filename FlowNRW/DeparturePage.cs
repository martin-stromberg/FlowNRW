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
    private readonly Label monitorStatus;
    private readonly Label favoriteStatus;
    private bool active;
    private long appearance;
    private readonly RefreshFreshness freshness;
    private CancellationTokenSource? resume;

    /// <summary>Creates the departure board.</summary>
    /// <param name="model">Shared monitor session.</param>
    /// <param name="favorites">Persisted technical stop selections.</param>
    /// <param name="settings">Shared refresh interval.</param>
    /// <param name="foreground">Active window state.</param>
    /// <param name="freshness">Shared realtime age policy.</param>
    public DeparturePage(StopMonitorViewModel model, FavoriteHomeViewModel favorites,
        RefreshSettingsViewModel settings, ForegroundState foreground, RefreshFreshness freshness)
    {
        this.model = model;
        this.favorites = favorites;
        this.settings = settings;
        this.foreground = foreground;
        this.freshness = freshness;
        refreshLoop = new RefreshLoop(model.RefreshAutomaticallyAsync, model.CancelPending);
        BindingContext = model;
        SetBinding(TitleProperty, new Binding(nameof(model.Title)));
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        var station = new VerticalStackLayout { Spacing = 6, Padding = 16 };
        var stop = new Label { FontSize = 28, FontAttributes = FontAttributes.Bold, AutomationId = "MonitorStop" };
        stop.SetBinding(Label.TextProperty, nameof(model.Title));
        var stationHeader = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
        stationHeader.Add(stop, 0);
        var busy = new ActivityIndicator { AutomationId = "MonitorBusy", VerticalOptions = LayoutOptions.Center };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(model.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(model.IsBusy));
        SemanticProperties.SetDescription(busy, "Abfahrten werden aktualisiert");
        stationHeader.Add(busy, 1);
        station.Children.Add(stationHeader);
        layout.Children.Add(new Border { Content = station });
        toggleFavorite = new AsyncRelayCommand(async () =>
        {
            var selected = model.SelectedStop;
            if (selected is not null) await favorites.ToggleAsync(selected);
        }, () => model.SelectedStop is not null && !favorites.IsSaving,
            _ => Title = "Favorit konnte nicht gespeichert werden");
        favoriteButton = new Button { AutomationId = "ToggleFavorite", Command = toggleFavorite, LineBreakMode = LineBreakMode.TailTruncation };
        var refresh = TransitVisuals.SecondaryAction("↻", "Abfahrten aktualisieren", "RefreshDepartures", model.RefreshCommand);
        var settingsButton = TransitVisuals.SecondaryAction("⚙", "Aktualisierung einstellen", "OpenRefreshSettings",
            new AsyncRelayCommand(() => Shell.Current.GoToAsync("refresh-settings"), () => true,
                _ => Title = "Einstellungen konnten nicht geöffnet werden"));
        var actions = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 8 };
        actions.Add(refresh, 0); actions.Add(favoriteButton, 1); actions.Add(settingsButton, 2);
        layout.Children.Add(actions);
        favoriteStatus = new Label { AutomationId = "FavoriteToggleStatus", BindingContext = favorites };
        favoriteStatus.SetBinding(Label.TextProperty, nameof(favorites.Status));
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") == "1") return;
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
        monitorStatus = new Label { AutomationId = "MonitorStatus" };
        monitorStatus.SetBinding(Label.TextProperty, nameof(model.Status));
        var monitorState = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(4, 0) };
        monitorState.Children.Add(monitorStatus); monitorState.Children.Add(favoriteStatus);
        layout.Children.Add(monitorState);
        layout.Children.Add(items);
        Content = TransitVisuals.Page(layout);
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
        UpdateMonitorStatus();
        UpdateFavoriteStatus();
        RenderItems();
        await settings.LoadAsync();
        if (version == appearance) ReconcileRefreshLoop();
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        active = false;
        appearance++;
        resume?.Cancel();
        resume = null;
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
        if (args.PropertyName is nameof(model.Status) or nameof(model.IsBusy)) UpdateMonitorStatus();
    }

    private void UpdateMonitorStatus()
    {
        var status = model.Status;
        monitorStatus.IsVisible = !model.IsBusy
            && (status.Contains("Keine nächsten", StringComparison.OrdinalIgnoreCase)
                || status.Contains("unvollständig", StringComparison.OrdinalIgnoreCase)
                || status.Contains("fehlgeschlagen", StringComparison.OrdinalIgnoreCase)
                || status.Contains("nicht geladen", StringComparison.OrdinalIgnoreCase)
                || status.Contains("abgebrochen", StringComparison.OrdinalIgnoreCase)
                || status.Contains("Letzte bekannte", StringComparison.OrdinalIgnoreCase)
                || status.Contains("Datenstand", StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateFavoriteStatus()
    {
        var status = favorites.Status;
        favoriteStatus.IsVisible = favorites.IsSaving
            || status.Contains("fehlgeschlagen", StringComparison.OrdinalIgnoreCase)
            || status.Contains("nicht gespeichert", StringComparison.OrdinalIgnoreCase)
            || status.Contains("maximal", StringComparison.OrdinalIgnoreCase)
            || status.Contains("vollständige", StringComparison.OrdinalIgnoreCase)
            || status is "Favorit gespeichert." or "Favorit entfernt.";
    }

    private void FavoritesChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        RefreshFavorite();
        if (args.PropertyName is nameof(favorites.Status) or nameof(favorites.IsSaving)) UpdateFavoriteStatus();
    }

    private void SettingsChanged(object? sender, EventArgs args) => ReconcileRefreshLoop();

    private async void ForegroundChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (!foreground.IsActive)
        {
            resume?.Cancel();
            resume = null;
            refreshLoop.Stop();
            model.CancelPending();
            return;
        }
        if (!active || !settings.IsLoaded || settings.IntervalSeconds == 0 || resume is not null) return;
        using var source = new CancellationTokenSource();
        resume = source;
        try { await model.RefreshIfStaleAsync(freshness, source.Token); }
        catch (OperationCanceledException) { }
        catch (Exception) { Title = "Aktualisierung fehlgeschlagen"; }
        finally
        {
            if (ReferenceEquals(resume, source))
            {
                resume = null;
                ReconcileRefreshLoop();
            }
        }
    }

    private void ReconcileRefreshLoop()
    {
        if (active && foreground.IsActive && settings.IsLoaded && resume is null) refreshLoop.Start(settings.IntervalSeconds);
        else refreshLoop.Stop();
    }

    private void RefreshFavorite()
    {
        var saved = model.SelectedStop is { } stop && favorites.IsFavorite(stop);
        favoriteButton.Text = saved ? "★ Entfernen" : "☆ Speichern";
        SemanticProperties.SetDescription(favoriteButton, saved ? "Favorit entfernen" : "Als Favorit speichern");
        toggleFavorite.Refresh();
    }

    private void RenderItems()
    {
        items.Children.Clear();
        for (var index = 0; index < model.Items.Count; index++)
        {
            items.Children.Add(new DepartureCardView(model.Items[index], "Departure" + index));
        }
    }
}
