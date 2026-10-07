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
    private readonly VerticalStackLayout nearbyCards = new() { Spacing = 8 };
    private readonly List<Action> unsubscribe = [];
    private readonly List<AsyncRelayCommand> actions = [];
    private readonly StopMonitorViewModel monitor;
    private long appearance;
    private bool active;
    private readonly RefreshSettingsViewModel settings;
    private readonly ForegroundState foreground;
    private readonly RefreshFreshness freshness;
    private CancellationTokenSource? resume;
    private readonly Dictionary<FavoriteMonitorViewModel, RefreshLoop> refreshLoops = [];

    /// <summary>Creates the home view.</summary>
    /// <param name="model">Retained favorite state.</param>
    /// <param name="monitor">Shared single-stop monitor.</param>
    /// <param name="map">Validated favorite map snapshots.</param>
    /// <param name="settings">Persisted refresh interval.</param>
    /// <param name="foreground">Active window state.</param>
    /// <param name="freshness">Shared realtime age policy.</param>
    public HomePage(FavoriteHomeViewModel model, StopMonitorViewModel monitor, MapViewModel map,
        RefreshSettingsViewModel settings, ForegroundState foreground, RefreshFreshness freshness)
    {
        this.model = model;
        this.monitor = monitor;
        this.settings = settings;
        this.foreground = foreground;
        this.freshness = freshness;
        BindingContext = model;
        Title = "Meine Haltestellen";
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        var primaryNavigation = new Button { Text = "Verbindung suchen", AutomationId = "OpenJourneySearch", Command = Navigate("//main/connections-tab/search") };
        SemanticProperties.SetDescription(primaryNavigation, "Verbindung suchen");
        layout.Children.Add(primaryNavigation);
        var showMap = new AsyncRelayCommand(async () => { map.ShowFavorites(model); await Shell.Current.GoToAsync("map"); },
            () => model.Cards.Count > 0, _ => Title = "Karte konnte nicht geöffnet werden");
        var shortcuts = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 8 };
        shortcuts.Add(TransitVisuals.SecondaryAction("＋", "Haltestelle hinzufügen", "OpenHomeStops", Navigate("//main/stations-tab/stops")), 0);
        shortcuts.Add(TransitVisuals.SecondaryAction("⌖", "Favoriten auf Karte zeigen", "HomeMap", showMap), 1);
        shortcuts.Add(TransitVisuals.SecondaryAction("↻", "Entfernungen aktualisieren", "SortFavorites", model.LocationCommand), 2);
        shortcuts.Add(TransitVisuals.SecondaryAction("⚙", "Aktualisierung einstellen", "OpenRefreshSettings", Navigate("refresh-settings")), 3);
        layout.Children.Add(shortcuts);
        var status = new Label { AutomationId = "HomeStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.Status));
        var location = new Label { AutomationId = "HomeLocationStatus" };
        location.SetBinding(Label.TextProperty, nameof(model.LocationStatus));
        layout.Children.Add(status);
        layout.Children.Add(location);
        layout.Children.Add(cards);
        layout.Children.Add(TransitVisuals.Text("Nächste Haltestellen", 22, true, "NearbyHeading"));
        var nearbyStatus = new Label { AutomationId = "NearbyStatus" }; nearbyStatus.SetBinding(Label.TextProperty, nameof(model.NearbyStatus)); layout.Children.Add(nearbyStatus);
        layout.Children.Add(nearbyCards);
        model.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(model.Cards))
            {
                RenderCards();
                if (active && foreground.IsActive && resume is null) _ = RefreshMissingAsync();
                ReconcileRefreshLoops();
            }
            status.IsVisible = ShowsHomeStatus();
            location.IsVisible = ShowsLocationStatus();
            nearbyStatus.IsVisible = ShowsNearbyStatus();
            RenderNearby();
            showMap.Refresh();
            foreach (var action in actions.ToArray()) action.Refresh();
        };
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") == "1") return;
            if (layout.Children.Any(child => child.AutomationId == "HomeScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "HomeScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario)); layout.Children.Insert(0, scenario);
            var calls = new Label { AutomationId = "FavoriteCalls", BindingContext = fixture };
            calls.SetBinding(Label.TextProperty, nameof(fixture.DepartureCalls)); layout.Children.Insert(1, calls);
        };
