using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Api.Features.Books.DTOs;

namespace Shelfly.Api.Features.Books.Services;

public class BookService(ShelflyDbContext dbContext) : IBookService
{
    public async Task<PagedBookListDto> GetBooksAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<BookEntity> query = dbContext.Books
            .Where(b => b.UserId == userId);

        int totalCount = await query.CountAsync(cancellationToken);

        IEnumerable<BookEntity> books = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedBookListDto(
            books.Select(MapToResponse),
            totalCount);
    }

    public async Task<BookResponseDto?> GetBookByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        BookEntity? book = await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        return book is not null ? MapToResponse(book) : null;
    }

    public async Task<BookResponseDto> CreateBookAsync(Guid userId, BookCreateDto dto, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.Books
            .AnyAsync(b => b.UserId == userId && b.ISBN == dto.ISBN, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"ISBN '{dto.ISBN}' already exists for user.");
        }

        BookEntity book = new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Publisher = dto.Publisher,
            PublishDate = dto.PublishDate
        };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(book);
    }

    public async Task<BookResponseDto?> UpdateBookAsync(Guid userId, Guid id, BookUpdateDto dto, CancellationToken cancellationToken)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        if (book is null)
        {
            return null;
        }

        if (dto.Title is not null)
        {
            book.Title = dto.Title;
        }

        if (dto.Author is not null)
        {
            book.Author = dto.Author;
        }

        if (dto.ISBN is not null)
        {
            book.ISBN = dto.ISBN;
        }

        if (dto.Publisher is not null)
        {
            book.Publisher = dto.Publisher;
        }

        if (dto.PublishDate.HasValue)
        {
            book.PublishDate = dto.PublishDate.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(book);
    }

    public async Task<BookResponseDto?> PatchBookAsync(Guid userId, Guid id, BookPatchDto dto, CancellationToken cancellationToken)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        if (book is null)
        {
            return null;
        }

        if (dto.Title is not null)
        {
            book.Title = dto.Title;
        }

        if (dto.Author is not null)
        {
            book.Author = dto.Author;
        }

        if (dto.ISBN is not null)
        {
            book.ISBN = dto.ISBN;
        }

        if (dto.Publisher is not null)
        {
            book.Publisher = dto.Publisher;
        }

        if (dto.PublishDate.HasValue)
        {
            book.PublishDate = dto.PublishDate.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(book);
    }

    public async Task<bool> SoftDeleteBookAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        BookEntity? book = await dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        if (book is null)
        {
            return false;
        }

        book.DeletedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static BookResponseDto MapToResponse(BookEntity book) => new(
        book.Id,
        book.Title,
        book.Author,
        book.ISBN,
        book.Publisher,
        book.PublishDate,
        book.CreatedAt,
        book.LastModifiedAt,
        book.DeletedAt);
}
