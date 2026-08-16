using Shelfly.Api.Authentication.Validators;
using Shelfly.Api.Bookmarks;
using Shelfly.Api.Models;
using FluentValidation;

namespace Shelfly.Api.Shared.DI;

public static class BookmarksFeatureExtensions
{
    public static IServiceCollection AddBookmarksFeature(this IServiceCollection services)
    {
        // Register bookmark-related services with DI container
        services.AddScoped<BookmarkService>();

        // Register validators for bookmark requests
        services.AddScoped<IValidator<CreateBookmarkRequest>, CreateBookmarkValidator>();
        services.AddScoped<IValidator<UpdateBookmarkRequest>, UpdateBookmarkValidator>();

        return services;
    }

    public static WebApplication MapBookmarksFeatureEndpoints(this WebApplication app)
    {
        // Map all bookmark-related endpoints
        app.MapBookmarkEndpoints();

        return app;
    }
}
