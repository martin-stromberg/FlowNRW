namespace FlowNRW.Core.Transit;

/// <summary>Replaceable development endpoints and bounded HTTP settings.</summary>
public sealed record TransitProviderOptions
{
    /// <summary>National development gateway.</summary>
    /// <value>The configured or normalized value.</value>
    public Uri DbRestBaseUrl { get; init; } = new("https://v6.db.transport.rest/");
    /// <summary>VRR OpenService development gateway; production access must be agreed separately.</summary>
    /// <value>The configured or normalized value.</value>
    public Uri EfaBaseUrl { get; init; } = new("https://openservice-test.vrr.de/openservice/");
    /// <summary>Optional alternate EFA development endpoint.</summary>
    public Uri? EfaFallbackBaseUrl { get; init; }
    /// <summary>Supported EFA output format.</summary>
    public string EfaFormat { get; init; } = "rapidJSON";
    /// <summary>Per-attempt timeout.</summary>
    /// <value>The configured or normalized value.</value>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
    /// <summary>Maximum retries after the first attempt.</summary>
    public int MaxRetries { get; init; } = 1;
    /// <summary>Maximum normalized results.</summary>
    public int MaxResults { get; init; } = 100;
    /// <summary>Maximum search text length.</summary>
    public int MaxSearchLength { get; init; } = 200;
    /// <summary>Validates settings without echoing values into errors.</summary>
    public void Validate()
    {
        if (!SafeEndpoint(DbRestBaseUrl) || !SafeEndpoint(EfaBaseUrl) ||
            (EfaFallbackBaseUrl is not null && !SafeEndpoint(EfaFallbackBaseUrl)) ||
            EfaFormat != "rapidJSON" || Timeout <= TimeSpan.Zero || Timeout > TimeSpan.FromMinutes(1) ||
            MaxRetries is < 0 or > 3 || MaxResults is < 1 or > 100 || MaxSearchLength is < 1 or > 1000)
            throw new ArgumentException("Invalid transit provider configuration.");
    }

    private static bool SafeEndpoint(Uri uri) => uri.IsAbsoluteUri && uri.Scheme == "https" &&
        string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment);
}
