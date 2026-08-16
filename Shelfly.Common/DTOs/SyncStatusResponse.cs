namespace Shelfly.Common.DTOs;

public class SyncStatusResponse
{
    public bool Reachable { get; set; }
    public DateTimeOffset? LastSynced { get; set; }
    public int PendingChanges { get; set; }
}
