using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Features.Books.Validators;

namespace Shelfly.Api.Features.Books.Endpoints;

public static class BookStatusEndpoints
{
    private static Guid ExtractUserId(HttpContext context)
    {
        string? subClaim = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (Guid.TryParse(subClaim, out Guid userId))
        {
            return userId;
        }

        return Guid.CreateVersion7();
    }

    extension(WebApplication app)
    {
        public WebApplication MapBookStatusEndpoints()
        {
            // PATCH /books/{id}/status — change deletion state (soft-delete or restore via DeletedAt timestamp)
            app.MapPatch("/books/{id}/status",
                async (Guid id, BookStatusUpdateRequest request, HttpContext httpContext, IValidator<BookStatusUpdateRequest> validator, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    ValidationResult validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    // Find the book owned by this user
                    var existingBook = await context.Books
                        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                    if (existingBook is null)
                    {
                        return Results.NotFound();
                    }

                    // Update deletion timestamp based on request status
                    existingBook.DeletedAt = request.Status == "SoftDeleted" ? DateTimeOffset.UtcNow : null;
                    existingBook.LastModified = DateTimeOffset.UtcNow;

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Id = existingBook.Id,
                        Title = existingBook.Title,
                        Author = existingBook.Author,
                        DeletedAt = existingBook.DeletedAt,
                        LastModified = existingBook.LastModified
                    });
                }).RequireAuthorization();

            // GET /books/trash — list soft-deleted books (trash)
            app.MapGet("/books/trash",
                async (HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var trashBooks = await context.Books
                        .Where(b => b.UserId == userId && b.DeletedAt != null)
                        .Select(b => new
                        {
                            Id = b.Id,
                            Title = b.Title,
                            Author = b.Author,
                            ISBN = b.ISBN,
                            DeletedAt = b.DeletedAt,
                            LastModified = b.LastModified
                        })
                        .ToListAsync();

                    return Results.Ok(trashBooks ?? []);
                }).RequireAuthorization();

            // POST /books/{id}/restore — restore a soft-deleted book from trash (clear DeletedAt timestamp)
            app.MapPost("/books/{id}/restore",
                async (Guid id, HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var existingBook = await context.Books
                        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.DeletedAt != null);

                    if (existingBook is null)
                    {
                        return Results.NotFound();
                    }

                    // Restore the book by clearing deletion timestamp (preserve LastModified)
                    existingBook.DeletedAt = null;

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Id = existingBook.Id,
                        Title = existingBook.Title,
                        Author = existingBook.Author,
                        DeletedAt = existingBook.DeletedAt,
                        LastModified = existingBook.LastModified
                    });
                }).RequireAuthorization();

            // POST /books/bulk-delete — soft-delete multiple books at once (set DeletedAt timestamp)
            app.MapPost("/books/bulk-delete",
                async (List<Guid> ids, HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var booksToUpdate = await context.Books
                        .Where(b => b.UserId == userId && ids.Contains(b.Id))
                        .ToListAsync();

                    foreach (var book in booksToUpdate ?? [])
                    {
                        book.DeletedAt = DateTimeOffset.UtcNow;
                    }

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Count = booksToUpdate?.Count ?? 0,
                        Message = $"Soft-deleted {booksToUpdate?.Count ?? 0} books"
                    });
                }).RequireAuthorization();

            // POST /books/bulk-restore — restore multiple soft-deleted books from trash (clear DeletedAt timestamp)
            app.MapPost("/books/bulk-restore",
                async (List<Guid> ids, HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var booksToUpdate = await context.Books
                        .Where(b => b.UserId == userId && ids.Contains(b.Id) && b.DeletedAt != null)
                        .ToListAsync();

                    foreach (var book in booksToUpdate ?? [])
                    {
                        book.DeletedAt = null;
                    }

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Count = booksToUpdate?.Count ?? 0,
                        Message = $"Restored {booksToUpdate?.Count ?? 0} books from trash"
                    });
                }).RequireAuthorization();

            return app;
        }
    }
}
