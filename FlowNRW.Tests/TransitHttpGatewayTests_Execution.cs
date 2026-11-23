using System.Net;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>HTTP retry, timeout and cancellation behavior.</summary>
public sealed class TransitHttpGatewayTests_Execution
{
    /// <summary>A transient failure is retried once and then returned safely.</summary>
    [Fact]
    public async Task GetAsync_Repeated503_StopsAtConfiguredBound()
    {
        using var handler = new TransitTestHandler { Respond = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)) };
        using var client = new HttpClient(handler);
        var gateway = new TransitHttpGateway(client, new(), new TransitDiagnostics(_ => { }), new());
        var result = await gateway.GetAsync("db-rest", new("https://example.test/?address=private"));
        Assert.Equal("http-503", result.ErrorCode);
        Assert.Equal(2, handler.Calls);
    }

    /// <summary>A slow request is stopped at the configured timeout.</summary>
    [Fact]
    public async Task GetAsync_Timeout_ReturnsSafeError()
    {
        using var handler = new TransitTestHandler { Respond = async (_, token) => { await Task.Delay(Timeout.Infinite, token); return new HttpResponseMessage(); } };
        using var client = new HttpClient(handler);
        var gateway = new TransitHttpGateway(client, new() { Timeout = TimeSpan.FromMilliseconds(25), MaxRetries = 0 }, new TransitDiagnostics(_ => { }), new());
        var result = await gateway.GetAsync("efa", new("https://example.test/"));
        Assert.Equal("timeout", result.ErrorCode);
        Assert.Equal(1, handler.Calls);
    }

    /// <summary>Caller cancellation propagates to transport without retry.</summary>
    [Fact]
    public async Task GetAsync_CallerCancellation_PropagatesWithoutRetry()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new TransitTestHandler { Respond = async (_, token) => { started.SetResult(); await Task.Delay(Timeout.Infinite, token); return new HttpResponseMessage(); } };
        using var client = new HttpClient(handler);
        using var cancellation = new CancellationTokenSource();
        var gateway = new TransitHttpGateway(client, new(), new TransitDiagnostics(_ => { }), new());
        var request = gateway.GetAsync("efa", new("https://example.test/"), cancellation.Token);
        await started.Task;
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
        Assert.Equal(1, handler.Calls);
    }

    /// <summary>Successful responses preserve JSON and request only JSON.</summary>
    [Fact]
    public async Task GetAsync_Success_ReturnsPayload()
    {
        using var handler = new TransitTestHandler();
        using var client = new HttpClient(handler);
        var gateway = new TransitHttpGateway(client, new(), new TransitDiagnostics(_ => { }), new());
        var result = await gateway.GetAsync("efa", new("https://example.test/"));
        Assert.Equal("{}", Assert.Single(result.Items));
    }
}
