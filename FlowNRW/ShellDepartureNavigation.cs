using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Shell navigation for the selected stop without identity in route parameters.</summary>
public sealed class ShellDepartureNavigation : IDepartureNavigation
{
    /// <inheritdoc />
    public Task ShowMonitorAsync() => Shell.Current.GoToAsync("departures");
}
