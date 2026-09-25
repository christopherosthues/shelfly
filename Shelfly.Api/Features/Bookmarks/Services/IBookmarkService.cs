using Shelfly.Api.Features.Bookmarks.DTOs;

namespace Shelfly.Api.Features.Bookmarks.Services;

public interface IBookmarkService
{
    Task<PagedBookmarkListDto> GetBookmarksAsync(Guid userId, Guid? bookId, int page, int pageSize, CancellationToken cancellationToken);
    Task<BookmarkResponseDto?> GetBookmarkByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<BookmarkResponseDto> CreateBookmarkAsync(Guid userId, BookmarkCreateDto dto, CancellationToken cancellationToken);
    Task<BookmarkResponseDto?> UpdateBookmarkAsync(Guid userId, Guid id, BookmarkUpdateDto dto, CancellationToken cancellationToken);
    Task<BookmarkResponseDto?> PatchBookmarkAsync(Guid userId, Guid id, BookmarkPatchDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteBookmarkAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
