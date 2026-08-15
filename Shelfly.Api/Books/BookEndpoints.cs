using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Shelfly.Api.Models;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Books;

public static class BookEndpoints
{
    extension(WebApplication app)
    {
        public WebApplication MapBookEndpoints()
        {
            app.MapGet("/books",
                    async (HttpContext httpContext, BookService bookService) =>
                    {
                        Guid userId = ExtractUserId(httpContext);
                        return await bookService.GetBooksAsync(userId);
                    })
                .RequireAuthorization();

            app.MapGet("/books/{id}",
                async (Guid id, HttpContext httpContext, BookService bookService) =>
                {
                    Guid userId = ExtractUserId(httpContext);
                    return await bookService.GetBookAsync(userId, id);
                }).RequireAuthorization();

            app.MapPost("/books",
                async (CreateBookRequest request, HttpContext httpContext, IValidator<CreateBookRequest> validator, BookService bookService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    ValidationResult validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    Book book = new()
                    {
                        Title = request.Title,
                        Author = request.Author,
                        ISBN = request.ISBN,
                        PublishDate = request.PublishDate
                    };

                    await bookService.AddBookAsync(userId, book);

                    return Results.Created($"/api/books/{book.Id}", book);
                }).RequireAuthorization();

            app.MapPut("/books/{id}",
                async (Guid id, UpdateBookRequest request, HttpContext httpContext, IValidator<UpdateBookRequest> validator, BookService bookService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    ValidationResult validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    Book? existingBook = await bookService.GetBookAsync(userId, id);
                    if (existingBook is null)
                    {
                        return Results.NotFound();
                    }

                    Book book = new()
                    {
                        Id = id,
                        Title = request.Title,
                        Author = request.Author,
                        ISBN = request.ISBN,
                        PublishDate = request.PublishDate
                    };

                    await bookService.UpdateBookAsync(userId, book);

                    return Results.Ok(book);
                }).RequireAuthorization();

            app.MapDelete("/books/{id}",
                async (Guid id, HttpContext httpContext, BookService bookService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    Book? existingBook = await bookService.GetBookAsync(userId, id);
                    if (existingBook is null)
                    {
                        return Results.NotFound();
                    }

                    await bookService.DeleteBookAsync(userId, new Book { Id = id });

                    return Results.NoContent();
                }).RequireAuthorization();

            return app;
        }

        static Guid ExtractUserId(HttpContext context)
        {
            string? subClaim = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (Guid.TryParse(subClaim, out Guid userId))
            {
                return userId;
            }

            return Guid.CreateVersion7();
        }
    }
}
