using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
#if RELEASE
using NLog.Extensions.Logging;
#endif
using Shelfly.App.Data;
using Shelfly.App.Features.About;
using Shelfly.App.Features.BookEditor;
using Shelfly.App.Features.BookmarkEditor;
using Shelfly.App.Features.Library;
using Shelfly.App.Features.Trash;
using Shelfly.App.Migrations;
using Shelfly.App.Services;

namespace Shelfly.App;

public static class MauiProgram
{

    public static MauiApp CreateMauiApp()
    {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
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

        builder.Services.AddDbContext<LocalDbContext>(options =>
        {
            string databasePath = Path.Combine(FileSystem.AppDataDirectory, "shelfly.db");
            options.UseSqlite($"Data Source={databasePath}",
                sql => sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
            options.AddInterceptors(new AuditTimestampInterceptor());
        });
        builder.Services.AddScoped<AuditTimestampInterceptor>();
        builder.Services.AddScoped<LibraryService>();
        builder.Services.AddScoped<LibraryExportService>();
        builder.Services.AddScoped<TrashService>();
        builder.Services.AddScoped<LicenseDataService>();

        builder.Services.AddScopedWithShellRoute<BookListPage, BookListViewModel>(Routes.BookListPage);
        builder.Services.AddScopedWithShellRoute<BookEditPage, BookEditViewModel>(Routes.BookEditPage);
        builder.Services.AddScopedWithShellRoute<BookDetailPage, BookDetailViewModel>(Routes.BookDetailPage);
        builder.Services.AddScopedWithShellRoute<BookmarkEditPage, BookmarkEditViewModel>(Routes.BookmarkEditPage);
        builder.Services.AddScopedWithShellRoute<TrashListPage, TrashListViewModel>(Routes.TrashListPage);
        builder.Services.AddScopedWithShellRoute<TrashBookDetailPage, TrashBookDetailViewModel>(Routes.TrashBookDetailPage);
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddScopedWithShellRoute<AboutPage, AboutViewModel>(Routes.AboutPage);

        return builder.Build();
    }
}
