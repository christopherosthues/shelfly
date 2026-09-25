namespace Shelfly.Api.Features.Books.DTOs;

public record PagedBookListDto(
    IEnumerable<BookResponseDto> Items,
    int TotalCount);
