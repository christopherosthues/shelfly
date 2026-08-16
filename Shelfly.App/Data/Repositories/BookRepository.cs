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
            book.DeletionStatus = Shelfly.Common.Enums.DeletionStatus.SoftDeleted;
            await UpdateAsync(book);
        }
    }
}
