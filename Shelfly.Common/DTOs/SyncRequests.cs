namespace Shelfly.Common.DTOs;

public class SyncUploadRequest
{
    public string EntityType { get; set; } = string.Empty;
    public List<SyncItem> Items { get; set; } = [];
}

public class SyncItem
{
    public Guid LocalGuid { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public List<SyncBookmarkItem>? Bookmarks { get; set; } = [];
}

public class SyncBookmarkItem
{
    public Guid LocalGuid { get; set; }
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset LastModified { get; set; }
}

public class SyncUploadResponse
{
    public List<SyncUploadedItem> Uploaded { get; set; } = [];
    public List<SyncConflictItem> Conflicts { get; set; } = [];
}

public class SyncUploadedItem
{
    public Guid LocalGuid { get; set; }
    public Guid RemoteGuid { get; set; }
    public string EntityType { get; set; } = string.Empty;
}

public class SyncConflictItem
{
    public Guid LocalGuid { get; set; }
    public Guid RemoteGuid { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class SyncConflictResolutionRequest
{
    public Guid LocalGuid { get; set; }
    public Guid RemoteGuid { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
}

public class SyncConflictResolutionResponse
{
    public bool Resolved { get; set; }
    public string AppliedVersion { get; set; } = string.Empty;
}

public class SyncDownloadRequest
{
    public string EntityType { get; set; } = string.Empty;
    public List<Guid> LocalGuids { get; set; } = [];
}

public class SyncDownloadResponse
{
    public List<SyncDownloadItem> Downloaded { get; set; } = [];
    public List<SyncDeletedItem> Deleted { get; set; } = [];
}

public class SyncDownloadItem
{
    public Guid RemoteGuid { get; set; }
    public Guid? LocalGuid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTimeOffset LastModified { get; set; }
    public string DeletionStatus { get; set; } = "Active";
    public List<SyncDownloadBookmarkItem>? Bookmarks { get; set; } = [];
}

public class SyncDownloadBookmarkItem
{
    public Guid RemoteGuid { get; set; }
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset LastModified { get; set; }
}

public class SyncDeletedItem
{
    public Guid RemoteGuid { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public DateTimeOffset SoftDeletedAt { get; set; }
}
