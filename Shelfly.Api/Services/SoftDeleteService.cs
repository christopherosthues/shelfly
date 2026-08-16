using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;

namespace Shelfly.Api.Services;

public class CleanupService(ShelflyDbContext context, IConfiguration configuration, ILogger<CleanupService> logger) : BackgroundService
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
                logger.LogError(ex, "CleanupService error during hard delete operation");
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
            .Where(b => b.DeletedAt != null && b.DeletedAt <= cutoffTime)
            .ToListAsync();

        foreach (Data.Entities.BookEntity book in expiredBooks ?? [])
        {
            // Remove associated bookmarks first (cascade delete will handle this, but explicit removal is clearer)
            List<Data.Entities.BookmarkEntity>? expiredBookmarks = await context.Bookmarks
                .Where(bm => bm.BookId == book.Id && bm.DeletedAt != null)
                .ToListAsync();

            // Remove from database (physical row deletion — hard delete)
            context.Bookmarks.RemoveRange(expiredBookmarks ?? []);
            context.Books.Remove(book);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Data.Entities.BookEntity?> RestoreFromTrashAsync(Guid bookId)
    {
        Data.Entities.BookEntity? book = await context.Books
            .FirstOrDefaultAsync(b => b.Id == bookId && b.DeletedAt != null);

        if (book != null)
        {
            // Clear deletion timestamp to restore the book (preserve LastModified)
            book.DeletedAt = null;

            await context.SaveChangesAsync();
        }

        return book;
    }
}
