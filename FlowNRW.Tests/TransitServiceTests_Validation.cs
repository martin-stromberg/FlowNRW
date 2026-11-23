using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Validation and latest-request service boundaries.</summary>
public sealed class TransitServiceTests_Validation
{
    /// <summary>Invalid search inputs never reach a provider.</summary>
    [Fact]
    public async Task SearchAsync_EmptySearch_ReturnsValidationError()
    {
        var provider = new TransitTestProvider();
        var result = await new StopSearchService(provider, new()).SearchAsync(" ");
        Assert.Equal("invalid-search", result.ErrorCode);
        Assert.Equal(0, provider.Calls);
    }

    /// <summary>Missing route endpoints never reach a provider.</summary>
    [Fact]
    public async Task RouteAsync_MissingEndpoint_ReturnsValidationError()
    {
        var provider = new TransitTestProvider();
        var result = await new RoutingService(provider, new()).RouteAsync(new(), new(), DateTimeOffset.UtcNow);
        Assert.Equal("invalid-location", result.ErrorCode);
        Assert.Equal(0, provider.Calls);
    }

    /// <summary>Missing stop identity is rejected safely.</summary>
    [Fact]
    public async Task DeparturesAsync_MissingStop_ReturnsValidationError()
    {
        var provider = new TransitTestProvider();
        var result = await new DepartureService(provider, new()).DeparturesAsync(new(), DateTimeOffset.UtcNow);
        Assert.Equal("invalid-stop", result.ErrorCode);
        Assert.Equal(0, provider.Calls);
    }

    /// <summary>An exactly full provider response is complete; only an observed extra item marks truncation.</summary>
    [Fact]
    public async Task DeparturesAsync_ExactlyAtLimit_IsNotMarkedTruncated()
    {
        var provider = new TransitTestProvider
        {
            Departures = new()
            {
                Items = [new StopEvent { PlannedTime = DateTimeOffset.UtcNow.AddMinutes(1) }, new StopEvent { PlannedTime = DateTimeOffset.UtcNow.AddMinutes(2) }]
            }
        };
        var result = await new DepartureService(provider, new TransitProviderOptions { MaxResults = 2 })
            .DeparturesAsync(new Stop { Id = "stop", Source = "fixture" }, DateTimeOffset.UtcNow);
        Assert.Equal(2, result.Items.Count);
        Assert.DoesNotContain("truncated-response", result.Warnings);
    }

    /// <summary>A newer search suppresses even an older provider that ignores cancellation.</summary>
    [Fact]
    public async Task SearchAsync_NewerRequest_SuppressesOlderResult()
    {
        var delayed = new TaskCompletionSource<ProviderResult<Address>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = new TransitTestProvider { Search = _ => delayed.Task };
        var service = new StopSearchService(provider, new());
        var older = service.SearchAsync("old");
        provider.Search = _ => Task.FromResult(new ProviderResult<Address> { Items = new[] { new Address { Name = "new" } } });
        Assert.Equal("new", Assert.Single((await service.SearchAsync("new")).Items).Name);
        delayed.SetResult(new() { Items = new[] { new Address { Name = "old" } } });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => older);
    }
}
