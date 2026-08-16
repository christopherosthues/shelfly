using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data.Repositories;

public class BookmarkRepository(LocalDbContext dbContext) : IBookmarkRepository
{
    public async Task<List<LocalBookmark>> GetAllAsync()
    {
        return await dbContext.LocalBookmarks
            .Include(bm => bm.LocalBook)
            .ToListAsync();
    }

    public async Task<List<LocalBookmark>> GetByBookIdAsync(Guid bookId)
    {
        return await dbContext.LocalBookmarks
            .Where(bm => bm.LocalBookId == bookId)
            .ToListAsync();
    }

    public async Task<LocalBookmark?> GetByIdAsync(Guid id)
    {
        return await dbContext.LocalBookmarks
            .Include(bm => bm.LocalBook)
            .FirstOrDefaultAsync(bm => bm.LocalGuid == id);
    }

    public async Task<LocalBookmark> AddAsync(LocalBookmark bookmark)
    {
        dbContext.LocalBookmarks.Add(bookmark);
        await dbContext.SaveChangesAsync();
        return bookmark;
    }

    public async Task UpdateAsync(LocalBookmark bookmark)
    {
        bookmark.LastModified = DateTimeOffset.UtcNow;
        dbContext.LocalBookmarks.Update(bookmark);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        LocalBookmark? bookmark = await GetByIdAsync(id);
        if (bookmark != null)
        {
            bookmark.DeletedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(bookmark);
        }
    }

    public async Task RestoreAsync(Guid id)
    {
        LocalBookmark? bookmark = await GetByIdAsync(id);
        if (bookmark != null && bookmark.DeletedAt != null)
        {
            bookmark.DeletedAt = null;
            await UpdateAsync(bookmark);
        }
    }

    public async Task<List<LocalBookmark>> GetTrashItemsAsync()
    {
        return await dbContext.LocalBookmarks
            .Include(bm => bm.LocalBook)
            .Where(bm => bm.DeletedAt != null)
            .ToListAsync();
    }

    public async Task CleanupExpiredAsync(DateTimeOffset cutoffTime)
    {
        var expired = await dbContext.LocalBookmarks
            .Where(bm => bm.DeletedAt != null && bm.DeletedAt <= cutoffTime)
            .Select(bm => bm.LocalGuid)
            .ToListAsync();

        foreach (var guid in expired)
        {
            dbContext.LocalBookmarks.RemoveRange(dbContext.LocalBookmarks.Where(bm => bm.LocalGuid == guid));
        }

        await dbContext.SaveChangesAsync();
    }
}
