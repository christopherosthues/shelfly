namespace Shelfly.Api.Features.Bookmarks.DTOs;

public record PagedBookmarkListDto(
    IEnumerable<BookmarkResponseDto> Items,
    int TotalCount);
