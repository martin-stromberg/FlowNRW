namespace FlowNRW;

/// <summary>
/// Root navigation shell of the application.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    /// <param name="search">Composed search page.</param>
    public AppShell(SearchPage search)
    {
        InitializeComponent();
        Items.Add(new ShellContent { Title = "Suche", Route = "search", Content = search });
        Routing.RegisterRoute("results", typeof(ResultsPage));
        Routing.RegisterRoute("detail", typeof(JourneyDetailPage));
        Routing.RegisterRoute("stops", typeof(StopSearchPage));
        Routing.RegisterRoute("departures", typeof(DeparturePage));
        search.ToolbarItems.Add(new ToolbarItem
        {
            Text = "Abfahrten",
            AutomationId = "OpenStopSearch",
            Command = new FlowNRW.Core.Presentation.AsyncRelayCommand(() => GoToAsync("stops"), () => true,
                _ => search.Title = "Haltestellensuche konnte nicht geöffnet werden")
        });
    }
}
