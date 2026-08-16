using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common.DTOs;
using Shelfly.Common.Enums;

namespace Shelfly.Api.Services;

public class SyncService(ShelflyDbContext context)
{
    public async Task<SyncUploadResponse> UploadAsync(Guid userId, SyncUploadRequest request)
    {
        SyncUploadResponse response = new();

        foreach (SyncItem item in request.Items)
        {
            BookEntity? existingBook = await context.Books
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Id == item.LocalGuid);

            if (existingBook != null)
            {
                // Conflict detection - compare LastModified timestamps
                if (item.LastModified > existingBook.LastModified)
                {
                    // Local version is newer, update remote
                    existingBook.Title = item.Title ?? existingBook.Title;
                    existingBook.Author = item.Author ?? existingBook.Author;
                    existingBook.ISBN = item.Isbn ?? existingBook.ISBN;
                    if (item.PublishDate.HasValue)
                    {
                        existingBook.PublishDate = item.PublishDate.Value;
                    }
                    existingBook.LastModified = item.LastModified;

                    response.Conflicts.Add(new SyncConflictItem
                    {
                        LocalGuid = item.LocalGuid,
                        RemoteGuid = existingBook.Id,
                        EntityType = "Book",
                        Resolution = "LocalWins",
                        Reason = "Local version has newer LastModified timestamp"
                    });
                }
                else
                {
                    // Remote version is newer or equal, skip update
                    response.Conflicts.Add(new SyncConflictItem
                    {
                        LocalGuid = item.LocalGuid,
                        RemoteGuid = existingBook.Id,
                        EntityType = "Book",
                        Resolution = "RemoteWins",
                        Reason = "Remote version has newer or equal LastModified timestamp"
                    });
                }

                response.Uploaded.Add(new SyncUploadedItem
                {
                    LocalGuid = item.LocalGuid,
                    RemoteGuid = existingBook.Id,
                    EntityType = "Book"
                });
            }
            else
            {
                // New book on remote side
                BookEntity newBook = new()
                {
                    Id = Guid.NewGuid(),
                    Title = item.Title ?? string.Empty,
                    Author = item.Author ?? string.Empty,
                    ISBN = item.Isbn ?? string.Empty,
                    PublishDate = item.PublishDate ?? DateTime.UtcNow,
                    UserId = userId,
                    LastModified = item.LastModified
                };

                await context.Books.AddAsync(newBook);

                response.Uploaded.Add(new SyncUploadedItem
                {
                    LocalGuid = item.LocalGuid,
                    RemoteGuid = newBook.Id,
                    EntityType = "Book"
                });
            }

            // Handle bookmarks for this book
            if (item.Bookmarks != null)
            {
                foreach (SyncBookmarkItem bookmark in item.Bookmarks)
                {
                    BookmarkEntity? existingBookmark = await context.Bookmarks
                        .FirstOrDefaultAsync(bm => bm.UserId == userId && bm.Id == bookmark.LocalGuid);

                    if (existingBookmark != null)
                    {
                        // Conflict detection for bookmarks
                        if (bookmark.LastModified > existingBookmark.LastModified)
                        {
                            existingBookmark.StartPage = bookmark.StartPage;
                            existingBookmark.EndPage = bookmark.EndPage;
                            existingBookmark.Note = bookmark.Note;
                            existingBookmark.LastModified = bookmark.LastModified;

                            response.Conflicts.Add(new SyncConflictItem
                            {
                                LocalGuid = bookmark.LocalGuid,
                                RemoteGuid = existingBookmark.Id,
                                EntityType = "Bookmark",
                                Resolution = "LocalWins",
                                Reason = "Local version has newer LastModified timestamp"
                            });
                        }

                        response.Uploaded.Add(new SyncUploadedItem
                        {
                            LocalGuid = bookmark.LocalGuid,
                            RemoteGuid = existingBookmark.Id,
                            EntityType = "Bookmark"
                        });
                    }
                    else
                    {
                        // New bookmark on remote side
                        BookmarkEntity newBookmark = new()
                        {
                            Id = Guid.NewGuid(),
                            StartPage = bookmark.StartPage,
                            EndPage = bookmark.EndPage,
                            Note = bookmark.Note,
                            UserId = userId,
                            BookId = item.LocalGuid,
                            LastModified = bookmark.LastModified
                        };

                        await context.Bookmarks.AddAsync(newBookmark);

                        response.Uploaded.Add(new SyncUploadedItem
                        {
                            LocalGuid = bookmark.LocalGuid,
                            RemoteGuid = newBookmark.Id,
                            EntityType = "Bookmark"
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
        return response;
    }

    public async Task<SyncDownloadResponse> DownloadAsync(Guid userId, SyncDownloadRequest request)
    {
        SyncDownloadResponse response = new();

        // Get all books for this user that match the requested local GUIDs
        List<BookEntity>? remoteBooks = await context.Books
            .Where(b => b.UserId == userId && request.LocalGuids.Contains(b.Id))
            .Include(b => b.Bookmarks)
            .ToListAsync();

        foreach (BookEntity book in remoteBooks ?? [])
        {
            SyncDownloadItem downloadItem = new()
            {
                RemoteGuid = book.Id,
                LocalGuid = book.Id,
                Title = book.Title,
                Author = book.Author,
                LastModified = book.LastModified,
                DeletionStatus = book.DeletionStatus == DeletionStatus.SoftDeleted ? "SoftDeleted" : "Active",
                Bookmarks = book.Bookmarks?.Select(bm => new SyncDownloadBookmarkItem
                {
                    RemoteGuid = bm.Id,
                    StartPage = bm.StartPage,
                    EndPage = bm.EndPage,
                    Note = bm.Note,
                    LastModified = bm.LastModified
                }).ToList() ?? []
            };

            response.Downloaded.Add(downloadItem);
        }

        // Check for soft-deleted items that need to be reported
        List<BookEntity>? deletedBooks = await context.Books
            .Where(b => b.UserId == userId && request.LocalGuids.Contains(b.Id) && b.DeletionStatus == DeletionStatus.SoftDeleted)
            .ToListAsync();

        foreach (BookEntity book in deletedBooks ?? [])
        {
            response.Deleted.Add(new SyncDeletedItem
            {
                RemoteGuid = book.Id,
                EntityType = "Book",
                DeletedAt = book.LastModified  // Use LastModified as the soft-delete timestamp
            });
        }

        return response;
    }

    public async Task<SyncConflictResolutionResponse> ResolveConflictAsync(Guid userId, SyncConflictResolutionRequest request)
    {
        SyncConflictResolutionResponse response = new();

        if (request.EntityType == "Book")
        {
            BookEntity? book = await context.Books
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Id == request.RemoteGuid);

            if (book != null)
            {
                // Apply the resolution based on the requested version
                response.Resolved = true;
                response.AppliedVersion = request.Resolution;
            }
        }
        else if (request.EntityType == "Bookmark")
        {
            BookmarkEntity? bookmark = await context.Bookmarks
                .FirstOrDefaultAsync(bm => bm.UserId == userId && bm.Id == request.RemoteGuid);

            if (bookmark != null)
            {
                response.Resolved = true;
                response.AppliedVersion = request.Resolution;
            }
        }

        await context.SaveChangesAsync();
        return response;
    }

    public async Task<SyncDownloadResponse> RestoreFromTrashAsync(Guid userId, Guid remoteGuid)
    {
        SyncDownloadResponse response = new();

        BookEntity? book = await context.Books
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Id == remoteGuid);

        if (book != null && book.DeletionStatus == DeletionStatus.SoftDeleted)
        {
            // Restore the book by setting status to Active
            book.DeletionStatus = DeletionStatus.Active;
            book.LastModified = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync();

            response.Downloaded.Add(new SyncDownloadItem
            {
                RemoteGuid = book.Id,
                LocalGuid = book.Id,
                Title = book.Title,
                Author = book.Author,
                LastModified = book.LastModified,
                DeletionStatus = "Active"
            });
        }

        return response;
    }
}
