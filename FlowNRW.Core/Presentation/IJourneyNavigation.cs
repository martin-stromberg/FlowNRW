namespace FlowNRW.Core.Presentation;

/// <summary>Navigation independent of the platform shell.</summary>
public interface IJourneyNavigation
{
    /// <summary>Shows current results.</summary>
    /// <returns>Navigation completion.</returns>
    Task ShowResultsAsync();
    /// <summary>Shows the selected journey.</summary>
    /// <returns>Navigation completion.</returns>
    Task ShowDetailAsync();
}
