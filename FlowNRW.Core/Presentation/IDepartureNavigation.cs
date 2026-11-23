namespace FlowNRW.Core.Presentation;

/// <summary>Navigation to the selected stop monitor.</summary>
public interface IDepartureNavigation
{
    /// <summary>Shows the monitor for the current session stop.</summary>
    /// <returns>Navigation completion.</returns>
    Task ShowMonitorAsync();
}
