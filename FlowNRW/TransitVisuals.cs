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

    /// <summary>Wraps a full-width candidate button and its optional detail line without technical identifiers.</summary>
    /// <param name="candidate">Complete provider candidate.</param>
    /// <param name="action">Prepared select button; text, padding and accessible description are applied here.</param>
    /// <param name="suffix">Additional detail appended to the description, for example a distance.</param>
    /// <param name="detailsId">Optional accessible identifier of the secondary detail line.</param>
    /// <returns>A bordered card containing the button and, when the suffix carries content, the detail line.</returns>
    internal static Border Candidate(FlowNRW.Core.Transit.Address candidate, Button action, string suffix = "", string? detailsId = null)
    {
        var displayName = FlowNRW.Core.Presentation.JourneyPresentation.DisplayName(candidate.Name);
        var description = displayName + suffix;
        action.Text = description;
        action.Padding = new Thickness(0);
        action.SetAppThemeColor(Button.BackgroundColorProperty, Colors.Transparent, Colors.Transparent);
        action.SetAppThemeColor(Button.TextColorProperty, Ink, InkDark);
        SemanticProperties.SetDescription(action, description);
        var details = description.StartsWith(displayName, StringComparison.Ordinal) ? description[displayName.Length..].Trim(' ', '·') : description;
        var layout = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(2) };
        layout.Children.Add(action);
        if (!string.IsNullOrWhiteSpace(details)) layout.Children.Add(Secondary(details, detailsId));
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

    /// <summary>A 48×48 warning button that reveals an accompanying provenance label on demand.</summary>
    internal sealed class NoticeToggle
    {
        /// <summary>Creates the toggle pair for one data-notice location.</summary>
        /// <param name="noticeId">Accessible identifier of the toggle button.</param>
        /// <param name="metadataId">Accessible identifier of the expandable provenance label.</param>
        internal NoticeToggle(string noticeId, string metadataId)
        {
            Metadata = new Label { AutomationId = metadataId, IsVisible = false };
            Button = new Button { AutomationId = noticeId, HeightRequest = 48, WidthRequest = 48 };
            TransitIcons.SetIcon(Button, TransitIcons.Warning, "Hinweis zu den Daten anzeigen");
            Button.Command = new FlowNRW.Core.Presentation.RelayCommand(_ =>
            {
                Metadata.IsVisible = !Metadata.IsVisible;
                SemanticProperties.SetDescription(Button, Metadata.IsVisible ? "Hinweis schließen" : "Hinweis zu den Daten anzeigen");
            });
        }

        /// <summary>Toggle button showing the warning glyph.</summary>
        /// <value>The themed warning color is applied via <see cref="ApplyWarningColors"/> after <see cref="ApplyRoles(Element)"/>.</value>
        internal Button Button { get; }
        /// <summary>Expandable provenance detail revealed by the button.</summary>
        /// <value>Initially hidden label.</value>
        internal Label Metadata { get; }

        /// <summary>Shows the button only while a notice exists and collapses the detail whenever it is hidden.</summary>
        /// <param name="visible">Whether a data-quality notice is currently presentable.</param>
        internal void Update(bool visible)
        {
            Button.IsVisible = visible;
            if (!visible && Metadata.IsVisible)
            {
                Metadata.IsVisible = false;
                SemanticProperties.SetDescription(Button, "Hinweis zu den Daten anzeigen");
            }
        }

        /// <summary>Applies the themed warning color to <see cref="Button"/>.</summary>
        /// <remarks>
        /// The shared button role in <see cref="ApplyRoles(Element)"/> recolors every button
        /// glyph; the warning semantics must survive, so call this only after the roles ran.
        /// </remarks>
        internal void ApplyWarningColors()
        {
            Button.SetAppThemeColor(Button.TextColorProperty, Warning, WarningDark);
        }
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
