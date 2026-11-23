using System.Net;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Retry policy distinguishes transient and permanent statuses.</summary>
public sealed class RetryPolicyTests_Bounds
{
    /// <summary>Only eligible failures within the bound are retried.</summary>
    /// <param name="status">Response status.</param>
    /// <param name="attempt">Completed attempt.</param>
    /// <param name="expected">Whether retry is allowed.</param>
    [Theory]
    [InlineData(503, 0, true)]
    [InlineData(503, 1, false)]
    [InlineData(429, 0, true)]
    [InlineData(408, 0, true)]
    [InlineData(400, 0, false)]
    [InlineData(401, 0, false)]
    [InlineData(302, 0, false)]
    public void ShouldRetry_UsesStatusAndAttemptBound(int status, int attempt, bool expected) =>
        Assert.Equal(expected, new RetryPolicy().ShouldRetry((HttpStatusCode)status, attempt, 1));
}
