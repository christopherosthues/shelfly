using Shelfly.Api.Authentication.Validators;
using Shelfly.Api.Books;
using Shelfly.Api.Features.Books.Endpoints;
using Shelfly.Api.Features.Books.Validators;
using Shelfly.Api.Models;
using FluentValidation;

namespace Shelfly.Api.Shared.DI;

public static class BooksFeatureExtensions
{
    public static IServiceCollection AddBooksFeature(this IServiceCollection services)
    {
        // Register book-related services with DI container
        services.AddScoped<BookService>();

        // Register validators for book requests
        services.AddScoped<IValidator<CreateBookRequest>, CreateBookValidator>();
        services.AddScoped<IValidator<UpdateBookRequest>, UpdateBookValidator>();
        services.AddScoped<IValidator<BookStatusUpdateRequest>, BookStatusUpdateValidator>();

        return services;
    }

    public static WebApplication MapBooksFeatureEndpoints(this WebApplication app)
    {
        // Map all book-related endpoints
        app.MapBookEndpoints();
        app.MapBookStatusEndpoints();

        return app;
    }
}
