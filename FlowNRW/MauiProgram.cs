using Microsoft.Extensions.Logging;

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

        return builder.Build();
    }
}
