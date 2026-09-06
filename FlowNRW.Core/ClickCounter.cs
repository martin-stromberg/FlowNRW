namespace FlowNRW.Core;

/// <summary>
/// Counts button clicks and formats the caption shown on the counter button.
/// </summary>
public sealed class ClickCounter
{
    /// <summary>
    /// Gets the number of clicks registered so far.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Registers one click and returns the updated caption.
    /// </summary>
    /// <returns>The caption to show for the current click count.</returns>
    public string Click()
    {
        Count++;
        return FormatCaption(Count);
    }

    /// <summary>
    /// Formats the caption for a given click count.
    /// </summary>
    /// <param name="count">The number of clicks; must not be negative.</param>
    /// <returns>"Clicked 1 time" for exactly one click, otherwise "Clicked N times".</returns>
    public static string FormatCaption(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return count == 1 ? "Clicked 1 time" : $"Clicked {count} times";
    }
}
