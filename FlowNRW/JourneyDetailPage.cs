using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using System.ComponentModel;

namespace FlowNRW;

/// <summary>Native itinerary that renews expired data without silently selecting another journey.</summary>
public sealed class JourneyDetailPage : ContentPage
{
    private readonly JourneyDetailViewModel model;
    private readonly ForegroundState foreground;
    private readonly RefreshSettingsViewModel settings;
    private readonly RefreshFreshness freshness;
    private readonly VerticalStackLayout sections = new() { Spacing = 16, AutomationId = "JourneyDetails" };
    private readonly AsyncRelayCommand showMap;
    private bool active;

    /// <summary>Creates the detail page.</summary>
    /// <param name="model">Selected journey projection.</param>
    /// <param name="map">Shared map snapshot.</param>
    /// <param name="foreground">Shared active-window state.</param>
    /// <param name="settings">Committed automatic refresh preference.</param>
    /// <param name="freshness">Realtime age policy.</param>
    public JourneyDetailPage(JourneyDetailViewModel model, FlowNRW.Core.Maps.MapViewModel map,
        ForegroundState foreground, RefreshSettingsViewModel settings, RefreshFreshness freshness)
    {
        this.model = model; this.foreground = foreground; this.settings = settings; this.freshness = freshness;
        BindingContext = model; Title = "Verbindungsdetails";
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        showMap = new AsyncRelayCommand(async () =>
        {
            if (model.Session.SelectedJourney is { } journey)
            { map.ShowJourney(journey, model.Session.Metadata); await Shell.Current.GoToAsync("map"); }
        }, () => model.Session.SelectedJourney is not null, _ => Title = "Karte konnte nicht geöffnet werden");
        var mapButton = TransitVisuals.SecondaryAction("⌖", "Verlauf auf Karte öffnen", "ShowJourneyMap", showMap);
        layout.Children.Add(mapButton);
        var status = new Label { AutomationId = "DetailStatus", BindingContext = model.Session };
        status.SetBinding(Label.TextProperty, nameof(model.Session.Status));
        var metadata = new Label { AutomationId = "DetailMetadata", BindingContext = model.Session };
        metadata.SetBinding(Label.TextProperty, nameof(model.Session.Metadata));
        var sourceState = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(4, 0) };
        sourceState.Children.Add(status); sourceState.Children.Add(metadata);
        layout.Children.Add(sourceState);
        layout.Children.Add(sections);
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
        Content = TransitVisuals.Page(layout);
        RenderDetails();
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing(); active = true;
        foreground.PropertyChanged += ForegroundChanged;
        model.Session.PropertyChanged += SessionChanged;
        RenderDetails();
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
        if (args.PropertyName == nameof(model.Session.SelectedJourney)) RenderDetails();
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

    private void RenderDetails()
    {
        sections.Children.Clear();
        if (model.Session.SelectedJourney is { } journey)
        {
            sections.Children.Add(new JourneyTimelineView(journey));
            showMap.Refresh();
            return;
        }
        var index = 0;
        foreach (var section in model.Details.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
            sections.Children.Add(new Border { Padding = 16, Content = new Label { Text = section, FontSize = 18, AutomationId = "JourneyDetailSection" + index++ } });
        showMap.Refresh();
    }
}
