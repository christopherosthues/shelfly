namespace Shelfly.Api.Features.Books.DTOs;

public record BookPatchDto(
    string? Title,
    string? Author,
    string? ISBN,
    string? Publisher,
    DateTime? PublishDate);
