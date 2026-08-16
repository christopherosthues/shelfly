using Shelfly.Common.Enums;

namespace Shelfly.App.Data.Entities;

public abstract class BaseEntity
{
    public Guid LocalGuid { get; set; } = Guid.NewGuid();
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public DeletionStatus DeletionStatus { get; set; } = DeletionStatus.Active;
}
