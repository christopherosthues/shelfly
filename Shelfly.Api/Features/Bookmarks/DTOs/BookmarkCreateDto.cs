namespace Shelfly.Api.Features.Bookmarks.DTOs;

public record BookmarkCreateDto(
    Guid BookId,
    int StartPage,
    int? EndPage,
    string? Note);
