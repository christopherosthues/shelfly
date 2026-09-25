namespace Shelfly.Api.Features.Bookmarks.DTOs;

public record BookmarkResponseDto(
    Guid Id,
    Guid BookId,
    int StartPage,
    int? EndPage,
    string? Note,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);
