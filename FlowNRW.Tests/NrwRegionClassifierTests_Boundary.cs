using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Official boundary classification including outside points within the bounding rectangle.</summary>
public sealed class NrwRegionClassifierTests_Boundary
{
    /// <summary>Verifies locations inside and outside NRW, including outside VRR.</summary>
    /// <param name="latitude">Latitude.</param>
    /// <param name="longitude">Longitude.</param>
    /// <param name="expected">Expected state membership.</param>
    [Theory]
    [InlineData(51.4556, 7.0116, true)]
    [InlineData(50.7753, 6.0839, true)]
    [InlineData(52.0302, 8.5325, true)]
    [InlineData(51.9625, 7.6252, true)]
    [InlineData(52.2799, 8.0472, false)]
    [InlineData(50.3569, 7.5889, false)]
    [InlineData(50.8514, 5.6910, false)]
    [InlineData(52.5200, 13.4050, false)]
    public void IsInNrw_UsesActualStateBoundary(double latitude, double longitude, bool expected) =>
        Assert.Equal(expected, new NrwRegionClassifier().IsInNrw(new(latitude, longitude)));

    /// <summary>Missing location cannot activate regional priority.</summary>
    [Fact]
    public void IsInNrw_MissingCoordinate_ReturnsFalse() => Assert.False(new NrwRegionClassifier().IsInNrw(null));
}
