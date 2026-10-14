using Microsoft.Maui.Controls.Shapes;
using System.Windows.Input;

namespace FlowNRW;

/// <summary>Shared native text and surface roles from the accepted design reference.</summary>
internal static class TransitVisuals
{
    /// <summary>Interaction color on light surfaces (token <c>Primary</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color Action = Color.FromArgb("#0058BC");

    /// <summary>Interaction color on dark surfaces (token <c>PrimaryDark</c>, icon/button text on <see cref="GroupStrongDark"/>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color ActionDark = Color.FromArgb("#A5C9FF");

    /// <summary>Primary text on light surfaces (token <c>InkLight</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color Ink = Color.FromArgb("#1A1B1F");

    /// <summary>Primary text on dark surfaces (token <c>InkDark</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color InkDark = Color.FromArgb("#F5F5F7");

    /// <summary>Secondary text on light surfaces (token <c>MutedLight</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color Muted = Color.FromArgb("#414755");

    /// <summary>Secondary text on dark surfaces (token <c>MutedDark</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color MutedDark = Color.FromArgb("#C5C7D0");

    /// <summary>Stronger grouping surface on light canvas (token <c>GroupStrong</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color GroupStrong = Color.FromArgb("#EEEDF3");

    /// <summary>Stronger grouping surface on dark canvas (token <c>GroupStrongDark</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color GroupStrongDark = Color.FromArgb("#2C2C2E");

    /// <summary>Confirmation semantics on light surfaces (token <c>Success</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color Success = Color.FromArgb("#006E28");

    /// <summary>Confirmation semantics on dark surfaces (token <c>SuccessDark</c>, ≥4.5:1 on card/group surfaces).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color SuccessDark = Color.FromArgb("#3ECF6B");

    /// <summary>Warning semantics on light surfaces (token <c>Warning</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color Warning = Color.FromArgb("#894D00");

    /// <summary>Warning semantics on dark surfaces (token <c>WarningDark</c>, ≥4.5:1 on card/group surfaces).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color WarningDark = Color.FromArgb("#F0A840");

    /// <summary>Error semantics on light surfaces (token <c>Error</c>).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color Error = Color.FromArgb("#BA1A1A");

    /// <summary>Error semantics on dark surfaces (token <c>ErrorDark</c>, ≥4.5:1 on card/group surfaces).</summary>
    /// <value>Design-token-backed color.</value>
    internal static readonly Color ErrorDark = Color.FromArgb("#FF897D");

    internal static Border Candidate(FlowNRW.Core.Transit.Address candidate, Button action, string suffix = "", bool showTechnicalIdentity = true)
    {
        var description = (showTechnicalIdentity ? FlowNRW.Core.Presentation.JourneyPresentation.Address(candidate) : candidate.Name) + suffix;
        action.Text = candidate.Name + suffix;
        action.Padding = new Thickness(0);
        action.SetAppThemeColor(Button.BackgroundColorProperty, Colors.Transparent, Colors.Transparent);
        action.SetAppThemeColor(Button.TextColorProperty, Ink, InkDark);
        SemanticProperties.SetDescription(action, description);
        var details = description.StartsWith(candidate.Name, StringComparison.Ordinal) ? description[candidate.Name.Length..].Trim(' ', '·') : description;
        var layout = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(2) };
        layout.Children.Add(action);
        if (!string.IsNullOrWhiteSpace(details)) layout.Children.Add(Secondary(details));
        return new Border { Padding = 16, Content = layout };
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
            label.SetAppThemeColor(Label.TextColorProperty, Muted, MutedDark);
        }
        if (element is Button button && button.AutomationId is { } id && !new[] { "SearchJourneys", "FindStops", "RefreshDepartures", "SaveRefreshSettings", "OpenJourneySearch" }.Contains(id))
        {
            button.SetAppThemeColor(Button.BackgroundColorProperty, GroupStrong, GroupStrongDark);
            button.SetAppThemeColor(Button.TextColorProperty, Action, ActionDark);
        }
        foreach (var child in ((IVisualTreeElement)element).GetVisualChildren().OfType<Element>()) ApplyRoles(child);
    }

    internal static Label Text(string text, double size = 17, bool bold = false, string? id = null)
    {
        return new Label
        {
            Text = text,
            FontSize = size,
            FontFamily = bold ? Fonts.InterBold : Fonts.Inter,
            AutomationId = id,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    internal static Label Secondary(string text, string? id = null)
    {
        var label = Text(text, 13, id: id);
        label.SetAppThemeColor(Label.TextColorProperty, Muted, MutedDark);
        return label;
    }

    /// <summary>Creates the struck-through planned time shown when the reported actual time deviates.</summary>
    /// <param name="text">Formatted planned time.</param>
    /// <param name="id">Optional accessible identifier.</param>
    /// <returns>A muted label with strikethrough decoration in the shared secondary size.</returns>
    internal static Label StrikethroughTime(string text, string? id = null)
    {
        var label = Secondary(text, id);
        label.TextDecorations = TextDecorations.Strikethrough;
        return label;
    }

    /// <summary>Creates a small inline icon label from a <see cref="TransitIcons"/> glyph.</summary>
    /// <param name="glyph">Icon codepoint from the embedded Material Symbols Rounded font.</param>
    /// <returns>A label rendering the icon glyph; callers assign the themed text color.</returns>
    internal static Label IconLabel(string glyph)
    {
        return new Label
        {
            Text = glyph,
            FontFamily = TransitIcons.FontFamily,
            FontSize = 16,
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    /// <summary>Creates a status chip combining an icon glyph with text; color is never the only carrier.</summary>
    /// <param name="glyph">Icon codepoint from <see cref="TransitIcons"/>.</param>
    /// <param name="text">Short status wording, for example a delay or a cancellation.</param>
    /// <param name="light">Semantic color of icon and text on light surfaces.</param>
    /// <param name="dark">Semantic color of icon and text on dark surfaces.</param>
    /// <returns>A capsule-shaped border containing the icon and the status text.</returns>
    internal static Border StatusChip(string glyph, string text, Color light, Color dark)
    {
        var icon = IconLabel(glyph);
        icon.SetAppThemeColor(Label.TextColorProperty, light, dark);
        var label = Text(text, 13, true);
        label.SetAppThemeColor(Label.TextColorProperty, light, dark);
        var chip = new Border
        {
            Padding = new Thickness(10, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            HorizontalOptions = LayoutOptions.Start,
            Content = new HorizontalStackLayout { Spacing = 6, Children = { icon, label } }
        };
        chip.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromRgba(light.Red, light.Green, light.Blue, 0.14f),
            Color.FromRgba(dark.Red, dark.Green, dark.Blue, 0.22f));
        return chip;
    }

    internal static Button SecondaryAction(string icon, string description, string automationId, ICommand command)
    {
        var button = new Button
        {
            AutomationId = automationId,
            Command = command,
            HeightRequest = 48,
            WidthRequest = 48
        };
        TransitIcons.SetIcon(button, icon, description);
        return button;
    }

    internal static Border Badge(string line, string? mode = null)
    {
        var color = FlowNRW.Core.Presentation.DeparturePresentation.BadgeColor(line, mode);
        return new Border
        {
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 7 },
            BackgroundColor = Color.FromArgb(color),
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = line,
                TextColor = Colors.White,
                FontSize = 13,
                FontFamily = Fonts.InterBold,
                // A badge is a compact unit. Let its containing row allocate the
                // available width instead of splitting its label across lines.
                LineBreakMode = LineBreakMode.TailTruncation
            }
        };
    }
}
