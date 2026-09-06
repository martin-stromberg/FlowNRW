using FlowNRW.Core;

namespace FlowNRW.Tests;

/// <summary>
/// Tests for <see cref="ClickCounter"/>.
/// </summary>
public class ClickCounterTests
{
    /// <summary>Clicking twice yields count 2 and singular/plural captions.</summary>
    [Fact]
    public void Click_IncrementsCountAndReturnsCaption()
    {
        var counter = new ClickCounter();

        var first = counter.Click();
        var second = counter.Click();

        Assert.Equal(2, counter.Count);
        Assert.Equal("Clicked 1 time", first);
        Assert.Equal("Clicked 2 times", second);
    }

    /// <summary>The caption uses the singular form only for exactly one click.</summary>
    /// <param name="count">Click count under test.</param>
    /// <param name="expected">Expected caption.</param>
    [Theory]
    [InlineData(0, "Clicked 0 times")]
    [InlineData(1, "Clicked 1 time")]
    [InlineData(5, "Clicked 5 times")]
    public void FormatCaption_UsesSingularOnlyForOne(int count, string expected)
    {
        Assert.Equal(expected, ClickCounter.FormatCaption(count));
    }

    /// <summary>Negative counts are rejected.</summary>
    [Fact]
    public void FormatCaption_RejectsNegativeCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClickCounter.FormatCaption(-1));
    }
}
