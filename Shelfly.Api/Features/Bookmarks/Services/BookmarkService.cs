using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Api.Features.Bookmarks.DTOs;

namespace Shelfly.Api.Features.Bookmarks.Services;

public class BookmarkService(ShelflyDbContext dbContext) : IBookmarkService
{
    public async Task<PagedBookmarkListDto> GetBookmarksAsync(Guid userId, Guid? bookId, int page, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<BookmarkEntity> query = dbContext.Bookmarks
            .Where(b => b.UserId == userId);

        if (bookId.HasValue)
        {
            query = query.Where(b => b.BookId == bookId.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        IEnumerable<BookmarkEntity> bookmarks = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedBookmarkListDto(
            bookmarks.Select(MapToResponse),
            totalCount);
    }

    public async Task<BookmarkResponseDto?> GetBookmarkByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        return bookmark is not null ? MapToResponse(bookmark) : null;
    }

    public async Task<BookmarkResponseDto> CreateBookmarkAsync(Guid userId, BookmarkCreateDto dto, CancellationToken cancellationToken)
    {
        bool bookExists = await dbContext.Books
            .AnyAsync(b => b.Id == dto.BookId && b.UserId == userId, cancellationToken);

        if (!bookExists)
        {
            throw new InvalidOperationException($"Book '{dto.BookId}' not found for user.");
        }

        BookmarkEntity bookmark = new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            BookId = dto.BookId,
            StartPage = dto.StartPage,
            EndPage = dto.EndPage,
            Note = dto.Note
        };

        dbContext.Bookmarks.Add(bookmark);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(bookmark);
    }

    public async Task<BookmarkResponseDto?> UpdateBookmarkAsync(Guid userId, Guid id, BookmarkUpdateDto dto, CancellationToken cancellationToken)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        if (bookmark is null)
        {
            return null;
        }

        if (dto.BookId.HasValue)
        {
            bookmark.BookId = dto.BookId.Value;
        }

        if (dto.StartPage.HasValue)
        {
            bookmark.StartPage = dto.StartPage.Value;
        }

        bookmark.EndPage = dto.EndPage;
        bookmark.Note = dto.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(bookmark);
    }

    public async Task<BookmarkResponseDto?> PatchBookmarkAsync(Guid userId, Guid id, BookmarkPatchDto dto, CancellationToken cancellationToken)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        if (bookmark is null)
        {
            return null;
        }

        if (dto.BookId.HasValue)
        {
            bookmark.BookId = dto.BookId.Value;
        }

        if (dto.StartPage.HasValue)
        {
            bookmark.StartPage = dto.StartPage.Value;
        }

        if (dto.EndPage.HasValue)
        {
            bookmark.EndPage = dto.EndPage.Value;
        }

        if (dto.Note is not null)
        {
            bookmark.Note = dto.Note;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(bookmark);
    }

    public async Task<bool> DeleteBookmarkAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        BookmarkEntity? bookmark = await dbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

        if (bookmark is null)
        {
            return false;
        }

        dbContext.Bookmarks.Remove(bookmark);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static BookmarkResponseDto MapToResponse(BookmarkEntity bookmark) => new(
        bookmark.Id,
        bookmark.BookId,
        bookmark.StartPage,
        bookmark.EndPage,
        bookmark.Note,
        bookmark.CreatedAt,
        bookmark.LastModifiedAt);
}
