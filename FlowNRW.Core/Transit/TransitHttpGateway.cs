using System.Diagnostics;
using System.Net;
using System.Text;
using FlowNRW.Core.Diagnostics;

namespace FlowNRW.Core.Transit;

/// <summary>HTTPS gateway with bounded response size, retries and per-attempt timeout.</summary>
public sealed class TransitHttpGateway : ITransitHttpGateway
{
    private const int CircuitOpenFailures = 2;
    private static readonly TimeSpan CircuitCooldown = TimeSpan.FromMinutes(2);
    private readonly HttpClient client;
    private readonly TransitProviderOptions options;
    private readonly ITransitDiagnostics diagnostics;
    private readonly RetryPolicy retry;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Circuit> circuits = new();

    private sealed class Circuit { public int Failures; public DateTimeOffset OpenUntil; }

    /// <summary>Constructs a gateway. Inject clients with automatic redirects disabled.</summary>
    /// <param name="client">Client owned by the composition root.</param>
    /// <param name="options">Validated execution limits.</param>
    /// <param name="diagnostics">Technical diagnostics.</param>
    /// <param name="retry">Bounded retry decisions.</param>
    public TransitHttpGateway(HttpClient client, TransitProviderOptions options, ITransitDiagnostics diagnostics, RetryPolicy retry)
    {
        options.Validate();
        this.client = client;
        this.options = options;
        this.diagnostics = diagnostics;
        this.retry = retry;
    }

    /// <inheritdoc />
    public async Task<ProviderResult<string>> GetAsync(string provider, Uri uri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!uri.IsAbsoluteUri || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment))
            return Failure(provider, "invalid-endpoint");
        var host = uri.Host;
        var circuit = circuits.GetOrAdd(host, _ => new Circuit());
        if (circuit.OpenUntil > DateTimeOffset.UtcNow)
        {
            AppLog.Write("http", $"{provider} {uri.AbsolutePath} -> circuit-open (skipped)");
            return Failure(provider, "circuit-open");
        }
        var watch = Stopwatch.StartNew();
        for (int attempt = 0; ; attempt++)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.Timeout);
            HttpStatusCode? status = null;
            string code;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Accept.ParseAdd("application/json");
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
                status = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    if (response.Content.Headers.ContentLength > 8 * 1024 * 1024)
                        return Failure(provider, "response-too-large");
                    await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
                    using var buffer = new MemoryStream();
                    var bytes = new byte[8192];
                    int length;
                    while ((length = await stream.ReadAsync(bytes, timeout.Token).ConfigureAwait(false)) > 0)
                    {
                        if (buffer.Length + length > 8 * 1024 * 1024) return Failure(provider, "response-too-large");
                        buffer.Write(bytes, 0, length);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    diagnostics.Record(provider, watch.Elapsed, "ok", 1);
                    circuit.Failures = 0;
                    AppLog.Write("http", $"{provider} {uri.AbsolutePath} -> {(int)status} in {watch.ElapsedMilliseconds} ms");
                    return new() { Source = provider, Items = new[] { Encoding.UTF8.GetString(buffer.ToArray()) } };
                }
                code = "http";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                diagnostics.Record(provider, watch.Elapsed, "cancelled", 0);
                throw;
            }
            catch (OperationCanceledException) { status = null; code = "timeout"; }
            catch (HttpRequestException) { status = null; code = "transport"; }
            catch (IOException) { status = null; code = "transport"; }
            diagnostics.Record(provider, watch.Elapsed, code, 0);
            if (!retry.ShouldRetry(status, attempt, options.MaxRetries))
            {
                var failure = status is null ? code : $"http-{(int)status}";
                if (status is null || (int)status >= 500)
                {
                    circuit.Failures++;
                    if (circuit.Failures >= CircuitOpenFailures) { circuit.OpenUntil = DateTimeOffset.UtcNow.Add(CircuitCooldown); circuit.Failures = 0; }
                }
                AppLog.Write("http", $"{provider} {uri.AbsolutePath} -> {failure} after {watch.ElapsedMilliseconds} ms");
                return Failure(provider, failure);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100 * (attempt + 1)), cancellationToken).ConfigureAwait(false);
        }
    }

    private static ProviderResult<string> Failure(string source, string code) => new() { Source = source, ErrorCode = code };
}

