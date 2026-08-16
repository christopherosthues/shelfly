using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data.Repositories;

public interface IBookmarkRepository
{
    Task<List<LocalBookmark>> GetAllAsync();
    Task<List<LocalBookmark>> GetByBookIdAsync(Guid bookId);
    Task<LocalBookmark?> GetByIdAsync(Guid id);
    Task<LocalBookmark> AddAsync(LocalBookmark bookmark);
    Task UpdateAsync(LocalBookmark bookmark);
    Task DeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
    Task<List<LocalBookmark>> GetTrashItemsAsync();
    Task CleanupExpiredAsync(DateTimeOffset cutoffTime);
}
