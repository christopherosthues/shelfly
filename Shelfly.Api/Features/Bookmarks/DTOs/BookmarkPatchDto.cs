namespace Shelfly.Api.Features.Bookmarks.DTOs;

public record BookmarkPatchDto(
    Guid? BookId,
    int? StartPage,
    int? EndPage,
    string? Note);
