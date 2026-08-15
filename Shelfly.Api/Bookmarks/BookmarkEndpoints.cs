using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Shelfly.Api.Books;
using Shelfly.Api.Models;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Bookmarks;

public static class BookmarkEndpoints
{
    extension(WebApplication app)
    {
        public WebApplication MapBookmarkEndpoints()
        {
            app.MapGet("/bookmarks/{bookId}",
                async (Guid bookId, HttpContext httpContext, BookService bookService, BookmarkService bookmarkService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    Book? book = await bookService.GetBookAsync(userId, bookId);
                    if (book is null)
                    {
                        return Results.NotFound();
                    }

                    List<Bookmark> bookmarks = await bookmarkService.GetBookmarksAsync(userId, bookId);
                    return Results.Ok(bookmarks);
                })
                .RequireAuthorization();

            app.MapPost("/bookmarks/{bookId}",
                async (Guid bookId, CreateBookmarkRequest request, HttpContext httpContext, IValidator<CreateBookmarkRequest> validator, BookService bookService, BookmarkService bookmarkService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    ValidationResult validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    Book? book = await bookService.GetBookAsync(userId, bookId);
                    if (book is null)
                    {
                        return Results.NotFound();
                    }

                    Bookmark bookmark = new()
                    {
                        StartPage = request.StartPage,
                        EndPage = request.EndPage,
                        Note = request.Note
                    };

                    await bookmarkService.AddBookmarkAsync(userId, bookId, bookmark);

                    return Results.Created($"/api/bookmarks/{bookId}", bookmark);
                })
                .RequireAuthorization();

            app.MapPut("/bookmarks/{id}",
                async (Guid id, UpdateBookmarkRequest request, HttpContext httpContext, IValidator<UpdateBookmarkRequest> validator, BookmarkService bookmarkService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    ValidationResult validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    Bookmark? existingBookmark = await bookmarkService.GetBookmarkAsync(userId, id);
                    if (existingBookmark is null)
                    {
                        return Results.NotFound();
                    }

                    Bookmark bookmark = new()
                    {
                        Id = id,
                        StartPage = request.StartPage,
                        EndPage = request.EndPage,
                        Note = request.Note
                    };

                    await bookmarkService.UpdateBookmarkAsync(userId, id, bookmark);

                    return Results.Ok(bookmark);
                })
                .RequireAuthorization();

            app.MapDelete("/bookmarks/{id}",
                async (Guid id, HttpContext httpContext, BookmarkService bookmarkService) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    Bookmark? existingBookmark = await bookmarkService.GetBookmarkAsync(userId, id);
                    if (existingBookmark is null)
                    {
                        return Results.NotFound();
                    }

                    await bookmarkService.DeleteBookmarkAsync(userId, id);

                    return Results.NoContent();
                })
                .RequireAuthorization();

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
