namespace Shelfly.Api.Features.Books.DTOs;

public record BookResponseDto(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    string Publisher,
    DateTime? PublishDate,
    DateTime CreatedAt,
    DateTime? LastModifiedAt,
    DateTime? DeletedAt);
