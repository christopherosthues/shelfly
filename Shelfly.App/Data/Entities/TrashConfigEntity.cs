namespace Shelfly.App.Data.Entities;

public class TrashConfigEntity
{
    public int Id { get; set; } = 1;
    public bool CleanupEnabled { get; set; } = false;
    public int RetentionDays { get; set; } = 30;
    public string? AccountUrl { get; set; }
}
