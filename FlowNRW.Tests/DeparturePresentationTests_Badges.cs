using FlowNRW.Core.Presentation;

namespace FlowNRW.Tests;

/// <summary>Supplied transport modes and unambiguous fallback labels retain readable badges.</summary>
public sealed class DeparturePresentationTests_Badges
{
    /// <summary>Uses provider modes first without inventing a mode for numeric or ambiguous names.</summary>
    /// <param name="name">Delivered line name.</param>
    /// <param name="mode">Delivered transport mode.</param>
    /// <param name="expected">Expected accessible color.</param>
    [Theory]
    [InlineData("107", "tram", "#B33F00")]
    [InlineData("107", null, "#414755")]
    [InlineData("RE 1", "bus", "#6F2C91")]
    [InlineData("F1", "ferry", "#00777F")]
    [InlineData("RE 1", null, "#BA1B1D")]
    [InlineData("RB32", null, "#BA1B1D")]
    [InlineData("S 6", null, "#007A39")]
    [InlineData("U79", null, "#005A9C")]
    [InlineData("REISE", null, "#414755")]
    [InlineData("Regionalexpress", "unknown", "#414755")]
    public void BadgeColor_SuppliedModeOrRecognizedName_DoesNotInventCategory(string name, string? mode, string expected)
        => Assert.Equal(expected, DeparturePresentation.BadgeColor(name, mode));

    /// <summary>Every transport badge supports normal-sized white text at WCAG AA contrast.</summary>
    /// <param name="mode">Transport mode to verify.</param>
    [Theory]
    [InlineData("rail")]
    [InlineData("suburban")]
    [InlineData("subway")]
    [InlineData("tram")]
    [InlineData("bus")]
    [InlineData("ferry")]
    [InlineData("unknown")]
    public void BadgeColor_WhiteText_DoesNotFallBelowNormalTextContrast(string mode)
    {
        var color = DeparturePresentation.BadgeColor("107", mode);
        static double Linear(int channel)
        {
            var component = channel / 255d;
            return component <= 0.04045 ? component / 12.92 : Math.Pow((component + 0.055) / 1.055, 2.4);
        }
        var luminance = 0.2126 * Linear(Convert.ToInt32(color.Substring(1, 2), 16))
            + 0.7152 * Linear(Convert.ToInt32(color.Substring(3, 2), 16))
            + 0.0722 * Linear(Convert.ToInt32(color.Substring(5, 2), 16));
        Assert.True(1.05 / (luminance + 0.05) >= 4.5, $"White on {color} must reach 4.5:1");
    }
}
