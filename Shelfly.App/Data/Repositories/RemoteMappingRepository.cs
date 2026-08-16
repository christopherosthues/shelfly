using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data.Repositories;

public class RemoteMappingRepository(LocalDbContext dbContext) : IRemoteMappingRepository
{
    public async Task<List<RemoteMapping>> GetAllAsync()
    {
        return await dbContext.RemoteMappings
            .Include(rm => rm.LocalBook)
            .ToListAsync();
    }

    public async Task<List<RemoteMapping>> GetByBookIdAsync(Guid bookGuid)
    {
        return await dbContext.RemoteMappings
            .Where(rm => rm.LocalBookGuid == bookGuid)
            .ToListAsync();
    }

    public async Task<RemoteMapping?> GetByIdAsync(int id)
    {
        return await dbContext.RemoteMappings
            .Include(rm => rm.LocalBook)
            .FirstOrDefaultAsync(rm => rm.Id == id);
    }

    public async Task<RemoteMapping?> GetByServerAndRemoteGuidAsync(string serverUrl, Guid remoteGuid)
    {
        return await dbContext.RemoteMappings
            .Where(rm => rm.ServerUrl == serverUrl && rm.RemoteGuid == remoteGuid)
            .FirstOrDefaultAsync();
    }

    public async Task<RemoteMapping> AddAsync(RemoteMapping mapping)
    {
        dbContext.RemoteMappings.Add(mapping);
        await dbContext.SaveChangesAsync();
        return mapping;
    }

    public async Task UpdateAsync(RemoteMapping mapping)
    {
        dbContext.RemoteMappings.Update(mapping);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        RemoteMapping? mapping = await GetByIdAsync(id);
        if (mapping != null)
        {
            dbContext.RemoteMappings.Remove(mapping);
            await dbContext.SaveChangesAsync();
        }
    }
}
