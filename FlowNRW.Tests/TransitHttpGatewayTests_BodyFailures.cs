using FlowNRW.Core.Transit;
namespace FlowNRW.Tests;
/// <summary>Body failures after successful response headers.</summary>
public sealed class TransitHttpGatewayTests_BodyFailures
{
    /// <summary>Body failures retain their transport classification and retry bound.</summary>
    /// <param name="timeout">Whether the body stalls or fails immediately.</param>
    /// <param name="expected">Expected safe error code.</param>
    [Theory]
    [InlineData(true, "timeout")]
    [InlineData(false, "transport")]
    public async Task GetAsync_BodyFailsAfter200_RetriesAndPreservesError(bool timeout, string expected)
    {
        using var handler = new TransitTestHandler { Respond = (_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StreamContent(new FailingBody(timeout)) }) };
        using var client = new HttpClient(handler);
        var gateway = new TransitHttpGateway(client, new() { Timeout = TimeSpan.FromMilliseconds(25), MaxRetries = 1 }, new TransitDiagnostics(_ => { }), new());
        var result = await gateway.GetAsync("efa", new("https://example.test/"));
        Assert.Equal(expected, result.ErrorCode);
        Assert.Equal(2, handler.Calls);
    }
    private sealed class FailingBody(bool timeout) : MemoryStream
    {
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (timeout) await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new IOException("private body failure");
        }
    }
}
