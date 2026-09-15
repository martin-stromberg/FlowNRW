using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Validates untrusted coordinates and configuration.</summary>
public sealed class TransitModelTests_Validation
{
    /// <summary>Rejects invalid WGS84 inputs.</summary>
    /// <param name="latitude">Latitude.</param>
    /// <param name="longitude">Longitude.</param>
    [Theory]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    public void GeoCoordinate_InvalidValues_Throws(double latitude, double longitude) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoCoordinate(latitude, longitude));

    /// <summary>Allows exact valid coordinate bounds.</summary>
    [Fact]
    public void GeoCoordinate_BoundaryValues_Accepts() => Assert.Equal(180, new GeoCoordinate(-90, 180).Longitude);

    /// <summary>Unknown realtime must not imply punctuality or no cancellation.</summary>
    [Fact]
    public void RealtimeStatus_MissingFields_RemainUnknown()
    {
        var status = new RealtimeStatus();
        Assert.Null(status.ActualTime);
        Assert.Null(status.Delay);
        Assert.Null(status.Cancelled);
    }

    /// <summary>Rejects unsafe endpoint settings.</summary>
    /// <param name="url">Unsafe URL.</param>
    [Theory]
    [InlineData("http://example.test/")]
    [InlineData("https://user:password@example.test/")]
    [InlineData("https://example.test/?key=secret")]
    [InlineData("https://example.test/#secret")]
    public void ProviderOptions_UnsafeEndpoint_RejectsWithoutEcho(string url)
    {
        var error = Assert.Throws<ArgumentException>(() => (new TransitProviderOptions { DbRestBaseUrl = new Uri(url) }).Validate());
        Assert.Equal("Invalid transit provider configuration.", error.Message);
    }
}
