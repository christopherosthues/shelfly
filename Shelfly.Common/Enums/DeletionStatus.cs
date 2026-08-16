namespace Shelfly.Common.Enums;

public enum DeletionStatus
{
    Active,       // Normal use — entity is visible and accessible
    SoftDeleted   // In trash — recoverable via restore operation
}
