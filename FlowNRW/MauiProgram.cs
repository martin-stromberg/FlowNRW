using Microsoft.Extensions.Logging;
using FlowNRW.Core.Transit;
using FlowNRW.Core.Presentation;
using FlowNRW.Core.Maps;

namespace FlowNRW;

/// <summary>
/// Composes the MAUI application (fonts, logging, services).
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds the configured <see cref="MauiApp"/>.
    /// </summary>
    /// <returns>The application instance to run.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
#if WINDOWS
        Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", Path.Combine(FileSystem.AppDataDirectory, "MapWebView"));
#endif
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var providerOptions = new TransitProviderOptions
        {
            DbRestBaseUrl = new Uri(builder.Configuration["TransitProviders:DbRest:BaseUrl"] ?? "https://v6.db.transport.rest/"),
            EfaBaseUrl = new Uri(builder.Configuration["TransitProviders:Efa:BaseUrl"] ?? "https://openservice-test.vrr.de/openservice/"),
            EfaFallbackBaseUrl = Uri.TryCreate(builder.Configuration["TransitProviders:Efa:FallbackBaseUrl"], UriKind.Absolute, out var fallback) ? fallback : null,
            EfaFormat = builder.Configuration["TransitProviders:Efa:Format"] ?? "rapidJSON",
            Timeout = TimeSpan.Parse(builder.Configuration["TransitHttp:Timeout"] ?? "00:00:10", System.Globalization.CultureInfo.InvariantCulture),
            MaxRetries = int.Parse(builder.Configuration["TransitHttp:MaxRetries"] ?? "1", System.Globalization.CultureInfo.InvariantCulture),
            MaxResults = int.Parse(builder.Configuration["TransitProvider:MaxResults"] ?? "100", System.Globalization.CultureInfo.InvariantCulture)
        };
        var cacheOptions = new TransitCacheOptions
        {
            MaxEntries = int.Parse(builder.Configuration["TransitCache:MaxEntries"] ?? "256", System.Globalization.CultureInfo.InvariantCulture),
            StopTimeToLive = TimeSpan.Parse(builder.Configuration["TransitCache:StopTimeToLive"] ?? "1.00:00:00", System.Globalization.CultureInfo.InvariantCulture),
            RealtimeTimeToLive = TimeSpan.Parse(builder.Configuration["TransitCache:RealtimeTimeToLive"] ?? "00:00:30", System.Globalization.CultureInfo.InvariantCulture),
            MaxStaleAge = TimeSpan.Parse(builder.Configuration["TransitCache:MaxStaleAge"] ?? "00:05:00", System.Globalization.CultureInfo.InvariantCulture)
        };
        providerOptions.Validate();
        cacheOptions.Validate();
        builder.Services.AddSingleton(providerOptions);
        builder.Services.AddSingleton(cacheOptions);
        builder.Services.AddSingleton(new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = Timeout.InfiniteTimeSpan });
        builder.Services.AddSingleton<RetryPolicy>();
        builder.Services.AddSingleton<ITransitDiagnostics, TransitDiagnostics>();
        builder.Services.AddSingleton<ITransitHttpGateway, TransitHttpGateway>();
        builder.Services.AddSingleton<ITransitCache, MemoryTransitCache>();
        builder.Services.AddSingleton<INrwRegionClassifier, NrwRegionClassifier>();
        builder.Services.AddSingleton<IRealtimeConsolidator, RealtimeConsolidator>();
        builder.Services.AddSingleton<DbRestResponseMapper>();
        builder.Services.AddSingleton<EfaResponseMapper>();
        builder.Services.AddSingleton<DbRestProvider>();
        builder.Services.AddSingleton<IEfaProvider, EfaProvider>();
        builder.Services.AddSingleton<IProviderOrchestrator>(services => new ProviderOrchestrator(
            services.GetRequiredService<DbRestProvider>(), services.GetRequiredService<IEfaProvider>(),
            services.GetRequiredService<INrwRegionClassifier>(), services.GetRequiredService<IRealtimeConsolidator>(),
            services.GetRequiredService<ITransitCache>(), cacheOptions, providerOptions));
        builder.Services.AddTransient<IStopSearchService, StopSearchService>();
        builder.Services.AddTransient<IRoutingService, RoutingService>();
        builder.Services.AddTransient<IDepartureService, DepartureService>();
#if UI_TEST_FIXTURES
        builder.Services.AddTransient<IStopSearchService, UiTestFixtureServices>();
        builder.Services.AddTransient<IRoutingService, UiTestFixtureServices>();
        builder.Services.AddTransient<IDepartureService, UiTestFixtureServices>();
#endif
        builder.Services.AddSingleton<IJourneyNavigation, ShellJourneyNavigation>();
        builder.Services.AddSingleton(services => new JourneySearchViewModel(
            new EndpointViewModel(services.GetRequiredService<IStopSearchService>(), providerOptions.MaxSearchLength),
            new EndpointViewModel(services.GetRequiredService<IStopSearchService>(), providerOptions.MaxSearchLength),
            services.GetRequiredService<IRoutingService>(), services.GetRequiredService<IJourneyNavigation>()));
        builder.Services.AddSingleton<ResultsViewModel>();
        builder.Services.AddSingleton<JourneyDetailViewModel>();
        builder.Services.AddSingleton<SearchPage>();
        builder.Services.AddTransient<ResultsPage>();
        builder.Services.AddTransient<JourneyDetailPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<IDepartureNavigation, ShellDepartureNavigation>();
        builder.Services.AddSingleton(services => new StopMonitorViewModel(
            services.GetRequiredService<IStopSearchService>(), services.GetRequiredService<IDepartureService>(),
            services.GetRequiredService<IDepartureNavigation>(), providerOptions.MaxSearchLength));
        builder.Services.AddSingleton<StopSearchPage>();
        builder.Services.AddTransient<DeparturePage>();
        var mapOptions = new MapOptions
        {
            TileUrl = builder.Configuration["Map:TileUrl"] ?? "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
            Attribution = builder.Configuration["Map:Attribution"] ?? "© OpenStreetMap contributors · openstreetmap.org/copyright",
            MinZoom = int.Parse(builder.Configuration["Map:MinZoom"] ?? "1", System.Globalization.CultureInfo.InvariantCulture),
            MaxZoom = int.Parse(builder.Configuration["Map:MaxZoom"] ?? "18", System.Globalization.CultureInfo.InvariantCulture),
            CacheMaxBytes = long.Parse(builder.Configuration["Map:CacheMaxBytes"] ?? "67108864", System.Globalization.CultureInfo.InvariantCulture),
            Timeout = TimeSpan.Parse(builder.Configuration["Map:Timeout"] ?? "00:00:10", System.Globalization.CultureInfo.InvariantCulture)
        };
        mapOptions.Validate();
        builder.Services.AddSingleton(mapOptions);
        builder.Services.AddSingleton<IMapTileService>(_ => new MapTileService(
            new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = Timeout.InfiniteTimeSpan }, mapOptions,
            Path.Combine(FileSystem.CacheDirectory, "MapTiles")));
        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddTransient<MapPage>();
#if UI_TEST_FIXTURES
        builder.Services.AddSingleton<IMapTileService, UiTestMapTiles>();
#endif

        return builder.Build();
    }
}
