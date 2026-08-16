using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shelfly.App.Data;
using Shelfly.Common.DTOs;
using Shelfly.App.Data.Entities;
using Shelfly.App.Data.Repositories;

namespace Shelfly.App.Services;

public class SyncService(
    IBookRepository bookRepository,
    IBookmarkRepository bookmarkRepository,
    ServerConnectionService serverConnectionService,
    ILogger<SyncService> logger,
    LocalDbContext localDbContext)
{
    public async Task<SyncResult> SyncBooksAsync()
    {
        SyncResult result = new();

        // Get all local books that are not soft-deleted
        List<LocalBook>? localBooks = await bookRepository.GetAllAsync();
        List<LocalBook>? activeBooks = localBooks?.Where(b => b.DeletedAt == null).ToList() ?? [];

        // Upload phase - send local changes to remote server
        foreach (LocalBook book in activeBooks)
        {
            try
            {
                SyncUploadRequest uploadRequest = new()
                {
                    EntityType = "Book",
                    Items = [new SyncItem
                    {
                        LocalGuid = book.LocalGuid,
                        Title = book.Title,
                        Author = book.Author,
                        Isbn = book.Isbn,
                        PublishDate = book.PublishDate,
                        LastModified = book.LastModified,
                        Bookmarks = book.LocalBookmarks?.Where(bm => bm.DeletedAt == null)
                            .Select(bm => new SyncBookmarkItem
                            {
                                LocalGuid = bm.LocalGuid,
                                StartPage = bm.StartPage,
                                EndPage = bm.EndPage,
                                Note = bm.Note,
                                LastModified = bm.LastModified
                            }).ToList() ?? []
                    }]
                };

                HttpClient client = new();
                HttpResponseMessage uploadResponseMessage = await client.PostAsJsonAsync($"{serverConnectionService.ServerUrl}api/sync/upload", uploadRequest);

                if (uploadResponseMessage.IsSuccessStatusCode)
                {
                    SyncUploadResponse? uploadResponse = await uploadResponseMessage.Content.ReadFromJsonAsync<SyncUploadResponse>();
                    if (uploadResponse != null)
                    {
                        result.Uploaded.AddRange(uploadResponse.Uploaded);
                        result.Conflicts.AddRange(uploadResponse.Conflicts);

                        // Update remote mappings for uploaded items
                        foreach (SyncUploadedItem item in uploadResponse.Uploaded)
                        {
                            RemoteMapping? existingMapping = await GetOrCreateRemoteMapping(book.LocalGuid, serverConnectionService.ServerUrl, item.RemoteGuid);
                            if (existingMapping != null)
                            {
                                existingMapping.LastSynced = DateTimeOffset.UtcNow;
                            }
                        }

                        result.SuccessCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading book {BookId}", book.LocalGuid);
                result.ErrorMessages.Add($"Upload failed for book '{book.Title}': {ex.Message}");
                result.FailureCount++;
            }
        }

        // Download phase - fetch remote changes
        try
        {
            List<Guid> localGuids = activeBooks.Select(b => b.LocalGuid).ToList();
            SyncDownloadRequest downloadRequest = new()
            {
                EntityType = "Book",
                LocalGuids = localGuids
            };

            HttpClient client = new();
            HttpResponseMessage downloadResponseMessage = await client.PostAsJsonAsync($"{serverConnectionService.ServerUrl}api/sync/download", downloadRequest);

            if (downloadResponseMessage.IsSuccessStatusCode)
            {
                SyncDownloadResponse? downloadResponse = await downloadResponseMessage.Content.ReadFromJsonAsync<SyncDownloadResponse>();
                if (downloadResponse != null)
                {
                    // Process downloaded items
                    foreach (SyncDownloadItem item in downloadResponse.Downloaded)
                    {
                        LocalBook? existingBook = await bookRepository.GetByIdAsync(item.LocalGuid ?? Guid.Empty);
                        if (existingBook == null && item.LocalGuid.HasValue)
                        {
                            // New remote book - create local entry
                            LocalBook newBook = new()
                            {
                                Title = item.Title,
                                Author = item.Author,
                                LastModified = item.LastModified,
                                DeletedAt = item.DeletedAt  // Map deletion timestamp from remote
                            };

                            await bookRepository.AddAsync(newBook);
                            result.DownloadedCount++;
                        }
                        else if (existingBook != null)
                        {
                            // Update existing local book with remote changes
                            if (item.LastModified > existingBook.LastModified)
                            {
                                existingBook.Title = item.Title;
                                existingBook.Author = item.Author;
                                existingBook.LastModified = item.LastModified;
                                existingBook.DeletedAt = item.DeletedAt;  // Sync deletion timestamp

                                await bookRepository.UpdateAsync(existingBook);
                                result.DownloadedCount++;
                            }
                        }
                    }

                    // Process deleted items
                    foreach (SyncDeletedItem deleted in downloadResponse.Deleted)
                    {
                        LocalBook? localBook = await bookRepository.GetByIdAsync(deleted.RemoteGuid);
                        if (localBook != null && localBook.DeletedAt == null)
                        {
                            localBook.DeletedAt = deleted.DeletedAt;  // Set deletion timestamp from remote
                            await bookRepository.UpdateAsync(localBook);
                            result.DeletedCount++;
                        }
                    }

                    serverConnectionService.UpdateLastSynced(DateTimeOffset.UtcNow);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading books");
            result.ErrorMessages.Add($"Download failed: {ex.Message}");
        }

        return result;
    }

    private async Task<RemoteMapping?> GetOrCreateRemoteMapping(Guid localBookGuid, string serverUrl, Guid remoteGuid)
    {
        // Check if mapping already exists using injected DbContext (constructor DI per Constitution V)
        RemoteMapping? existingMapping = await localDbContext.RemoteMappings
            .FirstOrDefaultAsync(m => m.LocalBookGuid == localBookGuid && m.ServerUrl == serverUrl && m.RemoteGuid == remoteGuid);

        if (existingMapping != null)
        {
            return existingMapping;
        }

        // Create new mapping
        RemoteMapping newMapping = new()
        {
            LocalBookGuid = localBookGuid,
            ServerUrl = serverUrl,
            RemoteGuid = remoteGuid,
            LastSynced = DateTimeOffset.UtcNow
        };

        localDbContext.RemoteMappings.Add(newMapping);
        await localDbContext.SaveChangesAsync();

        return newMapping;
    }
}

public class SyncResult
{
    public List<SyncUploadedItem> Uploaded { get; set; } = [];
    public List<SyncConflictItem> Conflicts { get; set; } = [];
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int DownloadedCount { get; set; }
    public int DeletedCount { get; set; }
    public List<string> ErrorMessages { get; set; } = [];
}
