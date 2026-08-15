using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Bookmarks;

public class BookmarkService(ShelflyDbContext context)
{
    public async Task<List<Bookmark>> GetBookmarksAsync(Guid userId, Guid bookId)
    {
        return await context.Bookmarks
            .Where(b => b.BookId == bookId && b.UserId == userId)
            .Select(b => new Bookmark
            {
                Id = b.Id,
                StartPage = b.StartPage,
                EndPage = b.EndPage,
                Note = b.Note
            })
            .ToListAsync();
    }

    public async Task<Bookmark?> GetBookmarkAsync(Guid userId, Guid bookmarkId)
    {
        BookmarkEntity? entity = await context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmarkId && b.UserId == userId);
        return entity is null ? null : new Bookmark
        {
            Id = entity.Id,
            StartPage = entity.StartPage,
            EndPage = entity.EndPage,
            Note = entity.Note
        };
    }

    public async Task AddBookmarkAsync(Guid userId, Guid bookId, Bookmark bookmark)
    {
        BookmarkEntity entity = new()
        {
            Id = Guid.NewGuid(),
            StartPage = bookmark.StartPage,
            EndPage = bookmark.EndPage,
            Note = bookmark.Note,
            UserId = userId,
            BookId = bookId
        };

        await context.Bookmarks.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBookmarkAsync(Guid userId, Guid bookmarkId, Bookmark bookmark)
    {
        BookmarkEntity? entity = await context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmarkId && b.UserId == userId);
        if (entity != null)
        {
            entity.StartPage = bookmark.StartPage;
            entity.EndPage = bookmark.EndPage;
            entity.Note = bookmark.Note;

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteBookmarkAsync(Guid userId, Guid bookmarkId)
    {
        BookmarkEntity? entity = await context.Bookmarks.FirstOrDefaultAsync(b => b.Id == bookmarkId && b.UserId == userId);
        if (entity != null)
        {
            context.Bookmarks.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
