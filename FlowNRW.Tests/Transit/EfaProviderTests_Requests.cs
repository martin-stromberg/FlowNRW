using FlowNRW.Core.Transit;

namespace FlowNRW.Tests.Transit;

/// <summary>Provider request construction and alternate endpoint fallback.</summary>
public sealed class EfaProviderTests_Requests
{
    /// <summary>Coordinate endpoints and Germany-local date are transmitted.</summary>
    [Fact]
    public async Task RouteAsync_ForeignIds_UsesCoordinatesAndLocalDate()
    {
        var gateway = new AdapterGateway(); gateway.Add("{\"journeys\":[]}");
        var provider = new EfaProvider(gateway, new(new()), new());
        var location = new Address { Stop = new() { Id = "foreign", Source = "db-rest", Coordinate = new(51, 7) } };
        await provider.RouteAsync(location, location, DateTimeOffset.Parse("2026-09-08T23:30:00Z"));
        var query = Uri.UnescapeDataString(Assert.Single(gateway.Requests).Query);
        Assert.Contains("type_origin=coord", query);
        Assert.Contains("itdDate=20260909", query);
        Assert.DoesNotContain("foreign", query);
    }
    /// <summary>Configured fallback URL is actually called and marked.</summary>
    [Fact]
    public async Task SearchAsync_PrimaryUnavailable_CallsFallbackEndpoint()
    {
        var gateway = new AdapterGateway(); gateway.Responses.Enqueue(new() { ErrorCode = "http_503" }); gateway.Add("{\"locations\":[{\"id\":\"a\",\"name\":\"A\",\"type\":\"stop\"}]}");
        var options = new TransitProviderOptions { EfaFallbackBaseUrl = new("https://alternate.example/efa/") };
        var result = await new EfaProvider(gateway, new(options), options).SearchAsync("A & B");
        Assert.True(result.IsFallback);
        Assert.True(result.HasData);
        Assert.Equal("alternate.example", gateway.Requests[1].Host);
        Assert.Contains("efa_endpoint_fallback", result.Warnings);
        Assert.Contains("name_sf=A%20%26%20B", gateway.Requests[0].AbsoluteUri);
    }
    /// <summary>Invalid input performs no HTTP request.</summary>
    [Fact]
    public async Task SearchAsync_EmptyInput_ReturnsValidationError()
    {
        var gateway = new AdapterGateway();
        var result = await new EfaProvider(gateway, new(new()), new()).SearchAsync(" ");
        Assert.Equal("invalid_input", result.ErrorCode);
        Assert.Empty(gateway.Requests);
    }
    /// <summary>Departures resolve foreign identities by name.</summary>
    [Fact]
    public async Task DeparturesAsync_ForeignStop_ResolvesProviderStop()
    {
        var gateway = new AdapterGateway(); gateway.Add("{\"locations\":[{\"id\":\"resolved\",\"name\":\"A\",\"type\":\"stop\"}]}"); gateway.Add("{\"stopEvents\":[]}");
        await new EfaProvider(gateway, new(new()), new()).DeparturesAsync(new() { Id = "foreign", Source = "db-rest", Name = "A" }, DateTimeOffset.UtcNow);
        Assert.Contains("name_dm=resolved", gateway.Requests[1].Query);
        Assert.DoesNotContain("foreign", gateway.Requests[1].Query);
    }
    /// <summary>Nearby asks for geographic output and stop filters.</summary>
    [Fact]
    public async Task NearbyAsync_Coordinate_UsesStopRadiusAndWgs84()
    {
        var gateway = new AdapterGateway(); gateway.Add("{\"locations\":[]}");
        await new EfaProvider(gateway, new(new()), new()).NearbyAsync(new(51, 7));
        Assert.Contains("coordOutputFormat=WGS84[DD.ddddd]", Uri.UnescapeDataString(gateway.Requests[0].Query));
        Assert.Contains("radius_1=1000", gateway.Requests[0].Query);
    }
}
