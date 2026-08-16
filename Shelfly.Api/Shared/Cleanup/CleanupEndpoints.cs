using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Services;
using Shelfly.Common.Enums;

namespace Shelfly.Api.Shared.Cleanup;

public static class CleanupEndpoints
{
    extension(WebApplication app)
    {
        public WebApplication MapCleanupEndpoints()
        {
            // POST /cleanup/run — manually trigger trash cleanup past retention period
            app.MapPost("/cleanup/run",
                async (CleanupService cleanupService) =>
                {
                    await cleanupService.HardDeleteExpiredItemsAsync();

                    return Results.Ok(new
                    {
                        Message = "Trash cleanup completed successfully"
                    });
                }).RequireAuthorization();

            // POST /books/{id}/cleanup — permanently delete a specific soft-deleted book (hard delete)
            app.MapPost("/books/{id}/cleanup",
                async (Guid id, HttpContext httpContext, ShelflyDbContext context) =>
                {
                    Guid userId = ExtractUserId(httpContext);

                    var existingBook = await context.Books
                        .Include(b => b.Bookmarks)
                        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.DeletionStatus == DeletionStatus.SoftDeleted);

                    if (existingBook is null)
                    {
                        return Results.NotFound();
                    }

                    // Remove associated bookmarks first (physical row deletion — hard delete)
                    var softDeletedBookmarks = existingBook.Bookmarks
                        .Where(bm => bm.DeletionStatus == DeletionStatus.SoftDeleted)
                        .ToList();

                    context.Bookmarks.RemoveRange(softDeletedBookmarks);
                    context.Books.Remove(existingBook);

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        Message = $"Book '{existingBook.Title}' and its soft-deleted bookmarks permanently removed"
                    });
                }).RequireAuthorization();

            return app;
        }

        private static Guid ExtractUserId(HttpContext context)
        {
            string? subClaim = context.User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (Guid.TryParse(subClaim, out Guid userId))
            {
                return userId;
            }

            return Guid.CreateVersion7();
        }
    }
}
