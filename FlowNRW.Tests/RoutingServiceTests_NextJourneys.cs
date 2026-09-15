using FlowNRW.Core.Transit;

namespace FlowNRW.Tests;

/// <summary>Only upcoming connections are returned in chronological order.</summary>
public sealed class RoutingServiceTests_NextJourneys
{
    /// <summary>EFA may return already departed trips; the service removes them.</summary>
    [Fact]
    public async Task RouteAsync_ProviderIncludesEarlierJourneys_RemovesPastDepartures()
    {
        var departure = new DateTimeOffset(2026, 9, 8, 14, 0, 0, TimeSpan.FromHours(2));
        var provider = new TransitTestProvider
        {
            Journeys = new()
            {
                Items = new[]
        {
            new Journey { Id = "past", Legs = new[] { new JourneyLeg { Departure = new() { PlannedTime = departure.AddMinutes(-20) } } } },
            new Journey { Id = "next", Legs = new[] { new JourneyLeg { Departure = new() { PlannedTime = departure.AddMinutes(10) } } } }
        }
            }
        };
        var result = await new RoutingService(provider, new()).RouteAsync(new() { Name = "Berlin" }, new() { Name = "Hamburg" }, departure);
        Assert.Equal("next", Assert.Single(result.Items).Id);
    }
}
