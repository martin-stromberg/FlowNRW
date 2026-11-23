using FlowNRW.Core.Presentation;

namespace FlowNRW;

/// <summary>Shell adapter without endpoint route parameters.</summary>
public sealed class ShellJourneyNavigation : IJourneyNavigation
{
    /// <inheritdoc />
    public Task ShowResultsAsync() => Shell.Current.GoToAsync("results");
    /// <inheritdoc />
    public Task ShowDetailAsync() => Shell.Current.GoToAsync("detail");
}