#endif
        Content = TransitVisuals.Page(layout);
        status.IsVisible = ShowsHomeStatus();
        location.IsVisible = ShowsLocationStatus();
        nearbyStatus.IsVisible = ShowsNearbyStatus();
        RenderNearby();
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
            RenderCards(); ReconcileRefreshLoops();
            if (version == appearance && foreground.IsActive) _ = RefreshAtStartupAsync(version);
            await model.RefreshNearbyAsync();
        }
        catch (Exception) { Title = "Favoriten konnten nicht geladen werden"; }
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        appearance++;
        active = false;
        resume?.Cancel();
        resume = null;
        settings.Changed -= SettingsChanged;
        foreground.PropertyChanged -= ForegroundChanged;
        ReconcileRefreshLoops();
        model.CancelPending();
        base.OnDisappearing();
    }

    private AsyncRelayCommand Navigate(string route) => new(() => Shell.Current.GoToAsync(route), () => true,
        _ => Title = "Seite konnte nicht geöffnet werden");

    private void SettingsChanged(object? sender, EventArgs args) => ReconcileRefreshLoops();

    private async void ForegroundChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (!foreground.IsActive)
        {
            resume?.Cancel();
            resume = null;
            ReconcileRefreshLoops();
            model.CancelPending();
            return;
        }
        if (!active || !settings.IsLoaded || settings.IntervalSeconds == 0 || resume is not null) return;
        using var source = new CancellationTokenSource();
        resume = source;
        try { await model.RefreshStaleAsync(freshness, source.Token); }
        catch (OperationCanceledException) { }
        catch (Exception) { Title = "Aktualisierung fehlgeschlagen"; }
        finally
        {
            if (ReferenceEquals(resume, source))
            {
                resume = null;
                ReconcileRefreshLoops();
            }
        }
    }

    private void ReconcileRefreshLoops()
    {
        var running = active && foreground.IsActive && settings.IsLoaded && resume is null;
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

    private async Task RefreshAtStartupAsync(long version)
    {
        try { await model.RefreshAtStartupAsync(); }
        catch (Exception) when (version == appearance) { Title = "Abfahrten konnten nicht aktualisiert werden"; }
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
            var header = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], ColumnSpacing = 8 };
            var title = TransitVisuals.Text(card.Title, 22, true, "FavoriteName" + index);
            header.Add(title, 0);
            var busy = new ActivityIndicator { AutomationId = "FavoriteBusy" + index, VerticalOptions = LayoutOptions.Center };
            busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(card.IsBusy));
            busy.SetBinding(IsVisibleProperty, nameof(card.IsBusy));
            SemanticProperties.SetDescription(busy, "Abfahrten werden aktualisiert");
            header.Add(busy, 1);
            layout.Children.Add(header);
            AddLabel(layout, "FavoriteDistance" + index, nameof(card.DistanceLabel));
            var status = new Label { AutomationId = "FavoriteStatus" + index };
            status.SetBinding(Label.TextProperty, nameof(card.Status));
            status.IsVisible = ShowsFavoriteStatus(card) && !card.IsBusy;
            layout.Children.Add(status);
            var summary = new HorizontalStackLayout { Spacing = 8, AutomationId = "FavoriteLines" + index };
            var summaryContainer = new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = summary };
            void RenderSummary()
            {
                summary.Children.Clear();
                var descriptions = new List<string>();
                foreach (var line in card.Lines)
                {
                    var matching = card.Items.Where(item => (item.Line?.Name ?? item.Identity.Line) == line).ToArray();
                    var next = matching
                        .Select(item => item.Realtime.ActualTime ?? item.PlannedTime)
                        .Where(time => time is not null && time >= DateTimeOffset.Now).OrderBy(time => time).FirstOrDefault();
                    var text = next is { } time ? line + "  " + TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin")).ToString("HH:mm") : line;
                    summary.Children.Add(TransitVisuals.Badge(text, matching.FirstOrDefault()?.Line?.Mode));
                    descriptions.Add(text);
                }
                SemanticProperties.SetDescription(summary, string.Join(" · ", descriptions));
            }
            layout.Children.Add(summaryContainer);
            var departures = new VerticalStackLayout { Spacing = 8 };
            void RenderDepartures()
            {
                departures.Children.Clear();
                for (var item = 0; item < Math.Min(card.Items.Count, 5); item++)
                    departures.Children.Add(new DepartureCardView(card.Items[item], $"FavoriteDeparture{cardIndex}_{item}", compact: true));
            }
            PropertyChangedEventHandler changed = (_, args) =>
            {
                if (args.PropertyName is nameof(card.Items) or nameof(card.Lines)) { RenderDepartures(); RenderSummary(); }
                if (args.PropertyName is nameof(card.Status) or nameof(card.IsBusy) or nameof(card.Result) or nameof(card.IsExpanded))
                {
                    status.IsVisible = ShowsFavoriteStatus(card) && !card.IsBusy;
                    departures.IsVisible = card.IsExpanded;
                    summaryContainer.IsVisible = !card.IsExpanded;
                }
            };
            card.PropertyChanged += changed;
            unsubscribe.Add(() => card.PropertyChanged -= changed);
            RenderSummary(); RenderDepartures(); departures.IsVisible = card.IsExpanded; summaryContainer.IsVisible = !card.IsExpanded; layout.Children.Add(departures);
            var actions = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)], ColumnSpacing = 8 };
            var expand = new Button { Text = card.IsExpanded ? "⌃" : "⌄", AutomationId = "ToggleFavorite" + index, HeightRequest = 48, WidthRequest = 48 };
            SemanticProperties.SetDescription(expand, card.IsExpanded ? "Abfahrten einklappen" : "Abfahrten aufklappen");
            expand.Command = new AsyncRelayCommand(() => { card.ToggleExpanded(); expand.Text = card.IsExpanded ? "⌃" : "⌄"; SemanticProperties.SetDescription(expand, card.IsExpanded ? "Abfahrten einklappen" : "Abfahrten aufklappen"); return Task.CompletedTask; }, () => true, _ => Title = "Abfahrten konnten nicht ein- oder ausgeklappt werden");
            actions.Add(expand, 0);
            unsubscribe.Add(() => expand.Command = null);
            var refreshButton = new Button { Text = "↻", AutomationId = "RefreshFavorite" + index, Command = card.RefreshCommand, HeightRequest = 48, WidthRequest = 48 };
            SemanticProperties.SetDescription(refreshButton, "Abfahrten aktualisieren");
            actions.Add(refreshButton, 1);
            unsubscribe.Add(() => refreshButton.Command = null);
            var open = new AsyncRelayCommand(() => monitor.OpenFavoriteAsync(model, card), () => model.Contains(card),
                _ => Title = "Monitor konnte nicht geöffnet werden");
            var remove = new AsyncRelayCommand(() => model.RemoveAsync(card), () => !model.IsSaving && model.Contains(card),
                _ => Title = "Favorit konnte nicht entfernt werden");
            // Cards render while a removal still runs (IsSaving true); without
            // this re-evaluation the fresh remove button stays disabled forever.
            PropertyChangedEventHandler savingChanged = (_, args) =>
            { if (args.PropertyName == nameof(model.IsSaving)) remove.InvalidateExecution(); };
            model.PropertyChanged += savingChanged;
            unsubscribe.Add(() => model.PropertyChanged -= savingChanged);
            var openButton = new Button { Text = "▣", AutomationId = "OpenFavorite" + index, Command = open, HeightRequest = 48, WidthRequest = 48 };
            SemanticProperties.SetDescription(openButton, "Abfahrtsmonitor öffnen");
            actions.Add(openButton, 2);
            unsubscribe.Add(() => openButton.Command = null);
            var removeButton = new Button { Text = "☆", AutomationId = "RemoveFavorite" + index, Command = remove, HeightRequest = 48, WidthRequest = 48 };
            SemanticProperties.SetDescription(removeButton, "Favorit entfernen");
            actions.Add(removeButton, 3);
            layout.Children.Add(actions);
            unsubscribe.Add(() => removeButton.Command = null);
            cards.Children.Add(new Border { Padding = 16, Content = layout });
            TransitVisuals.ApplyRoles(layout);
        }
    }

    private void RenderNearby()
    {
        nearbyCards.Children.Clear();
        for (var index = 0; index < model.NearbyStops.Count; index++)
        {
            var candidate = model.NearbyStops[index];
            var button = new Button { AutomationId = "NearbyStop" + index, HeightRequest = 48, HorizontalOptions = LayoutOptions.Fill };
            SemanticProperties.SetDescription(button, "Nahe Haltestelle " + candidate.Name);
            button.Command = new AsyncRelayCommand(() => monitor.OpenNearbyFromHomeAsync(candidate), () => candidate.Stop is not null,
                _ => Title = "Haltestelle konnte nicht geöffnet werden");
            nearbyCards.Children.Add(TransitVisuals.Candidate(candidate, button));
        }
    }

    private static void AddLabel(VerticalStackLayout layout, string id, string property, double size = 14)
    {
        var label = new Label { AutomationId = id, FontSize = size };
        label.SetBinding(Label.TextProperty, property); layout.Children.Add(label);
    }

    private bool ShowsHomeStatus() => model.Cards.Count == 0 || model.Status.Contains("konnten nicht", StringComparison.OrdinalIgnoreCase)
        || model.Status.Contains("keine vollständige", StringComparison.OrdinalIgnoreCase)
        || model.Status.Contains("maximal", StringComparison.OrdinalIgnoreCase);

    private bool ShowsLocationStatus() => model.IsLocating || (!model.LocationStatus.StartsWith("Entfernungen unbekannt", StringComparison.OrdinalIgnoreCase)
        && !model.LocationStatus.StartsWith("Nach Luftlinie sortiert", StringComparison.OrdinalIgnoreCase)
        && !model.LocationStatus.StartsWith("Standortabfrage abgebrochen", StringComparison.OrdinalIgnoreCase));

    private bool ShowsNearbyStatus() => model.NearbyStops.Count == 0 || model.NearbyStatus.Contains("werden geladen", StringComparison.OrdinalIgnoreCase)
        || model.NearbyStatus.Contains("wird ermittelt", StringComparison.OrdinalIgnoreCase)
        || model.NearbyStatus.Contains("nicht verfügbar", StringComparison.OrdinalIgnoreCase)
        || model.NearbyStatus.Contains("nicht geladen", StringComparison.OrdinalIgnoreCase);

    private static bool ShowsFavoriteStatus(FavoriteMonitorViewModel card) => card.Status.Contains("Keine nächsten", StringComparison.OrdinalIgnoreCase)
        || card.Status.Contains("Letzter Stand", StringComparison.OrdinalIgnoreCase)
        || card.Status.Contains("unvollständig", StringComparison.OrdinalIgnoreCase)
        || card.Status.Contains("fehlgeschlagen", StringComparison.OrdinalIgnoreCase)
        || card.Status.Contains("nicht geladen", StringComparison.OrdinalIgnoreCase);
}
