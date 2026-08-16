using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;

namespace Shelfly.Api.Services;

public class SoftDeleteService(ShelflyDbContext context, IConfiguration configuration) : BackgroundService
{
    private TimeSpan _retentionPeriod = TimeSpan.FromDays(30);
    private const string ConfigSectionName = "TrashConfig";

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Load retention period from configuration
        int? retentionDays = configuration.GetValue<int?>($"{ConfigSectionName}:RetentionDays");
        if (retentionDays.HasValue && retentionDays.Value > 0)
        {
            _retentionPeriod = TimeSpan.FromDays(retentionDays.Value);
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Perform periodic hard delete job
                await HardDeleteExpiredItemsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SoftDeleteService error: {ex.Message}");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    public async Task HardDeleteExpiredItemsAsync()
    {
        DateTimeOffset cutoffTime = DateTimeOffset.UtcNow - _retentionPeriod;

        // Find books that have been soft-deleted beyond retention period
        List<Data.Entities.BookEntity>? expiredBooks = await context.Books
            .Where(b => b.SoftDeletedAt.HasValue && b.SoftDeletedAt <= cutoffTime)
            .ToListAsync();

        foreach (Data.Entities.BookEntity book in expiredBooks ?? [])
        {
            // Set HardDeletedAt timestamp before removal
            book.HardDeletedAt = DateTimeOffset.UtcNow;

            // Remove associated bookmarks first
            List<Data.Entities.BookmarkEntity>? expiredBookmarks = await context.Bookmarks
                .Where(bm => bm.BookId == book.Id && bm.SoftDeletedAt.HasValue)
                .ToListAsync();

            foreach (Data.Entities.BookmarkEntity bookmark in expiredBookmarks ?? [])
            {
                bookmark.HardDeletedAt = DateTimeOffset.UtcNow;
            }

            // Remove from database
            context.Bookmarks.RemoveRange(expiredBookmarks ?? []);
            context.Books.Remove(book);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Data.Entities.BookEntity?> RestoreFromTrashAsync(Guid bookId)
    {
        Data.Entities.BookEntity? book = await context.Books
            .FirstOrDefaultAsync(b => b.Id == bookId && b.SoftDeletedAt.HasValue);

        if (book != null)
        {
            // Clear SoftDeletedAt to restore the book
            book.SoftDeletedAt = null;
            book.LastModified = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync();
        }

        return book;
    }
}
