using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data.Repositories;

public class BookRepository(LocalDbContext dbContext) : IBookRepository
{
    public async Task<List<LocalBook>> GetAllAsync()
    {
        return await dbContext.LocalBooks
            .Include(b => b.RemoteMappings)
            .Include(b => b.LocalBookmarks)
            .ToListAsync();
    }

    public async Task<LocalBook?> GetByIdAsync(Guid id)
    {
        return await dbContext.LocalBooks
            .Include(b => b.RemoteMappings)
            .Include(b => b.LocalBookmarks)
            .FirstOrDefaultAsync(b => b.LocalGuid == id);
    }

    public async Task<LocalBook> AddAsync(LocalBook book)
    {
        dbContext.LocalBooks.Add(book);
        await dbContext.SaveChangesAsync();
        return book;
    }

    public async Task UpdateAsync(LocalBook book)
    {
        book.LastModified = DateTimeOffset.UtcNow;
        dbContext.LocalBooks.Update(book);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        LocalBook? book = await GetByIdAsync(id);
        if (book != null)
        {
            book.DeletedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(book);
        }
    }

    public async Task RestoreAsync(Guid id)
    {
        LocalBook? book = await GetByIdAsync(id);
        if (book != null && book.DeletedAt != null)
        {
            book.DeletedAt = null;
            await UpdateAsync(book);
        }
    }

    public async Task<List<LocalBook>> GetTrashItemsAsync()
    {
        return await dbContext.LocalBooks
            .Include(b => b.RemoteMappings)
            .Include(b => b.LocalBookmarks)
            .Where(b => b.DeletedAt != null)
            .ToListAsync();
    }

    public async Task CleanupExpiredAsync(DateTimeOffset cutoffTime)
    {
        var expired = await dbContext.LocalBooks
            .Where(b => b.DeletedAt != null && b.DeletedAt <= cutoffTime)
            .Select(b => b.LocalGuid)
            .ToListAsync();

        foreach (var guid in expired)
        {
            dbContext.LocalBooks.RemoveRange(dbContext.LocalBooks.Where(b => b.LocalGuid == guid));
        }

        await dbContext.SaveChangesAsync();
    }
}
