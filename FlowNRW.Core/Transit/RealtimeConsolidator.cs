namespace FlowNRW.Core.Transit;

/// <summary>Conservatively merges events with unique stop and trip matches.</summary>
public sealed class RealtimeConsolidator : IRealtimeConsolidator
{
    /// <inheritdoc />
    public IReadOnlyList<StopEvent> Consolidate(IReadOnlyList<StopEvent> scheduled, IReadOnlyList<StopEvent> regional)
    {
        return scheduled.Select(item =>
        {
            var matches = regional.Where(candidate => Matches(item.Identity, candidate.Identity)).ToArray();
            if (matches.Length != 1 || scheduled.Count(other => Matches(other.Identity, matches[0].Identity)) != 1)
                return item;
            var actual = matches[0].Realtime;
            return item with
            {
                Realtime = item.Realtime with
                {
                    ActualTime = actual.ActualTime ?? item.Realtime.ActualTime,
                    Delay = actual.Delay ?? item.Realtime.Delay,
                    Cancelled = actual.Cancelled ?? item.Realtime.Cancelled,
                    Platform = actual.Platform ?? item.Realtime.Platform,
                    PlannedPlatform = actual.PlannedPlatform ?? item.Realtime.PlannedPlatform,
                    Source = actual.Source,
                    RetrievedAt = actual.RetrievedAt
                }
            };
        }).ToArray();
    }

    internal static bool Matches(TripIdentity left, TripIdentity right)
    {
        if (!SameStop(left.Stop, right.Stop)) return false;
        if (left.PlannedTime is null || right.PlannedTime is null ||
            (left.PlannedTime.Value - right.PlannedTime.Value).Duration() > TimeSpan.FromSeconds(60)) return false;
        if (Present(left.SharedTripId) && Present(right.SharedTripId))
            return left.SharedTripId == right.SharedTripId;
        if (left.Source == right.Source && Present(left.TripId) && Present(right.TripId))
            return left.TripId == right.TripId;
        return Equal(left.Line, right.Line) && Equal(left.Operator, right.Operator) && Equal(left.Direction, right.Direction);
    }

    internal static bool SameStop(Stop left, Stop right)
    {
        if (Present(left.Dhid) && Present(right.Dhid)) return left.Dhid == right.Dhid;
        if (left.Source == right.Source && Present(left.Id) && Present(right.Id)) return left.Id == right.Id;
        if (left.Coordinate is null || right.Coordinate is null || !Equal(left.Name, right.Name)) return false;
        double latitude = (left.Coordinate.Latitude + right.Coordinate.Latitude) * Math.PI / 360;
        double dx = (left.Coordinate.Longitude - right.Coordinate.Longitude) * Math.Cos(latitude) * 111320;
        double dy = (left.Coordinate.Latitude - right.Coordinate.Latitude) * 111320;
        return Math.Sqrt(dx * dx + dy * dy) <= 100;
    }

    internal static bool SameJourney(Journey left, Journey right)
    {
        if (left.Legs.Count == 0 || left.Legs.Count != right.Legs.Count) return false;
        return left.Legs.Zip(right.Legs).All(pair =>
        {
            if ((pair.First.Walking is null) != (pair.Second.Walking is null)) return false;
            if (pair.First.Walking is null)
                return Matches(pair.First.Departure.Identity, pair.Second.Departure.Identity) &&
                    Matches(pair.First.Arrival.Identity, pair.Second.Arrival.Identity);
            return SameWalkingEvent(pair.First.Departure, pair.Second.Departure) && SameWalkingEvent(pair.First.Arrival, pair.Second.Arrival);
        });
    }

    private static bool SameWalkingEvent(StopEvent left, StopEvent right) =>
        left.PlannedTime is { } leftTime && right.PlannedTime is { } rightTime && (leftTime - rightTime).Duration() <= TimeSpan.FromSeconds(60) &&
        SameStop(left.Identity.Stop, right.Identity.Stop);

    private static bool Present(string? value) => !string.IsNullOrWhiteSpace(value);
    private static bool Equal(string? left, string? right) => Present(left) && Present(right) && Normalize(left!) == Normalize(right!);
    private static string Normalize(string value) => string.Join(' ', value.Trim().ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

