using System.Diagnostics;

namespace FlowNRW.Core.Transit;

/// <summary>Records bounded technical events without retaining requests or responses.</summary>
public sealed class TransitDiagnostics : ITransitDiagnostics
{
    private readonly Action<string> sink;

    /// <summary>Creates diagnostics with an optional technical event sink.</summary>
    /// <param name="sink">Receives sanitized metadata only.</param>
    public TransitDiagnostics(Action<string>? sink = null) => this.sink = sink ?? (message => Trace.WriteLine(message));

    /// <inheritdoc />
    public void Record(string provider, TimeSpan duration, string status, int count)
    {
        string source = provider is "db-rest" or "efa" ? provider : "provider";
        string code = status is "ok" or "http" or "timeout" or "transport" or "cancelled" or "invalid" ? status : "error";
        sink($"transit provider={source} durationMs={Math.Clamp((long)duration.TotalMilliseconds, 0, 600000)} status={code} count={Math.Clamp(count, 0, 100)}");
    }
}
