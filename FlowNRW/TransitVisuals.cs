using Microsoft.Maui.Controls.Shapes;
using System.Windows.Input;

namespace FlowNRW;

/// <summary>Shared native text and surface roles from the accepted design reference.</summary>
internal static class TransitVisuals
{
    internal static Border Candidate(FlowNRW.Core.Transit.Address candidate, Button action, string suffix = "")
    {
        var description = FlowNRW.Core.Presentation.JourneyPresentation.Address(candidate) + suffix;
        action.Text = candidate.Name + suffix;
        action.Padding = new Thickness(0);
        action.SetAppThemeColor(Button.BackgroundColorProperty, Colors.Transparent, Colors.Transparent);
        action.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb("#1A1B1F"), Color.FromArgb("#F5F5F7"));
        SemanticProperties.SetDescription(action, description);
        var details = description.StartsWith(candidate.Name, StringComparison.Ordinal) ? description[candidate.Name.Length..].Trim(' ', '·') : description;
        var layout = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(2) };
        layout.Children.Add(action);
        if (!string.IsNullOrWhiteSpace(details)) layout.Children.Add(Secondary(details));
        return new Border { Padding = 14, Content = layout };
    }

    internal static ScrollView Page(VerticalStackLayout layout)
    {
        layout.MaximumWidthRequest = 840;
        layout.HorizontalOptions = LayoutOptions.Fill;
        var scroll = new ScrollView { Content = layout };
        scroll.SizeChanged += (_, _) => layout.Padding = scroll.Width >= 640 ? 24 : 16;
        ApplyRoles(layout);
        return scroll;
    }

    internal static void ApplyRoles(Element element)
    {
        if (element is Label label && (label.AutomationId?.Contains("Metadata", StringComparison.Ordinal) == true
            || label.AutomationId?.EndsWith("Status", StringComparison.Ordinal) == true
            || label.AutomationId == "RefreshIntervalStatus"))
        {
            label.FontSize = 13;
            label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#414755"), Color.FromArgb("#C5C7D0"));
        }
        if (element is Button button && button.AutomationId is { } id && !new[] { "SearchJourneys", "FindStops", "RefreshDepartures", "SaveRefreshSettings", "OpenJourneySearch" }.Contains(id))
        {
            button.SetAppThemeColor(Button.BackgroundColorProperty, Color.FromArgb("#EEEDF3"), Color.FromArgb("#2C2C2E"));
            button.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb("#0058BC"), Color.FromArgb("#A8C8FF"));
        }
        foreach (var child in ((IVisualTreeElement)element).GetVisualChildren().OfType<Element>()) ApplyRoles(child);
    }

    internal static Label Text(string text, double size = 17, bool bold = false, string? id = null)
    {
        return new Label
        {
            Text = text, FontSize = size, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
            AutomationId = id, LineBreakMode = LineBreakMode.WordWrap
        };
    }

    internal static Label Secondary(string text, string? id = null)
    {
        var label = Text(text, 13, id: id);
        label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#414755"), Color.FromArgb("#C5C7D0"));
        return label;
    }

    internal static Button SecondaryAction(string icon, string description, string automationId, ICommand command)
    {
        var button = new Button
        {
            Text = icon,
            AutomationId = automationId,
            Command = command,
            HeightRequest = 48,
            WidthRequest = 48,
            Padding = new Thickness(0)
        };
        SemanticProperties.SetDescription(button, description);
        return button;
    }

    internal static Border Badge(string line, string? mode = null)
    {
        var color = FlowNRW.Core.Presentation.DeparturePresentation.BadgeColor(line, mode);
        return new Border
        {
            Padding = new Thickness(8, 4), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 7 },
            BackgroundColor = Color.FromArgb(color), VerticalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 120,
            Content = new Label { Text = line, TextColor = Colors.White, FontSize = 13, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.WordWrap }
        };
    }
}
