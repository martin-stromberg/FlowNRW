namespace FlowNRW;

/// <summary>
/// Central icon definitions for Material Symbols Rounded (FILL0, opsz 24, wght 400).
/// All codepoints were verified against the .codepoints manifest of the embedded
/// <c>MaterialSymbolsRounded.ttf</c> (kept at docs/design/licenses/MaterialSymbolsRounded.codepoints).
/// Filled variants (for example the saved-favorite star) share the same codepoint and
/// switch the glyph shape through <see cref="FilledFontFamily"/> (FILL1 instance).
/// </summary>
internal static class TransitIcons
{
    /// <summary>Font alias of the outlined icon font (Material Symbols Rounded, FILL0, opsz 24).</summary>
    internal const string FontFamily = "MaterialSymbols";

    /// <summary>Font alias of the filled icon font instance (Material Symbols Rounded, FILL1, opsz 24).</summary>
    internal const string FilledFontFamily = "MaterialSymbolsFilled";

    /// <summary>Glyph <c>add</c> (U+E145): Haltestelle hinzufügen.</summary>
    internal const string Add = "\uE145";

    /// <summary>Glyph <c>my_location</c> (U+E55C): Standortbezogene Aktionen.</summary>
    internal const string MyLocation = "\uE55C";

    /// <summary>Glyph <c>refresh</c> (U+E5D5): Aktualisieren.</summary>
    internal const string Refresh = "\uE5D5";

    /// <summary>Glyph <c>settings</c> (U+E8B8): Einstellungen öffnen.</summary>
    internal const string Settings = "\uE8B8";

    /// <summary>Glyph <c>expand_less</c> (U+E5CE): Bereich einklappen.</summary>
    internal const string ExpandLess = "\uE5CE";

    /// <summary>Glyph <c>expand_more</c> (U+E5CF): Bereich aufklappen.</summary>
    internal const string ExpandMore = "\uE5CF";

    /// <summary>Glyph <c>star</c> (U+F09A): gefüllter Stern — mit <see cref="FilledFontFamily"/> rendern.</summary>
    internal const string Star = "\uF09A";

    /// <summary>Glyph <c>star_outline</c> (U+F09A): Umriss-Stern, gleicher Codepoint in der FILL0-Font.</summary>
    internal const string StarOutline = "\uF09A";

    /// <summary>Glyph <c>search</c> (U+EF7A): Suchen.</summary>
    internal const string Search = "\uEF7A";

    /// <summary>Glyph <c>check</c> (U+E668): Bestätigen/Übernehmen.</summary>
    internal const string Check = "\uE668";

    /// <summary>Glyph <c>swap_vert</c> (U+E8D5): Start und Ziel vertauschen.</summary>
    internal const string SwapVert = "\uE8D5";

    /// <summary>Glyph <c>info</c> (U+E88E): Informationen ein-/ausblenden.</summary>
    internal const string Info = "\uE88E";

    /// <summary>Glyph <c>map</c> (U+E55B): Auf Karte zeigen.</summary>
    internal const string Map = "\uE55B";

    /// <summary>Glyph <c>schedule</c> (U+EFD6): Verspätungs-Status.</summary>
    internal const string Schedule = "\uEFD6";

    /// <summary>Glyph <c>cancel</c> (U+E888): Ausfall-Status.</summary>
    internal const string Cancel = "\uE888";

    /// <summary>Glyph <c>error</c> (U+F8B6): Fehler-Status.</summary>
    internal const string Error = "\uF8B6";

    /// <summary>Glyph <c>departure_board</c> (U+E576): Abfahrtsmonitor.</summary>
    internal const string DepartureBoard = "\uE576";

    /// <summary>Glyph <c>route</c> (U+EACD): Verbindungsverlauf.</summary>
    internal const string Route = "\uEACD";

    /// <summary>Glyph <c>location_on</c> (U+F1DB): Haltestellen/Ort.</summary>
    internal const string LocationOn = "\uF1DB";

    /// <summary>
    /// Sets an icon glyph on a button, keeping glyph, spacing and accessibility description consistent.
    /// Clears the button padding so the glyph is not clipped inside fixed-size icon buttons.
    /// </summary>
    /// <param name="button">Button carrying the icon glyph as its text.</param>
    /// <param name="glyph">One of the <see cref="TransitIcons"/> glyph constants.</param>
    /// <param name="description">Optional accessible description applied together with the glyph.</param>
    /// <param name="filled">Whether the FILL1 font instance renders the glyph (for filled-state toggles).</param>
    /// <param name="size">Glyph size in device-independent units.</param>
    internal static void SetIcon(Button button, string glyph, string? description = null, bool filled = false, double size = 24)
    {
        button.Text = glyph;
        button.FontFamily = filled ? FilledFontFamily : FontFamily;
        button.FontSize = size;
        button.Padding = new Thickness(0);
        if (description is not null) SemanticProperties.SetDescription(button, description);
    }

    /// <summary>
    /// Creates a <see cref="FontImageSource"/> for an icon glyph next to regular button text.
    /// The color follows the themed secondary-action text color.
    /// </summary>
    /// <param name="glyph">One of the <see cref="TransitIcons"/> glyph constants.</param>
    /// <param name="filled">Whether the FILL1 font instance renders the glyph.</param>
    /// <param name="size">Icon size in device-independent units.</param>
    /// <returns>A themed font icon usable as <see cref="Button.ImageSource"/>.</returns>
    internal static FontImageSource IconImage(string glyph, bool filled = false, double size = 20)
    {
        var icon = new FontImageSource
        {
            Glyph = glyph,
            FontFamily = filled ? FilledFontFamily : FontFamily,
            Size = size
        };
        icon.SetAppThemeColor(FontImageSource.ColorProperty, TransitVisuals.Action, TransitVisuals.ActionDark);
        return icon;
    }

    /// <summary>
    /// Creates a <see cref="FontImageSource"/> for a shell tab icon in the themed interaction color.
    /// </summary>
    /// <param name="glyph">One of the <see cref="TransitIcons"/> glyph constants.</param>
    /// <returns>A themed font icon usable as the <see cref="Tab"/> icon.</returns>
    internal static FontImageSource TabIcon(string glyph)
    {
        return IconImage(glyph, size: 24);
    }
}
