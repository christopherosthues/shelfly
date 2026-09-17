using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Features.Settings.Services;

public class SettingsService(LocalDbContext dbContext, CredentialStore credentialStore, ApiClient apiClient)
{
    public async Task<ApiResult<bool>> TestConnectionAsync(string url)
    {
        return await apiClient.TestConnectionAsync(url);
    }

    public async Task<List<SavedServerEntry>> GetSavedServersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SavedServerEntries
            .Include(e => e.Server)
            .ToListAsync(cancellationToken);
    }

    public async Task<SyncState> GetSyncStateAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SyncStates.FirstAsync(cancellationToken);
    }

    public async Task<SavedServerEntry?> GetSavedServerEntryByIdAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SavedServerEntries
            .Include(e => e.Server)
            .FirstOrDefaultAsync(e => e.Id == entryId, cancellationToken);
    }

    public async Task<ApiResult<SavedServerEntry>> RegisterAndSaveServerAsync(string url, string username, string email, string password, CancellationToken cancellationToken = default)
    {
        // Call API to register the user
        ApiResult<string> loginResult = await apiClient.RegisterAsync(url, username, email, password, cancellationToken);

        if (!loginResult.IsSuccess)
        {
            return ApiResult<SavedServerEntry>.Failure(loginResult.ErrorMessage ?? "Registration failed");
        }

        // Create or find server entity
        ServerEntity? server = await dbContext.Servers
            .FirstOrDefaultAsync(s => s.Url == url, cancellationToken);

        if (server is null)
        {
            server = new ServerEntity
            {
                Id = Guid.CreateVersion7(),
                Url = url,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Servers.Add(server);
        }

        // Create saved server entry
        SavedServerEntry entry = new()
        {
            Id = Guid.CreateVersion7(),
            ServerId = server.Id,
            ProfileUsername = username,
            ProfileEmail = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.SavedServerEntries.Add(entry);

        // Update SyncState
        SyncState syncState = await GetSyncStateAsync(cancellationToken);
        syncState.ActiveServerEntryId = entry.Id;
        syncState.SyncEnabled = true;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<SavedServerEntry>.Success(entry);
    }

    public async Task<ApiResult<bool>> SignInExistingServerAsync(Guid serverEntryId, string username, string password, CancellationToken cancellationToken = default)
    {
        // Find the saved server entry
        SavedServerEntry? entry = await GetSavedServerEntryByIdAsync(serverEntryId, cancellationToken);

        if (entry is null)
        {
            return ApiResult<bool>.Failure("Server entry not found");
        }

        // Call API to login the user
        ApiResult<string> loginResult = await apiClient.LoginAsync(entry.Server?.Url ?? string.Empty, username, password, cancellationToken);

        if (!loginResult.IsSuccess)
        {
            return ApiResult<bool>.Failure(loginResult.ErrorMessage ?? "Sign in failed");
        }

        // Update entry with profile data
        entry.ProfileUsername = username;

        // Deactivate all other entries
        List<SavedServerEntry> allEntries = await GetSavedServersAsync(cancellationToken);
        foreach (SavedServerEntry other in allEntries)
        {
            other.IsActive = other.Id == entry.Id;
        }

        // Update SyncState
        SyncState syncState = await GetSyncStateAsync(cancellationToken);
        syncState.ActiveServerEntryId = entry.Id;
        syncState.SyncEnabled = true;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Success(true);
    }

    public async Task SetActiveServerEntryAsync(Guid entryId, bool syncEnabled, CancellationToken cancellationToken = default)
    {
        // Deactivate all other entries
        List<SavedServerEntry> allEntries = await GetSavedServersAsync(cancellationToken);
        foreach (SavedServerEntry entry in allEntries)
        {
            entry.IsActive = entry.Id == entryId;
        }

        // Update SyncState
        SyncState syncState = await GetSyncStateAsync(cancellationToken);
        syncState.ActiveServerEntryId = entryId;
        syncState.SyncEnabled = syncEnabled;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearActiveServerEntryAsync(CancellationToken cancellationToken = default)
    {
        SyncState syncState = await GetSyncStateAsync(cancellationToken);
        syncState.ActiveServerEntryId = null;
        syncState.SyncEnabled = false;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSyncStateAsync(bool syncEnabled, Guid? activeServerEntryId, CancellationToken cancellationToken = default)
    {
        SyncState syncState = await GetSyncStateAsync(cancellationToken);
        syncState.SyncEnabled = syncEnabled;
        syncState.ActiveServerEntryId = activeServerEntryId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void SaveCredentials(Guid serverEntryId, string username, string email, string jwtToken)
    {
        credentialStore.SaveCredentials(serverEntryId, username, email, jwtToken);
    }

    public (string Username, string Email, string JwtToken)? LoadCredentials(Guid serverEntryId)
    {
        return credentialStore.LoadCredentials(serverEntryId);
    }

    public void ClearCredentials(Guid serverEntryId)
    {
        credentialStore.ClearCredentials(serverEntryId);
    }
}