using Shelfly.Common;

namespace Shelfly.Api.Bookmarks;

public class BookmarkService
{
    public async Task<List<Bookmark>> GetBookmarks(Guid userId, Guid bookId)
    {
        return [];
    }

    public async Task<Bookmark?> GetBookmark(Guid userId, Guid bookmarkId)
    {
        Bogus.Faker<Bookmark> bookmarkFaker = new Bogus.Faker<Bookmark>();
        Bookmark bookmark = bookmarkFaker.Generate();
        return bookmark;
    }

    public async Task AddBookmark(Guid userId, Guid bookId, Bookmark bookmark)
    {
        await Task.CompletedTask;
    }

    public async Task UpdateBookmark(Guid userId, Guid bookmarkId)
    {
        await Task.CompletedTask;
    }

    public async Task DeleteBookmark(Guid userId, Guid bookmarkId)
    {
        await Task.CompletedTask;
    }
}