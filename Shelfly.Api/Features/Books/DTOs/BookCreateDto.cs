namespace Shelfly.Api.Features.Books.DTOs;

public record BookCreateDto(
    string Title,
    string Author,
    string ISBN,
    string Publisher,
    DateTime? PublishDate);
