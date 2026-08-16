using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data.Repositories;

public interface IRemoteMappingRepository
{
    Task<List<RemoteMapping>> GetAllAsync();
    Task<List<RemoteMapping>> GetByBookIdAsync(Guid bookGuid);
    Task<RemoteMapping?> GetByIdAsync(int id);
    Task<RemoteMapping?> GetByServerAndRemoteGuidAsync(string serverUrl, Guid remoteGuid);
    Task<RemoteMapping> AddAsync(RemoteMapping mapping);
    Task UpdateAsync(RemoteMapping mapping);
    Task DeleteAsync(int id);
}
