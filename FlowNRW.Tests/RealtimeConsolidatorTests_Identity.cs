using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Conservative cross-provider stop and trip identity matching.</summary>
public sealed class RealtimeConsolidatorTests_Identity
{
    private static StopEvent Event(string source, string trip = "trip") => new()
    {
        Identity = new()
        {
            Source = source,
            TripId = trip,
            Stop = new() { Id = source, Source = source, Dhid = "de:05513:5613", Name = "Hbf", Coordinate = new(51.5, 7.1) },
            Line = "RE 2",
            Operator = "DB",
            Direction = "Münster",
            PlannedTime = new(2026, 9, 8, 12, 0, 0, TimeSpan.FromHours(2))
        },
        Realtime = new() { Source = source }
    };

    /// <summary>A unique semantic match receives regional realtime.</summary>
    [Fact]
    public void Consolidate_UniqueTripAndStop_MergesRealtime()
    {
        var scheduled = Event("db-rest");
        var regional = Event("efa", "different-provider-id") with { Realtime = new() { Delay = TimeSpan.FromMinutes(4), Cancelled = false, Source = "efa" } };
        var result = Assert.Single(new RealtimeConsolidator().Consolidate(new[] { scheduled }, new[] { regional }));
        Assert.Equal(TimeSpan.FromMinutes(4), result.Realtime.Delay);
        Assert.Equal(scheduled.Identity, result.Identity);
    }

    /// <summary>Shared stop identity alone cannot merge different journeys.</summary>
    [Fact]
    public void Consolidate_SameDhidDifferentDirection_DoesNotMerge()
    {
        var scheduled = Event("db-rest");
        var regional = Event("efa") with { Identity = Event("efa").Identity with { Direction = "Düsseldorf" }, Realtime = new() { Cancelled = true } };
        Assert.Same(scheduled, Assert.Single(new RealtimeConsolidator().Consolidate(new[] { scheduled }, new[] { regional })));
    }

    /// <summary>Multiple possible matches stay separate.</summary>
    [Fact]
    public void Consolidate_AmbiguousCandidates_DoesNotMerge()
    {
        var scheduled = Event("db-rest");
        Assert.Same(scheduled, Assert.Single(new RealtimeConsolidator().Consolidate(new[] { scheduled }, new[] { Event("efa", "one"), Event("efa", "two") })));
    }

    /// <summary>Conflicting same-provider IDs cannot be overridden by matching line details.</summary>
    [Fact]
    public void Consolidate_ConflictingTripIds_DoesNotMerge()
    {
        var scheduled = Event("efa", "one");
        Assert.Same(scheduled, Assert.Single(new RealtimeConsolidator().Consolidate(new[] { scheduled }, new[] { Event("efa", "two") })));
    }

    /// <summary>Geographic and normalized name matching is allowed only within 100 meters.</summary>
    [Fact]
    public void Consolidate_NoDhid_UsesNameAndDistance()
    {
        var scheduled = Event("db-rest") with { Identity = Event("db-rest").Identity with { Stop = new() { Name = " hbf ", Coordinate = new(51.5, 7.1), Source = "db-rest" } } };
        var regional = Event("efa") with { Realtime = new() { Cancelled = true, Source = "efa" } };
        Assert.True(Assert.Single(new RealtimeConsolidator().Consolidate(new[] { scheduled }, new[] { regional })).Realtime.Cancelled);
    }
}
