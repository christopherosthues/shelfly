namespace Shelfly.App.Data.Entities;

public class RemoteMapping
{
    public int Id { get; set; }
    public Guid LocalBookGuid { get; set; }
    public string ServerUrl { get; set; } = string.Empty;
    public Guid RemoteGuid { get; set; }
    public DateTimeOffset? LastSynced { get; set; }

    public LocalBook? LocalBook { get; set; }
}
