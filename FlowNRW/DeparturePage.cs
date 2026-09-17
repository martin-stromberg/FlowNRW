using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Manually refreshed departure board retaining the last known data on errors.</summary>
public sealed class DeparturePage : ContentPage
{
    private readonly StopMonitorViewModel model;
    private readonly VerticalStackLayout items = new() { Spacing = 12 };

    /// <summary>Creates the departure board.</summary>
    /// <param name="model">Shared monitor session.</param>
    public DeparturePage(StopMonitorViewModel model)
    {
        this.model = model;
        BindingContext = model;
        SetBinding(TitleProperty, new Binding(nameof(model.Title)));
        var layout = new VerticalStackLayout { Padding = 16, Spacing = 16 };
        var stop = new Label { FontSize = 24, FontAttributes = FontAttributes.Bold, AutomationId = "MonitorStop" };
        stop.SetBinding(Label.TextProperty, nameof(model.Title));
        layout.Children.Add(stop);
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
        RenderItems();
    }

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        model.PropertyChanged -= ModelChanged;
        model.CancelPending();
        base.OnDisappearing();
    }

    private void ModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(model.Items)) RenderItems();
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
