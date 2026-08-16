using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shelfly.App.Authentication;
using Shelfly.App.Books;
using Shelfly.App.Data;
using Shelfly.App.Data.Repositories;
using Shelfly.App.Routing;
using Shelfly.App.Services;
using AddEditBookViewModel = Shelfly.App.Books.AddEditBookViewModel;
using BookDetailViewModel = Shelfly.App.Books.BookDetailViewModel;
using BooksViewModel = Shelfly.App.Books.BooksViewModel;

namespace Shelfly.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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
#endif

        builder.Services.AddSingleton<AuthTokenHandler>(handler =>
        {
            HttpClientHandler innerHandler = new()
            {
                // TODO: URL input by user
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            return new AuthTokenHandler(innerHandler);
        });

        builder.Services.AddScoped<BookApiService>(services =>
        {
            AuthTokenHandler handler = services.GetRequiredService<AuthTokenHandler>();
            HttpClient client = new(handler)
            {
                // TODO: URL input by user
                BaseAddress = new("http://localhost:5000/")
            };
            return new BookApiService(client);
        });

        builder.Services.AddScoped<BookmarkApiService>(services =>
        {
            AuthTokenHandler handler = services.GetRequiredService<AuthTokenHandler>();
            HttpClient client = new(handler)
            {
                // TODO: URL input by user
                BaseAddress = new("http://localhost:5000/")
            };
            return new BookmarkApiService(client);
        });
        builder.Services.AddHttpClient();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shelfly_local.db");
        builder.Services.AddDbContext<LocalDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        builder.Services.AddScoped<IRemoteMappingRepository, RemoteMappingRepository>();

        // Sync and Trash services
        builder.Services.AddSingleton<ServerConnectionService>();
        builder.Services.AddScoped<SyncService>();
        builder.Services.AddScoped<ConflictResolver>();
        builder.Services.AddScoped<TrashService>();

        builder.Services.AddScopedWithShellRoute<LoginPage, LoginViewModel>(Routes.LoginPage);
        builder.Services.AddScopedWithShellRoute<RegistrationPage, RegistrationViewModel>(Routes.RegistrationPage);

        builder.Services.AddScopedWithShellRoute<BooksPage, BooksViewModel>(Routes.BooksPage);
        builder.Services.AddScopedWithShellRoute<BookDetailPage, BookDetailViewModel>(Routes.BookDetailPage);
        builder.Services.AddScopedWithShellRoute<AddEditBookPage, AddEditBookViewModel>(Routes.AddEditBookPage);

        return builder.Build();
    }
}
