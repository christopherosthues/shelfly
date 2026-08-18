using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NLog;

namespace Shelfly.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "nlog.config");
        if (File.Exists(configPath))
        {
            NLog.LogManager.Setup().LoadConfigurationFromFile(configPath);
        }

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#else
        builder.Logging.ClearProviders().AddNLog();
#endif

        return builder.Build();
    }
}
