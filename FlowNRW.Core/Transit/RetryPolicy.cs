using System.Net;

namespace FlowNRW.Core.Transit;

/// <summary>Bounded retries for transient failures only.</summary>
public sealed class RetryPolicy
{
    /// <summary>Determines whether another attempt is allowed.</summary>
    /// <param name="status">HTTP status, or null for a transport failure.</param>
    /// <param name="attempt">Zero-based completed attempt.</param>
    /// <param name="maximumRetries">Configured retry bound.</param>
    /// <returns>Whether the failure is transient and the bound permits retry.</returns>
    public bool ShouldRetry(HttpStatusCode? status, int attempt, int maximumRetries) =>
        attempt < maximumRetries && (status is null or HttpStatusCode.RequestTimeout or
            HttpStatusCode.TooManyRequests || (int)status >= 500);
}
