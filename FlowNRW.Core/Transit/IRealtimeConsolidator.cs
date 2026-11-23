namespace FlowNRW.Core.Transit;

/// <summary>Contract for RealtimeConsolidator.</summary>
public interface IRealtimeConsolidator
{
    /// <summary>Merge only uniquely identified events.</summary>
    /// <param name="scheduled">scheduled input.</param>
    /// <param name="regional">regional input.</param>
    /// <returns>Operation result.</returns>
    IReadOnlyList<StopEvent> Consolidate(IReadOnlyList<StopEvent> scheduled, IReadOnlyList<StopEvent> regional);

}
