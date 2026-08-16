namespace Shelfly.App.Data.Entities;

public abstract class BaseEntity
{
    public Guid LocalGuid { get; set; } = Guid.NewGuid();
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
}
