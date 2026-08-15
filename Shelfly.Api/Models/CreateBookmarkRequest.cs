namespace Shelfly.Api.Models;

public record CreateBookmarkRequest(
    int StartPage,
    int? EndPage,
    string? Note);
