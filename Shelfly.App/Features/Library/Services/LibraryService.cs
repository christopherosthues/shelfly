using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Enums;
using Shelfly.Common;

namespace Shelfly.App.Features.Library.Services;

public class LibraryService(LocalDbContext dbContext)
{
    public async Task<List<BookEntity>> GetAllBooksAsync(CancellationToken cancellationToken = default)
    {
        return
        [
            .. (await dbContext.Books
                .Select(e => new
                {
                    Book = e,
                    BookmarkCount = e.Bookmarks.Count
                })
                .ToListAsync(cancellationToken))
            .Select(book =>
            {
                book.Book.BookmarkCount = book.BookmarkCount;
                return book.Book;
            })
        ];
    }

    public async Task<List<BookEntity>> SearchSortedBooksAsync(string query, SortCriterion criterion, SortDirection direction, CancellationToken cancellationToken = default)
    {
        IQueryable<BookEntity> baseQuery = dbContext.Books
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            string lowerQuery = query.ToLowerInvariant();
            baseQuery = baseQuery.Where(book =>
                EF.Functions.Like(book.Title, $"%{lowerQuery}%") ||
                EF.Functions.Like(book.Author, $"%{lowerQuery}%") ||
                EF.Functions.Like(book.Publisher, $"%{lowerQuery}%") ||
                EF.Functions.Like(book.ISBN, $"%{lowerQuery}%"));
        }

        IQueryable<BookEntity> sortedQuery = criterion switch
        {
            SortCriterion.Title => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.Title) : baseQuery.OrderByDescending(b => b.Title),
            SortCriterion.Author => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.Author) : baseQuery.OrderByDescending(b => b.Author),
            SortCriterion.Publisher => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.Publisher) : baseQuery.OrderByDescending(b => b.Publisher),
            SortCriterion.PublishDate => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.PublishDate ?? DateTime.MinValue)
                : baseQuery.OrderByDescending(b => b.PublishDate ?? DateTime.MaxValue),
            SortCriterion.CreatedAt => direction == SortDirection.Ascending ? baseQuery.OrderBy(b => b.CreatedAt) : baseQuery.OrderByDescending(b => b.CreatedAt),
            SortCriterion.LastModifiedAt => direction == SortDirection.Ascending
                ? baseQuery.OrderBy(b => b.LastModifiedAt ?? DateTime.MinValue)
                : baseQuery.OrderByDescending(b => b.LastModifiedAt ?? DateTime.MaxValue),
            _ => baseQuery
        };

        return
        [
            .. (await sortedQuery.Select(e => new
            {
                Book = e,
                BookmarkCount = e.Bookmarks.Count
            }).ToListAsync(cancellationToken))
            .Select(book =>
            {
                book.Book.BookmarkCount = book.BookmarkCount;
                return book.Book;
            })
        ];
    }

    public async Task<BookEntity?> SoftDeleteBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is not null)
        {
            book.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return book;
    }

    public async Task<Result<BookEntity>> AddBookAsync(string title, string author, string isbn, string publisher, DateTime? publishDate, CancellationToken cancellationToken = default)
    {
        BookEntity? existingBook = await dbContext.Books.IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.ISBN == isbn, cancellationToken);

        if (existingBook is not null)
        {
            return Result<BookEntity>.Failure("ISBN already exists");
        }

        BookEntity book = new()
        {
            Id = IdGenerator.NewId(),
            Title = title,
            Author = author,
            ISBN = isbn,
            Publisher = publisher,
            PublishDate = publishDate,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BookEntity>.Success(book);
    }

    public async Task<Result<BookEntity>> UpdateBookAsync(Guid bookId, string title, string author, string isbn, string publisher, DateTime? publishDate, CancellationToken cancellationToken = default)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            return Result<BookEntity>.Failure("Book not found");
        }

        BookEntity? existingBook = await dbContext.Books.IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.ISBN == isbn && b.Id != bookId, cancellationToken);

        if (existingBook is not null)
        {
            return Result<BookEntity>.Failure("ISBN already exists");
        }

        book.Title = title;
        book.Author = author;
        book.ISBN = isbn;
        book.Publisher = publisher;
        book.PublishDate = publishDate;
        book.LastModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BookEntity>.Success(book);
    }

    public async Task<BookEntity?> GetBookByIdAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
    }

    public async Task<Result<BookmarkEntity>> AddBookmarkAsync(Guid bookId, int startPage, int? endPage, string? note, CancellationToken cancellationToken = default)
    {
        if (startPage <= 0)
        {
            return Result<BookmarkEntity>.Failure("Start page must be a positive number");
        }

        if (endPage.HasValue && endPage.Value < startPage)
        {
            return Result<BookmarkEntity>.Failure("End page must be greater than or equal to start page");
        }

        if (note is not null && note.Length > 1000)
        {
            return Result<BookmarkEntity>.Failure("Note exceeds maximum length of 1000 characters");
        }

        BookmarkEntity bookmark = new()
        {
            Id = IdGenerator.NewId(),
            BookId = bookId,
            StartPage = startPage,
            EndPage = endPage,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Bookmarks.Add(bookmark);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BookmarkEntity>.Success(bookmark);
    }

    public async Task<Result<BookmarkEntity>> UpdateBookmarkAsync(Guid bookmarkId, int startPage, int? endPage, string? note, CancellationToken cancellationToken = default)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == bookmarkId, cancellationToken);

        if (bookmark is null)
        {
            return Result<BookmarkEntity>.Failure("Bookmark not found");
        }

        if (startPage <= 0)
        {
            return Result<BookmarkEntity>.Failure("Start page must be a positive number");
        }

        if (endPage.HasValue && endPage.Value < startPage)
        {
            return Result<BookmarkEntity>.Failure("End page must be greater than or equal to start page");
        }

        if (note is not null && note.Length > 1000)
        {
            return Result<BookmarkEntity>.Failure("Note exceeds maximum length of 1000 characters");
        }

        bookmark.StartPage = startPage;
        bookmark.EndPage = endPage;
        bookmark.Note = note;
        bookmark.LastModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BookmarkEntity>.Success(bookmark);
    }

    public async Task<Result<BookmarkEntity?>> DeleteBookmarkAsync(Guid bookmarkId, CancellationToken cancellationToken = default)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == bookmarkId, cancellationToken);

        if (bookmark is null)
        {
            return Result<BookmarkEntity?>.Failure("Bookmark not found");
        }

        dbContext.Bookmarks.Remove(bookmark);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BookmarkEntity?>.Success(bookmark);
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

    public async Task<Result<bool>> SoftDeleteBookWithBookmarksAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            return Result<bool>.Failure("Book not found");
        }

        book.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}