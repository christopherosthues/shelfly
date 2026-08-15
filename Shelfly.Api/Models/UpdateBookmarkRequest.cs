namespace Shelfly.Api.Models;

public record UpdateBookmarkRequest(
    int StartPage,
    int? EndPage,
    string? Note);
