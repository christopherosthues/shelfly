namespace Shelfly.App.Data.Entities;

public class LocalBook : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public DateTime? PublishDate { get; set; }

    public ICollection<RemoteMapping> RemoteMappings { get; set; } = [];
    public ICollection<LocalBookmark> LocalBookmarks { get; set; } = [];
}
