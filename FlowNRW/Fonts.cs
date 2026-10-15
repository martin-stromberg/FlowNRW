namespace FlowNRW;

/// <summary>
/// Central aliases of the embedded text fonts registered in <see cref="MauiProgram"/>.
/// XAML styles repeat these aliases as literals; C# view helpers use the constants so a
/// typo fails at compile time instead of silently falling back to a system font.
/// </summary>
internal static class Fonts
{
    /// <summary>Font alias of the regular Inter text face (<c>Inter-Regular.ttf</c>).</summary>
    internal const string Inter = "Inter";

    /// <summary>Font alias of the semibold Inter text face (<c>Inter-SemiBold.ttf</c>).</summary>
    internal const string InterSemiBold = "InterSemiBold";

    /// <summary>Font alias of the bold Inter text face (<c>Inter-Bold.ttf</c>).</summary>
    internal const string InterBold = "InterBold";

    /// <summary>Font alias of the monospaced JetBrains Mono face for prominent times (<c>JetBrainsMono-Bold.ttf</c>).</summary>
    internal const string JetBrainsMono = "JetBrainsMono";
}
