namespace Shelfly.Common.DTOs;

public class TrashConfig
{
    public bool CleanupEnabled { get; set; } = false;
    public int RetentionDays { get; set; } = 30;
}
