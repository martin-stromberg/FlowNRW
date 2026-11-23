using FlowNRW.Core.Transit;

namespace FlowNRW.Tests.Transit;

/// <summary>Captured real EFA location normalization.</summary>
public sealed class EfaResponseMapperTests_Locations
{
    /// <summary>Search preserves foreign-state DHIDs and WGS84.</summary>
    [Fact]
    public void Search_BerlinFixture_PreservesDhidAndCoordinates()
    {
        var result = new EfaResponseMapper(new()).Search(AdapterGateway.Fixture("efa-search-live.json"));
        Assert.True(result.HasData);
        Assert.Equal("de:11000:900003200", result.Items[0].Stop!.Dhid);
        Assert.Equal(52.525593, result.Items[0].Coordinate!.Latitude);
        Assert.Contains("provider_message", result.Warnings);
    }
    /// <summary>Nearby retains measured distances.</summary>
    [Fact]
    public void Nearby_LiveFixture_PreservesDistance()
    {
        var result = new EfaResponseMapper(new()).Nearby(AdapterGateway.Fixture("efa-nearby-live.json"));
        Assert.Equal(478, result.Items[0].DistanceMeters);
        Assert.Equal(51.503014, result.Items[0].Stop.Coordinate!.Latitude);
    }
    /// <summary>Malformed data fails safely.</summary>
    [Fact]
    public void Search_MalformedJson_ReturnsSafeError()
    {
        var result = new EfaResponseMapper(new()).Search("private invalid response");
        Assert.Equal("invalid_response", result.ErrorCode);
        Assert.Contains("invalid_response", result.Warnings);
    }
    /// <summary>Parseable JSON without the documented locations array is rejected rather than treated as empty.</summary>
    [Fact]
    public void Search_MissingPayload_ReturnsSafeError()
    {
        var result = new EfaResponseMapper(new()).Search("{}");
        Assert.Equal("invalid_response", result.ErrorCode);
        Assert.Contains("invalid_response", result.Warnings);
    }
    /// <summary>Limits apply to normalized results.</summary>
    [Fact]
    public void Search_ResultLimit_TruncatesLocations()
    {
        Assert.Single(new EfaResponseMapper(new() { MaxResults = 1 }).Search(AdapterGateway.Fixture("efa-search-live.json")).Items);
    }
}
