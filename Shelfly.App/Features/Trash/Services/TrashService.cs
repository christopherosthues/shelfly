using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Enums;

namespace Shelfly.App.Features.Trash.Services;

public class TrashService(LocalDbContext dbContext)
{
    public async Task<List<BookEntity>> GetAllTrashBooksAsync(CancellationToken cancellationToken = default)
    {
        return
        [
            .. (await dbContext.Books
                .IgnoreQueryFilters()
                .Where(book => book.DeletedAt != null)
                .Select(book => new
                {
                    Book = book,
                    BookmarkCount = book.Bookmarks.Count
                })
                .ToListAsync(cancellationToken))
            .Select(book =>
            {
                book.Book.BookmarkCount = book.BookmarkCount;
                return book.Book;
            })
        ];
    }

    public async Task<List<BookEntity>> SearchSortedTrashBooksAsync(string query, SortCriterion criterion,
        SortDirection direction, CancellationToken cancellationToken = default)
    {
        IQueryable<BookEntity> baseQuery = dbContext.Books
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

        IQueryable<BookEntity> sortedQuery = criterion switch
        {
            SortCriterion.Title => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.Title)
                : baseQuery.OrderByDescending(b => b.Title),
            SortCriterion.Author => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.Author)
                : baseQuery.OrderByDescending(b => b.Author),
            SortCriterion.Publisher => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.Publisher)
                : baseQuery.OrderByDescending(b => b.Publisher),
            SortCriterion.PublishDate => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.PublishDate ?? DateTime.MinValue)
                : baseQuery.OrderByDescending(b => b.PublishDate ?? DateTime.MaxValue),
            SortCriterion.CreatedAt => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.CreatedAt)
                : baseQuery.OrderByDescending(b => b.CreatedAt),
            SortCriterion.LastModifiedAt => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.LastModifiedAt ?? DateTime.MinValue)
                : baseQuery.OrderByDescending(b => b.LastModifiedAt ?? DateTime.MaxValue),
            _ => baseQuery
        };

        return
        [
            .. (await sortedQuery.Select(book => new
            {
                Book = book,
                BookmarkCount = book.Bookmarks.Count
            }).ToListAsync(cancellationToken))
            .Select(book =>
            {
                book.Book.BookmarkCount = book.BookmarkCount;
                return book.Book;
            })
        ];
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

    public async Task<List<BookmarkEntity>> GetBookmarksByBookIdAsync(Guid bookId,
        CancellationToken cancellationToken = default)
    {
        List<BookmarkEntity> bookmarks = await dbContext.Bookmarks
            .Where(b => b.BookId == bookId)
            .ToListAsync(cancellationToken);

        return [.. bookmarks.OrderBy(b => b.StartPage).ThenBy(b => b.EndPage ?? int.MaxValue)];
    }
}