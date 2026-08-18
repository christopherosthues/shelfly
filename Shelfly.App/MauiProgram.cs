using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NLog;
using Shelfly.App.Data;
using Shelfly.App.Features.BookEditor.Pages;
using Shelfly.App.Features.BookEditor.ViewModels;
using Shelfly.App.Features.BookmarkEditor.Pages;
using Shelfly.App.Features.BookmarkEditor.ViewModels;
using Shelfly.App.Features.Library.Pages;
using Shelfly.App.Features.Library.ViewModels;

namespace Shelfly.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "nlog.config");
        if (File.Exists(configPath))
        {
            LogManager.Setup().LoadConfigurationFromFile(configPath);
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

        builder.Services.AddSingleton<LocalDbContext>();
        builder.Services.AddSingleton<AuditTimestampInterceptor>();

        builder.Services.AddScopedWithShellRoute<BookListPage, BookListViewModel>("BookListPage");
        builder.Services.AddScopedWithShellRoute<BookEditPage, BookEditViewModel>("BookEditPage");
        builder.Services.AddScopedWithShellRoute<BookDetailPage, BookDetailViewModel>("BookDetailPage");
        builder.Services.AddScopedWithShellRoute<BookmarkEditPage, BookmarkEditViewModel>("BookmarkEditPage");

        return builder.Build();
    }
}
