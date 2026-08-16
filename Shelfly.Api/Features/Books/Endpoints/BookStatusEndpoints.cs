using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Features.Books.Validators;
using Shelfly.Common.Enums;

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
            // PATCH /books/{id}/status — change DeletionStatus between Active and SoftDeleted
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

                    // Update deletion status and last modified timestamp
                    existingBook.DeletionStatus = request.Status;
                    existingBook.LastModified = DateTimeOffset.UtcNow;

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Id = existingBook.Id,
                        Title = existingBook.Title,
                        Author = existingBook.Author,
                        DeletionStatus = existingBook.DeletionStatus.ToString(),
                        LastModified = existingBook.LastModified
                    });
                }).RequireAuthorization();

            // GET /books/trash — list soft-deleted books (trash)
            app.MapGet("/books/trash",
                async (HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var trashBooks = await context.Books
                        .Where(b => b.UserId == userId && b.DeletionStatus == DeletionStatus.SoftDeleted)
                        .Select(b => new
                        {
                            Id = b.Id,
                            Title = b.Title,
                            Author = b.Author,
                            ISBN = b.ISBN,
                            LastModified = b.LastModified
                        })
                        .ToListAsync();

                    return Results.Ok(trashBooks ?? []);
                }).RequireAuthorization();

            // POST /books/{id}/restore — restore a soft-deleted book from trash to Active status
            app.MapPost("/books/{id}/restore",
                async (Guid id, HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var existingBook = await context.Books
                        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.DeletionStatus == DeletionStatus.SoftDeleted);

                    if (existingBook is null)
                    {
                        return Results.NotFound();
                    }

                    // Restore the book by setting status to Active
                    existingBook.DeletionStatus = DeletionStatus.Active;
                    existingBook.LastModified = DateTimeOffset.UtcNow;

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Id = existingBook.Id,
                        Title = existingBook.Title,
                        Author = existingBook.Author,
                        DeletionStatus = existingBook.DeletionStatus.ToString(),
                        LastModified = existingBook.LastModified
                    });
                }).RequireAuthorization();

            return app;
        }
    }
}
