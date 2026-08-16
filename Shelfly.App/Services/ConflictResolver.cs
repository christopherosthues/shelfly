using Shelfly.Common.DTOs;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Services;

public class ConflictResolver
{
    public SyncConflictResolution Resolve(LocalBook localBook, SyncDownloadItem remoteItem)
    {
        if (remoteItem.LastModified > localBook.LastModified)
        {
            return new SyncConflictResolution
            {
                Winner = "Remote",
                Reason = "Remote version has newer LastModified timestamp",
                AppliedVersion = remoteItem
            };
        }
        else
        {
            return new SyncConflictResolution
            {
                Winner = "Local",
                Reason = "Local version has newer or equal LastModified timestamp",
                AppliedVersion = localBook
            };
        }
    }

    public SyncConflictResolution Resolve(LocalBookmark localBookmark, SyncDownloadBookmarkItem remoteItem)
    {
        if (remoteItem.LastModified > localBookmark.LastModified)
        {
            return new SyncConflictResolution
            {
                Winner = "Remote",
                Reason = "Remote version has newer LastModified timestamp",
                AppliedVersion = remoteItem
            };
        }
        else
        {
            return new SyncConflictResolution
            {
                Winner = "Local",
                Reason = "Local version has newer or equal LastModified timestamp",
                AppliedVersion = localBookmark
            };
        }
    }

    public void ApplyResolution(LocalBook localBook, SyncConflictResolution resolution)
    {
        if (resolution.Winner == "Remote" && resolution.AppliedVersion is SyncDownloadItem remoteItem)
        {
            localBook.Title = remoteItem.Title;
            localBook.Author = remoteItem.Author;
            localBook.LastModified = remoteItem.LastModified;
        }
    }

    public void ApplyResolution(LocalBookmark localBookmark, SyncConflictResolution resolution)
    {
        if (resolution.Winner == "Remote" && resolution.AppliedVersion is SyncDownloadBookmarkItem remoteItem)
        {
            localBookmark.StartPage = remoteItem.StartPage;
            localBookmark.EndPage = remoteItem.EndPage;
            localBookmark.Note = remoteItem.Note;
            localBookmark.LastModified = remoteItem.LastModified;
        }
    }
}

public class SyncConflictResolution
{
    public string Winner { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public object? AppliedVersion { get; set; }
}
