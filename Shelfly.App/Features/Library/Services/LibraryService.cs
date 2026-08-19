using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Features.Library.Services;

public class LibraryService(LocalDbContext dbContext)
{
    public async Task<List<BookEntity>> GetAllBooksAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .Where(book => book.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BookEntity>> SearchBooksAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllBooksAsync(cancellationToken);
        }

        string lowerQuery = query.ToLowerInvariant();

        return await dbContext.Books
            .Where(book =>
                book.DeletedAt == null &&
                (EF.Functions.Like(book.Title, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.Author, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.Publisher, $"%{lowerQuery}%") ||
                 EF.Functions.Like(book.ISBN, $"%{lowerQuery}%")))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BookEntity>> SortBooksAsync(SortCriterion criterion, CancellationToken cancellationToken = default)
    {
        List<BookEntity> books = await GetAllBooksAsync(cancellationToken);

        return criterion switch
        {
            SortCriterion.Title => [.. books.OrderBy(b => b.Title)],
            SortCriterion.Author => [.. books.OrderBy(b => b.Author)],
            SortCriterion.Publisher => [.. books.OrderBy(b => b.Publisher)],
            SortCriterion.PublishDate => [.. books.OrderBy(b => b.PublishDate ?? DateTime.MinValue)],
            _ => books
        };
    }

    public async Task<BookEntity?> SoftDeleteBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == bookId && b.DeletedAt == null, cancellationToken);

        if (book is not null)
        {
            book.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return book;
    }
}

public enum SortCriterion
{
    Title,
    Author,
    Publisher,
    PublishDate
}
