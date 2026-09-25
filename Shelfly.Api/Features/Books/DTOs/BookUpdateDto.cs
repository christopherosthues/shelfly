namespace Shelfly.Api.Features.Books.DTOs;

public record BookUpdateDto(
    string? Title,
    string? Author,
    string? ISBN,
    string? Publisher,
    DateTime? PublishDate);
