using Shelfly.Common.Enums;

namespace Shelfly.Common.DTOs;

public class SyncMetadata
{
    public Guid LocalGuid { get; set; } = Guid.NewGuid();
    public Guid? RemoteGuid { get; set; }
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public DeletionStatus DeletionStatus { get; set; } = DeletionStatus.Active;
}
