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
    /// <param name="stops">Station search and nearby stops.</param>
    public AppShell(SearchPage search, HomePage home, StopSearchPage stops)
    {
        InitializeComponent();
        var tabs = new TabBar { Route = "main" };
        tabs.Items.Add(new Tab { Title = "Abfahrten", Route = "departures-tab", Items = { new ShellContent { Title = "Abfahrten", Route = "home", Content = home } } });
        tabs.Items.Add(new Tab { Title = "Verbindungen", Route = "connections-tab", Items = { new ShellContent { Title = "Verbindungen", Route = "search", Content = search } } });
        tabs.Items.Add(new Tab { Title = "Haltestellen", Route = "stations-tab", Items = { new ShellContent { Title = "Haltestellen", Route = "stops", Content = stops } } });
        Items.Add(tabs);
        Routing.RegisterRoute("results", typeof(ResultsPage));
        Routing.RegisterRoute("detail", typeof(JourneyDetailPage));
        Routing.RegisterRoute("departures", typeof(DeparturePage));
        Routing.RegisterRoute("map", typeof(MapPage));
        Routing.RegisterRoute("refresh-settings", typeof(RefreshSettingsPage));
        search.ToolbarItems.Add(new ToolbarItem
        {
            Text = "Haltestellen",
            AutomationId = "OpenStopSearch",
            Command = new FlowNRW.Core.Presentation.AsyncRelayCommand(() => GoToAsync("//main/stations-tab/stops"), () => true,
                _ => search.Title = "Haltestellensuche konnte nicht geöffnet werden")
        });
    }
}
