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
    /// <param name="home">Favorite departure boards.</param>
    public AppShell(SearchPage search, HomePage home)
    {
        InitializeComponent();
        Items.Add(new ShellContent { Title = "Start", Route = "home", Content = home });
        Routing.RegisterRoute("search", typeof(SearchPage));
        Routing.RegisterRoute("results", typeof(ResultsPage));
        Routing.RegisterRoute("detail", typeof(JourneyDetailPage));
        Routing.RegisterRoute("stops", typeof(StopSearchPage));
        Routing.RegisterRoute("departures", typeof(DeparturePage));
        Routing.RegisterRoute("map", typeof(MapPage));
        search.ToolbarItems.Add(new ToolbarItem
        {
            Text = "Abfahrten",
            AutomationId = "OpenStopSearch",
            Command = new FlowNRW.Core.Presentation.AsyncRelayCommand(() => GoToAsync("stops"), () => true,
                _ => search.Title = "Haltestellensuche konnte nicht geöffnet werden")
        });
    }
}
