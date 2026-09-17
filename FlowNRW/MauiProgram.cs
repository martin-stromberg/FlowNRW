using Microsoft.Extensions.Logging;
using FlowNRW.Core.Transit;
using FlowNRW.Core.Presentation;

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

        return builder.Build();
    }
}
