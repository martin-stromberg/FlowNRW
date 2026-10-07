namespace FlowNRW.Core.Presentation;

/// <summary>Detail projection of the selected journey.</summary>
public sealed class JourneyDetailViewModel : ObservableObject
{
    /// <summary>Creates the detail projection.</summary>
    /// <param name="session">Shared session.</param>
    public JourneyDetailViewModel(JourneySearchViewModel session)
    {
        Session = session;
        session.PropertyChanged += (_, _) => Notify(nameof(Details));
    }
    /// <summary>Retained search state.</summary>
    public JourneySearchViewModel Session { get; }
    /// <summary>All supplied leg and transfer information.</summary>
    public string Details
    {
        get
        {
            return Session.SelectedJourney is null
                ? (string.IsNullOrWhiteSpace(Session.Status) ? "Keine Verbindung ausgewählt." : Session.Status)
                : JourneyPresentation.Detail(Session.SelectedJourney);
        }
    }
}
