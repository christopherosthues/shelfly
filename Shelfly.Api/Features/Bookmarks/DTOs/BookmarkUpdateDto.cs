namespace Shelfly.Api.Features.Bookmarks.DTOs;

public record BookmarkUpdateDto(
    Guid? BookId,
    int? StartPage,
    int? EndPage,
    string? Note);
