using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Messaging;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Messages;

namespace Shelfly.App.Features.Settings.Services;

public sealed class SyncService : IRecipient<LibraryChangedMessage>
{
    private readonly LocalDbContext _dbContext;
    private readonly ApiClient _apiClient;
    private DateTime? _lastSyncAttempt;
    private const int CooldownSeconds = 30;

    public SyncService(LocalDbContext dbContext, ApiClient apiClient)
    {
        _dbContext = dbContext;
        _apiClient = apiClient;
        WeakReferenceMessenger.Default.Register<LibraryChangedMessage>(this);
    }

    public void Receive(LibraryChangedMessage message)
    {
        _ = TrySyncAsync();
    }

    private async Task TrySyncAsync()
    {
        if (_lastSyncAttempt.HasValue && (DateTime.UtcNow - _lastSyncAttempt.Value).TotalSeconds < CooldownSeconds)
        {
            return;
        }

        await SyncAsync();
    }

    public async Task<ApiResult<string>> SyncAsync(CancellationToken cancellationToken = default)
    {
        SyncState syncState = await GetActiveSyncStateAsync();

        if (!syncState.SyncEnabled || !syncState.ActiveServerEntryId.HasValue)
        {
            return ApiResult<string>.Failure("Sync not enabled or no active server");
        }

        SavedServerEntry? activeEntry = await _dbContext.SavedServerEntries
            .FirstOrDefaultAsync(e => e.Id == syncState.ActiveServerEntryId.Value, cancellationToken);

        if (activeEntry is null)
        {
            return ApiResult<string>.Failure("Active server entry not found");
        }

        try
        {
            await UploadLocalChangesAsync(activeEntry, cancellationToken);
            await DownloadServerChangesAsync(activeEntry, cancellationToken);

            syncState.LastSuccessfulSyncAt = DateTime.UtcNow;
            syncState.LastSyncResult = "Sync completed successfully";
            await _dbContext.SaveChangesAsync(cancellationToken);
            _lastSyncAttempt = DateTime.UtcNow;

            return ApiResult<string>.Success("Synchronization completed");
        }
        catch (OperationCanceledException)
        {
            return ApiResult<string>.Failure("Sync cancelled");
        }
        catch (IOException ex)
        {
            syncState.LastSyncResult = $"Sync failed: {ex.Message}";
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ApiResult<string>.Failure($"Network error: {ex.Message}");
        }
    }

    private async Task UploadLocalChangesAsync(SavedServerEntry activeEntry, CancellationToken cancellationToken)
    {
        ServerEntity? server = await _dbContext.Servers
            .FirstOrDefaultAsync(s => s.Id == activeEntry.ServerId, cancellationToken);

        if (server is null)
        {
            return;
        }

        List<BookServerMapping> existingMappings = await _dbContext.BookServerMappings
            .Where(m => m.ServerId == server.Id)
            .ToListAsync(cancellationToken);

        HashSet<Guid> syncedBookIds = new(existingMappings.Select(m => m.BookId));

        List<BookEntity> localBooks = await _dbContext.Books
            .Include(b => b.Bookmarks)
            .ToListAsync(cancellationToken);

        foreach (BookEntity book in localBooks)
        {
            if (!syncedBookIds.Contains(book.Id))
            {
                ApiResult<string> uploadResult = await _apiClient.UploadBookAsync(book, cancellationToken);

                if (uploadResult.IsSuccess && !string.IsNullOrEmpty(uploadResult.Value))
                {
                    BookServerMapping mapping = new()
                    {
                        Id = Guid.CreateVersion7(),
                        BookId = book.Id,
                        ServerId = server.Id,
                        ServerAssignedId = uploadResult.Value!
                    };

                    _dbContext.BookServerMappings.Add(mapping);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    foreach (BookmarkEntity bookmark in book.Bookmarks)
                    {
                        ApiResult<string> bmResult = await _apiClient.UploadBookmarkAsync(bookmark, cancellationToken);
                    }

                    UpdateProfileSyncRecord(book.Id, activeEntry.ProfileUsername);
                }
            }
        }
    }

    private async Task DownloadServerChangesAsync(SavedServerEntry activeEntry, CancellationToken cancellationToken)
    {
        ApiResult<List<BookEntity>> fetchResult = await _apiClient.FetchServerBooksAsync(cancellationToken);

        if (!fetchResult.IsSuccess || fetchResult.Value is null)
        {
            return;
        }

        List<BookServerMapping> existingMappings = await _dbContext.BookServerMappings
            .Where(m => m.ServerId == activeEntry.ServerId)
            .ToListAsync(cancellationToken);

        Dictionary<string, BookServerMapping> mappingByServerId = existingMappings
            .ToDictionary(m => m.ServerAssignedId, m => m);

        foreach (BookEntity serverBook in fetchResult.Value)
        {
            if (!mappingByServerId.TryGetValue(serverBook.ISBN, out BookServerMapping? existingMapping))
            {
                BookEntity? localBook = await _dbContext.Books
                    .FirstOrDefaultAsync(b => b.ISBN == serverBook.ISBN, cancellationToken);

                if (localBook is null)
                {
                    localBook = new()
                    {
                        Id = Guid.CreateVersion7(),
                        Title = serverBook.Title,
                        Author = serverBook.Author,
                        ISBN = serverBook.ISBN,
                        Publisher = serverBook.Publisher,
                        PublishDate = serverBook.PublishDate,
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbContext.Books.Add(localBook);
                }
                else if (serverBook.LastModifiedAt > localBook.LastModifiedAt)
                {
                    localBook.Title = serverBook.Title;
                    localBook.Author = serverBook.Author;
                    localBook.Publisher = serverBook.Publisher;
                    localBook.PublishDate = serverBook.PublishDate;
                    localBook.LastModifiedAt = serverBook.LastModifiedAt;
                }

                ApiResult<List<BookmarkEntity>> bmResult = await _apiClient.FetchServerBookmarksAsync(serverBook.Id, cancellationToken);

                if (bmResult.IsSuccess && bmResult.Value is not null)
                {
                    foreach (BookmarkEntity serverBm in bmResult.Value)
                    {
                        BookmarkEntity? localBm = await _dbContext.Bookmarks
                            .FirstOrDefaultAsync(b => b.BookId == serverBook.Id && b.StartPage == serverBm.StartPage, cancellationToken);

                        if (localBm is null)
                        {
                            localBm = new()
                            {
                                Id = Guid.CreateVersion7(),
                                BookId = serverBook.Id,
                                StartPage = serverBm.StartPage,
                                EndPage = serverBm.EndPage,
                                Note = serverBm.Note,
                                CreatedAt = DateTime.UtcNow
                            };

                            _dbContext.Bookmarks.Add(localBm);
                        }
                    }
                }

                UpdateProfileSyncRecord(serverBook.Id, activeEntry.ProfileUsername);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private void UpdateProfileSyncRecord(Guid bookId, string profileUsername)
    {
        BookProfileSyncRecord? existing = _dbContext.BookProfileSyncRecords
            .FirstOrDefault(r => r.BookId == bookId && r.ProfileUsername == profileUsername);

        if (existing is null)
        {
            _dbContext.BookProfileSyncRecords.Add(new()
            {
                Id = Guid.CreateVersion7(),
                BookId = bookId,
                ProfileUsername = profileUsername,
                LastSyncAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.LastSyncAt = DateTime.UtcNow;
        }
    }

    private async Task<SyncState> GetActiveSyncStateAsync()
    {
        return await _dbContext.SyncStates.FirstAsync();
    }
}
