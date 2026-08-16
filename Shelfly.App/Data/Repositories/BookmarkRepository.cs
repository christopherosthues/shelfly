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
            bookmark.DeletionStatus = Shelfly.Common.Enums.DeletionStatus.SoftDeleted;
            await UpdateAsync(bookmark);
        }
    }
}
