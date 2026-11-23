using FlowNRW.Core.Transit;

namespace FlowNRW.Tests.Transit;

/// <summary>Real EFA departures and connections.</summary>
public sealed class EfaResponseMapperTests_Journeys
{
    /// <summary>National connection includes actual geometry.</summary>
    [Fact]
    public void Journeys_BerlinHamburgFixture_PreservesTransportAndGeometry()
    {
        var result = new EfaResponseMapper(new()).Journeys(AdapterGateway.Fixture("efa-journeys-live.json"));
        var leg = result.Items[0].Legs[0];
        Assert.Equal("de:11000:900003200", leg.Departure.Identity.Stop.Dhid);
        Assert.Equal("de:02000:10950", leg.Arrival.Identity.Stop.Dhid);
        Assert.Equal("DB Fernverkehr AG", leg.Line!.Operator!.Name);
        Assert.True(leg.Geometry!.Coordinates.Count > 100);
        Assert.NotNull(result.Items[0].Legs[1].Walking);
        Assert.Equal(DateTimeOffset.Parse("2026-09-08T11:37:00Z"), leg.Departure.PlannedTime);
    }
    /// <summary>Platform hierarchy resolves to station identity.</summary>
    [Fact]
    public void Departures_NrwFixture_NormalizesParentStopAndRealtime()
    {
        var result = new EfaResponseMapper(new()).Departures(AdapterGateway.Fixture("efa-departures-live.json"));
        var realtime = result.Items.First(e => e.Realtime.ActualTime is not null);
        Assert.Equal("de:05513:5613", realtime.Identity.Stop.Dhid);
        Assert.NotNull(realtime.Realtime.Platform);
        Assert.Equal(realtime.Realtime.ActualTime - realtime.PlannedTime, realtime.Realtime.Delay);
    }
    /// <summary>Missing realtime stays unknown.</summary>
    [Fact]
    public void Departures_ScheduleOnly_PreservesUnknown()
    {
        var result = new EfaResponseMapper(new()).Departures("""{"stopEvents":[{"location":{"id":"1","name":"A","type":"stop"},"departureTimePlanned":"2026-09-08T23:59:00+02:00"}]}""");
        Assert.Null(result.Items[0].Realtime.ActualTime);
        Assert.Null(result.Items[0].Realtime.Cancelled);
    }
    /// <summary>Missing offset remains unknown and warns consumers.</summary>
    [Fact]
    public void Departures_TimeWithoutOffset_ReportsUnknownTime()
    {
        var result = new EfaResponseMapper(new()).Departures("""{"stopEvents":[{"location":{"id":"1"},"departureTimePlanned":"2026-09-08T23:59:00"}]}""");
        Assert.Null(result.Items[0].PlannedTime);
        Assert.Contains("invalid_time", result.Warnings);
    }
    /// <summary>Explicit cancelled event remains cancelled.</summary>
    [Fact]
    public void Departures_Cancelled_PreservesCancellation()
    {
        var result = new EfaResponseMapper(new()).Departures("""{"stopEvents":[{"location":{"id":"1"},"realtimeStatus":["CANCELLED"]}]}""");
        Assert.True(result.Items[0].Realtime.Cancelled);
    }
    /// <summary>Walking and transfers survive normalization.</summary>
    [Fact]
    public void Journeys_WalkBetweenTransitLegs_ProducesTransfer()
    {
        var result = new EfaResponseMapper(new()).Journeys("""{"journeys":[{"legs":[{"origin":{"departureTimePlanned":"2026-09-08T23:00:00Z"},"destination":{"arrivalTimePlanned":"2026-09-08T23:30:00Z"},"transportation":{"id":"a"}},{"duration":60,"distance":80,"transportation":{"product":{"class":99}}},{"origin":{"departureTimePlanned":"2026-09-09T00:00:00Z"},"transportation":{"id":"b"}}]}]}""");
        Assert.Equal(TimeSpan.FromMinutes(30), Assert.Single(result.Items[0].Transfers).Duration);
        Assert.Equal(80, result.Items[0].Legs[1].Walking!.DistanceMeters);
    }
}


