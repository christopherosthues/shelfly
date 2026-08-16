using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data.Repositories;

public interface IBookRepository
{
    Task<List<LocalBook>> GetAllAsync();
    Task<LocalBook?> GetByIdAsync(Guid id);
    Task<LocalBook> AddAsync(LocalBook book);
    Task UpdateAsync(LocalBook book);
    Task DeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
    Task<List<LocalBook>> GetTrashItemsAsync();
    Task CleanupExpiredAsync(DateTimeOffset cutoffTime);
}
