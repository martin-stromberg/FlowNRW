using FlowNRW.Core.Transit;

namespace FlowNRW.Core.Presentation;

/// <summary>Results projected from the retained session.</summary>
public sealed class ResultsViewModel : ObservableObject
{
    /// <summary>Creates the results projection.</summary>
    /// <param name="session">Shared session.</param>
    public ResultsViewModel(JourneySearchViewModel session) { Session = session; }
    /// <summary>Retained search state.</summary>
    public JourneySearchViewModel Session { get; }
    /// <summary>Opens a current journey.</summary>
    /// <param name="journey">Selected result.</param>
    /// <returns>Navigation completion.</returns>
    public Task OpenJourneyAsync(Journey journey) => Session.OpenJourneyAsync(journey);
}
