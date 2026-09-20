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

    /// <summary>Creates the interval settings page.</summary>
    /// <param name="model">Persisted shared refresh preference.</param>
    public RefreshSettingsPage(RefreshSettingsViewModel model)
    {
        this.model = model;
        BindingContext = model;
        Title = "Automatische Aktualisierung";
        interval = new Picker
        {
            Title = "Aktualisierungsintervall", AutomationId = "RefreshInterval",
            ItemsSource = new[] { "Aus", "30 Sekunden", "60 Sekunden", "2 Minuten", "5 Minuten" }
        };
        interval.SelectedIndexChanged += (_, _) =>
        {
            if (interval.SelectedIndex >= 0) model.SelectedSeconds = Values[interval.SelectedIndex];
        };
        save = new AsyncRelayCommand(model.SaveAsync, () => !model.IsSaving,
            _ => Title = "Einstellung konnte nicht gespeichert werden");
        var status = new Label { AutomationId = "RefreshSettingsStatus" };
        status.SetBinding(Label.TextProperty, nameof(model.Status));
        var current = new Label { AutomationId = "RefreshIntervalStatus" };
        current.SetBinding(Label.TextProperty, nameof(model.Description));
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16, Spacing = 16,
                Children =
                {
                    new Label { Text = "Abfahrten automatisch laden", FontSize = 24, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "Gilt für geöffnete Abfahrtsmonitore und Favoriten, solange die App aktiv ist. Manuell aktualisieren bleibt jederzeit möglich." },
                    interval,
                    new Button { Text = "Speichern", AutomationId = "SaveRefreshSettings", Command = save },
                    status, current,
                    new Label { Text = "Standard: 60 Sekunden. Mindestens 30 Sekunden begrenzen die Datenabrufe. Während eines laufenden Abrufs wird keine zweite Anfrage gestartet." }
                }
            }
        };
#if UI_TEST_FIXTURES
        Loaded += (_, _) =>
        {
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
        save.Refresh();
    }
}
