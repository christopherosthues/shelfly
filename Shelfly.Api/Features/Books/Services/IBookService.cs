using Shelfly.Api.Features.Books.DTOs;

namespace Shelfly.Api.Features.Books.Services;

public interface IBookService
{
    Task<PagedBookListDto> GetBooksAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<BookResponseDto?> GetBookByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<BookResponseDto> CreateBookAsync(Guid userId, BookCreateDto dto, CancellationToken cancellationToken);
    Task<BookResponseDto?> UpdateBookAsync(Guid userId, Guid id, BookUpdateDto dto, CancellationToken cancellationToken);
    Task<BookResponseDto?> PatchBookAsync(Guid userId, Guid id, BookPatchDto dto, CancellationToken cancellationToken);
    Task<bool> SoftDeleteBookAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
