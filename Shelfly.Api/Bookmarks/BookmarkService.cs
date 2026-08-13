using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common;

namespace Shelfly.Api.Bookmarks;

public class BookmarkService(AppDbContext context)
{
    public async Task<List<Bookmark>> GetBookmarksAsync(Guid userId, Guid bookId)
    {
        return await context.Bookmarks
            .Where(b => b.BookId == bookId)
            .Select(b => new Bookmark
            {
                Id = b.Id,
                PageNumber = b.PageNumber
            })
            .ToListAsync();
    }

    public async Task<Bookmark?> GetBookmarkAsync(Guid userId, Guid bookmarkId)
    {
        BookmarkEntity? entity = await context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmarkId);
        return entity is null ? null : new Bookmark
        {
            Id = entity.Id,
            PageNumber = entity.PageNumber
        };
    }

    public async Task AddBookmarkAsync(Guid userId, Guid bookId, Bookmark bookmark)
    {
        BookmarkEntity entity = new()
        {
            Id = Guid.NewGuid(),
            PageNumber = bookmark.PageNumber,
            BookId = bookId
        };

        await context.Bookmarks.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBookmarkAsync(Guid userId, Guid bookmarkId)
    {
        BookmarkEntity? entity = await context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmarkId);
        if (entity != null)
        {
            context.Bookmarks.Update(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteBookmarkAsync(Guid userId, Guid bookmarkId)
    {
        BookmarkEntity? entity = await context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmarkId);
        if (entity != null)
        {
            context.Bookmarks.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
