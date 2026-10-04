using FlowNRW.Core.Presentation;
using FlowNRW.Core.Refresh;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace FlowNRW;

/// <summary>Explicitly saved foreground refresh interval.</summary>
public sealed class RefreshSettingsPage : ContentPage
{
    private static readonly int[] Values = [0, 30, 60, 120, 300];
    private readonly RefreshSettingsViewModel model;
    private readonly Picker interval;
    private readonly AsyncRelayCommand save;
    private readonly Label status;

    /// <summary>Creates the interval settings page.</summary>
    /// <param name="model">Persisted shared refresh preference.</param>
    public RefreshSettingsPage(RefreshSettingsViewModel model)
    {
        this.model = model;
        BindingContext = model;
        Title = "Automatische Aktualisierung";
        interval = new Picker
        {
            Title = "Aktualisierungsintervall",
            AutomationId = "RefreshInterval",
            ItemsSource = new[] { "Aus", "30 Sekunden", "60 Sekunden", "2 Minuten", "5 Minuten" }
        };
        interval.SelectedIndexChanged += (_, _) =>
        {
            if (interval.SelectedIndex >= 0) model.SelectedSeconds = Values[interval.SelectedIndex];
        };
        save = new AsyncRelayCommand(model.SaveAsync, () => !model.IsSaving,
            _ => Title = "Einstellung konnte nicht gespeichert werden");
        status = new Label { AutomationId = "RefreshSettingsStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.Status));
        status.IsVisible = ShowsStatus();
        var current = new Label { AutomationId = "RefreshIntervalStatus" };
        current.SetBinding(Label.TextProperty, nameof(model.Description));
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 16,
                Children =
                {
                    new Border { Padding = 16, Content = new VerticalStackLayout { Spacing = 12, Children = { TransitVisuals.Text("Abfahrten automatisch laden", 22, true), interval } } },
                    new Button { Text = "Speichern", AutomationId = "SaveRefreshSettings", Command = save },
                    status, current
                }
            }
        };
        Content = TransitVisuals.Page((VerticalStackLayout)((ScrollView)Content).Content);
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
            if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_HIDE_CONTROLS") == "1") return;
            var layout = (VerticalStackLayout)((ScrollView)Content).Content;
            if (layout.Children.Any(child => child.AutomationId == "RefreshSettingsScenario")) return;
            var fixture = Handler!.MauiContext!.Services.GetRequiredService<UiTestLocationServices>();
            var scenario = new Entry { AutomationId = "RefreshSettingsScenario", BindingContext = fixture };
            scenario.SetBinding(Entry.TextProperty, nameof(fixture.Scenario)); layout.Children.Insert(0, scenario);
        };
#endif
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        model.PropertyChanged += ModelChanged;
        await model.LoadAsync();
        interval.SelectedIndex = Array.IndexOf(Values, model.IntervalSeconds);
        save.Refresh();
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        model.PropertyChanged -= ModelChanged;
        base.OnDisappearing();
    }

    private void ModelChanged(object? sender, PropertyChangedEventArgs args)
    {
        interval.IsEnabled = !model.IsSaving;
        if (args.PropertyName is nameof(model.Status) or nameof(model.IsSaving) or nameof(model.IsLoaded))
            status.IsVisible = ShowsStatus();
        save.Refresh();
    }

    private bool ShowsStatus() => !model.IsLoaded || model.IsSaving
        || model.Status.Contains("konnte nicht", StringComparison.OrdinalIgnoreCase)
        || model.Status.StartsWith("Bitte ", StringComparison.Ordinal)
        || model.Status.Contains("gespeichert", StringComparison.OrdinalIgnoreCase);
}
