using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>Provider-ordered native result list.</summary>
public sealed class ResultsPage : ContentPage
{
    private readonly ResultsViewModel model;
    private readonly VerticalStackLayout journeys = new() { Spacing = 16 };
    private readonly ForegroundState foreground;
    private readonly RefreshSettingsViewModel settings;
    private readonly RefreshFreshness freshness;
    private readonly ConnectionFavoritesViewModel connectionFavorites;
    private readonly Label connectionHeader;
    private readonly Button favoriteConnection;
    private bool active;
    /// <summary>Creates the result view.</summary>
    /// <param name="model">Session projection.</param>
    /// <param name="foreground">Shared active-window state.</param>
    /// <param name="settings">Committed automatic refresh preference.</param>
    /// <param name="freshness">Realtime age policy.</param>
    /// <param name="connectionFavorites">Explicitly saved reusable journey endpoint pairs.</param>
    public ResultsPage(ResultsViewModel model, ForegroundState foreground, RefreshSettingsViewModel settings, RefreshFreshness freshness,
        ConnectionFavoritesViewModel connectionFavorites)
    {
        this.foreground = foreground; this.settings = settings; this.freshness = freshness;
        this.connectionFavorites = connectionFavorites;
        this.model = model; BindingContext = model.Session; Title = "Verbindungen";
        var layout = new VerticalStackLayout { Spacing = 16, Padding = 16 };
        var connection = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], ColumnSpacing = 8 };
        connectionHeader = TransitVisuals.Text("", 20, true, "ConnectionHeader");
        connection.Add(connectionHeader, 0);
        favoriteConnection = TransitVisuals.SecondaryAction("☆", "Verbindung speichern", "ToggleConnectionFavorite",
            new AsyncRelayCommand(ToggleConnectionAsync, () => CanToggleConnection(), _ => Title = "Verbindung konnte nicht gespeichert werden"));
        connection.Add(favoriteConnection, 1);
        layout.Children.Add(connection);
        var status = new Label { FontSize = 24, AutomationId = "ResultsStatus" }; status.SetBinding(Label.TextProperty, nameof(model.Session.Status)); layout.Children.Add(status);
        var metadata = new Label { AutomationId = "ResultsMetadata" }; metadata.SetBinding(Label.TextProperty, nameof(model.Session.Metadata));
        layout.Children.Add(journeys); layout.Children.Add(metadata); Content = TransitVisuals.Page(layout);
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") == "1") return;
            if (layout.Children.Any(child => child.AutomationId == "LifecycleScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "LifecycleScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario)); layout.Children.Insert(0, scenario);
            var calls = new Label { AutomationId = "RouteCalls", BindingContext = fixture };
            calls.SetBinding(Label.TextProperty, nameof(fixture.RouteCalls)); layout.Children.Insert(1, calls);
        };
#endif
    }
    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing(); active = true;
        foreground.PropertyChanged += ForegroundChanged;
        model.Session.PropertyChanged += SessionChanged;
        RenderJourneys();
        try { await connectionFavorites.LoadAsync(); }
        catch (Exception) { Title = "Gespeicherte Verbindungen konnten nicht geladen werden"; }
        RenderConnectionHeader();
        await settings.LoadAsync();
        await ResumeAsync();
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        active = false;
        foreground.PropertyChanged -= ForegroundChanged;
        model.Session.PropertyChanged -= SessionChanged;
        model.Session.CancelPending();
        base.OnDisappearing();
    }

    private void SessionChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(model.Session.Journeys)) RenderJourneys();
        if (args.PropertyName is nameof(model.Session.Result) or nameof(model.Session.Origin) or nameof(model.Session.Destination)) RenderConnectionHeader();
    }

    private async void ForegroundChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (!foreground.IsActive) model.Session.CancelPending();
        else await ResumeAsync();
    }

    private async Task ResumeAsync()
    {
        if (!active || !foreground.IsActive || !settings.IsLoaded || settings.IntervalSeconds == 0) return;
        try { await model.Session.RefreshIfStaleAsync(freshness); }
        catch (Exception) { Title = "Aktualisierung fehlgeschlagen"; }
    }

    private void RenderJourneys()
    {
        foreach (var card in journeys.Children.OfType<JourneyCardView>()) card.Detach();
        journeys.Children.Clear();
        for (var i = 0; i < model.Session.Journeys.Count; i++)
        {
            var journey = model.Session.Journeys[i];
            var command = new AsyncRelayCommand(() => model.OpenJourneyAsync(journey), () => true, _ => { Title = "Details konnten nicht geöffnet werden"; });
            journeys.Children.Add(new JourneyCardView(journey, "Journey" + i, command));
        }
    }

    private bool CanToggleConnection() => model.Session.Origin.SelectedAddress?.Stop is not null && model.Session.Destination.SelectedAddress?.Stop is not null;

    private async Task ToggleConnectionAsync()
    {
        await connectionFavorites.ToggleAsync(model.Session.Origin.SelectedAddress, model.Session.Destination.SelectedAddress);
        RenderConnectionHeader();
    }

    private void RenderConnectionHeader()
    {
        var origin = model.Session.Origin.SelectedAddress;
        var destination = model.Session.Destination.SelectedAddress;
        connectionHeader.Text = origin is null || destination is null ? "" : origin.Name + " → " + destination.Name;
        favoriteConnection.IsVisible = CanToggleConnection();
        favoriteConnection.Text = connectionFavorites.Contains(origin, destination) ? "★" : "☆";
        SemanticProperties.SetDescription(favoriteConnection, connectionFavorites.Contains(origin, destination) ? "Gespeicherte Verbindung entfernen" : "Verbindung speichern");
        ((AsyncRelayCommand)favoriteConnection.Command!).Refresh();
    }
}
