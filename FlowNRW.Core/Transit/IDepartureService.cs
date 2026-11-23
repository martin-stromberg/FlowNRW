namespace FlowNRW.Core.Transit;

/// <summary>Contract for DepartureService.</summary>
public interface IDepartureService
{
    /// <summary>Find departures.</summary>
    /// <param name="stop">stop input.</param>
    /// <param name="departure">departure input.</param>
    /// <param name="cancellationToken">cancellationToken input.</param>
    /// <returns>Operation result.</returns>
    Task<ProviderResult<StopEvent>> DeparturesAsync(Stop stop, DateTimeOffset departure, CancellationToken cancellationToken = default);

}
