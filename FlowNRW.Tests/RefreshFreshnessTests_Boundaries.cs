using FlowNRW.Core.Refresh;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Realtime freshness follows cache limits, including exact boundaries and invalid clocks.</summary>
public sealed class RefreshFreshnessTests_Boundaries
{
    /// <summary>Only data younger than the configured lifetime is fresh.</summary>
    /// <param name="ageSeconds">Signed age of retained data.</param>
    /// <param name="expected">Whether renewal is required.</param>
    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(29, false)]
    [InlineData(30, true)]
    [InlineData(300, true)]
    public void IsStale_AgeBoundary_UsesConfiguredLifetime(int ageSeconds, bool expected)
    {
        var clock = new TransitTestClock();
        var policy = new RefreshFreshness(new(), clock);
        var result = new ProviderResult<StopEvent> { RetrievedAt = clock.Now.AddSeconds(-ageSeconds) };
        Assert.Equal(expected, policy.IsStale(result));
    }

    /// <summary>Missing data always needs an initial request.</summary>
    [Fact]
    public void IsStale_MissingResult_NeedsRenewal() => Assert.True(new RefreshFreshness(new()).IsStale<StopEvent>(null));

    /// <summary>A provider's stale or failed result cannot become fresh from its timestamp alone.</summary>
    /// <param name="stale">Explicit stale marker.</param>
    /// <param name="error">Optional safe error code.</param>
    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "offline")]
    public void IsStale_ProviderState_NeedsRenewal(bool stale, string? error)
    {
        var clock = new TransitTestClock();
        Assert.True(new RefreshFreshness(new(), clock).IsStale(new ProviderResult<StopEvent>
        { RetrievedAt = clock.Now, IsStale = stale, ErrorCode = error }));
    }
}
