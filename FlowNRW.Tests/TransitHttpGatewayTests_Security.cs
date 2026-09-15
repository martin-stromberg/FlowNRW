using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Transport safety and data minimization.</summary>
public sealed class TransitHttpGatewayTests_Security
{
    /// <summary>Unsafe URLs are rejected before transport.</summary>
    /// <param name="url">Untrusted endpoint.</param>
    [Theory]
    [InlineData("http://example.test/?address=private")]
    [InlineData("https://secret@example.test/")]
    [InlineData("https://example.test/#secret")]
    public async Task GetAsync_UnsafeUrl_DoesNotSend(string url)
    {
        using var handler = new TransitTestHandler();
        using var client = new HttpClient(handler);
        var gateway = new TransitHttpGateway(client, new(), new TransitDiagnostics(_ => { }), new());
        var result = await gateway.GetAsync("efa", new(url));
        Assert.Equal("invalid-endpoint", result.ErrorCode);
        Assert.Equal(0, handler.Calls);
    }

    /// <summary>Neither arbitrary provider strings nor status strings enter diagnostics.</summary>
    [Fact]
    public void Record_SensitiveInputs_UsesOnlyAllowlistedMetadata()
    {
        var messages = new List<string>();
        var diagnostics = new TransitDiagnostics(messages.Add);
        diagnostics.Record("https://private-address?key=secret", TimeSpan.FromMilliseconds(12), "response secret", 9999);
        Assert.Equal("transit provider=provider durationMs=12 status=error count=100", Assert.Single(messages));
    }

    /// <summary>Transport exceptions cannot expose query strings.</summary>
    [Fact]
    public async Task GetAsync_TransportException_DoesNotEchoException()
    {
        using var handler = new TransitTestHandler { Respond = (_, _) => throw new HttpRequestException("secret address") };
        using var client = new HttpClient(handler);
        var gateway = new TransitHttpGateway(client, new() { MaxRetries = 0 }, new TransitDiagnostics(_ => { }), new());
        Assert.Equal("transport", (await gateway.GetAsync("efa", new("https://example.test/"))).ErrorCode);
    }
}
