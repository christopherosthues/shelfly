using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Shelfly.App.Authentication;
using Shelfly.App.Books;
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
            HttpClient innerClient = new()
            {
                BaseAddress = new("http://localhost:5000/")
            };
            return new AuthTokenHandler(innerClient);
        });

        builder.Services.AddScoped<BookApiService>(services =>
        {
            AuthTokenHandler handler = services.GetRequiredService<AuthTokenHandler>();
            HttpClient client = new(handler)
            {
                BaseAddress = new("http://localhost:5000/")
            };
            return new BookApiService(client);
        });

        builder.Services.AddScoped<BookmarkApiService>(services =>
        {
            AuthTokenHandler handler = services.GetRequiredService<AuthTokenHandler>();
            HttpClient client = new(handler)
            {
                BaseAddress = new("http://localhost:5000/")
            };
            return new BookmarkApiService(client);
        });
        builder.Services.AddHttpClient();

        builder.Services.AddScopedWithShellRoute<LoginPage, LoginViewModel>(Routes.LoginPage);
        builder.Services.AddScopedWithShellRoute<RegistrationPage, RegistrationViewModel>(Routes.RegistrationPage);

        builder.Services.AddScopedWithShellRoute<BooksPage, BooksViewModel>(Routes.BooksPage);
        builder.Services.AddScopedWithShellRoute<BookDetailPage, BookDetailViewModel>(Routes.BookDetailPage);
        builder.Services.AddScopedWithShellRoute<AddEditBookPage, AddEditBookViewModel>(Routes.AddEditBookPage);

        return builder.Build();
    }
}
