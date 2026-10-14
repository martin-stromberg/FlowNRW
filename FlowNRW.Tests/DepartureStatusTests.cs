using FlowNRW.Core.Presentation;
using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Departure status derivation and delay wording stay limited to supplied realtime fields.</summary>
public sealed class DepartureStatusTests
{
    private static StopEvent Departure(RealtimeStatus realtime, DateTimeOffset? planned = null)
        => new() { PlannedTime = planned, Realtime = realtime };

    /// <summary>A reported cancellation wins over every other realtime field.</summary>
    [Fact]
    public void Status_CancelledEvent_ReturnsCancelled()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { Cancelled = true, ActualTime = planned }, planned);
        Assert.Equal(DepartureStatus.Cancelled, DeparturePresentation.Status(item));
    }

    /// <summary>An actual time deviating from the plan is a delay.</summary>
    [Fact]
    public void Status_DelayedEvent_ReturnsDelayed()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { ActualTime = planned.AddMinutes(6) }, planned);
        Assert.Equal(DepartureStatus.Delayed, DeparturePresentation.Status(item));
    }

    /// <summary>A positive reported delay without a deviating actual time is a delay.</summary>
    [Fact]
    public void Status_PositiveDelayField_ReturnsDelayed()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { ActualTime = planned, Delay = TimeSpan.FromMinutes(4) }, planned);
        Assert.Equal(DepartureStatus.Delayed, DeparturePresentation.Status(item));
    }

    /// <summary>Realtime without any deviation reports the departure on time.</summary>
    [Fact]
    public void Status_OnTimeEvent_ReturnsOnTime()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { ActualTime = planned, Cancelled = false }, planned);
        Assert.Equal(DepartureStatus.OnTime, DeparturePresentation.Status(item));
    }

    /// <summary>Without a reported actual time nothing is invented and the plan stays scheduled.</summary>
    [Fact]
    public void Status_NoRealtime_ReturnsScheduled()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus(), planned);
        Assert.Equal(DepartureStatus.Scheduled, DeparturePresentation.Status(item));
    }

    /// <summary>A reported actual time deviating from the plan is formatted as a positive delay text.</summary>
    [Fact]
    public void DelayText_RealDeviation_FormatsPositiveMinutes()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { ActualTime = planned.AddMinutes(7) }, planned);
        Assert.Equal("+7 Min.", DeparturePresentation.DelayText(item));
    }

    /// <summary>Punctual realtime produces no delay text.</summary>
    [Fact]
    public void DelayText_OnTimeEvent_ReturnsNull()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { ActualTime = planned }, planned);
        Assert.Null(DeparturePresentation.DelayText(item));
    }

    /// <summary>A cancellation produces no delay text even when a delay is reported.</summary>
    [Fact]
    public void DelayText_CancelledEvent_ReturnsNull()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { Cancelled = true, Delay = TimeSpan.FromMinutes(9) }, planned);
        Assert.Null(DeparturePresentation.DelayText(item));
    }

    /// <summary>Without realtime no delay text is invented.</summary>
    [Fact]
    public void DelayText_NoRealtime_ReturnsNull()
    {
        var item = Departure(new RealtimeStatus());
        Assert.Null(DeparturePresentation.DelayText(item));
    }

    /// <summary>DelayText and Describe derive the same effective delay from the reported delay field.</summary>
    [Fact]
    public void EffectiveDelay_ReportedDelayField_SharedByDelayTextAndDescribe()
    {
        var item = Departure(new RealtimeStatus { Delay = TimeSpan.FromMinutes(3) });
        Assert.Equal("+3 Min.", DeparturePresentation.DelayText(item));
        Assert.Contains("Abweichung: +3 Min.", DeparturePresentation.Describe(item));
    }

    /// <summary>DelayText and Describe derive the same effective delay from an actual/planned deviation.</summary>
    [Fact]
    public void EffectiveDelay_ActualDeviation_SharedByDelayTextAndDescribe()
    {
        var planned = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);
        var item = Departure(new RealtimeStatus { ActualTime = planned.AddMinutes(4) }, planned);
        Assert.Equal("+4 Min.", DeparturePresentation.DelayText(item));
        Assert.Contains("Abweichung: +4 Min.", DeparturePresentation.Describe(item));
    }
}
