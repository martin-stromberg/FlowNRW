using FlowNRW.Core.Transit;

namespace FlowNRW.Tests.Transit;

/// <summary>db-rest response normalization regression scenarios.</summary>
public sealed class DbRestResponseMapperTests_Normalization
{
    /// <summary>Search handles stations and addresses.</summary>
    [Fact]
    public void Search_MixedLocations_PreservesAddressAndStop()
    {
        var result = new DbRestResponseMapper(new()).Search("""[{"type":"stop","id":"1","name":"A","ids":{"dhid":"de:1:2"},"location":{"latitude":51,"longitude":7}},{"type":"location","address":"Street","latitude":52,"longitude":8}]""");
        Assert.Equal("de:1:2", result.Items[0].Stop!.Dhid);
        Assert.Equal("Street", result.Items[1].Name);
        Assert.Null(result.Items[1].Stop);
    }
    /// <summary>Realtime fields cross midnight without local-time assumptions.</summary>
    [Fact]
    public void Departures_MidnightDelay_PreservesOffset()
    {
        var result = new DbRestResponseMapper(new()).Departures("""{"departures":[{"stop":{"id":"1"},"plannedWhen":"2026-09-08T23:59:00+02:00","when":"2026-09-09T00:02:00+02:00","delay":180,"cancelled":false,"platform":"2","plannedPlatform":"1","line":{"name":"RE1","operator":{"name":"DB"}}}]}""");
        Assert.Equal(TimeSpan.FromMinutes(3), result.Items[0].Realtime.Delay);
        Assert.Equal(9, result.Items[0].Realtime.ActualTime!.Value.Day);
        Assert.Equal("2", result.Items[0].Realtime.Platform);
    }
    /// <summary>Unknown realtime is not a zero delay.</summary>
    [Fact]
    public void Departures_NoRealtime_PreservesUnknown()
    {
        var result = new DbRestResponseMapper(new()).Departures("""{"departures":[{"stop":{"id":"1"},"plannedWhen":"2026-09-08T12:00:00Z","when":"2026-09-08T12:00:00Z"}]}""");
        Assert.Null(result.Items[0].Realtime.ActualTime);
        Assert.Null(result.Items[0].Realtime.Delay);
    }
    /// <summary>Geometry converts GeoJSON longitude-first coordinates.</summary>
    [Fact]
    public void Journeys_WalkingGeometry_ConvertsCoordinateOrder()
    {
        var result = new DbRestResponseMapper(new()).Journeys("""{"journeys":[{"legs":[{"walking":true,"distance":100,"origin":{"name":"A"},"destination":{"name":"B"},"plannedDeparture":"2026-09-08T12:00:00Z","plannedArrival":"2026-09-08T12:02:00Z","polyline":{"features":[{"geometry":{"coordinates":[7,51]}}]}}]}]}""");
        var walk = result.Items[0].Legs[0].Walking!;
        Assert.Equal(51, walk.Geometry!.Coordinates[0].Latitude);
        Assert.Equal(TimeSpan.FromMinutes(2), walk.Duration);
    }
    /// <summary>Invalid JSON gets a safe technical error.</summary>
    [Fact]
    public void Departures_InvalidJson_ReportsError()
    {
        var result = new DbRestResponseMapper(new()).Departures("{");
        Assert.Equal("invalid_response", result.ErrorCode);
        Assert.Empty(result.Items);
    }

    /// <summary>Parseable JSON without the required departure array is not a valid empty provider response.</summary>
    [Fact]
    public void Departures_MissingPayload_ReportsError()
    {
        var result = new DbRestResponseMapper(new()).Departures("{}");
        Assert.Equal("invalid_response", result.ErrorCode);
        Assert.Contains("invalid_response", result.Warnings);
    }
}
