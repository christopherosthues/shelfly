using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;

namespace Shelfly.App.Features.Trash.Services;

public class TrashService(LocalDbContext dbContext)
{
    public async Task<List<BookEntity>> GetAllTrashBooksAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .IgnoreQueryFilters()
            .Where(book => book.DeletedAt != null)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BookEntity>> GetSortedTrashBooksAsync(SortCriterion criterion, SortDirection direction, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Books
            .IgnoreQueryFilters()
            .Where(book => book.DeletedAt != null)
            .AsQueryable();

        query = criterion switch
        {
            SortCriterion.Title => direction == SortDirection.Ascending ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title),
            SortCriterion.Author => direction == SortDirection.Ascending ? query.OrderBy(b => b.Author) : query.OrderByDescending(b => b.Author),
            SortCriterion.Publisher => direction == SortDirection.Ascending ? query.OrderBy(b => b.Publisher) : query.OrderByDescending(b => b.Publisher),
            SortCriterion.PublishDate => direction == SortDirection.Ascending ? query.OrderBy(b => b.PublishDate ?? DateTime.MinValue) : query.OrderByDescending(b => b.PublishDate ?? DateTime.MaxValue),
            _ => query
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<BookEntity>> SearchTrashBooksAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllTrashBooksAsync(cancellationToken);
        }

        string lowerQuery = query.ToLowerInvariant();

        return await dbContext.Books
            .IgnoreQueryFilters()
            .Where(book =>
                book.DeletedAt != null &&
                (EF.Functions.Like(book.Title, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.Author, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.Publisher, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.ISBN, $"%{lowerQuery}%")))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BookEntity>> SearchSortedTrashBooksAsync(string query, SortCriterion criterion, SortDirection direction, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.Books
            .IgnoreQueryFilters()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            string lowerQuery = query.ToLowerInvariant();
            baseQuery = baseQuery.Where(book =>
                book.DeletedAt != null &&
                (EF.Functions.Like(book.Title, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.Author, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.Publisher, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.ISBN, $"%{lowerQuery}%")));
        }
        else
        {
            baseQuery = baseQuery.Where(book => book.DeletedAt != null);
        }

        var sortedQuery = criterion switch
        {
            SortCriterion.Title => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.Title) : baseQuery.OrderByDescending(b => b.Title),
            SortCriterion.Author => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.Author) : baseQuery.OrderByDescending(b => b.Author),
            SortCriterion.Publisher => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.Publisher) : baseQuery.OrderByDescending(b => b.Publisher),
            SortCriterion.PublishDate => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.PublishDate ?? DateTime.MinValue) : baseQuery.OrderByDescending(b => b.PublishDate ?? DateTime.MaxValue),
            _ => baseQuery
        };

        return await sortedQuery.ToListAsync(cancellationToken);
    }

    public async Task<BookEntity?> RestoreBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        BookEntity? book = await dbContext.Books
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == bookId && b.DeletedAt != null, cancellationToken);

        if (book is not null)
        {
            book.DeletedAt = null;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return book;
    }

    public async Task<BookEntity?> HardDeleteBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        BookEntity? book = await dbContext.Books
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == bookId && b.DeletedAt != null, cancellationToken);

        if (book is not null)
        {
            // Delete associated bookmarks first
            List<BookmarkEntity> bookmarks = await dbContext.Bookmarks
                .Where(b => b.BookId == bookId)
                .ToListAsync(cancellationToken);

            dbContext.Bookmarks.RemoveRange(bookmarks);
            dbContext.Books.Remove(book);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return book;
    }

    public async Task<int> RestoreAllAsync(CancellationToken cancellationToken = default)
    {
        List<BookEntity> trashBooks = await GetAllTrashBooksAsync(cancellationToken);

        foreach (BookEntity book in trashBooks)
        {
            book.DeletedAt = null;
        }

        int count = await dbContext.SaveChangesAsync(cancellationToken);

        return trashBooks.Count;
    }

    public async Task<int> DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        List<BookEntity> trashBooks = await GetAllTrashBooksAsync(cancellationToken);

        // Collect all bookmark IDs associated with trash books
        HashSet<Guid> bookmarkIds = new();

        foreach (BookEntity book in trashBooks)
        {
            List<BookmarkEntity> bookmarks = await dbContext.Bookmarks
                .Where(b => b.BookId == book.Id)
                .ToListAsync(cancellationToken);

            foreach (BookmarkEntity bookmark in bookmarks)
            {
                bookmarkIds.Add(bookmark.Id);
            }
        }

        // Remove all bookmarks associated with trash books
        List<BookmarkEntity> bookmarksToRemove = await dbContext.Bookmarks
            .Where(b => bookmarkIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        dbContext.Bookmarks.RemoveRange(bookmarksToRemove);
        dbContext.Books.RemoveRange(trashBooks);

        int count = await dbContext.SaveChangesAsync(cancellationToken);

        return trashBooks.Count;
    }

    public async Task<List<BookmarkEntity>> GetBookmarksByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        List<BookmarkEntity> bookmarks = await dbContext.Bookmarks
            .Where(b => b.BookId == bookId)
            .ToListAsync(cancellationToken);

        return [.. bookmarks.OrderBy(b => b.StartPage).ThenBy(b => b.EndPage ?? int.MaxValue)];
    }

    public async Task<BookmarkEntity?> GetBookmarkByIdAsync(Guid bookmarkId, CancellationToken cancellationToken = default)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == bookmarkId, cancellationToken);

        return bookmark;
    }
}
